//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Handles the spawning and returning of the ItemPackage
//
//=============================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

using InventorySystem;
using InteractionSystem;
using VRPlayer;

namespace Valve.VR.InteractionSystem
{
	//-------------------------------------------------------------------------
	[RequireComponent( typeof( Interactable ) )]
	public class ItemPackageSpawner : MonoBehaviour, IInteractable
	{
		
		public void OnInspectedStart(Interactor interactor) {

			Inventory inventory = interactor.GetComponentInParent<Inventory>();
			ItemPackage currentAttachedItemPackage = GetAttachedItemPackage( inventory );

			if ( currentAttachedItemPackage == itemPackage ) // the item at the top of the hand's stack has an associated ItemPackage
			{
				if ( takeBackItem && !requireReleaseActionToReturn ) // if we want to take back matching items and aren't waiting for a trigger press
				{
					TakeBackItem( inventory );
				}
			}

			if (!requireGrabActionToTake) // we don't require trigger press for pickup. Spawn and attach object.
			{
				SpawnAndAttachObject( inventory, interactor.interactorID );//, GrabTypes.Scripted );
			}

		}
        public void OnInspectedEnd(Interactor interactor){
						justPickedUpItem = false;


		}
        public void OnInspectedUpdate(Interactor interactor){

			// if ( takeBackItem && requireReleaseActionToReturn )
			// {
            //     if (hand.isActive)
			// 	{
			// 		ItemPackage currentAttachedItemPackage = GetAttachedItemPackage( hand );
            //         if (currentAttachedItemPackage == itemPackage && hand.GetGrabUp())
			// 		{
			// 			TakeBackItem( hand );
			// 			return; // So that we don't pick up an ItemPackage the same frame that we return it
			// 		}
			// 	}
			// }

			// if ( requireGrabActionToTake )
			// {
            //     // GrabTypes startingGrab = hand.GetGrabStarting();

			// 	// if (startingGrab != GrabTypes.None)
			// 	if (hand.GetGrabDown())
			// 	{
			// 		SpawnAndAttachObject( hand);//, GrabTypes.Scripted);
			// 	}
			// }

		}
        public void OnUsedStart(Interactor interactor, int useIndex){
			if ( requireGrabActionToTake )
			{
                // GrabTypes startingGrab = hand.GetGrabStarting();

				// if (startingGrab != GrabTypes.None)
				// if (hand.GetGrabDown())
				// {
					SpawnAndAttachObject( interactor.GetComponentInParent<Inventory>(), interactor.interactorID );//, GrabTypes.Scripted);
				// }
			}

		}
        public void OnUsedEnd(Interactor interactor, int useIndex){
			if ( takeBackItem && requireReleaseActionToReturn )
			{
					Inventory inventory = interactor.GetComponentInParent<Inventory>();
                	ItemPackage currentAttachedItemPackage = GetAttachedItemPackage( inventory );
                    if (currentAttachedItemPackage == itemPackage)
					{
						TakeBackItem( inventory );
						//return; // So that we don't pick up an ItemPackage the same frame that we return it
					}
				
			}
			
		}
        public void OnUsedUpdate(Interactor interactor, int useIndex){

		}



		public ItemPackage itemPackage
		{
			get
			{
				return _itemPackage;
			}
			set
			{
				CreatePreviewObject();
			}
		}

		public ItemPackage _itemPackage;

		private bool useItemPackagePreview = true;
		private bool useFadedPreview = false;
		private GameObject previewObject;

		public bool requireGrabActionToTake = false;
		public bool requireReleaseActionToReturn = false;
		// public bool showTriggerHint = false;

		// [EnumFlags]
		// public Hand.AttachmentFlags attachmentFlags = Hand.defaultAttachmentFlags;

		public bool takeBackItem = false; // if a hand enters this trigger and has the item this spawner dispenses at the top of the stack, remove it from the stack

		public bool acceptDifferentItems = false;

		private GameObject spawnedItem;
		private bool itemIsSpawned = false;

		public UnityEvent pickupEvent;
		public UnityEvent dropEvent;

