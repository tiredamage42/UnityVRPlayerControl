using UnityEngine;

using GameUI;
using SimpleUI;
using Valve.VR;

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
        protected InventoryManagementUIHandler myUIHandler;
        

        Vector2Int GetUIInputs (int equipID, SteamVR_Input_Sources hand) {
            
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

            
        
        Vector2Int GetUIInputs () {
            bool usesID = myUIHandler.EquipIDSpecific();
            int equipID = usesID ? myUIHandler.workingWithEquipID : 0;
            SteamVR_Input_Sources hand = usesID ? VRManager.Int2Hand( equipID ) : SteamVR_Input_Sources.Any;
            return GetUIInputs(equipID, hand);
        }


        string[] actionNames;

        protected virtual void OnEnable () {

            myUIHandler = InventoryManagementUIHandler.GetUIHandlerByContext(context);

            if (myUIHandler != null) {
                actionNames = myUIHandler.GetInputNames();
                myUIHandler.onUIClose += OnCloseUI;
                myUIHandler.onUIOpen += OnOpenUI;
                myUIHandler.SetUIInputCallback(GetUIInputs);
            }
        }

        protected virtual void OnDisable () {
            if (myUIHandler != null) {
                myUIHandler.onUIClose -= OnCloseUI;
                myUIHandler.onUIOpen -= OnOpenUI;
            }
        }

        void OnOpenUI (UIElementHolder uiObject) {
            SteamVR_Input_Sources hand = myUIHandler.EquipIDSpecific() ? VRManager.Int2Hand( myUIHandler.workingWithEquipID ) : SteamVR_Input_Sources.Any;

            for (int i = 0; i < controls.list.Length; i++) {
                StandardizedVRInput.MarkActionOccupied(controls.list[i].action, hand);
                StandardizedVRInput.instance.ShowHint(hand, controls.list[i].action, actionNames[i]);
            }     

            if (equipBehavior != null) {
                TransformBehavior.AdjustTransform(uiObject.baseObject.transform, Player.instance.GetHand(hand).transform, equipBehavior, 0);
            }

            VRUIInput.SetUIHand(hand);
        }

        void OnCloseUI (UIElementHolder uiObject) {
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
            base.OnInspectorGUI();
            string[] actionNames = InventoryManagementUIHandler.GetHandlerInputNames(uiHandler.context);
            if (actionNames != null) {
                EditorGUILayout.HelpBox("Actions:\n" + string.Join(", ", actionNames) , MessageType.Info);
            }
            else {
                EditorGUILayout.HelpBox("No context found :\n" + uiHandler.context, MessageType.Error);
            }
        }
    }

#endif
}

