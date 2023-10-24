using RDE.Editor.NodeTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[Serializable]
public struct EventCall
{
    public string eventName;
    public UnityEvent eventCallback;
}

[Serializable]
public class DataCallback : SerializableCallback<string> { }

[Serializable]
public struct DataCall
{
    public string dataName;
    public DataCallback callback;
}

public class DialogManager : MonoBehaviour
{
    public DialogTree tree;

    public Image speakerImage;
    public Text textBox;

    public GameObject optionPrefab;

    public List<EventCall> eventCalls;
    public List<DataCall> dataCalls;

    private void Start()
    {
        RunTree(tree);
    }

    public void RunStartCalls(DialogTree tree)
    {
        foreach (string callName in tree.startEventCalls)
        {
            foreach (EventCall call in eventCalls)
            {
                if (call.eventName == callName)
                {
                    call.eventCallback.Invoke();
                }
            }
        }
    }

    public string testCallback()
    {
        return "dfajsd;lfaj";
    }

    private void RunTree(DialogTree tree)
    {
        if(tree.startEventCalls.Length > 0)
        {
            RunStartCalls(tree);
        }

        RootNode root = tree.root;

        if(root.child is DialogNode dialogNode)
        {
            StartCoroutine(DisplayLine(dialogNode, tree));
        }
    }

    public IEnumerator DisplayLine(DialogNode node, DialogTree tree)
    {

        string message = HandleDataCalls(node.speakerMessage);

        int currentTypingSpeed = tree.defaultTypingSpeed;

        yield return new WaitForSeconds(currentTypingSpeed);
    }

    private string HandleDataCalls(string speakerMessage)
    {

        int callStart = speakerMessage.IndexOf("${");
        int callEnd = speakerMessage.IndexOf("}");

        if(callStart == -1 || callEnd == -1)
        {
            return speakerMessage;
        }

        string callName = speakerMessage.Substring(callStart + 2, callEnd - callStart - 2);
        string message = speakerMessage.Substring(0, callStart);

        foreach (DataCall dataCall in dataCalls)
        {
            if (dataCall.dataName.Equals(callName))
            {
                message += dataCall.callback.Invoke();
            }
        }

        message += speakerMessage.Substring(callEnd + 1);

        return HandleDataCalls(message);
    }
}
