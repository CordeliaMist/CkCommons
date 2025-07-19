namespace CkCommons;

public enum DeepDungeonType
{
    Unknown,
    PalaceOfTheDead,
    HeavenOnHigh,
    EurekaOrthos,
}


// Pulled from:
// https://github.com/NightmareXIV/ECommons/blob/71ee09f7cc2230a73503b945422760da1368405c/ECommons/ExcelServices/TerritoryIntendedUseEnum.cs

/// <summary>
/// Because the new Lumina Territory Intended use is all listed as Unknowns, 
/// this parses the proper intended use of the territory.
/// </summary>
public enum TerritoryIntendedUseEnum : uint
{
    [CommonlyUsed] City_Area = 0,
    [CommonlyUsed] Open_World = 1,
    [CommonlyUsed] Inn = 2,
    [CommonlyUsed] Dungeon = 3,
    [CommonlyUsed] Variant_Dungeon = 4,
    Gaol = 5,
    Starting_Area = 6,
    Quest_Area = 7,
    [CommonlyUsed] Alliance_Raid = 8,
    Quest_Battle = 9,
    [CommonlyUsed] Trial = 10,
    Quest_Area_2 = 12,
    [CommonlyUsed] Residential_Area = 13,
    [CommonlyUsed] Housing_Instances = 14,
    Quest_Area_3 = 15,
    Raid = 16,
    [CommonlyUsed] Raid_2 = 17,
    Frontline = 18,
    Chocobo_Square = 20,
    Restoration_Event = 21,
    Sanctum = 22,
    Gold_Saucer = 23,
    Lord_of_Verminion = 25,
    Diadem = 26,
    Hall_of_the_Novice = 27,
    Crystalline_Conflict = 28,
    Quest_Battle_2 = 29,
    Barracks = 30,
    [CommonlyUsed] Deep_Dungeon = 31,
    Seasonal_Event = 32,
    Treasure_Map_Duty = 33,
    Seasonal_Event_Duty = 34,
    Battlehall = 35,
    Crystalline_Conflict_2 = 37,
    Diadem_2 = 38,
    Rival_Wings = 39,
    Unknown_1 = 40,
    [CommonlyUsed] Eureka = 41,
    Seasonal_Event_2 = 43,
    Leap_of_Faith = 44,
    Masked_Carnivale = 45,
    Ocean_Fishing = 46,
    Diadem_3 = 47,
    [CommonlyUsed] Bozja = 48,
    [CommonlyUsed] Island_Sanctuary = 49,
    Battlehall_2 = 50,
    Battlehall_3 = 51,
    [CommonlyUsed] Large_Scale_Raid = 52,
    [CommonlyUsed] Large_Scale_Savage_Raid = 53,
    Quest_Area_4 = 54,
    Tribal_Instance = 56,
    [CommonlyUsed] Criterion_Duty = 57,
    [CommonlyUsed] Criterion_Savage_Duty = 58,
    Blunderville = 59,
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

// Only ones we care about for GagSpeak.
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
