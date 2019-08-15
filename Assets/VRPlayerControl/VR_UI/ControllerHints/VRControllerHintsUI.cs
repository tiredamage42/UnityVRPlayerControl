using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace VRPlayer.UI {

    public class VRControllerHintsUI : MonoBehaviour
    {

        // indicies on transform behavior...
        // 0 = 1 hint
        // 1 / 2 = 2 hints
        // 3 / 4 = 3 hints (first is 0)

        const int maxHintsShown = 3;
        static List<List<ControllerHintUIElement>> hintsShown = new List<List<ControllerHintUIElement>>() {
            new List<ControllerHintUIElement>(), new List<ControllerHintUIElement>()
        };

        static ControllerHintUIElement[][] handHints;//leftHand, rightHand;
        public GameObject leftHandHints, rightHandHints;
        public TransformBehavior hintTransforms;
        

        static VRControllerHintsUI instance;

        void Start () {
            // AdjustHints();
            HideAllHints();
        }
        void Awake () {
            instance = this;

            handHints = new ControllerHintUIElement[2][];
            for (int i = 0; i < 2; i++) {
                SteamVR_Input_Sources hand = VRManager.Int2Hand(i);
                handHints[i] = InitializeHints (hand == SteamVR_Input_Sources.LeftHand ? leftHandHints : rightHandHints, action2Hints[i]);
            }
            // leftHand = InitializeHints (leftHandHints, action2hintLeft);
            // rightHand = InitializeHints (rightHandHints, action2hintRight);

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

// #if UNITY_EDITOR
//         void Update () {
//             AdjustHints () ;
//         }
// #endif


        // indicies on transform behavior...
        // 0 = 1 hint
        // 3 / 4 = 2 hints
        // 1 / 2 = 3 hints (first is 0)


        static void AdjustHints () {
            for (int i = 0; i < 2; i++) {
                int c = hintsShown[i].Count;
                if (c == 0) continue;
                Transform handTransform = Player.instance.GetHand(VRManager.Int2Hand(i)).transform;
                Vector3 multiplier = new Vector3(i == 0 ? -1 : 1, 1, 1);
                for (int x = 0; x < c; x++) TransformBehavior.AdjustTransform(hintsShown[i][x].transform, handTransform, instance.hintTransforms, x + ( c == 2 ? maxHintsShown : 0 ), multiplier);
            }

            // for (int i = 0; i < rightHand.Length; i++) {
            //     TransformBehavior.AdjustTransform(rightHand[i].transform, Player.instance.rightHand.transform, hintTransforms, i);
            //     TransformBehavior.AdjustTransform(leftHand[i].transform, Player.instance.leftHand.transform, hintTransforms, i, new Vector3(-1,1,1));
            // }
        }



        // static Dictionary<StandardizedVRInput.InputType, ControllerHintUIElement> action2hintLeft = new  Dictionary<StandardizedVRInput.InputType, ControllerHintUIElement>(), action2hintRight = new  Dictionary<StandardizedVRInput.InputType, ControllerHintUIElement>();
        static List<Dictionary<StandardizedVRInput.InputType, ControllerHintUIElement>> action2Hints = new List<Dictionary<StandardizedVRInput.InputType, ControllerHintUIElement>> () {
            new  Dictionary<StandardizedVRInput.InputType, ControllerHintUIElement>(), 
            new  Dictionary<StandardizedVRInput.InputType, ControllerHintUIElement>() 
        };

        
        public static void ShowHint (StandardizedVRInput.InputType action, SteamVR_Input_Sources forHand, string text) {

            for (int i = 0; i > 2; i++) {
                if (!action2Hints[i].ContainsKey(action)) {
                    Debug.LogError("no key for action " + action);
                    return;
                }
            }
            // if (!action2hintLeft.ContainsKey(action) || !action2hintLeft.ContainsKey(action)) {
            //     Debug.LogError("no key for action " + action);
            //     return;
            // }
            if (forHand == SteamVR_Input_Sources.Any) {
                for (int i = 0; i < 2; i++) {
                    if (hintsShown[i].Count < maxHintsShown) {
                        action2Hints[i][action].Show(text);
                        hintsShown[i].Add(action2Hints[i][action]);
                    }
                }
                AdjustHints();



                // action2hintLeft[action].Show(text);
                // action2hintRight[action].Show(text);
            }
            else if (forHand == SteamVR_Input_Sources.LeftHand) {
                int index = VRManager.Hand2Int(forHand);
                if (hintsShown[index].Count < maxHintsShown) {
                    action2Hints[index][action].Show(text);
                    hintsShown[index].Add(action2Hints[index][action]);
                    AdjustHints();
                }

                // action2hintLeft[action].Show(text);
            }
            else if (forHand == SteamVR_Input_Sources.RightHand) {
                
                int index = VRManager.Hand2Int(forHand);
                if (hintsShown[index].Count < maxHintsShown) {
                    action2Hints[index][action].Show(text);
                    hintsShown[index].Add(action2Hints[index][action]);
                    AdjustHints();
                }

                
                // action2hintRight[action].Show(text);
            }
        }
        public static void HideHint (StandardizedVRInput.InputType action, SteamVR_Input_Sources forHand) {
            // if (!action2hintLeft.ContainsKey(action) || !action2hintLeft.ContainsKey(action)) {
            //     Debug.LogError("no key for action " + action);
            //     return;
            // }
            for (int i = 0; i > 2; i++) {
                if (!action2Hints[i].ContainsKey(action)) {
                    Debug.LogError("no key for action " + action);
                    return;
                }
            }
            
            
            if (forHand == SteamVR_Input_Sources.Any) {
                // action2hintLeft[action].Hide();
                // action2hintRight[action].Hide();

                for (int i = 0; i < 2; i++) {
                    if (hintsShown[i].Contains(action2Hints[i][action])) {
                        action2Hints[i][action].Hide();
                        hintsShown[i].Remove(action2Hints[i][action]);
                    }
                }
                


                AdjustHints();
            }
            else if (forHand == SteamVR_Input_Sources.LeftHand) {

                int index = VRManager.Hand2Int(forHand);
                if (hintsShown[index].Contains(action2Hints[index][action])) {
                    action2Hints[index][action].Hide();
                    hintsShown[index].Remove(action2Hints[index][action]);
                    AdjustHints();
                }

                // action2hintLeft[action].Hide();

            }
            else if (forHand == SteamVR_Input_Sources.RightHand) {

                int index = VRManager.Hand2Int(forHand);
                if (hintsShown[index].Contains(action2Hints[index][action])) {
                    action2Hints[index][action].Hide();
                    hintsShown[index].Remove(action2Hints[index][action]);
                    AdjustHints();
                }

                // action2hintRight[action].Hide();
            }
        }
        public static void HideAllHints () {
            for (int i = 0; i < 2; i++) {
                hintsShown[i].Clear();
                
                for (int x = 0; x < handHints[i].Length; x++) {
                    handHints[i][x].Hide();                
                }

            }
        }       
    }
}
