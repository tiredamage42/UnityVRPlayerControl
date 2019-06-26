using System.Collections;
using System.Collections.Generic;
using UnityEngine;




namespace InventorySystem {
    public class StashedItem : MonoBehaviour
    {
        public void OnItemStashed(Inventory inventory, ItemBehavior item, int count) {
            InitializeListeners();
            for (int i = 0; i < listeners.Count; i++) {
                listeners[i].OnItemStashed(inventory, item, count);
            }
        }
        public void OnItemDropped(Inventory inventory, ItemBehavior item, int count) {
            InitializeListeners();
            for (int i = 0; i < listeners.Count; i++) {
                listeners[i].OnItemDropped(inventory, item, count);
            }
        }
        public void OnItemConsumed(Inventory inventory, ItemBehavior item, int equipSlot) {
            InitializeListeners();
            for (int i = 0; i < listeners.Count; i++) {
                listeners[i].OnItemConsumed(inventory, item, equipSlot);
            }
        }

        List<IStashedItem> listeners = new List<IStashedItem>();
        void InitializeListeners() {
            if (listeners.Count == 0) {
                IStashedItem[] listeners_ = GetComponents<IStashedItem>();
                for (int i = 0; i< listeners_.Length; i++) {
                    this.listeners.Add(listeners_[i]);
                }
                if (listeners.Count == 0) {
                    Debug.LogError("calling callbacks on stashed item: " + name + ", but it has no listeners");
                }
            }
        }
    }
}
