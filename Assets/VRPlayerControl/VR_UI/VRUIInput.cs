using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR;

using StandaloneInputModule = SimpleUI.StandaloneInputModule;

namespace VRPlayer{

    /*
        interface for any ui input system set up
    */

    public class VRUIInput : BaseInput
    {

        static StandaloneInputModule inputModule;

        //assumes input module turns on and off with ui
        public static bool uiInputActive {
            get {
                return inputModule.gameObject.activeInHierarchy;//.UIManager.uiInputActive;
            }
        }
        public static bool HandOccupied (SteamVR_Input_Sources forHand) {
            return uiInputActive && (forHand == currentUIHand || currentUIHand == SteamVR_Input_Sources.Any);
        }
        public static bool ActionOccupied (SteamVR_Action action, SteamVR_Input_Sources forHand) {
            return HandOccupied(forHand) && (
                (action == instance.submitButton) || (action == instance.cancelButton) || (action == selectionAxis) 
            );
        }
        

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
      

        public static SteamVR_Input_Sources GetUIHand () {
            return currentUIHand != SteamVR_Input_Sources.Any ? currentUIHand : lastUsedUIHand;
        }

        static VRUIInput _instance;
		public static VRUIInput instance {
			get {
				if ( _instance == null )
					_instance = FindObjectOfType<VRUIInput>();
				return _instance;
			}
		}

        public static void SetUIHand (SteamVR_Input_Sources hand) {
            currentUIHand = hand;
        }

        public SteamVR_Action_Boolean submitButton;
        public SteamVR_Action_Boolean cancelButton;      
        [Header(".15f is a good setting when\nstandalone input module has 60 actions per second")]  
        public float deltaThresholdForScroll = .15f;
        
        static SteamVR_Action_Vector2 selectionAxis {
            get {
                return StandardizedVRInput.instance.TrackpadAxis;
            }
        }
        static SteamVR_Input_Sources lastUsedUIHand = SteamVR_Input_Sources.Any;
        static SteamVR_Input_Sources currentUIHand = SteamVR_Input_Sources.Any;

        public override Vector2 mousePosition { get { return selectionAxis.GetAxis( currentUIHand ); } }

        bool checkedFrameX, checkedFramey;
        Vector2 savedAxis, lastAxis;
        void LateUpdate () {
            checkedFrameX = false;
            checkedFramey = false;
        }


        /*
            make axis react to scrolling action on trackpad
        */
        float GetAxisRaw(int axisIndex, Vector2 currentAxis, ref bool checkedFrame) {

            if (checkedFrame) {
                return savedAxis[axisIndex];
            }
            float delta = currentAxis[axisIndex] - lastAxis[axisIndex];
            float returnAxis = 0;

            if (delta != 0 && Mathf.Abs(delta) >= deltaThresholdForScroll) {
                if (lastAxis[axisIndex] == 0 || currentAxis[axisIndex] == 0) {
                    if (lastAxis[axisIndex] == 0) {
                        // Debug.LogError("on scroll start");
                    }
                    else {
                        // Debug.LogError("on scroll end");
                    }
                }
                else {
                    returnAxis = Mathf.Clamp(delta * 99999, -1, 1);
                }
                lastAxis[axisIndex] = currentAxis[axisIndex];
            }
            checkedFrame = true;
            savedAxis[axisIndex] = returnAxis;
            return returnAxis;
        }


        public override float GetAxisRaw(string axisName) {

            Vector2 axis = selectionAxis.GetAxis(currentUIHand);
            if (currentUIHand == SteamVR_Input_Sources.Any) {
                lastUsedUIHand = selectionAxis.GetAxis(SteamVR_Input_Sources.RightHand) != Vector2.zero ? 
                    SteamVR_Input_Sources.RightHand : 
                    SteamVR_Input_Sources.LeftHand;
            }

            if (axisName == inputModule.horizontalAxis){ 
                return GetAxisRaw(0, axis, ref checkedFrameX);
            }
            else if (axisName == inputModule.verticalAxis){
                return GetAxisRaw(1, axis, ref checkedFramey);
            }   
            return 0;
        }
    
        public override bool GetButtonDown(string buttonName) {
            if (buttonName==inputModule.submitButton) {
                return submitButton.GetStateDown( currentUIHand );
            } else if (buttonName==inputModule.cancelButton) {
                return cancelButton.GetStateDown( currentUIHand );
            }
            return false;
        }
    }
}

