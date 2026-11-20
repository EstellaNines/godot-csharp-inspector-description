// Finds InspectorLabel, InspectorGroupLabel and InspectorTooltip attributes on members of C# Godot scripts.
using System;
using System.Collections.Generic;
using System.Reflection;
using Godot;

namespace CSharpInspectorDescription;

/// <summary>
/// Looks up Inspector attributes for members of C# Godot scripts, including group labels.
/// </summary>
public static class InspectorDescriptions
{
	// Private members are included so base-class fields are found.
	// DeclaredOnly checks one class at a time, so the walk in Find controls the order.
	private const BindingFlags MemberFlags = BindingFlags.Instance | BindingFlags.Static
		| BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

	// Built lazily: it scans every type, and lookups run once per Inspector property.
	private static Dictionary<string, Type> _scriptTypes;

	/// <summary>Returns the attribute of type T on a member declared on the type or on a base class, or null.</summary>
	public static T Find<T>(Type type, string memberName) where T : Attribute
	{
		if (type == null || string.IsNullOrEmpty(memberName))
		{
			return null;
		}

		for (Type current = type; current != null; current = current.BaseType)
		{
			MemberInfo member = (MemberInfo)current.GetProperty(memberName, MemberFlags)
				?? current.GetField(memberName, MemberFlags);
			if (member == null)
			{
				continue;
			}

			// Without the attribute here, keep walking so an override can reuse the base text.
			T attribute = member.GetCustomAttribute<T>(false);
			if (attribute != null)
			{
				return attribute;
			}
		}

		return null;
	}

	/// <summary>Returns the InspectorGroupLabel text of the export group or subgroup at sectionPath, or null.</summary>
	public static string FindGroupLabel(Type type, string sectionPath)
	{
		if (type == null || string.IsNullOrEmpty(sectionPath))
		{
			return null;
		}

		// Subgroup paths are "Group/Subgroup"; a bare name is a group, or a subgroup with no group.
		// ponytail: two subgroups with the same name match the first one found.
		int slash = sectionPath.LastIndexOf('/');
		string name = sectionPath.Substring(slash + 1);
		for (Type current = type; current != null; current = current.BaseType)
		{
			foreach (MemberInfo member in current.GetMembers(MemberFlags))
			{
				InspectorGroupLabelAttribute label = member.GetCustomAttribute<InspectorGroupLabelAttribute>(false);
				if (label == null)
				{
					continue;
				}

				string declared = slash >= 0
					? member.GetCustomAttribute<ExportSubgroupAttribute>(false)?.Name
					: member.GetCustomAttribute<ExportGroupAttribute>(false)?.Name
						?? member.GetCustomAttribute<ExportSubgroupAttribute>(false)?.Name;
				if (declared == name)
				{
					return label.Text;
				}
			}
		}

		return null;
	}

	/// <summary>Maps a res:// C# script path to its class, or returns null when no class uses it.</summary>
	public static Type ResolveScriptType(string scriptResourcePath)
	{
		if (string.IsNullOrEmpty(scriptResourcePath))
		{
			return null;
		}

		_scriptTypes ??= BuildScriptTypeMap();
		return _scriptTypes.TryGetValue(scriptResourcePath, out Type type) ? type : null;
	}

	/// <summary>Returns the C# script class of a Godot object, including editor placeholder instances, or null.</summary>
	public static Type ResolveType(GodotObject obj)
	{
		if (obj == null)
		{
			return null;
		}

		Type type = obj.GetType();
		if (type.Assembly != typeof(GodotObject).Assembly)
		{
			// A real C# instance exposes its own script class.
			return type;
		}

		// Editor placeholders only expose the native class, so find the script class by its path.
		Script script = obj.GetScript().As<Script>();
		return script == null ? null : ResolveScriptType(script.ResourcePath);
	}

	private static Dictionary<string, Type> BuildScriptTypeMap()
	{
		Dictionary<string, Type> map = new Dictionary<string, Type>();
		foreach (Type type in typeof(InspectorDescriptions).Assembly.GetTypes())
		{
			// The Godot source generator tags each script class with its res:// path.
			ScriptPathAttribute scriptPath = type.GetCustomAttribute<ScriptPathAttribute>(false);
			if (scriptPath != null)
			{
				map[scriptPath.Path] = type;
			}
		}

		return map;
	}
}
