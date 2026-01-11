using Dalamud.Game.ClientState.Aetherytes;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;
using Aetheryte = Lumina.Excel.Sheets.Aetheryte;
using PlayerState = FFXIVClientStructs.FFXIV.Client.Game.UI.PlayerState;

namespace CkCommons;

/// <summary> 
///     Static Accessor for everything Player Related one might need to access.
/// </summary>
public static class PlayerContent
{
    private static Dictionary<uint, string> Cache = [];
    public static ushort TerritoryID => Svc.ClientState.TerritoryType;

    public static string TerritoryName
    {
        get
        {
            var t = Svc.Data.GetExcelSheet<TerritoryType>().GetRowOrDefault(TerritoryID);
            return $"{t?.ContentFinderCondition.ValueNullable?.Name.ToString() ?? (t?.PlaceName.ValueNullable?.Name.ToString())}";
        }
    }

    // I love you ECommons.
    public static string GetTerritoryName(ushort id)
    {
        if (Cache.TryGetValue(id, out var val))
            return val;

        if (Svc.Data.GetExcelSheet<TerritoryType>()!.GetRowOrDefault(id) is { } data)
        {
            var zoneName = data.PlaceName.ValueNullable?.Name.ToString() ?? "";
            if (zoneName != string.Empty)
            {
                if (data.ContentFinderCondition.ValueNullable is { } cfc)
                {
                    var cfcStr = cfc.Name.ToString();
                    if (cfcStr != string.Empty)
                    {
                        Cache[id] = $"{zoneName} ({cfcStr})";
                        return Cache[id];
                    }
                }
                Cache[id] = zoneName;
                return Cache[id];
            }
        }
        Cache[id] = id.ToString();
        return Cache[id];
    }

    public static unsafe ushort TerritoryIdInstanced => (ushort)GameMain.Instance()->CurrentTerritoryTypeId;
    public static uint Territory => Svc.ClientState.TerritoryType;
    public static IntendedUseEnum TerritoryIntendedUse => (IntendedUseEnum)(Svc.Data.GetExcelSheet<TerritoryType>().GetRowOrDefault(Territory)?.TerritoryIntendedUse.ValueNullable?.RowId ?? default);
    public unsafe static IAetheryteEntry? HomeAetheryte => Svc.AetheryteList[PlayerState.Instance()->HomeAetheryteId];
    public static bool InMainCity => Svc.Data.GetExcelSheet<Aetheryte>()?.Any(x => x.IsAetheryte && x.Territory.RowId == Territory && x.Territory.Value.TerritoryIntendedUse.Value.RowId is 0) ?? false;
    public static string MainCityName => Svc.Data.GetExcelSheet<Aetheryte>()?.FirstOrDefault(x => x.IsAetheryte && x.Territory.RowId == Territory && x.Territory.Value.TerritoryIntendedUse.Value.RowId is 0).PlaceName.ToString() ?? "Unknown";
    public static TerritoryType TerritoryType => Svc.Data.GetExcelSheet<TerritoryType>()?.GetRowOrDefault(Territory) ?? default;

    public static void OpenMapWithMapLink(MapLinkPayload mapLink) => Svc.GameGui.OpenMapWithMapLink(mapLink);
    public static DeepDungeonType GetDeepDungeonType()
    {
        if (Svc.Data.GetExcelSheet<TerritoryType>()?.GetRow(Svc.ClientState.TerritoryType) is { } territoryInfo)
        {
            return territoryInfo switch
            {
                { TerritoryIntendedUse.Value.RowId: 31, ExVersion.RowId: 0 or 1 } => DeepDungeonType.PalaceOfTheDead,
                { TerritoryIntendedUse.Value.RowId: 31, ExVersion.RowId: 2 } => DeepDungeonType.HeavenOnHigh,
                { TerritoryIntendedUse.Value.RowId: 31, ExVersion.RowId: 4 } => DeepDungeonType.EurekaOrthos,
                _ => DeepDungeonType.Unknown,
            };
        }
        return DeepDungeonType.Unknown;
    }

}
