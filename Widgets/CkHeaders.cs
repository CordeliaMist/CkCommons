using Dalamud.Bindings.ImGui;
using OtterGuiInternal;
using System.Runtime.InteropServices;

namespace CkCommons.Widgets;

/// <summary> Helper for all functions related to drawing the header section of respective UI's </summary>
/// <remarks> Contains functions for icon row display, filters, and more. </remarks>
public class CkHeader
{
    /// <summary> Stores the position of a draw region, and its size. </summary> 
    [StructLayout(LayoutKind.Sequential)]
    public readonly record struct DrawRegion(float PosX, float PosY, float SizeX, float SizeY)
    {
        public Vector2 Pos  => new(PosX, PosY);
        public Vector2 Size => new(SizeX, SizeY);
        public Vector2 Max  => Pos + Size;
        public DrawRegion(Vector2 pos, Vector2 size)
            : this(pos.X, pos.Y, size.X, size.Y)
        { }
    }

    /// <summary> A struct to contain the 4 corner PosSize regions for a CkHeader drawn window. </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly record struct QuadDrawRegions(DrawRegion TopLeft, DrawRegion TopRight, DrawRegion BotLeft, DrawRegion BotRight)
    {
        public Vector2 TopSize => TopRight.Max - TopLeft.Pos;
        public Vector2 BotSize => BotRight.Max - BotLeft.Pos;
    }

    /// <summary> Helper function that draws a flat header title past the window padding to the window edge. </summary>
    public static QuadDrawRegions Flat(uint color, Vector2 innerSize, float leftWidth, float splitWidth)
    {
        var winPadding = ImGui.GetStyle().WindowPadding;
        var window = ImGuiInternal.GetCurrentWindow();
        var innerRect = window.InnerRect;
        var contentRect = window.ContentRegionRect;
        var outerOffset = contentRect.Min - innerRect.Min;

        var innerMinPos = window.InnerRect.Min;
        var innerMaxPos = window.InnerRect.Max;

        window.DrawList.PushClipRect(innerMinPos, innerMaxPos, false);
        var paddedSize = innerSize + winPadding * 2;
        // Draw the base header, and the left region positions.
        window.DrawList.AddRectFilled(innerMinPos, innerMinPos + paddedSize, color, 0, DFlags.None);
        var topLeftPos = contentRect.Min + new Vector2(outerOffset.X, winPadding.Y);
        var botLeftPos = topLeftPos + new Vector2(0, paddedSize.Y);
        var botRegionH = contentRect.Max.Y - botLeftPos.Y - winPadding.Y;

        // define the midpoint positions, and also our right positions after we know the divider.
        var splitPos = botLeftPos + new Vector2(leftWidth + winPadding.X, 0);
        var topRightPos = new Vector2(splitPos.X + splitWidth + winPadding.X, topLeftPos.Y);
        var botRightPos = topRightPos with { Y = botLeftPos.Y };

        window.DrawList.AddRectFilled(splitPos, new Vector2(splitPos.X + splitWidth, contentRect.Max.X - winPadding.Y), CkCol.CurvedHeader.Uint());
        window.DrawList.PopClipRect();

        // we need to return the content region struct, so create our end result content regions below.
        var topLeft = new DrawRegion(topLeftPos, new Vector2(leftWidth, innerSize.Y));
        var botLeft = new DrawRegion(botLeftPos, new Vector2(leftWidth, botRegionH));
        var topRight = new DrawRegion(topRightPos, new Vector2(contentRect.Max.X - topRightPos.X, innerSize.Y));
        var botRight = new DrawRegion(botRightPos, new Vector2(contentRect.Max.X - botRightPos.X, botRegionH));

        // Use for debugging purposes.
        //wdl.AddRect(topLeft.Pos, topLeft.Max, 0xFFFFFFFF);
        //wdl.AddRect(topRight.Pos, topRight.Max, 0xFFFFFFFF);
        //wdl.AddRect(botLeft.Pos, botLeft.Max, 0xFFFFFFFF);
        //wdl.AddRect(botRight.Pos, botRight.Max, 0xFFFFFFFF);

        return new(topLeft, topRight, botLeft, botRight);
    }

