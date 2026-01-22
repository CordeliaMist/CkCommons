using System.Text.RegularExpressions;

namespace CkCommons.DrawSystem;

// the internal, private operations for the dynamic file system.
public partial class DynamicDrawSystem<T>
{
    protected enum Result
    {
        Success,
        SuccessNothingDone,
        InvalidOperation,
        ItemExists,
        PartialSuccess,
        CircularReference,
        NoSuccess,
    }

    /// <summary>
    ///     Internally rename a Folder or FolderGroup to a new name. <para />
    ///     Fails if the new name is invalid, or the name exists in the drawSystem already.
    /// </summary>
    /// <param name="folder"> the folder to rename. </param>
    /// <param name="newName"> the new name. </param>
    private Result RenameFolder(IDynamicCollection<T> folder, string newName)
    {
        var fixedNewName = newName.FixName();

        // Prevent renaming to root.
        if (string.Equals(newName, DDSHelpers.RootName, StringComparison.Ordinal))
            return Result.InvalidOperation;

        // If the name is no different, return success with nothing done.
        if (fixedNewName == folder.Name)
            return Result.SuccessNothingDone;

        // Mark the item as existing if it is present in the drawSystem already.
        if (_folderMap.ContainsKey(fixedNewName))
            return Result.ItemExists;

        // Otherwise, update the name of the folder.
        // (Do not sort, leave that to the folder's AutoSort function)
        if (folder is DynamicFolderGroup<T> fc)
        {
            _folderMap.Remove(folder.Name);
            fc.SetName(newName, false);
            _folderMap.Add(fc.Name, fc);
            return Result.Success;
        }
        // works on any folder kind becuz abstract yes yes.
        else if (folder is DynamicFolder<T> f)
        {
            _folderMap.Remove(folder.Name);
            f.SetName(newName, false);
            _folderMap.Add(f.Name, f);
            return Result.Success;
        }
        else
        {
            return Result.InvalidOperation;
        }
    }

    // Moves multiple folders at once to a new destination.
    // Moved folders can be Folders or FolderGroups.
    // Destination location can also be defined for matches.
    private Result BulkMoveCollections(IEnumerable<IDynamicCollection<T>> toMove, DynamicFolderGroup<T> newParent, IDynamicCollection<T>? destChild = null)
    {
        // If the destChild is not null and it's parent is not the destination return invalid operation.
        if (destChild is not null && destChild.Parent != newParent)
            return Result.InvalidOperation;

        // Partition into same-parent and foreign-parent within a single loop.
        var spNodes = new List<IDynamicCollection<T>>();
        var otherNodes = new List<IDynamicCollection<T>>();
        foreach (var node in toMove)
        {
            if (node == newParent)
                continue;

            if (node.Parent == newParent)
                spNodes.Add(node);
            else
                otherNodes.Add(node);
        }

        // Only update them if a valid dest was provided, otherwise do nothing for them.
        if (spNodes.Count > 0)
        {
            // If there were any nodes under the same parent, we should move them.
            if (destChild is not null)
            {
                int destIdx = newParent.Children.IndexOf(destChild);
                // Collect & sort source indices
                int count = spNodes.Count;
                int[] fromIndices = new int[spNodes.Count];
                for (int i = 0; i < spNodes.Count; i++)
                    fromIndices[i] = newParent.Children.IndexOf(spNodes[i]);

                Array.Sort(fromIndices);
                // Calculate the shift
                int shift = 0;
                for (int i = 0; i < spNodes.Count; i++)
                    if (fromIndices[i] < destIdx)
                        shift++;
                // Determine the final insertion index
                int insertIndex = destIdx - shift;
                // remove from highest index downward
                for (int i = count - 1; i >= 0; i--)
                    newParent.Children.RemoveAt(fromIndices[i]);
                // Insert same-parent nodes
                newParent.Children.InsertRange(insertIndex, spNodes);
                // Now we should also insert the other nodes.
                if (otherNodes.Count > 0)
                    newParent.InsertChildren(otherNodes, insertIndex);
            }
            else
            {
                newParent.AddChildren(otherNodes);
            }
            // Perform the post-sort.
            newParent.SortChildren();
            return Result.Success;
        }
        // Otherwise, if the otherNodes were present, do it the same way but without the same-parent logic.
        if (otherNodes.Count > 0)
        {
            if (destChild is not null)
            {
                int destIdx = newParent.Children.IndexOf(destChild);
                newParent.InsertChildren(otherNodes, destIdx);
            }
            else
            {
                newParent.AddChildren(otherNodes);
            }
            // Perform the post-sort.
            newParent.SortChildren();
            return Result.Success;
        }

        return Result.SuccessNothingDone;
    }

    // Moves a Folder to a new location within the DDS.
    private Result MoveCollection(IDynamicCollection<T> folder, DynamicFolderGroup<T> newParent)
    {
        if (folder == newParent)
            return Result.InvalidOperation;
        // If the old and new parents are the same, do nothing.
        if (folder.Parent == newParent)
            return Result.SuccessNothingDone;
        // Fail if the new parent already has this child.
        if (newParent.Children.Contains(folder))
            return Result.ItemExists;
        // Transfer the child from the previous parent to the new one.
        newParent.AddChild(folder);
        return Result.Success;
    }


