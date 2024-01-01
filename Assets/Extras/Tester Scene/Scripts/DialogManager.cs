using RDE.NodeTypes;
using RDE.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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

        if (tree.startEventCalls.Length > 0)
        {
            RunStartCalls(tree);
        }

        FillDialogQueue(tree, start);

        dialogTree = tree;

        DisplayNextLine();
    }

    public void DisplayNextLine()
    {
        if (nodeQueue.Count <= 0)
        {
            EndDialog();
            return;
        }

        if (displayingLine) return;

        NodeData node = nodeQueue.Dequeue();

        if (node is DialogNode dialogNode)
        {
            StartCoroutine(DisplayLine(dialogNode));
        }

        if (node is BranchNode branchNode)
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
            if (node is DialogNode dialogNode)
            {
                DialogOptionButton dialogOption = Instantiate(optionPrefab, tfOptions).GetComponent<DialogOptionButton>();
                dialogOption.optionText.text = dialogNode.speakerMessage;
                dialogOption.button.onClick.AddListener(() => RunTree(dialogTree, dialogNode));

            } else if (node is BranchNode branchNodeChild)
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

        if (dialogTree.endEventCalls.Length > 0)
            foreach (string callName in dialogTree.endEventCalls)
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
            if (node is DialogNode dialogNode)
            {

                nodeQueue.Enqueue(dialogNode);

                if (dialogNode.child == null)
                    break;

                node = dialogNode.child;

                continue;
            }

            if (node is BranchNode branchNode)
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

        Queue<MessageSection> sections = new Queue<MessageSection>();

        foreach (MessageSection section in node.GetMessageSections(dialogTree.defaultTypingSpeed))
            sections.Enqueue(section);

        
        while (sections.Count > 0)
        {
            MessageSection currentSection = sections.Dequeue();

            Dictionary<int, string> eventCalls = new Dictionary<int, string>();

            string calledSection = HandleEventCalls(HandleDataCalls(currentSection.section), out eventCalls);
            char[] chars = calledSection.ToCharArray();

            for (int i = 0; i < chars.Length; i++)
            {
                bool foundEventCall = false;

                foreach(KeyValuePair<int, string> pair in eventCalls)
                {
                    Debug.Log(pair.ToString() + chars.Length);

                    if(pair.Key == i)
                    {
                        EventCallbacks.Invoke(pair.Value);

                        foundEventCall = true;
                    }
                }

                if (foundEventCall) continue;

                yield return new WaitForSeconds(1.0f / currentSection.speed.speed);
                textBox.text += chars[i];
            }
        }

        displayingLine = false; 
    }

    public void TestCallBack(string callback)
    {
        Debug.Log(callback);
    }

    private string HandleEventCalls(string section, out Dictionary<int, string> eventCalls)
    {
        string editedSection = section;
        eventCalls = new Dictionary<int, string>();

        //Remove Event Calls
        int startPoint = 0;
        while (editedSection.Substring(startPoint).Contains("&{"))
        {
            int dataCallIndex = editedSection.IndexOf("&{");
            int dataCallEndIndex = editedSection.Substring(dataCallIndex).IndexOf("}");

            startPoint = dataCallIndex + 2;

            if (dataCallEndIndex != -1)
            {
                eventCalls.Add(dataCallIndex, editedSection.Substring(2 + dataCallIndex, dataCallEndIndex - 2));
                editedSection = editedSection.Substring(0, dataCallIndex) + " " + editedSection.Substring(dataCallIndex + dataCallEndIndex + 1);

                startPoint = 0;
            }
        }

        return editedSection;
    }

    private string HandleDataCalls(string section)
    {

        int callStart = section.IndexOf("${");
        

        if(callStart == -1)
        {
            return section;
        }

        int callEnd = section[callStart..].IndexOf("}") + callStart-2;

        string callName = section.Substring(callStart + 2, callEnd - callStart);

        string message = section.Substring(0, callStart);

        foreach (DataCall dataCall in dataCalls)
        {
            if (dataCall.dataName.Equals(callName))
            {
                message += dataCall.callback.Invoke();
            }
        }

        message += section.Substring(callEnd+3);

        return HandleDataCalls(message);
    }
}
