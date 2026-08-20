using CkCommons.Gui;
using CkCommons.Gui.Utility;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using OtterGui.Text;
using System.Globalization;

namespace CkCommons.Widgets;

// DatePicker was derrived from the C++ ImGuiDatePicker code:
// https://github.com/DnA-IntRicate/ImGuiDatePicker
// And was heavily modified for C# formatting, culture support, and optimizations.

public static class DatePicker
{
    /// <summary>
    ///   Renders a localized DatePicker widget. <br/>
    ///   Assumes the provided date is in the exact timezone you want to display and output.
    /// </summary>
    public static bool DrawPicker(string label, float width, float innerWidth, ref DateTime date, DateTime? min = null, DateTime? max = null, CFlags flags = CFlags.None, CultureInfo? culture = null)
    {
        culture ??= CultureInfo.CurrentCulture;
        return DrawPickerInternal(label, width, innerWidth, ref date, min, max, flags, culture);
    }

    /// <summary>
    ///   Renders a localized DatePicker widget explicitly for UTC backend variables. <br/>
    ///   By default, converts the UTC date to LocalTime for the user interface, then parses the selection back to UTC.
    /// </summary>
    public static bool DrawPickerUTC(string label, float width, float innerWidth, ref DateTime date, DateTime? min = null, DateTime? max = null, bool displayAsLocal = true, CFlags flags = CFlags.None, CultureInfo? culture = null)
    {
        culture ??= CultureInfo.CurrentCulture;
        var displayDate = displayAsLocal ? date.ToLocalTime() : date.ToUniversalTime();

        var displayMin = min.HasValue ? (displayAsLocal ? min.Value.ToLocalTime() : min.Value.ToUniversalTime()) : (DateTime?)null;
        var displayMax = max.HasValue ? (displayAsLocal ? max.Value.ToLocalTime() : max.Value.ToUniversalTime()) : (DateTime?)null;
        var modified = DrawPickerInternal(label, width, innerWidth, ref displayDate, displayMin, displayMax, flags, culture);

        if (modified)
            date = displayDate.ToUniversalTime();

        return modified;
    }

