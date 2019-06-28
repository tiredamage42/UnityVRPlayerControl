﻿using System.Collections;
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


buffs can be applied on stash (when ever player stashes item)
on equip (guns add stats, or armor)
on stash use


items have modifiers that modify owner values


Set | Add | Multiply

Base | Max     

Variable Name

Value

isOneOff  
    (modifier cant be removed, and is permanent 
        i.e level up adds 100 to max health, 
        or health pack adds health but then is let go (so cant remove modifier)
    )

gameMessage 




 */



 /*
 
    scene representation of the item
 
  */

    public class Item : MonoBehaviour, IInteractable
    {

        static Dictionary<int, HashSet<Item>> itemPoolsPerPrefab = new Dictionary<int, HashSet<Item>>();
        
        public static Item GetSceneItem (ItemBehavior itemBehavior){// prefab) {// Inventory quickEquipInventory){// bool dropOnUseEnd) {
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

            InitializeListeners();
        }

        public void OnInspectedStart(Interactor interactor) {}
        public void OnInspectedEnd(Interactor interactor) {}
        public void OnInspectedUpdate(Interactor interactor) {}
        public void OnUsedStart(Interactor interactor, int useIndex) {
            bool wasStashed = false;
            Inventory inventory = interactor.GetComponentInParent<Inventory>();

            if (itemBehavior.stashActions.Contains(useIndex)) {
                if (inventory.CanStashItem(this.itemBehavior)) {
                    inventory.StashItem(this);
                    wasStashed = true;
                }
            }
                    

            if (itemBehavior.equipActions.Contains(useIndex)) {
                
                // if we stashed it, equip normally, else just quick equip this one
                // equip slot negative one because slot is driven by inventory's
                // last set equip index

                
                // -1 if set by where equipped from
                // else equip point index (overwritten if quick equipped)
                int equipSlot = itemBehavior.equipSlot;
                
                //quick equipped in manually set slot
                if (!wasStashed) {
                    equipSlot = -1;
                }
                inventory.EquipItem(itemBehavior, equipSlot, wasStashed ? null : this );
            }

        }
        public void OnUsedEnd(Interactor interactor, int useIndex) {

        }
        public void OnUsedUpdate(Interactor interactor, int useIndex) {

        }

        [HideInInspector] public Inventory linkedInventory;
        [HideInInspector] public EquipPoint myEquipPoint;



        void SetItemColliderLayers () {
            Collider[] cols = GetComponentsInChildren<Collider>();
            for (int i = 0; i < cols.Length; i++) {
                colliderLayers.Add(new ColliderLayerPair(cols[i], Inventory.equippedItemLayer));
            }
        }
        void ResetItemColliderLayers () {
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
            // this.parentInventory = inventory;
            interactable.isAvailable = false;


            SetItemColliderLayers();
            

            // if (itemBehavior.hoverLockOnEquip)
            //             interactor.HoverLock( interactable );
            
            for (int i = 0; i < listeners.Count; i++) {
                listeners[i].OnEquipped(inventory);
            }
        }
        public void OnUnequipped (Inventory inventory) {
            // this.parentInventory = null;
            interactable.isAvailable = true;

            ResetItemColliderLayers();

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

        void OnDestroy()
        {
            if (linkedInventory != null)
            {
                linkedInventory.UnequipItem(this, false);//, false);
            }
        }
    }
}
