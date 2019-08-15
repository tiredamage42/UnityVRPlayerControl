using UnityEngine;

namespace InventorySystem {

    [CreateAssetMenu(menuName="Stashed Item Behaviors/Equip Behavior")]
    public class EquipBehavior : StashedItemBehavior
    {
        public bool equipOnConsume, equipOnManualStash;

        public override void OnItemStashed (Inventory inventory, ItemBehavior item, int count, int equipSlot, bool manual) {
            
            if (equipOnManualStash && manual) {
                InventoryEqupping ie = inventory.equipping;
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
                InventoryEqupping ie = inventory.equipping;
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
