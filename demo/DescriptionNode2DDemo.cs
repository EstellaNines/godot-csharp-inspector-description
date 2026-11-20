using CSharpInspectorDescription;
using Godot;

/// <summary>
/// Node2D script for the scene checks: Resource, enum hint, color, NodePath and subgroup.
/// Native Node2D properties such as Position must not get a description tooltip.
/// </summary>
public partial class DescriptionNode2DDemo : Node2D
{
	[Export]
	[InspectorDescription("Stats resource. Expand it to check the descriptions inside the Resource.")]
	public DescriptionResource Stats { get; set; }

	[Export(PropertyHint.Enum, "Easy,Normal,Hard")]
	[InspectorDescription("Difficulty chosen from a dropdown (int with PropertyHint.Enum).")]
	public int Difficulty { get; set; } = 1;

	[Export]
	[InspectorDescription("Tint color for this example node.")]
	public Color Tint { get; set; } = new Color(1.0f, 0.8f, 0.4f, 1.0f);

	// NodePath is read straight from the scene file, so it does not depend on load-time node resolution.
	[Export]
	[InspectorDescription("Node path to ../PlaceholderNode, set in the scene file.")]
	public NodePath Target { get; set; } = new NodePath("");

	// Control case: no description, so no tooltip should appear.
	[Export]
	public int Layer { get; set; }

	[ExportSubgroup("Tuning")]
	[Export]
	[InspectorDescription("Reaction speed scale, shown inside the Tuning subgroup.")]
	public float Sensitivity { get; set; } = 1.0f;
}
