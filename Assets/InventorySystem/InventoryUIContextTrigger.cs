using System.Collections.Generic;
using UnityEngine;

using InteractionSystem;

namespace InventorySystem{
    public class InventoryUIContextTrigger : MonoBehaviour, IInteractable
    {
        
        public int useAction = 0;
        public string contextName;
        public List<int> categoryFilter = new List<int>();
        public Inventory linkedInventory;
        void Awake () {
            if (linkedInventory == null) linkedInventory = GetComponent<Inventory>();
        }


        public void OnInteractableAvailabilityChange(bool available) { }
        public void OnInteractableInspectedStart (InteractionPoint interactor) { }
        public void OnInteractableInspectedEnd (InteractionPoint interactor) { }
        public void OnInteractableInspectedUpdate (InteractionPoint interactor) { }

        public void OnInteractableUsedStart (InteractionPoint interactor, int useAction) {
            if ( useAction != this.useAction ) return;
            Inventory interactorInventory = interactor.GetComponentInParent<Inventory>();
            if (interactorInventory != null) {
                interactorInventory.InitiateInventoryManagement(contextName, interactor.interactorID, linkedInventory, categoryFilter);
            }
        }

        public void OnInteractableUsedEnd (InteractionPoint interactor, int useAction) { }
        public void OnInteractableUsedUpdate (InteractionPoint interactor, int useAction) { }

    }
}
