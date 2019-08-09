// using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using InteractionSystem;

namespace InventorySystem{

    [RequireComponent(typeof(Interactable))]
    public class InventoryUIContextTrigger : MonoBehaviour, IInteractable
    {
        public string contextName;
        public int useAction = 0;
        public string useActionName;
        public List<int> categoryFilter = new List<int>();

        public Inventory linkedInventory;

        Interactable interactable;
        void Awake () {
            interactable = GetComponent<Interactable>();
            if (linkedInventory == null)
                linkedInventory = GetComponent<Inventory>();

            UpdateInteractableUseNames();
        }

#if UNITY_EDITOR
        void Update () {
            UpdateInteractableUseNames();
        }
#endif

        void UpdateInteractableUseNames () {
            if (interactable.actionNames.Length < useAction+1) {
                System.Array.Resize(ref interactable.actionNames, useAction+1);
            }
            interactable.actionNames[useAction] = useActionName;//"Trade";
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
