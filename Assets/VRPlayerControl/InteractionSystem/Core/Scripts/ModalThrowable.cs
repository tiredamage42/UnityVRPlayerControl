//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Basic throwable object
//
//=============================================================================

using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace Valve.VR.InteractionSystem
{
    public class ModalThrowable : Throwable
    {
        [Tooltip("The local point which acts as a positional and rotational offset to use while held with a grip type grab")]
        public Transform gripOffset;

        [Tooltip("The local point which acts as a positional and rotational offset to use while held with a pinch type grab")]
        public Transform pinchOffset;

        [Header("0-original 1-pinch 2-grip")]
        [Range(0,2)] public int offsetIndex = 0;
        
        protected override void HandHoverUpdate(Hand hand)
        {
            // GrabTypes startingGrabType = hand.GetGrabStarting();

            // if (startingGrabType != GrabTypes.None)
            if (hand.GetGrabDown())
            {
                if (offsetIndex == 1)
                // if (startingGrabType == GrabTypes.Pinch)
                {
                    hand.AttachObject(gameObject, 
                        // startingGrabType, 
                        attachmentFlags, pinchOffset);
                }
                else if (offsetIndex == 2)
                
                // else if (startingGrabType == GrabTypes.Grip)
                {
                    hand.AttachObject(gameObject, 
                    // startingGrabType, 
                    attachmentFlags, gripOffset);
                }
                else
                {
                    hand.AttachObject(gameObject, 
                    // startingGrabType, 
                    attachmentFlags, attachmentOffset);
                }

                hand.HideGrabHint();
            }
        }
        protected override void HandAttachedUpdate(Hand hand)
        {
            if (interactable.skeletonPoser != null)
            {
                bool enablePinchPos = offsetIndex ==1;
                interactable.skeletonPoser.SetBlendingBehaviourEnabled("PinchPose", enablePinchPos);//hand.currentAttachedObjectInfo.Value.grabbedWithType == GrabTypes.Pinch);
            }

            base.HandAttachedUpdate(hand);
        }
    }
}