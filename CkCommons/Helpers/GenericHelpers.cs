using Dalamud.Plugin.Services;
using System.Runtime.CompilerServices;
using System;

namespace CkCommons;

public static class GenericHelpers
{
    // #nullable disable
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Safe(Action a, bool suppressErrors = false)
    {
        try
        {
            a();
        }
        catch (Exception e)
        {
            if (!suppressErrors) PluginLog.Error($"{e.Message}\n{e.StackTrace ?? ""}");
        }
    }
}