    /// <summary>
    ///   Operates entirely on the DateTime Kind it receives without making assumptions.
    /// </summary>
    private static bool DrawPickerInternal(string label, float width, float innerWidth, ref DateTime workingDate, DateTime? min, DateTime? max, CFlags flags, CultureInfo culture)
    {
        var modified = false;
        var uiMinDate = min ?? DateTime.MinValue;
        var uiMaxDate = max ?? DateTime.MaxValue;

        var dateStr = workingDate.ToString("d", culture);

        using var _ = ImRaii.PushId(label);
        ImGui.SetNextItemWidth(width);
        using (var combo = ImUtf8.Combo(""u8, dateStr, flags | CFlags.HeightLarge))
        {
            if (combo)
            {
                ImGui.Dummy(new(innerWidth, 0f));
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() - ImUtf8.ItemSpacing.Y);
                var spacing = ImGui.GetStyle().ItemSpacing.X;

                // Dynamically distribute width between the Month and Year combo boxes
                var monthWidth = (innerWidth - spacing) * 0.55f;
                var yearWidth = innerWidth - monthWidth - spacing;

                // Month Selection
                if (CkGuiUtils.IntCombo("##months", monthWidth, workingDate.Month, out var newMonth, Enumerable.Range(1, 12), m => culture.DateTimeFormat.MonthNames[m - 1]))
                {
                    var newDay = Math.Min(workingDate.Day, DateTime.DaysInMonth(workingDate.Year, newMonth));
                    workingDate = new DateTime(workingDate.Year, newMonth, newDay, workingDate.Hour, workingDate.Minute, workingDate.Second, workingDate.Kind);
                    modified = true;
                }

                ImGui.SameLine();
                ImGui.SetNextItemWidth(yearWidth);

                // Year Selection
                var currentYear = workingDate.Year;
                if (ImGui.InputInt("##year", ref currentYear, 1, 10))
                {
                    currentYear = Math.Clamp(currentYear, uiMinDate.Year, uiMaxDate.Year);
                    var newDay = Math.Min(workingDate.Day, DateTime.DaysInMonth(currentYear, workingDate.Month));
                    workingDate = new DateTime(currentYear, workingDate.Month, newDay, workingDate.Hour, workingDate.Minute, workingDate.Second, workingDate.Kind);
                    modified = true;
                }

                // Nav Controls
                var arrowSize = ImUtf8.FrameHeight;
                var navBlockWidth = (arrowSize * 3) + (spacing * 2);
                ImGui.SetCursorPosX((ImGui.GetWindowContentRegionMax().X - navBlockWidth) * 0.5f);

                var isMinMonth = workingDate.Year == uiMinDate.Year && workingDate.Month == uiMinDate.Month;
                var isMaxMonth = workingDate.Year == uiMaxDate.Year && workingDate.Month == uiMaxDate.Month;
                using (ImRaii.PushColor(ImGuiCol.Button, 0).Push(ImGuiCol.Border, 0))
                {
                    using (ImRaii.Disabled(isMinMonth))
                        if (ImGui.ArrowButton("##arrow-left", ImGuiDir.Left))
                        {
                            workingDate = workingDate.AddMonths(-1);
                            modified = true;
                        }

                    ImGui.SameLine();
                    CkGui.FramedIconText(FAI.Calendar);

                    ImGui.SameLine();
                    using (ImRaii.Disabled(isMaxMonth))
                        if (ImGui.ArrowButton("##arrow-right", ImGuiDir.Right))
                        {
                            workingDate = workingDate.AddMonths(1);
                            modified = true;
                        }
                }

                // Calendar Grid
                var tFlags = ImGuiTableFlags.BordersOuter | ImGuiTableFlags.SizingStretchSame | ImGuiTableFlags.NoHostExtendX;
                using (var t = ImRaii.Table("calendar-grid", 7, tFlags, new Vector2(innerWidth, ImUtf8.FrameHeightSpacing * 7)))
                {
                    if (t)
                    {
                        var firstDayOfWeek = culture.DateTimeFormat.FirstDayOfWeek;

                        // Init Headers
                        using (ImRaii.PushColor(ImGuiCol.HeaderHovered, ImGui.GetStyle().Colors[(int)ImGuiCol.TableHeaderBg])
                                     .Push(ImGuiCol.HeaderActive, ImGui.GetStyle().Colors[(int)ImGuiCol.TableHeaderBg]))
                        {
                            for (int i = 0; i < 7; i++)
                            {
                                int dayIndex = ((int)firstDayOfWeek + i) % 7;
                                string dayName = culture.DateTimeFormat.AbbreviatedDayNames[dayIndex];
                                dayName = dayName.Length > 2 ? dayName[..2] : dayName;
                                ImGui.TableSetupColumn(dayName, ImGuiTableColumnFlags.NoHeaderWidth);
                            }
                            ImGui.TableHeadersRow();
                        }

                        // Init days
                        var daysInMonth = DateTime.DaysInMonth(workingDate.Year, workingDate.Month);
                        var firstDayOfMonth = new DateTime(workingDate.Year, workingDate.Month, 1).DayOfWeek;

                        var startCol = ((int)firstDayOfMonth - (int)firstDayOfWeek + 7) % 7;
                        var numWeeks = (int)Math.Ceiling((daysInMonth + startCol) / 7.0f);
                        var currentDay = 1;

                        for (int week = 0; week < numWeeks; week++)
                        {
                            ImGui.TableNextRow();
                            for (int col = 0; col < 7; col++)
                            {
                                ImGui.TableSetColumnIndex(col);
                                if (week is 0 && col < startCol)
                                    continue;
                                if (currentDay > daysInMonth)
                                    continue;

                                var isSelected = (currentDay == workingDate.Day);
                                var cellDate = new DateTime(workingDate.Year, workingDate.Month, currentDay);
                                var isOutOfBounds = (cellDate < uiMinDate.Date) || (cellDate > uiMaxDate.Date);

                                if (!isSelected)
                                {
                                    ImGui.PushStyleColor(ImGuiCol.Button, 0);
                                    ImGui.PushStyleColor(ImGuiCol.Border, 0);
                                }

                                if (CkGui.ButtonEx($"{currentDay}##day_{currentDay}", new Vector2(-1, ImUtf8.FrameHeight), isOutOfBounds))
                                {
                                    workingDate = new DateTime(workingDate.Year, workingDate.Month, currentDay);
                                    modified = true;
                                    ImGui.CloseCurrentPopup();
                                }

                                if (!isSelected)
                                    ImGui.PopStyleColor(2);

                                currentDay++;
                            }
                        }
                    }
                }
            }

            if (modified)
                workingDate = workingDate < uiMinDate ? uiMinDate : (workingDate > uiMaxDate ? uiMaxDate : workingDate);

            return modified;
        }
    }
}