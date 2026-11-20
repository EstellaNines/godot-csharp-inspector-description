# C# Inspector Description

[![CI](https://github.com/EstellaNines/godot-csharp-inspector-description/actions/workflows/ci.yml/badge.svg)](https://github.com/EstellaNines/godot-csharp-inspector-description/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
![Godot 4.7](https://img.shields.io/badge/Godot-4.7-478cbf?logo=godotengine&logoColor=white)

让 C# 导出属性与字段在 Inspector 中显示说明：属性上方显示淡色标签，悬停时的提示框中也显示同一段文字，效果与 GDScript 的 `##` 文档注释相同。

[English](README.md) | 简体中文

<!-- TODO: 发布前添加 docs/tooltip.png（被标注属性的悬停提示截图）。 -->
![被 InspectorDescription 标注的成员的悬停提示](docs/tooltip.png)

## 功能

- 为导出的 C# 属性和字段标注 `[InspectorDescription("...")]`。
- 说明文字以淡色标签显示在属性上方，并显示在 Godot 原生的悬停提示框中，提示框的位置与 GDScript `##` 注释相同。
- 支持私有字段、基类继承的成员、分组与子分组中的成员、`Resource` 对象的成员，以及编辑器中显示为 placeholder 的节点。
- Godot 默认编辑器（数值、颜色、枚举、资源与节点选择器）保持不变。插件只改变提示框的内容，并在编辑器上方加入说明标签。
- 编辑器部分只在定义 `TOOLS` 时编译，Godot 的 Debug 构建配置会定义它。特性本身是一个小型运行时类，游戏代码需要引用它。

## 环境要求

- Godot 4.7 .NET 版。以 4.7.2 stable 开发并测试。
- .NET 8 SDK（工程目标框架为 `net8.0`）。

## 安装

1. 将 `addons/inspector_description/` 文件夹复制到工程的 `addons/` 文件夹中。发布到 Godot Asset Library 后，也可以在资源库页签中安装。
2. 用 Godot 4.7 .NET 打开工程，点击编辑器工具栏中的 **Build** 按钮，构建 C# 项目。
3. 在 **Project → Project Settings → Plugins** 中启用 **Inspector Description**；也可以在 `project.godot` 中加入：

   ```ini
   [editor_plugins]

   enabled=PackedStringArray("res://addons/inspector_description/plugin.cfg")
   ```

## 用法

添加 `using CSharpInspectorDescription;`，然后在导出的属性或字段上标注特性：

```csharp
using CSharpInspectorDescription;
using Godot;

public partial class Player : CharacterBody2D
{
	[Export]
	[InspectorDescription("角色的最大生命值。")]
	public int MaxHealth { get; set; } = 100;

	[Export]
	[InspectorDescription("每秒移动的像素数。")]
	private float _moveSpeed = 200.0f;

	[ExportGroup("Combat")]
	[Export]
	[InspectorDescription("一次普通攻击造成的伤害。")]
	public int AttackDamage { get; set; } = 10;

	// No attribute: the Inspector shows Godot's default tooltip only.
	[Export]
	public int Level { get; set; } = 1;
}
```

- **私有字段**可以标注，如上面的 `_moveSpeed`。
- **继承的成员**可以查到。基类中声明的私有字段也能从派生类中找到。派生类重写成员但没有自己的特性时，沿用基类成员的说明。
- **分组与子分组**（`[ExportGroup]`、`[ExportSubgroup]`）不影响查找。
- **Resource** 同样支持。展开资源后，其中的属性也会显示说明：

```csharp
using CSharpInspectorDescription;
using Godot;

[GlobalClass]
public partial class DamageStats : Resource
{
	[Export]
	[InspectorDescription("护甲生效前，每次命中造成的伤害。")]
	public int BaseDamage { get; set; } = 10;
}
```

## 工作原理

1. `DescriptionInspectorPlugin` 为 Inspector 构建的每个属性调用 `InspectorDescriptions.Find`，查询说明。
2. 查到说明时，插件通过 `EditorInspector.InstantiatePropertyEditor` 取得 Godot 原本就会创建的编辑器，再用 `AddPropertyEditor` 交给 Inspector。由于该调用会再次运行全部 Inspector 插件（包括本插件），插件用重入标志跳过这一次，避免无限递归。在调用 `AddPropertyEditor` 之前，插件还会用 `AddCustomControl` 加入一个淡色 `Label`。Inspector 按加入的先后顺序排列控件，因此标签位于编辑器上方。
3. `editor/inspector/editor_inspector.cpp` 中的 `EditorInspector::update_tree` 会把每个属性的提示设为 `property|<类>|<属性>`。引擎文档库中没有 C# 脚本的条目，因此 Godot 原生提示为空。
4. 插件延迟一帧，把提示改写为 `local_variable|<类>|<属性>|{"description": "..."}`。`editor/doc/editor_help.cpp` 中的 `EditorHelpBit::parse_symbol` 会从该 JSON 读取 `description`，于是原生提示框显示说明。
5. `InspectorDescriptions.Find` 从对象的类型开始，沿 `BaseType` 逐级向上查找。对于编辑器中的 placeholder（非 `[Tool]` 脚本显示为 placeholder），它通过 `GetScript()` 取得脚本路径，再借助 Godot 源码生成器为每个脚本类附加的 `ScriptPathAttribute` 找到脚本类。

以上引擎引用已对照 Godot `4.7.2-stable` 源码核实。之所以采用提示框方案，是因为 `EditorInspector::add_custom_property_description` 在 4.7.2 中存在，但没有向脚本公开；而 `CSharpScript.get_doc_class_name()` 仍返回空名称（引擎中的 `TODO`）。

## 已知限制

- 提示框标题显示为 **Local Variable**，而不是 **Property**，且没有 **Open documentation** 跳转。这是复用 `local_variable` 符号格式的代价。
- 依赖 Godot 4.7 编辑器的内部实现：提示符号格式与 `update_tree` 的调用顺序。每次升级 Godot 后都应重新检查 demo。
- 说明从构建产物中读取。修改说明或新增脚本后，请先构建项目，再重新选中节点（或重新打开场景）。
- 成员按 C# 名称精确匹配。重命名成员后，请同步修改特性。
- 若其他 Inspector 插件也接管同一属性，由插件的先后顺序决定使用哪个编辑器。
- 若某个 Inspector 关闭了文档提示（`use_doc_hints`），悬停时显示的是原始符号文本，而不是说明。
- 已知的小问题：若带说明的属性被加入**收藏（Favorites）**，标签仍留在原位置，而编辑器会移到收藏区。这一点是根据引擎源码推断的，尚未在编辑器中验证。

## Demo 工程

仓库根目录本身就是可以直接打开的 Godot 工程。

1. 用 Godot 4.7 .NET 打开该目录，并构建 C# 项目。
2. 打开 `demo/Demo.tscn`，按 **F6** 运行。画面中的 Guide 标签列出了需要悬停检查的属性。

场景包含六个子节点：`PlaceholderNode`（非 `[Tool]` 脚本）、`ToolNode`（`[Tool]` 脚本）、`Node2DHolder`、`InlineResourceHolder`、用于对照的 `GDScriptCompare`（GDScript），以及 `Guide` 标签。

## 运行检查

```sh
dotnet build InspectorDescriptionDemo.csproj --nologo
godot --headless --path . res://tests/InspectorDescriptionChecks.tscn
```

请将 `godot` 替换为你本机 Godot .NET 编辑器的路径。检查通过时输出 `PASS: inspector description lookup.` 并以退出码 0 结束；失败时输出异常，并以退出码 1 结束。

自动检查只覆盖查找逻辑，不覆盖提示框与标签的渲染。这两项需在编辑器中结合 demo 手动确认。

`.github/workflows/ci.yml` 中的工作流会在 Ubuntu 上使用 Godot 4.7.2 .NET 执行相同的步骤。

## 参与贡献

欢迎提交问题报告和拉取请求。请使用问题报告模板，写明 Godot 与 .NET 版本，并在提交拉取请求前运行上述检查。提交信息遵循 [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/)。

## 更新日志

见 [CHANGELOG.md](CHANGELOG.md)。

## 许可证

MIT，见 [LICENSE](LICENSE)。Copyright (c) 2026 EstellaNines。
