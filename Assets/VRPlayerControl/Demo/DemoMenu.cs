using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace VRPlayer {

    public class DemoMenu : MonoBehaviour
    {
        // Start is called before the first frame update
        // void Start()
        // {
            
        // }

        // // Update is called once per frame
        // void Update()
        // {
            
        // }

        public void ShowTextHints ( )
		{
			StandardizedVRInput.instance.PlayDebugRoutine();
		}
		public void DisableHints ( )
		{
			StandardizedVRInput.instance.StopHintRoutine();
		}


        public void AnimateHandWithController()
        {
            for (int handIndex = 0; handIndex < Player.instance.hands.Length; handIndex++)
            {
                Hand hand = Player.instance.hands[handIndex];
                hand.SetSkeletonRangeOfMotion(Valve.VR.EVRSkeletalMotionRange.WithController);
            }
        }

        public void AnimateHandWithoutController()
        {
            for (int handIndex = 0; handIndex < Player.instance.hands.Length; handIndex++)
            {
                Hand hand = Player.instance.hands[handIndex];
                hand.SetSkeletonRangeOfMotion(Valve.VR.EVRSkeletalMotionRange.WithoutController);
            }
        }

        public void ShowController()
        {
            for (int handIndex = 0; handIndex < Player.instance.hands.Length; handIndex++)
            {
                Hand hand = Player.instance.hands[handIndex];
                hand.ShowController(true);
            }
        }

        public void SetRenderModel(GameObject[] data)
        {
            if (data.Length < 2) {
                return;
            }


            for (int handIndex = 0; handIndex < Player.instance.hands.Length; handIndex++)
            {
                Hand hand = Player.instance.hands[handIndex];
                if (hand.handType == SteamVR_Input_Sources.RightHand)
                    hand.SetRenderModel(data[0]);
                if (hand.handType == SteamVR_Input_Sources.LeftHand)
                    hand.SetRenderModel(data[1]);
                
            }
        }

        public void HideController()
        {
            for (int handIndex = 0; handIndex < Player.instance.hands.Length; handIndex++)
            {
                Player.instance.hands[handIndex].HideController(true);
                
            }
        }
    }
}
