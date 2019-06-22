using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;





namespace VRPlayer {
    /*
        Add this to the list of main menu options in the game
    */
    public class VRMainMenuAddOn : MonoBehaviour
    {
        public void ShowTextHints ( )
		{
			StandardizedVRInput.instance.PlayDebugRoutine();
		}
		public void DisableHints ( )
		{
			StandardizedVRInput.instance.StopHintRoutine();
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
        public void RecalibrateRealLifeHeight (GameObject[] data) {
            Player.instance.RecalibrateRealLifeHeight();
        }
    }
}
