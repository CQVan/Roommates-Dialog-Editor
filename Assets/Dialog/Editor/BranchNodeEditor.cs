using RDE.Editor.NodeTypes;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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
