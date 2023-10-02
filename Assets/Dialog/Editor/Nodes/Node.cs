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

public abstract class Node : ScriptableObject
{
    [HideInInspector]public string guid;
    [HideInInspector]public Vector2 position;

    public string speakerName;
    public string speakerMessage;
    public Sprite speakerImage;

    public Dictionary<int, int> typingSpeeds;
    public Dictionary<DialogCallbacks, string> eventCalls;
}
