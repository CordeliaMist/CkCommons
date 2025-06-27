using ImGuiNET;
using CkCommons.Helpers;

namespace CkCommons.Classes;

/// <summary>
///     Keeps a TimeSpan converted into a string cached lazily until updated.
/// </summary>
/// <remarks>
///     This is a placeholder class until I can figure out something more optimized to handle this.
/// </remarks>
public class InputTextTimeSpan
{
    private string _timeSpanString;
    private TimeSpan _timeSpan;
    private readonly Func<TimeSpan> _getter;
    private readonly Action<TimeSpan> _setter;

    public InputTextTimeSpan(Func<TimeSpan> getter, Action<TimeSpan> setter)
    {
        _getter = getter;
        _setter = setter;
        _timeSpan = getter(); // Get the initial value
        _timeSpanString = ToRemainingTime(_timeSpan);
    }

    public void DrawInputTimer(string label, float width, string hint)
    {
        // Refresh the cached value if it has changed externally
        var currentValue = _getter();
        if (currentValue != _timeSpan)
        {
            _timeSpan = currentValue;
            _timeSpanString = ToRemainingTime(_timeSpan);
        }

        ImGui.SetNextItemWidth(width);
        ImGui.InputTextWithHint(label, hint, ref _timeSpanString, 16);
        // Apply updates only when editing finishes
        if (ImGui.IsItemDeactivatedAfterEdit() && RegexEx.TryParseTimeSpan(_timeSpanString, out var newSpan))
        {
            if (newSpan != _timeSpan) // Prevent unnecessary updates
            {
                _timeSpan = newSpan;
                _setter(newSpan); // Apply new value via provided function
            }
        }
    }

    private string ToRemainingTime(TimeSpan time)
    {
        var sb = new StringBuilder();
        if (time.Days > 0) sb.Append($"{time.Days}d ");
        if (time.Hours > 0) sb.Append($"{time.Hours}h ");
        if (time.Minutes > 0) sb.Append($"{time.Minutes}m ");
        if (time.Seconds > 0 || sb.Length == 0) sb.Append($"{time.Seconds}s ");
        return sb.ToString();
    }
}
