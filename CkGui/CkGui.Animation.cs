namespace CkCommons.Gui;

public static partial class CkGui
{
    public static void AnimatedHourglass(int milliseconds, Vector4? color)
    {
        // Divide the interval into 3 equal segments
        var tick = Environment.TickCount64 % milliseconds;
        var segment = milliseconds / 3;
        int index = (int)(tick / segment);
        // Display the appropriate icon
        switch (index)
        {
            case 0:
                FramedIconText(FAI.HourglassStart, color);
                break;
            case 1:
                FramedIconText(FAI.HourglassHalf, color);
                break;
            default:
                FramedIconText(FAI.HourglassEnd, color);
                break;
        }
    }

    public static void AnimatedHourglass(int milliseconds, uint? color = null)
    {
        // Divide the interval into 3 equal segments
        var tick = Environment.TickCount64 % milliseconds;
        var segment = milliseconds / 3;
        int index = (int)(tick / segment);
        // Display the appropriate icon
        switch (index)
        {
            case 0:
                FramedIconText(FAI.HourglassStart, color);
                break;
            case 1:
                FramedIconText(FAI.HourglassHalf, color);
                break;
            default:
                FramedIconText(FAI.HourglassEnd, color);
                break;
        }
    }
}
