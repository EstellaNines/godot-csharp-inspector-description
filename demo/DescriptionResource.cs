using CSharpInspectorDescription;
using Godot;

/// <summary>
/// Resource with a described property, used to check descriptions on Resource objects.
/// </summary>
[GlobalClass]
public partial class DescriptionResource : Resource
{
	[Export]
	[InspectorDescription("Damage dealt per hit, before armor is applied.")]
	public int BaseDamage { get; set; } = 10;
}
