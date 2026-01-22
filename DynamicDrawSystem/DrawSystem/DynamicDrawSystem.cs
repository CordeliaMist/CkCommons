using System.Diagnostics.CodeAnalysis;

namespace CkCommons.DrawSystem;

public enum DDSChange
{
    CollectionAdded,   // [Listener for FileSaving & FilterCache Reload] A Collection was added, and could be attached to a FolderGroup.
    CollectionRemoved, // [Listener for FileSaving & FilterCache Remove/Dirty] A Collection was removed from the hierarchy.
    CollectionMoved,   // [Listener for FileSaving & FilterCache Reload] A Collection moved from one FolderGroup to another.
    CollectionMerged,  // [Listener for FileSaving & FilterCache Reload] A Collection was merged into a FolderGroup.
    CollectionRenamed, // [Listener for FileSaving & FilterCache Reload] Collection was renamed.
    BulkMove,          // [Listener for FileSaving & FilterCache Reload] Multiple collections were moved at once. Usually requires full refresh.
    FullReloadStarting,// [Listener for Selections ] A full reload is starting. Good idea to store any names of selected nodes before they are cleared.
    FullReloadFinished,// [Listener for Selections ] Full reload finished, and is ready for selections to be updated.
}

public enum CollectionUpdate
{
    FolderUpdated,      // [Listener for Selections ] Re-processed GetAllItems(). For listeners wanting to track removed leaves. Auto-Sorting was handled already.
    SortDirectionChange,// [FilterCache Sort Listener] The sort direction was changed (Asc/Desc) for the folder's children.
    SorterChange,       // [FilterCache Sort Listener] The DynamicSorter of an IDynamicCollection changed.
    OpenStateChange,    // [FilterCache Reload Listener] A Collection was opened or closed.
}

/// <summary>
///     A DynamicDrawSystem works in a similar fashion to CkFileSystem / OtterGui.FileSystem,
///     except all leaves are managed internally through generators. <para />
///     Every created folder is assigned with a Generator, which updates the respective leaves in it. <para />
///     As such, only folders and folder collections can be moved, renamed, or have other actions performed on it. <para />
///     Update this overtime, or potentially make the dynamic draw system abstract for additional forced implementations.
/// </summary>
public abstract partial class DynamicDrawSystem<T> where T : class
{
    // For essential, structure-altering changes within the DDS.
    public delegate void DDSChangeDelegate(DDSChange kind, IDynamicNode<T> node, IDynamicCollection<T>? prevParent, IDynamicCollection<T>? newParent);
    public event DDSChangeDelegate? DDSChanged;

    // Whenever a notable update occurs to a collection that external sources should be notified for.
    // (Maybe change to IDynamicNode<T> for the enumerable if we need it.
    public delegate void CollectionUpdateDelegate(CollectionUpdate kind, IDynamicCollection<T> collection, IEnumerable<DynamicLeaf<T>>? affected);
    public event CollectionUpdateDelegate? CollectionUpdated;

    // Internal folder mapping for quick access by name.
    private readonly Dictionary<string, IDynamicCollection<T>> _folderMap = new(StringComparer.OrdinalIgnoreCase);
    // could do a leaf map as well but do not see any need to.

    /// <summary> The private, incrementing ID counter for this dynamic folder system. </summary>
    protected uint idCounter { get; private set; } = 0;

    /// <summary> The root folder collection of this dynamic folder system </summary>
    protected DynamicFolderGroup<T> root = DynamicFolderGroup<T>.CreateRoot([DynamicSorterEx.ByFolderName<T>()]);

    /// <summary>
    ///     Read-only Accessor for root via classes desiring inspection while preventing edits.
    ///     (This is technically already dont via internal setters but whatever).
    /// </summary>
    public IDynamicFolderGroup<T> Root
        => root;

    // Temporary for debugger assistance.
    public IReadOnlyDictionary<string, IDynamicCollection<T>> FolderMap
        => _folderMap;

    // Attempts to get a DynamicFolderGroup by its name, returning true if found.
    public bool TryGetFolderGroup(string name, [NotNullWhen(true)] out DynamicFolderGroup<T>? folderGroup)
    {
        if (_folderMap.TryGetValue(name, out var folder) && folder is DynamicFolderGroup<T> fg)
        {
            folderGroup = fg;
            return true;
        }
        folderGroup = null;
        return false;
    }

