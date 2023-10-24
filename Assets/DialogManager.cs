using RDE.Editor.NodeTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    public TextMeshProUGUI textBox;

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

    public string testCallback(string input)
    {
        return input;
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

        textBox.text = "";

        string message = HandleDataCalls(node.speakerMessage);

        int currentTypingSpeed = tree.defaultTypingSpeed;

        if(node.typingSpeeds.Length > 0)
        {

        }
        else
        {
            char[] messageChars = message.ToCharArray();

            for(int i = 0; i < messageChars.Length; i++)
            {
                if (messageChars[i] == '&' && messageChars[i + 1] == '{')
                {
                    if (message.Substring(i).IndexOf('}') != -1)
                    {
                        string callName = message.Substring(i + 2, message.Substring(i).IndexOf('}') - 2);

                        foreach (EventCall call in eventCalls)
                        {
                            if(call.eventName == callName)
                            {
                                i = message.IndexOf('}') + 1;

                                if (i >= messageChars.Length) yield break;

                                call.eventCallback.Invoke();
                            }
                        }
                    }


                }

                textBox.text += messageChars[i];
                yield return new WaitForSeconds(1 / currentTypingSpeed);
            }
        }
    }

    private string HandleDataCalls(string speakerMessage)
    {

        int callStart = speakerMessage.IndexOf("${");
        

        if(callStart == -1)
        {
            return speakerMessage;
        }

        int callEnd = speakerMessage[callStart..].IndexOf("}") + callStart-2;

        string callName = speakerMessage.Substring(callStart + 2, callEnd - callStart);

        string message = speakerMessage.Substring(0, callStart);

        foreach (DataCall dataCall in dataCalls)
        {
            if (dataCall.dataName.Equals(callName))
            {
                message += dataCall.callback.Invoke();
            }
        }

        message += speakerMessage.Substring(callEnd+3);

        return HandleDataCalls(message);
    }
}
