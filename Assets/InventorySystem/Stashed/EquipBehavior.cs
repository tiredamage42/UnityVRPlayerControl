// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;



namespace InventorySystem {

    [CreateAssetMenu(menuName="Stashed Item Behaviors/Equip Behavior")]
    public class EquipBehavior : StashedItemBehavior// MonoBehaviour, IStashedItem
    {

        public bool equipOnConsume, equipOnManualStash;


        public override void OnItemStashed (Inventory inventory, ItemBehavior item, int count, int equipSlot, bool manual) {
            
            if (equipOnManualStash && manual) {
                if (!inventory.ItemIsEquipped(equipSlot, item)) {
                    inventory.EquipItem(item, equipSlot, null);
                }
            }
        }
        public override void OnItemDropped (Inventory inventory, ItemBehavior item, int count) {

        }
        public override void OnItemConsumed (Inventory inventory, ItemBehavior item, int count, int equipSlot) {
            if (equipOnConsume) {
                // Debug.LogError("Equipping on consume");
                if (inventory.ItemIsEquipped(equipSlot, item)) {
                    inventory.UnequipItem(equipSlot, false);
                }
                else {
                    inventory.EquipItem(item, equipSlot, null);
                }
            }
        }
    }


}
