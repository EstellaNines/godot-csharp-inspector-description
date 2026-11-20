using CSharpInspectorDescription;
using Godot;
using System;

/// <summary>
/// Headless checks for InspectorDescriptions. Uses no editor classes.
/// </summary>
public partial class InspectorDescriptionChecks : Node
{
	private const string DemoScriptPath = "res://demo/DescriptionDemo.cs";
	private const string ResourceScriptPath = "res://demo/DescriptionResource.cs";
	private const string SamplePath = "res://demo/sample.tres";
	private const string ToolScriptPath = "res://demo/DescriptionToolDemo.cs";
	private const string Node2DScriptPath = "res://demo/DescriptionNode2DDemo.cs";
	private const string GdScriptPath = "res://demo/gdscript_compare.gd";
	private const string ScenePath = "res://demo/Demo.tscn";

	private const string MaxHealthText = "Maximum health points the character can have.";
	private const string MoveSpeedText = "Movement speed in pixels per second.";
	private const string AttackDamageText = "Damage dealt by one basic attack.";
	private const string AttackCooldownText = "Seconds to wait between two attacks.";
	private const string RespawnLimitText = "Lives granted when the player respawns, declared in the base class.";
	private const string BaseDamageText = "Damage dealt per hit, before armor is applied.";
	private const string ToolPriorityText = "Example priority value. The node does not use it.";
	private const string ToolScaleText = "Example scale shown as a range. The node does not use it.";
	private const string ToolLabelText = "Example text for a string export.";
	private const string StatsText = "Stats resource. Expand it to check the descriptions inside the Resource.";
	private const string DifficultyText = "Difficulty chosen from a dropdown (int with PropertyHint.Enum).";
	private const string TintText = "Tint color for this example node.";
	private const string TargetText = "Node path to ../PlaceholderNode, set in the scene file.";
	private const string SensitivityText = "Reaction speed scale, shown inside the Tuning subgroup.";
	private const string CombatGroupText = "Combat stats of the character.";
	private const string TuningGroupText = "Reaction tuning values for this node.";

	public override void _Ready()
	{
		try
		{
			CheckLookup();
			CheckScene();
			GD.Print("PASS: inspector description lookup.");
			GetTree().Quit();
		}
		catch (Exception exception)
		{
			GD.PushError(exception.ToString());
			GetTree().Quit(1);
		}
	}

