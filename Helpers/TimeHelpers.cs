namespace CkCommons.Helpers;

public class TimeHelpers
{
    /// <summary>
    ///   The list of all Abreviations from https://www.timeanddate.com/time/zones/
    /// </summary>
    /// <remarks>
    ///   Needs to be maintained. Any entry could represent mutliple timezones. <br/>
    ///   Use primarily for identification and existance of.
    /// </remarks>
    public static readonly HashSet<string> TzAbbreviations = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "ACDT", "ACST", "ACT", "ACWST", "ADT", "AEDT", "AEST", "AET", "AFT", "AKDT",
        "AKST", "ALMT", "AMST", "AMT", "ANAST", "ANAT", "AoE", "AQTT", "ART", "AST",
        "AT", "AWDT", "AWST", "AZOST", "AZOT", "AZST", "AZT", "BNT", "BOT", "BRST",
        "BRT", "BST", "BTT", "CAST", "CAT", "CCT", "CDT", "CEST", "CET", "CHADT",
        "CHAST", "CHOST", "CHOT", "ChST", "CHUT", "CIDST", "CIST", "CKT", "CLST",
        "CLT", "COT", "CST", "CT", "CVT", "CXT", "DAVT", "DDUT", "EASST", "EAST",
        "EAT", "ECT", "EDT", "EEST", "EET", "EGST", "EGT", "EST", "ET", "FET",
        "FJST", "FJT", "FKST", "FKT", "FNT", "GALT", "GAMT", "GET", "GFT", "GILT",
        "GMT", "GST", "GYT", "HDT", "HST", "HKT", "HOVST", "HOVT", "ICT", "IDT",
        "IOT", "IRDT", "IRKST", "IRKT", "IRST", "IST", "JST", "KGT", "KOST", "KRAST",
        "KRAT", "KST", "KUYT", "LHDT", "LHST", "LINT", "MAGST", "MAGT", "MART",
        "MAWT", "MDT", "MHT", "MMT", "MSD", "MSK", "MST", "MT", "MUT", "MVT", "MYT",
        "NCT", "NDT", "NFDT", "NFT", "NOVST", "NOVT", "NPT", "NRT", "NST", "NUT",
        "NZDT", "NZST", "OMSST", "OMST", "ORAT", "PDT", "PET", "PETST", "PETT", "PGT",
        "PHOT", "PHT", "PKT", "PMDT", "PMST", "PONT", "PST", "PT", "PWT", "PYST",
        "PYT", "QYZT", "RET", "SAKT", "SAMT", "SAST", "SBT", "SCT", "SGT", "SRET",
        "SRT", "SST", "SYOT", "TAHT", "TFT", "TJT", "TKT", "TLT", "TMT", "TOST",
        "TOT", "TRT", "TVT", "ULAST", "ULAT", "UTC", "UYST", "UYT", "UZT", "VET",
        "VLAST", "VLAT", "VOST", "VUT", "WAKT", "WARST", "WAST", "WAT", "WEST",
        "WET", "WFT", "WGST", "WGT", "WIB", "WIT", "WITA", "WST", "WT", "YAKST",
        "YAKT", "YAPT", "YEKST", "YEKT"
    };

    public static bool TryParseTimeSpan(string input, out TimeSpan result)
    {
        result = TimeSpan.Zero;
        var regex = new Regex(@"^\s*(?:(\d+)d\s*)?\s*(?:(\d+)h\s*)?\s*(?:(\d+)m\s*)?\s*(?:(\d+)s\s*)?$");
        var match = regex.Match(input);

        if (!match.Success)
            return false;

        var days = match.Groups[1].Success ? int.Parse(match.Groups[1].Value) : 0;
        var hours = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 0;
        var minutes = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : 0;
        var seconds = match.Groups[4].Success ? int.Parse(match.Groups[4].Value) : 0;

        result = new TimeSpan(days, hours, minutes, seconds);
        return true;
    }
}

public static class TimeEx
{
    public static string ToTimeSpanStr(this TimeSpan timeSpan)
    {
        var sb = new StringBuilder();
        if (timeSpan.Days > 0) sb.Append($"{timeSpan.Days}d ");
        if (timeSpan.Hours > 0) sb.Append($"{timeSpan.Hours}h ");
        if (timeSpan.Minutes > 0) sb.Append($"{timeSpan.Minutes}m ");
        if (timeSpan.Seconds > 0 || sb.Length == 0) sb.Append($"{timeSpan.Seconds}s ");
        return sb.ToString();
    }
}


