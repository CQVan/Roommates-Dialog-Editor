using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Permissions;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class NodeView : UnityEditor.Experimental.GraphView.Node
{
    public Node node;
    public Port input;
    public List<Port> outputs;

    public Action<NodeView> OnNodeSelected;

    ChoiceNode.Choice[] choices;

    public NodeView(Node node)
    {
        
        

        if(node is ChoiceNode choiceNode)
        {
            EditorApplication.update += ChoiceNodeUpdate;
            choices = choiceNode.choices;
        }


        this.node = node;
        title = node.name;

        style.left = node.position.x;
        style.top = node.position.y;

        CreateInputPorts();
        CreateOutputPorts();

        if(node is RootNode)
        {
            title = "Start";
            capabilities -= Capabilities.Deletable;
            capabilities -= Capabilities.Copiable;
        }
    }

    public void ChoiceNodeUpdate()
    {
        ChoiceNode choiceNode = node as ChoiceNode;

        if(choiceNode.choices.Equals(choices)) { return; }

        choices = choiceNode.choices;


        outputContainer.Clear();
        CreateOutputPorts();
    }

    private void CreateOutputPorts()
    {
        if(node is RootNode)
        {
            Port port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(string));
            port.portName = "Next";

            outputContainer.Add(port);
        }

        if (node is DialogNode)
        {
            Port port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(string));
            port.portName = "Next";

            outputContainer.Add(port);
        }

        if (node is ChoiceNode choiceNode)
        {
            ChoiceNode.Choice[] nodeChoices = choiceNode.choices;

            for (int i = 0; i < nodeChoices.Length; i++)
            {
                Port port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(string));

                port.name = GUID.Generate().ToString();
                nodeChoices[i].guid = port.name;

                port.portName = "Choice " + (i);

                outputContainer.Add(port);
            }
                
        }
    }

    private void CreateInputPorts()
    {
        if (node is RootNode) return;

        input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));

        if(input != null)
        {
            input.portName = "Input";

            inputContainer.Add(input);
        }
    }

    public override void SetPosition(Rect newPos)
    {
        base.SetPosition(newPos);
        node.position.x = newPos.xMin;
        node.position.y = newPos.yMin;
    }

    public override void OnSelected()
    {
        base.OnSelected();

        OnNodeSelected?.Invoke(this);

    }

    
}
