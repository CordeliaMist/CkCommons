using Dalamud.Game.Text.SeStringHandling;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace CkCommons.Helpers;

// the 'partial' is the regex generator.
public static partial class RegexEx
{
    public static bool TryParseTimeSpan(string input, out TimeSpan result)
    {
        result = TimeSpan.Zero;
        var regex = new Regex(@"^\s*(?:(\d+)d\s*)?\s*(?:(\d+)h\s*)?\s*(?:(\d+)m\s*)?\s*(?:(\d+)s\s*)?$");
        var match = regex.Match(input);

        if (!match.Success)
        {
            return false;
        }

        var days = match.Groups[1].Success ? int.Parse(match.Groups[1].Value) : 0;
        var hours = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 0;
        var minutes = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : 0;
        var seconds = match.Groups[4].Success ? int.Parse(match.Groups[4].Value) : 0;

        result = new TimeSpan(days, hours, minutes, seconds);
        return true;
    }

    public static Match TryMatchTriggerWord(string message, string triggerWord)
    {
        var triggerRegex = $@"(?<=^|\s){triggerWord}(?=[^a-z])";
        return Regex.Match(message, triggerRegex);
    }

    public static string EnsureUniqueName<T>(string baseName, IEnumerable<T> collection, Func<T, string> nameSelector)
    {
        // Regex to match the base name and the (X) suffix if it exists
        var suffixPattern = @"^(.*?)(?: \((\d+)\))?$";
        var match = Regex.Match(baseName, suffixPattern);

        var namePart = match.Groups[1].Value; // The base part of the name
        var currentNumber = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 0;

        // Increment current number for the new copy
        currentNumber = Math.Max(1, currentNumber);

        var newName = baseName;

        // Ensure the name is unique by appending (X) and incrementing if necessary
        while (collection.Any(item => nameSelector(item) == newName))
            newName = $"{namePart} ({currentNumber++})";

        return newName;
    }

    /// <summary>
    ///     Removes all the BBCode from the color tags.
    /// </summary>
    public static string StripColorTags(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;
        // Define a regex pattern to match any [color=...] and [/color] tags
        var pattern = @"\[\/?(color|glow)(=[^\]]*)?\]";
        // Use Regex.Replace to remove the tags
        return Regex.Replace(input, pattern, string.Empty); 
    }

    /// <summary>
    ///     Encapsulates the puppeteer command within '(' and ')'
    /// </summary>
    public static SeString GetSubstringWithinParentheses(this SeString str, char startBracket = '(', char EndBracket = ')')
    {
        var startIndex = str.TextValue.IndexOf(startBracket);
        var endIndex = str.TextValue.IndexOf(EndBracket);
        // If both brackets are found and the end bracket is after the start bracket
        if (startIndex >= 0 && endIndex >= 0 && endIndex > startIndex)
            return str.TextValue.Substring(startIndex + 1, endIndex - startIndex - 1).Trim();

        return str;
    }

    /// <summary>
    ///     Converts square brackets to angle brackets
    /// </summary>
    public static SeString ConvertSquareToAngleBrackets(this SeString str)
        => str.TextValue.Replace("[", "<").Replace("]", ">");


    /// <summary>
    ///     Attempts to convert an input string of a Vector4 as seen in the code editor to a Vector4 value. <para/>
    ///     <c>new Vector4(0.0f, 0.0f, 0.0f, 1.0f)</c>
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseVec4Code(this string input, out Vector4 result)
    {
        result = Vector4.Zero;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var match = Vec4CodeRegex().Match(input);
        if (!match.Success) return false;

        return
            float.TryParse(match.Groups["x"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var x) &&
            float.TryParse(match.Groups["y"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var y) &&
            float.TryParse(match.Groups["z"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var z) &&
            float.TryParse(match.Groups["w"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var w)
                ? (result = new Vector4(x, y, z, w)) != null : false;
    }

    /// <summary>
    ///     Seperates a BBCode into their individual components, such as the text and the color tags. <para/>
    ///     Useful for CkRichText parsing.
    /// </summary>
    [GeneratedRegex(@"(\[color=[0-9a-zA-Z]+\])|(\[\/color\])|(\[glow=[0-9a-zA-Z]+\])|(\[\/glow\])|(\[i\])|(\[\/i\])", RegexOptions.IgnoreCase, "en-US")]
    public static partial Regex SplitRegex();

    /// <summary>
    ///     Matches a Vector4 as displayed in a code editor. <para/>
    ///     <c>new Vector4(0.0f, 0.0f, 0.0f, 1.0f)</c>
    /// </summary>
    [GeneratedRegex(@"new\s+Vector4\s*\(\s*(?<x>[+-]?(?:\d+\.?\d*|\.\d+))f?\s*,\s*(?<y>[+-]?(?:\d+\.?\d*|\.\d+))f?\s*,\s*(?<z>[+-]?(?:\d+\.?\d*|\.\d+))f?\s*,\s*(?<w>[+-]?(?:\d+\.?\d*|\.\d+))f?\s*\)$", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    public static partial Regex Vec4CodeRegex();
}

