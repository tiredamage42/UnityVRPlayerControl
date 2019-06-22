using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using InteractionSystem;

namespace InventorySystem {

    public class Item : MonoBehaviour, IInteractable
    {
        public Inventory.EquipType equipType = Inventory.EquipType.Normal;
        public TransformBehavior equipBehavior;
        public bool hoverLockOnEquip = true;

        Interactable interactable;
        new public Rigidbody rigidbody;

        void Awake () {
            interactable = GetComponent<Interactable>();
            rigidbody = GetComponent<Rigidbody>();

            InitializeInteractableComponents();
        }

        public int useActionForEquip = 0;

        [Header("Set to -1 for no stash")]
        public int useActionForStash = 1;

        public void OnInspectStart(Interactor interactor) {}
        public void OnInspectEnd(Interactor interactor) {}
        public void OnInspectUpdate(Interactor interactor) {}
        public void OnUseStart(Interactor interactor, int useIndex) {
            if (useIndex == useActionForEquip) {
                Inventory inventory = interactor.GetComponent<Inventory>();
                if (inventory != null) {
                    if (hoverLockOnEquip)
                        interactor.HoverLock( interactable );

                    
                    inventory.EquipItem(this);
                }
            }
            else if (useIndex == useActionForStash) {

            }
            else {
                if (useIndex != 0 && useIndex != 1) 
                    Debug.LogError("unknown action for item " + name + "\naction: " + useIndex + "\ninteractor:" + interactor.name);
            }
        }
        public bool unequipOnUseEnd = true;
        public void OnUseEnd(Interactor interactor, int useIndex) {
            if (!unequipOnUseEnd)
                return;
                
            if (useIndex == useActionForEquip) {
                // Debug.LogError("should beeee");
                Inventory inventory = interactor.GetComponent<Inventory>();
                if (inventory != null) {
                    if (inventory == parentInventory) {
                    
                        if (hoverLockOnEquip)
                            interactor.HoverUnlock( interactable );


                        inventory.UnequipItem(this);
                        

                        // Uncomment to detach ourselves late in the frame.
                        // This is so that any vehicles the player is attached to
                        // have a chance to finish updating themselves.
                        // If we detach now, our position could be behind what it
                        // will be at the end of the frame, and the object may appear
                        // to teleport behind the hand when the player releases it.
                        //StartCoroutine( LateDetach( hand ) );

                    }
                }
            }
            else if (useIndex == useActionForStash) {
                Debug.LogError("stash!");

            }
            else {
                if (useIndex != 0 && useIndex != 1) 
                    Debug.LogError("unknown action for item " + name + "\naction: " + useIndex + "\ninteractor:" + interactor.name);
            }
        }
        public void OnUseUpdate(Interactor interactor, int useIndex) {

        }








        protected virtual IEnumerator LateDetach( Inventory inventory )
		{
			yield return new WaitForEndOfFrame();
            inventory.UnequipItem(this);
		}







        [HideInInspector] public Inventory parentInventory;

        public void OnEquipped (Inventory inventory) {
            this.parentInventory = inventory;
            interactable.isAvailable = false;
                        
            
            for (int i = 0; i < itemComponents.Count; i++) {
                itemComponents[i].OnEquipped(inventory);
            }
        }
        public void OnUnequipped (Inventory inventory) {
            this.parentInventory = null;
            interactable.isAvailable = true;
                        
            
            for (int i = 0; i < itemComponents.Count; i++) {
                itemComponents[i].OnUnequipped(inventory);
            }
        }
        public void OnEquippedUpdate(Inventory inventory) {
            for (int i = 0; i < itemComponents.Count; i++) {
                itemComponents[i].OnEquippedUpdate(inventory);
            }
        }


        public void OnEquippedUseStart (Inventory interactor, int useIndex) {
            for (int i = 0; i < itemComponents.Count; i++) {
                itemComponents[i].OnEquippedUseStart(interactor, useIndex);
            }
            // if (onUseStart != null) {
            //     onUseStart(interactor, useIndex);
            // }
        }
        public void OnEquippedUseEnd (Inventory interactor, int useIndex) {
            for (int i = 0; i < itemComponents.Count; i++) {
                itemComponents[i].OnEquippedUseEnd(interactor, useIndex);
            }
            // if (onUseEnd != null) {
            //     onUseEnd(interactor, useIndex);
            // }
        }
        public void OnEquippedUseUpdate(Inventory interactor, int useIndex) {
            for (int i = 0; i < itemComponents.Count; i++) {
                itemComponents[i].OnEquippedUseUpdate(interactor, useIndex);
            }
            // if (onUseUpdate != null) {
            //     onUseUpdate(interactor, useIndex);
            // }
        }

        List<IInventoryItem> itemComponents = new List<IInventoryItem>();
        void InitializeInteractableComponents() {
            IInventoryItem[] itemComponents = GetComponents<IInventoryItem>();
            for (int i = 0; i< itemComponents.Length; i++) {
                this.itemComponents.Add(itemComponents[i]);
            }
        }
        public void AddItemComponent (IInventoryItem itemComponent) {
            itemComponents.Add(itemComponent);
        }

        void OnDestroy()
        {
            if (parentInventory != null)
            {
                parentInventory.UnequipItem(this, false);
            }
        }
    }
}
