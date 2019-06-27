

using UnityEngine;
namespace InventorySystem {
    public interface ISceneItem {
        void OnEquipped(Inventory inventory);
        void OnUnequipped(Inventory inventory);
        void OnEquippedUpdate(Inventory inventory);   

        void OnEquippedUseStart(Inventory inventory, int useIndex);
        void OnEquippedUseEnd(Inventory inventory, int useIndex);
        void OnEquippedUseUpdate(Inventory inventory, int useIndex);
    }

    public interface IStashedItem {

        void OnItemStashed (Inventory inventory, ItemBehavior item, int count);
        void OnItemDropped (Inventory inventory, ItemBehavior item, int count);
        void OnItemConsumed (Inventory inventory, ItemBehavior item, int count, int equipSlot);



        // void OnEquipped(Inventory inventory);
        // void OnUnequipped(Inventory inventory);
        // void OnEquippedUpdate(Inventory inventory);   

        // void OnEquippedUseStart(Inventory inventory, int useIndex);
        // void OnEquippedUseEnd(Inventory inventory, int useIndex);
        // void OnEquippedUseUpdate(Inventory inventory, int useIndex);
    }
}
