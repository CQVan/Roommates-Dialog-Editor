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
    public Image speakerImage;
    public TextMeshProUGUI textBox;

    public GameObject optionPrefab;

    public List<EventCall> eventCalls;
    public List<DataCall> dataCalls;

    private Queue<NodeData> nodeQueue = new Queue<NodeData>();
    private bool displayingLine = false;
    private DialogTree dialogTree;

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

    public void RunTree(DialogTree tree)
    {

        if(tree.startEventCalls.Length > 0)
        {
            RunStartCalls(tree);
        }

        FillDialogQueue(tree);

        dialogTree = tree;

        DisplayNextLine();
    }

    public void DisplayNextLine()
    {
        if(nodeQueue.Count <= 0)
        {
            EndDialog();
            return;
        }

        if (displayingLine) return;

        NodeData node = nodeQueue.Dequeue();

        if(node is DialogNode dialogNode)
        {
            StartCoroutine(DisplayLine(dialogNode));
        }

        
    }

    private void EndDialog()
    {
        
    }

    

    private void FillDialogQueue(DialogTree tree, DialogNode start = null)
    {
        nodeQueue.Clear();

        NodeData node = (start == null) ? tree.root.child : start;

        while (true)
        {
            if(node is DialogNode dialogNode)
            {

                nodeQueue.Enqueue(dialogNode);
                
                if (dialogNode.child == null)
                    break;

                node = dialogNode.child;

                continue;  
            }

            if(node is BranchNode branchNode)
            {
                if (branchNode.children.Count > 0)
                    nodeQueue.Enqueue(branchNode);

                break;
            }
        }
    }

    public IEnumerator DisplayLine(DialogNode node)
    {

        textBox.text = "";
        displayingLine = true;

        string message = HandleDataCalls(node.speakerMessage);

        float currentTypingSpeed = dialogTree.defaultTypingSpeed;

        if(node.typingSpeeds.Length > 0)
        {

        }
        else
        {
            char[] messageChars = message.ToCharArray();

            for(int i = 0; i < messageChars.Length; i++)
            {
                if (messageChars[i] == ' ')
                {
                    textBox.text += messageChars[i];
                    continue;
                }

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
                yield return new WaitForSeconds(1.0f / currentTypingSpeed);
            }
        }

        displayingLine = false;
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
