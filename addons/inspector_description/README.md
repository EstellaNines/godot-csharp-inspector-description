# C# Inspector Description

Shows `[InspectorLabel("...")]` text as a dimmed label above C# exported properties and fields, and `[InspectorTooltip("...")]` text in their Inspector hover tooltip, the way GDScript `##` doc comments do. Use either one or both. `[InspectorGroupLabel("...")]` puts a dimmed label at the top of an export group.

## Install

1. Copy this `inspector_description` folder into the `addons/` folder of your project.
2. Open the project in Godot 4.7 .NET and build the C# project.
3. Enable **Inspector Description** in **Project → Project Settings → Plugins**.

## Use

```csharp
using CSharpInspectorDescription;
using Godot;

[Export]
[InspectorLabel("Maximum health points.")]
[InspectorTooltip("Maximum health points the character can have. Healing never goes above it.")]
public int MaxHealth { get; set; } = 100;
```

Put `[InspectorGroupLabel]` on the same member as `[ExportGroup]` or `[ExportSubgroup]` to label that group:

```csharp
[ExportGroup("Combat")]
[InspectorGroupLabel("Combat stats of the character.")]
[Export]
public int AttackDamage { get; set; } = 10;
```

Build the project again after you change a description, then select the node again.

Full documentation, limitations and the demo project are in the [repository README](https://github.com/EstellaNines/godot-csharp-inspector-description#readme).
