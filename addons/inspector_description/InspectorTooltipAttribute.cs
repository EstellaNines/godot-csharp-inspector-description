// Attribute that sets the Inspector hover tooltip of a C# exported member.
using System;

namespace CSharpInspectorDescription;

/// <summary>
/// Gives a C# exported property or field a hover tooltip in the Inspector.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
public sealed class InspectorTooltipAttribute : Attribute
{
	/// <summary>Creates the attribute with the tooltip text.</summary>
	public InspectorTooltipAttribute(string text)
	{
		Text = text;
	}

	/// <summary>Text shown when the mouse hovers over the property.</summary>
	public string Text { get; }
}
