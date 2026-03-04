using Lumina.Excel.Sheets;
using System.Collections.ObjectModel;

namespace CkCommons;

// Adopted from ECommons helpers from tedious segments I cant be arsed to mimic.
// https://github.com/NightmareXIV/ECommons/blob/master/ECommons/ExcelServices/ExcelJobHelper.cs

public static class JobHelper
{
    public static readonly ReadOnlyDictionary<JobType, JobType> Upgrades = new Dictionary<JobType, JobType>()
    {
        [JobType.GLA] = JobType.PLD,
        [JobType.PGL] = JobType.MNK,
        [JobType.MRD] = JobType.WAR,
        [JobType.LNC] = JobType.DRG,
        [JobType.ARC] = JobType.BRD,
        [JobType.CNJ] = JobType.WHM,
        [JobType.THM] = JobType.BLM,
        [JobType.ACN] = JobType.SMN,
        [JobType.ROG] = JobType.NIN,
    }.AsReadOnly();

    public static JobType GetUpgradedJob(this JobType j)
    {
        if (Upgrades.TryGetValue(j, out var job))
            return job;
        return j;
    }

    public static JobType GetDowngradedJob(this JobType job)
    {
        foreach (var kv in Upgrades)
            if (kv.Value == job)
                return kv.Key;
        return job;
    }

    public static bool IsJobInCategory(this ClassJobCategory cat, JobType job)
    {
        if (job == JobType.ADV && cat.ADV) return true;
        if (job == JobType.GLA && cat.GLA) return true;
        if (job == JobType.PGL && cat.PGL) return true;
        if (job == JobType.MRD && cat.MRD) return true;
        if (job == JobType.LNC && cat.LNC) return true;
        if (job == JobType.ARC && cat.ARC) return true;
        if (job == JobType.CNJ && cat.CNJ) return true;
        if (job == JobType.THM && cat.THM) return true;
        if (job == JobType.CRP && cat.CRP) return true;
        if (job == JobType.BSM && cat.BSM) return true;
        if (job == JobType.ARM && cat.ARM) return true;
        if (job == JobType.GSM && cat.GSM) return true;
        if (job == JobType.LTW && cat.LTW) return true;
        if (job == JobType.WVR && cat.WVR) return true;
        if (job == JobType.ALC && cat.ALC) return true;
        if (job == JobType.CUL && cat.CUL) return true;
        if (job == JobType.MIN && cat.MIN) return true;
        if (job == JobType.BTN && cat.BTN) return true;
        if (job == JobType.FSH && cat.FSH) return true;
        if (job == JobType.PLD && cat.PLD) return true;
        if (job == JobType.MNK && cat.MNK) return true;
        if (job == JobType.WAR && cat.WAR) return true;
        if (job == JobType.DRG && cat.DRG) return true;
        if (job == JobType.BRD && cat.BRD) return true;
        if (job == JobType.WHM && cat.WHM) return true;
        if (job == JobType.BLM && cat.BLM) return true;
        if (job == JobType.ACN && cat.ACN) return true;
        if (job == JobType.SMN && cat.SMN) return true;
        if (job == JobType.SCH && cat.SCH) return true;
        if (job == JobType.ROG && cat.ROG) return true;
        if (job == JobType.NIN && cat.NIN) return true;
        if (job == JobType.MCH && cat.MCH) return true;
        if (job == JobType.DRK && cat.DRK) return true;
        if (job == JobType.AST && cat.AST) return true;
        if (job == JobType.SAM && cat.SAM) return true;
        if (job == JobType.RDM && cat.RDM) return true;
        if (job == JobType.BLU && cat.BLU) return true;
        if (job == JobType.GNB && cat.GNB) return true;
        if (job == JobType.DNC && cat.DNC) return true;
        if (job == JobType.RPR && cat.RPR) return true;
        if (job == JobType.SGE && cat.SGE) return true;
        if (job == JobType.VPR && cat.VPR) return true;
        if (job == JobType.PCT && cat.PCT) return true;
        return false;
    }
}

