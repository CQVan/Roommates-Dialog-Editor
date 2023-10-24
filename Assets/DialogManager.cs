using RDE.Editor.NodeTypes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public struct EventCall
{
    public string eventName;
    public UnityEvent function;
}

public struct DataCall
{
    public string dataName;
    public UnityEvent getter;
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
                    call.function.Invoke();
                }
            }
        }
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

        while(node.speakerMessage.IndexOf("$(") != -1)
        {

        }

        int currentTypingSpeed = tree.defaultTypingSpeed;

        if(node.typingSpeeds.Length > 0)
        {

        }
        else //uses default typing speed
        {
            



        }
    }
}
