# Changelog

All notable changes to this project are documented in this file.

The format follows [Keep a Changelog 1.1.0](https://keepachangelog.com/en/1.1.0/), and the project uses [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.0] - 2026-11-20

### Added

- `[InspectorDescription]` attribute for exported C# properties and fields, in the `CSharpInspectorDescription` namespace.
- Inspector plugin that shows the description in Godot's native hover tooltip.
- Lookup for private fields, inherited members and editor placeholder instances.
- Manual demo scene (`demo/Demo.tscn`) with an on-screen checklist.
- Headless checks (`tests/InspectorDescriptionChecks.tscn`) and a GitHub Actions workflow that runs them with Godot 4.7.2 .NET.
- MIT license, and English and Chinese README files.

[Unreleased]: https://github.com/EstellaNines/godot-csharp-inspector-description/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/EstellaNines/godot-csharp-inspector-description/releases/tag/v1.0.0
