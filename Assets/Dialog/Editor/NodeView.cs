using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class NodeView : UnityEditor.Experimental.GraphView.Node
{
    public Node node;
    public Port input;
    public List<Port> outputs;

    public Action<NodeView> OnNodeSelected;

    public NodeView(Node node)
    {
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

    private void CreateOutputPorts()
    {
        
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
