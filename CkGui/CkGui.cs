using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using OtterGui;
using OtterGui.Text;

namespace CkCommons.Gui;

// Primary Partial Class
public static partial class CkGui
{
    /// <summary> A helper function for centering the next displayed window. </summary>
    /// <param name="width"> The width of the window. </param>
    /// <param name="height"> The height of the window. </param>
    /// <param name="cond"> The condition for the ImGuiWindow to be displayed . </param>
    public static void CenterNextWindow(float width, float height, ImGuiCond cond = ImGuiCond.None)
    {
        var center = ImGui.GetMainViewport().GetCenter();
        ImGui.SetNextWindowPos(new Vector2(center.X - width / 2, center.Y - height / 2), cond);
    }

    /// <summary> A helper function for retrieving the proper color value given RGBA. </summary>
    /// <returns> The color formatted as a uint </returns>
    public static uint Color(byte r, byte g, byte b, byte a)
    { uint ret = a; ret <<= 8; ret += b; ret <<= 8; ret += g; ret <<= 8; ret += r; return ret; }

    /// <summary> A helper function for retrieving the proper color value given a vector4. </summary>
    /// <returns> The color formatted as a uint </returns>
    public static uint Color(Vector4 color)
    {
        uint ret = (byte)(color.W * 255);
        ret <<= 8;
        ret += (byte)(color.Z * 255);
        ret <<= 8;
        ret += (byte)(color.Y * 255);
        ret <<= 8;
        ret += (byte)(color.X * 255);
        return ret;
    }

    public static float GetAlpha(uint color)
    {
        var alpha = (byte)(color >> 24);
        return alpha / 255f;
    }

    public static uint InvertColor(uint color)
    {
        var r = 0xFF - (color & 0xFF);
        var g = 0xFF - ((color >> 8) & 0xFF);
        var b = 0xFF - ((color >> 16) & 0xFF);
        var a = (color >> 24) & 0xFF;

        return (a << 24) | (b << 16) | (g << 8) | r;
    }

    public static Vector4 InvertColor(Vector4 color)
        => new Vector4(1f - color.X, 1f - color.Y, 1f - color.Z, color.W);


    public static Vector4 GetBoolColor(bool input) => input ? ImGuiColors.ParsedGreen : ImGuiColors.DalamudRed;

/*    public float GetFontScalerFloat() => ImGuiHelpers.GlobalScale * (_pi.UiBuilder.DefaultFontSpec.SizePt / 12f); */
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

    public static float GetWindowContentRegionWidth()
        => ImGui.GetWindowContentRegionMax().X - ImGui.GetWindowContentRegionMin().X;

    public static void InlineSpacing()
    {
        ImGui.Spacing();
        ImGui.SameLine();
    }

    public static void InlineSpacingInner()
    {
        ImGui.Spacing();
        ImUtf8.SameLineInner();
    }

    /// <summary>
    ///     Checkbox, but with a disable condition.
    /// </summary>
    public static bool Checkbox(ImU8String label, ref bool v, bool disabled)
    {
        using var _ = ImRaii.Disabled(disabled);
        return ImGui.Checkbox(label, ref v);
    }

    public static float GetSeparatorVWidth(float? width = null)
        => ImGui.GetStyle().ItemSpacing.X * 2 + (width ?? 1 * ImGuiHelpers.GlobalScale);

    public static float GetSeparatorHeight(float? height = null)
        => height + ImGui.GetStyle().ItemSpacing.Y * 2 ?? ImGui.GetStyle().ItemSpacing.Y * 3;

    public static float GetSeparatorSpacedHeight(float? height = null)
        => height + ImGui.GetStyle().ItemSpacing.Y * 4 ?? ImGui.GetStyle().ItemSpacing.Y * 5;

    public static void Separator(float? width = null, float? height = null)
    {
        ImGui.Dummy(new Vector2(width ?? ImGui.GetContentRegionAvail().X, height ?? ImGui.GetStyle().ItemSpacing.Y));
    }


    public static void Separator(uint col, float? width = null, float? height = null)
    {
        ImGui.Dummy(new Vector2(width ?? ImGui.GetContentRegionAvail().X, height ?? ImGui.GetStyle().ItemSpacing.Y));
        ImGui.GetWindowDrawList().AddRectFilled(ImGui.GetItemRectMin(), ImGui.GetItemRectMax(), col);
    }

