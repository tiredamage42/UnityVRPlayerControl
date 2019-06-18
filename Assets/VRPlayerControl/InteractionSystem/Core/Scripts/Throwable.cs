﻿//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Basic throwable object
//
//=============================================================================

using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using InteractionSystem;
using InventorySystem;
using VRPlayer;

namespace Valve.VR.InteractionSystem
{
	//-------------------------------------------------------------------------
	[RequireComponent( typeof( Interactable ) )]
	[RequireComponent( typeof( Rigidbody ) )]
    // [RequireComponent( typeof(VelocityEstimator))]
	public class Throwable : MonoBehaviour, IInventoryItem
	{
        public void OnEquippedUseStart(Inventory inventory, int useIndex) {}
        public void OnEquippedUseEnd(Inventory inventory, int useIndex) {}
        public void OnEquippedUseUpdate(Inventory inventory, int useIndex) {}



        public void OnEquipped (Inventory inventory) {

            // Hand hand = (Hand)owner;
            //Debug.Log("<b>[SteamVR Interaction]</b> Pickup: " + hand.GetGrabStarting().ToString());

            hadInterpolation = this.rigidbody.interpolation;

            attached = true;

			onPickUp.Invoke();

			// hand.HoverLock( null );
            
            rigidbody.interpolation = RigidbodyInterpolation.None;
            

            inventory.GetComponent<VelocityEstimator>().BeginEstimatingVelocity();
		    // hand.velocityEstimator.BeginEstimatingVelocity();

			attachTime = Time.time;
			attachPosition = transform.position;
			attachRotation = transform.rotation;

        }
        public void OnUnequipped (Inventory inventory) {

            attached = false;

            onDetachFromHand.Invoke();

            // hand.HoverUnlock(null);
            
            rigidbody.interpolation = hadInterpolation;

            Vector3 velocity;
            Vector3 angularVelocity;

            GetReleaseVelocities(inventory.GetComponent<Hand>(), out velocity, out angularVelocity);

            rigidbody.velocity = velocity;
            rigidbody.angularVelocity = angularVelocity;
            
        }

        public void OnEquippedUpdate (Inventory inventory) {
            
            // if (onHeldUpdate != null)
            //     onHeldUpdate.Invoke(hand);

        }
        
		void OnInspectStart(Interactor interactor) {
            // bool showHint = false;

            // "Catch" the throwable by holding down the interaction button instead of pressing it.
            // Only do this if the throwable is moving faster than the prescribed threshold speed,
            // and if it isn't attached to another hand
            // if ( !attached && catchingSpeedThreshold != -1)
            // {
            //     float catchingThreshold = catchingSpeedThreshold * SteamVR_Utils.GetLossyScale(Player.instance.trackingOriginTransform);

            //     // GrabTypes bestGrabType = hand.GetBestGrabbingType();

            //     // if ( bestGrabType != GrabTypes.None )
			// 	if (hand.IsGrabbing())
            //     {
			// 		if (rigidbody.velocity.magnitude >= catchingThreshold)
			// 		{
			// 			hand.AttachObject( gameObject, attachmentFlags );
			// 		}
			// 	}
			// }

		}
        void OnInspectEnd(Interactor interactor){

		}
        void OnInspectUpdate(Interactor interactor){

		}
        void OnUseStart(Interactor interactor, int useIndex){

		}
        void OnUseEnd(Interactor interactor, int useIndex){
			
		}
        void OnUseUpdate(Interactor interactor, int useIndex){

		}






		[EnumFlags]
		[Tooltip( "The flags used to attach this object to the hand." )]
		public Hand.AttachmentFlags attachmentFlags = Hand.AttachmentFlags.ParentToHand | Hand.AttachmentFlags.DetachFromOtherHand | Hand.AttachmentFlags.TurnOnKinematic;

        [Tooltip("The local point which acts as a positional and rotational offset to use while held")]
        public Transform attachmentOffset;

