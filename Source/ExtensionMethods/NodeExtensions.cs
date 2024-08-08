using System.Runtime.CompilerServices;
using Godot;
using Godot.Collections;

namespace Savage.GD;

#nullable enable

public static class NodeExtensions {
    /// <summary> Searches up the scene tree for the first parent node of the matching type. </summary>
    public static bool TryGetAncestor<T>(this Node node, out T? matchingAncestor) where T : Node {
        Node ancestor = node;

        do {
            ancestor = ancestor.GetParent();
            if (ancestor is not T match)
                continue;
            matchingAncestor = match;
            return true;
        } while (ancestor is null == false);

        matchingAncestor = null;
        return false;
    }

    /// <summary>
    /// Returns the first direct child of the desired type if one is present under this node. <br/>
    /// Otherwise, returns null.
    /// </summary>
    public static T? GetChild<T>(this Node node) where T : Node {
        foreach (var child in node.GetChildren()) {
            if (child is T desiredType)
                return desiredType;
        }

        return null;
    }

    /// <summary> Creates a new list containing all the direct children of the desired type. </summary>
    public static List<T> GetChildren<T>(this Node node) where T : Node {
        Array<Node> children = node.GetChildren();
        var typedChildren = new List<T>(children.Count);

        for (int i = 0; i < children.Count; ++i)
            if (children[i] is T match)
                typedChildren.Add(match);

        return typedChildren;
    }

    /// <summary> Adds the Node to a group with the name of the specified type. </summary>
    public static void AddToTypeGroup<T>(this Node node) => node.AddToGroup(typeof(T).Name);

    public static void AddChildDeferred(this Node node, Node child) => node.CallDeferred(Node.MethodName.AddChild, child);

    public static void RemoveChildDeferred(this Node node, Node child) => node.CallDeferred(Node.MethodName.RemoveChild, child);

    public static void QueueFreeDeferred(this Node node) => node.CallDeferred(Node.MethodName.QueueFree);

    public static void LogIfComponentMissing(this Node? node, [CallerMemberName] string caller = "") {
        if (node is not null)
            return;

    #if SAVAGE_LOGS
        Log.Error($"missing component {caller}");
    #else
        Godot.GD.PrintErr($"missing component {caller}");
    #endif
    }
}

#nullable restore