using CSharpInspectorDescription;
using Godot;

/// <summary>
/// Manual Inspector test node. Not a [Tool] script, so the editor shows a placeholder instance.
/// </summary>
public partial class DescriptionDemo : DescriptionDemoBase
{
	[Export]
	[InspectorDescription("Maximum health points the character can have.")]
	public int MaxHealth { get; set; } = 100;

	[Export]
	[InspectorDescription("Movement speed in pixels per second.")]
	private float _moveSpeed = 200.0f;

	// Control case: no description, so no tooltip should appear.
	[Export]
	public int Level { get; set; } = 1;

	[ExportGroup("Combat")]
	[Export]
	[InspectorDescription("Damage dealt by one basic attack.")]
	public int AttackDamage { get; set; } = 10;

	[Export(PropertyHint.Range, "0.01,10,0.01,or_greater,suffix:s")]
	[InspectorDescription("Seconds to wait between two attacks.")]
	public float AttackCooldown { get; set; } = 1.0f;

	public override void _Ready()
	{
		GD.Print($"DescriptionDemo ready: MaxHealth={MaxHealth}, AttackDamage={AttackDamage}.");
	}
}
