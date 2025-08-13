using Dalamud.Bindings.ImGui;
using System.Runtime.InteropServices;

namespace CkCommons.Gui.Utility;
public static partial class CkGuiUtils
{
    private static int FindWrapPosition(string text, float wrapWidth)
    {
        float currentWidth = 0;
        int lastSpacePos = -1;
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            currentWidth += ImGui.CalcTextSize(c.ToString()).X;
            if (char.IsWhiteSpace(c))
            {
                lastSpacePos = i;
            }
            if (currentWidth > wrapWidth)
            {
                return lastSpacePos >= 0 ? lastSpacePos : i;
            }
        }
        return -1;
    }

    private static string FormatTextForDisplay(string text, float wrapWidth)
    {
        // Normalize newlines for processing
        text = text.Replace("\r\n", "\n");
        List<string> lines = text.Split('\n').ToList();

        // Traverse each line to check if it exceeds the wrap width
        for (int i = 0; i < lines.Count; i++)
        {
            float lineWidth = ImGui.CalcTextSize(lines[i]).X;

            while (lineWidth > wrapWidth)
            {
                // Find where to break the line
                int wrapPos = FindWrapPosition(lines[i], wrapWidth);
                if (wrapPos >= 0)
                {
                    // Insert a newline at the wrap position
                    string part1 = lines[i].Substring(0, wrapPos);
                    string part2 = lines[i].Substring(wrapPos).TrimStart();
                    lines[i] = part1;
                    lines.Insert(i + 1, part2);
                    lineWidth = ImGui.CalcTextSize(part2).X;
                }
                else
                {
                    break;
                }
            }
        }

        // Join lines with \n for internal representation
        return string.Join("\n", lines);
    }

    private static unsafe int TextEditCallback(ImGuiInputTextCallbackData* data, float wrapWidth)
    {
        string text = Marshal.PtrToStringAnsi((IntPtr)data->Buf, data->BufTextLen);

        // Normalize newlines for processing
        text = text.Replace("\r\n", "\n");
        List<string> lines = text.Split('\n').ToList();

        bool textModified = false;

        // Traverse each line to check if it exceeds the wrap width
        for (int i = 0; i < lines.Count; i++)
        {
            float lineWidth = ImGui.CalcTextSize(lines[i]).X;

            // Skip wrapping if this line ends with \r (i.e., it's a true newline)
            if (lines[i].EndsWith("\r"))
            {
                continue;
            }

            while (lineWidth > wrapWidth)
            {
                // Find where to break the line
                int wrapPos = FindWrapPosition(lines[i], wrapWidth);
                if (wrapPos >= 0)
                {
                    // Insert a newline at the wrap position
                    string part1 = lines[i].Substring(0, wrapPos);
                    string part2 = lines[i].Substring(wrapPos).TrimStart();
                    lines[i] = part1;
                    lines.Insert(i + 1, part2);
                    textModified = true;
                    lineWidth = ImGui.CalcTextSize(part2).X;
                }
                else
                {
                    break;
                }
            }
        }

        // Merge lines back to the buffer
        if (textModified)
        {
            string newText = string.Join("\n", lines); // Use \n for internal representation

            byte[] newTextBytes = Encoding.UTF8.GetBytes(newText.PadRight(data->BufSize, '\0'));
            Marshal.Copy(newTextBytes, 0, (IntPtr)data->Buf, newTextBytes.Length);
            data->BufTextLen = newText.Length;
            data->BufDirty = 1;
            data->CursorPos = Math.Min(data->CursorPos, data->BufTextLen);
        }

        return 0;
    }

    public unsafe static bool InputTextWrapMultiline(string id, ref string text, int maxLength = 500, int lineHeight = 2, float? width = null)
    {
        float wrapWidth = width ?? ImGui.GetContentRegionAvail().X; // Determine wrap width

        // Format text for display
        text = FormatTextForDisplay(text, wrapWidth);

        bool result = ImGui.InputTextMultiline(id, ref text, maxLength,
             new(width ?? ImGui.GetContentRegionAvail().X, ImGui.GetTextLineHeightWithSpacing() * lineHeight), // Expand height calculation
             ImGuiInputTextFlags.CallbackEdit | ImGuiInputTextFlags.NoHorizontalScroll, // Flag settings
             (data) => { return TextEditCallback(data, wrapWidth); });

        // Restore \r\n for display consistency
        text = text.Replace("\n", "");

        return result;
    }

}
