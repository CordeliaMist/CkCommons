using Dalamud.Bindings.ImGui;
using OtterGui.Raii;
using OtterGui.Text;
using System;

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
            using var endObject = ImRaii.Group();
            draw(enumerator.Current, usedWidth);
            endObject.Dispose();
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

        return (lastVisible >= firstVisible && firstVisible != -1) ? (data.Count() - lastVisible -1) : 0;
    }

    /// <summary>
    ///     Performs a dynamic clipped gallery draw with specified columns and item width. <para />
    ///     Can have any height on drawn items.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"> Items must be greater than 0. </exception>
    /// <returns> The remainder index. </returns>
    public static int DynamicClippedGalleryDraw<T>(IEnumerable<T> data, Action<T> draw, int columns, float itemWidth)
    {
        // Exception Handles.
        if (columns <= 0)   throw new ArgumentOutOfRangeException(nameof(columns), "Columns must be greater than zero.");
        if (itemWidth <= 0) throw new ArgumentOutOfRangeException(nameof(itemWidth), "Item width must be greater than zero.");

        // view the data in enumerator format.
        using IEnumerator<T> enumerator = data.GetEnumerator();

        // total row width (may exceed available region; caller passed explicit width)
        var rowWidth = columns * itemWidth + ((columns - 1) * ImUtf8.ItemSpacing.X);

        // Tracked variables.
        int index = 0;
        int firstVisible = -1;
        int lastVisible = -1;
        bool endReached = false;

        // While we have not reached the end of our drawn data.
        while (!endReached)
        {
            // track if the row has contents.
            bool rowHadVisible = false;

            // Draw out the column items.
            for (int col = 0; col < columns; ++col)
            {
                // If we are no longer able to move to the next item, we hit the end, so break out of the for-loop.
                if (!enumerator.MoveNext())
                {
                    endReached = true;
                    break;
                }

                // Otherwise define the group and draw the item.
                using var endObj = ImRaii.Group();
                draw(enumerator.Current);
                endObj.Dispose();
                // place next item on same line if not last column
                if (col < columns - 1)
                    ImUtf8.SameLineInner();

                // If the item is not visible, we can skip it.
                if (ImGui.IsItemVisible())
                {
                    if (firstVisible == -1)
                        firstVisible = index;
                    lastVisible = index;
                    rowHadVisible = true;
                }

                index++;
            }

            // If we already started seeing invisible rows and this row had no visible items, we are done.
            if (firstVisible != -1 && !rowHadVisible)
                break;
        }

        // Return the remainder index.
        return (lastVisible >= firstVisible && firstVisible != -1) ? (lastVisible - firstVisible + 1) : 0;
    }

    /// <summary>
    ///     Clipped gallery draw with specified columns and item width. Can have any height on drawn items. <para />
    ///     Passes the local IDX iterator to the draw actions.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"> Items must be greater than 0. </exception>
    /// <returns> The remainder index. </returns>
    public static int DynamicClippedGalleryDraw<T>(IEnumerable<T> data, Action<T, int> draw, int columns, float itemWidth)
    {
        // Exception Handles.
        if (columns <= 0) throw new ArgumentOutOfRangeException(nameof(columns), "Columns must be greater than zero.");
        if (itemWidth <= 0) throw new ArgumentOutOfRangeException(nameof(itemWidth), "Item width must be greater than zero.");

        // view the data in enumerator format.
        using IEnumerator<T> enumerator = data.GetEnumerator();

        // total row width (may exceed available region; caller passed explicit width)
        var rowWidth = columns * itemWidth + ((columns - 1) * ImUtf8.ItemInnerSpacing.X);

        // Tracked variables.
        int index = 0;
        int firstVisible = -1;
        int lastVisible = -1;
        bool endReached = false;

        // While we have not reached the end of our drawn data.
        while (!endReached)
        {
            // track if the row has contents.
            bool rowHadVisible = false;

            // Draw out the column items.
            for (int col = 0; col < columns; ++col)
            {
                // If we are no longer able to move to the next item, we hit the end, so break out of the for-loop.
                if (!enumerator.MoveNext())
                {
                    endReached = true;
                    break;
                }

                // Otherwise define the group and draw the item.
                using ImRaii.IEndObject endObj = ImRaii.Group();
                draw(enumerator.Current, index);
                endObj.Dispose();

                // place next item on same line if not last column
                if (col < columns - 1)
                    ImUtf8.SameLineInner();

                // If the item is not visible, we can skip it.
                if (ImGui.IsItemVisible())
                {
                    if (firstVisible == -1)
                        firstVisible = index;
                    lastVisible = index;
                    rowHadVisible = true;
                }

                index++;
            }

            // If we already started seeing invisible rows and this row had no visible items, we are done.
            if (firstVisible != -1 && !rowHadVisible)
                break;
        }

        // Return the remainder index.
        return (lastVisible >= firstVisible && firstVisible != -1) ? (lastVisible - firstVisible + 1) : 0;
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
