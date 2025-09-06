using Dalamud.Bindings.ImGui;
using OtterGui.Text.EndObjects;
using System.Runtime.InteropServices;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.GroupPoseModule;

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
        var wdl = ImGui.GetWindowDrawList();
        var winPadding = ImGui.GetStyle().WindowPadding;
        var winClipX = ImGui.GetWindowContentRegionMin().X / 2;
        var minPos = wdl.GetClipRectMin();
        var maxPos = wdl.GetClipRectMax();

        var outerXOffset = Math.Abs(winClipX - winPadding.X);
        var paddedSize = innerSize + winPadding * 2;
        var expandedMin = minPos - new Vector2(winClipX, 0); // Extend the min boundary to include the padding
        var expandedMax = maxPos + new Vector2(winClipX, 0); // Extend the max boundary to include the padding
        wdl.PushClipRect(expandedMin, expandedMax, false);

        // Draw the base header, and the left region positions.
        wdl.AddRectFilled(expandedMin, expandedMin + paddedSize, color, 0, DFlags.None);
        var topLeftPos = minPos + new Vector2(outerXOffset, winPadding.Y);
        var botLeftPos = topLeftPos + new Vector2(0, paddedSize.Y);
        var botRegionH = maxPos.Y - botLeftPos.Y - winPadding.Y;

        // define the midpoint positions, and also our right positions after we know the divider.
        var splitPos = botLeftPos + new Vector2(leftWidth + winPadding.X, 0);
        var topRightPos = new Vector2(splitPos.X + splitWidth + winPadding.X, topLeftPos.Y);
        var botRightPos = topRightPos with { Y = botLeftPos.Y };

        wdl.AddRectFilled(splitPos, new Vector2(splitPos.X + splitWidth, maxPos.Y - winPadding.Y), CkColor.FancyHeader.Uint());
        wdl.PopClipRect();

        // we need to return the content region struct, so create our end result content regions below.
        var topLeft = new DrawRegion(topLeftPos, new Vector2(leftWidth, innerSize.Y));
        var botLeft = new DrawRegion(botLeftPos, new Vector2(leftWidth, botRegionH));
        var topRight = new DrawRegion(topRightPos, new Vector2(maxPos.X - outerXOffset - topRightPos.X, innerSize.Y));
        var botRight = new DrawRegion(botRightPos, new Vector2(maxPos.X - outerXOffset - botRightPos.X, botRegionH));

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
        var wdl = ImGui.GetWindowDrawList();
        var winPadding = ImGui.GetStyle().WindowPadding;
        var winClipX = ImGui.GetWindowContentRegionMin().X / 2;
        var minPos = wdl.GetClipRectMin();
        var maxPos = wdl.GetClipRectMax();
        var outerXOffset = Math.Abs(winClipX - winPadding.X);
        
        var expandedMin = minPos - new Vector2(winClipX, 0); // Extend the min boundary to include the padding
        var expandedMax = maxPos + new Vector2(winClipX, 0); // Extend the max boundary to include the padding
        wdl.PushClipRect(expandedMin, expandedMax, false);

        // Get necessary positions.
        var clipOffset = new Vector2(outerXOffset, winPadding.Y);
        var paddedHeight = height + winPadding.Y * 2;
        var fullWidth = maxPos.X - minPos.X;
        var contentWidthInner = fullWidth - clipOffset.X * 2;
        var dividerSpace = splitW is 0 ? 0 : splitW + winPadding.X * 2;

        var contentPosTL = minPos + clipOffset;
        var contentPosBL = contentPosTL + new Vector2(0, paddedHeight);
        var topSizeInner = new Vector2((contentWidthInner - dividerSpace) / 2, height);
        var botSizeInner = new Vector2(topSizeInner.X, maxPos.Y - winPadding.Y - contentPosBL.Y);

        var expandedPosTR = expandedMax with { Y = expandedMin.Y };
        var circleLeftCenter = expandedMin + new Vector2(radius, paddedHeight + radius);
        var circleRightCenter = expandedPosTR + new Vector2(-radius, paddedHeight + radius);
        var midpoint = fullWidth / 2;

        // Draw the left convex shape.
        wdl.PathClear();
        wdl.PathLineTo(expandedMin);
        wdl.PathArcTo(circleLeftCenter, radius, float.Pi, 3 * float.Pi / 2);
        wdl.PathLineTo(expandedMin + new Vector2(midpoint, paddedHeight));
        wdl.PathLineTo(expandedMin + new Vector2(midpoint, 0));
        wdl.PathFillConvex(color);

        // Draw the right convex shape.
        wdl.PathClear();
        wdl.PathLineTo(expandedPosTR);
        wdl.PathArcTo(circleRightCenter, radius, 2 * float.Pi, 3 * float.Pi / 2);
        wdl.PathLineTo(expandedMin + new Vector2(midpoint, paddedHeight));
        wdl.PathLineTo(expandedMin + new Vector2(midpoint, 0));
        wdl.PathFillConvex(color);

        wdl.PopClipRect();

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


    /// <summary> A helper function that draws out the fancy curved header (not to be used for restraint sets) </summary>
    public static QuadDrawRegions FancyCurve(uint col, float leftH, float splitW, float rightW, float curveRadius, bool showSplit = true)
    {
        // Grab the window padding that is currently set.
        var wdl = ImGui.GetWindowDrawList();
        var winPadding = ImGui.GetStyle().WindowPadding;
        var winClipX = ImGui.GetWindowContentRegionMin().X / 2;
        var minPos = wdl.GetClipRectMin();
        var maxPos = wdl.GetClipRectMax();
        var outerXOffset = Math.Abs(winClipX - winPadding.X);

        var fullWidth = maxPos.X - minPos.X;
        var fullWidthInner = fullWidth - outerXOffset * 2;
        var dividerSpace = splitW is 0 ? 0 : splitW + winPadding.X * 2;
        var topLeftSizeInner = new Vector2(fullWidthInner - rightW - dividerSpace, leftH);
        var topRightSizeInner = new Vector2(rightW, topLeftSizeInner.Y + splitW * 2);

        var paddedLeftSize = topLeftSizeInner + winPadding * 2;
        var clippedOffset = new Vector2(outerXOffset, winPadding.Y);

        // Extend the min boundary to include the padding
        var expandedMin = minPos - new Vector2(winPadding.X / 2, 0);
        var expandedMax = maxPos + new Vector2(winPadding.X / 2, 0);
        wdl.PushClipRect(expandedMin, expandedMax, false);

        wdl.PathClear(); // top right
        wdl.PathLineTo(expandedMax with { Y = expandedMin.Y }); // top left
        wdl.PathLineTo(expandedMin); // bottom left
        wdl.PathLineTo(expandedMin + new Vector2(0, paddedLeftSize.Y)); // to first curve

        var contentPosTL = minPos + clippedOffset;
        var contentPosBL = contentPosTL + new Vector2(0, paddedLeftSize.Y);
        var contentPosTR = contentPosTL + new Vector2(topLeftSizeInner.X + dividerSpace, 0);
        var contentPosBR = contentPosTR + new Vector2(0, topRightSizeInner.Y + splitW + winPadding.Y);

        var circleOneCenter = expandedMin + paddedLeftSize + new Vector2(-splitW, splitW);
        var circleTwoCenter = circleOneCenter + new Vector2(splitW * 2, 0);

        // left center curve.
        wdl.PathArcTo(circleOneCenter, splitW, -float.Pi / 2, 0, 16);
        wdl.PathArcTo(circleTwoCenter, splitW, float.Pi, float.Pi / 2, 16);

        // bottom right curve.
        var circleThreeCenter = new Vector2(expandedMax.X - curveRadius, circleTwoCenter.Y + splitW + curveRadius);
        wdl.PathArcTo(circleThreeCenter, curveRadius, -float.Pi / 2, 0);
        wdl.PathLineTo(expandedMax with { Y = expandedMin.Y });
        wdl.PathFillConvex(col);

        // if we are not editing, draw the splitter.
        if (showSplit)
        {
            // clear the path.
            wdl.PathClear();
            var circleFourCenter = circleTwoCenter + new Vector2(0, splitW);
            var originPoint = new Vector2(circleOneCenter.X + splitW, expandedMax.Y - winPadding.Y);
            // bottom left
            wdl.PathLineTo(originPoint);
            wdl.PathArcTo(circleFourCenter, splitW, float.Pi, float.Pi / 2);
            // bottom right
            wdl.PathLineTo(originPoint + new Vector2(splitW, 0));
            wdl.PathFillConvex(col);
        }

        wdl.PopClipRect();

        var topLeft = new DrawRegion(contentPosTL, topLeftSizeInner);
        var botLeft = new DrawRegion(contentPosBL, new Vector2(topLeftSizeInner.X, maxPos.Y - winPadding.Y - contentPosBL.Y));
        var topRight = new DrawRegion(contentPosTR, topRightSizeInner);
        var botRight = new DrawRegion(contentPosBR, new Vector2(topRightSizeInner.X, maxPos.Y - winPadding.Y - contentPosBR.Y));

        // Use for debugging purposes.
        //wdl.AddRect(topLeft.Pos, topLeft.Max, 0x77FFFFFF);
        //wdl.AddRect(topRight.Pos, topRight.Max, 0x77FFFFFF);
        //wdl.AddRect(botLeft.Pos, botLeft.Max, 0x77FFFFFF);
        //wdl.AddRect(botRight.Pos, botRight.Max, 0x77FFFFFF);
        //var splitPos = contentPosTR - new Vector2(winPadding.X + splitW, 0);
        //wdl.AddRect(splitPos, new Vector2(splitPos.X + splitW, expandedMax.Y - winPadding.Y), 0x770000FF);

        return new(topLeft, topRight, botLeft, botRight);
    }

    /// <summary> A helper function that draws out the fancy curved header (not to be used for restraint sets) </summary>
    public static QuadDrawRegions FancyCurveFlipped(uint col, float iconBarWidth, float splitWidth, float searchHeight, bool showSplit = true)
    {
        // Grab the window padding that is currently set.
        var wdl = ImGui.GetWindowDrawList();
        var winPadding = ImGui.GetStyle().WindowPadding;
        var winClipX = ImGui.GetWindowContentRegionMin().X / 2;
        var minPos = wdl.GetClipRectMin();
        var maxPos = wdl.GetClipRectMax();
        var size = maxPos - minPos;
        var outerXOffset = Math.Abs(winClipX - winPadding.X);

        var iconBarSizeInner = new Vector2(iconBarWidth, searchHeight + splitWidth);
        var searchSizeInner = new Vector2((maxPos.X - minPos.X) - iconBarWidth - splitWidth, searchHeight);
        var paddedSearchSize = searchSizeInner + winPadding * 2;

        var curveRadius = splitWidth / 2;
        var clippedOffset = new Vector2(outerXOffset, winPadding.Y);

        var expandedMin = minPos - new Vector2(winPadding.X / 2, 0); // Extend the min boundary to include the padding
        var expandedMax = maxPos + new Vector2(winPadding.X / 2, 0); // Extend the max boundary to include the padding
        wdl.PushClipRect(expandedMin, expandedMax, false);

        var topRightContentPos = minPos + new Vector2(size.X -clippedOffset.X - searchSizeInner.X, clippedOffset.Y);
        var botRightContentPos = topRightContentPos + new Vector2(0, paddedSearchSize.Y);
        var topLeftContentPos = minPos + clippedOffset;
        var botLeftContentPos = topLeftContentPos + new Vector2(0, searchSizeInner.Y + splitWidth + winPadding.Y + curveRadius);

        var drawRegionTR = new DrawRegion(topRightContentPos, searchSizeInner);
        var drawRegionBR = new DrawRegion(botRightContentPos, maxPos - clippedOffset - botRightContentPos);
        var drawRegionTL = new DrawRegion(topLeftContentPos, iconBarSizeInner);
        var drawRegionBL = new DrawRegion(botLeftContentPos, new Vector2(iconBarSizeInner.X, maxPos.Y - clippedOffset.Y - botLeftContentPos.Y));

        wdl.PathClear();

        // top left
        wdl.PathLineTo(expandedMin);

        // top right.
        var topRightPos = expandedMax with { Y = expandedMin.Y };
        wdl.PathLineTo(topRightPos);

        // bot right.
        var rightSideMaxPos = topRightPos + new Vector2(0, paddedSearchSize.Y);
        wdl.PathLineTo(rightSideMaxPos);

        // the center curves.
        var circleOneCenter = rightSideMaxPos + new Vector2(-(paddedSearchSize.X - curveRadius), curveRadius);
        var circleTwoCenter = circleOneCenter - new Vector2(splitWidth, 0);
        wdl.PathArcTo(circleOneCenter, curveRadius, 3 * float.Pi / 2, float.Pi, 16);
        wdl.PathArcTo(circleTwoCenter, curveRadius, 0, float.Pi / 2, 16);


        var circleThreeCenter = expandedMin + new Vector2(splitWidth, paddedSearchSize.Y + splitWidth * 2);
        wdl.PathArcTo(circleThreeCenter, splitWidth, 3 * float.Pi / 2, float.Pi);
        wdl.PathLineTo(expandedMin);
        wdl.PathFillConvex(col);

        // if we are not editing, draw the splitter.
        if (showSplit)
        {
            // clear the path.
            wdl.PathClear();
            var circleFourCenter = circleTwoCenter + new Vector2(0, curveRadius);
            var originPoint = new Vector2(circleFourCenter.X + curveRadius, expandedMax.Y - winPadding.Y);
            // bottom left
            wdl.PathLineTo(originPoint);
            wdl.PathArcTo(circleFourCenter, curveRadius, 0, float.Pi / 2, 16);
            // bottom right
            wdl.PathLineTo(originPoint - new Vector2(curveRadius, 0));
            wdl.PathFillConvex(col);
        }

        wdl.PopClipRect();

        // we need to return the content region struct, so create our end result content regions below.
        return new(drawRegionTL, drawRegionTR, drawRegionBL, drawRegionBR);
    }
}
