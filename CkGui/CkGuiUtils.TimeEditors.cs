using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Utility.Raii;

namespace CkCommons.Gui.Utility;
public static partial class CkGuiUtils
{
    public static float GetDateTimeDisplayWidth(DateTimeOffset time, IFontHandle fontHandle)
    {
        using var font = fontHandle.Push();
        // 5 spacings accounts for 4 spacings on sides of '00', for two '00' and 1 for the ':'
        return ImGui.CalcTextSize(new string('0', 4)).X + ImGui.GetStyle().ItemSpacing.X * 5;
    }

    public static float GetTimeSpanDisplayWidth(TimeSpan maxTime, string format, IFontHandle fontHandle)
    {
        using var font = fontHandle.Push();
        var extraPadding = ImGui.GetStyle().ItemSpacing.X;

        // Define the regex pattern to match any of the time units (hh, mm, ss, fff)
        var regex = new Regex(@"hh|mm|ss|fff");
        var matches = regex.Matches(format);

        // Concatenate all time parts into a single string
        var timeString = string.Concat(matches
            .Select(match => match.Value switch
            {
                "hh" => $"{maxTime.Hours:00}h",
                "mm" => $"{maxTime.Minutes:00}m",
                "ss" => $"{maxTime.Seconds:00}s",
                "fff" => $"{maxTime.Milliseconds:000}ms",
                _ => string.Empty
            }));

        // Calculate the total width of the concatenated string with padding
        return ImGui.CalcTextSize(timeString).X + (extraPadding * matches.Count) - extraPadding;
    }

    public static float GetTimeDisplayHeight(IFontHandle fontHandle)
    {
        var smallH = ImGui.GetTextLineHeightWithSpacing() * 2;
        using var font = fontHandle.Push();
        var bigH = ImGui.GetTextLineHeight();
        return smallH + bigH;
    }

    // Draws a datetime editor that accepts a UTC time, and displays in local.
    public static void DateTimePreviewUtcAsLocal(string id, DateTimeOffset time, IFontHandle font, float? width = null)
        => DateTimeDisplayInternal(id, ref time, false, font, width);

    public static void DateTimeEditorUtcAsLocal(string id, ref DateTimeOffset time, IFontHandle font, float? width = null)
        => DateTimeDisplayInternal(id, ref time, true, font, width);

    private static void DateTimeDisplayInternal(string id, ref DateTimeOffset time, bool isEditor, IFontHandle font, float? width = null)
    {
        using var s = ImRaii.PushStyle(ImGuiStyleVar.CellPadding, Vector2.Zero);

        var fullWidth = width ?? GetDateTimeDisplayWidth(time, font);
        var localTime = time.ToLocalTime();
        float hourSize;
        float splitSize;
        float minuteSize;
        using (font.Push())
        {
            hourSize = ImGui.CalcTextSize($"{localTime.Hour:00}").X;
            splitSize = ImGui.CalcTextSize(":").X;
            minuteSize = ImGui.CalcTextSize($"{localTime.Minute:00}").X;
        }

        // Get the remaining width to be used for splitting.
        var spacingWidth = fullWidth - (hourSize + minuteSize + splitSize);
        var individualSpacing = spacingWidth / 2;

        using (var t = ImRaii.Table($"DateTimeEditor_{id}", 3, ImGuiTableFlags.NoPadOuterX | ImGuiTableFlags.NoPadInnerX))
        {
            if (!t) return;

            ImGui.TableSetupColumn("Hour", ImGuiTableColumnFlags.WidthFixed, hourSize + individualSpacing);
            ImGui.TableSetupColumn("Split", ImGuiTableColumnFlags.WidthFixed, splitSize);
            ImGui.TableSetupColumn("Minute", ImGuiTableColumnFlags.WidthFixed, minuteSize + individualSpacing);

            // Hours
            float cursorY = 0;
            ImGui.TableNextColumn();
            CkGui.ColorTextCentered($"{(localTime.Hour - 1 + 24) % 24:00}", ImGuiColors.DalamudGrey);
            cursorY = ImGui.GetCursorPosY();
            CkGui.FontTextCentered($"{localTime.Hour:00}", font);
            if (ImGui.IsItemHovered() && isEditor && ImGui.GetIO().MouseWheel != 0)
            {
                var newHour = (localTime.Hour - (int)ImGui.GetIO().MouseWheel + 24) % 24;
                var newLocalTime = new DateTime(localTime.Year, localTime.Month, localTime.Day, newHour, localTime.Minute, 0);
                time = new DateTimeOffset(newLocalTime, TimeZoneInfo.Local.GetUtcOffset(newLocalTime)).ToUniversalTime();
            }
            CkGui.ColorTextCentered($"{(localTime.Hour + 1) % 24:00}", ImGuiColors.DalamudGrey);

            // Divider
            ImGui.TableNextColumn();
            ImGui.SetCursorPosY(cursorY);
            CkGui.FontTextCentered(":", font);

            // Minutes.
            ImGui.TableNextColumn();
            CkGui.ColorTextCentered($"{(localTime.Minute - 1 + 60) % 60:00}", ImGuiColors.DalamudGrey);
            CkGui.FontTextCentered($"{localTime.Minute:00}", font);
            if (ImGui.IsItemHovered() && isEditor && ImGui.GetIO().MouseWheel != 0)
            {
                var newMinute = (time.Minute - (int)ImGui.GetIO().MouseWheel + 60) % 60;
                var newLocalTime = new DateTime(localTime.Year, localTime.Month, localTime.Day, localTime.Hour, newMinute, 0);
                time = new DateTimeOffset(newLocalTime, TimeZoneInfo.Local.GetUtcOffset(newLocalTime)).ToUniversalTime();
            }
            CkGui.ColorTextCentered($"{(localTime.Minute + 1) % 60:00}", ImGuiColors.DalamudGrey);
        }
    }