		public bool justPickedUpItem = false;


		//-------------------------------------------------
		private void CreatePreviewObject()
		{
			if ( !useItemPackagePreview )
			{
				return;
			}

			ClearPreview();

			if ( useItemPackagePreview )
			{
				if ( itemPackage == null )
				{
					return;
				}

				if ( useFadedPreview == false ) // if we don't have a spawned item out there, use the regular preview
				{
					if ( itemPackage.previewPrefab != null )
					{
						previewObject = Instantiate( itemPackage.previewPrefab, transform.position, Quaternion.identity ) as GameObject;
						previewObject.transform.parent = transform;
						previewObject.transform.localRotation = Quaternion.identity;
					}
				}
				else // there's a spawned item out there. Use the faded preview
				{
					if ( itemPackage.fadedPreviewPrefab != null )
					{
						previewObject = Instantiate( itemPackage.fadedPreviewPrefab, transform.position, Quaternion.identity ) as GameObject;
						previewObject.transform.parent = transform;
						previewObject.transform.localRotation = Quaternion.identity;
					}
				}
			}
		}


		//-------------------------------------------------
		void Start()
		{
			VerifyItemPackage();
		}


		//-------------------------------------------------
		private void VerifyItemPackage()
		{
			if ( itemPackage == null )
			{
				ItemPackageNotValid();
			}

			if ( itemPackage.itemPrefab == null )
			{
				ItemPackageNotValid();
			}
		}


		//-------------------------------------------------
		private void ItemPackageNotValid()
		{
			Debug.LogError("<b>[SteamVR Interaction]</b> ItemPackage assigned to " + gameObject.name + " is not valid. Destroying this game object." );
			Destroy( gameObject );
		}


		//-------------------------------------------------
		private void ClearPreview()
		{
			foreach ( Transform child in transform )
			{
				if ( Time.time > 0 )
				{
					GameObject.Destroy( child.gameObject );
				}
				else
				{
					GameObject.DestroyImmediate( child.gameObject );
				}
			}
		}


		//-------------------------------------------------
		void Update()
		{
			if ( ( itemIsSpawned == true ) && ( spawnedItem == null ) )
			{
				itemIsSpawned = false;
				useFadedPreview = false;
				dropEvent.Invoke();
				CreatePreviewObject();
			}
		}


		//-------------------------------------------------
		// private void OnHandHoverBegin( Hand hand )
		// {
		// 	// ItemPackage currentAttachedItemPackage = GetAttachedItemPackage( hand );

		// 	// if ( currentAttachedItemPackage == itemPackage ) // the item at the top of the hand's stack has an associated ItemPackage
		// 	// {
		// 	// 	if ( takeBackItem && !requireReleaseActionToReturn ) // if we want to take back matching items and aren't waiting for a trigger press
		// 	// 	{
		// 	// 		TakeBackItem( hand );
		// 	// 	}
		// 	// }

		// 	// if (!requireGrabActionToTake) // we don't require trigger press for pickup. Spawn and attach object.
		// 	// {
		// 	// 	SpawnAndAttachObject( hand);//, GrabTypes.Scripted );
		// 	// }

		// 	// if (requireGrabActionToTake && showTriggerHint )
		// 	// {
        //     //     hand.ShowGrabHint("PickUp");
		// 	// }
		// }


		//-------------------------------------------------
		private void TakeBackItem( Inventory inventory )
		{
			RemoveMatchingItemsFromHandStack( itemPackage, inventory );

			// if ( itemPackage.packageType == ItemPackage.ItemPackageType.TwoHanded )
			// {
			// 	RemoveMatchingItemsFromHandStack( itemPackage, inventory.otherInventory );
			// }
		}
		// private void TakeBackItem( Hand hand )
		// {
		// 	RemoveMatchingItemsFromHandStack( itemPackage, hand );

		// 	if ( itemPackage.packageType == ItemPackage.ItemPackageType.TwoHanded )
		// 	{
		// 		RemoveMatchingItemsFromHandStack( itemPackage, hand.otherHand );
		// 	}
		// }


