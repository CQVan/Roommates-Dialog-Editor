using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoiceNode : Node
{
    [Serializable]
    public struct Choice
    {
        public Node next;
        [HideInInspector] public string guid;
    }

    public Choice[] choices;
}
