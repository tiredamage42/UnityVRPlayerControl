

using UnityEngine;
namespace InventorySystem {
    public interface IInventoryItem {
        void OnEquipped(Inventory inventory);
        void OnUnequipped(Inventory inventory);
        void OnEquippedUpdate(Inventory inventory);   

        void OnEquippedUseStart(Inventory inventory, int useIndex);
        void OnEquippedUseEnd(Inventory inventory, int useIndex);
        void OnEquippedUseUpdate(Inventory inventory, int useIndex);
    }
}
