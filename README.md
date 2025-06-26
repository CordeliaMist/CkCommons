## What Is CkCommons?

**CkCommons** is a modular library tailored for use with Dalamud plugin projects. It offers utility classes, customizable widgets, helper methods, and quality-of-life abstractions to reduce boilerplate and streamline plugin development.

In addition, CkCommons includes a robust ImGui wrapper, **CkGui**, which enhances the visual styling of ImGui elements without sacrificing rendering performance.

---

## Adding CkCommons to Your Project

To integrate CkCommons as a Git submodule in your plugin repository, run:

```bash
git submodule add https://github.com/CordeliaMist/CkCommons.git
```

If CkCommons is not already referenced as a submodule, manually include it in your `.csproj` file:

```xml
<ItemGroup>
    <ProjectReference Include="..\CkCommons\CkCommons.csproj" />
</ItemGroup>
```

### Initialization

To use features that rely on Dalamud plugin services (hooks, textures, data, etc.), **you must manually initialize CkCommons**. Dependency injection is not supported.

In your `IDalamudPlugin` entry point:

```csharp
CkCommonsMain.Init(pluginInterface, this);
```

Where `pluginInterface` is the `IDalamudPluginInterface` instance provided by Dalamud.

### Disposal

On plugin unload (or when your runtime service scope is disposed, if using DI), call:

```csharp
CkCommonsMain.DisposeAll();
```

---

## Features

### 🖼️ CkGui, CkGuiEx, and CkGuiUtils

A comprehensive wrapper over ImGui/ImGui.NET, providing:

* Beautifully styled ImGui elements
* QoL extensions for ImGui and DrawList methods
* Reduced boilerplate for standard UI patterns

---

### 🎨 CkRaii
An enhancement of OtterGui’s `ImRaii`, providing:
* Extended styling and scoped UI helpers
* Integration with CkGui defaults
* Additional original CkRaii elements that template stylized ImGui components.

---

### 💅 CkStyle
A centralized styling system reflecting the aesthetic and structural patterns used across Cordelia's plugins. 
- Provides reusable defaults for CkGui and CkRaii components.

---

### 📝 CkRichText
A performance-optimized text rendering system that mimics Dalamud's SeString formatting. Features include:
* Colored text
* Glows
* Icons and images
* Minimal draw-time impact even with large volumes of formatted text

---

### 📦 Utility Classes
* `OptionalBoolCheckbox`: An enhanced OtterGui checkbox with additional customization
* `OptionalBoolIconCheckbox`: A version of the above with icon support
* `Gradient`: Based on ECommons' gradient utility

---

### 🕹️ Game Helpers
Methods and interfaces that provide deep interaction with:
* Dalamud game services
* Low-level `FFXIVClientStructs`
* Game data access and manipulation

---

### 💾 Hybrid Save Service
A configurable save system for objects implementing `IHybridConfig`. Allows:
* Fine-tuned control over save/load behavior
* Automatic save handling
* Support for both JSON serialization and StreamWriter-based serialization
---

### 🧹 Custom Widgets
Reusable UI elements designed for plugin UIs, including:
* CkHeaders
* Stylized Search Bars
* TabBar variants (standard, icon-based, image-based)
* TagCollection (convert CSV strings or string lists into tabbed button groups)
