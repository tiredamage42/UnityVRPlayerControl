//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Basic throwable object
//
//=============================================================================

using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using InventorySystem;
namespace Valve.VR.InteractionSystem
{
    public class ModalThrowable : Throwable, IInventoryItem
    {

        public void OnEquippedUpdate (Inventory inventory) {
            if (interactable.skeletonPoser != null)
            {
                bool enablePinchPos = offsetIndex == 1;
                interactable.skeletonPoser.SetBlendingBehaviourEnabled("PinchPose", enablePinchPos);//hand.currentAttachedObjectInfo.Value.grabbedWithType == GrabTypes.Pinch);
            }
        }


        public void OnEquipped(Inventory inventory) {

        }
        public void OnUnequipped (Inventory inventory) {

        }
       
		// void OnInspectStart(Interactor interactor) {

		// }
        // void OnInspectEnd(Interactor interactor){

		// }
        // void OnInspectUpdate(Interactor interactor){

		// }
        // void OnUseStart(Interactor interactor, int useIndex){

		// }
        // void OnUseEnd(Interactor interactor, int useIndex){
			
		// }
        // void OnUseUpdate(Interactor interactor, int useIndex){

		// }



        [Tooltip("The local point which acts as a positional and rotational offset to use while held with a grip type grab")]
        public Transform gripOffset;

        [Tooltip("The local point which acts as a positional and rotational offset to use while held with a pinch type grab")]
        public Transform pinchOffset;

        [Header("0-original 1-pinch 2-grip")]
        [Range(0,2)] public int offsetIndex = 0;
        
        // protected override void HandHoverUpdate(Hand hand)
        // {
        //     if (hand.GetGrabDown())
        //     {
        //         if (offsetIndex == 1)
        //         {
        //             hand.AttachObject(gameObject, attachmentFlags, pinchOffset);
        //         }
        //         else if (offsetIndex == 2)
        //         {
        //             hand.AttachObject(gameObject, attachmentFlags, gripOffset);
        //         }
        //         else
        //         {
        //             hand.AttachObject(gameObject, attachmentFlags, attachmentOffset);
        //         }
        //     }
        // }
        // protected override void HandAttachedUpdate(Hand hand)
        // {
        //     if (interactable.skeletonPoser != null)
        //     {
        //         bool enablePinchPos = offsetIndex == 1;
        //         interactable.skeletonPoser.SetBlendingBehaviourEnabled("PinchPose", enablePinchPos);//hand.currentAttachedObjectInfo.Value.grabbedWithType == GrabTypes.Pinch);
        //     }

        //     base.HandAttachedUpdate(hand);
        // }
    }
}