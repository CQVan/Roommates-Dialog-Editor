using System.Collections;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static Unity.VisualScripting.Metadata;

[Serializable]
public struct NodeConnectionData
{
    public NodeData start;
    public string portName;
    public NodeData end;
}

[CreateAssetMenu()]
public class DialogTree : ScriptableObject
{
    public RootNode root;

    public List<NodeData> nodes;

    public NodeData CreateNode(System.Type type)
    {
        NodeData node = CreateInstance(type) as NodeData;
        node.name = type.Name;
        node.guid = GUID.Generate().ToString();
        nodes.Add(node);

        AssetDatabase.AddObjectToAsset(node, this);
        AssetDatabase.SaveAssets();

        return node;
    }

    public void DeleteNode(NodeData node)
    {
        nodes.Remove(node);
        AssetDatabase.RemoveObjectFromAsset(node);
        AssetDatabase.SaveAssets();
    }

    public void AddChild(NodeData parent, NodeData child)
    {
        if (parent is RootNode rootNode)
        {
            rootNode.child = child;
        }

        if (parent is DialogNode dialogNode)
        {
            dialogNode.child = child;
        }

        if (parent is BranchNode branchNode)
        {
            branchNode.children.Add(child);
        }
    }
    
    public void RemoveChild(NodeData parent, NodeData child)
    {
        if (parent is RootNode rootNode)
        {
            rootNode.child = null;
        }

        if (parent is DialogNode dialogNode)
        {
            dialogNode.child = null;
        }

        if (parent is BranchNode branchNode)
        {
            branchNode.children.Remove(child);
        }
    }

    public List<NodeData> GetChildren(NodeData parent)
    {
        List<NodeData> children = new List<NodeData>();

        if (parent is RootNode rootNode)
        {
            children.Add(rootNode.child);
        }

        if (parent is DialogNode dialogNode)
        {
            children.Add(dialogNode.child);
        }

        if(parent is BranchNode branchNode)
        {
            children.AddRange(branchNode.children);
        }

        return children;
    }
}
