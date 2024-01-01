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

    public struct MessageSection
    {
        public MessageSection(TypingSpeed speed, string section)
        {
            this.speed = speed;
            this.section = section;
        }

        public TypingSpeed speed;
        public string section;

        public override string ToString()
        {
            return "speed: " + speed.speed + " | section:" + section;
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

        [TextArea(5, 20)]
        [SerializeField]
        public string speakerMessage;

        public List<TypingSpeed> typingSpeeds;

        private void OnValidate()
        {
            foreach (TypingSpeed speed in typingSpeeds)
            {
                string editedMessage = RemoveCalls(speakerMessage, true);

                if (speed.startPoint > editedMessage.Length)
                {
                    speed.startPoint = editedMessage.Length;
                }
            }

        }

        public static Dictionary<int, string> GetCallLocations(string speakerMessage)
        {
            if(String.IsNullOrEmpty(speakerMessage)) return new Dictionary<int, string>();

            string editedMessage = speakerMessage;
            Dictionary<int, string> callLocations = new Dictionary<int, string>();

            //Remove Data Calls
            int startPoint = 0;
            while (editedMessage.Substring(startPoint).Contains("${"))
            {
                int dataCallIndex = editedMessage.IndexOf("${");
                int dataCallEndIndex = editedMessage.Substring(dataCallIndex).IndexOf("}");

                startPoint = dataCallIndex + 2;

                if (dataCallEndIndex != -1)
                {
                    //Add Data Call to Dictionary
                    callLocations.Add(dataCallIndex, editedMessage.Substring(dataCallIndex, dataCallEndIndex + 1));

                    editedMessage = editedMessage.Substring(0, dataCallIndex) + editedMessage.Substring(dataCallIndex + dataCallEndIndex);

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
                    //Add Event Call to Dictionary
                    callLocations.Add(dataCallIndex, editedMessage.Substring(dataCallIndex, dataCallEndIndex + 1));

                    editedMessage = editedMessage.Substring(0, dataCallIndex) + editedMessage.Substring(dataCallIndex + dataCallEndIndex);

                    startPoint = 0;
                }
            }

            return callLocations;
        }

        public static string RemoveCalls(string speakerMessage, bool replaceWithSpace = false)
        {
            if (String.IsNullOrEmpty(speakerMessage)) return "";

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

                    editedMessage = editedMessage.Substring(0, dataCallIndex) + (replaceWithSpace ? " " : "") + editedMessage.Substring(dataCallIndex + dataCallEndIndex + 1);

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

                    editedMessage = editedMessage.Substring(0, dataCallIndex) + (replaceWithSpace ? " " : "") + editedMessage.Substring(dataCallIndex + dataCallEndIndex + 1);

                    startPoint = 0;
                }
            }

            return editedMessage;
        }

        public List<MessageSection> GetMessageSections(int defaultSpeed)
        {
            List<MessageSection> sections = new List<MessageSection>();
            TypingSpeed[] speeds = SortSpeeds(typingSpeeds);
            string editedMessage = RemoveCalls(speakerMessage, true);
            callLocations = GetCallLocations(speakerMessage);

            int sectionIndex = 0;

            for (int i = 0; i < speeds.Length; i++)
            {
                //Before next section
                if (speeds[i].startPoint - sectionIndex != 0)
                {
                    TypingSpeed sectionSpeed = new TypingSpeed(sectionIndex, speeds[i].startPoint - sectionIndex, defaultSpeed);

                    sections.Add(new MessageSection(sectionSpeed, GenerateSectionString(sectionIndex, speeds[i].startPoint - sectionIndex)));

                    sectionIndex += speeds[i].startPoint - sectionIndex;
                }

                
                //Checks for colored section for edge cases
                int coloredSectionLength = speeds[i].length;

                if (i + 1 < speeds.Length)
                {
                    coloredSectionLength = Math.Min(coloredSectionLength, speeds[i + 1].startPoint - sectionIndex);
                }

                if (sectionIndex + coloredSectionLength > editedMessage.Length)
                    coloredSectionLength = editedMessage.Length - sectionIndex;

                // Apply Section
                if(coloredSectionLength != 0)
                {
                    sections.Add(new MessageSection(speeds[i], GenerateSectionString(sectionIndex, coloredSectionLength)));
                    sectionIndex += coloredSectionLength;
                }


            }

            if (sectionIndex < editedMessage.Length)
            {
                TypingSpeed sectionSpeed = new TypingSpeed(sectionIndex, editedMessage.Length - sectionIndex, defaultSpeed);

                sections.Add(new MessageSection(sectionSpeed, GenerateSectionString(sectionIndex, editedMessage.Length - sectionIndex)));
            }

            return sections;
        } 

        public TypingSpeed[] SortSpeeds(List<TypingSpeed> typingSpeeds)
        {
            if (typingSpeeds == null)
            {
                return new TypingSpeed[0];
            }

            TypingSpeed[] array = typingSpeeds.ToArray();
            int size = array.Length;

            for (int step = 1; step < size; step++)
            {
                TypingSpeed key = array[step];
                int j = step - 1;

                // Compare key with each element on the left of it until an element smaller than
                // it is found.
                // For descending order, change key<array[j] to key>array[j].
                while (j >= 0 && key.startPoint < array[j].startPoint)
                {
                    array[j + 1] = array[j];
                    --j;
                }

                // Place key at after the element just smaller than it.
                array[j + 1] = key;
            }

            return array;
        }

        Dictionary<int, string> callLocations = new Dictionary<int, string>();
        public string GenerateSectionString(int start, int length) 
        {

            List<int> usedCalls = new List<int>();
            List<string> section = new List<string>();

            foreach(char cr in RemoveCalls(speakerMessage, true).Substring(start, length).ToCharArray())
            {
                section.Add("" + cr);
            }
            section.Add("");

            foreach (KeyValuePair<int, string> call in callLocations)
            {

                if(start <= call.Key && call.Key <= length + start)
                {
                    section[call.Key - start] = call.Value;
                    usedCalls.Add(call.Key);
                }
            }

            foreach(int call in usedCalls)
            {
                callLocations.Remove(call);
            }

            return String.Join("", section);
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

        string message = GenerateRichText();

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

    private string GenerateRichText()
    {
        string richText = "";

        foreach (MessageSection section in node.GetMessageSections(node.tree.defaultTypingSpeed))
        {
            richText += "<color=#" + ColorUtility.ToHtmlStringRGB(section.speed.color).ToString() + ">";
            richText += section.section;
            richText += "</color>";
        }

        return richText;
    }

}

#endif 


 