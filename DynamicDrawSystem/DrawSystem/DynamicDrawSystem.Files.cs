using CkCommons;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace CkCommons.DrawSystem;

// Process Config Saving & Loading.
// ------------------------
// DDS Only stores the folder hierarchy, and the folders opened states.
// Update this overtime as we fix things.
public partial class DynamicDrawSystem<T>
{
    /// <summary>
    ///     Stores the FolderGroup Hierarchy and the paths of all opened folders in dictionary format. <para />
    ///     All of this is written into the streamWriter.
    /// </summary>
    protected void SaveToFile(StreamWriter writer)
    {
        using var j = new JsonTextWriter(writer);
        j.Formatting = Formatting.Indented;

        // Track which folders are currently opened at the time of saving.
        var rootFolders = root.GetAllFolderDescendants();
        var opened = new List<IDynamicCollection<T>>();

        j.WriteStartObject();
        // Dictionary for the DynamicCollection<T> => FullPath.
        // Dictates the hierarchy structure of the draw system.
        j.WritePropertyName("GroupHierarchy");
        j.WriteStartObject();
        foreach(var group in rootFolders.OfType<IDynamicFolderGroup<T>>())
        {
            // Only add the path if the parent is not root.
            j.WritePropertyName(group.Name);
            j.WriteValue(group.FullPath);

            if (group.IsOpen)
                opened.Add(group);
        }
        j.WriteEndObject();

        // Dictionary for the DynamicCollection<T> => FullPath.
        // Determines which groups the folders are attached to.
        j.WritePropertyName("FolderParents");
        j.WriteStartObject();
        foreach (var folder in rootFolders.OfType<IDynamicFolder<T>>())
        {
            // Skip if the parent was root.
            if (folder.IsOpen)
                opened.Add(folder);

            if (folder.Parent.IsRoot)
                continue;

            j.WritePropertyName(folder.Name);
            j.WriteValue(folder.Parent.Name);
        }
        j.WriteEndObject();

        // Write out all folders that were opened.
        // Separately, write the FullPaths of the opened folders.
        j.WritePropertyName("OpenedCollections");
        j.WriteStartArray();
        foreach (var collection in opened)
            j.WriteValue(collection.Name);
        j.WriteEndArray();

        // End of the main object.
        j.WriteEndObject();
    }

    /// <summary>
    ///     Helper function to specify the file location that we want to load our JObject from. <para />
    ///     Returns the dictionary mapping all Folders to their location in the FolderHierarchy, 
    ///     and also all opened folder paths. (may change overtime)
    /// </summary>
    protected bool LoadFile(FileInfo file)
    {
        JObject? jObj = null;
        // Safely load the JObject if it exists.
        if (File.Exists(file.FullName))
            Generic.Safe(() => jObj = JObject.Parse(File.ReadAllText(file.FullName)));
        else
        {
            Svc.Log.Warning($"DDS LoadFile called but file does not exist: {file.FullName}");
            return false;
        }
        // Then perform the internal load function.
        return LoadObject(jObj);
    }
    /// <summary>
    ///     Generates the DynamicDrawSystem from the contents of the JObject.
    /// </summary>
    /// <returns> If any FolderGroups, or folders were created. </returns>
    protected bool LoadObject(JObject? jObject)
    {
        // Invoke the reload is beginning
        DDSChanged?.Invoke(DDSChange.FullReloadStarting, root, null, null);
        // Reset all data, completely.
        idCounter = 0;
        _folderMap.Clear();
        root.Children.Clear();
        // We cleared everything, which is technically a change.
        var foldersCreated = true;
        if (jObject != null)
        {
            // Now loading new data, so assume no changes.
            foldersCreated = false;
            try
            {
                // Obtain all relevent data from the folder.
                var groupHierarchy = jObject["GroupHierarchy"]?.ToObject<Dictionary<string, string>>() ?? [];
                var folderMap = jObject["FolderParents"]?.ToObject<Dictionary<string, string>>() ?? [];
                var openedCollections = jObject["OpenedCollections"]?.ToObject<List<string>>() ?? [];

                // Construct all Groups that do not already exist.
                foreach (var (groupName, groupPath) in groupHierarchy)
                {
                    // If we created any groups in this process, mark the changes are true.
                    if (CreateAllGroups(groupPath, out _) is (Result.Success or Result.SuccessNothingDone))
                    {
                        // If this was success or success nothing done, at least one folder was created.
                        foldersCreated = true;
                    }
                }

                // Now we must process all of the folder creations and mapping of their parents.
                foldersCreated |= EnsureAllFolders(folderMap);

                // Now we must ensure that these Folders or FolderGroups have the correct expanded state.
                // This can affect what is displayed but we can process it internally if desired only.
                OpenFolders(openedCollections, true);
            }
            catch (Exception ex)
            {
                Svc.Log.Error($"Bagagwa Summoned during loading: {ex}");
                // If any error occured, we are stuck with just root, which was a change, so make true.
                foldersCreated = true;
            }
        }
        else
        {
            Svc.Log.Warning("No DDS JObject found during load, starting fresh with only root folder.");
        }
        
        // The entire reload process is now complete, and we can notify listeners of such.
        DDSChanged?.Invoke(DDSChange.FullReloadFinished, root, null, null);
        return foldersCreated;
    }

    /// <summary>
    ///     Ensures that all expected folders are created. <para />
    ///     Provides the folder map obtained via loading, to determine 
    ///     what folders link to which FolderGroups.
    /// </summary>
    /// <remarks> If a folder does not have a mapping, it is assumed to bind to Root. </remarks>
    /// <returns> If any folders were created. </returns>
    protected abstract bool EnsureAllFolders(Dictionary<string, string> folderMap);
}