	private static void CheckLookup()
	{
		// Described members on the demo class, including the private field inherited from its base.
		CheckText(typeof(DescriptionDemo), "MaxHealth", MaxHealthText, "Public property text");
		CheckText(typeof(DescriptionDemo), "_moveSpeed", MoveSpeedText, "Private field text");
		CheckText(typeof(DescriptionDemo), "AttackDamage", AttackDamageText, "Grouped member text");
		CheckText(typeof(DescriptionDemo), "AttackCooldown", AttackCooldownText, "Range property text");
		CheckText(typeof(DescriptionDemo), "_respawnLimit", RespawnLimitText, "Inherited private field text");
		CheckText(typeof(DescriptionDemo), "Level", null, "Undescribed member has no text");
		CheckText(typeof(DescriptionDemo), "NoSuchMember", null, "Unknown member has no text");

		Check(InspectorDescriptions.ResolveScriptType(DemoScriptPath) == typeof(DescriptionDemo), "Resolve demo script class");
		Check(InspectorDescriptions.ResolveScriptType(ResourceScriptPath) == typeof(DescriptionResource), "Resolve resource script class");
		Check(InspectorDescriptions.ResolveScriptType("res://demo/Missing.cs") == null, "Unknown script path has no class");

		// Real instance: GetType() already returns the script class.
		DescriptionDemo demo = new DescriptionDemo();
		CheckText(InspectorDescriptions.ResolveType(demo), "MaxHealth", MaxHealthText, "Real instance text");
		demo.Free();

		// Editor placeholders reach the class through GetScript().As<Script>().ResourcePath.
		// Real placeholders only exist in the editor, so check the conversion that route relies on here.
		Variant scriptValue = GD.Load<Script>(DemoScriptPath);
		Check(scriptValue.As<Script>().ResourcePath == DemoScriptPath, "Script value converts to its path");

		// Native node without a script: the lookup must return null without throwing.
		Node plain = new Node();
		CheckText(InspectorDescriptions.ResolveType(plain), "MaxHealth", null, "Plain node has no text");
		plain.Free();

		DescriptionResource sample = GD.Load<DescriptionResource>(SamplePath);
		Check(sample != null, "Load sample resource");
		CheckText(InspectorDescriptions.ResolveType(sample), "BaseDamage", BaseDamageText, "Resource instance text");

		// New script classes: described members, an undescribed control and a native Node2D name.
		Check(InspectorDescriptions.ResolveScriptType(ToolScriptPath) == typeof(DescriptionToolDemo), "Resolve tool script class");
		Check(InspectorDescriptions.ResolveScriptType(Node2DScriptPath) == typeof(DescriptionNode2DDemo), "Resolve Node2D script class");
		CheckText(typeof(DescriptionToolDemo), "ToolPriority", ToolPriorityText, "Tool script text");
		CheckText(typeof(DescriptionToolDemo), "ToolScale", ToolScaleText, "Tool script range text");
		CheckText(typeof(DescriptionToolDemo), "ToolLabel", ToolLabelText, "Tool script string text");
		CheckText(typeof(DescriptionToolDemo), "ToolActive", null, "Tool script undescribed member has no text");
		CheckText(typeof(DescriptionNode2DDemo), "Stats", StatsText, "Node2D resource text");
		CheckText(typeof(DescriptionNode2DDemo), "Difficulty", DifficultyText, "Node2D enum text");
		CheckText(typeof(DescriptionNode2DDemo), "Tint", TintText, "Node2D color text");
		CheckText(typeof(DescriptionNode2DDemo), "Target", TargetText, "Node2D node path text");
		CheckText(typeof(DescriptionNode2DDemo), "Sensitivity", SensitivityText, "Node2D subgroup text");
		CheckText(typeof(DescriptionNode2DDemo), "Layer", null, "Node2D undescribed member has no text");
		CheckText(typeof(DescriptionNode2DDemo), "Position", null, "Inherited Node2D Position has no text");

		// Group labels: a group name on ExportGroup, a bare subgroup name (no group), and an unknown name.
		Check(InspectorDescriptions.FindGroupLabel(typeof(DescriptionDemo), "Combat") == CombatGroupText, "Group label text");
		Check(InspectorDescriptions.FindGroupLabel(typeof(DescriptionNode2DDemo), "Tuning") == TuningGroupText, "Subgroup label text");
		Check(InspectorDescriptions.FindGroupLabel(typeof(DescriptionDemo), "NoSuchGroup") == null, "Unknown group has no label");
		Check(InspectorDescriptions.FindGroupLabel(typeof(DescriptionDemo), "Tuning") == null, "Subgroup name does not match a group");
	}

	// Adds the scene to the tree so NodePath references resolve, as they do when run with F6.
	private void CheckScene()
	{
		PackedScene packed = GD.Load<PackedScene>(ScenePath);
		Check(packed != null, "Load scene");

		Control scene = packed.Instantiate<Control>();
		AddChild(scene);
		try
		{
			CheckSceneNodes(scene);
		}
		finally
		{
			RemoveChild(scene);
			scene.Free();
		}
	}

