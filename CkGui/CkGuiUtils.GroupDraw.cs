using CkCommons.Raii;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using OtterGui.Text;
using OtterGui.Widgets;

namespace CkCommons.Gui.Utility;

// Mimics the penumbra mod groups from penumbra mod selection.
public static partial class CkGuiUtils
{
    /// <summary>
    ///     Draws a stylized progress bar with rounded corners, outlined text, and customizable size, color, and label.
    /// </summary>
    /// <param name="progress">Progress value from 0.0 to 1.0.</param>
    /// <param name="size">Total size of the bar. Height should usually match font height.</param>
    /// <param name="text">Optional label text. If null, a percentage will be shown.</param>
    /// <param name="fillColor">The color to fill the progress with.</param>
    /// <param name="rounding">Optional rounding radius. Default is 25f.</param>
    public static void DrawProgressBar(Vector2 size, string text, float progress, uint? fillColor = null, float rounding = 25f)
    {
        // Clamp progress.
        progress = Math.Clamp(progress, 0f, 1f);

        var drawList = ImGui.GetWindowDrawList();
        var cursorStart = ImGui.GetCursorScreenPos();
        var textSize = ImGui.CalcTextSize(text);
        var fillCol = fillColor.HasValue ? fillColor.Value : CkColor.VibrantPink.Uint();

        // Bg Layers.
        var bgStart = cursorStart;
        var bgEnd = bgStart + size;
        drawList.AddRectFilled(bgStart - Vector2.One, bgEnd + Vector2.One, CkGui.Color(0, 0, 0, 100), rounding, DFlags.RoundCornersAll); // Shadow
        drawList.AddRectFilled(bgStart, bgEnd, CkGui.Color(0, 0, 0, 100), rounding, DFlags.RoundCornersAll);                          // Background

        // Progress Bar
        if (progress > 0.025f)
        {
            var fillEnd = new Vector2(bgStart.X + size.X * progress, bgEnd.Y);
            drawList.AddRectFilled(bgStart, fillEnd, fillCol, rounding, DFlags.RoundCornersAll);
        }

        // Centered text
        var textPos = bgStart + new Vector2((size.X - textSize.X) / 2f, (size.Y - textSize.Y) / 2f);
        drawList.OutlinedFont(text, textPos, CkGui.Color(255, 255, 255, 255), CkGui.Color(53, 24, 39, 255), 1);

        // Reserve space so ImGui continues correctly
        ImGui.Dummy(size);
    }

    public static void FramedEditDisplay(string id, float width, bool inEdit, string curLabel,
        Action<float> drawAct, uint editorBg = 0, float? height = null)
    {
        uint col = inEdit ? editorBg : CkColor.FancyHeaderContrast.Uint();
        using (CkRaii.Child(id + "frameDisp", new Vector2(width, height ?? ImGui.GetFrameHeight()), col, 
            CkStyle.ChildRounding(), DFlags.RoundCornersAll))
        {
            if (inEdit)
                drawAct?.Invoke(width);
            else
                CkGui.CenterTextAligned(curLabel);
        }
    }


    /// <summary> Draw a single group selector as a combo box. (For Previewing) </summary>
    public static void DrawSingleGroupCombo(string groupName, string[] options, string current)
    {
        float comboWidth = ImGui.GetContentRegionAvail().X / 2;
        StringCombo(groupName, comboWidth, current, out _, options.AsEnumerable(), "None Selected...");
    }
    /// <summary> Draw a single group selector as a set of radio buttons. (for Previewing) </summary>
    public static void DrawSingleGroupRadio(string groupName, string[] options, string current)
    {
        string newSelection = current; // Ensure assignment
        using OtterGui.Text.EndObjects.Id id = ImUtf8.PushId(groupName);
        float minWidth = Widget.BeginFramedGroup(groupName);

        using (ImRaii.Disabled(false))
        {
            for (int idx = 0; idx < options.Length; ++idx)
            {
                using OtterGui.Text.EndObjects.Id i = ImUtf8.PushId(idx);
                string option = options[idx];
                if (ImUtf8.RadioButton(option, current == option))
                    newSelection = option;
            }
        }
        Widget.EndFramedGroup();
    }

    /// <summary> Draw a multi group selector as a bordered set of checkboxes. (for previewing) </summary>
    public static void DrawMultiGroup(string groupName, string[] options, string[] current)
    {
        using OtterGui.Text.EndObjects.Id id = ImUtf8.PushId(groupName);
        float minWidth = Widget.BeginFramedGroup(groupName);

        using (ImRaii.Disabled(false))
        {
            for (int idx = 0; idx < options.Length; ++idx)
            {
                using OtterGui.Text.EndObjects.Id i = ImUtf8.PushId(idx);
                string option = options[idx];
                bool isSelected = current.Contains(option);
                ImUtf8.Checkbox(option, ref isSelected);
            }
        }
        Widget.EndFramedGroup();
    }
}