    public static void SeparatorSpaced(float? height = null, float? width = null)
    {
        ImGui.Spacing();
        ImGui.Dummy(new Vector2(width ?? ImGui.GetContentRegionAvail().X, height ?? ImGui.GetStyle().ItemSpacing.Y));
        ImGui.Spacing();
    }

    public static void SeparatorSpaced(uint col, float? height = null, float? width = null)
    {
        ImGui.Spacing();
        ImGui.Dummy(new Vector2(width ?? ImGui.GetContentRegionAvail().X, height ?? ImGui.GetStyle().ItemSpacing.Y));
        ImGui.GetWindowDrawList().AddRectFilled(ImGui.GetItemRectMin(), ImGui.GetItemRectMax(), col);
        ImGui.Spacing();
    }

    public static void SeparatorV(float? width = null, uint? col = null, float? height = null)
    {
        ImGui.SameLine(0, ImUtf8.ItemSpacing.X);
        var lineHeight = height ?? ImGui.GetContentRegionAvail().Y;
        var lineWidth = width ?? 1 * ImGuiHelpers.GlobalScale;
        col ??= ImGui.GetColorU32(ImGuiCol.Border);
        ImGui.Dummy(new Vector2(lineWidth, lineHeight));
        ImGui.GetWindowDrawList().AddRectFilled(ImGui.GetItemRectMin(), ImGui.GetItemRectMax(), col.Value);
        ImGui.SameLine();
    }

    public static void TextLineSeparatorV(float? width = null, uint? col = null)
        => SeparatorV(width, col, ImGui.GetTextLineHeight());

    public static void FrameSeparatorV(float? width = null, uint? col = null)
        => SeparatorV(width, col, ImGui.GetFrameHeight());

    // A 'small button' that is not a button. Alternatively could use some disabled trickery,
    // but this saves the extra render data with buttons.
    // If using a disabled small button is more benificial than this, do that instead.
    public static void TagLabelText(string text, uint? col = null, float? padding = null)
    {
        var pos = ImGui.GetCursorScreenPos();
        var size = ImGui.CalcTextSize(text);
        var fPad = ImGui.GetStyle().FramePadding;
        pos.Y += fPad.Y;
        var tagCol = col ?? ImGui.GetColorU32(ImGuiCol.Button);
        var tagPadX = padding ?? fPad.X;
        var padWidth = new Vector2(tagPadX, 0);
        // Draw out the text and stuff.
        ImGui.GetWindowDrawList().AddRectFilled(pos - padWidth, pos + size + padWidth, tagCol, ImGui.GetStyle().FrameRounding);
        ImGui.Text(text);
    }

    public static void TagLabelText(string text, Vector4 col, float? padding = null)
    {
        var pos = ImGui.GetCursorScreenPos();
        var size = ImGui.CalcTextSize(text);
        var tagPadX = padding ?? ImUtf8.FramePadding.X;
        var padWidth = new Vector2(tagPadX, 0);
        // Draw out the text and stuff.
        ImGui.GetWindowDrawList().AddRectFilled(pos - padWidth, pos + size + padWidth, col.ToUint(), ImGui.GetStyle().FrameRounding);
        ImGui.Text(text);
    }

    public static void TagLabelTextFrameAligned(string text, Vector4 col, float? padding = null)
    {
        var pos = ImGui.GetCursorScreenPos();
        var size = ImGui.CalcTextSize(text);
        var fPad = ImGui.GetStyle().FramePadding; 
        pos.Y += fPad.Y;
        var tagPadX = padding ?? fPad.X;
        var padWidth = new Vector2(tagPadX, 0);
        // Draw out the text and stuff.
        ImGui.GetWindowDrawList().AddRectFilled(pos - padWidth, pos + size + padWidth, col.ToUint(), ImGui.GetStyle().FrameRounding);
        ImGui.Text(text);
    }

    // maybe support padding here later or something idk.
    public static void TagLabelColorText(string text, uint col, uint? labelCol = null)
    {
        var labelColor = labelCol ?? ImGui.GetColorU32(ImGuiCol.Button);
        var size = ImGui.CalcTextSize(text);
        var padding = ImGui.GetStyle().FramePadding;
        var padWidth = new Vector2(padding.X, 0);
        var pos = ImGui.GetCursorScreenPos();
        pos.Y += padding.Y;
        // Draw out the text and stuff.
        ImGui.GetWindowDrawList().AddRectFilled(pos - padWidth, pos + size + padWidth * 2, labelColor, ImGui.GetStyle().FrameRounding);
        CkGui.ColorText(text, col);
    }

