using CkCommons.Gui;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;

namespace CkCommons.RichText;

public class TimestampSegment : IRichSegment
{
    private bool _isInline = false;
    private readonly DateTime _localTime;
    private readonly string _displayText;
    private readonly char _format;
    private readonly bool _isValid;
    private Vector2 _size;

    public TimestampSegment(string timeData, char format = 'f')
    {
        _isValid = true;
        _format = format;
        // Check for a unix seconds (UTC)
        if (timeData.All(char.IsDigit) && long.TryParse(timeData, out long unixSeconds))
        {
            _localTime = DateTimeOffset.FromUnixTimeSeconds(unixSeconds).ToLocalTime().DateTime;
        }
        // Fallback to custom strings (13:00, 9AM PST, ext)
        else if (TryParseCustomTime(timeData, out var parsedOffset))
        {
            _localTime = parsedOffset.ToLocalTime().DateTime;
        }
        else
        {
            _isValid = false;
            _displayText = timeData;
        }

        // Format this display text based on the format type.
        _displayText = format switch
        {
            't' => _localTime.ToString("t"), // 1:49 PM
            'T' => _localTime.ToString("T"), // 1:49:49 PM
            'd' => _localTime.ToString("d"), // 09/17/2026
            'D' => _localTime.ToString("D"), // September 17, 2026
            'f' => _localTime.ToString("f"), // September 17, 2026 1:49 PM
            'F' => _localTime.ToString("F"), // Thursday, September 17, 2026 1:49 PM
            'R' or 'r' => GetRelativeTime(_localTime), // "2 hours ago"
            _ => _localTime.ToString("t") // Default fallback
        };
    }

    public void Draw(RichStringContext ctx)
    {
        if (_isInline)
            ImGui.SameLine(0, 0);

        // Render with a distinct color to show it's an interactive tag
        var pos = ImGui.GetCursorScreenPos();
        ImGui.GetWindowDrawList().AddRectFilled(pos - Vector2.One, pos + _size + Vector2.One, 0x66444444, 5f * ImGuiHelpers.GlobalScale);
        ImGui.Text(_displayText);
        if (_isValid && ImGui.IsItemHovered())
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            using (ImRaii.Tooltip())
            {
                var tooltipTime = (_format is 'R' or 'r') ? GetRelativeTime(_localTime) : _localTime.ToString(_format.ToString());
                CkGui.ColorText($"{tooltipTime} (Local Time)", ImGuiColors.DalamudGrey);
            }
        }
    }

    public void UpdateCache(ref RichStringContext ctx, int segmentIdx)
    {
        var prevWidth = ctx.CurrLineWidth;
        _size = ImGui.CalcTextSize(_displayText);
        var width = _size.X;

        if (prevWidth + width > ctx.WrapWidth)
        {
            ctx.CurrLineWidth = width;
            ctx.LineCount++;
            _isInline = false;
        }
        else
        {
            ctx.CurrLineWidth = prevWidth + width;
            _isInline = segmentIdx > 0 && prevWidth > 0f;
        }
    }

    // Attempts to parse out the numerous ways people can format a timestamp into an actual offset in UTC.
    private static bool TryParseCustomTime(string input, out DateTimeOffset result)
    {
        result = default;
        // Assume that the timezone is seperated by a space.
        var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // If no timezone was defined, there would have been no split, and we assume UTC
        if (parts.Length is 0)
            return false;

        // Otherwise we will attempt to retrieve the timezone.
        var offset = TimeSpan.Zero;
        var cleanInput = input;
        // If we attached a timezone to it, we should 
        var lastWord = parts[^1].ToUpperInvariant();
        if (TimezoneOffsets.TryGetValue(lastWord, out var tzOffset))
        {
            offset = tzOffset;
            // Remove the timezone offset after setting it so we have the 'utc' equivalent.
            cleanInput = string.Join(" ", parts.Take(parts.Length - 1));
        }
        // Attempt to parse out the time from the remaining value.
        if (!DateTime.TryParse(cleanInput, out var dt))
            return false;

        // Add that offset we calculated back on afterwards.
        result = new DateTimeOffset(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond, offset);
        return true;
    }

    // Gets hovertext if the time passes the displayed time.
    private static string GetRelativeTime(DateTime time)
    {
        var span = time - DateTime.Now;
        var isFuture = span.TotalSeconds > 0;
        span = span.Duration();

        var text = span switch
        {
            { TotalDays: >= 365 } => $"{span.TotalDays / 365:N0} years",
            { TotalDays: >= 30 } => $"{span.TotalDays / 30:N0} months",
            { TotalDays: >= 1 } => $"{span.TotalDays:N0} days",
            { TotalHours: >= 1 } => $"{span.TotalHours:N0} hours",
            { TotalMinutes: >= 1 } => $"{span.TotalMinutes:N0} minutes",
            _ => "a few seconds"
        };

        return isFuture ? $"in {text}" : $"{text} ago";
    }

    // Helper to get timezone offsets by their shorthand name.
    private static readonly Dictionary<string, TimeSpan> TimezoneOffsets = new(StringComparer.OrdinalIgnoreCase)
    {
        // Reflect the same space.
        { "ST", TimeSpan.Zero }, { "UTC", TimeSpan.Zero }, { "GMT", TimeSpan.Zero },

        // Pacific Standard / Daylight Time
        { "PST", TimeSpan.FromHours(-8) }, { "PDT", TimeSpan.FromHours(-7) },
        // Mountain Standard / Daylight Time
        { "MST", TimeSpan.FromHours(-7) }, { "MDT", TimeSpan.FromHours(-6) },
        // Central Standard / Daylight Time
        { "CST", TimeSpan.FromHours(-6) }, { "CDT", TimeSpan.FromHours(-5) },
        // Eastern Standard / Daylight Time
        { "EST", TimeSpan.FromHours(-5) }, { "EDT", TimeSpan.FromHours(-4) },

        // Western European Time / Summer Time
        { "WET", TimeSpan.Zero }, { "WEST", TimeSpan.FromHours(1) },
        // British Summer Time
        { "BST", TimeSpan.FromHours(1) },
        // Central European Time / Summer Time
        { "CET", TimeSpan.FromHours(1) }, { "CEST", TimeSpan.FromHours(2) },
        // Eastern European Time / Summer Time
        { "EET", TimeSpan.FromHours(2) }, { "EEST", TimeSpan.FromHours(3) },

        // Japan Standard Time
        { "JST", TimeSpan.FromHours(9) }, 

        // Australian Western Standard Time
        { "AWST", TimeSpan.FromHours(8) },
        // Australian Central Standard / Daylight Time
        { "ACST", TimeSpan.FromHours(9.5) }, { "ACDT", TimeSpan.FromHours(10.5) },
        // Australian Eastern Standard / Daylight Time
        { "AEST", TimeSpan.FromHours(10) },  { "AEDT", TimeSpan.FromHours(11) },
        // New Zealand Standard / Daylight Time
        { "NZST", TimeSpan.FromHours(12) },  { "NZDT", TimeSpan.FromHours(13) }
    };
}