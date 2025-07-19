using CkCommons;
using Dalamud.Interface;
using ImGuiNET;
using OtterGui.Text.Widget;

namespace CkCommons.Classes;
public class IconCheckboxEx(FAI icon, uint colorTrue = 0xFF00FF00, uint colorFalse = 0xFF0000FF) : FontAwesomeCheckbox<bool>
{
    protected override (FAI? Icon, uint? Color) GetIcon(bool value)
        => value ? (icon, colorTrue) : (icon, colorFalse);

    protected override bool NextValue(bool value)
        => !value;

    protected override bool PreviousValue(bool value)
        => !value;
}

/// <summary> A base class for a multi state checkbox displaying different icons. </summary>
public abstract class FontAwesomeCheckbox<T> : MultiStateCheckbox<T>
{
    protected abstract (FontAwesomeIcon? Icon, uint? Color) GetIcon(T value);

    protected override void RenderSymbol(T value, Vector2 position, float size)
    {
        var (icon, color) = GetIcon(value);
        if (!icon.HasValue)
            return;

        using var font = Svc.PluginInterface.UiBuilder.IconFontFixedWidthHandle.Push();
        var text = string.Empty + (char)icon;
        var iconSize = ImGui.CalcTextSize(text);
        var iconPos = position + (new Vector2(size) - iconSize) * 0.5f;
        ImGui.GetWindowDrawList()
            .AddText(iconPos, color ?? ImGui.GetColorU32(ImGuiCol.CheckMark), text);
    }
}
