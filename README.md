<section id="about">
<a href="#about" alt="About"><h1>What Is CkCommons?</h1></a>
  <p>
     CkCommons is a library designed to work with Dalamud Projects. CkCommons helps provide classes, widgets, methods, 
     and shortcuts to help reduce boiler plate, and add QoL paramater customizations. 
  </p>
  <p>
     In addition it contains a fleshed out ImGui wrapper CkGui, that beautifies ImGui with extra customization without hindering drawtime performance.
  </p>
</section>

<section id="adding-as-submodule">
<a href="#adding-as-submodule" alt="Adding CkCommons"><h2>Adding CkCommons</h2></a>
To add CkCommons to your plugin's project, use the following command:

```
git submodule add https://github.com/CordeliaMist/CkCommons.git
```
If this is not referenced via a submodule, add it in your plugin's CSProj file like so:

```  
<ItemGroup>
    <ProjectReference Include="..\CkCommons\CkCommons.csproj" />
</ItemGroup>
```

To use any methods that make use of Dalamud Plugin services (hooks, textures, data, ext.), 
You must handle submodule initialization and disposal.

Please note that CkCommons does not support dependancy Injection for startup and should not be added as a Singleton.
Instead, Initialize CkCommons in your IDalamudPlugin's entry point like so:
```
CkCommonsMain.Init(pluginInterface, this);
```
In the case above, `pluginInterface` is the `IDalamudPluginInterface` passed in with your `IDalamudPlugin`'s instance.

<b>Upon plugin disposal (or runtimeServiceScope disposal if using DI)</b>, call CkCommons disposal method:
```
CkCommonsMain.DisposeAll();
```

# Features
> CkCommons is an all-in-one toolkit that provides helpers across the board to provide Utility. 

## CkGui, CkGuiEx, CkGuiUtils
A Wrapper over ImGui and ImGuiNET that helps beautify ImGui elements.
CkGui also provides extentions for ImGui methods and DrawList methods to help with QoL and boilerplate reduction.

## CkRaii
Similar to ImRaii from OtterGui, CkRaii helps apply the beautify aspect of its ImGui wrapper methods to ImRaii methods.
CkRaii also adds its own Raii methods not included in ImRaii, and its own common CkStyle public accessors.

## CkStyle
Customized Style format used by many of Cordelia's Plugin projects and also reflect many of the references used by CkGui 
and CkRaii method defaults.

## CkRichText
CkRichText helps provide the beloved format of Dalamuds SeStrings for colortext, glow, icons, images, map links, and creates
a way to display these through ImGui with excessive optimization that allows for display with neglegable drawtime impact.

## Classes
- OptionalBoolCheckbox (overload from OtterGui with more customization)
- OptionalBoolIconCheckbox (overload of OptionalBoolCheckbox with additional info)
- Gradient (Curdisy of ECommons Gradient method.)

## Game Helpers
Interface with various methods for interaction with Dalamuds game data, mostly through `FFXIVClientStruct` interaction, 
but also Dalamuds Services as well.

## Hybrid Save Service
A save service that helps autosave config files tagged with `IHybridConfig`. These configs allow you to decide how
each of your configurations save and load, based on what method their structure is better suited for, allowing 
both JsonSerialization and StreamWrite serialization.

## Widgets
Provides several designed Widgets used by Cordelia's plugins, such as:
- CkHeaders
- Fancy SearchBars
- Fancy TabBars
- Icon TabBar
- Image TabBar
- Tab Collection (lets you interface with tags as buttons, using a csv string or collection of strings)
