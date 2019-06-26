using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace InventorySystem {

    public class EquipOnConsume : MonoBehaviour, IStashedItem
    {


        public void OnItemStashed (Inventory inventory, ItemBehavior item, int count) {

        }
        public void OnItemDropped (Inventory inventory, ItemBehavior item, int count) {

        }
        public void OnItemConsumed (Inventory inventory, ItemBehavior item, int equipSlot) {

            Debug.LogError("Equipping on consume");
            if (inventory.ItemIsEquipped(equipSlot, item)) {
                inventory.UnequipItem(equipSlot, false);
            }
            else {
                inventory.EquipItem(item, equipSlot, null);
            }
        
        }
    }
}
