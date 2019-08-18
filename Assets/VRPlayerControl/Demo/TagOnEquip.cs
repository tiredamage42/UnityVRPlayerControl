﻿using UnityEngine;
using RenderTricks;
using Game.InventorySystem;

namespace VRPlayerDemo
{
	public class TagOnEquip : MonoBehaviour, ISceneItem
	{
		public void OnEquippedUseStart(Inventory inventory, int useIndex) {}
        public void OnEquippedUseEnd(Inventory inventory, int useIndex) {}
        public void OnEquippedUseUpdate(Inventory inventory, int useIndex) {}

		public void OnEquipped(Inventory inventory) {
			ColorTag.TagObject(gameObject, Color.red);
		}
        public void OnUnequipped(Inventory inventory){
			ColorTag.UntagObject(gameObject);
		}
        public void OnEquippedUpdate(Inventory inventory){

		}
	}
}