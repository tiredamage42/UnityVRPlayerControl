﻿//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: The object attached to the player's hand that spawns and fires the
//			arrow
//
//=============================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using InventorySystem;
using InteractionSystem;
namespace Valve.VR.InteractionSystem
{
	//-------------------------------------------------------------------------
	public class ArrowHand : MonoBehaviour, IInventoryItem, IInteractable
	{
		// private Hand hand;
		private Longbow bow;

		private GameObject currentArrow;
		public GameObject arrowPrefab;

		public Transform arrowNockTransform;

		public float nockDistance = 0.1f;
		public float lerpCompleteDistance = 0.08f;
		public float rotationLerpThreshold = 0.15f;
		public float positionLerpThreshold = 0.15f;

		private bool allowArrowSpawn = true;
		private bool nocked;
        // private GrabTypes nockedWithType = GrabTypes.None;

		private bool inNockRange = false;
		private bool arrowLerpComplete = false;

		public SoundPlayOneshot arrowSpawnSound;

		// private AllowTeleportWhileAttachedToHand allowTeleport = null;

		public int maxArrowCount = 10;
		private List<GameObject> arrowList;

		// Interactable interactable;
		//-------------------------------------------------
		void Awake()
		{
			// interactable = GetComponent<Interactable>();
			// allowTeleport = GetComponent<AllowTeleportWhileAttachedToHand>();
			
			// //allowTeleport.teleportAllowed = true;
			
			// allowTeleport.overrideHoverLock = false;

			arrowList = new List<GameObject>();

			// GetComponent<Item>().attachmentFlags = Inventory.defaultAttachmentFlags;
		}

		// void OnEnable () {
		// 	interactable.onEquipped += OnEquipped;
		// 	interactable.onUnequipped += OnUnequipped;
		// }
		// void OnDisable () {
		// 	interactable.onEquipped -= OnEquipped;
		// 	interactable.onUnequipped -= OnUnequipped;
		// }



		//-------------------------------------------------
		public void OnEquipped(Inventory inventory)// Object attachedHand )
		{
			// hand = (Hand)attachedHand;
			FindBow(inventory);
		}
		public void OnEquippedUpdate (Inventory inventory) {

			if ( bow == null )
			{
				FindBow(inventory);
			}

			if ( bow == null )
			{
				return;
			}

			if ( allowArrowSpawn && ( currentArrow == null ) ) // If we're allowed to have an active arrow in hand but don't yet, spawn one
			{
				currentArrow = InstantiateArrow();
				arrowSpawnSound.Play();
			}

			float distanceToNockPosition = Vector3.Distance( transform.parent.position, bow.nockTransform.position );

			// If there's an arrow spawned in the hand and it's not nocked yet
			if ( !nocked )
			{
				// If we're close enough to nock position that we want to start arrow rotation lerp, do so
				if ( distanceToNockPosition < rotationLerpThreshold )
				{
					float lerp = Util.RemapNumber( distanceToNockPosition, rotationLerpThreshold, lerpCompleteDistance, 0, 1 );

					arrowNockTransform.rotation = Quaternion.Lerp( arrowNockTransform.parent.rotation, bow.nockRestTransform.rotation, lerp );
				}
				else // Not close enough for rotation lerp, reset rotation
				{
					arrowNockTransform.localRotation = Quaternion.identity;
				}

				// If we're close enough to the nock position that we want to start arrow position lerp, do so
				if ( distanceToNockPosition < positionLerpThreshold )
				{
					float posLerp = Util.RemapNumber( distanceToNockPosition, positionLerpThreshold, lerpCompleteDistance, 0, 1 );

					posLerp = Mathf.Clamp( posLerp, 0f, 1f );

					arrowNockTransform.position = Vector3.Lerp( arrowNockTransform.parent.position, bow.nockRestTransform.position, posLerp );
				}
				else // Not close enough for position lerp, reset position
				{
					arrowNockTransform.position = arrowNockTransform.parent.position;
				}


				// Give a haptic tick when lerp is visually complete
				if ( distanceToNockPosition < lerpCompleteDistance )
				{
					if ( !arrowLerpComplete )
					{
						arrowLerpComplete = true;
						// hand.TriggerHapticPulse( 500 );
					}
				}
				else
				{
					if ( arrowLerpComplete )
					{
						arrowLerpComplete = false;
					}
				}

				// Allow nocking the arrow when controller is close enough
				if ( distanceToNockPosition < nockDistance )
				{
					if ( !inNockRange )
					{
						inNockRange = true;
						bow.ArrowInPosition();
					}
				}
				else
				{
					if ( inNockRange )
					{
						inNockRange = false;
					}
				}

            //     // GrabTypes bestGrab = hand.GetBestGrabbingType(GrabTypes.Pinch, true);
			// 	bool grabDown = hand.GetGrabDown();

            //     // If arrow is close enough to the nock position and we're pressing the trigger, and we're not nocked yet, Nock
            //     // if ( ( distanceToNockPosition < nockDistance ) && bestGrab != GrabTypes.None && !nocked )
				
			// 	if ( ( distanceToNockPosition < nockDistance ) && grabDown && !nocked )
				
			// 	{
			// 		if ( currentArrow == null )
			// 		{
			// 			currentArrow = InstantiateArrow();
			// 		}

			// 		nocked = true;
            //         // nockedWithType = bestGrab;
			// 		bow.StartNock( this );
			// 		hand.HoverLock( GetComponent<Interactable>() );
			// 		Debug.Log("hoverLocking " + hand.name);
					
			// 		// allowTeleport.teleportAllowed = false;
					
			// 		currentArrow.transform.parent = bow.nockTransform;
			// 		Util.ResetTransform( currentArrow.transform );
			// 		Util.ResetTransform( arrowNockTransform );
			// 	}
			}


			
		}


