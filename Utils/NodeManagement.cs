using Godot;

namespace Dim.Utils;

public static class NodeManagement
{
    public static Node FindUniqueNamedNodeEverywhere(Node contextNode, string name)
    {
        return FindUniqueNamedNodeEverywhereRecursive(contextNode.GetTree().Root, name);
    }

    private static Node FindUniqueNamedNodeEverywhereRecursive(Node baseNode, string name)
    {
        if (baseNode.Name == name)
            return baseNode;

        foreach (Node child in baseNode.GetChildren())
        {
            var result = FindUniqueNamedNodeEverywhereRecursive(child, name);
            if (result != null)
                return result;
        }

        return null;
    }
}