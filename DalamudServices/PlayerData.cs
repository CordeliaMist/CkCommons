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
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using PlayerState = FFXIVClientStructs.FFXIV.Client.Game.UI.PlayerState;
using LuminaWorld = Lumina.Excel.Sheets.World;
#nullable disable

namespace CkCommons;

/// <summary> 
///     Static Accessor for everything Player Related one might need to access. <para />
///     Many of these calls can be made off the main thread and will still be valid.
/// </summary>
public static unsafe class PlayerData
{
    // Temporary placeholder for people who absolutely need it.
    public static IPlayerCharacter PlayerChara => Svc.Objects.LocalPlayer;

    // Could use GameObjectManager.Instance()->Objects.IndexSorted[0].Value also.
    public static GameObject*   Object      => (GameObject*)BattleChara;
    public static Character*    Character   => (Character*)BattleChara;
    public static BattleChara*  BattleChara => Control.Instance()->LocalPlayer;
    public static nint Address => (nint)BattleChara;
    public static bool Available => Control.Instance()->LocalPlayer is not null;
    public static bool Interactable => Available && Object->GetIsTargetable();

    // Info about local player.
    public static ulong EntityId => Available ? Object->EntityId : 0;
    public static ulong GameObjectId => Available ? Object->GetGameObjectId().ObjectId : ulong.MaxValue;
    public static ushort ObjIndex => Available ? Object->ObjectIndex : ushort.MaxValue;
    public static nint DrawObjAddress => Available ? (nint)Object->DrawObject : nint.Zero;
    public static ulong RenderFlags => Available ? (ulong)Object->RenderFlags : 0;
    public static bool HasModelInSlotLoaded => Available ? ((CharacterBase*)Object->DrawObject)->HasModelInSlotLoaded != 0 : false;
    public static bool HasModelFilesInSlotLoaded => Available ? ((CharacterBase*)Object->DrawObject)->HasModelFilesInSlotLoaded != 0 : false;


    // Overview (I have not tested PlayerState results yet, but can use FFXIVClientStructs.FFXIV.Client.Game.UI.PlayerState to optimize calls a bit.
    public static string Name => Character->NameString ?? string.Empty;
    public static string CharacterName => PlayerState.Instance()->IsLoaded ? PlayerState.Instance()->CharacterNameString : string.Empty;
    public static string NameWithWorld => Character->GetNameWithWorld();
    public static ulong  CID => PlayerState.Instance()->ContentId;
    public static string GetNameWithWorld(this Character chara)
        => chara.NameString + "@" + (Svc.Data.GetExcelSheet<LuminaWorld>().GetRowOrDefault(chara.HomeWorld) is { } w ? w.Name.ToString() : string.Empty);


    // Could have been simple as new(BattleChara->GetStatusManager()), but they made that internal.
    public static StatusList StatusList => StatusList.CreateStatusListReference((nint)BattleChara->GetStatusManager())!;
    public static byte Sex => PlayerState.Instance()->Sex;
    public static int Level => Svc.PlayerState.Level;
    public static byte MaxLevel => PlayerState.Instance()->MaxLevel;
    public static bool IsLevelSynced => PlayerState.Instance()->IsLevelSynced;
    public static int SyncedLevel => PlayerState.Instance()->SyncedLevel;
    public static int GetUnsyncedLevel(uint job) => PlayerState.Instance()->ClassJobLevels[Svc.Data.GetExcelSheet<ClassJob>().GetRowOrDefault(job).Value.ExpArrayIndex];
    public static uint Health => Control.Instance()->LocalPlayer->Health;
    public static uint Mana => Control.Instance()->LocalPlayer->Mana;
    public static uint CurrentHp => Control.Instance()->LocalPlayer->Character.CharacterData.Health;
    public static uint CurrentMp => Control.Instance()->LocalPlayer->Character.CharacterData.Mana;


    // Excel related information.
    public static RowRef<Race> Race => Svc.PlayerState.Race;
    public static RowRef<Tribe> Tribe => CreateRef<Tribe>(PlayerState.Instance()->Tribe);
    public static RowRef<LuminaWorld> HomeWorld => Svc.PlayerState.HomeWorld;
    public static RowRef<LuminaWorld> CurrentWorld => Svc.PlayerState.CurrentWorld;
    public static RowRef<WorldDCGroupType> HomeDateCenter => HomeWorld.Value.DataCenter;
    public static RowRef<WorldDCGroupType> CurrentDataCenter => CurrentWorld.Value.DataCenter;
    public static RowRef<TerritoryType> Territory => CreateRef<TerritoryType>(GameMain.Instance()->CurrentTerritoryTypeId);
    public static RowRef<ClassJob> ClassJob => Svc.PlayerState.ClassJob;
    public static RowRef<OnlineStatus> OnlineStatus => CreateRef<OnlineStatus>(BattleChara->OnlineStatus);
    public static RowRef<ContentFinderCondition> ContentFinderCondition => CreateRef<ContentFinderCondition>(GameMain.Instance()->CurrentContentFinderConditionId);