        public void OnEquippedUseUpdate(Inventory inventory, int useIndex) {}


		public void OnEquippedUseStart(Inventory inventory, int useIndex) {
		// public void OnUseStart (Interactor interactor, int useIndex) {
				Debug.LogError("nocked");
                // If arrow is close enough to the nock position and we're pressing the trigger, and we're not nocked yet, Nock
                // if ( ( distanceToNockPosition < nockDistance ) && bestGrab != GrabTypes.None && !nocked )
				
				if ( inNockRange && !nocked )
				
				{
					if ( currentArrow == null )
					{
						currentArrow = InstantiateArrow();
					}

					nocked = true;
                    // nockedWithType = bestGrab;
					bow.StartNock( this );
					inventory.GetComponent<Interactor>().HoverLock( GetComponent<Interactable>() );
					Debug.Log("hoverLocking " + inventory.name);
					
					// allowTeleport.teleportAllowed = false;
					
					currentArrow.transform.parent = bow.nockTransform;
					Util.ResetTransform( currentArrow.transform );
					Util.ResetTransform( arrowNockTransform );
				}
		}

public void OnEquippedUseEnd(Inventory inventory, int useIndex) {
        Debug.LogError("suse end arrow");
		// public void OnUseEnd (Interactor interactor, int useIndex) {
			// If arrow is nocked, and we release the trigger
			// if ( nocked && hand.IsGrabbingWithType(nockedWithType) == false )
			if ( nocked )
			
			{
				Debug.LogError("suse end arrow nocked");
		
				if ( bow.pulled ) // If bow is pulled back far enough, fire arrow, otherwise reset arrow in arrowhand
				{
					FireArrow();
				}
				else
				{
					arrowNockTransform.rotation = currentArrow.transform.rotation;
					currentArrow.transform.parent = arrowNockTransform;
					Util.ResetTransform( currentArrow.transform );
					nocked = false;
                    // nockedWithType = GrabTypes.None;
					bow.ReleaseNock();

					inventory.GetComponent<Interactor>().HoverUnlock( GetComponent<Interactable>() );
					Debug.Log("hover unLocking " + inventory.name);

					// allowTeleport.teleportAllowed = true;
				}

				bow.StartRotationLerp(); // Arrow is releasing from the bow, tell the bow to lerp back to controller rotation
			}

		}


public void OnUseStart (Interactor interactor, int useIndex) {
}
public void OnUseEnd (Interactor interactor, int useIndex) {
}
		public void OnUseUpdate (Interactor interactor, int useIndex) {

		}

		public void OnInspectStart (Interactor interactor) {

		}
		public void OnInspectEnd (Interactor interactor) {

		}
		public void OnInspectUpdate (Interactor interactor) {

		}







