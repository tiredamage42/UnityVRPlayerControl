﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Valve.VR;
using Valve.VR.InteractionSystem;

namespace VRPlayer {

   

    public class StandardizedVRInput : MonoBehaviour
    {
        
        public ControllerLayoutHintRoutine debugRoutine;
        public void PlayDebugRoutine () {
            if (debugRoutine == null) {
                Debug.LogError("Forgot to put in default debug routine");
                return;
            }
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
            TriggerButton, TriggerAxis,
            TrackpadButton, TrackpadAxis,
            DpadUp, DpadDown, DpadLeft, DpadRight,
            MenuButton, SideButton
        };

        public ISteamVR_Action_In_Source GetAction(InputType inputType) {
            switch(inputType) {
                case InputType.TriggerButton:
                    return TriggerButton;
                case InputType.TriggerAxis:
                    return TriggerAxis;
                
                case InputType.TrackpadButton:
                    return TrackpadButton;
                case InputType.TrackpadAxis:
                    return TrackpadAxis;
                
                case InputType.DpadUp:
                    return DpadUp;
                case InputType.DpadDown:
                    return DpadDown;
                case InputType.DpadLeft:
                    return DpadLeft;
                case InputType.DpadRight:
                    return DpadRight;
                
                case InputType.MenuButton:
                    return MenuButton;
                case InputType.SideButton:
                    return SideButton;
                
            }
            return null;
        }

        public SteamVR_Action_Boolean TriggerButton, TrackpadButton, MenuButton, SideButton;
        public SteamVR_Action_Single TriggerAxis;
        public SteamVR_Action_Vector2 TrackpadAxis;
        public SteamVR_Action_Boolean DpadUp, DpadDown, DpadLeft, DpadRight;

        IEnumerator HintRoutine (ControllerLayoutHintRoutine routine) {
            Player player = Player.instance;

            for (int i = 0; i < player.hands.Length; i++) {
                ControllerButtonHints.HideAllTextHints( player.hands[i] );
                ControllerButtonHints.HideAllButtonHints( player.hands[i] );
            }

			while ( true )
            {
                for (int i = 0; i < routine.routineNodes.Length; i++)
                {
                    ISteamVR_Action_In_Source action = GetAction(routine.routineNodes[i].inputType);

                    ControllerButtonHints.ShowTextHint(player.GetHand(routine.routineNodes[i].hand), action, routine.routineNodes[i].name);
                    yield return new WaitForSeconds(routine.timeBetweenButtons);
                    ControllerButtonHints.HideTextHint(player.GetHand(routine.routineNodes[i].hand), action);
                    yield return new WaitForSeconds(0.5f);

                    yield return null;
                }

                for (int i = 0; i < player.hands.Length; i++) {
                    ControllerButtonHints.HideAllTextHints( player.hands[i] );
                    ControllerButtonHints.HideAllButtonHints( player.hands[i] );
                }

                yield return new WaitForSeconds(routine.timeBetweenRepeats);
			}
	

        }
        Coroutine currentHintRoutine;

        public void StopHintRoutine () {
            if (currentHintRoutine != null) {

                StopCoroutine(currentHintRoutine);
                currentHintRoutine = null;
            }

            Player player = Player.instance;

            for (int i = 0; i < player.hands.Length; i++) {
                ControllerButtonHints.HideAllTextHints( player.hands[i] );
                ControllerButtonHints.HideAllButtonHints( player.hands[i] );
            }

        }

        public void PlayControllerLayoutHintRoutine (ControllerLayoutHintRoutine routine) {

            currentHintRoutine = StartCoroutine(HintRoutine(routine));
            

        }
    }







}