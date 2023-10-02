using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu()]
public class DialogTree : ScriptableObject
{
    public Node rootNode;
    public List<Node> nodes;

    private void OnEnable()
    {
        if (rootNode == null)
        {
            rootNode = CreateInstance<RootNode>();
        }
    }

    public Node CreateNode(System.Type type)
    {
        Node node = CreateInstance(type) as Node;
        node.name = type.Name;
        node.guid = GUID.Generate().ToString();

        AssetDatabase.AddObjectToAsset(node, this);
        AssetDatabase.SaveAssets();
        nodes.Add(node);

        return node;
    }

    public void DestroyNode(Node node)
    {
        nodes.Remove(node);
        AssetDatabase.RemoveObjectFromAsset(node);
        AssetDatabase.SaveAssets();
    }

    
}
