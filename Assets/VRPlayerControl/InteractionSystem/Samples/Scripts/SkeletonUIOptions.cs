//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using UnityEngine;
using System.Collections;
using Valve.VR.InteractionSystem;


using VRPlayer;

using InteractionSystem;
namespace Valve.VR.InteractionSystem.Sample
{
    public class SkeletonUIOptions : MonoBehaviour
    {
        public Interactable showControllerInteractable, hideControllerInteractable;
        public Interactable animateWithControllerInteractable, animateWithoutControllerInteractable;

        void OnEnable () {
			if (showControllerInteractable != null)
				showControllerInteractable.onUseStart += ShowController;
			if (hideControllerInteractable != null)
				hideControllerInteractable.onUseStart += HideController;


            if (animateWithControllerInteractable != null)
				animateWithControllerInteractable.onUseStart += AnimateHandWithController;
			if (animateWithoutControllerInteractable != null)
				animateWithoutControllerInteractable.onUseStart += AnimateHandWithoutController;
		}
		void OnDisable () {
			if (showControllerInteractable != null)
				showControllerInteractable.onUseStart -= ShowController;
			if (hideControllerInteractable != null)
				hideControllerInteractable.onUseStart -= HideController;
			

            if (animateWithControllerInteractable != null)
				animateWithControllerInteractable.onUseStart -= AnimateHandWithController;
			if (animateWithoutControllerInteractable != null)
				animateWithoutControllerInteractable.onUseStart -= AnimateHandWithoutController;
			
		}





        public void AnimateHandWithController(Interactor user, int useAction)
        {
            for (int handIndex = 0; handIndex < Player.instance.hands.Length; handIndex++)
            {
                Hand hand = Player.instance.hands[handIndex];
                if (hand != null)
                {
                    hand.SetSkeletonRangeOfMotion(Valve.VR.EVRSkeletalMotionRange.WithController);
                }
            }
        }

        public void AnimateHandWithoutController(Interactor user, int useAction)
        {
            for (int handIndex = 0; handIndex < Player.instance.hands.Length; handIndex++)
            {
                Hand hand = Player.instance.hands[handIndex];
                if (hand != null)
                {
                    hand.SetSkeletonRangeOfMotion(Valve.VR.EVRSkeletalMotionRange.WithoutController);
                }
            }
        }

        public void ShowController(Interactor user, int useAction)
        {
            for (int handIndex = 0; handIndex < Player.instance.hands.Length; handIndex++)
            {
                Hand hand = Player.instance.hands[handIndex];
                if (hand != null)
                {
                    hand.ShowController(true);
                }
            }
        }

        public void SetRenderModel(RenderModelChangerUI prefabs)
        {
            for (int handIndex = 0; handIndex < Player.instance.hands.Length; handIndex++)
            {
                Hand hand = Player.instance.hands[handIndex];
                if (hand != null)
                {
                    if (hand.handType == SteamVR_Input_Sources.RightHand)
                        hand.SetRenderModel(prefabs.rightPrefab);
                    if (hand.handType == SteamVR_Input_Sources.LeftHand)
                        hand.SetRenderModel(prefabs.leftPrefab);
                }
            }
        }

        public void HideController(Interactor user, int useAction)
        {
            for (int handIndex = 0; handIndex < Player.instance.hands.Length; handIndex++)
            {
                Player.instance.hands[handIndex].HideController(true);
                
            }
        }
    }
}