    public static void TimeSpanEditor(string id, TimeSpan maxTime, ref TimeSpan timeRef, string format, IFontHandle font, float? width = null)
        => DrawTimeSpanInternal(id, maxTime, ref timeRef, format, true, font, width);

    // Draws a datetime editor that accepts a UTC time, and displays in local.
    public static void TimeSpanPreview(string id, TimeSpan maxTime, TimeSpan timeRef, string format, IFontHandle font, float? width = null)
        => DrawTimeSpanInternal(id, maxTime, ref timeRef, format, false, font, width);

    private static void DrawTimeSpanInternal(string id, TimeSpan maxTime, ref TimeSpan timeRef, string format, bool editorMode, IFontHandle font, float? width = null)
    {
        using var style = ImRaii.PushStyle(ImGuiStyleVar.CellPadding, new Vector2(ImGui.GetStyle().CellPadding.X, 0));

        var fullWidth = width ?? GetTimeSpanDisplayWidth(maxTime, format, font);
        float h, m, s, ms = 0;

        // get the regex for the format so we know how many columns to make and sizes to calculate.
        var regex = new Regex(@"hh|mm|ss|fff");
        var matches = regex.Matches(format).Select(m => m.Value).ToArray();
        var totalColumns = matches.Length;

        // Calculate the sizes for each time unit based on the format.
        // (curse the left spacing for forcing me to do this twice)
        using (font.Push())
        {
            h = matches.Contains("hh") ? ImGui.CalcTextSize("0h ").X : 0;
            m = matches.Contains("mm") ? ImGui.CalcTextSize("00m ").X : 0;
            s = matches.Contains("ss") ? ImGui.CalcTextSize("00s ").X : 0;
            ms = matches.Contains("fff") ? ImGui.CalcTextSize("000ms ").X : 0;
        }

        // Get the remaining width to be used for splitting.
        var spacingWidth = fullWidth - (h + m + s + ms);
        var individualSpacing = spacingWidth / 2;

        using (var t = ImRaii.Table($"DateTimePreview_{id}", totalColumns + 2, ImGuiTableFlags.NoPadOuterX | ImGuiTableFlags.NoPadInnerX))
        {
            if (!t) return;

            ImGui.TableSetupColumn("SpaceLeft", ImGuiTableColumnFlags.WidthFixed, individualSpacing);
            foreach (var match in matches)
                switch (match)
                {
                    case "hh":  ImGui.TableSetupColumn("Hour", ImGuiTableColumnFlags.WidthFixed, h);        break;
                    case "mm":  ImGui.TableSetupColumn("Minute", ImGuiTableColumnFlags.WidthFixed, m);      break;
                    case "ss":  ImGui.TableSetupColumn("Second", ImGuiTableColumnFlags.WidthFixed, s);      break;
                    case "fff": ImGui.TableSetupColumn("Millisecond", ImGuiTableColumnFlags.WidthFixed, ms);break;
                }
            ImGui.TableSetupColumn("SpaceRight", ImGuiTableColumnFlags.WidthFixed, individualSpacing);

            // Left Spacing
            ImGui.TableNextColumn();

            // Draw the components based on the matches
            foreach (var match in matches)
            {
                ImGui.TableNextColumn();
                (var prev, var cur, var next) = GetDisplayTriplet(timeRef, maxTime, match);
                CkGui.ColorTextCentered(prev, ImGuiColors.DalamudGrey);

                CkGui.FontTextCentered(cur, font);
                if(editorMode) AdjustTimeSpan(ref timeRef, maxTime, match);

                CkGui.ColorTextCentered(next, ImGuiColors.DalamudGrey);
            }

            // Right Spacing
            ImGui.TableNextColumn();
        }
    }

