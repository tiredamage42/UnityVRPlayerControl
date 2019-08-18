using UnityEngine;
using Game.InventorySystem;

[CreateAssetMenu(menuName="Inventory System/Item Behaviors/Item Equip")]
public class Item_Equip : StashedItemBehavior
{
    public bool equipOnConsume, equipOnManualStash;

    public override void OnItemEquipped (Inventory inventory, ItemBehavior item, int equipSlot) { }
    public override void OnItemUnequipped (Inventory inventory, ItemBehavior item, int equipSlot) { }
    
    public override void OnItemStashed (Inventory inventory, ItemBehavior item, int count, int equipSlot, bool manual) {
        
        if (equipOnManualStash && manual) {
            InventoryEquipper ie = inventory.equipper;
            if (ie != null) {
                if (!ie.ItemIsEquipped(equipSlot, item)) {
                    ie.EquipItem(item, equipSlot, null);
                }
            }
        }
    }
    public override void OnItemDropped (Inventory inventory, ItemBehavior item, int count) { }
    public override void OnItemConsumed (Inventory inventory, ItemBehavior item, int count, int equipSlot) {
        if (equipOnConsume) {
            InventoryEquipper ie = inventory.equipper;
            if (ie != null) {
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


