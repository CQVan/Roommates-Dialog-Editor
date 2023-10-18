using RDE.Editor.NodeTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogTreeView : GraphView
{
    public new class UxmlFactory : UxmlFactory<DialogTreeView, UxmlTraits> { };
    public Action<NodeVisual> OnNodeSelected;

    public DialogTree tree;

    public DialogTreeView()
    {
        Insert(0, new GridBackground());

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new ContentZoomer());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Dialog/Editor/Resources/DialogEditor.uss");
        styleSheets.Add(styleSheet);

        Undo.undoRedoPerformed += OnUndoRedo;
    }

    private void OnUndoRedo()
    {
        //Debug.Log(tree);
        
        if(tree == null) return;
        
        PopulateView(tree);
        AssetDatabase.SaveAssets();
        PopulateView(tree);
    }

    internal void PopulateView(DialogTree tree)
    {

        this.tree = tree;

        graphViewChanged -= OnGraphViewChanged;
        DeleteElements(graphElements);
        graphViewChanged += OnGraphViewChanged;

        foreach (NodeData nodeData in tree.nodes)
        {
            CreateNodeVisual(nodeData);
        }
        
        if(tree.root == null)
        {
            tree.root = CreateNode(typeof(RootNode), Vector2.zero) as RootNode;
        }

        foreach(NodeVisual visuals in graphElements)
        {
            Debug.Log(visuals.viewDataKey  == visuals.data.guid);
        }

        LoadEdges(tree.nodes);
        
    }

    private void LoadEdges(List<NodeData> nodes)
    {
        foreach(NodeData node in nodes)
        {
            List<NodeData> children = tree.GetChildren(node);

            foreach(NodeData child in children)
            {
                
                if (child == null) continue;
                
                NodeVisual parentVisual = GetNodeByGuid(node);
                NodeVisual childVisual = GetNodeByGuid(child);

                Edge edge = parentVisual.output.ConnectTo(childVisual.input);
                AddElement(edge);
            }
        }
    }

    public NodeVisual GetNodeByGuid (NodeData data)
    {
        return GetNodeByGuid(data.guid) as NodeVisual;
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports.ToList().Where(endPort => endPort != startPort && endPort.direction != startPort.direction && endPort.node != startPort.node).ToList();
    }

    private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
    {
        if(graphViewChange.elementsToRemove != null)
        {
            RemoveNodes(graphViewChange.elementsToRemove);
            RemoveEdges(graphViewChange.elementsToRemove);
        }

        if(graphViewChange.edgesToCreate != null)
        {
            CreateEdges(graphViewChange.edgesToCreate);
        }

        return graphViewChange;
    }

    private void RemoveEdges(List<GraphElement> elementsToRemove)
    {
        List<Edge> edgesToRemove = new List<Edge>();

        foreach (GraphElement element in elementsToRemove)
        {
            if (element is Edge edge) edgesToRemove.Add(edge);
        }
            

        foreach(Edge edge in edgesToRemove)
        {
            NodeVisual parent = edge.output.node as NodeVisual;
            NodeVisual child = edge.input.node as NodeVisual;

            tree.RemoveChild(parent.data, child.data);
        }
    }

    private void CreateEdges(List<Edge> edgesToCreate)
    {

        foreach (Edge edge in edgesToCreate)
        {

            NodeVisual parent = edge.output.node as NodeVisual;
            NodeVisual child = edge.input.node as NodeVisual;

            tree.AddChild(parent.data, child.data);
        }
    }

    private void RemoveNodes(List<GraphElement> elementsToRemove)
    {
        foreach (VisualElement elem in elementsToRemove)
        {
            if(elem is NodeVisual nodeVisual)
            {
                tree.DeleteNode(nodeVisual.data);
            }
        }
    }

    private void CreateNodeVisual(NodeData nodeData)
    {
        NodeVisual nodeVisual = new(nodeData);
        nodeVisual.OnNodeSelected = OnNodeSelected;
        AddElement(nodeVisual);

    }

    private NodeData CreateNode(System.Type type, Vector2 position)
    {
        NodeData nodeData = tree.CreateNode(type);
        nodeData.position = position;

        CreateNodeVisual(nodeData);
        
        return nodeData;
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        if(evt.currentTarget is DialogTreeView)
        {

            var types = TypeCache.GetTypesDerivedFrom<NodeData>();

            foreach (var type in types)
            {
                if (type == typeof(RootNode)) continue;

                var pos = viewTransform.matrix.inverse.MultiplyPoint(evt.localMousePosition);
                evt.menu.AppendAction("Create " + type.Name, (a) => CreateNode(type, pos));
            }

            evt.menu.AppendSeparator();

        }

        base.BuildContextualMenu(evt);
    }

}