    public bool TryGetFolder(string name, [NotNullWhen(true)] out IDynamicCollection<T>? folder)
        => _folderMap.TryGetValue(name, out folder);

    // Dunno why this is needed anymore.
    //public bool Equal(ReadOnlySpan<char> lhs, ReadOnlySpan<char> rhs)
    //    => _nameComparer.BaseComparer.Compare(lhs, rhs) is 0;

    // Useful for dynamic folders wanting to contain their own nested folders.
    protected DynamicFolderGroup<T> GetFolderGroupByName(string parentName)
       => _folderMap.TryGetValue(parentName, out var f) && f is DynamicFolderGroup<T> fg
        ? fg : root;

    protected bool AddFolderGroup(string name, DynamicFolderGroup<T>? requestedParent = null, FolderFlags flags = FolderFlags.None)
    {
        // If the folder under the same name already exists, abort creation.
        if (_folderMap.ContainsKey(name))
            return false;

        var parent = requestedParent != null ? GetFolderGroupByName(requestedParent.Name) : root;
        var folderGroup = new DynamicFolderGroup<T>(parent, idCounter + 1u, name, flags: flags);

        // Folder is valid, attempt to assign it. If it fails, throw an exception.
        if (folderGroup.Parent.Children.Contains(folderGroup))
            throw new Exception($"Could not add folder group [{folderGroup.Name}] to group [{folderGroup.Parent.Name}]: Folder with the same name exists.");

        // Successful, so increment the ID counter and update the map and contents.
        folderGroup.Parent.AddChild(folderGroup);
        ++idCounter;
        _folderMap[folderGroup.Name] = folderGroup;
        DDSChanged?.Invoke(DDSChange.CollectionAdded, folderGroup, null, folderGroup.Parent);
        return true;
    }

    // Could make this return Result instead but its more for internal stuff.
    protected bool AddFolder(DynamicFolder<T> folder)
    {
        // If the folder under the same name already exists, abort creation.
        if (_folderMap.ContainsKey(folder.Name))
            return false;

        // Ensure Validity. If parent is null or the id counter is off, fail creation.
        if (folder.Parent is null)
            return false;

        if (folder.ID != idCounter + 1u)
            return false;

        // Folder is valid, so attempt to assign it. If it fails, throw an exception.
        if (folder.Parent.Children.Contains(folder))
            throw new Exception($"Could not add folder [{folder.Name}] to group [{folder.Parent.Name}]: Folder with the same name exists.");

        // Successful, so increment the ID counter and update the map and contents.
        folder.Parent.AddChild(folder);
        ++idCounter;
        folder.Update(out _);
        _folderMap[folder.Name] = folder;
        DDSChanged?.Invoke(DDSChange.CollectionAdded, folder, null, folder.Parent);
        return true;
    }

    // Retrieve the updated state of all folders.
    public void UpdateFolders()
    {
        IEnumerable<DynamicLeaf<T>> removed = [];

        foreach (var folder in _folderMap.Values.OfType<DynamicFolder<T>>())
            if (folder.Update(out var rem))
                removed = removed.Concat(rem);
        // Notify of removed leaves.
        CollectionUpdated?.Invoke(CollectionUpdate.FolderUpdated, root, removed);
    }

    public void UpdateFolder(DynamicFolder<T> folder)
    {
        folder.Update(out var removed);
        CollectionUpdated?.Invoke(CollectionUpdate.FolderUpdated, folder, removed);
    }

    public void UpdateFolder(string folderName)
    {
        if (_folderMap.TryGetValue(folderName, out var folder) && folder is DynamicFolder<T> f)
            UpdateFolder(f);
    }

    public void SetSortDirection(IDynamicCollection<T> folder, bool isDescending)
    {
        if (folder is DynamicFolderGroup<T> fc)
        {
            fc.Sorter.FirstDescending = isDescending;
            CollectionUpdated?.Invoke(CollectionUpdate.SortDirectionChange, folder, null);
        }
        else if (folder is DynamicFolder<T> f)
        {
            f.Sorter.FirstDescending = isDescending;
            CollectionUpdated?.Invoke(CollectionUpdate.SortDirectionChange, folder, null);
        }
    }

