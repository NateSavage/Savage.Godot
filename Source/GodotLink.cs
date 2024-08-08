using Godot;

namespace Savage.GD;

/// <summary> A singleton node you can autoload so classes not inheriting from any Godot type can communicate with the engine more easily. </summary>
public partial class GodotLink : Node {
    public static SceneTree SceneTree { get; private set; }

    /// <summary> Root node of the full scene tree. </summary>
    public static Node Root { get; private set; }

    /// <summary> Invoked when the Godot engine calls <see cref="_Process(double)"/>. </summary>
    public static event Action<double> Process;

    /// <summary> Invoked when the Godot engine calls <see cref="_PhysicsProcess(double)"/>. </summary>
    public static event Action<double> PhysicsProcess;

    public override void _Ready() {
        Root = GetNode("/root");
        SceneTree = Root.GetTree();
    }

    public override void _Process(double delta) {
        Process?.Invoke(delta);
    }

    public override void _PhysicsProcess(double delta) {
        PhysicsProcess?.Invoke(delta);
    }
}