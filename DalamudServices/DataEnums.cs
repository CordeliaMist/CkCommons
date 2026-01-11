namespace CkCommons;

public enum DeepDungeonType
{
    Unknown,
    PalaceOfTheDead,
    HeavenOnHigh,
    EurekaOrthos,
    PilgrimsTraverse,
}

/// <summary>
///     The TerritoryIntendedUse enum from FFXIVClientStructs. <br/>
///     Comes with attributes and rename to avoid conflict with Lumina.
/// </summary>
public enum IntendedUseEnum : byte
{
    [CommonlyUsed] Town,
    [CommonlyUsed] Overworld,
    [CommonlyUsed] Inn,
    [CommonlyUsed] Dungeon,
    [CommonlyUsed] VariantDungeon,
    MordionGaol,
    OpeningArea,
    BeforeTrialDung,
    [CommonlyUsed] AllianceRaid,
    PreEwOverworldQuestBattle,
    [CommonlyUsed] Trial,
    Unknown11,
    WaitingRoom,
    [CommonlyUsed] HousingOutdoor,
    [CommonlyUsed] HousingIndoor,
    SoloOverworldInstances,
    Raid1,
    [CommonlyUsed] Raid2,
    Frontline,
    ChocoboSquareOld,
    ChocoboRacing,
    Firmament,
    SanctumOfTheTwelve,
    GoldSaucer,
    OriginalStepsOfFaith,
    LordOfVerminion,
    ExploratoryMissions,
    HallOfTheNovice,
    CrystallineConflict,
    SoloDuty,
    GrandCompanyBarracks,
    [CommonlyUsed] DeepDungeon,
    Seasonal,
    TreasureMapInstance,
    SeasonalInstancedArea,
    TripleTriadBattlehall,
    ChaoticRaid,
    CrystallineConflictCustomMatch,
    HuntingGrounds,
    RivalWings,
    Unknown40,
    [CommonlyUsed] Eureka,
    Unknown42,
    TheCalamityRetold,
    LeapOfFaith,
    MaskedCarnival,
    OceanFishing,
    Diadem,
    [CommonlyUsed] Bozja,
    [CommonlyUsed] IslandSanctuary,
    TripleTriadOpenTournament,
    TripleTriadInvitationalParlor,
    [CommonlyUsed] DelubrumReginae,
    [CommonlyUsed] DelubrumReginaeSavage,
    EndwalkerMsqSoloOverworld,
    Unknown55,
    Elysion,
    [CommonlyUsed] CriterionDungeon,
    [CommonlyUsed] CriterionDungeonSavage,
    Blunderville,
    CosmicExploration,
    OccultCrescent,
    Unknown62
}

public class CommonlyUsedAttribute : Attribute { }

public enum JobRole : byte
{
    Unknown = 0,
    Tank = 1,
    Melee = 2,
    RangedPhysical = 3,
    RangedMagical = 4,
    Healer = 5,
    Crafter = 6,
    Gatherer = 7,
}

public enum ActionRoles : byte
{
    NonCombat = 0,
    Tank = 0x1,
    MeleeDps = 0x2,
    RangedDps = 0x3,
    Healer = 0x4
}

// Only ones we care about for GSpeak.
public enum LimitedActionEffectType : byte
{
    Nothing = 0,
    Miss = 1,
    Damage = 3,
    Heal = 4,
    BlockedDamage = 5,
    ParriedDamage = 6,
    Knockback = 32,
    Attract1 = 33, //Here is an issue bout knockback. some is 32 some is 33.
}


// ActionEffectTypes from ReceiveActionEffect calls.
// https://github.com/NightmareXIV/ECommons/blob/master/ECommons/Hooks/ActionEffectTypes/ActionEffectType.cs
public enum ActionEffectType : byte
{
    Nothing = 0,
    Miss = 1,
    FullResist = 2,
    Damage = 3,
    Heal = 4,
    BlockedDamage = 5,
    ParriedDamage = 6,
    Invulnerable = 7,
    NoEffectText = 8,
    Unknown_0 = 9,
    MpLoss = 10,
    MpGain = 11,
    TpLoss = 12,
    TpGain = 13,

    ApplyStatusEffectTarget = 14,
    [Obsolete("Please use ApplyStatusEffectTarget instead.")]
    GpGain = ApplyStatusEffectTarget,

    ApplyStatusEffectSource = 15,
    RecoveredFromStatusEffect = 16,
    LoseStatusEffectTarget = 17,
    LoseStatusEffectSource = 18,
    StatusNoEffect = 20,
    ThreatPosition = 24,
    EnmityAmountUp = 25,
    EnmityAmountDown = 26,

    StartActionCombo = 27,
    [Obsolete("Please use StartActionCombo instead.")]
    Unknown0 = StartActionCombo,

    ComboSucceed = 28,
    [Obsolete("Please use ComboSucceed instead.")]
    Unknown1 = ComboSucceed,

    Retaliation = 29,
    Knockback = 32,
    Attract1 = 33, //Here is an issue bout knockback. some is 32 some is 33.
    Attract2 = 34,
    Mount = 40,
    FullResistStatus = 52,
    FullResistStatus2 = 55,
    VFX = 59,
    Gauge = 60,
    JobGauge = 61,
    Shirk = 62, // temp filler from what i understand.
    SetModelState = 72,
    SetHP = 73,
    PartialInvulnerable = 74,
    Interrupt = 75,
};
