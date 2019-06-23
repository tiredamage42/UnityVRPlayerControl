using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

using Valve.VR.InteractionSystem;

namespace VRPlayer {

    public class StandardizedVRInput : MonoBehaviour
    {

        public struct ActionHandPair {
            
            public ISteamVR_Action_In_Source action;
            public SteamVR_Input_Sources hand;

            public ActionHandPair(ISteamVR_Action_In_Source action, SteamVR_Input_Sources hand) {
                this.action = action;
                this.hand = hand;
            }
            
        }

        public bool headsetIsOnPlayerHead;

        void Update()
        {
            if (headsetOnHead.GetStateDown(SteamVR_Input_Sources.Head))
            {
                Debug.Log("<b>SteamVR Interaction System</b> Headset placed on head");
                headsetIsOnPlayerHead = true;
            }
            else if (headsetOnHead.GetStateUp(SteamVR_Input_Sources.Head))
            {
                Debug.Log("<b>SteamVR Interaction System</b> Headset removed");
                headsetIsOnPlayerHead = false;
            }
        }


        static Dictionary<SteamVR_Action, SteamVR_Input_Sources> occupiedActions = new Dictionary<SteamVR_Action, SteamVR_Input_Sources>();

        public static void MarkActionOccupied(SteamVR_Action action, SteamVR_Input_Sources forHand) {
            occupiedActions[action] = forHand;
        }
        public static void MarkActionUnoccupied(SteamVR_Action action) {
            occupiedActions[action] = SteamVR_Input_Sources.Keyboard;
        }
        public static bool ActionOccupied (SteamVR_Action action, SteamVR_Input_Sources forHand) {
            if (VRUIInput.ActionOccupied(action, forHand)) {
                return true;
            }
            
            SteamVR_Input_Sources handValue;
            if (occupiedActions.TryGetValue(action, out handValue)) {
                return handValue != SteamVR_Input_Sources.Keyboard && forHand == handValue;
            }
            else {
                return false;
            }

        }

        
        public ControllerLayoutHintRoutine debugRoutine;
        public void PlayDebugRoutine () {
            if (debugRoutine == null) {
                Debug.LogError("Forgot to put in default debug routine");
                return;
            }
            // Debug.LogError("playign routine in input");
            PlayControllerLayoutHintRoutine(debugRoutine);
        }



        static StandardizedVRInput _instance;
        public static StandardizedVRInput instance {
            get {
                if (_instance == null) {
                    _instance = GameObject.FindObjectOfType<StandardizedVRInput>();
                }
                return _instance;
            }
        }

        public enum InputType {
            TriggerButton=0, TriggerAxis=1,
            TrackpadButton=2, TrackpadAxis=3,
            DpadUp=4, DpadDown=5, DpadLeft=6, DpadRight=7,
            MenuButton=8, SideButton=9
        };

        // public ISteamVR_Action_In_Source GetAction(InputType inputType) {
        //     switch(inputType) {
        //         case InputType.TriggerButton:
        //             return TriggerButton;
        //         case InputType.TriggerAxis:
        //             return TriggerAxis;
                
        //         case InputType.TrackpadButton:
        //             return TrackpadButton;
        //         case InputType.TrackpadAxis:
        //             return TrackpadAxis;
                
        //         case InputType.DpadUp:
        //             return DpadUp;
        //         case InputType.DpadDown:
        //             return DpadDown;
        //         case InputType.DpadLeft:
        //             return DpadLeft;
        //         case InputType.DpadRight:
        //             return DpadRight;
                
        //         case InputType.MenuButton:
        //             return MenuButton;
        //         case InputType.SideButton:
        //             return SideButton;
                
        //     }
        //     return null;
        // }
        public InputType Action2InputType(SteamVR_Action action) {
                    if (action ==  TriggerButton)
                return InputType.TriggerButton;
                    if (action ==   TriggerAxis)
                return InputType.TriggerAxis;
                
                    if (action ==   TrackpadButton)
                return InputType.TrackpadButton;
                    if (action ==   TrackpadAxis)
                return InputType.TrackpadAxis;
                
                    if (action ==   DpadUp)
                return InputType.DpadUp;
                    if (action == DpadDown)
                return InputType.DpadDown;
                    if (action ==   DpadLeft)
                return InputType.DpadLeft;
                    if (action ==   DpadRight)
                return InputType.DpadRight;
                
                    if (action ==   MenuButton)
                return InputType.MenuButton;
                    if (action ==   SideButton)
                return InputType.SideButton;
             
            return InputType.TriggerButton;
        }

        public SteamVR_Action_Boolean TriggerButton, TrackpadButton, MenuButton, SideButton;
        public SteamVR_Action_Single TriggerAxis;
        public SteamVR_Action_Vector2 TrackpadAxis;
        public SteamVR_Action_Boolean DpadUp, DpadDown, DpadLeft, DpadRight;

        [Tooltip("This action lets you know when the player has placed the headset on their head")]
        public SteamVR_Action_Boolean headsetOnHead = SteamVR_Input.GetBooleanAction("HeadsetOnHead");
        public SteamVR_Action_Vibration hapticAction = SteamVR_Input.GetAction<SteamVR_Action_Vibration>("Haptic");

        public void TriggerHapticPulse(SteamVR_Input_Sources hand, ushort microSecondsDuration)
        {
            float seconds = (float)microSecondsDuration / 1000000f;
            hapticAction.Execute(0, seconds, 1f / seconds, 1, hand);
        }

        public void TriggerHapticPulse(SteamVR_Input_Sources hand, float duration, float frequency, float amplitude)
        {
            hapticAction.Execute(0, duration, frequency, amplitude, hand);
        }


        public void HideHint(SteamVR_Input_Sources hand, SteamVR_Action action)
        {
            VRPlayer.UI.VRControllerHintsUI.HideHint(Action2InputType(action), hand);
        }

        public void ShowHint(SteamVR_Input_Sources hand, SteamVR_Action action, string text)
        {
            VRPlayer.UI.VRControllerHintsUI.ShowHint(Action2InputType(action), hand, text);
        }



        IEnumerator HintRoutine (ControllerLayoutHintRoutine routine) {
            Player player = Player.instance;

            VRPlayer.UI.VRControllerHintsUI.HideAllHints();
            
            while ( true )
            {
                for (int i = 0; i < routine.routineNodes.Length; i++)
                {
                    VRPlayer.UI.VRControllerHintsUI.ShowHint(routine.routineNodes[i].inputType, routine.routineNodes[i].hand, routine.routineNodes[i].name);
                    yield return new WaitForSeconds(routine.timeBetweenButtons);
                    VRPlayer.UI.VRControllerHintsUI.HideHint(routine.routineNodes[i].inputType, routine.routineNodes[i].hand);
                    yield return new WaitForSeconds(0.5f);
                    yield return null;
                }
                VRPlayer.UI.VRControllerHintsUI.HideAllHints();
                yield return new WaitForSeconds(routine.timeBetweenRepeats);
			}
        }
        Coroutine currentHintRoutine;

        public void StopHintRoutine () {
            if (currentHintRoutine != null) {

                StopCoroutine(currentHintRoutine);
                currentHintRoutine = null;
            }
            VRPlayer.UI.VRControllerHintsUI.HideAllHints();
        }

        public void PlayControllerLayoutHintRoutine (ControllerLayoutHintRoutine routine) {

            currentHintRoutine = StartCoroutine(HintRoutine(routine));
            
        }
    }







}
