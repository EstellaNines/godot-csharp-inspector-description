# Changelog

All notable changes to this project are documented in this file.

The format follows [Keep a Changelog 1.1.0](https://keepachangelog.com/en/1.1.0/), and the project uses [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [2.0.0] - 2026-11-20

### Changed

- **BREAKING:** `[InspectorDescription]` is split into `[InspectorLabel]` (dimmed label above the property) and `[InspectorTooltip]` (hover tooltip). Replace each `[InspectorDescription("x")]` with one or both of them.
- **BREAKING:** `InspectorDescriptions.Find(obj, name)` returning a string is replaced by `Find<T>(Type, name)`, where `T` is `InspectorLabelAttribute` or `InspectorTooltipAttribute`. Get the `Type` with `InspectorDescriptions.ResolveType(obj)`.

### Added

- `[InspectorLabel]` and `[InspectorTooltip]` can be used alone or together on the same member.
- `InspectorDescriptions.ResolveType(GodotObject)` returns the C# script class, including for editor placeholder instances.

## [1.1.0] - 2026-11-20

### Added

- Dimmed description label above each described property, shown together with the hover tooltip.

## [1.0.0] - 2026-11-20

### Added

- `[InspectorDescription]` attribute for exported C# properties and fields, in the `CSharpInspectorDescription` namespace.
- Inspector plugin that shows the description in Godot's native hover tooltip.
- Lookup for private fields, inherited members and editor placeholder instances.
- Manual demo scene (`demo/Demo.tscn`) with an on-screen checklist.
- Headless checks (`tests/InspectorDescriptionChecks.tscn`) and a GitHub Actions workflow that runs them with Godot 4.7.2 .NET.
- MIT license, and English and Chinese README files.

[Unreleased]: https://github.com/EstellaNines/godot-csharp-inspector-description/compare/v2.0.0...HEAD
[2.0.0]: https://github.com/EstellaNines/godot-csharp-inspector-description/compare/v1.1.0...v2.0.0
[1.1.0]: https://github.com/EstellaNines/godot-csharp-inspector-description/compare/v1.0.0...v1.1.0
[1.0.0]: https://github.com/EstellaNines/godot-csharp-inspector-description/releases/tag/v1.0.0
