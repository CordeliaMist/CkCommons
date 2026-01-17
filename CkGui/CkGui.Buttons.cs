using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using OtterGui.Text;
using OtterGuiInternal;
using System.Drawing;
namespace CkCommons.Gui;

// Primary Partial Class
public static partial class CkGui
{
    public const byte BUTTON_ACTIVE_OPACITY = 170;
    public const byte BUTTON_HOVER_OPACITY = 170;
    public const byte BUTTON_TRANSPARENCY = 100;

    public static float GetButtonSize(string text)
    {
        var vector2 = ImGui.CalcTextSize(text);
        return vector2.X + ImGui.GetStyle().FramePadding.X * 2f;
    }

    public static float IconTextButtonSize(FAI icon, string text)
    {
        Vector2 vector;
        using (Svc.PluginInterface.UiBuilder.IconFontFixedWidthHandle.Push())
            vector = ImGui.CalcTextSize(icon.ToIconString());

        var vector2 = ImGui.CalcTextSize(text);
        var num = 3f * ImGuiHelpers.GlobalScale;
        return vector.X + vector2.X + ImGui.GetStyle().FramePadding.X * 2f + num;
    }

    public static Vector2 IconButtonSize(FAI icon)
    {
        using var font = Svc.PluginInterface.UiBuilder.IconFontFixedWidthHandle.Push();
        return ImGuiHelpers.GetButtonSize(icon.ToIconString());
    }

    public static Vector2 IconSize(FAI icon)
    {
        using var font = Svc.PluginInterface.UiBuilder.IconFontFixedWidthHandle.Push();
        return ImGui.CalcTextSize(icon.ToIconString());
    }

    public static Vector2 IconsSize(FAI[] icons)
    {
        using var font = Svc.PluginInterface.UiBuilder.IconFontFixedWidthHandle.Push();
        var text = string.Concat(icons.Select(i => i.ToIconString()));
        return ImGui.CalcTextSize(text);
    }

    public static bool IconButtonFramed(FAI icon, string? id = null, bool disabled = false, bool inPopup = false)
    {
        using var col = ImRaii.PushColor(ImGuiCol.Button, new Vector4(1.0f, 1.0f, 1.0f, 0.0f), inPopup);
        using var dis = ImRaii.PushStyle(ImGuiStyleVar.Alpha, disabled ? 0.5f : 1f);
        using var font = Svc.PluginInterface.UiBuilder.IconFontFixedWidthHandle.Push();

        var iconText = (id == null) ? icon.ToIconString() : id + icon.ToIconString();
        var iconSize = ImGui.CalcTextSize(icon.ToIconString());
        var cursor = ImGui.GetCursorScreenPos();

        var ret = false;
        var buttonSize = new Vector2(ImUtf8.FrameHeight);

        using (ImRaii.PushId(iconText))
            ret = ImGui.Button(string.Empty, buttonSize);

        var iconPos = cursor + ((buttonSize - iconSize) / 2f);

        ImGui.GetWindowDrawList().AddText(iconPos, ImGui.GetColorU32(ImGuiCol.Text), iconText);
        return ret && !disabled;
    }

    /// <summary> 
    ///     The additional param for an ID is optional. if not provided, the id will be the text.
    /// </summary>
    public static bool IconButton(FAI icon, float? height = null, string? id = null, bool disabled = false, bool inPopup = false)
        => IconButtonInternal(icon, inPopup ? 0 : null, disabled, height, id);

    public static bool IconButtonColored(FAI icon, uint buttonCol, bool disabled = false, float ? height = null, string? id = null)
        => IconButtonInternal(icon, buttonCol, disabled, height, id);

