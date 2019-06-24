//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: The bow
//
//=============================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using InventorySystem;
using InteractionSystem;
using VRPlayer;

namespace Valve.VR.InteractionSystem
{
	//-------------------------------------------------------------------------
	[RequireComponent( typeof( Interactable ) )]
	public class Longbow : MonoBehaviour, IInventoryItem
	{

		public void OnEquippedUseStart(Inventory inventory, int useIndex) {}
        public void OnEquippedUseEnd(Inventory inventory, int useIndex) {}
        public void OnEquippedUseUpdate(Inventory inventory, int useIndex) {}







		public float nockDistance = 0.1f;
		private bool inNockRange = false;
		
		


		void HandleNockPositioning (EquipPoint oppositeEquipPoint) {//, float nockDistance) {

			// If there's an arrow spawned in the hand and it's not nocked yet
			if ( !nocked )
			{

				float distanceToNockPosition = Vector3.Distance( oppositeEquipPoint.transform.position, nockTransform.position );
				
				// Allow nocking the arrow when controller is close enough
				if ( distanceToNockPosition < nockDistance )
				{
					if ( !inNockRange )
					{
						arrowHand.arrowNockTransform.position = nockRestTransform.position;
						arrowHand.arrowNockTransform.rotation = nockRestTransform.rotation;

						// linkedInventory hand.TriggerHapticPulse( 500 );
						ArrowInPosition();

						inNockRange = true;
					}
				}
				else
				{
					if ( inNockRange )
					{
						inNockRange = false;
					}
				}
			}
		}
		private GameObject InstantiateArrow()
		{
			

			GameObject arrow = Instantiate( arrowHand.arrowPrefab, arrowHand.arrowNockTransform.position, arrowHand.arrowNockTransform.rotation ) as GameObject;
			arrow.name = "Bow Arrow";
			arrow.transform.parent = arrowHand.arrowNockTransform;
			Util.ResetTransform( arrow.transform );

			// arrowList.Add( arrow );

			// while ( arrowList.Count > maxArrowCount )
			// {
			// 	GameObject oldArrow = arrowList[0];
			// 	arrowList.RemoveAt( 0 );
			// 	if ( oldArrow )
			// 	{
			// 		Destroy( oldArrow );
			// 	}
			// }

			return arrow;
		}

		public void NockArrow (Inventory linkedInventory){//, ArrowHand arrowHand) {
				Debug.LogError("nocked");
                // If arrow is close enough to the nock position and we're pressing the trigger, and we're not nocked yet, Nock
        		
				if ( inNockRange && !nocked )
				
				{
					if ( arrowHand.currentArrow == null )
					{
						arrowHand.currentArrow = InstantiateArrow();
					}

					// nocked = true;
                    StartNock( );//arrowHand );
					
					linkedInventory.GetComponent<Interactor>().HoverLock( null );// arrowhand.GetComponent<Interactable>() );
					
					arrowHand.currentArrow.transform.parent = nockTransform;
					Util.ResetTransform( arrowHand.currentArrow.transform );
					Util.ResetTransform( arrowHand.arrowNockTransform );
				}
		}
		public void StartNock( )//ArrowHand currentArrowHand )
		{
			Debug.LogError("started nock here");
			// arrowHand = currentArrowHand;
			
			// parentInventoryInteractor.HoverLock( GetComponent<Interactable>() );
			
			nocked = true;
			
			nockLerpStartTime = Time.time;
			nockLerpStartRotation = pivotTransform.rotation;

			// Sound of arrow sliding on nock as it's being pulled back
			arrowSlideSound.Play();

			// Decide which hand we're drawing with and lerp to the correct side
			DoHandednessCheck();
		}

