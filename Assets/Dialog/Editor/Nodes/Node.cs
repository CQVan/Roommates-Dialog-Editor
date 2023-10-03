using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum DialogCallbacks
{
    start,
    end
}

[Serializable]
public abstract class Node : ScriptableObject
{
    [HideInInspector]public string guid;
    [HideInInspector]public Vector2 position;

    public string speakerName;
    public string speakerMessage;
    public Sprite speakerImage;

    [Serializable]
    public struct TypingSpeed
    {
        [Tooltip("How many characters the speed is applied to \nSet to -1 if it's for the rest of the message")]
        public int characterLength;
        [Tooltip("Characters per Second")]
        public int speed;
    }

    public TypingSpeed[] typingSpeeds;

    [Serializable]
    public struct EventCall
    {
        public DialogCallbacks callbackType;
        public string callbackName;
    }

    public EventCall[] eventCallbacks;
}