    private static bool IconButtonInternal(FAI icon, uint? buttonCol = null, bool disabled = false, float? height = null, string? id = null)
    {
        using var dis = ImRaii.PushStyle(ImGuiStyleVar.Alpha, disabled ? 0.5f : 1f);
        
        var text = icon.ToIconString();
        var num = 0;
        if (buttonCol.HasValue)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, buttonCol.Value);
            num++;
        }

        ImGui.PushID((id == null) ? text : id + text);
        Vector2 vector;
        using (Svc.PluginInterface.UiBuilder.IconFontFixedWidthHandle.Push())
            vector = ImGui.CalcTextSize(text);
        var windowDrawList = ImGui.GetWindowDrawList();
        var cursorScreenPos = ImGui.GetCursorScreenPos();
        var x = vector.X + ImGui.GetStyle().FramePadding.X * 2f;
        var frameHeight = height ?? ImGui.GetFrameHeight();
        var result = ImGui.Button(string.Empty, new Vector2(x, frameHeight));
        var pos = new Vector2(cursorScreenPos.X + ImGui.GetStyle().FramePadding.X,
            cursorScreenPos.Y + (height ?? ImGui.GetFrameHeight()) / 2f - (vector.Y / 2f));
        using (Svc.PluginInterface.UiBuilder.IconFontFixedWidthHandle.Push())
            windowDrawList.AddText(pos, ImGui.GetColorU32(ImGuiCol.Text), text);
        ImGui.PopID();
        if (num > 0)
        {
            ImGui.PopStyleColor(num);
        }
        return result && !disabled;
    }

    private static bool IconTextButtonInternal(FAI icon, string text, Vector4? defaultColor = null, float? width = null, bool disabled = false, string id = "")
    {
        using var dis = ImRaii.PushStyle(ImGuiStyleVar.Alpha, disabled ? 0.5f : 1f);
        var num = 0;
        if (defaultColor.HasValue)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, defaultColor.Value);
            num++;
        }

        ImGui.PushID(text + "##" + id);
        Vector2 vector;
        using (Svc.PluginInterface.UiBuilder.IconFontFixedWidthHandle.Push())
            vector = ImGui.CalcTextSize(icon.ToIconString());
        var vector2 = ImGui.CalcTextSize(text);
        var wdl = ImGui.GetWindowDrawList();
        var cursorScreenPos = ImGui.GetCursorScreenPos();
        var num2 = 3f * ImGuiHelpers.GlobalScale;
        var x = width ?? vector.X + vector2.X + ImGui.GetStyle().FramePadding.X * 2f + num2;
        var result = ImGui.Button(string.Empty, new Vector2(x, ImGui.GetFrameHeight()));
        var pos = new Vector2(cursorScreenPos.X + ImGui.GetStyle().FramePadding.X, cursorScreenPos.Y + ImGui.GetStyle().FramePadding.Y);
        using (Svc.PluginInterface.UiBuilder.IconFontFixedWidthHandle.Push())
            wdl.AddText(pos, ImGui.GetColorU32(ImGuiCol.Text), icon.ToIconString());
        var pos2 = new Vector2(pos.X + vector.X + num2, cursorScreenPos.Y + ImGui.GetStyle().FramePadding.Y);
        wdl.AddText(pos2, ImGui.GetColorU32(ImGuiCol.Text), text);
        ImGui.PopID();
        if (num > 0)
        {
            ImGui.PopStyleColor(num);
        }
        dis.Pop();

        return result && !disabled;
    }

    public static bool IconTextButton(FAI icon, string text, float? width = null, bool isInPopup = false, bool disabled = false, string id = "Identifier")
    {
        return IconTextButtonInternal(icon, text,
            isInPopup ? new Vector4(1.0f, 1.0f, 1.0f, 0.0f) : null,
            width <= 0 ? null : width,
            disabled, id);
    }

    private static bool SmallIconTextButtonInternal(FAI icon, string text, Vector4? defaultColor = null, float? width = null, bool disabled = false, string id = "")
    {
        using var dis = ImRaii.PushStyle(ImGuiStyleVar.Alpha, disabled ? 0.5f : 1f);
        var num = 0;
        if (defaultColor.HasValue)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, defaultColor.Value);
            num++;
        }

        ImGui.PushID(text + "##" + id);
        var iconSize = CkGui.CalcFontTextSize(icon.ToIconString(), Svc.PluginInterface.UiBuilder.IconFontFixedWidthHandle);
        var textSize = ImGui.CalcTextSize(text);

        var wdl = ImGui.GetWindowDrawList();
        var screenPos = ImGui.GetCursorScreenPos();
        var padding = ImGui.GetStyle().FramePadding;

        var num2 = 3f * ImGuiHelpers.GlobalScale;
        var x = width ?? iconSize.X + textSize.X + padding.X * 2f + num2;

        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + padding.Y);
        var result = ImGui.Button(string.Empty, new Vector2(x, ImGui.GetTextLineHeight()));

        var pos = new Vector2(screenPos.X + padding.X, screenPos.Y + padding.Y);
        using (Svc.PluginInterface.UiBuilder.IconFontFixedWidthHandle.Push())
            wdl.AddText(pos, ImGui.GetColorU32(ImGuiCol.Text), icon.ToIconString());

        var pos2 = new Vector2(pos.X + iconSize.X + num2, screenPos.Y + padding.Y);
        wdl.AddText(pos2, ImGui.GetColorU32(ImGuiCol.Text), text);

        ImGui.PopID();
        if (num > 0)
        {
            ImGui.PopStyleColor(num);
        }
        dis.Pop();

        return result && !disabled;
    }

    public static bool SmallIconTextButton(FAI icon, string text, float? width = null, bool isInPopup = false, bool disabled = false, string id = "Identifier")
    {
        return SmallIconTextButtonInternal(icon, text, 
            isInPopup ? new Vector4(1.0f, 1.0f, 1.0f, 0.0f) : null,
            width <= 0 ? null : width,
            disabled, id);
    }

    private static bool IconTextButtonCenteredInternal(FAI icon, string text, float width, Vector4 ? defaultColor = null, bool disabled = false, string id = "")
    {
        using var dis = ImRaii.PushStyle(ImGuiStyleVar.Alpha, disabled ? 0.5f : 1f);
        var num = 0;
        if (defaultColor.HasValue)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, defaultColor.Value);
            num++;
        }

        // Push the id.
        ImGui.PushID(text + "##" + id);
        // Calculate the widths.
        Vector2 iconSize;
        using (Svc.PluginInterface.UiBuilder.IconFontFixedWidthHandle.Push())
            iconSize = ImGui.CalcTextSize(icon.ToIconString());
        var textSize = ImGui.CalcTextSize(text);
        // Get draw items.
        var wdl = ImGui.GetWindowDrawList();
        var cursorScreenPos = ImGui.GetCursorScreenPos();
        var padding = ImUtf8.FramePadding;
        var num2 = 3f * ImGuiHelpers.GlobalScale;
        // Determine total width.
        var result = ImGui.Button(string.Empty, new Vector2(width, ImUtf8.FrameHeight));
        // Offset the icon pos to the center.
        var iconPos = cursorScreenPos + new Vector2((width - padding.X - (iconSize.X + textSize.X + num2)) / 2f, padding.Y);
        using (Svc.PluginInterface.UiBuilder.IconFontFixedWidthHandle.Push())
            wdl.AddText(iconPos, ImGui.GetColorU32(ImGuiCol.Text), icon.ToIconString());
        // text pos is offset of icon pos.
        var pos2 = new Vector2(iconPos.X + iconSize.X + num2, iconPos.Y);
        wdl.AddText(pos2, ImGui.GetColorU32(ImGuiCol.Text), text);
        // pop the id, and color if included.
        ImGui.PopID();
        if (num > 0)
        {
            ImGui.PopStyleColor(num);
        }
        dis.Pop();

        return result && !disabled;
    }

    public static bool IconTextButtonCentered(FAI icon, string text, float width, bool isInPopup = false, bool disabled = false, string id = "Identifier")
        => IconTextButtonCenteredInternal(icon, text, width, isInPopup ? new Vector4(1.0f, 1.0f, 1.0f, 0.0f) : null, disabled, id);

    // Try to adjust overtime for simplification, so that it does not take up as much processing power.
    // At the moment while it is pretty it lacks proper performance optimizations and design caching.
    // See if we can manage better displays for it down the line if possible.

    public static float GetFancyButtonHeight(int border = 1, int shadow = 2)
        => ImUtf8.FrameHeight + 2 * ImGuiHelpers.ScaledVector2(border + shadow).Y;

    /// <summary>
    ///     Please do remember that the button's height will not be fixated to the frameHeight. 
    ///     You must also consider the border and shadow to consider.
    /// </summary>
    /// <param name="icon"> The icon to display on the button. </param>
    /// <param name="text"> The text beside the icon. </param>
    /// <param name="width"> How wide the button is. </param>
    /// <param name="disabled"> If the button is disabled. </param>
    /// <param name="border"> border size. Defaulted at 1. </param>
    /// <param name="shadow"> shadow size. Defaulted at 2. </param>
    /// <returns> Whether the button was pressed. </returns>
    public static bool FancyButton(FAI icon, string text, float width, bool disabled = false, float rounding = 25f, int border = 1, int shadow = 2)
    {
        // Get the internal window directly.
        var window = ImGuiInternal.GetCurrentWindow();
        if (window.SkipItems)
            return false;

        // Aquire our ID for this new internal item
        var id = ImGui.GetID(text);

        // Otherwise, grab the style and frame-heights as defaults.
        var pos = window.DC.CursorPos;
        var frameH = ImUtf8.FrameHeight;
        var style = ImGui.GetStyle();

        // Get the scaled versions of the border and shadow.
        var borderVec = ImGuiHelpers.ScaledVector2(border);
        var shadowVec = ImGuiHelpers.ScaledVector2(shadow);
        var outerOffset = borderVec + shadowVec;
        var trueHeight = frameH + 2 * outerOffset.Y;

        // Aquire a bounding box for our location.
        var itemSize = new Vector2(width, trueHeight);
        var hitbox = new ImRect(pos, pos + itemSize);

        // Add the item into the ImGuiInternals
        // (2nd paramater tells us how much from the outer edge to shift for text)
        ImGuiInternal.ItemSize(itemSize, style.FramePadding.Y + outerOffset.Y);
        if (!ImGuiP.ItemAdd(hitbox, id, null))
            return false;

        // Process interaction with this 'button'
        var hovered = false;
        var active  = false;
        var clicked = ImGuiP.ButtonBehavior(hitbox, id, ref hovered, ref active);

        // Define our colors based on states. (Update this later to static values in a colors class.

        uint shadowCol = 0x64000000;
        uint borderCol = CkGui.ApplyAlpha(0xDCDCDCDC, GetBorderAlpha(active, hovered, disabled));
        uint bgCol     = CkGui.ApplyAlpha(0x64000000, GetBgAlpha(active, hovered, disabled));
        uint textFade  = CkGui.ApplyAlpha(0xFF1E191E, disabled ? 0.5f : 1f);
        uint textCol   = CkGui.ApplyAlpha(0xFFFFFFFF, disabled ? 0.5f : 1f);

        // Text computation.
        var textSize = ImGui.CalcTextSize(text);
        var iconTextWidth = frameH + textSize.X;
        var iconPos = hitbox.Min + new Vector2((width - iconTextWidth) / 2f, (trueHeight - textSize.Y) / 2f);
        var textPos = iconPos + new Vector2(frameH, 0);


        // Render possible nav highlight space over the bounding box region.
        ImGuiP.RenderNavHighlight(hitbox, id);

        // Outer Drop Shadow on bottom.
        window.DrawList.AddRectFilled(hitbox.Min, hitbox.Max, shadowCol, rounding, ImDrawFlags.RoundCornersAll);
        // Draw over with inner border, greyish look.
        window.DrawList.AddRectFilled(hitbox.Min + borderVec, hitbox.Max - borderVec, borderCol, rounding, ImDrawFlags.RoundCornersAll);
        // Draw over again with the bgColor.
        window.DrawList.AddRectFilled(hitbox.Min + outerOffset, hitbox.Max - outerOffset, bgCol, rounding, ImDrawFlags.RoundCornersAll);
        // Then draw out the icon and text.
        using (Svc.PluginInterface.UiBuilder.IconFontFixedWidthHandle.Push())
            window.DrawList.OutlinedFont(icon.ToIconString(), iconPos, textCol, textFade, (int)shadowVec.X);
        window.DrawList.OutlinedFont(text, textPos, textCol, textFade, (int)shadowVec.X);

        return clicked && !disabled;
    }

    // For Border we want it to be brighter the more active it is.
    public static float GetBorderAlpha(bool active, bool hovered, bool disabled)
        => disabled ? 0.27f : active ? 0.7f : hovered ? 0.63f : 0.39f;

    // For the background we want it to have less alpha the brighter we want it.
    public static float GetBgAlpha(bool active, bool hovered, bool disabled)
        => disabled ? 0.44f : active ? 0.19f : hovered ? 0.26f : 0.39f;
}
