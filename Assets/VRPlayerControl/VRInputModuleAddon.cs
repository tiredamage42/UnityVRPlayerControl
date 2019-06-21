// using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using Valve.VR;
using SimpleUI;
namespace VRPlayer{

    public class VRInputModuleAddon : BaseInput
    {

        private static VRInputModuleAddon _instance;
		public static VRInputModuleAddon instance
		{
			get
			{
				if ( _instance == null )
				{
					_instance = FindObjectOfType<VRInputModuleAddon>();
				}
				return _instance;
			}
		}


        protected override void OnEnable () {
            base.OnEnable();
            UIManager.onUIShow += OnUIShow;
        }

        void OnUIShow (UIElementHolder uiObject) {
            uiObject.onAnySelection = OnAnyUISelection;
        }

        void OnAnyUISelection () {


            SteamVR_Input_Sources hand = currentUIHand != SteamVR_Input_Sources.Any ? currentUIHand : lastUsedUIHand;
            Debug.LogError("triggering haptic on " + hand);
            return;
            
            StandardizedVRInput.instance. TriggerHapticPulse( hand, 
                // float duration, 
                .1f,
                // float frequency, 
                1.0f,
                // float amplitude
                1.0f
            );
        
        
            
        }


        protected override void OnDisable () {
            base.OnDisable();
            UIManager.onUIShow -= OnUIShow;
        }



        public static void SetUIHand (SteamVR_Input_Sources hand) {
            instance.currentUIHand = hand;
            // instance.submitHand = hand;
            // instance.cancelHand = hand;
            // instance.scrollHand = hand;
            // instance.mouseHand = hand;
        }


        public static bool ActionIsOccupied (SteamVR_Action action, SteamVR_Input_Sources forHand) {
            return instance.gameObject.activeSelf && (
                // (action == instance.submitButton && forHand == instance.submitHand) || (action == instance.cancelButton && forHand == instance.cancelHand)
                (action == instance.submitButton && forHand == instance.currentUIHand) || (action == instance.cancelButton && forHand == instance.currentUIHand)
            );
        }




        SteamVR_Input_Sources lastUsedUIHand = SteamVR_Input_Sources.Any;
        SteamVR_Input_Sources currentUIHand = SteamVR_Input_Sources.Any;
        

        public SteamVR_Action_Boolean submitButton;
        // public SteamVR_Input_Sources submitHand = SteamVR_Input_Sources.Any;
        public SteamVR_Action_Boolean cancelButton;
        // public SteamVR_Input_Sources cancelHand = SteamVR_Input_Sources.Any;
        
        // public SteamVR_Input_Sources mouseHand = SteamVR_Input_Sources.Any;
        // public SteamVR_Input_Sources scrollHand = SteamVR_Input_Sources.Any;
        
        public float minDeltaForScroll = .01f;
        
        SimpleUI.StandaloneInputModule inputModule;
        protected override void Awake() {
            _instance = this;

            inputModule = GetComponent<SimpleUI.StandaloneInputModule>();
            if (inputModule) inputModule.inputOverride = this;
        }
        public float deltaThreshold = .05f;
        Vector2 lastAxis;

        void LateUpdate () {
            checkedFrameX = false;
            checkedFramey = false;
        }


        public override Vector2 mousePosition { get { 
            return StandardizedVRInput.instance.TrackpadAxis.GetAxis(
                currentUIHand
                // mouseHand
            ); 
        } }

        /*
            make axis react to scrolling action on trackpad
        */

        bool checkedFrameX, checkedFramey;
        Vector2 savedAxis;


        float GetAxisRaw(int axisIndex, Vector2 currentAxis, ref bool checkedFrame) {

                if (checkedFrame) {
                    return savedAxis[axisIndex];
                }
                    
                float delta = currentAxis[axisIndex] - lastAxis[axisIndex];
                float returnAxis = 0;

            if (delta != 0 && Mathf.Abs(delta) >= deltaThreshold) {
                if (lastAxis[axisIndex] == 0 || currentAxis[axisIndex] == 0) {
                    if (lastAxis[axisIndex] == 0) {
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
                lastAxis[axisIndex] = currentAxis[axisIndex];
                
            }



            checkedFrame = true;
            savedAxis[axisIndex] = returnAxis;

            return returnAxis;

            
        }



        public override float GetAxisRaw(string axisName) {

        
            Vector2 axis = StandardizedVRInput.instance.TrackpadAxis.GetAxis(currentUIHand);// scrollHand);
            if (currentUIHand == SteamVR_Input_Sources.Any) {
                Vector2 axisCheck = StandardizedVRInput.instance.TrackpadAxis.GetAxis(SteamVR_Input_Sources.RightHand);// scrollHand);
                if (axisCheck != Vector2.zero) {
                    lastUsedUIHand = SteamVR_Input_Sources.RightHand;
                }
                else {
                    lastUsedUIHand = SteamVR_Input_Sources.LeftHand;
                }
            }


            if (axisName == inputModule.horizontalAxis){ 
                return GetAxisRaw(0, axis, ref checkedFrameX);


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
            if (axisName == inputModule.verticalAxis){
                return GetAxisRaw(1, axis, ref checkedFramey);


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
            if (axisName== inputModule.horizontalAxis){ 
                // float val = StandardizedVRInput.instance.TrackpadAxis.GetAxis(SteamVR_Input_Sources.Any).x;// > 0 ? 1 : 0;
                float val = axis.x;
                // float delta = deltaAxis.x;

                // float delta = val - lastAxis.x;

                // lastAxis.x = val;


                // return val != 0 ? delta : 0;
                // return val;
                
                

            } else if (axisName==inputModule.verticalAxis) {
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
            if (buttonName==inputModule.submitButton) {
                return submitButton.GetStateDown( currentUIHand);// submitHand);

                // your code here
            } else if (buttonName==inputModule.cancelButton) {
                return cancelButton.GetStateDown( currentUIHand);// cancelHand);

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

