using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RDE.Editor.NodeTypes
{
    public class RootNode : NodeData
    {
        [HideInInspector] public NodeData child;

        public string[] eventCalls;
    }
}