		public void AttemptArrowFire (Inventory linkedInventory) {
			if ( nocked )
			
			{
				
				if ( pulled ) // If bow is pulled back far enough, fire arrow, otherwise reset arrow in arrowhand
				{
					FireArrow();
				}
				else
				{
					arrowHand.arrowNockTransform.rotation = arrowHand.currentArrow.transform.rotation;
					arrowHand.currentArrow.transform.parent = arrowHand.arrowNockTransform;
					Util.ResetTransform( arrowHand.currentArrow.transform );
					nocked = false;
                    ReleaseNock();

					linkedInventory.GetComponent<Interactor>().HoverUnlock( null );//arrowhand.GetComponent<Interactable>() );
					
				}

				// Arrow is releasing from the bow, tell the bow to lerp back to controller rotation
				StartRotationLerp(); 
			}
		}





		//-------------------------------------------------
		private void FireArrow()
		{
			arrowHand.currentArrow.transform.parent = null;

			Arrow arrow = arrowHand.currentArrow.GetComponent<Arrow>();
			arrow.shaftRB.isKinematic = false;
			arrow.shaftRB.useGravity = true;
			arrow.shaftRB.transform.GetComponent<BoxCollider>().enabled = true;

			arrow.arrowHeadRB.isKinematic = false;
			arrow.arrowHeadRB.useGravity = true;
			arrow.arrowHeadRB.transform.GetComponent<BoxCollider>().enabled = true;

			arrow.arrowHeadRB.AddForce( arrowHand.currentArrow.transform.forward * GetArrowVelocity(), ForceMode.VelocityChange );
			arrow.arrowHeadRB.AddTorque( arrowHand.currentArrow.transform.forward * 10 );

			// nocked = false;
            // nockedWithType = GrabTypes.None;

			arrowHand.currentArrow.GetComponent<Arrow>().ArrowReleased( GetArrowVelocity() );
			
			ArrowReleased();

			arrowHand.allowArrowSpawn = false;
			Invoke( "EnableArrowSpawn", 0.5f );
			// StartCoroutine( ArrowReleaseHaptics() );

			arrowHand.currentArrow = null;
		}
		//-------------------------------------------------
		public void ArrowReleased()
		{
			nocked = false;
			
			// parentInventoryInteractor.HoverUnlock( GetComponent<Interactable>() );
			// parentInventory.otherInventory.GetComponent<Interactor>().HoverUnlock( arrowHand.GetComponent<Interactable>() );

			if ( releaseSound != null )
			{
				releaseSound.Play();
			}

			this.StartCoroutine( this.ResetDrawAnim() );
		}



