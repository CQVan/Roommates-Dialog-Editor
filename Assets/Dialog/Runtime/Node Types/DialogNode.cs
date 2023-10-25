using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct TypingSpeed
{
    [Tooltip("The length of characters the speed is to last for\nInput 0 if it is to last for the entire message")]
    public int charLength;
    [Tooltip("The speed in which the characters are typed out\nCharacters per second")]
    public float speed;
}

// Data Calls
//
// ${dataName} is data embeding
// &{eventName} is event calling

namespace RDE.Editor.NodeTypes
{
    public class DialogNode : NodeData
    {
        [HideInInspector] public NodeData child;

        public string speakerName;
        public Sprite speakerSprite;

        [TextArea(5,20)]
        public string speakerMessage;

        public TypingSpeed[] typingSpeeds;

    }
}


