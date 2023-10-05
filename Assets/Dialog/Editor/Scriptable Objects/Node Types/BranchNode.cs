using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BranchNode : NodeData
{
    [HideInInspector] public List<NodeData> children;
}
