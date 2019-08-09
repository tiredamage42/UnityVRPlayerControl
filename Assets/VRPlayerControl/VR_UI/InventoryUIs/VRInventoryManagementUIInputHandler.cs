using UnityEngine;

using GameUI;
using SimpleUI;
using Valve.VR;

namespace VRPlayer.UI {


    public class VRInventoryManagementUIInputHandler : MonoBehaviour 
    {
        public string context;
        [System.Serializable] public class UIInputControl {
            public SteamVR_Action_Boolean action;
            public bool handDependent;
        }

        [Header("Order Corresponds To Context Inputs:")]
        public UIInputControl[] controls;

        Vector2Int GetUIInputs (int equipID, SteamVR_Input_Sources hand) {
            for (int i = 0; i < controls.Length; i++) {
                if (controls[i].handDependent) {
                    
                    StandardizedVRInput.ButtonState[] buttonStates;
                    StandardizedVRInput.instance.GetInputActionInfo(controls[i].action, out buttonStates);
                    for (int b =0 ; b < buttonStates.Length; b++) {
                        if (buttonStates[b] == StandardizedVRInput.ButtonState.Down) {
                            return new Vector2Int(i, b);
                        }
                    }

                }
                else {
                    if (controls[i].action.GetStateDown(hand)) 
                        return new Vector2Int(i, equipID);
                }
            }
            return new Vector2Int(-1, equipID);        
        }

            
        public TransformBehavior equipBehavior;
        
        protected InventoryManagementUIHandler myUIHandler;
        
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

            for (int i = 0; i < controls.Length; i++) {
                StandardizedVRInput.MarkActionOccupied(controls[i].action, hand);
                StandardizedVRInput.instance.ShowHint(hand, controls[i].action, actionNames[i]);
            }     

            if (equipBehavior != null) {
                TransformBehavior.AdjustTransform(uiObject.baseObject.transform, Player.instance.GetHand(hand).transform, equipBehavior, 0);
            }

            VRUIInput.SetUIHand(hand);
        }

        void OnCloseUI (UIElementHolder uiObject) {
            for (int i = 0; i < controls.Length; i++) {
                StandardizedVRInput.MarkActionUnoccupied(controls[i].action);
                StandardizedVRInput.instance.HideHint(SteamVR_Input_Sources.Any, controls[i].action);    
            }     
        }
    }
}