		//-------------------------------------------------
		private void EnableArrowSpawn()
		{
			arrowHand.allowArrowSpawn = true;
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


		private void FindArrowHand(Inventory inventory)
		{
			for (int i =0 ; i< inventory.equippedSlots.Length; i++) {
				if (inventory.equippedSlots[i] != null) {
					arrowHand = inventory.equippedSlots[i].sceneItem.GetComponent<ArrowHand>();
					if (arrowHand != null) {
						return;
					}
				}
			}
			
			// arrowHand = inventory.otherInventory.GetComponentInChildren<ArrowHand>();
		}




		public void OnEquippedUpdate (Inventory inventory) {

			if ( arrowHand == null )
			{
				FindArrowHand(inventory);
			}
			if ( arrowHand == null )
			{
				return;
			}

			if ( arrowHand.allowArrowSpawn && ( arrowHand.currentArrow == null ) ) // If we're allowed to have an active arrow in hand but don't yet, spawn one
			{
				arrowHand.currentArrow = InstantiateArrow();
				arrowHand.arrowSpawnSound.Play();
			}






			// Reset transform since we cheated it right after getting poses on previous frame
			//transform.localPosition = Vector3.zero;
			//transform.localRotation = Quaternion.identity;

			// Update handedness guess
			EvaluateHandedness();



			HandleNockPositioning (arrowHand.GetComponent<Item>().myEquipPoint);// inventory.otherInventory);


			if ( nocked )
			{
				Vector3 arrowNockTransformParentPos = arrowHand.arrowNockTransform.parent.position;
				// Vector from bow nock transform to arrowhand nock transform - used to align bow when drawing
				Vector3 nockToarrowHand = ( arrowNockTransformParentPos - nockRestTransform.position ); 
				float nockToArrowHandLength = nockToarrowHand.magnitude;

				// Align bow
				// Time lerp value used for ramping into drawn bow orientation
				float lerp = Util.RemapNumberClamped( Time.time, nockLerpStartTime, ( nockLerpStartTime + lerpDuration ), 0f, 1f );

				// Normalized current state of bow draw 0 - 1
				float pullLerp = Util.RemapNumberClamped( nockToArrowHandLength, minPull, maxPull, 0f, 1f ); 

				Vector3 arrowNockTransformToHeadset = ( ( Player.instance.hmdTransform.position + ( Vector3.down * 0.05f ) ) - arrowNockTransformParentPos ).normalized;
				
				// Use this line to lerp arrowHand nock position
				Vector3 arrowHandPosition = ( arrowNockTransformParentPos + ( ( arrowNockTransformToHeadset * drawOffset ) * pullLerp ) ); 
				
				// Use this line if we don't want to lerp arrowHand nock position
				//Vector3 arrowHandPosition = arrowNockTransform.position; 

				Vector3 pivotToString = ( arrowHandPosition - pivotTransform.position ).normalized;
				Vector3 pivotToLowerHandle = ( handleTransform.position - pivotTransform.position ).normalized;
				Vector3 bowLeftVector = -Vector3.Cross( pivotToLowerHandle, pivotToString );
				pivotTransform.rotation = Quaternion.Lerp( nockLerpStartRotation, Quaternion.LookRotation( pivotToString, bowLeftVector ), lerp );

				// Move nock position
				if ( Vector3.Dot( nockToarrowHand, -nockTransform.forward ) > 0 )
				{
					float distanceToarrowHand = nockToArrowHandLength * lerp;
					

					float zOffset = Mathf.Clamp( -distanceToarrowHand, -maxPull, 0f );

					nockTransform.localPosition = new Vector3( 0f, 0f, zOffset ) ;

					float nockDistanceTravelled = -zOffset;

					arrowVelocity = Util.RemapNumber( nockDistanceTravelled, minPull, maxPull, arrowMinVelocity, arrowMaxVelocity );

					drawTension = Util.RemapNumberClamped( nockDistanceTravelled, 0, maxPull, 0f, 1f );

					// Send drawTension value to LinearMapping script, which drives the bow draw animation
					this.bowDrawLinearMapping.value = drawTension; 

					pulled = ( nockDistanceTravelled > minPull );
					
					HandleNockedEffects(nockDistanceTravelled, drawTension);
					
				}
				else
				{
					nockTransform.localPosition = new Vector3( 0f, 0f, 0f );
					this.bowDrawLinearMapping.value = 0f;
				}
			}
			else
			{
				if ( lerpBackToZeroRotation )
				{
					if (HandleLerpBackToRotationAfterArrowRelease()) {
						lerpBackToZeroRotation = false;
					}
				}
			}
		}

		void HandleNockedEffects (float nockDistanceTravelled, float drawTension) {
			if ( ( nockDistanceTravelled > ( lastTickDistance + hapticDistanceThreshold ) ) || nockDistanceTravelled < ( lastTickDistance - hapticDistanceThreshold ) )
			{
				// ushort hapticStrength = (ushort)Util.RemapNumber( nockDistanceTravelled, 0, maxPull, bowPullPulseStrengthLow, bowPullPulseStrengthHigh );
				// hand.TriggerHapticPulse( hapticStrength );
				// hand.otherHand.TriggerHapticPulse( hapticStrength );

				drawSound.PlayBowTensionClicks( drawTension );

				lastTickDistance = nockDistanceTravelled;
			}

			if ( nockDistanceTravelled >= maxPull )
			{
				if ( Time.time > nextStrainTick )
				{
					// hand.TriggerHapticPulse( 400 );
					// hand.otherHand.TriggerHapticPulse( 400 );

					drawSound.PlayBowTensionClicks( drawTension );

					nextStrainTick = Time.time + Random.Range( minStrainTickTime, maxStrainTickTime );
				}
			}
		}


		bool HandleLerpBackToRotationAfterArrowRelease () {

			float lerp = Mathf.Clamp01( (Time.time - lerpStartTime) / lerpDuration );
			// float lerp = Util.RemapNumber( Time.time, lerpStartTime, lerpStartTime + lerpDuration, 0, 1 );

			pivotTransform.localRotation = Quaternion.Lerp( lerpStartRotation, Quaternion.identity, lerp );

			return lerp >= 1;
		}


		public void OnEquipped (Inventory inventory) {

		}
		public void OnUnequipped (Inventory inventory) {
			Destroy( gameObject );

		}

		public enum Handedness { Left, Right };

		public Handedness currentHandGuess = Handedness.Left;
		private float timeOfPossibleHandSwitch = 0f;
		private float timeBeforeConfirmingHandSwitch = 1.5f;
		private bool possibleHandSwitch = false;

		public Transform pivotTransform;
		public Transform handleTransform;

		// private Hand hand;
		Inventory parentInventory {
			get {
				return GetComponent<Item>().linkedInventory;
			}
		}
		// Interactor parentInventoryInteractor {
		// 	get {
		// 		return parentInventory.GetComponent<Interactor>();
		// 	}
		// }

		
		private ArrowHand arrowHand;

		public Transform nockTransform;
		public Transform nockRestTransform;

		// public bool autoSpawnArrowHand = true;
		public ItemPackage arrowHandItemPackage;
		// public GameObject arrowHandPrefab;

		public bool nocked;
		public bool pulled;

		private const float minPull = 0.05f;
		private const float maxPull = 0.5f;
		// private float nockDistanceTravelled = 0f;
		private float hapticDistanceThreshold = 0.01f;
		private float lastTickDistance;
		// private const float bowPullPulseStrengthLow = 100;
		// private const float bowPullPulseStrengthHigh = 500;
		// private Vector3 bowLeftVector;

		public float arrowMinVelocity = 3f;
		public float arrowMaxVelocity = 30f;
		private float arrowVelocity = 30f;

		private float minStrainTickTime = 0.1f;
		private float maxStrainTickTime = 0.5f;
		private float nextStrainTick = 0;

		private bool lerpBackToZeroRotation;
		private float lerpStartTime;
		private float lerpDuration = 0.15f;
		private Quaternion lerpStartRotation;

		private float nockLerpStartTime;

		private Quaternion nockLerpStartRotation;

		public float drawOffset = 0.06f;

		public LinearMapping bowDrawLinearMapping;
        
		// private Vector3 lateUpdatePos;
		// private Quaternion lateUpdateRot;

		public SoundBowClick drawSound;
		private float drawTension;
		public SoundPlayOneshot arrowSlideSound;
		public SoundPlayOneshot releaseSound;
		public SoundPlayOneshot nockSound;

		// SteamVR_Events.Action newPosesAppliedAction;


		


		//-------------------------------------------------
		private IEnumerator ResetDrawAnim()
		{
			float startTime = Time.time;
			float startLerp = drawTension;

			while ( Time.time < ( startTime + 0.02f ) )
			{
				float lerp = Util.RemapNumberClamped( Time.time, startTime, startTime + 0.02f, startLerp, 0f );
				this.bowDrawLinearMapping.value = lerp;
				
				yield return null;
			}

			this.bowDrawLinearMapping.value = 0;

			yield break;
		}


		//-------------------------------------------------
		public float GetArrowVelocity()
		{
			return arrowVelocity;
		}


		//-------------------------------------------------
		public void StartRotationLerp()
		{
			lerpStartTime = Time.time;
			lerpBackToZeroRotation = true;
			lerpStartRotation = pivotTransform.localRotation;

			Util.ResetTransform( nockTransform );
		}


		//-------------------------------------------------
		


		//-------------------------------------------------
		private void EvaluateHandedness()
		{
            var handType = parentInventory.GetComponent<Hand>().handType;

			if ( handType == SteamVR_Input_Sources.LeftHand )// Bow hand is further left than arrow hand.
			{
				// We were considering a switch, but the current controller orientation matches our currently assigned handedness, so no longer consider a switch
				if ( possibleHandSwitch && currentHandGuess == Handedness.Left )
				{
					possibleHandSwitch = false;
				}

				// If we previously thought the bow was right-handed, and were not already considering switching, start considering a switch
				if ( !possibleHandSwitch && currentHandGuess == Handedness.Right )
				{
					possibleHandSwitch = true;
					timeOfPossibleHandSwitch = Time.time;
				}

				// If we are considering a handedness switch, and it's been this way long enough, switch
				if ( possibleHandSwitch && Time.time > ( timeOfPossibleHandSwitch + timeBeforeConfirmingHandSwitch ) )
				{
					currentHandGuess = Handedness.Left;
					possibleHandSwitch = false;
				}
			}
			else // Bow hand is further right than arrow hand
			{
				// We were considering a switch, but the current controller orientation matches our currently assigned handedness, so no longer consider a switch
				if ( possibleHandSwitch && currentHandGuess == Handedness.Right )
				{
					possibleHandSwitch = false;
				}

				// If we previously thought the bow was right-handed, and were not already considering switching, start considering a switch
				if ( !possibleHandSwitch && currentHandGuess == Handedness.Left )
				{
					possibleHandSwitch = true;
					timeOfPossibleHandSwitch = Time.time;
				}

				// If we are considering a handedness switch, and it's been this way long enough, switch
				if ( possibleHandSwitch && Time.time > ( timeOfPossibleHandSwitch + timeBeforeConfirmingHandSwitch ) )
				{
					currentHandGuess = Handedness.Right;
					possibleHandSwitch = false;
				}
			}
		}


		//-------------------------------------------------
		private void DoHandednessCheck()
		{
			// Based on our current best guess about hand, switch bow orientation and arrow lerp direction
			if ( currentHandGuess == Handedness.Left )
			{
				pivotTransform.localScale = new Vector3( 1f, 1f, 1f );
			}
			else
			{
				pivotTransform.localScale = new Vector3( 1f, -1f, 1f );
			}
		}


		//-------------------------------------------------
		public void ArrowInPosition()
		{
			DoHandednessCheck();

			if ( nockSound != null )
			{
				nockSound.Play();
			}
		}


		//-------------------------------------------------
		public void ReleaseNock() 
		{
			// ArrowHand tells us to do this when we release the buttons when bow is nocked but not drawn far enough
			nocked = false;
			Debug.LogError("release nock");

			// parentInventoryInteractor.HoverUnlock( GetComponent<Interactable>() );
			
			this.StartCoroutine( this.ResetDrawAnim() );
		}


		//-------------------------------------------------
		private void ShutDown()
		{

			if ( parentInventory != null)// && parentInventory.otherInventory.equippedItem != null)
			
			{
				for (int i =0 ; i < parentInventory.equippedSlots.Length; i++) {
					if (parentInventory.equippedSlots[i] != null) {

						// GameObject otherHandCurrentAttached = parentInventory.otherInventory.equippedItem.item.gameObject;
						GameObject otherHandCurrentAttached = parentInventory.equippedSlots[i].sceneItem.gameObject;
						
						if ( otherHandCurrentAttached.GetComponent<ItemPackageReference>() != null )
						{
							if ( otherHandCurrentAttached.GetComponent<ItemPackageReference>().itemPackage == arrowHandItemPackage )
							{
								parentInventory.UnequipItem(i, false);
								// parentInventory.otherInventory.UnequipItem( otherHandCurrentAttached.GetComponent<Item>() );
							}
						}
					}
				}
			}
		}




		//-------------------------------------------------
		void OnDestroy()
		{
			ShutDown();
		}
	}
}
