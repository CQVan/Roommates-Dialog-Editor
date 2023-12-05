using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RDE.NodeTypes
{
    public class RootNode : NodeData
    {
        [HideInInspector] public NodeData child;
    }
}
