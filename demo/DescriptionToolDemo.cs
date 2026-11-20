using CSharpInspectorDescription;
using Godot;

/// <summary>
/// Tool script for the real-instance path: the editor creates a real instance, so GetType() returns this class.
/// Kept inert on purpose: no _Process, _Ready or editor-side work.
/// </summary>
[Tool]
public partial class DescriptionToolDemo : Node
{
	[Export]
	[InspectorDescription("Example priority value. The node does not use it.")]
	public int ToolPriority { get; set; } = 1;

	[Export(PropertyHint.Range, "0,4,0.5")]
	[InspectorDescription("Example scale shown as a range. The node does not use it.")]
	public float ToolScale { get; set; } = 1.0f;

	[Export]
	[InspectorDescription("Example text for a string export.")]
	public string ToolLabel { get; set; } = "Editor example";

	// Control case: no description, so no tooltip should appear.
	[Export]
	public bool ToolActive { get; set; } = true;
}
