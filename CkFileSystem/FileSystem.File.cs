using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace CkCommons.FileSystem;

public partial class CkFileSystem<T>
{
    /// <summary>
    /// Save a current filesystem to a file, using a function that transforms the data value as well as its full path
    /// to a identifier string for the data as well as a bool whether this data should be saved.
    /// </summary>
    /// <remarks> If addEmptyFolders is true, folders without any leaves are stored separately. </remarks>
    protected void SaveToFile(StreamWriter writer, Func<T, string, (string, bool)> conversion, bool addEmptyFolders)
    {
        using JsonTextWriter j = new JsonTextWriter(writer);
        j.Formatting = Formatting.Indented;

        string typeName = typeof(T).Name; // Get the type name (e.g., "CursedItem")
        List<string> emptyFolders = new List<string>();
        j.WriteStartObject();
        j.WritePropertyName("Data");
        j.WriteStartObject();
        // Iterate lexicographically through all descendants, keep track of empty folders if necessary.
        // otherwise write all the paths that are given by the conversion function.
        if (Root.Children.Count > 0)
            foreach (IPath path in Root.GetAllDescendants(ISortMode<T>.Lexicographical))
            {
                switch (path)
                {
                    case Folder f:
                        if (addEmptyFolders && f.Children.Count == 0)
                            emptyFolders.Add(f.FullName());
                        break;
                    case Leaf l:
                        string fullPath = l.FullName();
                        (string name, bool write) = conversion(l.Value, fullPath);
                        if (write)
                        {
                            j.WritePropertyName(name);
                            j.WriteValue(fullPath);
                        }

                        break;
                }
            }

        j.WriteEndObject();
        // Write empty folders if applicable.
        if (addEmptyFolders)
        {
            j.WritePropertyName("EmptyFolders");
            j.WriteStartArray();
            foreach (string emptyFolder in emptyFolders)
                j.WriteValue(emptyFolder);
            j.WriteEndArray();
        }

        j.WriteEndObject();
    }

    protected bool Load(FileInfo file, IEnumerable<T> objects, Func<T, string> toIdentifier, Func<T, string> toName)
    {
        JObject? jObj = null;
        if (File.Exists(file.FullName))
            try
            {
                jObj = JObject.Parse(File.ReadAllText(file.FullName));
            }
            catch
            {
                // ignored
            }

        return Load(jObj, objects, toIdentifier, toName);
    }

    /// <summary>
    /// Load a given FileSystem from file, using an enumeration of data values, a function that corresponds a data value to its 
    /// identifier and a function that corresponds a data value not stored in the saved filesystem to its name.
    /// </summary>
    protected bool Load(JObject? jObject, IEnumerable<T> objects, Func<T, string> toIdentifier, Func<T, string> toName)
    {
        IdCounter = 1;
        Root.Children.Clear();

        string typeName = typeof(T).Name; // Get the type name (e.g., "CursedItem")
        bool changes = true;
        if (jObject != null)
        {
            changes = false;
            try
            {
                Dictionary<string, string> data         = jObject["Data"]?.ToObject<Dictionary<string, string>>() ?? new Dictionary<string, string>();
                string[] emptyFolders = jObject["EmptyFolders"]?.ToObject<string[]>() ?? Array.Empty<string>();

                foreach (T value in objects)
                {
                    string identifier = toIdentifier(value);
                    // If the data has a path in the filesystem, create all necessary folders and set the leaf.
                    if (data.TryGetValue(identifier, out string? path))
                    {
                        data.Remove(identifier);
                        string[] split = path.SplitDirectories();
                        (Result result, Folder folder) = CreateAllFolders(split[..^1]);
                        if (result is not Result.Success and not Result.SuccessNothingDone)
                        {
                            changes = true;
                            continue;
                        }

                        Leaf leaf = new Leaf(folder, split[^1], value, IdCounter++);
                        while (SetChild(folder, leaf, out _) == Result.ItemExists)
                        {
                            leaf.SetName(leaf.Name.IncrementDuplicate());
                            changes = true;
                        }
                    }
                    else
                    {
                        // Add a new leaf using the given toName function.
                        Leaf leaf = new Leaf(Root, toName(value), value, IdCounter++);
                        while (SetChild(Root, leaf, out _) == Result.ItemExists)
                        {
                            leaf.SetName(leaf.Name.IncrementDuplicate());
                            changes = true;
                        }
                    }
                }

                // Add all empty folders.
                foreach (string[]? split in emptyFolders.Select(folder => folder.SplitDirectories()))
                {
                    (Result result, Folder _) = CreateAllFolders(split);
                    if (result is not Result.Success and not Result.SuccessNothingDone)
                        changes = true;
                }
            }
            catch
            {
                changes = true;
            }
        }

        Changed?.Invoke(FileSystemChangeType.Reload, Root, null, null);
        return changes;
    }

    /// <summary>
    ///     Load a given FileSystem from pre-cached folder route addresses and pathings over the conventional file reading.
    /// </summary>
    protected bool Load(Dictionary<string, string> data, string[] emptyFolders, IEnumerable<T> objects, Func<T, string> toId, Func<T, string> toName)
    {
        IdCounter = 1;
        Root.Children.Clear();
        bool changes = false;
        try
        {
            foreach (T value in objects)
            {
                string identifier = toId(value);
                // If the data has a path in the filesystem, create all necessary folders and set the leaf.
                if (data.TryGetValue(identifier, out string? path))
                {
                    data.Remove(identifier);
                    string[] split = path.SplitDirectories();
                    (Result result, Folder folder) = CreateAllFolders(split[..^1]);
                    if (result is not Result.Success and not Result.SuccessNothingDone)
                    {
                        changes = true;
                        continue;
                    }

                    Leaf leaf = new Leaf(folder, split[^1], value, IdCounter++);
                    while (SetChild(folder, leaf, out _) == Result.ItemExists)
                    {
                        leaf.SetName(leaf.Name.IncrementDuplicate());
                        changes = true;
                    }
                }
                else
                {
                    // Add a new leaf using the given toName function.
                    Leaf leaf = new Leaf(Root, toName(value), value, IdCounter++);
                    while (SetChild(Root, leaf, out _) == Result.ItemExists)
                    {
                        leaf.SetName(leaf.Name.IncrementDuplicate());
                        changes = true;
                    }
                }
            }

            // Add all empty folders.
            foreach (string[]? split in emptyFolders.Select(folder => folder.SplitDirectories()))
            {
                (Result result, Folder _) = CreateAllFolders(split);
                if (result is not Result.Success and not Result.SuccessNothingDone)
                    changes = true;
            }
        }
        catch
        {
            changes = true;
        }

        Changed?.Invoke(FileSystemChangeType.Reload, Root, null, null);
        return changes;
    }
}
