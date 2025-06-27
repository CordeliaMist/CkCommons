using Dalamud.Game;
using Dalamud.Game.ClientState.Objects;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using System;
using System.Diagnostics.CodeAnalysis;

namespace CkCommons.Services;
#nullable disable

/// <summary>
///     A collection of internally handled Dalamud Interface static services
/// </summary>
/// <remarks> Keep in mind that interfaces are a performance loss. If you can get the data via FFXIVCLientStructs, do so. </remarks>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
public class Svc
{
    [PluginService] public static IDalamudPluginInterface PluginInterface { get; set; } = null!;
    [PluginService] public static IPluginLog Log { get; set; } = null!;
    [PluginService] public static IAddonLifecycle AddonLifecycle { get; set; } = null!;
    [PluginService] public static IAddonEventManager AddonEventManager { get; private set; }
    [PluginService] public static IAetheryteList AetheryteList { get; private set; }
    [PluginService] public static IChatGui Chat { get; set; } = null!;
    [PluginService] public static IClientState ClientState { get; set; } = null!;
    [PluginService] public static ICommandManager Commands { get; private set; }
    [PluginService] public static ICondition Condition { get; private set; }
    [PluginService] public static IContextMenu ContextMenu { get; private set; }
    [PluginService] public static IDataManager Data { get; private set; } = null!;
    [PluginService] public static IDtrBar DtrBar { get; private set; } = null!;
    [PluginService] public static IDutyState DutyState { get; private set; } = null!;
    [PluginService] public static IFramework Framework { get; private set; } = null!;
    [PluginService] public static IGameGui GameGui { get; private set; } = null!;
    [PluginService] public static IGameInteropProvider Hook { get; private set; } = null!;
    [PluginService] public static IGameConfig GameConfig { get; private set; } = null!;
    [PluginService] public static IGameLifecycle GameLifeCycle { get; private set; } = null!;
    [PluginService] public static IGamepadState GamepadState { get; private set; } = null!;
    [PluginService] public static IKeyState KeyState { get; private set; } = null!;
    [PluginService] public static INotificationManager Notifications { get; private set; } = null!;
    [PluginService] public static IObjectTable Objects { get; private set; } = null!;
    [PluginService] public static IPartyList Party { get; private set; } = null!;
    [PluginService] public static ISigScanner SigScanner { get; private set; } = null!;
    [PluginService] public static ITargetManager Targets { get; private set; } = null!;
    [PluginService] public static ITextureProvider Texture { get; private set; } = null!;
    [PluginService] public static IToastGui Toasts { get; private set; } = null!;

    // if initialization was successful. Only allow once!
    internal static bool IsInitialized = false;
    public static void Init(IDalamudPluginInterface pi)
    {
        if (IsInitialized)
            return;

        IsInitialized = true;
        try
        {
            pi.Create<Svc>();
        }
        catch(Exception ex)
        {
            Log.Fatal($"Failed to initialize Svc: {ex.Message}\n{ex.StackTrace ?? ""}");
        }
    }
}