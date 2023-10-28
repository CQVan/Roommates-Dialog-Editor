using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RDE.Editor.NodeTypes
{
    public class BranchNode : NodeData
    {
        
        [Tooltip("The Option Text of the branch if connected to another branch as a choice")]
        public string branchText = "";

        [HideInInspector]
        public List<NodeData> children = new List<NodeData>();

        
    }
}