    /// <summary> Draws a flat-header beyond window padding with inverted rounded curves at the bottom. </summary>
    public static QuadDrawRegions FlatWithBends(uint color, float height, float splitW, float radius)
    {
        var winPadding = ImGui.GetStyle().WindowPadding;
        var window = ImGuiInternal.GetCurrentWindow();
        var innerRect = window.InnerRect;
        var contentRect = window.ContentRegionRect;
        var outerOffset = contentRect.Min - innerRect.Min;

        var innerMinPos = window.InnerRect.Min;
        var innerMaxPos = window.InnerRect.Max;

        window.DrawList.PushClipRect(innerMinPos, innerMaxPos, false);

        // Get necessary positions.
        var clipOffset = new Vector2(outerOffset.X, winPadding.Y);
        var paddedHeight = height + winPadding.Y * 2;
        var fullWidth = innerRect.Max.X - innerRect.Min.X;
        var contentWidthInner = contentRect.Max.X - contentRect.Min.X;
        var dividerSpace = splitW is 0 ? 0 : splitW + winPadding.X * 2;

        var contentPosTL = contentRect.Min;
        var contentPosBL = contentPosTL + new Vector2(0, paddedHeight);
        var topSizeInner = new Vector2((contentWidthInner - dividerSpace) / 2, height);
        var botSizeInner = new Vector2(topSizeInner.X, contentRect.Max.Y - winPadding.Y - contentPosBL.Y);

        var expandedPosTR = innerMaxPos with { Y = innerMinPos.Y };
        var circleLeftCenter = innerMinPos + new Vector2(radius, paddedHeight + radius);
        var circleRightCenter = expandedPosTR + new Vector2(-radius, paddedHeight + radius);
        var midpoint = fullWidth / 2;

        // Draw the left convex shape.
        window.DrawList.PathClear();
        window.DrawList.PathLineTo(innerMinPos);
        window.DrawList.PathArcTo(circleLeftCenter, radius, float.Pi, 3 * float.Pi / 2);
        window.DrawList.PathLineTo(innerMinPos + new Vector2(midpoint, paddedHeight));
        window.DrawList.PathLineTo(innerMinPos + new Vector2(midpoint, 0));
        window.DrawList.PathFillConvex(color);

        // Draw the right convex shape.
        window.DrawList.PathClear();
        window.DrawList.PathLineTo(expandedPosTR);
        window.DrawList.PathArcTo(circleRightCenter, radius, 2 * float.Pi, 3 * float.Pi / 2);
        window.DrawList.PathLineTo(innerMinPos + new Vector2(midpoint, paddedHeight));
        window.DrawList.PathLineTo(innerMinPos + new Vector2(midpoint, 0));
        window.DrawList.PathFillConvex(color);

        window.DrawList.PopClipRect();

        var topLeft = new DrawRegion(contentPosTL, topSizeInner);
        var topRight = new DrawRegion(topLeft.Pos with { X = topLeft.PosX + topSizeInner.X + dividerSpace }, topSizeInner);
        var botLeft = new DrawRegion(contentPosBL, botSizeInner);
        var botRight = new DrawRegion(topRight.Pos with { Y = botLeft.PosY }, botSizeInner);

        // Use for debugging purposes.
        //wdl.AddRect(topLeft.Pos, topLeft.Max, 0xFFFFFFFF);
        //wdl.AddRect(topRight.Pos, topRight.Max, 0xFFFFFFFF);
        //wdl.AddRect(botLeft.Pos, botLeft.Max, 0xFFFFFFFF);
        //wdl.AddRect(botRight.Pos, botRight.Max, 0xFFFFFFFF);
        //var splitPos = botLeft.Pos + new Vector2(topSizeInner.X + winPadding.X, 0);
        //wdl.AddRect(splitPos, new Vector2(splitPos.X + splitW, expandedMax.Y - winPadding.Y), 0xFF0000FF);

        return new(topLeft, topRight, botLeft, botRight);
    }