		//-------------------------------------------------
		private GameObject InstantiateArrow()
		{
			GameObject arrow = Instantiate( arrowPrefab, arrowNockTransform.position, arrowNockTransform.rotation ) as GameObject;
			arrow.name = "Bow Arrow";
			arrow.transform.parent = arrowNockTransform;
			Util.ResetTransform( arrow.transform );

			arrowList.Add( arrow );

			while ( arrowList.Count > maxArrowCount )
			{
				GameObject oldArrow = arrowList[0];
				arrowList.RemoveAt( 0 );
				if ( oldArrow )
				{
					Destroy( oldArrow );
				}
			}

			return arrow;
		}


		//-------------------------------------------------
		// private void HandAttachedUpdate( Hand hand )
		// {
		// 	if ( bow == null )
		// 	{
		// 		FindBow();
		// 	}

		// 	if ( bow == null )
		// 	{
		// 		return;
		// 	}

		// 	if ( allowArrowSpawn && ( currentArrow == null ) ) // If we're allowed to have an active arrow in hand but don't yet, spawn one
		// 	{
		// 		currentArrow = InstantiateArrow();
		// 		arrowSpawnSound.Play();
		// 	}

		// 	float distanceToNockPosition = Vector3.Distance( transform.parent.position, bow.nockTransform.position );

		// 	// If there's an arrow spawned in the hand and it's not nocked yet
		// 	if ( !nocked )
		// 	{
		// 		// If we're close enough to nock position that we want to start arrow rotation lerp, do so
		// 		if ( distanceToNockPosition < rotationLerpThreshold )
		// 		{
		// 			float lerp = Util.RemapNumber( distanceToNockPosition, rotationLerpThreshold, lerpCompleteDistance, 0, 1 );

		// 			arrowNockTransform.rotation = Quaternion.Lerp( arrowNockTransform.parent.rotation, bow.nockRestTransform.rotation, lerp );
		// 		}
		// 		else // Not close enough for rotation lerp, reset rotation
		// 		{
		// 			arrowNockTransform.localRotation = Quaternion.identity;
		// 		}

		// 		// If we're close enough to the nock position that we want to start arrow position lerp, do so
		// 		if ( distanceToNockPosition < positionLerpThreshold )
		// 		{
		// 			float posLerp = Util.RemapNumber( distanceToNockPosition, positionLerpThreshold, lerpCompleteDistance, 0, 1 );

		// 			posLerp = Mathf.Clamp( posLerp, 0f, 1f );

		// 			arrowNockTransform.position = Vector3.Lerp( arrowNockTransform.parent.position, bow.nockRestTransform.position, posLerp );
		// 		}
		// 		else // Not close enough for position lerp, reset position
		// 		{
		// 			arrowNockTransform.position = arrowNockTransform.parent.position;
		// 		}


		// 		// Give a haptic tick when lerp is visually complete
		// 		if ( distanceToNockPosition < lerpCompleteDistance )
		// 		{
		// 			if ( !arrowLerpComplete )
		// 			{
		// 				arrowLerpComplete = true;
		// 				hand.TriggerHapticPulse( 500 );
		// 			}
		// 		}
		// 		else
		// 		{
		// 			if ( arrowLerpComplete )
		// 			{
		// 				arrowLerpComplete = false;
		// 			}
		// 		}

		// 		// Allow nocking the arrow when controller is close enough
		// 		if ( distanceToNockPosition < nockDistance )
		// 		{
		// 			if ( !inNockRange )
		// 			{
		// 				inNockRange = true;
		// 				bow.ArrowInPosition();
		// 			}
		// 		}
		// 		else
		// 		{
		// 			if ( inNockRange )
		// 			{
		// 				inNockRange = false;
		// 			}
		// 		}

        //         // GrabTypes bestGrab = hand.GetBestGrabbingType(GrabTypes.Pinch, true);
		// 		bool grabDown = hand.GetGrabDown();

        //         // If arrow is close enough to the nock position and we're pressing the trigger, and we're not nocked yet, Nock
        //         // if ( ( distanceToNockPosition < nockDistance ) && bestGrab != GrabTypes.None && !nocked )
				
		// 		if ( ( distanceToNockPosition < nockDistance ) && grabDown && !nocked )
				
		// 		{
		// 			if ( currentArrow == null )
		// 			{
		// 				currentArrow = InstantiateArrow();
		// 			}

