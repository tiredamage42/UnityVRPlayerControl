using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Valve.VR;

namespace VRPlayer {

    public class StandardizedVRInput : MonoBehaviour
    {

    
        // public bool headsetIsOnPlayerHead;
        // [Tooltip(".15f is a good setting when\nstandalone input module has 60 actions per second")]  
        // public Vector2 deltaThresholdForScroll = new Vector2(.15f, .15f);
        
        // void Update()
        // {
        //     if (headsetOnHead.GetStateDown(SteamVR_Input_Sources.Head)) {
        //         // Debug.Log("<b>SteamVR Interaction System</b> Headset placed on head");
        //         headsetIsOnPlayerHead = true;
        //     }
        //     else if (headsetOnHead.GetStateUp(SteamVR_Input_Sources.Head)) {
        //         // Debug.Log("<b>SteamVR Interaction System</b> Headset removed");
        //         headsetIsOnPlayerHead = false;
        //     }
        //     else if (headsetOnHead.GetState(SteamVR_Input_Sources.Head)) {
        //         headsetIsOnPlayerHead = true;
        //     }
        //     CalculateScrollDeltas();
        // }


        // public Vector2 GetScrollDelta (SteamVR_Input_Sources hand) {
        //     if (hand == SteamVR_Input_Sources.Any) return new Vector2(savedAxis.x, savedAxis.y) + new Vector2(savedAxis.z, savedAxis.w);
        //     int handOffset = 2*VRManager.Hand2Int(hand);
        //     return new Vector2(savedAxis[handOffset], savedAxis[handOffset+1]);
        // }
        // Vector4 savedAxis, lastAxis;
        // void CalculateScrollDeltas () {
        //     for (int i = 0; i < 2; i++) {
        //         int startIndex = 2*i;
        //         Vector2 current = TrackpadAxis.GetAxis(VRManager.Int2Hand(i));
        //         for (int x = 0; x < 2; x++) savedAxis[startIndex+x] = GetAxisRaw(startIndex+x, current[x], deltaThresholdForScroll[x]);
        //     }
        // }

        // /*
        //     make axis react to scrolling action on trackpad
        // */
        // float GetAxisRaw(int axisIndex, float currentAxis, float currentThreshold) {

        //     float delta = currentAxis - lastAxis[axisIndex];
        //     float returnAxis = 0;
        //     if (delta != 0 && Mathf.Abs(delta) >= currentThreshold){
        //         if (lastAxis[axisIndex] == 0 || currentAxis == 0) {
        //             // if (lastAxis[axisIndex] == 0) 
        //             //     Debug.LogError("on scroll start");
        //             // else 
        //             //     Debug.LogError("on scroll end");
                    
        //         }
        //         else {
        //             returnAxis = Mathf.Clamp(delta * 99999, -1, 1);
        //         }
        //         lastAxis[axisIndex] = currentAxis;
        //     }
        //     return returnAxis;
        // }


        // // static Dictionary<SteamVR_Action, SteamVR_Input_Sources> occupiedActions = new Dictionary<SteamVR_Action, SteamVR_Input_Sources>();

        // // public static void MarkActionOccupied(SteamVR_Action action, SteamVR_Input_Sources forHand) {
        // //     occupiedActions[action] = forHand;
        // // }
        // // public static void MarkActionUnoccupied(SteamVR_Action action) {
        // //     occupiedActions[action] = VRManager.errorVRSource;
        // // }
        // // public static bool ActionOccupied (SteamVR_Action action, SteamVR_Input_Sources forHand) {
        // //     if (VRUIInput.ActionOccupied(action, forHand)) {
        // //         return true;
        // //     }
            
        // //     SteamVR_Input_Sources handValue;
        // //     if (occupiedActions.TryGetValue(action, out handValue)) {
        // //         return handValue != SteamVR_Input_Sources.Keyboard && forHand == handValue;
        // //     }
        // //     else {
        // //         return false;
        // //     }
        // // }
        



        // static StandardizedVRInput _instance;
        // public static StandardizedVRInput instance {
        //     get {
        //         if (_instance == null) {
        //             _instance = GameObject.FindObjectOfType<StandardizedVRInput>();
        //         }
        //         return _instance;
        //     }
        // }

        // public SteamVR_Action_Vector2 TrackpadAxis;

        // [Tooltip("This action lets you know when the player has placed the headset on their head")]
        // public SteamVR_Action_Boolean headsetOnHead = SteamVR_Input.GetBooleanAction("HeadsetOnHead");
        // public SteamVR_Action_Vibration hapticAction = SteamVR_Input.GetAction<SteamVR_Action_Vibration>("Haptic");

        // public void TriggerHapticPulse(SteamVR_Input_Sources hand, ushort microSecondsDuration)
        // {
        //     float seconds = (float)microSecondsDuration / 1000000f;
        //     hapticAction.Execute(0, seconds, 1f / seconds, 1, hand);
        // }

        // public void TriggerHapticPulse(SteamVR_Input_Sources hand, float duration, float frequency, float amplitude)
        // {
        //     hapticAction.Execute(0, duration, frequency, amplitude, hand);
        // }

    }







}
