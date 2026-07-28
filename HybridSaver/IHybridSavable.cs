using System.IO;

namespace CkCommons.HybridSaver;

public interface IHybridSavable<in T> where T : IConfigFileProvider
{
    /// <summary> The current version of the configuration file. </summary>
    public int ConfigVersion { get; }

    /// <summary> Determines which SaveMethod is used. </summary>
    public HybridSaveType SaveType { get; }

    /// <summary> The last time this file was written to. </summary>
    /// <remarks> Used for knowing when to make a backup of the file. </remarks>
    public DateTime LastWriteTimeUTC { get; }

    public int MaxBackups { get; }

    public string ToFilePath(T filenameService);

    /// <summary> The Save method used if SaveType is Json. </summary>
    /// <returns> The JSON Object </returns>
    public string JsonSerialize();

    /// <summary> The Save method used if SaveType is StreamWrite. </summary>
    /// <param name="writer"> the writer passed in from the SaveService. </param>
    public void WriteToStream(StreamWriter writer);

    /// <summary> The displayed name of the type. </summary>
    public string TypeName
        => GetType().Name;
}
