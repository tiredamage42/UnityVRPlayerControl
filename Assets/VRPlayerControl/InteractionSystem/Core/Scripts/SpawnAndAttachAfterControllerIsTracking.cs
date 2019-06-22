// //======= Copyright (c) Valve Corporation, All rights reserved. ===============
// //
// // Purpose: Spawns and attaches an object to the hand after the controller has
// //			tracking
// //
// //=============================================================================

// using UnityEngine;
// using System.Collections;
// using InventorySystem;
// using VRPlayer;
// namespace Valve.VR.InteractionSystem
// {
// 	//-------------------------------------------------------------------------
// 	public class SpawnAndAttachAfterControllerIsTracking : MonoBehaviour
// 	{
// 		// private Hand hand;
// 		Inventory inventory;
// 		public GameObject itemPrefab;

// 		public Item itemPf;

	
// 		//-------------------------------------------------
// 		void Start()
// 		{
// 			inventory = GetComponentInParent<Inventory>();
// 		}


// 		//-------------------------------------------------
// 		void Update()
// 		{
// 			if ( itemPrefab != null )
// 			{
// 				Hand hand = inventory.GetComponent<Hand>();
//                 if (hand.isActive && hand.isPoseValid)
//                 {
//                     // GameObject objectToAttach = GameObject.Instantiate(itemPrefab);
//                     Item item = GameObject.Instantiate(itemPf);
                    
// 					item.gameObject.SetActive(true);

// 					inventory.EquipItem(item);
//                     // hand.AttachObject(objectToAttach);//, GrabTypes.Scripted);
                    

// 					// hand.TriggerHapticPulse(800);

// 					StandardizedVRInput.instance.TriggerHapticPulse(hand.handType, 800);
                    
// 					Destroy(gameObject);

//                     // If the player's scale has been changed the object to attach will be the wrong size.
//                     // To fix this we change the object's scale back to its original, pre-attach scale.
//                     item.transform.localScale = itemPrefab.transform.localScale;
//                 }
// 			}
// 		}
// 	}
// }
