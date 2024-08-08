using System.Runtime.CompilerServices;
using Godot;

namespace Savage;

#nullable  enable

/// <summary>
/// Declares that this object can create Nodes of type <see cref="T"/> with a special constructor for passing arguments in when instanced as a Godot node. <br/>
/// <br/>
/// Before <see cref="Godot.Node._EnterTree"/> runs, <see cref="InitializeData"/> will be called with your Data arguments. <br/>
/// <br/>
/// </summary>
public interface ISpecialConstruction<out T, in TInitializationData> where T : Node {
    // ReSharper disable once StaticMemberInGenericType
    protected static readonly string ScenePath = GetScenePath();

    private static string GetScenePath() {
        // we only want to fetch the script path from the final derived type
        var pathAttribute = (ScriptPathAttribute?)Attribute.GetCustomAttribute(typeof(T), typeof(ScriptPathAttribute), inherit: false);
        string scriptPath = pathAttribute?.Path ?? "";
        return string.Intern($"{scriptPath.Substring(0, scriptPath.Length - 3)}.tscn");
    }

    public void InitializeData(TInitializationData data);

    /// <summary>
    /// Creates a new instance of this node using your arguments. <br/>
    /// When <paramref name="parent"/> is not null, the node will be added to the scene under that parent immediately, otherwise you'll need to add it to the scene yourself. 
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static abstract T Create(TInitializationData? data, Node? parent = null, string? name = null);

    /// <inheritdoc cref="Create"/>
    public static T InstanceAsNode(TInitializationData? data, Node? parent, string? name) {
        // Nodes are always promised to have a parameterless constructor so this is safe
        dynamic node = Activator.CreateInstance<T>();
        if (name is not null) node.Name = name;
        node.InitializeData(data);
        parent?.AddChild(node);
        return (T)node;
    }

    public static T InstanceAsScene(TInitializationData? data, Node? parent, string? name) {
    #if DEBUG
        if (ResourceLoader.Exists(ScenePath) is false) {
            Godot.GD.PrintErr($"cannot find scene file associated with node type {typeof(T).Name} at path '{ScenePath}' are your script and scene not in the same folder?");
            return null!;
        }
    #endif
        dynamic node = ResourceLoader.Load<PackedScene>(ScenePath).Instantiate<T>();
        if (name is not null) node.Name = name;
        node.InitializeData(data);
        parent?.AddChild(node);
        return (T)node;
    }
}


#nullable restore