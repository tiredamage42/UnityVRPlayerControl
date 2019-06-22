﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace VRPlayer.UI {

    public class VRControllerHintsUI : MonoBehaviour
    {

        [System.Serializable] public class HintUIElement{
            public ControllerHintUIElement element;
            public StandardizedVRInput.InputType action;
            public Vector3 textOffset;
        }
            

        public HintUIElement[] leftHand, rightHand;

        public TransformBehavior hintTransforms;


        public float textScale = .00075f;
        public float hintScale = .75f;


        void Start () {
            AdjustHints();
            HideAllHints();
        }
        void Awake () {
            instance = this;


            for (int i = 0; i < leftHand.Length; i++) {
                action2hintLeft.Add(leftHand[i].action, leftHand[i]);
            }
            for (int i = 0; i < rightHand.Length; i++) {
                action2hintRight.Add(rightHand[i].action, rightHand[i]);
            }
        }
        void Update () {
#if UNITY_EDITOR
            AdjustHints () ;
#endif
        }


        void AdjustHints () {

            Vector3 textOffset = rightHand[0].textOffset;

            for (int i = 0; i < rightHand.Length; i++) {
                HintUIElement re = rightHand[i];
                ControllerHintUIElement rElement = re.element;
                rElement.transform.localScale = Vector3.one * hintScale;
                
                rElement.text.transform.localScale = Vector3.one * textScale;
                rElement.text.transform.localPosition = textOffset;//rightHand[i].textOffset;
                TransformBehavior.AdjustTransform(rElement.transform, Player.instance.rightHand.transform, hintTransforms, i);

                leftHand[i].element.transform.localScale = Vector3.one * hintScale;
                leftHand[i].element.text.transform.localScale = Vector3.one * textScale;
                leftHand[i].element.text.transform.localPosition = textOffset;//leftHand[i].textOffset;
                TransformBehavior.AdjustTransform(leftHand[i].element.transform, Player.instance.leftHand.transform, hintTransforms, i, new Vector3(-1,1,1));
            }
        }


        public static VRControllerHintsUI instance;


        static Dictionary<StandardizedVRInput.InputType, HintUIElement> action2hintLeft = new  Dictionary<StandardizedVRInput.InputType, HintUIElement>(), action2hintRight = new  Dictionary<StandardizedVRInput.InputType, HintUIElement>();

        
        public static void ShowHint (StandardizedVRInput.InputType action, SteamVR_Input_Sources forHand, string text) {
            
            Debug.LogError("showing here");
            if (forHand == SteamVR_Input_Sources.Any) {
            Debug.LogError("showing hand " + forHand);
            
                action2hintLeft[action].element.Show(text);
                action2hintRight[action].element.Show(text);
            }
            else if (forHand == SteamVR_Input_Sources.LeftHand) {
            Debug.LogError("showing hand " + forHand);
            
                action2hintLeft[action].element.Show(text);
            }
            else if (forHand == SteamVR_Input_Sources.RightHand) {
            Debug.LogError("showing hand " + forHand);
            
                action2hintRight[action].element.Show(text);
            }
        }
        public static void HideHint (StandardizedVRInput.InputType action, SteamVR_Input_Sources forHand) {
            if (forHand == SteamVR_Input_Sources.Any) {
                action2hintLeft[action].element.Hide();
                action2hintRight[action].element.Hide();
            }
            else if (forHand == SteamVR_Input_Sources.LeftHand) {
                action2hintLeft[action].element.Hide();
            }
            else if (forHand == SteamVR_Input_Sources.RightHand) {
                action2hintRight[action].element.Hide();
            }

        }
        public static void HideAllHints () {
            for (int i = 0; i < instance.leftHand.Length; i++) {
                instance.leftHand[i].element.Hide();

                
            }
            for (int i = 0; i < instance.rightHand.Length; i++) {
                instance.rightHand[i].element.Hide();
                
            }
        }

            
    }
}