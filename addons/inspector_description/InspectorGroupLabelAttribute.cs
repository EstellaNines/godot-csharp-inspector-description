// Attribute that shows a dimmed label at the top of a C# export group or subgroup in the Inspector.
using System;

namespace CSharpInspectorDescription;

/// <summary>
/// Put it on the member that carries [ExportGroup] or [ExportSubgroup]; the text shows at the top of that group.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
public sealed class InspectorGroupLabelAttribute : Attribute
{
	/// <summary>Creates the attribute with the group label text.</summary>
	public InspectorGroupLabelAttribute(string text)
	{
		Text = text;
	}

	/// <summary>Text shown at the top of the group.</summary>
	public string Text { get; }
}
