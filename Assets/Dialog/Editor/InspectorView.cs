using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using RDE.Editor.NodeTypes;
using RDE.Editor;

public class InspectorView : VisualElement
{
    public new class UxmlFactory : UxmlFactory<InspectorView, UxmlTraits> { };

    Editor editor;

    public InspectorView()
    {

    }


    internal void UpdateSelection(NodeVisual node)
    {
        Clear();

        NodeData nodeData = node.data;

        UnityEngine.Object.DestroyImmediate(editor);

        editor = Editor.CreateEditor(nodeData);

        IMGUIContainer container = new IMGUIContainer(() => 
        {
            if (editor.target)
            {
                editor.OnInspectorGUI();
            }
                
        });
        Add(container);
    }
    
}
