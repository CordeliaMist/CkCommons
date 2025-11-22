using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CkCommons.Helpers;

/// <summary> A class for all of the UI helpers, including basic functions for drawing repetative yet unique design elements </summary>
public static partial class KeyMonitor
{
    [LibraryImport("user32")]
    internal static partial short GetAsyncKeyState(int nVirtKey);

    /// <summary>
    /// Checks to see if a key is pressed
    /// </summary>
    public static bool IsKeyPressed(int vKey)
    {
        // if it isnt any key just return false
        if (vKey is 0)
            return false;

        return IsBitSet(GetAsyncKeyState(vKey), 15);
    }

    // see if the key bit is set
    public static bool IsBitSet(short b, int pos) => (b & (1 << pos)) != 0;

    /// <summary> 
    ///     Use ImGui.GetIO().KeyShift if detecting inside the UI draw frame.
    ///     This performs additional logic which can add up if called several times in a drawframe.
    /// </summary>
    public static bool ShiftPressed() => (IsKeyPressed(0xA1) || IsKeyPressed(0xA0));
    /// <summary> 
    ///     Use ImGui.GetIO().KeyCtrl if detecting inside the UI draw frame.
    ///     This performs additional logic which can add up if called several times in a drawframe.
    /// </summary>
    public static bool CtrlPressed() => (IsKeyPressed(0xA2) || IsKeyPressed(0xA3));
    /// <summary> 
    ///     Use ImGui.GetIO().KeyAlt if detecting inside the UI draw frame.
    ///     This performs additional logic which can add up if called several times in a drawframe.
    /// </summary>
    public static bool AltPressed() => (IsKeyPressed(0xA4) || IsKeyPressed(0xA5));
    public static bool BackPressed() => IsKeyPressed(0x08);
    public static bool TabPressed() => IsKeyPressed(0x09);
    public static bool Numpad0Pressed() => IsKeyPressed(0x60);

    public static bool RightMouseButtonDown() => IsKeyPressed(0x02);
    public static bool MiddleMouseButtonDown() => IsKeyPressed(0x04);
    public static bool IsBothMouseButtonsPressed() => IsKeyPressed((int)Keys.LButton) && IsKeyPressed((int)Keys.RButton);

}