		//-------------------------------------------------
		// private ItemPackage GetAttachedItemPackage( Hand hand )
		// {
		// 	// GameObject currentAttachedObject = hand.currentAttachedObject;

		// 	// if ( currentAttachedObject == null ) // verify the hand is holding something
		// 	// if (!hand.hasCurrentAttached)
		// 	if (hand.currentAttached == null)
		// 	{
		// 		return null;
		// 	}
		// 	GameObject currentAttachedObject = hand.currentAttached.attachedObject;

		// 	ItemPackageReference packageReference = currentAttachedObject.GetComponent<ItemPackageReference>();
		// 	if ( packageReference == null ) // verify the item in the hand is matchable
		// 	{
		// 		return null;
		// 	}

		// 	ItemPackage attachedItemPackage = packageReference.itemPackage; // return the ItemPackage reference we find.

		// 	return attachedItemPackage;
		// }
		private ItemPackage GetAttachedItemPackage( Inventory inventory )
		{

			for (int i = 0; i < inventory.equippedSlots.Length; i++){
				if (inventory.equippedSlots[i] != null) {
					ItemPackageReference packageReference = inventory.equippedSlots[i].sceneItem.GetComponent<ItemPackageReference>();
					if ( packageReference == null ) // verify the item in the hand is matchable
					{
						continue;
						// return null;
					}

					ItemPackage attachedItemPackage = packageReference.itemPackage; // return the ItemPackage reference we find.

					return attachedItemPackage;

				}
			}
			return null;


			// // GameObject currentAttachedObject = hand.currentAttachedObject;

			// // if ( currentAttachedObject == null ) // verify the hand is holding something
			// // if (!hand.hasCurrentAttached)
			// if (inventory.equippedItem == null)
			// {
			// 	return null;
			// }
			// GameObject currentAttachedObject = inventory.equippedItem.item.gameObject;

			// ItemPackageReference packageReference = currentAttachedObject.GetComponent<ItemPackageReference>();
			// if ( packageReference == null ) // verify the item in the hand is matchable
			// {
			// 	return null;
			// }

			// ItemPackage attachedItemPackage = packageReference.itemPackage; // return the ItemPackage reference we find.

			// return attachedItemPackage;
		}


		//-------------------------------------------------
		// private void HandHoverUpdate( Hand hand )
		// {
		// 	if ( takeBackItem && requireReleaseActionToReturn )
		// 	{
        //         if (hand.isActive)
		// 		{
		// 			ItemPackage currentAttachedItemPackage = GetAttachedItemPackage( hand );
        //             if (currentAttachedItemPackage == itemPackage && 
					
		// 				// hand.IsGrabEnding(currentAttachedItemPackage.gameObject)
		// 				hand.GetGrabUp()
		// 			)
		// 			{
		// 				TakeBackItem( hand );
		// 				return; // So that we don't pick up an ItemPackage the same frame that we return it
		// 			}
		// 		}
		// 	}

		// 	if ( requireGrabActionToTake )
		// 	{
        //         // GrabTypes startingGrab = hand.GetGrabStarting();

		// 		// if (startingGrab != GrabTypes.None)
		// 		if (hand.GetGrabDown())
		// 		{
		// 			SpawnAndAttachObject( hand);//, GrabTypes.Scripted);
		// 		}
		// 	}
		// }


		//-------------------------------------------------
		// private void OnHandHoverEnd( Hand hand )
		// {
		// 	// if ( !justPickedUpItem && requireGrabActionToTake && showTriggerHint )
		// 	// {
        //     //     hand.HideGrabHint();
		// 	// }

		// 	justPickedUpItem = false;
		// }


		//-------------------------------------------------
		// private void RemoveMatchingItemsFromHandStack( ItemPackage package, Hand hand )
		// {
        //     if (hand == null)
        //         return;

		// 	if (hand.currentAttached == null)
		// 		return;


		// 	// if (!hand.hasCurrentAttached)
		// 	// 	return;

