using CkCommons;
using SixLabors.ImageSharp;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CkCommons;
public static class Generic
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInRange<T>(this int idx, IReadOnlyCollection<T> collection)
        => (uint)idx < (uint)collection.Count;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Safe(Action a, bool suppressErrors = false)
    {
        try
        {
            a();
        }
        catch (TaskCanceledException tce)
        {
            if (!suppressErrors) Svc.Log.Warning($"Task was cancelled: {tce.Message}");
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
        catch (TaskCanceledException tce)
        {
            if (!suppressErrors) Svc.Log.Warning($"Task was cancelled: {tce.Message}");
            return default;
        }
        catch (Exception e)
        {
            if (!suppressErrors) Svc.Log.Error($"{e.Message}\n{e.StackTrace ?? ""}");
            return default;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task Safe(Func<Task> a, bool suppressErrors = false)
    {
        try
        {
            await a();
        }
        catch (TaskCanceledException tce)
        {
            if (!suppressErrors) Svc.Log.Warning($"Task was cancelled: {tce.Message}");
        }
        catch (Exception e)
        {
            if (!suppressErrors)
                Svc.Log.Error($"{e.Message}\n{e.StackTrace ?? ""}");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<T?> Safe<T>(Func<Task<T>> a, bool suppressErrors = false)
    {
        try
        {
            return await a();
        }
        catch (TaskCanceledException tce)
        {
            if (!suppressErrors) Svc.Log.Warning($"Task was cancelled: {tce.Message}");
            return default;
        }
        catch (Exception e)
        {
            if (!suppressErrors) Svc.Log.Error($"{e.Message}\n{e.StackTrace ?? ""}");
            return default;
        }
    }

    /// <summary> Consumes any ObjectDisposedExceptions when trying to cancel a token source. </summary>
    /// <remarks> Only use if you know what you are doing and need to consume the error. </remarks>
    public static void SafeCancel(this CancellationTokenSource? cts)
    {
        try
        {
            cts?.Cancel();
        }
        catch (ObjectDisposedException) { /* CONSUME THE VOID */ }
    }

    /// <summary> Consumes any ObjectDisposedExceptions when trying to dispose a token source. </summary>
    /// <remarks> Only use if you know what you are doing and need to consume the error. </remarks>
    public static void SafeDispose(this CancellationTokenSource? cts)
    {
        try
        {
            cts?.Dispose();
        }
        catch (ObjectDisposedException) { /* CONSUME THE VOID */ }
    }

    /// <summary> Consumes any ObjectDisposedExceptions when trying to cancel and dispose a token source. </summary>
    /// <remarks> Only use if you know what you are doing and need to consume the error. </remarks>
    public static void SafeCancelDispose(this CancellationTokenSource? cts)
    {
        try
        {
            cts?.Cancel();
            cts?.Dispose();
        }
        catch (ObjectDisposedException) { /* CONSUME THE VOID */ }
    }

    /// <summary> Consumes any ObjectDisposedExceptions when trying to recreate a token source. </summary>
    /// <remarks> Only use if you know what you are doing and need to consume the error. </remarks>
    public static CancellationTokenSource SafeCancelRecreate(this CancellationTokenSource? cts)
    {
        cts?.SafeCancelDispose();
        return new CancellationTokenSource();
    }

    /// <summary> Consumes any ObjectDisposedExceptions when trying to recreate a token source. </summary>
    /// <remarks> Only use if you know what you are doing and need to consume the error. </remarks>
    public static void SafeCancelRecreate(ref CancellationTokenSource? cts)
    {
        cts?.SafeCancelDispose();
        cts = new CancellationTokenSource();
    }
}
