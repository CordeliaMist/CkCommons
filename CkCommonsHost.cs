using CkCommons.RichText;
using CkCommons.Services;
using CkCommons.Textures;
using Dalamud.Plugin;
using Serilog.Events;
using System.Reflection;

namespace CkCommons;
#nullable disable

/// <summary>
///     The main hoster for CkCommons. <see cref="Init(IDalamudPluginInterface, IDalamudPlugin)"/> must
///     be called during your <see cref="IDalamudPlugin"/> entry point. <para/>
///     
///     Likewise, it's <see cref="Dispose"/> should be called once <see cref="IDalamudPlugin"/> falls
///     out of plugin scope.
/// </summary>
public static class CkCommonsHost
{
    public static IDalamudPlugin Instance = null;
    public static bool Disposed { get; private set; } = false;
    
    /// <summary>
    ///     CkCommons sections that use <see cref="IDalamudPlugin"/> accessors WON'T WORK calling this on plugin entry.
    /// </summary>
    public static void Init(IDalamudPluginInterface pluginInterface, IDalamudPlugin instance)
    {
        Instance = instance;
        Svc.Init(pluginInterface);

        CkRichText.Init();

        Svc.Log.MinimumLogLevel = LogEventLevel.Debug;
        Svc.Log.Information($"This is ECommons v{typeof(CkCommonsHost).Assembly.GetName().Version} " +
            $"and {Svc.PluginInterface.InternalName} v{instance.GetType().Assembly.GetName().Version}.");

        // Nothing to really initialize yet.
    }

    public static void CheckForObfuscation()
    {
        if (Assembly.GetCallingAssembly().GetTypes().FirstOrDefault(x => x.IsAssignableTo(typeof(IDalamudPlugin)))?.Name == Svc.PluginInterface.InternalName)
            Svc.Log.Fatal($"{Svc.PluginInterface.InternalName} name match error!");
    }

    /// <summary>
    ///   Disposes all CkCommons services and cleans up resources. <para/>
    ///   You are required to call this to free up resources!
    /// </summary>
    public static void Dispose()
    {
        Disposed = true;

        // Any classes that initialize, have an initializer, store data that should be replaced, or do not use IDisposable, should be manually disposed.
        CoreTextureManager.Dispose();
        CkRichText.Dispose();
    }
}