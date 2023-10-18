using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RDE.Editor.NodeTypes
{
    public class BranchNode : NodeData
    {
        //[HideInInspector]
        public List<NodeData> children = new List<NodeData>();
    }
}

