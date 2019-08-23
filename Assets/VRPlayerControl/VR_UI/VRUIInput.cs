using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR;
using Game;
using SimpleUI;
using StandaloneInputModule = SimpleUI.StandaloneInputModule;
namespace VRPlayer {

    /*
        interface for any ui input system set up
    */

    public class VRUIInput : BaseInput
    {

        static StandaloneInputModule inputModule;

        // public static bool HandOccupied (SteamVR_Input_Sources forHand) {
        //     return UIManager.uiInputActive && (forHand == currentUIHand || currentUIHand == SteamVR_Input_Sources.Any);
        // }
        // public static bool ActionOccupied (SteamVR_Action action, SteamVR_Input_Sources forHand) {
        //     return HandOccupied(forHand) && (
        //         (action == Player.instance.actions[UIManager.instance.submitAction]) || (action == Player.instance.actions[UIManager.instance.cancelAction]) || (action == selectionAxis) 
        //     );
        // }
        

        protected override void Awake() {
            base.Awake();
            inputModule = GameObject.FindObjectOfType<StandaloneInputModule>();
            if (inputModule) {
                inputModule.inputOverride = this;
            }
            else {
                Debug.LogError(" vr ui input couldnt find a standalone input module in the scene ");
            }
        }
        // protected override void OnEnable () {
        //     base.OnEnable();
        //     UIManager.onPopupOpen += OnPopupOpen;
        //     UIManager.onPopupClose += OnPopupClose;
        // }
        // protected override void OnDisable () {
        //     base.OnDisable();
        //     UIManager.onPopupOpen -= OnPopupOpen;
        //     UIManager.onPopupClose -= OnPopupClose;
        // }


        // // SteamVR_Input_Sources uiHandBeforePopup;
        // int uiHandBeforePopup;
        
        // void OnPopupOpen () {
        //     // uiHandBeforePopup = currentUIHand;
        //     uiHandBeforePopup = ControlsManager.currentUIController;

        //     // SetUIHand(SteamVR_Input_Sources.Any);
        //     ControlsManager.SetUIController(-1);
        // }

        // void OnPopupClose () {
        //     // SetUIHand(uiHandBeforePopup);
        //     ControlsManager.SetUIController(uiHandBeforePopup);
        // }
      
        // public static SteamVR_Input_Sources GetUIHand () {
        //     return currentUIHand != SteamVR_Input_Sources.Any ? currentUIHand : lastUsedUIHand;
        // }

        // public static void SetUIHand (SteamVR_Input_Sources hand) {
        //     currentUIHand = hand;
        // }
        // static SteamVR_Input_Sources lastUsedUIHand = SteamVR_Input_Sources.Any;
        // static SteamVR_Input_Sources currentUIHand = SteamVR_Input_Sources.Any;
        public override Vector2 mousePosition { get { return selectionAxis.GetAxis( ControlsManager.currentUIController < 0 ? SteamVR_Input_Sources.Any : VRManager.Int2Hand( ControlsManager.currentUIController) ); } }


        static SteamVR_Action_Vector2 selectionAxis { get { return Player.instance.trackpadAxis; } }

        
        /*
            make axis react to scrolling action on trackpad
        */
        public override float GetAxisRaw(string axisName) {

            // Vector2 axis = selectionAxis.GetAxis(currentUIHand);
            // if (currentUIHand == SteamVR_Input_Sources.Any) {

                int currentUIHand = ControlsManager.currentUIController;
            if (currentUIHand < 0) {
                ControlsManager.lastUsedUIController
                // lastUsedUIHand 
                    = selectionAxis.GetAxis(SteamVR_Input_Sources.RightHand) != Vector2.zero ? 
                    VRManager.Hand2Int( SteamVR_Input_Sources.RightHand ) : 
                    VRManager.Hand2Int( SteamVR_Input_Sources.LeftHand );
            }

            if (axisName == inputModule.horizontalAxis){ 
                return Player.instance.GetScrollDelta( currentUIHand < 0 ? SteamVR_Input_Sources.Any : VRManager.Int2Hand( currentUIHand)).x;
            }
            else if (axisName == inputModule.verticalAxis){
                return Player.instance.GetScrollDelta(currentUIHand < 0 ? SteamVR_Input_Sources.Any : VRManager.Int2Hand( currentUIHand)).y;
            }   
            return 0;
        }
    
        public override bool GetButtonDown(string buttonName) {
            // if (buttonName==inputModule.submitButton) {
            //     return submitButton.GetStateDown( currentUIHand );
            // } else if (buttonName==inputModule.cancelButton) {
            //     return cancelButton.GetStateDown( currentUIHand );
            // }
            return false;
        }
    }
}

