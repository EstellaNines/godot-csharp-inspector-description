using System;

namespace CSharpInspectorDescription;

/// <summary>
/// Gives a C# exported property or field a description shown as a hover tooltip in the Inspector.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
public sealed class InspectorDescriptionAttribute : Attribute
{
	/// <summary>Creates the attribute with the description text.</summary>
	public InspectorDescriptionAttribute(string text)
	{
		Text = text;
	}

	/// <summary>Description text, also used as the Inspector tooltip.</summary>
	public string Text { get; }
}
