using RDE.NodeTypes;
using RDE.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RDE.Runtime
{

    [Serializable]
    public class TypingSpeed
    {
        public int startPoint; // Start of speed change
        public int length; // End of speed change

        public Color color; // Color identifier of speed change

        public float speed; // Speed in characters per second

        public TypingSpeed(int startPoint, int length, float speed)
        {
            this.startPoint = startPoint;
            this.length = length;
            this.speed = speed;

            color = new Color(UnityEngine.Random.Range(0, 255), UnityEngine.Random.Range(0, 255), UnityEngine.Random.Range(0, 255));
        }
    }

}


// Data Calls
//
// ${dataName} is data embeding
// &{eventName} is event calling

namespace RDE.NodeTypes
{
    public class DialogNode : NodeData
    {
        [HideInInspector] public NodeData child;

        [SerializeField]
        public string speakerName;
        [SerializeField]
        public Sprite speakerSprite;

        [TextArea(5,20)]
        [SerializeField]
        public string speakerMessage;

        public List<TypingSpeed> typingSpeeds;

        private void OnValidate()
        {
            foreach(TypingSpeed speed in typingSpeeds)
            {
                string editedMessage = RemoveCalls(speakerMessage);
                if (speed.startPoint > editedMessage.Length)
                {
                    speed.startPoint = editedMessage.Length;
                }
            }
        }

        public static string RemoveCalls(string speakerMessage)
        {
            string editedMessage = speakerMessage;

            //Remove Data Calls
            int startPoint = 0;
            while (editedMessage.Substring(startPoint).Contains("${"))
            {
                int dataCallIndex = editedMessage.IndexOf("${");
                int dataCallEndIndex = editedMessage.Substring(dataCallIndex).IndexOf("}");

                startPoint = dataCallIndex + 2;

                if (dataCallEndIndex != -1)
                {

                    editedMessage = editedMessage.Substring(0, dataCallIndex) + editedMessage.Substring(dataCallIndex + dataCallEndIndex + 1);

                    startPoint = 0;
                }
            }

            //Remove Event Calls
            startPoint = 0;
            while (editedMessage.Substring(startPoint).Contains("&{"))
            {
                int dataCallIndex = editedMessage.IndexOf("&{");
                int dataCallEndIndex = editedMessage.Substring(dataCallIndex).IndexOf("}");

                startPoint = dataCallIndex + 2;

                if (dataCallEndIndex != -1)
                {

                    editedMessage = editedMessage.Substring(0, dataCallIndex) + editedMessage.Substring(dataCallIndex + dataCallEndIndex + 1);

                    startPoint = 0;
                }
            }

            return editedMessage;
        }

    }
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(TypingSpeed))]
public class TypingSpeedPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty startPoint = property.FindPropertyRelative("startPoint");
        SerializedProperty length = property.FindPropertyRelative("length");
        SerializedProperty color = property.FindPropertyRelative("color");
        SerializedProperty speed = property.FindPropertyRelative("speed");

        EditorGUIUtility.labelWidth = 60;
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), new GUIContent("Speed " + label.text.Substring(label.text.IndexOf(" ") + 1)));

        Rect startPointLabelPos = new Rect(position.x, position.y, (position.width / 3.0f) - 5, position.height / 3);
        Rect lengthLabelPos = new Rect(position.x + (position.width / 3.0f) + 5, position.y, (position.width / 3.0f) - 10, position.height / 3);
        Rect speedPosLabelPos = new Rect(position.x + ((position.width / 3.0f) * 2) + 5, position.y, (position.width / 3.0f) - 5, position.height / 3);

        EditorGUI.LabelField(speedPosLabelPos, new GUIContent("Speed"));
        EditorGUI.LabelField(lengthLabelPos, new GUIContent("Length"));
        EditorGUI.LabelField(startPointLabelPos, new GUIContent("Start Point"));

        Rect startPointPos = new Rect(position.x, position.y + position.height/3, (position.width / 3.0f) - 5, position.height/3);
        Rect lengthPos = new Rect(position.x + (position.width / 3.0f) + 5, position.y + position.height/3, (position.width / 3.0f) - 10, position.height/3);
        Rect speedPos = new Rect(position.x + ((position.width / 3.0f) * 2) + 5, position.y + position.height/3, (position.width / 3.0f) - 5, position.height/3);

        EditorGUI.PropertyField(speedPos, speed, GUIContent.none);
        EditorGUI.PropertyField(startPointPos, startPoint, GUIContent.none);
        EditorGUI.PropertyField(lengthPos, length, GUIContent.none);

        EditorGUIUtility.labelWidth = 50;
        Rect colorPos = new Rect(position.x, position.y + (2 * (position.height / 3)), position.width, position.height / 3);
        EditorGUI.PropertyField(colorPos, color);

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return (base.GetPropertyHeight(property, label) * 3);
    }
}

[CustomEditor(typeof(DialogNode))]
public class DialogNodeEditor : Editor
{
    private Vector2 scrollPosition = Vector2.zero;
    private DialogNode node;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        node = (DialogNode)target;

        EditorGUILayout.Space();

        string message = GenerateRichText(node.speakerMessage, SortSpeeds(node.typingSpeeds));