public enum JobType : uint
{
    /// <summary>
    /// Adventurer 
    /// </summary>
    ADV = 0,

    /// <summary>
    /// Gladiator 
    /// </summary>
    GLA = 1,

    /// <summary>
    /// Pugilist 
    /// </summary>
    PGL = 2,

    /// <summary>
    /// Marauder 
    /// </summary>
    MRD = 3,

    /// <summary>
    /// Lancer 
    /// </summary>
    LNC = 4,

    /// <summary>
    /// Archer 
    /// </summary>
    ARC = 5,

    /// <summary>
    /// Conjurer 
    /// </summary>
    CNJ = 6,

    /// <summary>
    /// Thaumaturge
    /// </summary>
    THM = 7,

    /// <summary>
    /// Carpenter
    /// </summary>
    CRP = 8,

    /// <summary>
    /// Blacksmith
    /// </summary>
    BSM = 9,

    /// <summary>
    /// Armorer
    /// </summary>
    ARM = 10,

    /// <summary>
    /// Goldsmith
    /// </summary>
    GSM = 11,

    /// <summary>
    /// Leatherworker
    /// </summary>
    LTW = 12,

    /// <summary>
    /// Weaver
    /// </summary>
    WVR = 13,

    /// <summary>
    /// Alchemist
    /// </summary>
    ALC = 14,

    /// <summary>
    /// Culinarian
    /// </summary>
    CUL = 15,

    /// <summary>
    /// Miner
    /// </summary>
    MIN = 16,

    /// <summary>
    /// Botanist
    /// </summary>
    BTN = 17,

    /// <summary>
    /// Fisher
    /// </summary>
    FSH = 18,

    /// <summary>
    /// Paladin 
    /// </summary>
    PLD = 19,

    /// <summary>
    /// Monk 
    /// </summary>
    MNK = 20,

    /// <summary>
    /// Warrior 
    /// </summary>
    WAR = 21,

    /// <summary>
    /// Dragoon 
    /// </summary>
    DRG = 22,

    /// <summary>
    /// Bard 
    /// </summary>
    BRD = 23,

    /// <summary>
    /// WhiteMage 
    /// </summary>
    WHM = 24,

    /// <summary>
    /// BlackMage
    /// </summary>
    BLM = 25,

    /// <summary>
    /// Arcanist 
    /// </summary>
    ACN = 26,

    /// <summary>
    /// Summoner 
    /// </summary>
    SMN = 27,

    /// <summary>
    /// Scholar 
    /// </summary>
    SCH = 28,

    /// <summary>
    /// Rogue 
    /// </summary>
    ROG = 29,

    /// <summary>
    /// Ninja 
    /// </summary>
    NIN = 30,

    /// <summary>
    /// Machinist 
    /// </summary>
    MCH = 31,

    /// <summary>
    /// DarkKnight 
    /// </summary>
    DRK = 32,

    /// <summary>
    /// Astrologian 
    /// </summary>
    AST = 33,

    /// <summary>
    /// Samurai 
    /// </summary>
    SAM = 34,

    /// <summary>
    /// RedMage 
    /// </summary>
    RDM = 35,

    /// <summary>
    /// BlueMage 
    /// </summary>
    BLU = 36,

    /// <summary>
    /// Gunbreaker 
    /// </summary>
    GNB = 37,

    /// <summary>
    /// Dancer 
    /// </summary>
    DNC = 38,

    /// <summary>
    /// Reaper 
    /// </summary>
    RPR = 39,

    /// <summary>
    /// Sage 
    /// </summary>
    SGE = 40,

    /// <summary>
    /// Viper 
    /// </summary>
    VPR = 41,

    /// <summary>
    /// Pictomancer 
    /// </summary>
    PCT = 42,
}

