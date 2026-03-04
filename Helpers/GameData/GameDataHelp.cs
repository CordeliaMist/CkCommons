using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Lumina.Excel.Sheets;

namespace CkCommons;

// Lifestream
// https://github.com/NightmareXIV/Lifestream/blob/main/Lifestream/Data/AddressBookEntry.cs#L1
public enum ResidentialAetheryteKind
{
    Uldah = 9,
    Gridania = 2,
    Limsa = 8,
    Foundation = 70,
    Kugane = 111,
}

public enum PropertyType
{
    House, Apartment, PrivateChambers
}

public static class GameDataHelp
{
    public static readonly Dictionary<ResidentialAetheryteKind, string> ResidentialNames = new()
    {
        [ResidentialAetheryteKind.Gridania] = "Lav. Beds",
        [ResidentialAetheryteKind.Limsa] = "Mist",
        [ResidentialAetheryteKind.Uldah] = "Goblet",
        [ResidentialAetheryteKind.Kugane] = "Shirogane",
        [ResidentialAetheryteKind.Foundation] = "Empyreum",
    };

    /// <summary>
    ///     Get a VFX path from a given status icon ID.
    /// </summary>
    /// <remarks> Can return an empty string if not found. </remarks>
    public static string GetVfxPathByID(uint iconID)
    {
        foreach (var x in Svc.Data.GetExcelSheet<Status>())
        {
            if (x.Icon == iconID)
                return x.HitEffect.ValueNullable?.Location.ValueNullable?.Location.ExtractText() ?? string.Empty;

            if (x.MaxStacks > 1 && iconID >= x.Icon + 1 && iconID < x.Icon + x.MaxStacks)
                return x.HitEffect.ValueNullable?.Location.ValueNullable?.Location.ExtractText() ?? string.Empty;
        }
        return string.Empty;
    }

    /// <summary>
    ///     Get a VFX related path from a given .avfx name.
    /// </summary>
    /// <remarks> Can return an empty string if not found. </remarks>
    public static string GetVfxPath(string path)
        => string.IsNullOrEmpty(path) ? string.Empty : $"vfx/common/eff/{path}.avfx";

    public static string ExtractText(this SeString seStr, bool onlyFirst = false)
    {
        StringBuilder sb = new();
        foreach (var x in seStr.Payloads)
        {
            if (x is TextPayload tp)
            {
                sb.Append(tp.Text);
                if (onlyFirst) break;
            }
            if (x.Type == PayloadType.Unknown && x.Encode().SequenceEqual<byte>([0x02, 0x1d, 0x01, 0x03]))
            {
                sb.Append(' ');
            }
        }
        return sb.ToString();
    }
}
