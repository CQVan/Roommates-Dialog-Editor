using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class NodeVisual : Node
{
    public NodeData data;

    public NodeVisual(NodeData data)
    {
        this.data = data;
        title = data.name;

        if(data is RootNode)
        {
            capabilities -= Capabilities.Deletable;
            capabilities -= Capabilities.Copiable;
        }

        GenerateInputs();
        GenerateOutputs();
    }

    private void GenerateOutputs()
    {
        if(data is RootNode || data is DialogNode)
        {
            Port output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
            output.portName = "Output";

            outputContainer.Add(output);
        }
    }

    public override void SetPosition(Rect newPos)
    {
        base.SetPosition(newPos);

        data.position.x = newPos.xMin;
        data.position.y = newPos.yMin;
    }

    private void GenerateInputs()
    {
        if (data is RootNode) return;

        Port input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
        input.portName = "Input";
            
        inputContainer.Add(input);
    }
}
