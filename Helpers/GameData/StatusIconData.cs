using Dalamud.Utility;
using Lumina.Excel.Sheets;

namespace CkCommons;

public enum IconType : byte
{
    Positive,
    Negative,
    Special
}

/// <summary>
///     Helper for parsing status icons.
/// </summary>
public struct StatusIconData
{
    /// <summary>
    ///     The Icons Name.
    /// </summary>
    public string Name;

    /// <summary>
    ///     It's ID
    /// </summary>
    public uint IconID;

    /// <summary> 
    ///     The Type of Status it is.
    /// </summary>
    public IconType Type;

    /// <summary>
    ///     If this Status Icon is stackable.
    /// </summary>
    public bool IsStackable;

    /// <summary>
    ///     The ClassJob Category the StatusIcon is associated with.
    /// </summary>
    public ClassJobCategory ClassJobCategory;

    /// <summary>
    ///     If it is an FC Buff
    /// </summary>
    public bool IsFCBuff;

    /// <summary>
    ///     The Description of the StatusIconData.
    /// </summary>
    public string Description;

    public StatusIconData(Status status)
    {
        Name = status.Name.ToDalamudString().ExtractText();
        IconID = status.Icon;
        Type = status.CanIncreaseRewards == 1 
            ? IconType.Special : (status.StatusCategory == 2 ? IconType.Negative : IconType.Positive);
        ClassJobCategory = status.ClassJobCategory.Value;
        IsFCBuff = status.IsFcBuff;
        IsStackable = status.MaxStacks > 1;
        Description = status.Description.ToDalamudString().ExtractText();
    }
}
