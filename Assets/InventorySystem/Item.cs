// using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using InteractionSystem;

namespace InventorySystem {


/*


can only perform action 0 when unequipped

for gun / weapon:

    stash action = 0 or 1 (either action will put it in the inventory)
    equip action = 0 (if we're unarmed automatically equip it)


for environment block:

    stash action = 1
    equip action = 0 

    can only equip with unarmed hand, can still stash though
    
health


OnUseStashed <- called from inventory


weapons:
//if equip on use stash:
    Equip item

aid items:
//if destroy on use stash destroy from inventory

 */

    /*
        scene representation of the item
    */
    public class Item : MonoBehaviour, IInteractable
    {

        static Dictionary<int, HashSet<Item>> itemPoolsPerPrefab = new Dictionary<int, HashSet<Item>>();
        
        public static Item GetSceneItem (ItemBehavior itemBehavior) {
            Item prefab = itemBehavior.scenePrefabVariations[Random.Range(0, itemBehavior.scenePrefabVariations.Length)];
            
            int instanceID = prefab.GetInstanceID();
            
            Item sceneItem = null;
            bool hasKey = false;
            if (itemPoolsPerPrefab.ContainsKey(instanceID)) {
                hasKey = true;

                foreach (var it in itemPoolsPerPrefab[instanceID]) {
                    if (!it.gameObject.activeInHierarchy) {
                        sceneItem = it;
                        break;
                    }
                }
            }
            
            if (sceneItem == null) {
                sceneItem = GameObject.Instantiate(prefab);

                if (hasKey) {
                    itemPoolsPerPrefab[instanceID].Add(sceneItem);
                }
                else {
                    itemPoolsPerPrefab.Add(instanceID, new HashSet<Item>(){ sceneItem });
                }
            }

            sceneItem.itemBehavior = itemBehavior;
            return sceneItem;
        }


        public int itemCount = 1;
        public ItemBehavior itemBehavior;
        Interactable interactable;
        [HideInInspector] new public Rigidbody rigidbody;

        void Awake () {
            interactable = GetComponent<Interactable>();

            interactable.actionNames = new string[] {
                "Grab", "Stash"
            };


            rigidbody = GetComponent<Rigidbody>();
            itemColliders = GetComponentsInChildren<Collider>();

            InitializeListeners();
        }
        List<ISceneItem> listeners = new List<ISceneItem>();
        void InitializeListeners() {
            ISceneItem[] listeners_ = GetComponents<ISceneItem>();
            for (int i = 0; i< listeners_.Length; i++) {
                this.listeners.Add(listeners_[i]);
            }
        }
        public void AddListener (ISceneItem listener) {
            listeners.Add(listener);
        }

            
        Collider[] itemColliders;
        
        public void OnInteractableAvailabilityChange(bool available) {
			
		}
		
        public void OnInteractableInspectedStart(InteractionPoint interactor) {}
        public void OnInteractableInspectedEnd(InteractionPoint interactor) {}
        public void OnInteractableInspectedUpdate(InteractionPoint interactor) {}
        public void OnInteractableUsedStart(InteractionPoint interactor, int useIndex) {

            // Inventory inventory = interactor.inventory;

            // if (inventory == null)
            //     return;

            if (interactor.inventory != null) interactor.inventory.OnSceneItemActionStart (this, interactor.interactorID, useIndex);


            // // if we cant quick equip this item, either action stashes it...
            // if (useIndex == Inventory.STASH_ACTION || (useIndex == Inventory.GRAB_ACTION && !itemBehavior.canQuickEquip)) {
            //     inventory.StashItem(this, interactor.interactorID);
            // }
            // else if (useIndex == Inventory.GRAB_ACTION && itemBehavior.canQuickEquip) {   
            //     InventoryEqupping ie = inventory.equipping;
            //     if (ie != null) ie.EquipItem(itemBehavior, interactor.interactorID, this );
            // }
        }

        public void OnInteractableUsedEnd(InteractionPoint interactor, int useIndex) {

        }
        public void OnInteractableUsedUpdate(InteractionPoint interactor, int useIndex) {

        }

        [HideInInspector] public Inventory parentInventory;
        [HideInInspector] public EquipPoint myEquipPoint;



        void TemporarilySetCollidersToEquippedItemLayer () {
            for (int i = 0; i < itemColliders.Length; i++) {
                colliderLayers.Add(new ColliderLayerPair(itemColliders[i], Inventory.equippedItemLayer));
            }
        }
        void ResetItemColliderLayersToOriginals () {
            for (int i = colliderLayers.Count - 1; i >= 0; i--) {
                colliderLayers[i].ResetPair();
                colliderLayers.Remove(colliderLayers[i]);
            }
        }

        List<ColliderLayerPair> colliderLayers = new List<ColliderLayerPair>();

        struct ColliderLayerPair {
            Collider collider;
            int originalLayer;
            public void ResetPair () {
                collider.gameObject.layer = originalLayer;
            }

            public ColliderLayerPair (Collider collider, string newLayer) {
                this.collider = collider;
                originalLayer = collider.gameObject.layer;
                collider.gameObject.layer = LayerMask.NameToLayer(newLayer);
            }
        }
        
        public void OnEquipped (Inventory inventory) {
            
            interactable.SetAvailable(false);

            TemporarilySetCollidersToEquippedItemLayer();
            
            // if (itemBehavior.hoverLockOnEquip)
            //             interactor.HoverLock( interactable );
            
            for (int i = 0; i < listeners.Count; i++) {
                listeners[i].OnEquipped(inventory);
            }
        }
        public void OnUnequipped (Inventory inventory) {
            interactable.SetAvailable(true);

            ResetItemColliderLayersToOriginals();

            // if (hoverLockOnEquip)
            //     interactor.HoverUnlock( interactable );

            for (int i = 0; i < listeners.Count; i++) {
                listeners[i].OnUnequipped(inventory);
            }
        }
        public void OnEquippedUpdate(Inventory inventory) {
            for (int i = 0; i < listeners.Count; i++) {
                listeners[i].OnEquippedUpdate(inventory);
            }
        }


        public void OnEquippedUseStart (Inventory interactor, int useIndex) {
            for (int i = 0; i < listeners.Count; i++) {
                listeners[i].OnEquippedUseStart(interactor, useIndex);
            }
            // if (onUseStart != null) {
            //     onUseStart(interactor, useIndex);
            // }
        }
        public void OnEquippedUseEnd (Inventory inventory, int useIndex) {

            for (int i = 0; i < listeners.Count; i++) {
                listeners[i].OnEquippedUseEnd(inventory, useIndex);
            }
            // if (onUseEnd != null) {
            //     onUseEnd(interactor, useIndex);
            // }
        }
        public void OnEquippedUseUpdate(Inventory interactor, int useIndex) {
            for (int i = 0; i < listeners.Count; i++) {
                listeners[i].OnEquippedUseUpdate(interactor, useIndex);
            }
            // if (onUseUpdate != null) {
            //     onUseUpdate(interactor, useIndex);
            // }
        }

        

        // void OnDestroy()
        // {
        //     if (linkedInventory != null)
        //     {
        //         linkedInventory.UnequipItem(this, false);//, false);
        //     }
        // }
    }
}