    public void SetSorterSteps(DynamicFolderGroup<T> folder, IEnumerable<ISortMethod<IDynamicCollection<T>>> steps)
    {
        folder.Sorter.SetSteps(steps);
        CollectionUpdated?.Invoke(CollectionUpdate.SorterChange, folder, null);
    }

    public void SetSorterSteps(DynamicFolder<T> folder, IEnumerable<ISortMethod<DynamicLeaf<T>>> steps)
    {
        folder.Sorter.SetSteps(steps);
        CollectionUpdated?.Invoke(CollectionUpdate.SorterChange, folder, null);
    }

    // Sets the expanded state of a folder to a new value.
    public bool SetOpenState(IDynamicCollection<T> folder, bool isOpen)
    {
        if (folder is DynamicFolderGroup<T> fc && fc.IsOpen != isOpen)
        {
            fc.SetIsOpen(isOpen);
            CollectionUpdated?.Invoke(CollectionUpdate.OpenStateChange, folder, null);
            return true;
        }
        else if (folder is DynamicFolder<T> f && f.IsOpen != isOpen)
        {
            f.SetIsOpen(isOpen);
            CollectionUpdated?.Invoke(CollectionUpdate.OpenStateChange, folder, null);
            return true;
        }
        // Fail otherwise.
        return false;
    }

    /// <summary>
    ///     Sets the opened state of multiple folders by name. <para />
    ///     This works on <b>FolderGroup's AND Folder's</b>.
    /// </summary>
    protected void OpenFolders(List<string> toOpen, bool newState)
    {
        bool anyOpened = false;
        foreach (var collectionName in toOpen)
        {
            // Attempt to locate the collection.
            if (!_folderMap.TryGetValue(collectionName, out var collection) || collection.IsOpen)
                continue;

            // Set the open state.
            if (collection is DynamicFolderGroup<T> fc)
                fc.SetIsOpen(newState);
            else if (collection is DynamicFolder<T> f)
                f.SetIsOpen(newState);

            anyOpened = true;
        }
        // Invoke if any opened. Just do root for simplicity, but if we wanted to we could change every single opened folder individually lol.
        if (anyOpened)
            CollectionUpdated?.Invoke(CollectionUpdate.OpenStateChange, root, null);
    }


    /// <summary>
    ///     Internally rename a defined folder in the DDS. <para />
    ///     Auto-Sorts the parent folder's children if enabled. <para />
    /// </summary>
    public void Rename(IDynamicCollection<T> node, string newName)
    {
        switch (RenameFolder(node, newName))
        {
            case Result.ItemExists: throw new Exception($"Can't rename {node.Name} to {newName}: another entity in {node.Name}'s Parent has the same name.");
            case Result.Success:
                DDSChanged?.Invoke(DDSChange.CollectionRenamed, node, null, null);
                return;
        }
    }

    /// <summary>
    ///     Splits path into successive subfolders of root and finds or creates the topmost folder. <para />
    ///     <b> WARNING: Very unstable, not tested with non-FolderGroup paths. Could break things. </b>
    /// </summary>
    /// <returns> The topmost folder. </returns>
    /// <exception cref="Exception"> If a folder can't be found or created due to an existing non-folder child with the same name. </exception>
    public IDynamicCollection<T> FindOrCreateAllGroups(string path)
    {
        // Respond based on the resulting action.
        switch (CreateAllGroups(path, out DynamicFolderGroup<T> topFolder))
        {
            case Result.Success:
                DDSChanged?.Invoke(DDSChange.CollectionAdded, topFolder, null, topFolder.Parent);
                break;

            case Result.ItemExists:
                // Throw Exception.
                throw new Exception($"Could not create new folder for {path}: {topFolder.FullPath} already contains an object with a required name.");
            case Result.PartialSuccess:
                DDSChanged?.Invoke(DDSChange.CollectionAdded, topFolder, null, topFolder.Parent);
                // Throw exception due to partial failure, since it expects to create all, but failed before finishing.
                throw new Exception($"Could not create all new folders for {path}: {topFolder.FullPath} already contains an object with a required name.");
        }
        // Return the top level folder.
        return topFolder;
    }

