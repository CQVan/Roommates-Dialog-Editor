using RDE.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RDE.NodeTypes
{
    public class NodeData : ScriptableObject
    {
        [HideInInspector] 
        public string guid;

        [HideInInspector] public Vector2 position;

        [HideInInspector]
        public DialogTree tree;
    }
}
