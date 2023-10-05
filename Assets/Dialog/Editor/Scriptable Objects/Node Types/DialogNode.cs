using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct TypingSpeed
{
    [Tooltip("The length of characters the speed is to last for\nInput 0 if it is to last for the entire message")]
    public int charLength;
    public int speed;
}

public enum EventCallType
{
    start,
    end,
}

public struct EventCalls
{
    EventCallType callType;
    string callName;
}

public class DialogNode : NodeData
{
    [HideInInspector]public NodeData child;

    public string speakerName;
    public string speakerMessage;
    public Sprite speakerSprite;

    public TypingSpeed[] typingSpeeds;
    
    public List<EventCalls> events;

}