	private static void CheckSceneNodes(Node scene)
	{
		// Each child covers one Inspector case, so each must exist with its expected type.
		DescriptionDemo placeholder = RequireChild<DescriptionDemo>(scene, "PlaceholderNode");
		DescriptionToolDemo toolNode = RequireChild<DescriptionToolDemo>(scene, "ToolNode");
		DescriptionNode2DDemo holder = RequireChild<DescriptionNode2DDemo>(scene, "Node2DHolder");
		DescriptionNode2DDemo inlineHolder = RequireChild<DescriptionNode2DDemo>(scene, "InlineResourceHolder");
		Node gdNode = RequireChild<Node>(scene, "GDScriptCompare");
		Label guide = RequireChild<Label>(scene, "Guide");
		Check(guide.Text.Length > 0, "Guide has checklist text");

		// Values set in the .tscn must reach the instances.
		Check(placeholder.MaxHealth == 150, "Scene overrides MaxHealth");
		Check(placeholder.AttackDamage == 25, "Scene overrides AttackDamage");
		Check(toolNode.ToolPriority == 3, "Scene overrides ToolPriority");
		Check(holder.Difficulty == 2, "Scene overrides Difficulty");
		Check(gdNode.Get("max_health").AsInt64() == 120, "Scene overrides GDScript max_health");

		// Real instance inside the scene: GetType() is the script class.
		CheckText(InspectorDescriptions.ResolveType(toolNode), "ToolPriority", ToolPriorityText, "Scene tool node text");
		CheckText(InspectorDescriptions.ResolveType(toolNode), "ToolActive", null, "Scene tool node undescribed member");

		// Native Node2D names and GDScript members must get no text.
		CheckText(InspectorDescriptions.ResolveType(holder), "position", null, "Native position has no text");
		CheckText(InspectorDescriptions.ResolveType(holder), "Position", null, "C# Position has no text");
		CheckText(InspectorDescriptions.ResolveType(gdNode), "max_health", null, "GDScript max_health has no text");
		CheckText(InspectorDescriptions.ResolveType(gdNode), "move_speed", null, "GDScript move_speed has no text");
		Check(gdNode.GetScript().As<Script>().ResourcePath == GdScriptPath, "GDScript node uses gdscript_compare.gd");

		// Stats: the shared .tres on Node2DHolder, a separate embedded copy on InlineResourceHolder.
		Check(holder.Stats != null, "Holder Stats is set");
		Check(holder.Stats.ResourcePath == SamplePath, "Holder Stats is sample.tres");
		Check(holder.Stats.BaseDamage == 25, "Holder Stats value from sample.tres");
		Check(inlineHolder.Stats != null, "Inline Stats is set");
		Check(inlineHolder.Stats.ResourcePath != SamplePath, "Inline Stats is not sample.tres");
		Check(inlineHolder.Stats.BaseDamage == 40, "Inline Stats value from SubResource");

		// The NodePath must name the sibling placeholder, and GetNodeOrNull must resolve it.
		Check(holder.Target.ToString() == "../PlaceholderNode", "Target stores ../PlaceholderNode");
		Check(holder.GetNodeOrNull(holder.Target)?.GetInstanceId() == placeholder.GetInstanceId(), "Target resolves to PlaceholderNode");
	}

	// Fails the check when the child is missing or has another type.
	private static T RequireChild<T>(Node parent, string name) where T : class
	{
		T child = parent.GetNodeOrNull(name) as T;
		Check(child != null, $"Child {name} exists as {typeof(T).Name}");
		return child;
	}

	// Checks both the label and the tooltip of a member; null means the member has no text.
	private static void CheckText(Type type, string memberName, string expected, string message)
	{
		Check(LabelText(type, memberName) == expected && TooltipText(type, memberName) == expected, message);
	}

	private static string LabelText(Type type, string memberName)
	{
		return InspectorDescriptions.Find<InspectorLabelAttribute>(type, memberName)?.Text;
	}

	private static string TooltipText(Type type, string memberName)
	{
		return InspectorDescriptions.Find<InspectorTooltipAttribute>(type, memberName)?.Text;
	}

	private static void Check(bool condition, string message)
	{
		if (!condition)
		{
			throw new InvalidOperationException(message);
		}
	}
}
