using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace VRPlayer.UI {

    public class VRControllerHintsUI : MonoBehaviour
    {

        static ControllerHintUIElement[] leftHand, rightHand;
        public GameObject leftHandHints, rightHandHints;
        public TransformBehavior hintTransforms;

        void Start () {
            AdjustHints();
            HideAllHints();
        }
        void Awake () {
            leftHand = InitializeHints (leftHandHints, action2hintLeft);
            rightHand = InitializeHints (rightHandHints, action2hintRight);

        }

        ControllerHintUIElement[] InitializeHints (GameObject handHints, Dictionary<StandardizedVRInput.InputType, ControllerHintUIElement> actionDictionary) {
            ControllerHintUIElement[] hand = handHints.GetComponentsInChildren<ControllerHintUIElement>();
            for (int i = 0; i < hand.Length; i++) {
                VRControllerHint vrHint = hand[i].GetComponent<VRControllerHint>();
                actionDictionary.Add(vrHint.action, hand[i]);

                if (vrHint.action == StandardizedVRInput.InputType.TriggerButton) {
                    actionDictionary.Add(StandardizedVRInput.InputType.TriggerAxis, hand[i]);
                }
                else if (vrHint.action == StandardizedVRInput.InputType.TriggerAxis) {
                    actionDictionary.Add(StandardizedVRInput.InputType.TriggerButton, hand[i]);
                }
            }
            return hand;
        }

        void Update () {
#if UNITY_EDITOR
            AdjustHints () ;
#endif
        }

        void AdjustHints () {
            for (int i = 0; i < rightHand.Length; i++) {
                TransformBehavior.AdjustTransform(rightHand[i].transform, Player.instance.rightHand.transform, hintTransforms, i);
                TransformBehavior.AdjustTransform(leftHand[i].transform, Player.instance.leftHand.transform, hintTransforms, i, new Vector3(-1,1,1));
            }
        }

        static Dictionary<StandardizedVRInput.InputType, ControllerHintUIElement> action2hintLeft = new  Dictionary<StandardizedVRInput.InputType, ControllerHintUIElement>(), action2hintRight = new  Dictionary<StandardizedVRInput.InputType, ControllerHintUIElement>();

        
        public static void ShowHint (StandardizedVRInput.InputType action, SteamVR_Input_Sources forHand, string text) {
            if (!action2hintLeft.ContainsKey(action) || !action2hintLeft.ContainsKey(action)) {
                Debug.LogError("no key for action " + action);
                return;
            }
            if (forHand == SteamVR_Input_Sources.Any) {
                action2hintLeft[action].Show(text);
                action2hintRight[action].Show(text);
            }
            else if (forHand == SteamVR_Input_Sources.LeftHand) {
                action2hintLeft[action].Show(text);
            }
            else if (forHand == SteamVR_Input_Sources.RightHand) {
                action2hintRight[action].Show(text);
            }
        }
        public static void HideHint (StandardizedVRInput.InputType action, SteamVR_Input_Sources forHand) {
            if (!action2hintLeft.ContainsKey(action) || !action2hintLeft.ContainsKey(action)) {
                Debug.LogError("no key for action " + action);
                return;
            }
            
            if (forHand == SteamVR_Input_Sources.Any) {
                action2hintLeft[action].Hide();
                action2hintRight[action].Hide();
            }
            else if (forHand == SteamVR_Input_Sources.LeftHand) {
                action2hintLeft[action].Hide();
            }
            else if (forHand == SteamVR_Input_Sources.RightHand) {
                action2hintRight[action].Hide();
            }

        }
        public static void HideAllHints () {
            for (int i = 0; i < leftHand.Length; i++) {
                leftHand[i].Hide();
                rightHand[i].Hide();
            }
        }       
    }
}
