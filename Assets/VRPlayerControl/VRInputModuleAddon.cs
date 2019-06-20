using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


using Valve.VR;
using SimpleUI;
namespace VRPlayer{

    public class VRInputModuleAddon : BaseInput
    {
        public float minDelta = .01f;
        
        CustomInputModule standaloneInputModule;
        protected override void Awake() {
            // StandaloneInputModule 
            standaloneInputModule = GetComponent<CustomInputModule>();

            if (standaloneInputModule) standaloneInputModule.inputOverride = this;
        }
        public float deltaThreshold = .05f;
        Vector2 lastAxis, deltaAxis, axis;


        public float scrollSensitivity = 1;
        public bool useExtremesMultiplier = true;

        // bool checkedAxes;

        void LateUpdate () {
            // checkedAxes = false;

            checkedFrameX = false;
            checkedFramey = false;

        }

        // void UpdateAxes () {
        //     axis = StandardizedVRInput.instance.TrackpadAxis.GetAxis(SteamVR_Input_Sources.Any);
        //     deltaAxis = lastAxis == Vector2.zero ? Vector2.zero : ((axis - lastAxis) * scrollSensitivity * (useExtremesMultiplier ? axis.magnitude : 1));
            
        //     if (deltaAxis.magnitude < minDelta) {
        //         deltaAxis = Vector2.zero;
        //     }
            
        //     lastAxis = axis;
        //     checkedAxes = true;
        // }
         
        // public override Vector2 mouseScrollDelta {
        //     get {
        //         if (!checkedAxes) {
        //             UpdateAxes();
        //         }
        //         return deltaAxis;
        //         // return base.mouseScrollDelta;
        //     }
        // }

        /*
            make axis react to scrolling action on trackpad
        */

        bool checkedFrameX, checkedFramey;
        Vector2 savedAxis;
        public override float GetAxisRaw(string axisName) {

           
            
            
        
            Vector2 axis = StandardizedVRInput.instance.TrackpadAxis.GetAxis(SteamVR_Input_Sources.Any);

            if (axisName == standaloneInputModule.horizontalAxis){ 

                 if (checkedFrameX) {
                return savedAxis.x;
            }
                    
            float delta = axis.x - lastAxis.x;
                float returnAxis = 0;

            if (delta != 0 && Mathf.Abs(delta) >= deltaThreshold) {
                // Debug.LogError("got here");
                if (lastAxis.x == 0 || axis.x == 0) {
                    if (lastAxis.x == 0) {
                        // Debug.LogError("on scroll start");
                    }
                    else {
                        // Debug.LogError("on scroll end");
                    }
                }
                else {
                    // Debug.LogError("should chekc names");
                    returnAxis = Mathf.Clamp(delta * 99999, -1, 1);
                    
                    

                }
                lastAxis.x = axis.x;
                
            }
            // else {
            //     return 0;
            // }



            checkedFrameX = true;
            savedAxis.x = returnAxis;

            return returnAxis;

            }
            if (axisName == standaloneInputModule.verticalAxis){
                if (checkedFramey) {
                return savedAxis.y;
            }
            float delta = axis.y - lastAxis.y;

            Debug.LogError("delta " + delta + "last axis y ::" + lastAxis.y + " current: " + axis.y);


                float returnAxis = 0;
            if (delta != 0 && Mathf.Abs(delta) >= deltaThreshold) {
                Debug.LogError("got here");
                if (lastAxis.y == 0 || axis.y == 0) {
                    if (lastAxis.y == 0) {
                        Debug.LogError("on scroll start");
                    }
                    else {
                        Debug.LogError("on scroll end");
                    }
                }
                else {
                        returnAxis = Mathf.Clamp(delta * 99999, -1, 1);

                        Debug.LogError("return axis" + returnAxis);
                      
                }



                lastAxis.y = axis.y;
                // return returnAxis;
            }
            // else {
            //     return 0;
            // }


            checkedFramey = true;
            savedAxis.y = returnAxis;

            return returnAxis;

            
            
            }   




            // return 0;
            // if (!checkedAxes) {
            //     UpdateAxes();
            // }
            // return 0;
                
            // if (!checkedAxes) {
            //Vector2 
            // axis = StandardizedVRInput.instance.TrackpadAxis.GetAxis(SteamVR_Input_Sources.Any);
            //     deltaAxis = lastAxis == Vector2.zero ? Vector2.zero : axis - lastAxis;
            //     lastAxis = axis;
            //     checkedAxes = true;
            // }
            // Debug.LogError("getting axis " + axisName);
            if (axisName== standaloneInputModule.horizontalAxis){ 
                // float val = StandardizedVRInput.instance.TrackpadAxis.GetAxis(SteamVR_Input_Sources.Any).x;// > 0 ? 1 : 0;
                float val = axis.x;
                // float delta = deltaAxis.x;

                // float delta = val - lastAxis.x;

                // lastAxis.x = val;


                // return val != 0 ? delta : 0;
                // return val;
                
                

            } else if (axisName==standaloneInputModule.verticalAxis) {
                float val = axis.y;// StandardizedVRInput.instance.TrackpadAxis.GetAxis(SteamVR_Input_Sources.Any).y;// > 0 ? 1 : 0;
                // float delta = val - lastAxis.y;
                // float delta = deltaAxis.y;
                // Debug.LogError("val : " + val + " // delta : " + delta);
                
                // lastAxis.y = val;
                // float returnval = val != 0 && Mathf.Abs(delta) > deltaThreshold ? Mathf.Clamp(delta * 99999, -1, 1) : 0;
                // return val;
                // Debug.LogError("return val" + returnval);
                // return returnval;
                
                
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
            // else if (buttonName==standaloneInputModule.verticalAxis || buttonName== standaloneInputModule.horizontalAxis) {
            //      axis = StandardizedVRInput.instance.TrackpadAxis.GetAxis(SteamVR_Input_Sources.Any);
           
            //     return lastAxis == Vector2.zero && axis != Vector2.zero;
            //     // your code here
            //     return Mathf.Abs(StandardizedVRInput.instance.TrackpadAxis.GetAxis(SteamVR_Input_Sources.Any).y) > 0 ? true : false;

            // }
            return false;
        }
    }

}