    // World Names
    public static ushort HomeWorldId => Control.Instance()->LocalPlayer->HomeWorld;
    public static ushort CurrentWorldId => Control.Instance()->LocalPlayer->CurrentWorld;
    public static uint JobId => Svc.PlayerState.ClassJob.RowId;
    public static ActionRoles JobRole => (ActionRoles)(ClassJob.Value.Role);


    // World IDs
    public static string HomeWorldName => HomeWorld.ValueNullable?.Name.ToString() ?? string.Empty;
    public static string CurrentWorldName => CurrentWorld.ValueNullable?.Name.ToString() ?? string.Empty;
    public static string HomeDataCenterName => HomeWorld.ValueNullable?.DataCenter.ValueNullable?.Name.ToString() ?? string.Empty;
    public static string CurrentDataCenterName => CurrentWorld.ValueNullable?.DataCenter.ValueNullable?.Name.ToString() ?? string.Empty;

    public static bool IsInHomeWorld => Available && CurrentWorld.RowId == HomeWorld.RowId;
    public static bool IsInHomeDC => Available && CurrentWorld.Value.DataCenter.RowId == HomeWorld.Value.DataCenter.RowId;

    // World Space.
    public static Vector3 Position => Control.Instance()->LocalPlayer->Position;
    public static float Rotation => Control.Instance()->LocalPlayer->Rotation;
    public static bool IsMoving => Available && (AgentMap.Instance()->IsPlayerMoving || IsJumping);
    public static bool IsJumping => Available && (Svc.Condition[ConditionFlag.Jumping] || Svc.Condition[ConditionFlag.Jumping61] || Character->IsJumping());
    public static bool Mounted => Svc.Condition[ConditionFlag.Mounted];
    public static bool Mounting => Svc.Condition[ConditionFlag.MountOrOrnamentTransition];
    public static bool CanMount => Svc.Data.GetExcelSheet<TerritoryType>().GetRow(PlayerContent.TerritoryID).Mount && PlayerState.Instance()->NumOwnedMounts > 0;
    public static bool CanFly => Control.CanFly;

    public static float DistanceTo(Vector3 other) => Vector3.Distance(Position, other);
    public static float DistanceTo(Vector2 other) => Vector2.Distance(new Vector2(Position.X, Position.Z), other);
    public static float DistanceTo(IGameObject other) => Vector3.Distance(Position, other.Position);


    // Dead or Alive?
    public static float AnimationLock => ActionManager.Instance()->AnimationLock;
    public static bool IsAnimationLocked => AnimationLock > 0;
    public static bool IsCasting => Available && Control.Instance()->LocalPlayer->IsCasting;
    public static bool IsDead => Svc.Condition[ConditionFlag.Unconscious];
    public static bool Revivable => IsDead && AgentRevive.Instance()->ReviveState != 0;


    // Additional information.
    public static unsafe short Commendations => PlayerState.Instance()->PlayerCommendations;
    public static byte GrandCompany => PlayerState.Instance()->GrandCompany;
    public static unsafe bool IsInDuty => GameMain.Instance()->CurrentContentFinderConditionId is not 0; // alternative method from IDutyState
    public static unsafe bool IsOnIsland => MJIManager.Instance()->IsPlayerInSanctuary;
    public static bool InPvP => GameMain.IsInPvPInstance();
    public static bool InGPose => GameMain.IsInGPose();
    public static bool IsLoggedIn => Svc.ClientState.IsLoggedIn;
    public static bool InQuestEvent => Svc.Condition[ConditionFlag.OccupiedInQuestEvent];
    public static bool IsChocoboRacing => Svc.Condition[ConditionFlag.ChocoboRacing];
    public static bool IsZoning => Svc.Condition[ConditionFlag.BetweenAreas] || Svc.Condition[ConditionFlag.BetweenAreas51];
    public static bool InDungeonDuty => Svc.Condition[ConditionFlag.BoundByDuty] || Svc.Condition[ConditionFlag.BoundByDuty56] || Svc.Condition[ConditionFlag.BoundByDuty95] || Svc.Condition[ConditionFlag.InDeepDungeon];
    public static bool InCutscene => !InDungeonDuty && Svc.Condition[ConditionFlag.OccupiedInCutSceneEvent] || Svc.Condition[ConditionFlag.WatchingCutscene78];

    public static int PartySize => Svc.Party.Length;
    public static bool InSoloParty => Svc.Party.Length <= 1 && IsInDuty;

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

    public static RowRef<T> CreateRef<T>(uint rowId) where T : struct, IExcelRow<T>
        => new(Svc.Data.Excel, rowId);
}
