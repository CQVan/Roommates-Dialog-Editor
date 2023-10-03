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
    public new class UxmlFactory : UxmlFactory<DialogTreeView, UxmlTraits> { }

    DialogTree tree;
    public Action<NodeView> OnNodeSelected;

    public DialogTreeView()
    {
        Insert(0, new GridBackground());

        this.AddManipulator(new ContentZoomer());
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Dialog/Editor/Resources/DialogTreeEditor.uss");
        styleSheets.Add(styleSheet);
    }

    internal void PopulateView(DialogTree tree)
    {
        this.tree = tree;

        graphViewChanged -= OnGraphViewChanged;
        DeleteElements(graphElements);
        graphViewChanged += OnGraphViewChanged;

        CreateNodeView(tree.rootNode);

        foreach (Node node in tree.nodes)
        {
            CreateNodeView(node);
        }

        NodeView rootView = FindNodeView(tree.rootNode);
        NodeView nextView = FindNodeView(tree.rootNode.next);

        Edge edge = rootView.outputs.ToArray()[0].ConnectTo(nextView.input);
        AddElement(edge);

        foreach (Node node in tree.nodes)
        {

        }
    }

    NodeView FindNodeView(Node node)
    {
        return GetNodeByGuid(node.guid) as NodeView;
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports.ToList().Where(endPort => 
        endPort != startPort && 
        endPort.direction != startPort.direction && 
        endPort.node != startPort.node).ToList();
    }

    private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
    {
        if(graphViewChange.elementsToRemove != null)
        {
            foreach (var elem in graphViewChange.elementsToRemove)
            {
                if (elem is NodeView nodeView)
                {
                    tree.DestroyNode(nodeView.node);
                }


                if (elem is Edge edge)
                {
                    NodeView parentView = edge.output.node as NodeView;

                    if (parentView.node is RootNode rootNode)
                    {
                        rootNode.next = null;
                        parentView.node = rootNode;
                    }

                    if (parentView.node is DialogNode dialogNode)
                    {
                        dialogNode.next = null;
                        parentView.node = dialogNode;
                    }

                    if (parentView.node is ChoiceNode choiceNode)
                    {
                        ChoiceNode.Choice[] nodeChoices = choiceNode.choices;

                        for (int i = 0; i < nodeChoices.Length; i++)
                        {
                            Debug.Log(edge.output.name.ToString());

                            if (edge.output.name.Equals(nodeChoices[i].guid))
                            {
                                nodeChoices[i].next = null;
                            }
                        }
                    }
                }
            }
            
        }

        if(graphViewChange.edgesToCreate != null)
        {
            foreach(var edge  in graphViewChange.edgesToCreate)
            {
                NodeView parentView = edge.output.node as NodeView;
                NodeView childView = edge.input.node as NodeView;

                if (parentView.node is RootNode rootNode)
                {
                    rootNode.next = childView.node;
                    parentView.node = rootNode;
                }

                if (parentView.node is DialogNode dialogNode)
                {
                    dialogNode.next = childView.node;
                    parentView.node = dialogNode;
                }

                if(parentView.node is ChoiceNode choiceNode)
                {
                    ChoiceNode.Choice[] nodeChoices = choiceNode.choices;

                    for (int i = 0; i < nodeChoices.Length; i++)
                    {
                        Debug.Log(edge.output.name.ToString());

                        if(edge.output.name.Equals(nodeChoices[i].guid))
                        {
                            nodeChoices[i].next = childView.node;
                        }
                    }
                }
            }
        }


        return graphViewChange;
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        if(evt.target.GetType() == typeof(DialogTreeView))
        {
            var types = TypeCache.GetTypesDerivedFrom<Node>();
            foreach (var type in types)
            {

                var pos = evt.localMousePosition;

                evt.menu.AppendAction(type.Name, a => CreateNode(type, pos));
            }
        }

        evt.menu.AppendSeparator();

        base.BuildContextualMenu(evt);
    }

    void CreateNode(System.Type type, Vector2 position)
    {
        Node node = tree.CreateNode(type);
        node.position = position;

        CreateNodeView(node);
    }

    void CreateNodeView(Node node)
    {
        NodeView nodeView = new NodeView(node);
        nodeView.OnNodeSelected = OnNodeSelected;
        AddElement(nodeView);
    }
}
