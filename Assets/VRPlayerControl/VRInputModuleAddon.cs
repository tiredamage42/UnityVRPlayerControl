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

        Vector2 lastAxis;

        /*
            make axis react to scrolling action on trackpad
        */
        public override float GetAxisRaw(string axisName) {
            // Debug.LogError("getting axis " + axisName);
            if (axisName== standaloneInputModule.horizontalAxis){ 
                float val = StandardizedVRInput.instance.TrackpadAxis.GetAxis(SteamVR_Input_Sources.Any).x;// > 0 ? 1 : 0;
                

                float delta = val - lastAxis.x;

                lastAxis.x = val;


                return val != 0 ? delta : 0;
                // return val;
                
                

            } else if (axisName==standaloneInputModule.verticalAxis) {
                float val = StandardizedVRInput.instance.TrackpadAxis.GetAxis(SteamVR_Input_Sources.Any).y;// > 0 ? 1 : 0;
                
                float delta = val - lastAxis.y;
                lastAxis.y = val;
                return val != 0 ? delta : 0;
                // return val;
                
                
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
            // if (buttonName== standaloneInputModule.horizontalAxis){ 
            //     return Mathf.Abs(StandardizedVRInput.instance.TrackpadAxis.GetAxis(SteamVR_Input_Sources.Any).x) > 0 ? true : false;

            //     // your code here
            // } else if (buttonName==standaloneInputModule.verticalAxis) {
            //     // your code here
            //     return Mathf.Abs(StandardizedVRInput.instance.TrackpadAxis.GetAxis(SteamVR_Input_Sources.Any).y) > 0 ? true : false;

            // }
            return false;
        }
    }

}