    public static void TagLabelColorText(string text, Vector4 col, Vector4 labelCol)
    {
        var size = ImGui.CalcTextSize(text);
        var padding = ImGui.GetStyle().FramePadding;
        var padWidth = new Vector2(padding.X, 0);
        var pos = ImGui.GetCursorScreenPos();
        pos.Y += padding.Y;
        // Draw out the text and stuff.
        ImGui.GetWindowDrawList().AddRectFilled(pos - padWidth, pos + size + padWidth * 2, labelCol.ToUint(), ImGui.GetStyle().FrameRounding);
        CkGui.ColorText(text, col);
    }

    /// <summary> The additional param for an ID is optional. if not provided, the id will be the text. </summary>
    public static bool IconButton(FAI icon, float? height = null, string? id = null, bool disabled = false, bool inPopup = false)
        => IconButtonInternal(icon, height, id, disabled, inPopup);
    private static bool IconButtonInternal(FAI icon, float? height = null, string? id = null, bool disabled = false, bool inPopup = false, uint? color = null)
    {
        using var dis = ImRaii.PushStyle(ImGuiStyleVar.Alpha, disabled ? 0.5f : 1f);
        var num = 0;
        if (inPopup)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(1.0f, 1.0f, 1.0f, 0.0f));
            num++;
        }
        var txtCol = color ?? ImGui.GetColorU32(ImGuiCol.Text);
        var text = icon.ToIconString();

        ImGui.PushID((id == null) ? icon.ToIconString() : id + icon.ToIconString());
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
            windowDrawList.AddText(pos, txtCol, text);
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

    private static bool IconSliderFloatInternal(string id, FAI icon, string label, ref float valueRef, float min,
        float max, Vector4? defaultColor = null, float? width = null, bool disabled = false, string format = "%.1f")
    {
        using var dis = ImRaii.PushStyle(ImGuiStyleVar.Alpha, disabled ? 0.5f : 1f);
        var num = 0;
        // Disable if issues, tends to be culpret
        if (defaultColor.HasValue)
        {
            ImGui.PushStyleColor(ImGuiCol.FrameBg, defaultColor.Value);
            num++;
        }

        ImGui.PushID(id);
        Vector2 vector;
        using (Svc.PluginInterface.UiBuilder.IconFontFixedWidthHandle.Push())
            vector = ImGui.CalcTextSize(icon.ToIconString());
        var vector2 = ImGui.CalcTextSize(label);
        var windowDrawList = ImGui.GetWindowDrawList();
        var cursorScreenPos = ImGui.GetCursorScreenPos();
        var num2 = 3f * ImGuiHelpers.GlobalScale;
        var x = width ?? vector.X + vector2.X + ImGui.GetStyle().FramePadding.X * 2f + num2;
        ImGui.SetCursorPosX(vector.X + ImGui.GetStyle().FramePadding.X * 2f);
        ImGui.SetNextItemWidth(x - vector.X - num2 * 4); // idk why this works, it probably doesnt on different scaling. Idfk. Look into later.
        var result = ImGui.SliderFloat(label + "##" + id, ref valueRef, min, max, format);

        var pos = new Vector2(cursorScreenPos.X + ImGui.GetStyle().FramePadding.X, cursorScreenPos.Y + ImGui.GetStyle().FramePadding.Y);
        using (Svc.PluginInterface.UiBuilder.IconFontFixedWidthHandle.Push())
            windowDrawList.AddText(pos, ImGui.GetColorU32(ImGuiCol.Text), icon.ToIconString());
        ImGui.PopID();
        if (num > 0)
        {
            ImGui.PopStyleColor(num);
        }
        dis.Pop();

        return result && !disabled;
    }

    public static bool IconSliderFloat(string id, FAI icon, string label, ref float valueRef,
        float min, float max, float? width = null, bool isInPopup = false, bool disabled = false)
    {
        return IconSliderFloatInternal(id, icon, label, ref valueRef, min, max,
            isInPopup ? new Vector4(1.0f, 1.0f, 1.0f, 0.1f) : null,
            width <= 0 ? null : width,
            disabled);
    }

    private static bool IconInputTextInternal(FAI icon, string label, string hint, ref string inputStr,
        int maxLength, Vector4? color = null, float? width = null, bool disabled = false)
    {
        using var dis = ImRaii.PushStyle(ImGuiStyleVar.Alpha, disabled ? 0.5f : 1f);
        var num = 0;
        // Disable if issues, tends to be culpret
        if (color.HasValue)
        {
            ImGui.PushStyleColor(ImGuiCol.FrameBg, color.Value);
            num++;
        }

        var flags = ITFlags.EnterReturnsTrue | (disabled ? ITFlags.ReadOnly : ITFlags.None);

        var id = label + "##" + icon.ToIconString();
        Vector2 vector;
        using (Svc.PluginInterface.UiBuilder.IconFontFixedWidthHandle.Push())
            vector = ImGui.CalcTextSize(icon.ToIconString());

        ImGui.PushID(id);

        var vector2 = ImGui.CalcTextSize(label);
        var windowDrawList = ImGui.GetWindowDrawList();
        var cursorScreenPos = ImGui.GetCursorScreenPos();
        var num2 = 3f * ImGuiHelpers.GlobalScale;
        var x = width ?? vector.X + vector2.X + ImGui.GetStyle().FramePadding.X * 2f + num2;
        var frameHeight = ImGui.GetFrameHeight();
        ImGui.SetCursorPosX(vector.X + ImGui.GetStyle().FramePadding.X * 2f);
        ImGui.SetNextItemWidth(x - vector.X - num2 * 4); // idk why this works, it probably doesnt on different scaling. Idfk. Look into later.
        var result = ImGui.InputTextWithHint(label, hint, ref inputStr, maxLength, flags);

        var pos = new Vector2(cursorScreenPos.X + ImGui.GetStyle().FramePadding.X, cursorScreenPos.Y + ImGui.GetStyle().FramePadding.Y);
        using (Svc.PluginInterface.UiBuilder.IconFontFixedWidthHandle.Push())
            windowDrawList.AddText(pos, ImGui.GetColorU32(ImGuiCol.Text), icon.ToIconString());
        ImGui.PopID();
        if (num > 0)
        {
            ImGui.PopStyleColor(num);
        }
        dis.Pop();

        return result && !disabled;
    }

    public static bool IconInputText(FAI icon, string label, string hint, ref string inputStr,
        int maxLength, float? width = null, bool isInPopup = false, bool disabled = false)
    {
        return IconInputTextInternal(icon, label, hint, ref inputStr, maxLength,
            isInPopup ? new Vector4(1.0f, 1.0f, 1.0f, 0.1f) : null,
            width <= 0 ? null : width,
            disabled);
    }

    public static void SetScaledWindowSize(float width, bool centerWindow = true)
    {
        var newLineHeight = ImGui.GetCursorPosY();
        ImGui.NewLine();
        newLineHeight = ImGui.GetCursorPosY() - newLineHeight;
        var y = ImGui.GetCursorPos().Y + ImGui.GetWindowContentRegionMin().Y - newLineHeight * 2 - ImGui.GetStyle().ItemSpacing.Y;

        SetScaledWindowSize(width, y, centerWindow, scaledHeight: true);
    }

    public static void SetScaledWindowSize(float width, float height, bool centerWindow = true, bool scaledHeight = false)
    {
        ImGui.SameLine();
        var x = width * ImGuiHelpers.GlobalScale;
        var y = scaledHeight ? height : height * ImGuiHelpers.GlobalScale;

        if (centerWindow)
        {
            CenterWindow(x, y);
        }

        ImGui.SetWindowSize(new Vector2(x, y));
    }

    public static void SetCursorXtoCenter(float width)
        => ImGui.SetCursorPosX((ImGui.GetWindowContentRegionMax().X - ImGui.GetWindowContentRegionMin().X) / 2 - width / 2);

    // make a non-framed variant of this soon.
    public static void BooleanToColoredIcon(bool value, bool inline = true, FAI trueIcon = FAI.Check, FAI falseIcon = FAI.Times, Vector4 colorTrue = default, Vector4 colorFalse = default)
    {
        if (inline)
            ImUtf8.SameLineInner();

        if (value)
            using (ImRaii.PushColor(ImGuiCol.Text, (colorTrue == default) ? ImGuiColors.HealerGreen : colorTrue)) FramedIconText(trueIcon);
        else
            using (ImRaii.PushColor(ImGuiCol.Text, (colorFalse == default) ? ImGuiColors.DalamudRed : colorFalse)) FramedIconText(falseIcon);
    }

    private static void CenterWindow(float width, float height, ImGuiCond cond = ImGuiCond.None)
    {
        var center = ImGui.GetMainViewport().GetCenter();
        ImGui.SetWindowPos(new Vector2(center.X - width / 2, center.Y - height / 2), cond);
    }
}
