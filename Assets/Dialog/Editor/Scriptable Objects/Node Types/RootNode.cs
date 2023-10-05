using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootNode : NodeData
{
    [HideInInspector] public NodeData child;

    public string[] eventCalls;
}