    /// <summary>
    ///     A helper function that draws out the fancy curved header (not to be used for restraint sets)
    /// </summary>
    public static QuadDrawRegions FancyCurve(uint col, float leftH, float splitW, float rightW, float curveRadius, bool showSplit = true)
    {
        var winPadding = ImGui.GetStyle().WindowPadding;
        var window = ImGuiInternal.GetCurrentWindow();
        var innerRect = window.InnerRect;
        var contentRect = window.ContentRegionRect;
        var outerOffset = contentRect.Min - innerRect.Min;

        var innerMinPos = window.InnerRect.Min;
        var innerMaxPos = window.InnerRect.Max;

        window.DrawList.PushClipRect(innerMinPos, innerMaxPos, false);

        var fullWidth = innerMaxPos.X - innerMinPos.X;
        var fullWidthInner = contentRect.Max.X - contentRect.Min.X;
        var dividerSpace = splitW is 0 ? 0 : splitW + winPadding.X * 2;
        var topLeftSizeInner = new Vector2(fullWidthInner - rightW - dividerSpace, leftH);
        var topRightSizeInner = new Vector2(rightW, topLeftSizeInner.Y + splitW * 2);

        var paddedLeftSize = topLeftSizeInner + outerOffset * 2;

        window.DrawList.PathClear(); // top right
        window.DrawList.PathLineTo(innerMaxPos with { Y = innerMinPos.Y }); // top left
        window.DrawList.PathLineTo(innerMinPos); // bottom left
        window.DrawList.PathLineTo(innerMinPos + new Vector2(0, paddedLeftSize.Y)); // to first curve

        var contentPosTL = innerMinPos + outerOffset;
        var contentPosBL = contentPosTL + new Vector2(0, paddedLeftSize.Y);
        var contentPosTR = contentPosTL + new Vector2(topLeftSizeInner.X + dividerSpace, 0);
        var contentPosBR = contentPosTR + new Vector2(0, topRightSizeInner.Y + splitW + winPadding.Y);

        var circleOneCenter = innerMinPos + paddedLeftSize + new Vector2(-splitW, splitW);
        var circleTwoCenter = circleOneCenter + new Vector2(splitW * 2, 0);

        // left center curve.
        window.DrawList.PathArcTo(circleOneCenter, splitW, -float.Pi / 2, 0, 16);
        window.DrawList.PathArcTo(circleTwoCenter, splitW, float.Pi, float.Pi / 2, 16);

        // bottom right curve.
        var circleThreeCenter = new Vector2(innerMaxPos.X - curveRadius, circleTwoCenter.Y + splitW + curveRadius);
        window.DrawList.PathArcTo(circleThreeCenter, curveRadius, -float.Pi / 2, 0);
        window.DrawList.PathLineTo(innerMaxPos with { Y = innerMinPos.Y });
        window.DrawList.PathFillConvex(col);

        // if we are not editing, draw the splitter.
        if (showSplit)
        {
            // clear the path.
            window.DrawList.PathClear();
            var circleFourCenter = circleTwoCenter + new Vector2(0, splitW);
            var originPoint = new Vector2(circleOneCenter.X + splitW, innerMaxPos.Y - winPadding.Y);
            // bottom left
            window.DrawList.PathLineTo(originPoint);
            window.DrawList.PathArcTo(circleFourCenter, splitW, float.Pi, float.Pi / 2);
            // bottom right
            window.DrawList.PathLineTo(originPoint + new Vector2(splitW, 0));
            window.DrawList.PathFillConvex(col);
        }

        window.DrawList.PopClipRect();

        var topLeft = new DrawRegion(contentPosTL, topLeftSizeInner);
        var botLeft = new DrawRegion(contentPosBL, new Vector2(topLeftSizeInner.X, contentRect.Max.Y - contentPosBL.Y));
        var topRight = new DrawRegion(contentPosTR, topRightSizeInner);
        var botRight = new DrawRegion(contentPosBR, new Vector2(topRightSizeInner.X, contentRect.Max.Y - contentPosBR.Y));

        // Use for debugging purposes.
        //wdl.AddRect(topLeft.Pos, topLeft.Max, 0x77FFFFFF);
        //wdl.AddRect(topRight.Pos, topRight.Max, 0x77FFFFFF);
        //wdl.AddRect(botLeft.Pos, botLeft.Max, 0x77FFFFFF);
        //wdl.AddRect(botRight.Pos, botRight.Max, 0x77FFFFFF);
        //var splitPos = contentPosTR - new Vector2(winPadding.X + splitW, 0);
        //wdl.AddRect(splitPos, new Vector2(splitPos.X + splitW, expandedMax.Y - winPadding.Y), 0x770000FF);

        return new(topLeft, topRight, botLeft, botRight);
    }
}