		// 	// for ( int i = 0; i < hand.AttachedObjects.Count; i++ )
		// 	// {
		// 		// ItemPackageReference packageReference = hand.AttachedObjects[i].attachedObject.GetComponent<ItemPackageReference>();
		// 		ItemPackageReference packageReference = hand.currentAttached.attachedObject.GetComponent<ItemPackageReference>();
				
		// 		if ( packageReference != null )
		// 		{
		// 			ItemPackage attachedObjectItemPackage = packageReference.itemPackage;
		// 			if ( ( attachedObjectItemPackage != null ) && ( attachedObjectItemPackage == package ) )
		// 			{
		// 				// GameObject detachedItem = hand.AttachedObjects[i].attachedObject;
		// 				GameObject detachedItem = hand.currentAttached.attachedObject;
						
		// 				hand.DetachObject( detachedItem );
		// 			}
		// 		}
		// 	// }
		// }

		private void RemoveMatchingItemsFromHandStack( ItemPackage package, Inventory inventory )
		{





			for (int i =0 ; i < inventory.equippedSlots.Length; i++) {
				if (inventory.equippedSlots[i] != null) {
					ItemPackageReference packageReference = inventory.equippedSlots[i].sceneItem.GetComponent<ItemPackageReference>();
					if ( packageReference != null )
					{
						ItemPackage attachedObjectItemPackage = packageReference.itemPackage;
						if ( ( attachedObjectItemPackage != null ) && ( attachedObjectItemPackage == package ) )
						{
							inventory.UnequipItem(i, false);
						}
					}
				}
			}




            // if (inventory.equippedItem == null)
			// 	return;

				
			// // if (!hand.hasCurrentAttached)
			// // 	return;

			// // for ( int i = 0; i < hand.AttachedObjects.Count; i++ )
			// // {
			// 	// ItemPackageReference packageReference = hand.AttachedObjects[i].attachedObject.GetComponent<ItemPackageReference>();
			// 	ItemPackageReference packageReference = inventory.equippedItem.item.GetComponent<ItemPackageReference>();
				
			// 	if ( packageReference != null )
			// 	{
			// 		ItemPackage attachedObjectItemPackage = packageReference.itemPackage;
			// 		if ( ( attachedObjectItemPackage != null ) && ( attachedObjectItemPackage == package ) )
			// 		{
			// 			// GameObject detachedItem = hand.AttachedObjects[i].attachedObject;
			// 			// GameObject detachedItem = inventory.equippedItem.attachedObject;
						
			// 			inventory.UnequipItem( inventory.equippedItem.item );
			// 		}
			// 	}
			// // }
		}


		//-------------------------------------------------
		private void RemoveMatchingItemTypesFromHand( ItemPackage.ItemPackageType packageType, Inventory inventory)//, Hand hand )
		{


			for (int i =0 ; i < inventory.equippedSlots.Length; i++) {
				if (inventory.equippedSlots[i] != null) {
					ItemPackageReference packageReference = inventory.equippedSlots[i].sceneItem.GetComponent<ItemPackageReference>();
					if ( packageReference != null )
					{
						if ( packageReference.itemPackage.packageType == packageType )
						{
							inventory.UnequipItem(i, false);
						}
					}
				}
			}


			// if (inventory.equippedItem == null)
			// // if (hand.currentAttached == null)
			// // if (!hand.hasCurrentAttached)
			// 	return;

			// // for ( int i = 0; i < hand.AttachedObjects.Count; i++ )
			// // {
			// 	// ItemPackageReference packageReference = hand.AttachedObjects[i].attachedObject.GetComponent<ItemPackageReference>();
			// 	ItemPackageReference packageReference = inventory.equippedItem.item.GetComponent<ItemPackageReference>();
				
			// 	if ( packageReference != null )
			// 	{
			// 		if ( packageReference.itemPackage.packageType == packageType )
			// 		{
			// 			// GameObject detachedItem = hand.AttachedObjects[i].attachedObject;
			// 			// GameObject detachedItem = hand.currentAttached.attachedObject;
						
			// 			inventory.UnequipItem( inventory.equippedItem.item );
			// 		}
			// 	}
			// // }
		}


