using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class NodeVisual : Node
{
    public NodeData data;
    public Action<NodeVisual> OnNodeSelected;

    public Port input;
    public Port output;

    public NodeVisual (NodeData data)
    {
        this.data = data;
        title = data.name;
        viewDataKey = data.guid;

        style.left = data.position.x;
        style.top = data.position.y;

        GenerateInput();
        GenerateOutput();
    }

    public override void SetPosition(Rect newPos)
    {
        base.SetPosition(newPos);
        data.position.x = newPos.xMin;
        data.position.y = newPos.yMin;
    }

    public override void OnSelected()
    {
        base.OnSelected();

        OnNodeSelected?.Invoke(this);
    }

    private void GenerateInput()
    {
        if (data is RootNode) return;

        input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
        input.portName = "Input";

        inputContainer.Add(input);
    }

    private void GenerateOutput()
    {
        if(data is BranchNode)
        {
            output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));
            output.portName = "Choices";

            outputContainer.Add(output);
            return;
        }

        output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
        output.portName = "Next";

        outputContainer.Add(output);
    }
}
