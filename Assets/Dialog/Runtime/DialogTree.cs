using System.Collections;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using RDE.NodeTypes;

namespace RDE.Runtime
{
    [CreateAssetMenu()]
    public class DialogTree : ScriptableObject
    {
        [HideInInspector]public RootNode root;

        [HideInInspector]public List<NodeData> nodes;

        [HideInInspector] public int group = -1;

        public int defaultTypingSpeed = 25;

        public string[] startEventCalls;

        public string[] endEventCalls;

    #if UNITY_EDITOR

        public NodeData CreateNode(System.Type type)
        {
            NodeData node = CreateInstance(type) as NodeData;
            node.name = type.Name;
            node.guid = GUID.Generate().ToString();

            Undo.RecordObject(this, "Created " + node.name);

            nodes.Add(node);

            AssetDatabase.AddObjectToAsset(node, this);
            Undo.RegisterCreatedObjectUndo(node, "Created " + node.name);

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
            Undo.RecordObject(parent, "Connected Nodes");

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

                if(child is BranchNode branchNodeChild)
                {
                    branchNodeChild.showBranchText = true;
                }
            }
        
            EditorUtility.SetDirty(parent);
        }
    
        public void RemoveChild(NodeData parent, NodeData child)
        {
            Undo.RecordObject(parent, "Disconnected Nodes");

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

                if (child is BranchNode branchNodeChild)
                {
                    branchNodeChild.showBranchText = false;
                    EditorUtility.SetDirty(child);
                }
            }

            EditorUtility.SetDirty(parent);
        
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

    #endif
    }

}