		[Tooltip( "How fast must this object be moving to attach due to a trigger hold instead of a trigger press? (-1 to disable)" )]
        public float catchingSpeedThreshold = -1;

        public ReleaseStyle releaseVelocityStyle = ReleaseStyle.GetFromHand;

        [Tooltip("The time offset used when releasing the object with the RawFromHand option")]
        public float releaseVelocityTimeOffset = -0.011f;

        public float scaleReleaseVelocity = 1.1f;

		[Tooltip( "When detaching the object, should it return to its original parent?" )]
		public bool restoreOriginalParent = false;

        

		// protected VelocityEstimator velocityEstimator;
        protected bool attached = false;
        protected float attachTime;
        protected Vector3 attachPosition;
        protected Quaternion attachRotation;
        protected Transform attachEaseInTransform;

		public UnityEvent onPickUp;
        public UnityEvent onDetachFromHand;
        public UnityEvent<Hand> onHeldUpdate;

        
        protected RigidbodyInterpolation hadInterpolation = RigidbodyInterpolation.None;

        protected new Rigidbody rigidbody;

        [HideInInspector]
        public Interactable interactable;


        //-------------------------------------------------
        protected virtual void Awake()
		{
			// velocityEstimator = GetComponent<VelocityEstimator>();
            interactable = GetComponent<Interactable>();

            GetComponent<Item>().attachmentFlags = attachmentFlags;

            rigidbody = GetComponent<Rigidbody>();
            rigidbody.maxAngularVelocity = 50.0f;


            if(attachmentOffset != null)
            {
                // remove?
                //interactable.handFollowTransform = attachmentOffset;
            }

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
        // protected virtual void OnHandHoverBegin( Hand hand )
		// {
		// 	// bool showHint = false;

        //     // "Catch" the throwable by holding down the interaction button instead of pressing it.
        //     // Only do this if the throwable is moving faster than the prescribed threshold speed,
        //     // and if it isn't attached to another hand
        //     if ( !attached && catchingSpeedThreshold != -1)
        //     {
        //         float catchingThreshold = catchingSpeedThreshold * SteamVR_Utils.GetLossyScale(Player.instance.trackingOriginTransform);

        //         // GrabTypes bestGrabType = hand.GetBestGrabbingType();

        //         // if ( bestGrabType != GrabTypes.None )
		// 		if (hand.IsGrabbing())
        //         {
		// 			if (rigidbody.velocity.magnitude >= catchingThreshold)
		// 			{
		// 				hand.AttachObject( gameObject, attachmentFlags );
		// 			}
		// 		}
		// 	}
		// }


        //-------------------------------------------------
        // protected virtual void OnHandHoverEnd( Hand hand )
		// {
        //     // hand.HideGrabHint();
		// }


        // //-------------------------------------------------
        // protected virtual void HandHoverUpdate( Hand hand )
        // {
        //     // GrabTypes startingGrabType = hand.GetGrabStarting();
            
        //     // if (startingGrabType != GrabTypes.None)
        //     if (hand.GetGrabDown())
        //     {
		// 		hand.AttachObject( gameObject, 
        //             // startingGrabType, 
        //             attachmentFlags, attachmentOffset );
        //         // hand.HideGrabHint();
        //     }
		// }

        //-------------------------------------------------
        // protected virtual void OnEquipped( Object owner )
		// {
        //     Hand hand = (Hand)owner;
        //     //Debug.Log("<b>[SteamVR Interaction]</b> Pickup: " + hand.GetGrabStarting().ToString());

        //     hadInterpolation = this.rigidbody.interpolation;

        //     attached = true;

		// 	onPickUp.Invoke();

		// 	hand.HoverLock( null );
            
        //     rigidbody.interpolation = RigidbodyInterpolation.None;
            

            
		//     hand.velocityEstimator.BeginEstimatingVelocity();

		// 	attachTime = Time.time;
		// 	attachPosition = transform.position;
		// 	attachRotation = transform.rotation;

		// }


        //-------------------------------------------------
        // protected virtual void OnUnequipped(Object owner)
        // {
        //     Hand hand = (Hand)owner;
        //     attached = false;

        //     onDetachFromHand.Invoke();

        //     hand.HoverUnlock(null);
            
        //     rigidbody.interpolation = hadInterpolation;

        //     Vector3 velocity;
        //     Vector3 angularVelocity;

        //     GetReleaseVelocities(hand, out velocity, out angularVelocity);

        //     rigidbody.velocity = velocity;
        //     rigidbody.angularVelocity = angularVelocity;
        // }


        public virtual void GetReleaseVelocities(Hand hand, out Vector3 velocity, out Vector3 angularVelocity)
        {
            // if (hand.noSteamVRFallbackCamera && releaseVelocityStyle != ReleaseStyle.NoChange)
            //     releaseVelocityStyle = ReleaseStyle.ShortEstimation; // only type that works with fallback hand is short estimation.

            switch (releaseVelocityStyle)
            {
                case ReleaseStyle.ShortEstimation:
                    Debug.Log("short estimeation");
                    hand.velocityEstimator.FinishEstimatingVelocity();
                    velocity = hand.velocityEstimator.GetVelocityEstimate();
                    angularVelocity = hand.velocityEstimator.GetAngularVelocityEstimate();
                    break;
                case ReleaseStyle.AdvancedEstimation:
                Debug.Log("advanced estimeation");
                    hand.GetEstimatedPeakVelocities(out velocity, out angularVelocity);
                    break;
                case ReleaseStyle.GetFromHand:
                Debug.Log("and");
                    velocity = hand.GetTrackedObjectVelocity(releaseVelocityTimeOffset);
                    angularVelocity = hand.GetTrackedObjectAngularVelocity(releaseVelocityTimeOffset);
                    break;
                default:
                case ReleaseStyle.NoChange:
                    velocity = rigidbody.velocity;
                    angularVelocity = rigidbody.angularVelocity;
                    break;
            }

            if (releaseVelocityStyle != ReleaseStyle.NoChange)
                velocity *= scaleReleaseVelocity;
        }

        //-------------------------------------------------
        // protected virtual void HandAttachedUpdate(Hand hand)
        // {
        //     // if (hand.IsGrabEnding(this.gameObject))
        //     if (hand.GetGrabUp())
        //     {
        //         hand.DetachObject(gameObject, restoreOriginalParent);

        //         // Uncomment to detach ourselves late in the frame.
        //         // This is so that any vehicles the player is attached to
        //         // have a chance to finish updating themselves.
        //         // If we detach now, our position could be behind what it
        //         // will be at the end of the frame, and the object may appear
        //         // to teleport behind the hand when the player releases it.
        //         //StartCoroutine( LateDetach( hand ) );
        //     }

        //     if (onHeldUpdate != null)
        //         onHeldUpdate.Invoke(hand);
        // }


        //-------------------------------------------------
        // protected virtual IEnumerator LateDetach( Hand hand )
		// {
		// 	yield return new WaitForEndOfFrame();

		// 	hand.DetachObject( gameObject, restoreOriginalParent );
		// }


        // //-------------------------------------------------
        // protected virtual void OnHandFocusAcquired( Hand hand )
		// {
        //     Debug.LogError("ON HAND FOCUS ACQUIRED " + name);
		// 	gameObject.SetActive( true );
		// 	hand.velocityEstimator.BeginEstimatingVelocity();
		// }


        // //-------------------------------------------------
        // protected virtual void OnHandFocusLost( Hand hand )
		// {
        //     Debug.LogError("ON HAND FOCUS Lost " + name);
			
		// 	gameObject.SetActive( false );
		// 	hand.velocityEstimator.FinishEstimatingVelocity();
		// }
	}

    public enum ReleaseStyle
    {
        NoChange,
        GetFromHand,
        ShortEstimation,
        AdvancedEstimation,
    }
}
