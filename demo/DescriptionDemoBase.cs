using CSharpInspectorDescription;
using Godot;

/// <summary>
/// Base class with a private exported field, used to check lookup through the inheritance chain.
/// </summary>
public partial class DescriptionDemoBase : Node
{
	[Export]
	[InspectorLabel("Lives granted when the player respawns, declared in the base class.")]
	[InspectorTooltip("Lives granted when the player respawns, declared in the base class.")]
	private int _respawnLimit = 3;
}
