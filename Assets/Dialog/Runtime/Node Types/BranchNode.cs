using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RDE.Editor.NodeTypes;

namespace RDE.Editor.NodeTypes
{
    public class BranchNode : NodeData
    {
        
        [Tooltip("The Option Text of the branch if connected to another branch as a choice")]
        [HideInInspector]
        public string optionText = "";

        [HideInInspector]
        public List<NodeData> children = new List<NodeData>();

        [HideInInspector]
        public bool showBranchText;

    }

    
}

#if UNITY_EDITOR

[CustomEditor(typeof(BranchNode))]
public class BranchNodeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        BranchNode node = target as BranchNode;

        if (node.showBranchText)
        {
            node.optionText = EditorGUILayout.TextField("Option Text", node.optionText);

        }
    }
}

#endif

