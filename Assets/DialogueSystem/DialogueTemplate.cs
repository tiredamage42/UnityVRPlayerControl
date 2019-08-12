using System.Collections.Generic;
using UnityEngine;

using ActorSystem;
using InventorySystem;
using QuestSystem;

#if UNITY_EDITOR 
using UnityEditor;
#endif

namespace DialogueSystem {

    /*
        Dialogue templates will be held on gameobjects/prefabs

        in order to give them the capability to reference objects in the scene

        in case dialogue triggers :
            enabling disabling
            quest only in scene

        current system assumes that only player will have template player component...
            who else could be choosing responses?

        so ::
            self game value checks refer to player actor values
            supplied game value checks refer to the actor the player is talking to
     
    */
   
    [System.Serializable] public class DialogueResponseArray : NeatArrayWrapper<DialogueResponse> { }
    [System.Serializable] public class DialogueResponse {
        [TextArea] public string bark;
        public float barkTime = 1;
        public AudioClip associatedAudio;
        public int nextDialogueStepID = -1;
        [NeatArray] public GameValueConditionArray conditions;
        public DialogueResponse() {
            nextDialogueStepID = -1;
            bark = "";
            barkTime = 1;
        }
    }
        
    [System.Serializable] public class DialogueStep {
        public int stepID;
        [TextArea] public string bark;
        public float barkTime = 1;
        public AudioClip associatedAudio;
        [Header("Dialogue ends if no responses")] [NeatArray] public DialogueResponseArray responses;
        [Header("Add Buffs")] [NeatArray] public GameValueModifierArray speakerBuffs;
        [NeatArray] public GameValueModifierArray playerBuffs;
        [Header("Inventory Swap")] [NeatArray] public ItemCompositionArray transferToSpeaker;
        [NeatArray] public ItemCompositionArray transferToPlayer;
        [Header("Start Inventory Management")] public string contextName;
        [NeatArray] public NeatIntList categoryFilter;
        [Header("Quests")] public Quest startQuest;
    }

    public class DialogueTemplate : MonoBehaviour
    {
        public DialogueStep[] allSteps;
    }

#if UNITY_EDITOR 

    [CustomPropertyDrawer(typeof(DialogueResponse))] public class DialogueResponseDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float singleLineHeight = EditorGUIUtility.singleLineHeight;
            GUIContent noContent = GUIContent.none;
            EditorGUI.BeginProperty(position, label, property);

            int oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            float x = EditorTools.DrawIndent (oldIndent, position.x);
            float y = position.y;

            EditorGUI.PropertyField(new Rect(x, y, position.width * .5f, singleLineHeight), property.FindPropertyRelative("associatedAudio"), GUIContent.none);
            
            GUIContent barkDurationLBL = new GUIContent("Duration: ");
            float lableWidth = EditorStyles.label.CalcSize(barkDurationLBL).x;
            EditorGUI.LabelField(new Rect(x + position.width * .5f, y, lableWidth, singleLineHeight), barkDurationLBL);

            EditorGUI.PropertyField(new Rect(x + position.width * .5f + lableWidth, y, 125, singleLineHeight), property.FindPropertyRelative("barkTime"), GUIContent.none);

