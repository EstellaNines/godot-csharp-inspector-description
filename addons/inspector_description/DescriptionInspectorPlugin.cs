// Inspector plugin that draws InspectorLabel, InspectorGroupLabel and InspectorTooltip text for C# members.
#if TOOLS
using System;
using System.Collections.Generic;
using Godot;

namespace CSharpInspectorDescription;

/// <summary>
/// Shows InspectorLabel text dimmed above a property, InspectorGroupLabel text at the top of a group, and InspectorTooltip text in the hover tooltip.
/// </summary>
[Tool]
public partial class DescriptionInspectorPlugin : EditorInspectorPlugin
{
	// InstantiatePropertyEditor calls every plugin again, including this one; the flag stops the recursion.
	private bool _instantiating;

	// Section paths already created in the current Inspector pass, per object; parse_group gets the full path for every new section.
	private readonly Dictionary<ulong, HashSet<string>> _openedSections = new();

	public override bool _CanHandle(GodotObject @object)
	{
		return true;
	}

	public override void _ParseBegin(GodotObject @object)
	{
		_openedSections[@object.GetInstanceId()] = new HashSet<string>();
	}

	// Godot rebuilds group sections for each category, so the same group path can open again.
	public override void _ParseCategory(GodotObject @object, string category)
	{
		if (_openedSections.TryGetValue(@object.GetInstanceId(), out HashSet<string> opened))
		{
			opened.Clear();
		}
	}

	public override void _ParseGroup(GodotObject @object, string group)
	{
		ulong id = @object.GetInstanceId();
		if (!_openedSections.TryGetValue(id, out HashSet<string> opened))
		{
			opened = new HashSet<string>();
			_openedSections[id] = opened;
		}

		// The first prefix not seen yet is the section Godot is creating now.
		string section = null;
		string prefix = "";
		foreach (string part in group.Split('/'))
		{
			prefix = prefix.Length == 0 ? part : prefix + "/" + part;
			if (opened.Add(prefix))
			{
				section = prefix;
				break;
			}
		}

		if (section == null)
		{
			return;
		}

		string text;
		try
		{
			text = InspectorDescriptions.FindGroupLabel(InspectorDescriptions.ResolveType(@object), section);
		}
		catch (Exception exception)
		{
			// Any lookup failure must not break the Inspector, so log it and skip the label.
			GD.PushWarning($"Inspector Description: group lookup failed for '{section}': {exception.Message}");
			return;
		}

		if (!string.IsNullOrEmpty(text))
		{
			AddCustomControl(CreateLabel(text));
		}
	}

	public override void _ParseEnd(GodotObject @object)
	{
		_openedSections.Remove(@object.GetInstanceId());
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
			AddCustomControl(CreateLabel(label));
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

	// Dimmed, wrapping text shared by property and group labels.
	private static Label CreateLabel(string text)
	{
		return new Label
		{
			Text = text,
			AutowrapMode = TextServer.AutowrapMode.WordSmart,
			Modulate = new Color(1, 1, 1, 0.6f),
		};
	}

	// EditorHelpBit reads the description from the JSON part only for local symbols; "property" symbols need engine docs.
	private static string BuildTooltipSymbol(string className, string propertyName, string description)
	{
		string data = Json.Stringify(new Godot.Collections.Dictionary { ["description"] = description });
		return $"local_variable|{className}|{propertyName}|{data}";
	}
}
#endif
