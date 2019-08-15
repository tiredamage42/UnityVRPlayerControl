using System.Collections.Generic;
using UnityEngine;

using InteractionSystem;

namespace InventorySystem{
    public class InventoryUIContextTrigger : MonoBehaviour, IInteractable
    {
        public int useAction = 0;
        public string contextName;
        public Inventory suppliedInventory;
        [NeatArray] public NeatIntList categoryFilter;
        void Awake () {
            if (suppliedInventory == null) suppliedInventory = GetComponent<Inventory>();
        }

        public void OnInteractableAvailabilityChange(bool available) { }
        public void OnInteractableInspectedStart (InteractionPoint interactor) { }
        public void OnInteractableInspectedEnd (InteractionPoint interactor) { }
        public void OnInteractableInspectedUpdate (InteractionPoint interactor) { }

        public void OnInteractableUsedStart (InteractionPoint interactor, int useAction) {
            if (useAction == this.useAction && interactor.inventory != null) interactor.inventory.InitiateInventoryManagement(contextName, interactor.interactorID, suppliedInventory, categoryFilter);
        }

        public void OnInteractableUsedEnd (InteractionPoint interactor, int useAction) { }
        public void OnInteractableUsedUpdate (InteractionPoint interactor, int useAction) { }

    }
}
