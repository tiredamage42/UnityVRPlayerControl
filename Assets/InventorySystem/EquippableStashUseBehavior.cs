using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace InventorySystem{
    
[CreateAssetMenu()]
    public class EquippableStashUseBehavior : StashUseBehavior {
        protected override void _OnStashedUse (Inventory inventory, ItemBehavior item, int useAction, int slotIndex, int affectingCount, Inventory secondaryInventory) {
            Debug.LogError("equipable stash use ation");
            if (useAction == Inventory.UI_USE_ACTION) {
                if (inventory.ItemIsEquipped(slotIndex, item)) {
                    inventory.UnequipItem(slotIndex, false);
                }
                else {
                    inventory.EquipItem(item, slotIndex, null);
                }
            }
        }
    }
}