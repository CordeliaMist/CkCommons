using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;
using System.Runtime.CompilerServices;

namespace CkCommons;
public static unsafe class AddonHelp
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAddonReady(AtkUnitBase* addon)
        => addon->IsVisible && addon->UldManager.LoadedState == AtkLoadState.Loaded && addon->IsFullyLoaded();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsReady(this AtkUnitBase addon)
        => addon.IsVisible && addon.UldManager.LoadedState == AtkLoadState.Loaded && addon.IsFullyLoaded();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsComponentReady(AtkComponentNode* addon)
        => addon->AtkResNode.IsVisible() && addon->Component->UldManager.LoadedState == AtkLoadState.Loaded;

    /// <summary>
    ///     Obtain the unit base of an AddonArgs instance.
    /// </summary>
    public static AtkUnitBase* Base(this AddonArgs args) 
        => (AtkUnitBase*)args.Addon.Address;

    /// <summary>
    ///     Obtain an addon* by its name alone. If it is not found, returns false.
    /// </summary>
    public static bool TryGetAddonByName<T>(string addon, out T* addonPtr) where T : unmanaged
    {
        // we can use a more direct approach now that we have access to static classes.
        var a = Svc.GameGui.GetAddonByName(addon, 1);
        if (a == IntPtr.Zero)
        {
            addonPtr = null;
            return false;
        }
        else
        {
            addonPtr = (T*)a.Address;
            return true;
        }
    }

    /// <summary>
    ///     Avoid constructing a list and instead allocate a single array, copying a trimmed version after. <para />
    ///     Helps safe possibility of passing the nodes list twice completely for all cases when possible. <para />
    ///     Can be reversed.
    /// </summary>
    public static AtkResNode*[] GetNodeIconArray(AtkResNode* node, bool reverse = false)
    {
        var atk = node->GetAsAtkComponentNode();
        if (atk == null)
            return [];

        var uldm = atk->Component->UldManager;
        int count = uldm.NodeListCount;
        if (count == 0)
            return [];

        // Create a temp allocated array to fill the data
        var tmp = new AtkResNode*[count];
        var written = 0;

        for (var i = 0; i < uldm.NodeListCount; i++)
        {
            var next = uldm.NodeList[i];
            if (next is null || (int)next->Type < 1000)
                continue;

            var compNode = next->GetAsAtkComponentNode();
            if (compNode is null)
                continue;

            var info = (AtkUldComponentInfo*)compNode->Component->UldManager.Objects;
            if (info->ComponentType != ComponentType.IconText)
                continue;

            tmp[written++] = next;
        }

        // If we didnt write anything ret empty
        if (written is 0)
            return [];

        // If reversed, construct the return array in reverse order
        if (reverse)
        {
            var ret = new AtkResNode*[written];
            for (var i = 0; i < written; i++)
                ret[i] = tmp[written - 1 - i];
            return ret;
        }

        // If it was the same as the count return as is.
        if (written == count)
            return tmp;

        // Otherwise copy the trimmed version over to be what we return
        var trimmed = new AtkResNode*[written];
        Array.Copy(tmp, trimmed, written);
        return trimmed;
    }

    public static string GetVfxPathByID(uint iconID)
    {
        foreach (var x in Svc.Data.GetExcelSheet<Status>())
        {
            if (x.Icon == iconID)
                return x.HitEffect.ValueNullable?.Location.ValueNullable?.Location.ExtractText() ?? string.Empty;

            if (x.MaxStacks > 1 && iconID >= x.Icon + 1 && iconID < x.Icon + x.MaxStacks)
                return x.HitEffect.ValueNullable?.Location.ValueNullable?.Location.ExtractText() ?? string.Empty;
        }
        return string.Empty;
    }

    /// <summary>
    ///     Get a VFX related path from a given .avfx name.
    /// </summary>
    /// <remarks> Can return an empty string if not found. </remarks>
    public static string GetVfxPath(string path)
        => string.IsNullOrEmpty(path) ? string.Empty : $"vfx/common/eff/{path}.avfx";

}
