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
public float deltaThreshold = .05f;
        Vector2 lastAxis, deltaAxis, axis;

        bool checkedAxes;

        void LateUpdate () {
            checkedAxes = false;
        }

        /*
            make axis react to scrolling action on trackpad
        */
        public override float GetAxisRaw(string axisName) {
            if (!checkedAxes) {
            //Vector2 
            axis = StandardizedVRInput.instance.TrackpadAxis.GetAxis(SteamVR_Input_Sources.Any);
                deltaAxis = lastAxis == Vector2.zero ? Vector2.zero : axis - lastAxis;
                lastAxis = axis;
                checkedAxes = true;
            }
            // Debug.LogError("getting axis " + axisName);
            if (axisName== standaloneInputModule.horizontalAxis){ 
                // float val = StandardizedVRInput.instance.TrackpadAxis.GetAxis(SteamVR_Input_Sources.Any).x;// > 0 ? 1 : 0;
                float val = axis.x;
                float delta = deltaAxis.x;

                // float delta = val - lastAxis.x;

                // lastAxis.x = val;


                return val != 0 ? delta : 0;
                // return val;
                
                

            } else if (axisName==standaloneInputModule.verticalAxis) {
                float val = axis.y;// StandardizedVRInput.instance.TrackpadAxis.GetAxis(SteamVR_Input_Sources.Any).y;// > 0 ? 1 : 0;
                // float delta = val - lastAxis.y;
                float delta = deltaAxis.y;
                // Debug.LogError("val : " + val + " // delta : " + delta);
                
                // lastAxis.y = val;
                float returnval = val != 0 && Mathf.Abs(delta) > deltaThreshold ? Mathf.Clamp(delta * 99999, -1, 1) : 0;
                // return val;
                // Debug.LogError("return val" + returnval);
                return returnval;
                
                
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
            //} 
            else if (buttonName==standaloneInputModule.verticalAxis || buttonName== standaloneInputModule.horizontalAxis) {
                 axis = StandardizedVRInput.instance.TrackpadAxis.GetAxis(SteamVR_Input_Sources.Any);
           
                return lastAxis == Vector2.zero && axis != Vector2.zero;
            //     // your code here
            //     return Mathf.Abs(StandardizedVRInput.instance.TrackpadAxis.GetAxis(SteamVR_Input_Sources.Any).y) > 0 ? true : false;

            }
            return false;
        }
    }

}

