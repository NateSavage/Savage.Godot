
using Godot;
using Godot.Collections;

namespace Savage.GD;

#nullable enable

public static class SceneTreeExtensions {
    public static async Task NextFrame(this SceneTree sceneTree) => await sceneTree.ToSignal(sceneTree, SceneTree.SignalName.ProcessFrame);

    public static IEnumerable<T> GetNodesInGroup<T>(this SceneTree sceneTree, string group) where T : Node {
        Array<Node> groupNodes = sceneTree.GetNodesInGroup(group);
        for (int i = 0; i < groupNodes.Count; ++i) {
            if (groupNodes[i] is T match)
                yield return match;
        }
    }

    public static IEnumerable<T> GetNodesInGroup<T>(this SceneTree sceneTree) where T : Node => GetNodesInGroup<T>(sceneTree, typeof(T).Name);
}

#nullable restore