using Dalamud.Game;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.MJI;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using PlayerState = FFXIVClientStructs.FFXIV.Client.Game.UI.PlayerState;
#nullable disable

namespace CkCommons;

/// <summary> 
///     Static Accessor for everything Player Related one might need to access.
/// </summary>
public static unsafe class PlayerData
{
    public static readonly int MaxLevel = 100;
    public static ClientLanguage Language => Svc.ClientState.ClientLanguage;
    public static IPlayerCharacter Object => Svc.Objects.LocalPlayer;
    public static IntPtr ObjectAddress => Svc.Objects.LocalPlayer?.Address ?? IntPtr.Zero;
    public static GameObject* ObjectThreadSafe => GameObjectManager.Instance()->Objects.IndexSorted[0].Value;
    public static bool Available => Svc.Objects.LocalPlayer != null;
    public unsafe static bool AvailableThreadSafe => GameObjectManager.Instance()->Objects.IndexSorted[0].Value != null;
    public static bool Interactable => Available && Object.IsTargetable;
    public static Vector3 PositionInstanced => Control.Instance()->LocalPlayer->Position;
    public static ulong ContentId => Svc.PlayerState.ContentId;
    public static ulong ContentIdInstanced => Control.Instance()->LocalPlayer->ContentId;
    public static StatusList Status => Object? .StatusList;

    // Name & World Info
    public static ushort HomeWorldId => (ushort)Svc.PlayerState.HomeWorld.RowId;
    public static ushort CurrentWorldId => (ushort)Svc.PlayerState.CurrentWorld.RowId;
    public static ushort HomeWorldIdInstanced => Control.Instance()->LocalPlayer->HomeWorld;
    public static ushort CurrentWorldIdInstanced => Control.Instance()->LocalPlayer->CurrentWorld;
    public static string Name => Svc.PlayerState.CharacterName;
    public static string HomeWorld => Svc.PlayerState.HomeWorld.Value.Name.ToString();
    public static string NameWithWorld => GetNameWithWorld(Object);
    public static string CurrentWorld => Svc.PlayerState.CurrentWorld.Value.Name.ToString();
    public static string HomeDataCenter => Svc.PlayerState.HomeWorld.Value.DataCenter.Value.Name.ToString();
    public static string CurrentDataCenter => Svc.PlayerState.CurrentWorld.Value.DataCenter.Value.Name.ToString();
    public static string GetNameWithWorld(this IPlayerCharacter pc) => pc is null ? string.Empty : (pc.Name.ToString() + "@" + pc.HomeWorld.Value.Name.ToString());

    public static string NameInstanced => Control.Instance()->LocalPlayer->NameString ?? string.Empty;
    public static string HomeWorldInstanced => Svc.Data.GetExcelSheet<World>().GetRowOrDefault(HomeWorldIdInstanced) is { } w ? w.Name.ToString() : string.Empty;
    public static string CurrentWorldInstanced => Svc.Data.GetExcelSheet<World>().GetRowOrDefault(CurrentWorldIdInstanced) is { } w ? w.Name.ToString() : string.Empty;
    public static string NameWithWorldInstanced => NameInstanced + "@" + HomeWorldInstanced;
    public static unsafe string GetNameWorldUnsafe(Character* character)
        => character->NameString + "@" + (Svc.Data.GetExcelSheet<World>().GetRowOrDefault(character->HomeWorld) is { } w ? w.Name.ToString() : string.Empty);

    public static uint OnlineStatus => Object?.OnlineStatus.RowId ?? 0;
    public static unsafe short Commendations => PlayerState.Instance()->PlayerCommendations;
    public static bool IsInHomeWorld => Available && Svc.PlayerState.CurrentWorld.RowId == Svc.PlayerState.HomeWorld.RowId;
    public static bool IsInHomeDC => Available && Svc.PlayerState.CurrentWorld.Value.DataCenter.RowId == Svc.PlayerState.HomeWorld.Value.DataCenter.RowId;
    public static unsafe bool IsInDuty => GameMain.Instance()->CurrentContentFinderConditionId is not 0; // alternative method from IDutyState
    public static unsafe bool IsOnIsland => MJIManager.Instance()->IsPlayerInSanctuary;
    public static bool IsInPvP => GameMain.IsInPvPInstance();
    public static bool IsInGPose => GameMain.IsInGPose();

