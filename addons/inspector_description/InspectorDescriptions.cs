using System;
using System.Collections.Generic;
using System.Reflection;
using Godot;

namespace CSharpInspectorDescription;

/// <summary>
/// Looks up InspectorDescription text for members of C# Godot scripts.
/// </summary>
public static class InspectorDescriptions
{
	// Private members are included so base-class fields are found.
	// DeclaredOnly checks one class at a time, so the walk in Find controls the order.
	private const BindingFlags MemberFlags = BindingFlags.Instance | BindingFlags.Static
		| BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

	// Built lazily: it scans every type, and lookups run once per Inspector property.
	private static Dictionary<string, Type> _scriptTypes;

	/// <summary>Returns the description of a member declared on the type or on a base class.</summary>
	public static string Find(Type type, string memberName)
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

			// Without text here, keep walking so an override can reuse the base description.
			InspectorDescriptionAttribute attribute = member.GetCustomAttribute<InspectorDescriptionAttribute>(false);
			if (attribute != null)
			{
				return attribute.Text;
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

	/// <summary>Returns the description of a member of a Godot object, including editor placeholder instances.</summary>
	public static string Find(GodotObject obj, string memberName)
	{
		if (obj == null)
		{
			return null;
		}

		Type type = obj.GetType();
		if (type.Assembly != typeof(GodotObject).Assembly)
		{
			// A real C# instance exposes its own script class.
			return Find(type, memberName);
		}

		// Editor placeholders only expose the native class, so find the script class by its path.
		Script script = obj.GetScript().As<Script>();
		if (script == null)
		{
			return null;
		}

		Type scriptType = ResolveScriptType(script.ResourcePath);
		return scriptType == null ? null : Find(scriptType, memberName);
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