    /// <summary>
    ///     Delete a folder from it's Parent. Locates the folder by name first. <para />
    ///     An Exception is thrown if the entity is root.
    /// </summary>
    /// <exception cref="Exception"></exception>
    public bool Delete(string folderName)
    {
        if (!_folderMap.TryGetValue(folderName, out var match))
            return false;
        // Perform the actual deletion.
        Delete(match);
        return true; // Assumed? Could be proven wrong.
    }

    /// <summary>
    ///     Delete a folder from it's Parent. <para />
    ///     An Exception is thrown if the entity is root.
    /// </summary>
    /// <exception cref="Exception"></exception>
    public void Delete(IDynamicCollection<T> folder)
    {
        switch (RemoveFolder(folder))
        {
            case Result.InvalidOperation:
                throw new Exception("Can't delete the root folder.");
            case Result.Success:
                DDSChanged?.Invoke(DDSChange.CollectionRemoved, folder, folder.Parent, null);
                return;
        }
    }

    // Temporary test for moving folders only.
    // Moves all collections into the newParent. If desired, merges all items of similar type of destChild at destChild's idx.
    public void BulkMove(IEnumerable<IDynamicCollection<T>> toMove, DynamicFolderGroup<T> newParent, IDynamicCollection<T>? destChild = null)
    {
        switch (BulkMoveCollections(toMove, newParent, destChild))
        {
            case Result.Success:
                DDSChanged?.Invoke(DDSChange.BulkMove, newParent, null, null);
                // If the folder is closed, open it.
                if (!newParent.IsOpen)
                {
                    newParent.SetIsOpen(true);
                    CollectionUpdated?.Invoke(CollectionUpdate.OpenStateChange, newParent, null);
                }
                break;
            case Result.SuccessNothingDone:
                return;
            case Result.InvalidOperation:
                throw new Exception("Can not move root directory.");
            case Result.ItemExists:
                throw new Exception($"Can't move one or more folders into {newParent.FullPath}, another folder inside it already exists.");
        }
    }

    public void Move(IDynamicCollection<T> folder, DynamicFolderGroup<T> newParent)
    {
        var oldParent = folder.Parent;
        switch (MoveCollection(folder, newParent))
        {
            case Result.Success:
                DDSChanged?.Invoke(DDSChange.CollectionMoved, folder, oldParent, newParent);
                break;

            case Result.SuccessNothingDone:
                return;

            case Result.InvalidOperation:
                throw new Exception("Can not move root directory.");

            case Result.CircularReference:
                throw new Exception($"Can not move {folder.FullPath} into {newParent.FullPath} since folders can not contain themselves.");

            case Result.ItemExists:
                // if and only if both folders are FolderCollections, should we allow a merge to occur.

                // Fix this as we tackle the merging territory.

                //var matchingIdx = Search(newParent, folder.Name);
                //// If we meet criteria to merge, do so.
                //if (folder is DynamicFolderGroup<T> fc && newParent.Children[matchingIdx] is DynamicFolderGroup<T> destFolder)
                //{
                //    Merge(fc, destFolder);
                //    return;
                //}

                throw new Exception($"Can't move {folder.Name} into {newParent.FullPath}, another folder inside it already exists.");
        }
    }

    public void Merge(DynamicFolderGroup<T> from, DynamicFolderGroup<T> to)
    {
        switch (MergeFolders(from, to))
        {
            case Result.SuccessNothingDone:
                return;

            case Result.InvalidOperation:
                throw new Exception($"Can not merge root directory into {to.FullPath}.");

            case Result.Success:
                DDSChanged?.Invoke(DDSChange.CollectionMerged, from, from, to);
                return;

            case Result.PartialSuccess:
                DDSChanged?.Invoke(DDSChange.CollectionMerged, from, from, to);
                return;

            case Result.NoSuccess:
                throw new Exception($"Could not merge {from.FullPath} into {to.FullPath}. All children already existed in the target.");
        }
    }
}