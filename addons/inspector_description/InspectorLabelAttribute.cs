// Attribute that shows a dimmed label above a C# exported member in the Inspector.
using System;

namespace CSharpInspectorDescription;

/// <summary>
/// Gives a C# exported property or field a dimmed label above it in the Inspector.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
public sealed class InspectorLabelAttribute : Attribute
{
	/// <summary>Creates the attribute with the label text.</summary>
	public InspectorLabelAttribute(string text)
	{
		Text = text;
	}

	/// <summary>Text shown above the property.</summary>
	public string Text { get; }
}