		// 			nocked = true;
        //             // nockedWithType = bestGrab;
		// 			bow.StartNock( this );
		// 			hand.HoverLock( GetComponent<Interactable>() );
		// 			Debug.Log("hoverLocking " + hand.name);
					
		// 			// allowTeleport.teleportAllowed = false;
					
		// 			currentArrow.transform.parent = bow.nockTransform;
		// 			Util.ResetTransform( currentArrow.transform );
		// 			Util.ResetTransform( arrowNockTransform );
		// 		}
		// 	}


		// 	// If arrow is nocked, and we release the trigger
		// 	// if ( nocked && hand.IsGrabbingWithType(nockedWithType) == false )
		// 	if ( nocked && hand.GetGrabUp())
			
		// 	{
		// 		if ( bow.pulled ) // If bow is pulled back far enough, fire arrow, otherwise reset arrow in arrowhand
		// 		{
		// 			FireArrow();
		// 		}
		// 		else
		// 		{
		// 			arrowNockTransform.rotation = currentArrow.transform.rotation;
		// 			currentArrow.transform.parent = arrowNockTransform;
		// 			Util.ResetTransform( currentArrow.transform );
		// 			nocked = false;
        //             // nockedWithType = GrabTypes.None;
		// 			bow.ReleaseNock();
		// 			hand.HoverUnlock( GetComponent<Interactable>() );
		// 			Debug.Log("hover unLocking " + hand.name);

		// 			// allowTeleport.teleportAllowed = true;
		// 		}

		// 		bow.StartRotationLerp(); // Arrow is releasing from the bow, tell the bow to lerp back to controller rotation
		// 	}
		// }


		//-------------------------------------------------
		public void OnUnequipped( Inventory inventory)// Object hand )
		{
			Destroy( gameObject );
		}


		//-------------------------------------------------
		private void FireArrow()
		{
			currentArrow.transform.parent = null;

			Arrow arrow = currentArrow.GetComponent<Arrow>();
			arrow.shaftRB.isKinematic = false;
			arrow.shaftRB.useGravity = true;
			arrow.shaftRB.transform.GetComponent<BoxCollider>().enabled = true;

			arrow.arrowHeadRB.isKinematic = false;
			arrow.arrowHeadRB.useGravity = true;
			arrow.arrowHeadRB.transform.GetComponent<BoxCollider>().enabled = true;

			arrow.arrowHeadRB.AddForce( currentArrow.transform.forward * bow.GetArrowVelocity(), ForceMode.VelocityChange );
			arrow.arrowHeadRB.AddTorque( currentArrow.transform.forward * 10 );

			nocked = false;
            // nockedWithType = GrabTypes.None;

			currentArrow.GetComponent<Arrow>().ArrowReleased( bow.GetArrowVelocity() );
			bow.ArrowReleased();

			allowArrowSpawn = false;
			Invoke( "EnableArrowSpawn", 0.5f );
			// StartCoroutine( ArrowReleaseHaptics() );

			currentArrow = null;
			// allowTeleport.teleportAllowed = true;
		}


		//-------------------------------------------------
		private void EnableArrowSpawn()
		{
			allowArrowSpawn = true;
		}


		//-------------------------------------------------
		// private IEnumerator ArrowReleaseHaptics()
		// {
		// 	yield return new WaitForSeconds( 0.05f );

		// 	hand.otherHand.TriggerHapticPulse( 1500 );
		// 	yield return new WaitForSeconds( 0.05f );

		// 	hand.otherHand.TriggerHapticPulse( 800 );
		// 	yield return new WaitForSeconds( 0.05f );

		// 	hand.otherHand.TriggerHapticPulse( 500 );
		// 	yield return new WaitForSeconds( 0.05f );

		// 	hand.otherHand.TriggerHapticPulse( 300 );
		// }


		//-------------------------------------------------
		// private void OnHandFocusLost( Hand hand )
		// {
		// 	Debug.LogError("ON HAND FOCUS Lost " + name);
			
		// 	gameObject.SetActive( false );
		// }


		// //-------------------------------------------------
		// private void OnHandFocusAcquired( Hand hand )
		// {
		// 	Debug.LogError("ON HAND FOCUS ACQUIRED " + name);
			
		// 	gameObject.SetActive( true );
		// }


		//-------------------------------------------------
		private void FindBow(Inventory inventory)
		{
			bow = inventory.otherInventory.GetComponentInChildren<Longbow>();
		}
	}
}
