// //======= Copyright (c) Valve Corporation, All rights reserved. ===============
// //
// // Purpose: Creates an object and attaches it to the hand
// //
// //=============================================================================

// using UnityEngine;
// using System.Collections;
// using InventorySystem;
// namespace Valve.VR.InteractionSystem
// {
// 	//-------------------------------------------------------------------------
// 	public class SpawnAndAttachToHand : MonoBehaviour
// 	{
// 		// public Hand hand;
// 		public GameObject prefab;

// 		public Item prefabItem;


// 		//-------------------------------------------------
// 		public void SpawnAndAttach( Inventory inventory) // Hand passedInhand )
// 		{
// 			Debug.LogError("Spawn and attach used by :: " + name);



// 			// Hand handToUse = passedInhand;
// 			// if ( passedInhand == null )
// 			// {
// 			// 	handToUse = hand;
// 			// }

// 			// if ( handToUse == null )
// 			// {
// 			// 	return;
// 			// }

// 			if (prefabItem == null) {
// 				Debug.LogError(name + " needs prefab item spawn attach");
// 			}
// 			Item cloned = Instantiate( prefabItem );

// 			if (inventory == null) {
// 				Debug.LogError(name + " needs inventory spawn attach");
// 				return;
// 			}

// 			inventory.EquipItem(cloned);

				

// 			// GameObject prefabObject = Instantiate( prefab ) as GameObject;
// 			// handToUse.AttachObject( prefabObject//, 
// 			// 	// GrabTypes.Scripted 
// 			// 	);
// 		}
// 	}
// }
