﻿//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Throwable that uses physics joints to attach instead of just
//			parenting
//
//=============================================================================

using UnityEngine;
using System.Collections.Generic;

using InteractionSystem;
using InventorySystem;
using VRPlayer;
namespace Valve.VR.InteractionSystem
{
	//-------------------------------------------------------------------------
	[RequireComponent( typeof( Interactable ) )]
	public class ComplexThrowable : MonoBehaviour, IInventoryItem
	{
		public void OnEquipped(Inventory inventory) {
			PhysicsAttach( inventory.transform );

		}

        public void OnUnequipped(Inventory inventory) {
			PhysicsDetach( inventory.transform );//holdingHands[i] );

		}
        public void OnEquippedUpdate(Inventory inventory){

		}



		// void OnInspectStart(Interactor interactor) {

		// }

        // void OnInspectEnd(Interactor interactor) {

		// }
        // void OnInspectUpdate(Interactor interactor){

		// }
        // void OnUseStart(Interactor interactor){
		// 	// PhysicsAttach( interactor.transform );
		// }
			
        // void OnUseEnd(Interactor interactor){

		// }
        // void OnUseUpdate(Interactor interactor){

		// }



		public enum AttachMode
		{
			FixedJoint,
			Force,
		}

		public float attachForce = 800.0f;
		public float attachForceDamper = 25.0f;

		public AttachMode attachMode = AttachMode.FixedJoint;

		[EnumFlags]
		public Hand.AttachmentFlags attachmentFlags = 0;

		// private List<Hand> holdingHands = new List<Hand>();
		// private List<Rigidbody> holdingBodies = new List<Rigidbody>();
		// private List<Vector3> holdingPoints = new List<Vector3>();
		private List<Rigidbody> rigidBodies = new List<Rigidbody>();


		//-------------------------------------------------
		void Awake()
		{
			GetComponentsInChildren<Rigidbody>( rigidBodies );
			GetComponent<Item>().attachmentFlags = attachmentFlags;
		}


		//-------------------------------------------------
		// void Update()
		// {
		// 	for ( int i = 0; i < holdingHands.Count; i++ )
		// 	{
		// 		if (holdingHands[i].GetGrabUp())
        //         // if (holdingHands[i].IsGrabEnding(this.gameObject))
        //         {
		// 			PhysicsDetach( holdingHands[i] );
		// 		}
		// 	}
		// }


		//-------------------------------------------------
		// private void OnHandHoverBegin( Hand hand )
		// {
		// 	if ( holdingHands.IndexOf( hand ) == -1 )
		// 	{
		// 		if ( hand.isActive )
		// 		{
		// 			hand.TriggerHapticPulse( 800 );
		// 		}
		// 	}
		// }


		//-------------------------------------------------
		// private void OnHandHoverEnd( Hand hand )
		// {
		// 	if ( holdingHands.IndexOf( hand ) == -1 )
		// 	{
		// 		if (hand.isActive)
		// 		{
		// 			hand.TriggerHapticPulse( 500 );
		// 		}
		// 	}
		// }


		//-------------------------------------------------
		// private void HandHoverUpdate( Hand hand )
		// {
        //     // GrabTypes startingGrabType = hand.GetGrabStarting();

        //     // if (startingGrabType != GrabTypes.None)
		// 	if (hand.GetGrabDown())
		// 	{
		// 		PhysicsAttach( hand);//, startingGrabType );
		// 	}
		// }


		//-------------------------------------------------
		// private void PhysicsAttach( Hand hand)//, GrabTypes startingGrabType )
		private void PhysicsAttach( Transform attachObj)//, GrabTypes startingGrabType )
		
		{
			PhysicsDetach( attachObj );

			// Rigidbody 
			holdingBody = null;
			// Vector3 
			holdingPoint = Vector3.zero;

			// The hand should grab onto the nearest rigid body
			float closestDistance = float.MaxValue;
			for ( int i = 0; i < rigidBodies.Count; i++ )
			{
				float distance = Vector3.Distance( rigidBodies[i].worldCenterOfMass, attachObj.position );
				if ( distance < closestDistance )
				{
					holdingBody = rigidBodies[i];
					closestDistance = distance;
				}
			}

			// Couldn't grab onto a body
			if ( holdingBody == null )
				return;

			// Create a fixed joint from the hand to the holding body
			if ( attachMode == AttachMode.FixedJoint )
			{
				Rigidbody handRigidbody = Util.FindOrAddComponent<Rigidbody>( attachObj.gameObject );
				handRigidbody.isKinematic = true;

				FixedJoint handJoint = attachObj.gameObject.AddComponent<FixedJoint>();
				handJoint.connectedBody = holdingBody;
			}

			// Don't let the hand interact with other things while it's holding us
			// hand.HoverLock( null );

			// Affix this point
			Vector3 offset = attachObj.position - holdingBody.worldCenterOfMass;
			offset = Mathf.Min( offset.magnitude, 1.0f ) * offset.normalized;
			holdingPoint = holdingBody.transform.InverseTransformPoint( holdingBody.worldCenterOfMass + offset );


			// hand.AttachObject( this.gameObject, attachmentFlags );
attached = true;
			// Update holding list
			// holdingHands.Add( hand );
			// holdingBodies.Add( holdingBody );
			// holdingPoints.Add( holdingPoint );
		}

		Vector3 holdingPoint;
		Rigidbody holdingBody;
		bool attached = false;


		//-------------------------------------------------
		// private bool PhysicsDetach( Transform attachObj )
		private void PhysicsDetach( Transform attachObj )
		
		{
			// int i = holdingHands.IndexOf( attachObj );

			// if ( i != -1 )
			// {
				// Detach this object from the hand
				// holdingHands[i].DetachObject( this.gameObject, false );

				// Allow the hand to do other things
				// holdingHands[i].HoverUnlock( null );

				// Delete any existing joints from the hand
				if ( attachMode == AttachMode.FixedJoint )
				{
					// Destroy( holdingHands[i].GetComponent<FixedJoint>() );
					Destroy( attachObj.GetComponent<FixedJoint>() );
				}
attached = false;
				// Util.FastRemove( holdingHands, i );
				// Util.FastRemove( holdingBodies, i );
				// Util.FastRemove( holdingPoints, i );

				// return true;
			// }

			// return false;
		}


		//-------------------------------------------------
		void FixedUpdate()
		{
			if (!attached) return;

			if ( attachMode == AttachMode.Force )
			{
				// for ( int i = 0; i < holdingHands.Count; i++ )
				// {
					// Vector3 targetPoint = holdingBodies[i].transform.TransformPoint( holdingPoints[i] );
					// Vector3 vdisplacement = holdingHands[i].transform.position - targetPoint;

					// holdingBodies[i].AddForceAtPosition( attachForce * vdisplacement, targetPoint, ForceMode.Acceleration );
					// holdingBodies[i].AddForceAtPosition( -attachForceDamper * holdingBodies[i].GetPointVelocity( targetPoint ), targetPoint, ForceMode.Acceleration );



					Vector3 targetPoint = holdingBody.transform.TransformPoint( holdingPoint );
					Vector3 vdisplacement = GetComponent<Item>().parentInventory.transform.position - targetPoint;

					holdingBody.AddForceAtPosition( attachForce * vdisplacement, targetPoint, ForceMode.Acceleration );
					holdingBody.AddForceAtPosition( -attachForceDamper * holdingBody.GetPointVelocity( targetPoint ), targetPoint, ForceMode.Acceleration );
				// }
			}
		}
	}
}
