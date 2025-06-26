using Dalamud.Plugin.Services;
using System.Runtime.CompilerServices;
using System;
using CkCommons.Services;

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
            if (!suppressErrors) Svc.Log.Error($"{e.Message}\n{e.StackTrace ?? ""}");
        }
    }
}