		//-------------------------------------------------
		// private void SpawnAndAttachObject( Hand hand)//, GrabTypes grabType )
		private void SpawnAndAttachObject( Inventory inventory, int interactorID)//, GrabTypes grabType )
		
		{

			// if ( hand.otherHand != null )
			
			// {
				//If the other hand has this item package, take it back from the other hand

				// Inventory otherInventory = inventory.otherInventory;
				ItemPackage otherHandItemPackage = GetAttachedItemPackage( inventory);// otherInventory );//hand.otherHand );
				if ( otherHandItemPackage == itemPackage )
				{
					TakeBackItem( inventory);// otherInventory) ;//hand.otherHand );
				}
			// }

			// if ( showTriggerHint )
			// {
            //     hand.HideGrabHint();
			// }

			// if ( itemPackage.otherHandItemPrefab != null )
			// {

			// 	if (otherInventory.GetComponent<Interactor>().hoverLocked)
			// 	// if ( hand.otherHand.hoverLocked )
			// 	{
            //         Debug.Log( "<b>[SteamVR Interaction]</b> Not attaching objects because other hand is hoverlocked and we can't deliver both items." );
            //         return;
			// 	}
			// }

			// if we're trying to spawn a one-handed item, remove one and two-handed items from this hand and two-handed items from both hands
			if ( itemPackage.packageType == ItemPackage.ItemPackageType.OneHanded )
			{
				RemoveMatchingItemTypesFromHand( ItemPackage.ItemPackageType.OneHanded, inventory);//hand );
				RemoveMatchingItemTypesFromHand( ItemPackage.ItemPackageType.TwoHanded, inventory);//hand );
				// RemoveMatchingItemTypesFromHand( ItemPackage.ItemPackageType.TwoHanded, otherInventory);//hand.otherHand );
			}

			// if we're trying to spawn a two-handed item, remove one and two-handed items from both hands
			if ( itemPackage.packageType == ItemPackage.ItemPackageType.TwoHanded )
			{
				RemoveMatchingItemTypesFromHand( ItemPackage.ItemPackageType.OneHanded, inventory);//hand );
				// RemoveMatchingItemTypesFromHand( ItemPackage.ItemPackageType.OneHanded, otherInventory);//hand.otherHand );
				RemoveMatchingItemTypesFromHand( ItemPackage.ItemPackageType.TwoHanded, inventory);//hand );
				// RemoveMatchingItemTypesFromHand( ItemPackage.ItemPackageType.TwoHanded, otherInventory);//hand.otherHand );
			}

			spawnedItem = GameObject.Instantiate( itemPackage.itemPrefab );
			spawnedItem.SetActive( true );



			//dont quick equip
			Item sceneItem = spawnedItem.GetComponent<Item>();
			inventory.EquipItem(sceneItem.itemBehavior, interactorID, null);// sceneItem.itemBehavior.equipSlot, null);
			// inventory.SwitchMainUsedEquipPoint();


			// inventory.EquipItem( spawnedItem.GetComponent<Item>()//, 
			// 	// grabType, 
			// 	// attachmentFlags 
			// 	);

			if ( ( itemPackage.otherHandItemPrefab != null ) )// && ( hand.otherHand.isActive ) )
			{
				GameObject otherHandObjectToAttach = GameObject.Instantiate( itemPackage.otherHandItemPrefab );
				otherHandObjectToAttach.SetActive( true );

				// otherInventory.EquipItem(otherHandObjectToAttach.GetComponent<Item>());


				sceneItem = otherHandObjectToAttach.GetComponent<Item>();
				//dont quick equip
				// otherInventory.
				inventory.EquipItem(sceneItem.itemBehavior, 1-interactorID, null);//sceneItem.itemBehavior.equipSlot, null);


			}

			itemIsSpawned = true;

			justPickedUpItem = true;

			if ( takeBackItem )
			{
				useFadedPreview = true;
				pickupEvent.Invoke();
				CreatePreviewObject();
			}
		}
	}
}