    /// <summary>
    ///     Creates all folders to the end, automatically excluding leaf paths from assignment.
    /// </summary>
    /// <param name="fullPath"> the entity's FullPath variable, excluding root. </param>
    /// <param name="topFolder"> the topmost available folder or folder collection in the path.</param>
    /// <returns> The result of this function. </returns>
    private Result CreateAllGroups(string fullPath, out DynamicFolderGroup<T> topFolder)
    {
        topFolder = root;
        Result res = Result.SuccessNothingDone;

        string[] parts = fullPath.Split("//", StringSplitOptions.RemoveEmptyEntries);
        // Process each part of the path, creating a folder group for each, branching out from root.
        foreach (var rawSegment in parts)
        {
            var segment = rawSegment.FixName();
            // Attempt to locate the FolderGroup by name.
            if (TryGetFolderGroup(segment, out var existing))
            {
                if (existing.Parent == topFolder)
                {
                    topFolder = existing;
                    continue;
                }
                else
                {
                    return Result.ItemExists;
                }
            }
            else
            {
                // The folder is not found, so we can create a new one.
                var newFolder = new DynamicFolderGroup<T>(topFolder, idCounter + 1u, segment);
                topFolder.AddChild(newFolder);
                _folderMap[newFolder.Name] = newFolder;
                ++idCounter;
                res = Result.Success;
                topFolder = newFolder;
            }
        }

        // Return the final result.
        return res;
    }

    // Removes a folder from the DrawSystem.
    // FolderGroups will merge it's children into the parent before removal.
    // Folders will simply be removed.
    private Result RemoveFolder(IDynamicCollection<T> folder)
    {
        // If the folder is not found, it was technically removed lol.
        if (!_folderMap.TryGetValue(folder.Name, out var match))
            return Result.SuccessNothingDone;

        // Prevent root from being removed.
        if (folder.Name == DDSHelpers.RootName)
            return Result.InvalidOperation;

        // Otherwise, it does exist, and we should remove it.
        if (folder is DynamicFolderGroup<T> fg)
        {
            // Merge the folder into the parent, then remove it.
            MergeFolders(fg, fg.Parent);
            Svc.Log.Debug($"Merged FolderGroup '{fg.Name}' into Parent '{fg.Parent.Name}'");
            return Result.Success;
        }
        else if (folder is DynamicFolder<T> f)
        {
            // Remove the folder from the child.
            f.Parent.Children.Remove(match);
            // Remove it from the mapping.
            _folderMap.Remove(folder.Name);
            return Result.Success;
        }

        // Invalid otherwise.
        return Result.InvalidOperation;
    }

    /// <summary> 
    ///     Try to merge all children <paramref name="from"/> into <paramref name="to"/>.
    /// </summary>
    private Result MergeFolders(DynamicFolderGroup<T> from, DynamicFolderGroup<T> to)
    {
        // if the collections are the same, fail.
        if (from == to)
            return Result.SuccessNothingDone;
        // If we are moving from root, fail.
        if (from.Name == DDSHelpers.RootName)
            return Result.InvalidOperation;
        // If a circular reference exists, fail.
        if (!HasValidHeritage(to, from))
            return Result.CircularReference;

        Svc.Log.Debug($"Merging FolderGroup '{from.Name}' into '{to.Name}'");
        // Otherwise we can proceed to merge.
        var result = from.TotalChildren is 0 ? Result.Success : Result.NoSuccess;
        // iterate through the children and move them.
        for (var i = 0; i < from.TotalChildren;)
        {
            var childName = from.Children[i].Name;
            Svc.Log.Debug($" Attempting to move child '{childName}'");
            var moveRet = MoveCollection(from.Children[i], to);
            // If the move was successful,
            if (moveRet is Result.Success)
            {
                Svc.Log.Debug($"  Moved child '{childName}' successfully.");
                // If previous result was NoSuccess, check if this is the first item
                if (result == Result.NoSuccess)
                    result = (i == 0) ? Result.Success : Result.PartialSuccess;
                // Otherwise keep the existing result if not (should be partial success)

                // We moved a child folder out of the list we are iterating through, so dont inc i.
                // Next child is at current index
            }
            else
            {
                Svc.Log.Debug($"  Failed to move child '{childName}'.");
                // Move failed, increment index
                i++;
                // Bump success down to partial success, if we were currently set to success.
                if (result is Result.Success)
                    result = Result.PartialSuccess;
            }
        }

        // return the final result.
        // If it was successful, remove the old folder from its parent, and the map.
        if (result is Result.Success)
        {
            from.Parent.Children.Remove(from);
            _folderMap.Remove(from.Name);
        }

        return result;
    }

    // Catch potential circular references in relationships.
    // Returns true if potentialParent is not anywhere up the tree from child, false otherwise.
    private static bool HasValidHeritage(DynamicFolderGroup<T> potentialParent, IDynamicCollection<T> folder)
    {
        var parent = potentialParent;
        // Root is an empty string.
        while (!string.Equals(parent.Name, DDSHelpers.RootName))
        {
            if (parent == folder)
                return false;

            parent = parent.Parent;
        }

        return true;
    }
}