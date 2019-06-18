

using UnityEngine;
namespace InventorySystem {
    public interface IInventoryItem {
        void OnEquipped(Inventory inventory);
        void OnUnequipped(Inventory inventory);
        void OnEquippedUpdate(Inventory inventory);   
    }
}
