using UnityEngine;

namespace InventorySystem {

    public abstract class StashedItemBehavior : ScriptableObject {
        public abstract void OnItemStashed(Inventory inventory, ItemBehavior item, int count, int equipSlot, bool manual);
        public abstract void OnItemDropped(Inventory inventory, ItemBehavior item, int count);
        public abstract void OnItemConsumed(Inventory inventory, ItemBehavior item, int count, int equipSlot);
    }
}
