#if TOOLS
using Godot;

namespace CSharpInspectorDescription;

/// <summary>
/// Editor plugin entry point that registers the description Inspector plugin.
/// </summary>
[Tool]
public partial class InspectorDescriptionPlugin : EditorPlugin
{
	private DescriptionInspectorPlugin _inspectorPlugin;

	public override void _EnterTree()
	{
		_inspectorPlugin = new DescriptionInspectorPlugin();
		AddInspectorPlugin(_inspectorPlugin);
	}

	public override void _ExitTree()
	{
		// Guard against an exit without a matching enter, so the removal never gets a null plugin.
		if (_inspectorPlugin == null)
		{
			return;
		}

		RemoveInspectorPlugin(_inspectorPlugin);
		_inspectorPlugin = null;
	}
}
#endif
