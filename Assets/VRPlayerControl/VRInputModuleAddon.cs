using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


using Valve.VR;
using SimpleUI;
namespace VRPlayer{

    public class VRInputModuleAddon : BaseInput
    {
        CustomInputModule standaloneInputModule;
        protected override void Awake() {
            // StandaloneInputModule 
            standaloneInputModule = GetComponent<CustomInputModule>();

            if (standaloneInputModule) standaloneInputModule.inputOverride = this;
        }
 
        public override float GetAxisRaw(string axisName) {
            // Debug.LogError("getting axis " + axisName);
            if (axisName== standaloneInputModule.horizontalAxis){ 
                return StandardizedVRInput.instance.TrackpadAxis.GetAxis(SteamVR_Input_Sources.Any).x;// > 0 ? 1 : 0;

                // your code here
            } else if (axisName==standaloneInputModule.verticalAxis) {
                // your code here
                return StandardizedVRInput.instance.TrackpadAxis.GetAxis(SteamVR_Input_Sources.Any).y;// > 0 ? 1 : 0;

            }
            return 0f;
        }
    
        public override bool GetButtonDown(string buttonName) {
            if (buttonName==standaloneInputModule.submitButton) {
                return StandardizedVRInput.instance.TriggerButton.GetStateDown(SteamVR_Input_Sources.Any);

                // your code here
            } else if (buttonName==standaloneInputModule.cancelButton) {
                return StandardizedVRInput.instance.SideButton.GetStateDown(SteamVR_Input_Sources.Any);

                // your code here
            }
            if (buttonName== standaloneInputModule.horizontalAxis){ 
                return Mathf.Abs(StandardizedVRInput.instance.TrackpadAxis.GetAxis(SteamVR_Input_Sources.Any).x) > 0 ? true : false;

                // your code here
            } else if (buttonName==standaloneInputModule.verticalAxis) {
                // your code here
                return Mathf.Abs(StandardizedVRInput.instance.TrackpadAxis.GetAxis(SteamVR_Input_Sources.Any).y) > 0 ? true : false;

            }
            return false;
        }
    }

}

