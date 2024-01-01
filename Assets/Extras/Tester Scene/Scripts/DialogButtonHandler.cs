using RDE.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogButtonHandler : MonoBehaviour
{
    public Button startButton;
    public Button continueButton;

    public DialogTree tree;

    private void Awake()
    {
        continueButton.onClick.AddListener(FindObjectOfType<DialogManager>().DisplayNextLine);
        startButton.onClick.AddListener(() => { FindObjectOfType<DialogManager>().RunTree(tree); });
    }
}