            SerializedProperty barkProp = property.FindPropertyRelative("bark");
            string s = barkProp.stringValue;
            if (string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s)) barkProp.stringValue = DialogueTemplateEditor.emptyResponsePrompt;
            
            EditorGUI.PropertyField(new Rect(x, y, position.width, singleLineHeight * 3), barkProp, noContent);
            y += singleLineHeight * 3;
                    
            SerializedProperty nextStepIDProp = property.FindPropertyRelative("nextDialogueStepID");

            string[] allNames;
            List<int> allIDs = DialogueTemplateEditor.instance.GetAllIDs (out allNames);

            int currentIDIndex = -1;
            for (int i = 0; i < allIDs.Count; i++) {
                if (allIDs[i] == nextStepIDProp.intValue) {
                    currentIDIndex = i;
                    break;
                }
            }

            GUI.enabled = nextStepIDProp.intValue != -1;

            bool wentForward = false;
            if (GUI.Button(new Rect(x, y, 64, singleLineHeight), "Next Step:", EditorStyles.miniButton)) {

                DialogueTemplateEditor.instance.GoForwardStep (nextStepIDProp.intValue, barkProp.stringValue);
                wentForward = true;
            }
            GUI.enabled = true;

            if (!wentForward) {

                float w = position.width;
                int newIDindex = EditorGUI.Popup(new Rect(x + 70, y, position.width * .5f, singleLineHeight), "", currentIDIndex, allNames);
                
                if (newIDindex >= 0 && newIDindex < allIDs.Count) {
                    //dont allow self...
                    if (newIDindex == DialogueTemplateEditor.instance.currentStepID) {
                        Debug.LogWarning("Response cant lead to current step...");
                        if (nextStepIDProp.intValue == newIDindex) nextStepIDProp.intValue = -1;
                    }
                    else {                    
                        nextStepIDProp.intValue = allIDs[newIDindex];
                    }
                }
                else if (newIDindex == allIDs.Count) {
                    Debug.Log("Adding new step");
                    nextStepIDProp.intValue = DialogueTemplateEditor.instance.AddStepBark();
                }
                else if (newIDindex < 0) {
                    nextStepIDProp.intValue = -1;
                }
            }

            EditorGUI.indentLevel = oldIndent + 1;
            SerializedProperty conditionsProp = property.FindPropertyRelative("conditions");
            EditorGUI.PropertyField(new Rect(position.x, y + singleLineHeight, position.width, (EditorGUI.GetPropertyHeight(conditionsProp, true))), conditionsProp, new GUIContent("Conditions"));
            EditorGUI.indentLevel = oldIndent;
            
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 4 + (EditorGUI.GetPropertyHeight(property.FindPropertyRelative("conditions"), true));
        }
    }

    [CustomEditor(typeof(DialogueTemplate))] public class DialogueTemplateEditor : Editor
    {   
        public static DialogueTemplateEditor instance;
        
        SerializedProperty allStepsProp;

        public List<int> GetAllIDs (out string[] allNames) {
            List<int> r = new List<int>();
            List<string> names = new List<string>();
            for (int i = 0; i < allStepsProp.arraySize; i++) {
                SerializedProperty stepProp = allStepsProp.GetArrayElementAtIndex(i);
                int id = stepProp.FindPropertyRelative("stepID").intValue;
                r.Add(id);
                names.Add("[" + id + "] " + stepProp.FindPropertyRelative("bark").stringValue);   
            }
            names.Add("New Step...");
            allNames = names.ToArray();
            return r;
        }

        const int maxTries = 999;
        int GetUnusedID () {
            int tryID = 0;
            bool foundID = false;
            while (!foundID && tryID <= maxTries) {
                foundID = true;
                for (int i = 0; i < allStepsProp.arraySize; i++) {
                    if (allStepsProp.GetArrayElementAtIndex(i).FindPropertyRelative("stepID").intValue == tryID) {
                        foundID = false;
                        tryID++;
                        break;
                    }
                }
            }
            return tryID >= maxTries ? -1 : tryID;
        }

        public int AddStepBark () {
            int unusedID = GetUnusedID();
            if (unusedID == -1) {
                Debug.LogWarning("max steps reached");
                return unusedID;
            }
            int index = allStepsProp.arraySize;
            allStepsProp.InsertArrayElementAtIndex(index);
            ResetStepProp(allStepsProp.GetArrayElementAtIndex(index), unusedID);
            return unusedID;
        }

        void OnEnable () {
            instance = this;
            allStepsProp = serializedObject.FindProperty("allSteps");
            InventorySystemEditorUtils.UpdateItemSelector();
        }

        void OnDisable () {
            instance = null;
        }
        void OnDestroy () {
            instance = null;
        }

        public int currentStepID;
        List<int> previousStepIDs = new List<int>();
        List<string> previousBarks = new List<string>();
        string convoSoFar;

        void CalcConvoSoFar () {
            convoSoFar = "";
            for (int i = 0; i < previousBarks.Count; i++) convoSoFar += previousBarks[i] + "\n";
        }

        SerializedProperty ID2StepProp (int id) {

            for (int i = 0; i < allStepsProp.arraySize; i++) {
                SerializedProperty stepProp = allStepsProp.GetArrayElementAtIndex(i);
                if (stepProp.FindPropertyRelative("stepID").intValue == id) {
                    return stepProp;
                }
            }
            return null;
        }

        public void GoForwardStep (int newID, string fromResponseBark) {
            SerializedProperty currentStepIDProp = ID2StepProp(currentStepID);
            string currentBark = currentStepIDProp.FindPropertyRelative("bark").stringValue;
            previousBarks.Add(currentBark + " >> " + fromResponseBark);
            previousStepIDs.Add(currentStepID);
            currentStepID = newID;
            CalcConvoSoFar();
        }
        public void GoBackStep () {
            int lastIndex = previousStepIDs.Count - 1;
            currentStepID = previousStepIDs[lastIndex];
            previousStepIDs.RemoveAt(lastIndex);
            previousBarks.RemoveAt(lastIndex);
            CalcConvoSoFar();
        }

        public void DeleteStep () {
            for (int i = allStepsProp.arraySize -1; i >= 0; i--) {
                SerializedProperty stepProp = allStepsProp.GetArrayElementAtIndex(i);
                if (stepProp.FindPropertyRelative("stepID").intValue == currentStepID) {
                    allStepsProp.DeleteArrayElementAtIndex(i);
                    return;
                }
            }
            GoBackStep();
        }

        void ClearNeatArray(SerializedProperty array) {
            array = array.FindPropertyRelative("list");
            for (int i = array.arraySize -1; i >= 0; i--) {
                // if(array.GetArrayElementAtIndex(i).objectReferenceValue != null) array.DeleteArrayElementAtIndex(i);
                array.DeleteArrayElementAtIndex(i);
            }
        }
            
        void ResetStepProp (SerializedProperty stepProp, int newID) {
            stepProp.FindPropertyRelative("stepID").intValue = newID;
            stepProp.FindPropertyRelative("bark").stringValue = "[ Dialogue Step: " + newID + " ]";
            stepProp.FindPropertyRelative("barkTime").floatValue = 1;
            
            stepProp.FindPropertyRelative("associatedAudio").objectReferenceValue = null;

            ClearNeatArray(stepProp.FindPropertyRelative("responses"));
            ClearNeatArray(stepProp.FindPropertyRelative("speakerBuffs"));
            ClearNeatArray(stepProp.FindPropertyRelative("playerBuffs"));
            ClearNeatArray(stepProp.FindPropertyRelative("transferToSpeaker"));
            ClearNeatArray(stepProp.FindPropertyRelative("transferToPlayer"));
            ClearNeatArray(stepProp.FindPropertyRelative("categoryFilter"));
            
            stepProp.FindPropertyRelative("contextName").stringValue = "";
            
            stepProp.FindPropertyRelative("startQuest").objectReferenceValue = null;
        }

        public const string emptyResponsePromptCheck = "[ Response ";
        public const string emptyResponsePrompt = emptyResponsePromptCheck + "]";

        void DrawDialogueStep(SerializedProperty stepProp) {
            if (stepProp == null)
                return;

            EditorGUILayout.PropertyField (stepProp.FindPropertyRelative("bark"));
            EditorGUILayout.PropertyField (stepProp.FindPropertyRelative("associatedAudio"), GUIContent.none);
            EditorGUILayout.PropertyField (stepProp.FindPropertyRelative("barkTime"));

            SerializedProperty responseProp = stepProp.FindPropertyRelative("responses");
            EditorGUILayout.PropertyField (responseProp);
            
            responseProp = responseProp.FindPropertyRelative("list");
            for (int i = 0; i < responseProp.arraySize; i++) {
                SerializedProperty barkProp = responseProp.GetArrayElementAtIndex(i).FindPropertyRelative("bark");
                if (barkProp.stringValue.StartsWith(emptyResponsePromptCheck)) barkProp.stringValue = emptyResponsePromptCheck + i + " ]";
            }

            EditorGUILayout.PropertyField (stepProp.FindPropertyRelative("speakerBuffs"));
            EditorGUILayout.PropertyField (stepProp.FindPropertyRelative("playerBuffs"));

            EditorGUILayout.PropertyField (stepProp.FindPropertyRelative("transferToSpeaker"), new GUIContent("Transfer To Speaker", "If any specified,\nresponses leading to this dialogue step will not be available if the player inventory doesnt have the requred items..."));
            EditorGUILayout.PropertyField (stepProp.FindPropertyRelative("transferToPlayer"));

            EditorGUILayout.PropertyField (stepProp.FindPropertyRelative("contextName"));            
            EditorGUILayout.PropertyField (stepProp.FindPropertyRelative("categoryFilter"));

            EditorGUILayout.PropertyField (stepProp.FindPropertyRelative("startQuest"));
        }

        static readonly Color32 deleteColor = new Color32(200,75,75,255);

        public override void OnInspectorGUI () {
            // base.OnInspectorGUI();

            int stepsCount = allStepsProp.arraySize;
            if (stepsCount == 0 && GUILayout.Button("Add First Dialogue Step")) AddStepBark();
            
            if (stepsCount > 0) {
                // EditorGUILayout.Space();
                // EditorGUILayout.Space();
                
                EditorGUILayout.HelpBox("Current system assumes that only player will have template player component...\n\nself game value checks refer to player actor values\nsupplied game value checks refer to the actor the player is talking to", MessageType.Info);
                
                GUI.enabled = previousBarks.Count > 0;
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(" Back Step ")) GoBackStep();
                GUI.backgroundColor = deleteColor;
                if (GUILayout.Button(" Delete Step ")) DeleteStep();
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.HelpBox(previousBarks.Count > 0 ? convoSoFar : "Base Dialogue Step", MessageType.Info);
                GUI.enabled = true;
                
                DrawDialogueStep (ID2StepProp(currentStepID));
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }

#endif

}