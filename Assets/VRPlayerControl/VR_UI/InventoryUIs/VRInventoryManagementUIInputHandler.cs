using UnityEngine;

using GameUI;
using SimpleUI;
using Valve.VR;
using InventorySystem;
using System.Collections.Generic;

#if UNITY_EDITOR 
using UnityEditor;
#endif

namespace VRPlayer.UI {

    [System.Serializable] public class UIInputControlArray : NeatArrayWrapper<VRInventoryManagementUIInputHandler.UIInputControl> { }
    public class VRInventoryManagementUIInputHandler : MonoBehaviour 
    {
        [System.Serializable] public class UIInputControl {
            public SteamVR_Action_Boolean action;
            public bool handDependent;
        }

        [Header("Order Corresponds To Context Inputs:")]
        [NeatArray] public UIInputControlArray controls;
        public TransformBehavior equipBehavior;
        public string context;

        [Header("Input uses only one Hand")]
        public bool usesSingleHand;
        public int enforcedEquipID = -1;


        bool UIShouldOpenCheck (object[] parameters){//Inventory inventory, int usingEquipPoint, Inventory otherInventory, List<int> categoryFilter) {
            int usingEquipPoint = (int)parameters[1];
            return enforcedEquipID == -1 || usingEquipPoint == enforcedEquipID;            
        }
        bool UIShouldCloseCheck (object[] parameters){//Inventory inventory, int usingEquipPoint) {
            int usingEquipPoint = (int)parameters[1];
            return enforcedEquipID == -1 || usingEquipPoint == enforcedEquipID;
        }


        protected UIHandler myUIHandler;
        
        bool needsSingleHandInput { get { return usesSingleHand || enforcedEquipID != -1; } }

            
        
        // Vector2Int GetUIInputs () {
        // }
        Vector2Int GetUIInputs (){//int equipID, SteamVR_Input_Sources hand) {
            // bool usesID = usesSingleHand || enforcedEquipID != -1;// ||  myUIHandler.EquipIDSpecific();
            int equipID = needsSingleHandInput ? workingWithEquipID : 0;
            SteamVR_Input_Sources hand = needsSingleHandInput ? VRManager.Int2Hand( equipID ) : SteamVR_Input_Sources.Any;
            // return GetUIInputs(equipID, hand);
            
            for (int i = 0; i < controls.list.Length; i++) {
                if (controls.list[i].handDependent) {
                    StandardizedVRInput.ButtonState[] buttonStates;
                    StandardizedVRInput.instance.GetInputActionInfo(controls.list[i].action, out buttonStates);
                    for (int b =0 ; b < buttonStates.Length; b++) {
                        if (buttonStates[b] == StandardizedVRInput.ButtonState.Down) {
                            return new Vector2Int(i, b);
                        }
                    }

                }
                else {
                    if (controls.list[i].action.GetStateDown(hand)) 
                        return new Vector2Int(i, equipID);
                }
            }
            return new Vector2Int(-1, equipID);        
        }


        // string[] actionNames;

        protected virtual void OnEnable () {

            myUIHandler = UIHandler.GetUIHandlerByContext(context);

            if (myUIHandler != null) {
                // actionNames = myUIHandler.GetInputNames();
                myUIHandler.onUIClose += OnCloseUI;
                myUIHandler.onUIOpen += OnOpenUI;
                myUIHandler.SetUIInputCallback(GetUIInputs);

                myUIHandler.shouldCloseCheck = UIShouldCloseCheck;
                myUIHandler.shouldOpenCheck = UIShouldOpenCheck;
                
            }
        }

        protected virtual void OnDisable () {
            if (myUIHandler != null) {
                myUIHandler.onUIClose -= OnCloseUI;
                myUIHandler.onUIOpen -= OnOpenUI;
            }
        }

// new object[] { inventory, usingEquipPoint, otherInventory, context, categoryFilter }


        int workingWithEquipID;
        void OnOpenUI (GameObject uiObject, object[] parameters) {
        // void OnOpenUI (UIElementHolder uiObject) {

            workingWithEquipID = (int)parameters[1];
        
            // SteamVR_Input_Sources hand = myUIHandler.EquipIDSpecific() ? VRManager.Int2Hand( myUIHandler.workingWithEquipID ) : SteamVR_Input_Sources.Any;

            SteamVR_Input_Sources hand = needsSingleHandInput ? VRManager.Int2Hand( workingWithEquipID ) : SteamVR_Input_Sources.Any;

            string[] inputNames = myUIHandler.inputNames;
            for (int i = 0; i < controls.list.Length; i++) {
                StandardizedVRInput.MarkActionOccupied(controls.list[i].action, hand);
                StandardizedVRInput.instance.ShowHint(hand, controls.list[i].action, inputNames[i]);
            }     

            if (equipBehavior != null) {
                // TransformBehavior.AdjustTransform(uiObject.baseObject.transform, Player.instance.GetHand(hand).transform, equipBehavior, 0);
                TransformBehavior.AdjustTransform(uiObject.transform, Player.instance.GetHand(hand).transform, equipBehavior, 0);
            }

            VRUIInput.SetUIHand(hand);
        }

        void OnCloseUI (GameObject uiObject) {
        // void OnCloseUI (UIElementHolder uiObject) {
        
            for (int i = 0; i < controls.list.Length; i++) {
                StandardizedVRInput.MarkActionUnoccupied(controls.list[i].action);
                StandardizedVRInput.instance.HideHint(SteamVR_Input_Sources.Any, controls.list[i].action);    
            }     
        }
    }


#if UNITY_EDITOR 
    [CustomPropertyDrawer(typeof(VRInventoryManagementUIInputHandler.UIInputControl))] public class VRInventoryManagementUIInputHandlerUIInputControlDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            float singleLineHeight = EditorGUIUtility.singleLineHeight;
            GUIContent noContent = GUIContent.none;
            EditorGUI.BeginProperty(position, label, property);

            int oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            float x = EditorTools.DrawIndent (oldIndent, position.x);

            EditorGUI.PropertyField(new Rect(x, position.y, 0, singleLineHeight), property.FindPropertyRelative("action"), GUIContent.none);
            
            EditorGUI.LabelField(new Rect(x, position.y, 100, singleLineHeight), new GUIContent("Hand Dependent:"));
            EditorGUI.PropertyField(new Rect(x + 100, position.y, 32, singleLineHeight), property.FindPropertyRelative("handDependent"), GUIContent.none);
            
            EditorGUI.indentLevel = oldIndent;
            
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }

    [CustomEditor(typeof(VRInventoryManagementUIInputHandler))]
    public class VRInventoryManagementUIInputHandlerEditor : Editor {
        VRInventoryManagementUIInputHandler uiHandler;
        void OnEnable () {
            uiHandler = target as VRInventoryManagementUIInputHandler;
        }
        public override void OnInspectorGUI () {
            string[] actionNames = UIHandler.GetHandlerInputNames(uiHandler.context);
            if (actionNames != null) {
                EditorGUILayout.HelpBox("Actions:\n" + string.Join(", ", actionNames) , MessageType.Info);
            }
            else {
                EditorGUILayout.HelpBox("No context found :\n" + uiHandler.context, MessageType.Error);
            }
            base.OnInspectorGUI();
        }
    }

#endif
}

