using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using OtterGui.Text;

namespace CkCommons.Gui;

// Partial Class for BitMask assistance.
public static partial class CkGui
{
    // Helper Methods for controlling bitmasking fields within drawmethods and edits.
    public static bool BitmaskEditCombo<T>(string id, string preview, float width, T current, ref T changes, Func<int, string>? toName = null, CFlags flags = CFlags.None) where T : unmanaged, Enum
    {
        var updated = false;

        // get the underlying value of the enum
        var enumType = typeof(T);
        var underlying = Enum.GetUnderlyingType(enumType);
        var maxBits = GetMaxBits(underlying);

        var curBitMask = Convert.ToInt32(current);
        var changeBitMask = Convert.ToInt32(changes);

        using var _ = ImRaii.PushId(id);
        ImGui.SetNextItemWidth(width);
        using var combo = ImUtf8.Combo(""u8, preview, flags);
        if (combo)
        {
            // Draw out all of the flags.
            for (var i = 0; i < maxBits; i++)
            {
                // shift by [i] bits to get the corrisponding flag.
                var flag = 1 << i;
                // get attributes about this flag from the current and changed, to know its state in each.
                var isSet = (changeBitMask & flag) != 0;
                var wasSet = (curBitMask & flag) != 0;

                // set the color if the flag is a changed flag.
                using var col = ImRaii.PushColor(ImGuiCol.Text, CkColor.TriStateCheck.Uint(), isSet != wasSet);
                // get the preview.
                var name = toName?.Invoke(flag) ?? $"Flag {i}";
                var temp = isSet;
                if (ImGui.Checkbox($"{name}##{id}_{i}", ref temp))
                {
                    if (temp)
                        changeBitMask |= flag;
                    else
                        changeBitMask &= ~flag;
                    updated = true;
                }
            }
            // update the changes if any occured.
            changes = (T)Enum.ToObject(enumType, changeBitMask);
        }
        return updated;
    }


    public static int GetMaxBits(Type underlying) 
        => Type.GetTypeCode(underlying) switch
        {
            TypeCode.Byte => 8,
            TypeCode.SByte => 8,
            TypeCode.Int16 => 16,
            TypeCode.UInt16 => 16,
            TypeCode.Int32 => 32,
            TypeCode.UInt32 => 32,
            TypeCode.Int64 => 64,
            TypeCode.UInt64 => 64,
            _ => throw new NotSupportedException($"Unsupported enum base type: {underlying}")
        };
}
