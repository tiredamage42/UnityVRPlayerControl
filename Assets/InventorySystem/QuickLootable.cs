using UnityEngine;
using InteractionSystem;
namespace InventorySystem{
    public class QuickLootable : MonoBehaviour, IInteractable
    {
        public Inventory linkedInventory;
        void Awake () {
            if (linkedInventory == null) linkedInventory = GetComponent<Inventory>();
        }

        public void OnInteractableAvailabilityChange(bool available) { }
            
        public void OnInteractableInspectedStart (InteractionPoint interactor) {
            if (linkedInventory != null && interactor.inventory != null) interactor.inventory.InitiateInventoryManagement(Inventory.quickTradeContext, interactor.interactorID, linkedInventory, null);
        }
        public void OnInteractableInspectedEnd (InteractionPoint interactor) {
            if (linkedInventory != null && interactor.inventory != null) interactor.inventory.EndInventoryManagement(Inventory.quickTradeContext, interactor.interactorID);                
        }

        public void OnInteractableInspectedUpdate (InteractionPoint interactor) { }
        public void OnInteractableUsedStart (InteractionPoint interactor, int useAction) { }
        public void OnInteractableUsedEnd (InteractionPoint interactor, int useAction) { }
        public void OnInteractableUsedUpdate (InteractionPoint interactor, int useAction) { }

    }

}
