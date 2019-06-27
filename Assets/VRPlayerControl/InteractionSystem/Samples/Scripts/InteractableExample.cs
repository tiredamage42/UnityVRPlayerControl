﻿//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Demonstrates how to create a simple interactable object
//
//=============================================================================

using UnityEngine;
using System.Collections;

using InteractionSystem;
using InventorySystem;
using VRPlayer;
namespace Valve.VR.InteractionSystem.Sample
{
	//-------------------------------------------------------------------------
	[RequireComponent( typeof( Interactable ) )]
	public class InteractableExample : MonoBehaviour, ISceneItem, IInteractable
    {

		public void OnEquippedUseStart(Inventory inventory, int useIndex) {}
        public void OnEquippedUseEnd(Inventory inventory, int useIndex) {}
        public void OnEquippedUseUpdate(Inventory inventory, int useIndex) {}



		public void OnEquipped(Inventory inventory) {
            generalText.text = string.Format("Attached: {0}", inventory.name);
            attachTime = Time.time;
		}
		
		public void OnUnequipped(Inventory inventory) {
			generalText.text = string.Format("Detached: {0}", inventory.name);
		}
		public void OnEquippedUpdate (Inventory inventory) {
			generalText.text = string.Format("Attached: {0} :: Time: {1:F2}", inventory.name, (Time.time - attachTime));
		}
					
		public void OnInspectedStart(Interactor interactor) {
			generalText.text = "Hovering hand: " + interactor.name;

		}
        public void OnInspectedEnd(Interactor interactor){
			generalText.text = "No Hand Hovering";
		}

        public void OnInspectedUpdate(Interactor interactor){

		}
        public void OnUsedStart(Interactor interactor, int useIndex){

			// Save our position/rotation so that we can restore it when we detach
			oldPosition = transform.position;
			oldRotation = transform.rotation;

			// Call this to continue receiving HandHoverUpdate messages,
			// and prevent the hand from hovering over anything else
			interactor.HoverLock(interactable);

			// Attach this object to the hand
			// hand.AttachObject(gameObject, attachmentFlags);
		}

        public void OnUsedEnd(Interactor interactor, int useIndex){
			

			// hand.DetachObject(gameObject);

			// Call this to undo HoverLock
			interactor.HoverUnlock(interactable);

			// Restore position/rotation
			transform.position = oldPosition;
			transform.rotation = oldRotation;
		}
			
        public void OnUsedUpdate(Interactor interactor, int useIndex){

		}

        private TextMesh generalText;
        private TextMesh hoveringText;
        private Vector3 oldPosition;
		private Quaternion oldRotation;

		private float attachTime;

		// private Hand.AttachmentFlags attachmentFlags = Hand.defaultAttachmentFlags & ( ~Hand.AttachmentFlags.SnapOnAttach ) & (~Hand.AttachmentFlags.DetachOthers) & (~Hand.AttachmentFlags.VelocityMovement);

        private Interactable interactable;

		//-------------------------------------------------
		void Awake()
		{
			// GetComponent<Item>().attachmentFlags = attachmentFlags;
			
			var textMeshs = GetComponentsInChildren<TextMesh>();
            generalText = textMeshs[0];
            hoveringText = textMeshs[1];

            generalText.text = "No Hand Hovering";
            hoveringText.text = "Hovering: False";

            interactable = this.GetComponent<Interactable>();
		}

		// void OnEnable () {

		// 	interactable.onInspectStart;
		// 	interactable.onInspectEnd += ;

		// 	interactable.onEquipped += OnEquipped;
		// 	interactable.onUnequipped += OnUnequipped;
		// }
		// void OnDisable () {
		// 	interactable.onEquipped -= OnEquipped;
		// 	interactable.onUnequipped -= OnUnequipped;
		// }


		//-------------------------------------------------
		// Called when a Hand starts hovering over this object
		//-------------------------------------------------
		// private void OnHandHoverBegin( Hand hand )
		// {
		// 	generalText.text = "Hovering hand: " + hand.name;
		// }



		//-------------------------------------------------
		// Called when a Hand stops hovering over this object
		//-------------------------------------------------
		// private void OnHandHoverEnd( Hand hand )
		// {
		// 	generalText.text = "No Hand Hovering";
		// }


		//-------------------------------------------------
		// Called every Update() while a Hand is hovering over this object
		// //-------------------------------------------------
		// private void HandHoverUpdate( Hand hand )
		// {
		// 	bool grabDown = hand.GetGrabDown();

        //     // GrabTypes startingGrabType = hand.GetGrabStarting();
        //     // bool isGrabEnding = hand.IsGrabEnding(this.gameObject);
			
		// 	bool noGrab = hand.GetGrabUp();

        //     if (interactable.attachedToHand == null && grabDown)//startingGrabType != GrabTypes.None)
        //     {
        //         // // Save our position/rotation so that we can restore it when we detach
        //         // oldPosition = transform.position;
        //         // oldRotation = transform.rotation;

        //         // // Call this to continue receiving HandHoverUpdate messages,
        //         // // and prevent the hand from hovering over anything else
        //         // hand.HoverLock(interactable);

        //         // // Attach this object to the hand
        //         // hand.AttachObject(gameObject, 
		// 		// 	// startingGrabType, 
		// 		// 	attachmentFlags);
        //     }
        //     else if (noGrab)//isGrabEnding)
        //     {

		// 		//check if we're attached first

        //         // Detach this object from the hand
        //         hand.DetachObject(gameObject);

        //         // Call this to undo HoverLock
        //         hand.HoverUnlock(interactable);

        //         // Restore position/rotation
        //         transform.position = oldPosition;
        //         transform.rotation = oldRotation;
        //     }
		// }


		// //-------------------------------------------------
		// // Called when this GameObject becomes attached to the hand
		// //-------------------------------------------------
		// private void OnEquipped( Object hand )
        // {
        //     generalText.text = string.Format("Attached: {0}", hand.name);
        //     attachTime = Time.time;
		// }
        


		// //-------------------------------------------------
		// // Called when this GameObject is detached from the hand
		// //-------------------------------------------------
		// private void OnUnequipped( Object hand )
		// {
        //     generalText.text = string.Format("Detached: {0}", hand.name);
		// }


		//-------------------------------------------------
		// Called every Update() while this GameObject is attached to the hand
		//-------------------------------------------------
		// private void HandAttachedUpdate( Hand hand )
		// {
        //     generalText.text = string.Format("Attached: {0} :: Time: {1:F2}", hand.name, (Time.time - attachTime));
		// }

        // private bool lastHovering = false;
        private void Update()
        {
            // if (interactable.isHovering != lastHovering) //save on the .tostrings a bit
            // {
            //     hoveringText.text = string.Format("Hovering: {0}", interactable.isHovering);
            //     lastHovering = interactable.isHovering;
            // }
        }


		//-------------------------------------------------
		// Called when this attached GameObject becomes the primary attached object
		//-------------------------------------------------
		// private void OnHandFocusAcquired( Hand hand )
		// {
		// 	Debug.LogError("ON HAND FOCUS ACQUIRED " + name);
			
		// }


		// //-------------------------------------------------
		// // Called when another attached GameObject becomes the primary attached object
		// //-------------------------------------------------
		// private void OnHandFocusLost( Hand hand )
		// {
		// 	Debug.LogError("ON HAND FOCUS Lost " + name);
			
		// }
	}
}
