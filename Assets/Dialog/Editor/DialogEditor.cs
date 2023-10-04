using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogEditor : EditorWindow
{

    DialogTreeView treeView;
    InspectorView inspectorView;

    [MenuItem("Window/Dialog Editor")]
    public static void OpenWindow()
    {
        DialogEditor wnd = GetWindow<DialogEditor>();
        wnd.titleContent = new GUIContent("DialogEditor");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Instantiate UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Dialog/Editor/Resources/DialogEditor.uxml");
        visualTree.CloneTree(root);

        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Dialog/Editor/Resources/DialogEditor.uss");
        root.styleSheets.Add(styleSheet);

        treeView = root.Q<DialogTreeView>();
        inspectorView = root.Q<InspectorView>();

        Toolbar toolbar = root.Q<Toolbar>();

        //dialogMenu.menu.AppendAction("Create Dialog Node", );
    }

    private void OnSelectionChange()
    {
       DialogTree tree = Selection.activeObject as DialogTree;

        if (tree != null && AssetDatabase.CanOpenAssetInEditor(tree.GetInstanceID()))
        {
            treeView.PopulateView(tree);
        }
    }
}
