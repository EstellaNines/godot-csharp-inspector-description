// Inspector plugin that draws InspectorLabel labels and InspectorTooltip tooltips for C# members.
#if TOOLS
using System;
using Godot;

namespace CSharpInspectorDescription;

/// <summary>
/// Shows InspectorLabel text dimmed above a property and InspectorTooltip text in its native hover tooltip.
/// </summary>
[Tool]
public partial class DescriptionInspectorPlugin : EditorInspectorPlugin
{
	// InstantiatePropertyEditor calls every plugin again, including this one; the flag stops the recursion.
	private bool _instantiating;

	public override bool _CanHandle(GodotObject @object)
	{
		return true;
	}

	public override bool _ParseProperty(GodotObject @object, Variant.Type type, string name, PropertyHint hintType, string hintString, PropertyUsageFlags usageFlags, bool wide)
	{
		if (_instantiating)
		{
			return false;
		}

		string label;
		string tooltip;
		try
		{
			Type scriptType = InspectorDescriptions.ResolveType(@object);
			label = InspectorDescriptions.Find<InspectorLabelAttribute>(scriptType, name)?.Text;
			tooltip = InspectorDescriptions.Find<InspectorTooltipAttribute>(scriptType, name)?.Text;
		}
		catch (Exception exception)
		{
			// Any lookup failure must not break the Inspector, so log it and keep the default editor.
			GD.PushWarning($"Inspector Description: lookup failed for '{name}': {exception.Message}");
			return false;
		}

		if (string.IsNullOrEmpty(label) && string.IsNullOrEmpty(tooltip))
		{
			return false;
		}

		EditorProperty editor;
		_instantiating = true;
		try
		{
			// Reuse the editor Godot would build itself, so only the label and tooltip change.
			editor = EditorInspector.InstantiatePropertyEditor(@object, type, name, hintType, hintString, (uint)usageFlags, wide);
		}
		finally
		{
			_instantiating = false;
		}

		if (editor == null)
		{
			return false;
		}

		// The Inspector adds controls in call order, so the label must come before the editor.
		if (!string.IsNullOrEmpty(label))
		{
			AddCustomControl(new Label
			{
				Text = label,
				AutowrapMode = TextServer.AutowrapMode.WordSmart,
				Modulate = new Color(1, 1, 1, 0.6f),
			});
		}
		AddPropertyEditor(name, editor);

		if (!string.IsNullOrEmpty(tooltip))
		{
			string symbol = BuildTooltipSymbol(@object.GetClass(), name, tooltip);

			// The Inspector resets tooltip_text right after adding the editor, so set ours one frame later.
			Callable.From(() =>
			{
				if (GodotObject.IsInstanceValid(editor))
				{
					editor.TooltipText = symbol;
				}
			}).CallDeferred();
		}

		return true;
	}

	// EditorHelpBit reads the description from the JSON part only for local symbols; "property" symbols need engine docs.
	private static string BuildTooltipSymbol(string className, string propertyName, string description)
	{
		string data = Json.Stringify(new Godot.Collections.Dictionary { ["description"] = description });
		return $"local_variable|{className}|{propertyName}|{data}";
	}
}
#endif