        GUIStyle gUIStyle = new GUIStyle(GUI.skin.label)
        {
            richText = true,
            wordWrap = true,
        };
        EditorGUILayout.LabelField("Type Speed Display", new GUIStyle(GUI.skin.label)
        {
            fontStyle = FontStyle.Bold
        });
        EditorGUILayout.LabelField(message, gUIStyle, GUILayout.MinWidth(0));

    }

    private string GenerateRichText(string speakerMessage, TypingSpeed[] sortedSpeeds)
    {
        if (string.IsNullOrEmpty(speakerMessage)) return "";
        string enrichedMessage = "";

        //remove calls and store their location
        Dictionary<int, string> callLocations = new Dictionary<int, string>();
        string editedMessage = RemoveCalls(speakerMessage, out callLocations);
        //add color tags

        int sectionIndex = 0;
        List<KeyValuePair<int, string>> highlights = new List<KeyValuePair<int, string>>();

        for(int i = 0; i < sortedSpeeds.Length; i++)
        {
            enrichedMessage += editedMessage.Substring(sectionIndex, sortedSpeeds[i].startPoint - sectionIndex);
            
            sectionIndex += sortedSpeeds[i].startPoint - sectionIndex;

            string colorTag = "<color=#" + ColorUtility.ToHtmlStringRGB(sortedSpeeds[i].color) + ">";
            enrichedMessage += colorTag;

            highlights.Add(new KeyValuePair<int, string>(sectionIndex, colorTag));

            int coloredSectionLength = sortedSpeeds[i].length;

            if (i + 1 < sortedSpeeds.Length)
            {
                coloredSectionLength = Math.Min(coloredSectionLength, sortedSpeeds[i + 1].startPoint - sectionIndex);
            }
            
            if(sectionIndex + coloredSectionLength > editedMessage.Length)
                coloredSectionLength = editedMessage.Length - sectionIndex;

            enrichedMessage += editedMessage.Substring(sectionIndex, coloredSectionLength);
            sectionIndex += coloredSectionLength;
            
            enrichedMessage += "</color>";
            highlights.Add(new KeyValuePair<int, string>(sectionIndex, "</color>"));
        }

        if(sectionIndex < editedMessage.Length)
        {
            enrichedMessage += editedMessage.Substring(sectionIndex);
        }

        //re-add calls

        int offset = 0;

        foreach(KeyValuePair<int, string> call in callLocations)
        {

            List<KeyValuePair<int, string>> newHighlights = new List<KeyValuePair<int, string>>();

            foreach (KeyValuePair<int,string> highlight in highlights)
            {
                if(highlight.Key < call.Key)
                {
                    offset += highlight.Value.Length;
                }
                else
                {
                    newHighlights.Add(highlight);
                }
            }
            highlights = newHighlights;
            enrichedMessage = enrichedMessage.Substring(0, call.Key + offset) + call.Value + enrichedMessage.Substring(call.Key + offset);
            offset += call.Value.Length;
        }

        return enrichedMessage;
    }

    public static string RemoveCalls(string speakerMessage, out Dictionary<int, string> callLocations) 
    {
        string editedMessage = speakerMessage;
        callLocations = new Dictionary<int, string>();

        //Remove Data Calls
        int startPoint = 0;
        while(editedMessage.Substring(startPoint).Contains("${"))
        {
            int dataCallIndex = editedMessage.IndexOf("${");
            int dataCallEndIndex = editedMessage.Substring(dataCallIndex).IndexOf("}");

            startPoint = dataCallIndex + 2;

            if(dataCallEndIndex != -1)
            {
                //Log Data Call
                callLocations.Add(dataCallIndex, editedMessage.Substring(dataCallIndex, dataCallEndIndex + 1));

                editedMessage = editedMessage.Substring(0, dataCallIndex) + editedMessage.Substring( dataCallIndex + dataCallEndIndex + 1);

                startPoint = 0;
            }
        }

        //Remove Event Calls
        startPoint = 0;
        while (editedMessage.Substring(startPoint).Contains("&{"))
        {
            int dataCallIndex = editedMessage.IndexOf("&{");
            int dataCallEndIndex = editedMessage.Substring(dataCallIndex).IndexOf("}");

            startPoint = dataCallIndex + 2;

            if (dataCallEndIndex != -1)
            {
                //Log Event Call
                callLocations.Add(dataCallIndex, editedMessage.Substring(dataCallIndex, dataCallEndIndex + 1));

                editedMessage = editedMessage.Substring(0, dataCallIndex) + editedMessage.Substring(dataCallIndex + dataCallEndIndex + 1);

                startPoint = 0;
            }
        } 
         
        return editedMessage;
    }

    private TypingSpeed[] SortSpeeds(List<TypingSpeed> typingSpeeds)
    {
        TypingSpeed[] sortedSpeeds = typingSpeeds.ToArray();

        for (int i = 0; i < sortedSpeeds.Length; i++)
        {
            TypingSpeed key = sortedSpeeds[i];
            int j = i - 1;

            while (j >= 0 && sortedSpeeds[j].startPoint > key.startPoint)
            {
                sortedSpeeds[j + 1] = sortedSpeeds[j];
                j = j - 1;
            }
            sortedSpeeds[j + 1] = key;
        }

        return sortedSpeeds;
    }


}

#endif


