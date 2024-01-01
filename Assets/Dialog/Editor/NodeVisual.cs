using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using RDE.NodeTypes;

namespace RDE.Editor
{
    [Serializable]
    public class NodeVisual : Node
    {
        public NodeData data;
        public Action<NodeVisual> OnNodeSelected;

        public Port input;
        public Port output;

        public NodeVisual (NodeData data)
        {
            this.data = data;
            title = data.name;
            viewDataKey = data.guid;

            style.left = data.position.x;
            style.top = data.position.y;

            AddDataBindings(data);

            GenerateInput();
            GenerateOutput();

            RefreshExpandedState();
            RefreshPorts(); 
        }

        private void AddDataBindings(NodeData data)
        {

            if(data is DialogNode dialogNode)
            {
                SerializedObject serializedDialogNode = new SerializedObject(dialogNode);

                extensionContainer.style.flexDirection = FlexDirection.Column;

                TextField speakerMessageDisplay = new TextField("Message");
                speakerMessageDisplay.multiline = true;
                speakerMessageDisplay.labelElement.style.minWidth = 15;

                extensionContainer.Add(speakerMessageDisplay);
                speakerMessageDisplay.BindProperty(serializedDialogNode.FindProperty("speakerMessage"));

                speakerMessageDisplay.style.paddingBottom = 3;
                speakerMessageDisplay.style.paddingLeft = 3;
                speakerMessageDisplay.style.paddingRight = 3;
                speakerMessageDisplay.style.paddingTop = 3;
                speakerMessageDisplay.style.textOverflow = TextOverflow.Ellipsis;
                speakerMessageDisplay.style.maxWidth = 150;
            }
            else if(data is BranchNode branchNode)
            {
                if(branchNode.showBranchText)
                {
                    SerializedObject serializedBranchNode = new SerializedObject(branchNode);

                    extensionContainer.style.flexDirection = FlexDirection.Column;

                    TextField speakerMessageDisplay = new TextField("Option Text");
                    speakerMessageDisplay.multiline = true;
                    speakerMessageDisplay.labelElement.style.minWidth = 15;

                    extensionContainer.Add(speakerMessageDisplay);
                    speakerMessageDisplay.BindProperty(serializedBranchNode.FindProperty("optionText"));

                    speakerMessageDisplay.style.paddingBottom = 3;
                    speakerMessageDisplay.style.paddingLeft = 3;
                    speakerMessageDisplay.style.paddingRight = 3;
                    speakerMessageDisplay.style.paddingTop = 3;
                    speakerMessageDisplay.style.textOverflow = TextOverflow.Ellipsis;
                }

            }
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            Undo.RecordObject(data, "Moved " + data.name);
            data.position.x = newPos.xMin;
            data.position.y = newPos.yMin;
        
            EditorUtility.SetDirty(data);
        }

        public override void OnSelected()
        {
            base.OnSelected();

            OnNodeSelected?.Invoke(this);
        }

        private void GenerateInput()
        {
            if (data is RootNode) return;

            input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
            input.portName = "Input";

            inputContainer.Add(input);
        }

        private void GenerateOutput()
        {
            if(data is BranchNode)
            {
                output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));
                output.portName = "Choices";

                outputContainer.Add(output);
                return;
            }

            output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
            output.portName = "Next";

            outputContainer.Add(output);
        }
    }

}
