using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

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

        UnityEngine.Object.DestroyImmediate(editor);

        editor = Editor.CreateEditor(node.data);
        IMGUIContainer container = new IMGUIContainer(() => 
        { 
            if(editor.target)
                editor.OnInspectorGUI();
        });
        Add(container);
    }

    
}
