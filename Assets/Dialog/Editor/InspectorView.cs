using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

public class InspectorView : VisualElement
{
    public new class UxmlFactory : UxmlFactory<InspectorView, UxmlTraits> { }

    Editor editor;

    public InspectorView()
    {

    }

    public void UpdateSelection(NodeView nodeView)
    {
        Debug.Log("help please");

        Clear();

        UnityEngine.Object.DestroyImmediate(editor);

        editor = Editor.CreateEditor(nodeView.node);
        Debug.Log(editor);
        IMGUIContainer container = new IMGUIContainer(() => { editor.OnInspectorGUI();  });
        Add(container);


        
    }

    
}
