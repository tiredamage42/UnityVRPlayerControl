//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Drives a linear mapping based on position between 2 positions
//
//=============================================================================

using UnityEngine;
using System.Collections;
using InteractionSystem;
using InventorySystem;
using VRPlayer;
namespace Valve.VR.InteractionSystem
{
	//-------------------------------------------------------------------------
	[RequireComponent( typeof( Interactable ) )]
	public class LinearDrive : MonoBehaviour, IInteractable, IInventoryItem
	{
		public void OnEquippedUseStart(Inventory inventory, int useIndex) {}
        public void OnEquippedUseEnd(Inventory inventory, int useIndex) {}
        public void OnEquippedUseUpdate(Inventory inventory, int useIndex) {}



		public void OnEquipped (Inventory inventory) {

			initialMappingOffset = linearMapping.value - CalculateLinearMapping( inventory.transform );
			sampleCount = 0;
			mappingChangeRate = 0.0f;

		}
		public void OnUnequipped (Inventory inventory) {
			CalculateMappingChangeRate();

		}
		public void OnEquippedUpdate (Inventory inventory) {
			UpdateLinearMapping(inventory.transform);


		}


		
		public void OnInspectStart(Interactor interactor) {

		}
        public void OnInspectEnd(Interactor interactor){

		}
        public void OnInspectUpdate(Interactor interactor){

		}
        public void OnUseStart(Interactor interactor, int useIndex){

		}
        public void OnUseEnd(Interactor interactor, int useIndex){
			
		}
        public void OnUseUpdate(Interactor interactor, int useIndex){

		}


		public Transform startPosition;
		public Transform endPosition;
		public LinearMapping linearMapping;
		public bool repositionGameObject = true;
		public bool maintainMomemntum = true;
		public float momemtumDampenRate = 5.0f;

        protected Hand.AttachmentFlags attachmentFlags = Hand.AttachmentFlags.DetachFromOtherHand;

        protected float initialMappingOffset;
        protected int numMappingChangeSamples = 5;
        protected float[] mappingChangeSamples;
        protected float prevMapping = 0.0f;
        protected float mappingChangeRate;
        protected int sampleCount = 0;

        protected Interactable interactable;


        protected virtual void Awake()
        {
            mappingChangeSamples = new float[numMappingChangeSamples];
            interactable = GetComponent<Interactable>();

			GetComponent<Item>().attachmentFlags = attachmentFlags;
        }

        protected virtual void Start()
		{
			if ( linearMapping == null )
			{
				linearMapping = GetComponent<LinearMapping>();
			}

			if ( linearMapping == null )
			{
				linearMapping = gameObject.AddComponent<LinearMapping>();
			}

            initialMappingOffset = linearMapping.value;

			if ( repositionGameObject )
			{
				UpdateLinearMapping( transform );
			}
		}

        // protected virtual void HandHoverUpdate( Hand hand )
        // {
        //     // GrabTypes startingGrabType = hand.GetGrabStarting();


        //     if (interactable.attachedToHand == null && hand.GetGrabDown())// startingGrabType != GrabTypes.None)
        //     {
        //         initialMappingOffset = linearMapping.value - CalculateLinearMapping( hand.transform );
		// 		sampleCount = 0;
		// 		mappingChangeRate = 0.0f;

        //         hand.AttachObject(gameObject, 
		// 			// startingGrabType, 
		// 			attachmentFlags);
        //     }
		// }

        // protected virtual void HandAttachedUpdate(Hand hand)
        // {
        //     UpdateLinearMapping(hand.transform);

		// 	if (hand.GetGrabUp())
        //     // if (hand.IsGrabEnding(this.gameObject))
        //     {
        //         hand.DetachObject(gameObject);
        //     }
        // }

        // protected virtual void OnUnequipped(Hand hand)
        // {
        //     CalculateMappingChangeRate();
        // }


        protected void CalculateMappingChangeRate()
		{
			//Compute the mapping change rate
			mappingChangeRate = 0.0f;
			int mappingSamplesCount = Mathf.Min( sampleCount, mappingChangeSamples.Length );
			if ( mappingSamplesCount != 0 )
			{
				for ( int i = 0; i < mappingSamplesCount; ++i )
				{
					mappingChangeRate += mappingChangeSamples[i];
				}
				mappingChangeRate /= mappingSamplesCount;
			}
		}

        protected void UpdateLinearMapping( Transform updateTransform )
		{
			prevMapping = linearMapping.value;
			linearMapping.value = Mathf.Clamp01( initialMappingOffset + CalculateLinearMapping( updateTransform ) );

			mappingChangeSamples[sampleCount % mappingChangeSamples.Length] = ( 1.0f / Time.deltaTime ) * ( linearMapping.value - prevMapping );
			sampleCount++;

			if ( repositionGameObject )
			{
				transform.position = Vector3.Lerp( startPosition.position, endPosition.position, linearMapping.value );
			}
		}

        protected float CalculateLinearMapping( Transform updateTransform )
		{
			Vector3 direction = endPosition.position - startPosition.position;
			float length = direction.magnitude;
			direction.Normalize();

			Vector3 displacement = updateTransform.position - startPosition.position;

			return Vector3.Dot( displacement, direction ) / length;
		}

        
		protected virtual void Update()
        {
            if ( maintainMomemntum && mappingChangeRate != 0.0f )
			{
				//Dampen the mapping change rate and apply it to the mapping
				mappingChangeRate = Mathf.Lerp( mappingChangeRate, 0.0f, momemtumDampenRate * Time.deltaTime );
				linearMapping.value = Mathf.Clamp01( linearMapping.value + ( mappingChangeRate * Time.deltaTime ) );

				if ( repositionGameObject )
				{
					transform.position = Vector3.Lerp( startPosition.position, endPosition.position, linearMapping.value );
				}
			}
		}
	}
}
