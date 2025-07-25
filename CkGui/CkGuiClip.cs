using ImGuiNET;
using OtterGui.Raii;

namespace CkCommons.Gui;

// ClippedDraw Methods are taken from OtterGui's ImGuiClip func, and modified to allow for a width parameter.
public static class CkGuiClip
{
    /// <summary>
    ///     A variant of ImGuiClip that works with unfixed heights, allowing it to draw large performance friendly lists.
    /// </summary>
    /// <returns> Returns the remainder index. </returns>
    public static int DynamicClippedDraw<T>(IEnumerable<T> data, Action<T, float> draw, float? width = null)
    {
        using IEnumerator<T> enumerator = data.GetEnumerator();
        float usedWidth = width ?? ImGui.GetContentRegionAvail().X;

        int index = 0;
        int firstVisible = -1;
        int lastVisible = -1;

        while (enumerator.MoveNext())
        {
            // draw to check for visibility.
            using ImRaii.IEndObject endObject = ImRaii.Group();
            draw(enumerator.Current, usedWidth);

            // If the item is not visible, we can skip it.
            if (ImGui.IsItemVisible())
            {
                if (firstVisible == -1)
                    firstVisible = index;

                lastVisible = index;
            }
            else if (firstVisible != -1)
            {
                // went beyond the total visible range, so break out.
                break;
            }
            index++;
        }

        return (lastVisible >= firstVisible && firstVisible != -1)
            ? (lastVisible - firstVisible + 1) : 0;
    }


    /// <summary>
    ///     A variant of ImGuiClip that accepts the width paramater to define drawlength.
    /// </summary>
    public static int FilteredClippedDraw<T>(IEnumerable<T> data, int skips, Func<T, bool> checkFilter, Action<T, float> draw, float? width = null)
        => ClippedDraw(data.Where(checkFilter), skips, draw, width);

    /// <summary>
    ///    A variant of ImGuiClip that accepts the width paramater to define drawlength.
    /// </summary>
    public static int ClippedDraw<T>(IEnumerable<T> data, int skips, Action<T, float> draw, float? width = null)
    {
        using IEnumerator<T> enumerator = data.GetEnumerator();
        bool flag = false;
        int num = 0;
        float usedWidth = width ?? ImGui.GetContentRegionAvail().X;
        while (enumerator.MoveNext())
        {
            if (num >= skips)
            {
                using ImRaii.IEndObject endObject = ImRaii.Group();
                draw(enumerator.Current, usedWidth);
                endObject.Dispose();
                if (!ImGui.IsItemVisible())
                {
                    if (flag)
                    {
                        int num2 = 0;
                        while (enumerator.MoveNext())
                        {
                            num2++;
                        }

                        return num2;
                    }
                }
                else
                {
                    flag = true;
                }
            }

            num++;
        }

        return ~num;
    }

    public static int ClippedDrawSetHeight<T>(IEnumerable<T> data, int skips, float height, Action<T, float, float> draw, float? width = null)
    {
        using IEnumerator<T> enumerator = data.GetEnumerator();
        bool flag = false;
        int num = 0;
        float usedWidth = width ?? ImGui.GetContentRegionAvail().X;
        while (enumerator.MoveNext())
        {
            if (num >= skips)
            {
                using ImRaii.IEndObject endObject = ImRaii.Group();
                draw(enumerator.Current, usedWidth, height);
                endObject.Dispose();
                if (!ImGui.IsItemVisible())
                {
                    if (flag)
                    {
                        int num2 = 0;
                        while (enumerator.MoveNext())
                        {
                            num2++;
                        }

                        return num2;
                    }
                }
                else
                {
                    flag = true;
                }
            }

            num++;
        }

        return ~num;
    }

}
