global using System;
global using System.Collections;
global using System.Collections.Concurrent;
global using System.Collections.Generic;
global using System.Linq;
global using System.Numerics;
global using System.Text;
global using System.Text.RegularExpressions;

global using CFlags = ImGuiNET.ImGuiComboFlags;
global using DFlags = ImGuiNET.ImDrawFlags;
global using FAI = Dalamud.Interface.FontAwesomeIcon;
global using ITFlags = ImGuiNET.ImGuiInputTextFlags;
global using WFlags = ImGuiNET.ImGuiWindowFlags;

global using MoodlesStatusInfo = (
    System.Guid GUID,
    int IconID,
    string Title,
    string Description,
    CkCommons.Textures.MoodleDisplay.StatusType Type,
    string Applier,
    bool Dispelable,
    int Stacks,
    bool Persistent,
    int Days,
    int Hours,
    int Minutes,
    int Seconds,
    bool NoExpire,
    bool AsPermanent,
    System.Guid StatusOnDispell,
    string CustomVFXPath,
    bool StackOnReapply,
    int StacksIncOnReapply
    );

