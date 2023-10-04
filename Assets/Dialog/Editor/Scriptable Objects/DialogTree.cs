using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[CreateAssetMenu()]
public class DialogTree : ScriptableObject
{
    public RootNode root;

    public struct NodeConnectionData
    {
        public NodeData start;
        public string portName;
        public NodeData end;
    }

    public List<NodeConnectionData> connections;
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

    public void CreateConnection(Edge edge)
    {
        NodeVisual startNodeVisual = edge.output.node as NodeVisual;
        NodeVisual endNodeVisual = edge.input.node as NodeVisual;

        NodeConnectionData nodeConnectionData = new NodeConnectionData()
        {
            start = startNodeVisual.data,
            portName = startNodeVisual.name,
            end = endNodeVisual.data
        };

        connections.Add(nodeConnectionData);
    }

    public void DeleteConnection(Edge edge)
    {
        NodeVisual startNodeVisual = edge.output.node as NodeVisual;
        NodeVisual endNodeVisual = edge.input.node as NodeVisual;

        NodeConnectionData nodeConnectionData = new NodeConnectionData()
        {
            start = startNodeVisual.data,
            portName = startNodeVisual.name,
            end = endNodeVisual.data
        };

        connections.Remove(nodeConnectionData);
    }
}
