# C# Inspector Description

[![CI](https://github.com/EstellaNines/godot-csharp-inspector-description/actions/workflows/ci.yml/badge.svg)](https://github.com/EstellaNines/godot-csharp-inspector-description/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
![Godot 4.7](https://img.shields.io/badge/Godot-4.7-478cbf?logo=godotengine&logoColor=white)

Show text for C# exported properties and fields in the Inspector: a dimmed label above the property, a hover tooltip, or both, the way GDScript `##` doc comments already work.

English | [简体中文](README.zh-CN.md)

<!-- TODO: add docs/tooltip.png (a screenshot of a described property's hover tooltip) before publishing. -->
![Hover tooltip for a member marked with InspectorTooltip](docs/tooltip.png)

## Features

- `[InspectorLabel("...")]` shows a dimmed label above an exported C# property or field.
- `[InspectorTooltip("...")]` shows its text in Godot's native hover tooltip, the same place GDScript `##` comments appear.
- Use either attribute alone, or both on the same member.
- Works for private fields, members inherited from a base class, grouped and subgrouped members, members of `Resource` objects, and nodes that the editor shows as placeholders.
- Godot's default editors (number, color, enum, resource and node pickers) are kept. The plugin changes only the tooltip and adds the label above the editor.
- The editor part is compiled only when `TOOLS` is defined, which Godot's Debug build configuration does. The attributes themselves are small runtime classes that game code references.

## Requirements

- Godot 4.7, .NET edition. Developed and tested with 4.7.2 stable.
- .NET 8 SDK (the project targets `net8.0`).

## Installation

1. Copy the `addons/inspector_description/` folder into the `addons/` folder of your project. Once this addon is published to the Godot Asset Library, you can also install it from the Asset Library tab.
2. Open the project in Godot 4.7 .NET and build the C# project with the **Build** button in the editor toolbar.
3. Enable **Inspector Description** in **Project → Project Settings → Plugins**, or add this to `project.godot`:

   ```ini
   [editor_plugins]

   enabled=PackedStringArray("res://addons/inspector_description/plugin.cfg")
   ```

## Usage

Add `using CSharpInspectorDescription;` and put `[InspectorLabel]`, `[InspectorTooltip]`, or both on an exported property or field:

```csharp
using CSharpInspectorDescription;
using Godot;

public partial class Player : CharacterBody2D
{
	// Both: a label above the property and a tooltip on hover.
	[Export]
	[InspectorLabel("Maximum health points.")]
	[InspectorTooltip("Maximum health points the character can have. Healing never goes above it.")]
	public int MaxHealth { get; set; } = 100;

	// Tooltip only: no label above the property.
	[Export]
	[InspectorTooltip("Movement speed in pixels per second.")]
	private float _moveSpeed = 200.0f;

	[ExportGroup("Combat")]
	// Label only: no tooltip on hover.
	[Export]
	[InspectorLabel("Damage per basic attack.")]
	public int AttackDamage { get; set; } = 10;

	// No attributes: the Inspector shows Godot's default tooltip only.
	[Export]
	public int Level { get; set; } = 1;
}
```

- **Private fields** work, as `_moveSpeed` shows above.
- **Inherited members** work. A private field declared in a base class is found from the derived class. An override without its own attribute of a given kind reuses the base class member's attribute of that kind.
- **Groups and subgroups** (`[ExportGroup]`, `[ExportSubgroup]`) do not change the lookup.
- **Resources** work too. Descriptions appear inside an expanded resource in the Inspector:

```csharp
using CSharpInspectorDescription;
using Godot;

[GlobalClass]
public partial class DamageStats : Resource
{
	[Export]
	[InspectorLabel("Base damage")]
	[InspectorTooltip("Damage dealt per hit, before armor is applied.")]
	public int BaseDamage { get; set; } = 10;
}
```

## Migrating from 1.x

Version 2.0.0 replaces `[InspectorDescription]` with two attributes. This is a breaking change.

- Replace `[InspectorDescription("x")]` with `[InspectorLabel("x")]`, `[InspectorTooltip("x")]`, or both lines. Version 1.x showed the text both above the property and in the tooltip, so use both lines to keep the same behavior.
- Code that calls `InspectorDescriptions.Find(obj, name)` now calls `Find<T>(ResolveType(obj), name)?.Text`, where `T` is `InspectorLabelAttribute` or `InspectorTooltipAttribute`. For example: `InspectorDescriptions.Find<InspectorTooltipAttribute>(InspectorDescriptions.ResolveType(obj), name)?.Text`.

## How it works

1. `DescriptionInspectorPlugin` asks `InspectorDescriptions.Find<InspectorLabelAttribute>` and `InspectorDescriptions.Find<InspectorTooltipAttribute>` for every property the Inspector builds. The class to search comes from `InspectorDescriptions.ResolveType`.
2. When there is a label or a tooltip, the plugin gets the editor Godot would create anyway from `EditorInspector.InstantiatePropertyEditor`, and passes it to the Inspector with `AddPropertyEditor`. A re-entrancy flag stops the recursion, because that call runs every Inspector plugin again, including this one. If there is a label, the plugin adds a dimmed `Label` with `AddCustomControl` before `AddPropertyEditor`. The Inspector adds controls in call order, so the label appears above the editor.
3. `EditorInspector::update_tree` in `editor/inspector/editor_inspector.cpp` sets each property's tooltip to `property|<class>|<name>`. That symbol is looked up in the engine documentation, which has no entry for a C# script, so Godot shows no text.
4. One frame later, if there is a tooltip, the plugin sets it to `local_variable|<class>|<name>|{"description": "..."}`. `EditorHelpBit::parse_symbol` in `editor/doc/editor_help.cpp` reads `description` from that JSON, so the native tooltip shows the text.
5. `InspectorDescriptions.Find<T>` starts at the class and walks up `BaseType`. `ResolveType` returns the script class for a real instance. For editor placeholders (a script that is not `[Tool]` is shown as a placeholder), it reads the script path from `GetScript()` and finds the script class through the `ScriptPathAttribute` that the Godot source generator adds to each script class.

The engine references above were checked against the Godot `4.7.2-stable` source. The tooltip route is used because `EditorInspector::add_custom_property_description` exists in 4.7.2 but is not exposed to scripts, and `CSharpScript.get_doc_class_name()` still returns an empty name (a `TODO` in the engine).

## Limitations

- The tooltip header reads **Local Variable** instead of **Property**, and there is no **Open documentation** link. This is the cost of reusing the `local_variable` symbol format.
- It depends on Godot 4.7 editor internals: the tooltip symbol format and the order of `update_tree`. Re-check the demo after each Godot upgrade.
- Attribute text is read from the built assembly. After you change a label or tooltip, or add a script, build the project and select the node again (or reopen the scene).
- Members are matched by their exact C# name. If you rename a member, update its attributes.
- If another Inspector plugin also takes over the same property, the plugin order decides which editor is used.
- If an Inspector has documentation hints turned off, hovering shows the raw symbol text instead of the tooltip.
- Known quirk: if a described property is added to **Favorites**, its label stays at the original position while the editor moves to the Favorites section. This follows from the engine code (`EditorInspector::update_tree`); it has not been checked in the editor.

## Demo project

The repository root is an openable Godot project.

1. Open the folder in Godot 4.7 .NET and build the C# project.
2. Open `demo/Demo.tscn` and press **F6**. The Guide label on screen lists what to hover over.

The scene has six child nodes: `PlaceholderNode` (a script that is not `[Tool]`), `ToolNode` (a `[Tool]` script), `Node2DHolder`, `InlineResourceHolder`, `GDScriptCompare` (a GDScript for comparison), and the `Guide` label.

## Running checks

```sh
dotnet build InspectorDescriptionDemo.csproj --nologo
godot --headless --path . res://tests/InspectorDescriptionChecks.tscn
```

Use the path to your Godot .NET editor binary in place of `godot`. A passing run prints `PASS: inspector description lookup.` and exits with code 0. A failure prints the exception and exits with code 1.

The checks cover the lookup logic only. Label and tooltip rendering are not covered by them; check them in the editor with the demo.

The workflow in `.github/workflows/ci.yml` runs the same steps on Ubuntu with Godot 4.7.2 .NET.

## Contributing

Bug reports and pull requests are welcome. Please use the bug report template, state your Godot and .NET versions, and run the checks above before you open a pull request. Commit messages follow [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/).

## Changelog

See [CHANGELOG.md](CHANGELOG.md).

## License

MIT. See [LICENSE](LICENSE). Copyright (c) 2026 EstellaNines.
