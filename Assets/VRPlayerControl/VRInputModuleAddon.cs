using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


using Valve.VR;
namespace VRPlayer{

    public class VRInputModuleAddon : BaseInput
    {
        StandaloneInputModule standaloneInputModule;
        protected override void Awake() {
            // StandaloneInputModule 
            standaloneInputModule = GetComponent<StandaloneInputModule>();

            if (standaloneInputModule) standaloneInputModule.inputOverride = this;
        }
 
        public override float GetAxisRaw(string axisName) {
            if (axisName== standaloneInputModule.horizontalAxis){ 
                return StandardizedVRInput.instance.TrackpadAxis.GetAxis(SteamVR_Input_Sources.Any).x;

                // your code here
            } else if (axisName==standaloneInputModule.verticalAxis) {
                // your code here
                return StandardizedVRInput.instance.TrackpadAxis.GetAxis(SteamVR_Input_Sources.Any).y;

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
            return false;
        }
    }

}