    private static (string prev, string cur, string next) GetDisplayTriplet(TimeSpan duration, TimeSpan maxTime, string suffix)
    {
        var prevValue = suffix switch
        {
            "hh" => $"{Math.Max(0, (duration.Hours - 1)):00}",
            "mm" => $"{Math.Max(0, (duration.Minutes - 1)):00}",
            "ss" => $"{Math.Max(0, (duration.Seconds - 1)):00}",
            "fff" => $"{Math.Max(0, (duration.Milliseconds - 10)):000}",
            _ => $"UNK"
        };

        var currentValue = suffix switch
        {
            "hh" => $"{duration.Hours:00}h",
            "mm" => $"{duration.Minutes:00}m",
            "ss" => $"{duration.Seconds:00}s",
            "fff" => $"{duration.Milliseconds:000}ms",
            _ => $"UNK"
        };

        var nextValue = suffix switch
        {
            "hh" => $"{Math.Min(maxTime.Hours, (duration.Hours + 1)):00}",
            "mm" => duration.Hours == maxTime.Hours
                ? $"{Math.Min(maxTime.Minutes, duration.Minutes + 1):00}"
                : $"{duration.Minutes + 1:00}",
            "ss" => duration.Minutes == maxTime.Minutes
                ? $"{Math.Min(maxTime.Seconds, duration.Seconds + 1):00}"
                : $"{duration.Seconds + 1:00}",
            "fff" => duration.Seconds == maxTime.Seconds
                ? $"{Math.Min(maxTime.Milliseconds, duration.Milliseconds + 10):000}"
                : $"{duration.Milliseconds + 10:000}",
            _ => $"UNK"
        };
        return (prevValue, currentValue, nextValue);
    }

    private static void AdjustTimeSpan(ref TimeSpan timeRef, TimeSpan maxDuration, string suffix)
    {
        if (!ImGui.IsItemHovered() || ImGui.GetIO().MouseWheel == 0)
            return;

        var delta = -(int)ImGui.GetIO().MouseWheel;

        var h = timeRef.Hours;
        var m = timeRef.Minutes;
        var s = timeRef.Seconds;
        var ms = timeRef.Milliseconds;

        switch (suffix)
        {
            case "hh": h += delta; break;
            case "mm": m += delta; break;
            case "ss": s += delta; break;
            case "fff": ms += delta * 10; break;
        }

        // Normalize up
        if (ms > 999) { s += ms / 1000; ms %= 1000; }
        if (s > 59) { m += s / 60; s %= 60; }
        if (m > 59) { h += m / 60; m %= 60; }

        // Normalize down
        if (ms < 0) { s += (ms / 1000) - 1; ms = 1000 + (ms % 1000); }
        if (s < 0) { m += (s / 60) - 1; s = 60 + (s % 60); }
        if (m < 0) { h += (m / 60) - 1; m = 60 + (m % 60); }

        // Build & clamp
        var adjusted = new TimeSpan(0, h, m, s, ms);
        if (adjusted < TimeSpan.Zero) adjusted = TimeSpan.Zero;
        if (adjusted > maxDuration) adjusted = maxDuration;

        timeRef = adjusted;
    }
}
