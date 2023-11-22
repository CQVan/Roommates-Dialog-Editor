using RDE.Editor;
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
public class DataCallback : SerializableCallback<string> { }

[Serializable]
public struct DataCall
{
    public string dataName;
    public DataCallback callback;
}

public class DialogManager : MonoBehaviour
{
    [Header("UI")]
    public Image speakerImage;
    public GameObject dialogTextContainer;
    public TextMeshProUGUI textBox;
    public TextMeshProUGUI nameBox;

    public GameObject optionPrefab;
    public Transform tfOptions;

    [Header("Callbacks")]
    public UnityEvent<string> EventCallbacks;
    public List<DataCall> dataCalls;

    private Queue<NodeData> nodeQueue = new Queue<NodeData>();
    private bool displayingLine = false;
    private DialogTree dialogTree;

    public void RunStartCalls(DialogTree tree)
    {
        foreach (string callName in tree.startEventCalls)
        {
            EventCallbacks?.Invoke(callName);
        }
    }

    public string testCallback(string input)
    {
        return input;
    }

    public void RunTree(DialogTree tree, NodeData start = null)
    {

        if(tree.startEventCalls.Length > 0)
        {
            RunStartCalls(tree);
        }

        FillDialogQueue(tree, start);

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

        if(node is BranchNode branchNode)
        {
            DisplayBranchOptions(branchNode);
        }
    }

    private void DisplayBranchOptions(BranchNode branchNode)
    {
        dialogTextContainer.SetActive(false);
        ClearOptions();

        foreach (NodeData node in branchNode.children)
        {
            if(node is DialogNode dialogNode)
            {
                DialogOptionButton dialogOption = Instantiate(optionPrefab, tfOptions).GetComponent<DialogOptionButton>();
                dialogOption.optionText.text = dialogNode.speakerMessage;
                dialogOption.button.onClick.AddListener(() => RunTree(dialogTree, dialogNode));

            }else if(node is BranchNode branchNodeChild)
            {
                DialogOptionButton dialogOption = Instantiate(optionPrefab, tfOptions).GetComponent<DialogOptionButton>();
                dialogOption.optionText.text = branchNodeChild.optionText;
                dialogOption.button.onClick.AddListener(() => RunTree(dialogTree, branchNodeChild));
            }
        }
    }

    private void EndDialog()
    {
        textBox.text = "";

        foreach(string callName in dialogTree.endEventCalls)
        {
            EventCallbacks?.Invoke(callName);
        }
    }

    

    private void FillDialogQueue(DialogTree tree, NodeData start = null)
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

    private void ClearOptions()
    {
        for (var i = tfOptions.childCount - 1; i >= 0; i--)
        {
            Destroy(tfOptions.GetChild(i).gameObject);
        }
    }

    public IEnumerator DisplayLine(DialogNode node)
    {
        ClearOptions();

        speakerImage.sprite = node.speakerSprite;

        dialogTextContainer.SetActive(true);
        textBox.text = "";
        nameBox.text = node.speakerName;
        displayingLine = true;

        Queue<TypingSpeed> speeds = new Queue<TypingSpeed>();
        foreach(TypingSpeed speed in node.typingSpeeds)
            speeds.Enqueue(speed);

        string message = HandleDataCalls(node.speakerMessage);

        TypingSpeed firstSpeed = speeds.Dequeue();
        float currentTypingSpeed = firstSpeed.speed;
        int currentCharLength = 0;

        char[] messageChars = message.ToCharArray();

        for(int i = 0; i < messageChars.Length; i++)
        {

            if (messageChars[i] == '&' && messageChars[i + 1] == '{')
            {
                if (message.Substring(i).IndexOf('}') != -1)
                {
                    string callName = message.Substring(i + 2, message.Substring(i).IndexOf('}') - 2);


                    i = message.IndexOf('}') + 1;

                    EventCallbacks?.Invoke(callName);

                    if (i >= messageChars.Length)
                    {
                        displayingLine = false;
                        yield break;
                    }

                }


            }

            textBox.text += messageChars[i];
            currentCharLength--;
            yield return new WaitForSeconds(1.0f / currentTypingSpeed);

            if(currentCharLength == 0)
            {
                TypingSpeed nextSpeed = speeds.Dequeue();
                currentTypingSpeed = nextSpeed.speed;
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
