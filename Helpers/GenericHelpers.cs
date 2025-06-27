using Dalamud.Plugin.Services;
using System.Runtime.CompilerServices;
using System;
using CkCommons.Services;
using System.Threading.Tasks;

namespace CkCommons;

public static class Generic
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? Safe<T>(Func<T> a, bool suppressErrors = false)
    {
        try
        {
            return a();
        }
        catch (Exception e)
        {
            if (!suppressErrors) Svc.Log.Error($"{e.Message}\n{e.StackTrace ?? ""}");
            return default;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<T?> Safe<T>(Func<Task<T>> a, bool suppressErrors = false)
    {
        try
        {
            return await a();
        }
        catch (Exception e)
        {
            if (!suppressErrors) Svc.Log.Error($"{e.Message}\n{e.StackTrace ?? ""}");
            return default;
        }
    }
}
