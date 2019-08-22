﻿using UnityEngine;

using Valve.VR;
using Game.UI;

using System.Collections.Generic;

#if UNITY_EDITOR 
using UnityEditor;
#endif

namespace VRPlayer.UI {

    [System.Serializable] public class UIInputControlArray : NeatArrayWrapper<UIInputControl> { }
    [System.Serializable] public class UIInputControl {
        public SteamVR_Action_Boolean action;
        public bool handDependent;
    }
    public class VRUIInputHandler : MonoBehaviour 
    {

        [Header("Order Corresponds To Context Inputs:")]
        [NeatArray] public UIInputControlArray controls;
        public TransformBehavior equipBehavior;
        public string context;

        // [Header("Input uses only one Hand")]
        // public bool usesSingleHand;
        

        protected UIHandler myUIHandler;
        
        bool needsSingleHandInput { get { return myUIHandler.controllerIndex != -1; } }

        
        // Vector2Int GetUIInputs (){
        //     int equipID = needsSingleHandInput ? workingWithEquipID : 0;
        //     SteamVR_Input_Sources hand = needsSingleHandInput ? VRManager.Int2Hand( equipID ) : SteamVR_Input_Sources.Any;
            
        //     for (int i = 0; i < controls.list.Length; i++) {
        //         if (controls.list[i].handDependent) {
        //             StandardizedVRInput.ButtonState[] buttonStates;
        //             StandardizedVRInput.instance.GetInputActionInfo(controls.list[i].action, out buttonStates);
        //             for (int b =0 ; b < buttonStates.Length; b++) {
        //                 if (buttonStates[b] == StandardizedVRInput.ButtonState.Down) {
        //                     return new Vector2Int(i, b);
        //                 }
        //             }
        //         }
        //         else {
        //             if (controls.list[i].action.GetStateDown(hand)) {
        //                 return new Vector2Int(i, equipID);
        //             }
        //         }
        //     }
        //     return new Vector2Int(-1, equipID);        
        // }

        protected virtual void OnEnable () {

            myUIHandler = UIHandler.GetUIHandlerByContext(context);

            if (myUIHandler != null) {
                myUIHandler.onUIClose += OnCloseUI;
                myUIHandler.onUIOpen += OnOpenUI;
                // myUIHandler.SetUIInputCallback(GetUIInputs);
            }
            else {
                Debug.LogError(context + " handler is null");
            }
        }

        protected virtual void OnDisable () {
            if (myUIHandler != null) {
                myUIHandler.onUIClose -= OnCloseUI;
                myUIHandler.onUIOpen -= OnOpenUI;
            }
        }

        // int workingWithEquipID;
        void OnOpenUI (GameObject uiObject, int interactorID) {

            // if (enforcedEquipID != -1) {
            //     // TODO: check if parameters are for inventory...(which uses single hand inputs)
            // }
            // workingWithEquipID = interactorID;// (int)parameters[1];
        
            // SteamVR_Input_Sources hand = needsSingleHandInput ? VRManager.Int2Hand( workingWithEquipID ) : SteamVR_Input_Sources.Any;
            SteamVR_Input_Sources hand = needsSingleHandInput ? VRManager.Int2Hand( myUIHandler.controllerIndex ) : SteamVR_Input_Sources.Any;

            // string[] inputNames = myUIHandler.GetInputNames();
            List<int> inputActions = myUIHandler.allActions;//.GetInputActions();
            for (int i = 0; i < inputActions.Count; i++) {
                StandardizedVRInput.MarkActionOccupied(Player.instance.actions[inputActions[i]], hand);
            }
            

            // for (int i = 0; i < controls.list.Length; i++) {
            //     StandardizedVRInput.MarkActionOccupied(controls.list[i].action, hand);
            //     StandardizedVRInput.instance.ShowHint(hand, controls.list[i].action, inputNames[i]);
            // }     

            if (equipBehavior != null) {
                TransformBehavior.AdjustTransform(uiObject.transform, Player.instance.GetHand(hand).transform, equipBehavior, 0);
            }

            VRUIInput.SetUIHand(hand);
        }

        void OnCloseUI (GameObject uiObject) {
            List<int> inputActions = myUIHandler.allActions;//.GetInputActions();
            for (int i = 0; i < inputActions.Count; i++) {
                StandardizedVRInput.MarkActionUnoccupied(Player.instance.actions[inputActions[i]]);
            }
            

            // for (int i = 0; i < controls.list.Length; i++) {
            //     StandardizedVRInput.MarkActionUnoccupied(controls.list[i].action);
            //     StandardizedVRInput.instance.HideHint(SteamVR_Input_Sources.Any, controls.list[i].action);    
            // }     
        }
    }


#if UNITY_EDITOR 
    [CustomPropertyDrawer(typeof(UIInputControl))] public class UIInputControlDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            float singleLineHeight = EditorGUIUtility.singleLineHeight;
            GUIContent noContent = GUIContent.none;
            EditorGUI.BeginProperty(position, label, property);

            int oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            float x = EditorTools.DrawIndent (oldIndent, position.x);

            EditorGUI.PropertyField(new Rect(x, position.y, 0, singleLineHeight), property.FindPropertyRelative("action"), noContent);
            
            EditorGUI.LabelField(new Rect(x, position.y, 100, singleLineHeight), new GUIContent("Hand Dependent:"));
            EditorGUI.PropertyField(new Rect(x + 100, position.y, 32, singleLineHeight), property.FindPropertyRelative("handDependent"), noContent);
            
            EditorGUI.indentLevel = oldIndent;
            
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }

    [CustomEditor(typeof(VRUIInputHandler))]
    public class VRUIInputHandlerEditor : Editor {
        VRUIInputHandler uiHandler;
        // string[] actionNames;

        
        // public static string[] GetHandlerInputNames (string context) {
        //     UIHandler[] allUIHandlers = GameObject.FindObjectsOfType<UIHandler>();
        //     for (int i = 0; i < allUIHandlers.Length; i++) {
        //         if (allUIHandlers[i].context == context) {
        //             return allUIHandlers[i].GetInputNames();
        //         }
        //     }
        //     return null;
        // }
        void OnEnable () {
            uiHandler = target as VRUIInputHandler;
            // actionNames = GetHandlerInputNames(uiHandler.context);
        }
            
        public override void OnInspectorGUI () {
            // TODO: change this game object find
            // string[] actionNames = UIHandler.GetHandlerInputNames(GameObject.FindObjectOfType<UIObjectInitializer>().gameObject, uiHandler.context);
            // if (actionNames != null) {
            //     EditorGUILayout.HelpBox("Actions:\n" + string.Join(", ", actionNames) , MessageType.Info);
            // }
            // else {
            //     EditorGUILayout.HelpBox("No context found :\n" + uiHandler.context, MessageType.Error);
            // }
            base.OnInspectorGUI();
        }
    }

#endif
}

