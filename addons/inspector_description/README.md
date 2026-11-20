# C# Inspector Description

Shows `[InspectorDescription("...")]` text in the Inspector hover tooltip of C# exported properties and fields, the way GDScript `##` doc comments do.

## Install

1. Copy this `inspector_description` folder into the `addons/` folder of your project.
2. Open the project in Godot 4.7 .NET and build the C# project.
3. Enable **Inspector Description** in **Project → Project Settings → Plugins**.

## Use

```csharp
using CSharpInspectorDescription;
using Godot;

[Export]
[InspectorDescription("Maximum health points the character can have.")]
public int MaxHealth { get; set; } = 100;
```

Build the project again after you change a description, then select the node again.

Full documentation, limitations and the demo project are in the [repository README](https://github.com/EstellaNines/godot-csharp-inspector-description#readme).
