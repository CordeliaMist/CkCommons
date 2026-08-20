using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using OtterGui.Text;

namespace CkCommons.Gui.Utility;

public static partial class CkGuiUtils
{
    public static bool ColorEditNative(string label, ref NativeUiColor colors, uint? tipCol = null, NativeUiColor defaultCol = default)
    {
        using var imId = ImRaii.PushId(label);
        var spacing = ImUtf8.ItemInnerSpacing.X;
        var fgCol = ConvertColor(colors.Foreground);
        var glowCol = ConvertColor(colors.Glow);
        var tooltipCol = tipCol.HasValue ? tipCol.Value : ImGui.GetColorU32(ImGuiCol.Text);

        var ret = ImGui.ColorEdit3("###foreground", ref fgCol, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel | ImGuiColorEditFlags.Uint8);
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            fgCol = ConvertColor(defaultCol.Foreground);
            ret = true;
        }
        CkGui.AttachTooltip($"Foreground color.--NL----COL--[R-Click]--COL-- Reset to the default color.", tooltipCol);
        
        ImUtf8.SameLineInner();
        ret |= ImGui.ColorEdit3("###glow", ref glowCol, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel | ImGuiColorEditFlags.Uint8);
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            glowCol = ConvertColor(defaultCol.Glow);
            ret = true;
        }
        CkGui.AttachTooltip($"Glow color.--NL----COL--[R-Click]--COL-- Reset to the default color.", tooltipCol);

        CkGui.TextInline(label);
        if (ret)
            colors = new(ConvertBackColor(fgCol), ConvertBackColor(glowCol));
        
        return ret;
    }

    public static bool ColorEditNativeForeground(string label, ref NativeUiColor colors, uint? tipCol = null, NativeUiColor defaultCol = default)
    {
        using var imId = ImRaii.PushId(label);
        var spacing = ImUtf8.ItemInnerSpacing.X;
        var fgCol = ConvertColor(colors.Foreground);
        var tooltipCol = tipCol.HasValue ? tipCol.Value : ImGui.GetColorU32(ImGuiCol.Text);
        var ret = ImGui.ColorEdit3("###foreground", ref fgCol, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel | ImGuiColorEditFlags.Uint8);
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            fgCol = ConvertColor(defaultCol.Foreground);
            ret = true;
        }
        CkGui.AttachTooltip($"Foreground color.--NL----COL--[R-Click]--COL-- Reset to the default color.", tooltipCol);
        CkGui.TextInline(label);
        if (ret)
            colors = new(ConvertBackColor(fgCol), colors.Glow);
        
        return ret;
    }

    public static bool ColorEditNativeGlow(string label, ref NativeUiColor colors, uint? tipCol = null, NativeUiColor defaultCol = default)
    {
        using var imId = ImRaii.PushId(label);
        var spacing = ImUtf8.ItemInnerSpacing.X;
        var glowCol = ConvertColor(colors.Glow);
        var tooltipCol = tipCol.HasValue ? tipCol.Value : ImGui.GetColorU32(ImGuiCol.Text);
        var ret = ImGui.ColorEdit3("###glow", ref glowCol, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel | ImGuiColorEditFlags.Uint8);
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            glowCol = ConvertColor(defaultCol.Glow);
            ret = true;
        }
        CkGui.AttachTooltip($"Glow color.--NL----COL--[R-Click]--COL-- Reset to the default color.", tooltipCol);
        CkGui.TextInline(label);
        if (ret)
            colors = new(colors.Foreground, ConvertBackColor(glowCol));
        
        return ret;
    }

    public static bool ColorEditNativeLabeled(string id, ref NativeUiColor colors, string label, string glowLabel, uint? tipCol = null, NativeUiColor defaultCol = default)
    {
        using var imId = ImRaii.PushId(id);
        var spacing = ImUtf8.ItemInnerSpacing.X;
        var fgCol = ConvertColor(colors.Foreground);
        var glowCol = ConvertColor(colors.Glow);
        var tooltipCol = tipCol.HasValue ? tipCol.Value : ImGui.GetColorU32(ImGuiCol.Text);

        var ret = ImGui.ColorEdit3($"{label}###foreground", ref fgCol, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel | ImGuiColorEditFlags.Uint8);
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            fgCol = ConvertColor(defaultCol.Foreground);
            ret = true;
        }
        CkGui.AttachTooltip($"{label} color.--NL----COL--[R-Click]--COL-- Reset to the default color.", tooltipCol);

        ImUtf8.SameLineInner();
        ret |= ImGui.ColorEdit3($"{glowLabel}###glow", ref glowCol, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel | ImGuiColorEditFlags.Uint8);
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            glowCol = ConvertColor(defaultCol.Glow);
            ret = true;
        }
        CkGui.AttachTooltip($"{glowLabel} color.--NL----COL--[R-Click]--COL-- Reset to the default color.", tooltipCol);

        if (ret)
            colors = new(ConvertBackColor(fgCol), ConvertBackColor(glowCol));
        
        return ret; 
    }

    private static Vector3 ConvertColor(uint color)
        => unchecked(new((byte)color / 255.0f, (byte)(color >> 8) / 255.0f, (byte)(color >> 16) / 255.0f));

    private static uint ConvertBackColor(Vector3 color)
        => byte.CreateSaturating(color.X * 255.0f) 
        | ((uint)byte.CreateSaturating(color.Y * 255.0f) << 8)
        | ((uint)byte.CreateSaturating(color.Z * 255.0f) << 16)
        | (255u << 24);
}