    public static uint Health => Available ? Object!.CurrentHp : 0;
    public static uint HealthInstanced => Control.Instance()->LocalPlayer->Health;
    public static int Level => Svc.PlayerState.Level;
    public static bool IsLevelSynced => PlayerState.Instance()->IsLevelSynced;
    public static int SyncedLevel => PlayerState.Instance()->SyncedLevel;
    public static int UnsyncedLevel => GetUnsyncedLevel(JobId);
    public static int GetUnsyncedLevel(uint job) => PlayerState.Instance()->ClassJobLevels[Svc.Data.GetExcelSheet<ClassJob>().GetRowOrDefault(job).Value.ExpArrayIndex];

    public static RowRef<ClassJob> ClassJob => Svc.PlayerState.ClassJob;
    public static uint JobId => Svc.PlayerState.ClassJob.RowId;
    public static unsafe ushort JobIdInstanced => PlayerState.Instance()->CurrentClassJobId;
    public static ActionRoles JobRole => (ActionRoles)(Object?.ClassJob.Value.Role ?? 0);
    public static byte GrandCompany => PlayerState.Instance()->GrandCompany;

    public static bool IsLoggedIn => Svc.ClientState.IsLoggedIn;
    public static bool InQuestEvent => Svc.Condition[ConditionFlag.OccupiedInQuestEvent];
    public static bool IsChocoboRacing => Svc.Condition[ConditionFlag.ChocoboRacing];
    public static bool IsZoning => Svc.Condition[ConditionFlag.BetweenAreas] || Svc.Condition[ConditionFlag.BetweenAreas51];
    public static bool InDungeonDuty => Svc.Condition[ConditionFlag.BoundByDuty] || Svc.Condition[ConditionFlag.BoundByDuty56] || Svc.Condition[ConditionFlag.BoundByDuty95] || Svc.Condition[ConditionFlag.InDeepDungeon];
    public static bool InCutscene => !InDungeonDuty && Svc.Condition[ConditionFlag.OccupiedInCutSceneEvent] || Svc.Condition[ConditionFlag.WatchingCutscene78];
    public static bool CanFly => Control.CanFly;
    public static bool Mounted => Svc.Condition[ConditionFlag.Mounted];
    public static bool Mounting => Svc.Condition[ConditionFlag.MountOrOrnamentTransition];
    public static bool CanMount => Svc.Data.GetExcelSheet<TerritoryType>().GetRow(PlayerContent.TerritoryID).Mount && PlayerState.Instance()->NumOwnedMounts > 0;

    public static int PartySize => Svc.Party.Length;
    public static bool InSoloParty => Svc.Party.Length <= 1 && IsInDuty;

    public static Character* Character => (Character*)Object.Address;
    public static BattleChara* BattleChara => (BattleChara*)Object.Address;
    public static GameObject* GameObject => (GameObject*)Object.Address;

    public static Vector3 Position => Available ? Object!.Position : Vector3.Zero;
    public static float Rotation => Available ? Object!.Rotation : 0;
    public static bool IsMoving => Available && (AgentMap.Instance()->IsPlayerMoving || IsJumping);
    public static bool IsJumping => Available && (Svc.Condition[ConditionFlag.Jumping] || Svc.Condition[ConditionFlag.Jumping61] || Character->IsJumping());
    public static bool IsDead => Svc.Condition[ConditionFlag.Unconscious];
    public static bool Revivable => IsDead && AgentRevive.Instance()->ReviveState != 0;
    public static float AnimationLock => *(float*)((nint)ActionManager.Instance() + 8);
    public static bool IsAnimationLocked => AnimationLock > 0;
    public static float DistanceToInstanced(Vector3 other) => Vector3.Distance(PositionInstanced, other);
    public static float DistanceTo(Vector3 other) => Vector3.Distance(Position, other);
    public static float DistanceTo(Vector2 other) => Vector2.Distance(new Vector2(Position.X, Position.Z), other);
    public static float DistanceTo(IGameObject other) => Vector3.Distance(Position, other.Position);

    public static void OpenMapWithMapLink(MapLinkPayload mapLink) => Svc.GameGui.OpenMapWithMapLink(mapLink);
    public static DeepDungeonType? GetDeepDungeonType()
    {
        if (Svc.Data.GetExcelSheet<TerritoryType>()?.GetRow(Svc.ClientState.TerritoryType) is { } territoryInfo)
        {
            return territoryInfo switch
            {
                { TerritoryIntendedUse.Value.RowId: 31, ExVersion.RowId: 0 or 1 } => DeepDungeonType.PalaceOfTheDead,
                { TerritoryIntendedUse.Value.RowId: 31, ExVersion.RowId: 2 } => DeepDungeonType.HeavenOnHigh,
                { TerritoryIntendedUse.Value.RowId: 31, ExVersion.RowId: 4 } => DeepDungeonType.EurekaOrthos,
                _ => null
            };
        }
        return null;
    }

}
