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
                InventoryEqupping ie = inventory.GetComponent<InventoryEqupping>();
                if (ie != null) {
                    if (!ie.ItemIsEquipped(equipSlot, item)) {
                        ie.EquipItem(item, equipSlot, null);
                    }

                }
            }
        }
        public override void OnItemDropped (Inventory inventory, ItemBehavior item, int count) {

        }
        public override void OnItemConsumed (Inventory inventory, ItemBehavior item, int count, int equipSlot) {
            if (equipOnConsume) {
                InventoryEqupping ie = inventory.GetComponent<InventoryEqupping>();
                if (ie != null) {
                
                // Debug.LogError("Equipping on consume");
                if (ie.ItemIsEquipped(equipSlot, item)) {
                    ie.UnequipItem(equipSlot, false);
                }
                else {
                    ie.EquipItem(item, equipSlot, null);
                }
                }
            }
        }
    }


}
