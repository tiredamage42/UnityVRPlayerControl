
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VRPlayer;


using Valve.VR;

using System;

using InteractionSystem;


namespace InventorySystem {


[RequireComponent(typeof(Inventory))]

public class InventoryEqupping : MonoBehaviour
{

    Inventory attachedInventory;
    void Awake () {
        attachedInventory = GetComponent<Inventory>();
    }

    void OnEnable () {
        attachedInventory.onDrop += OnDrop;
    }
    void OnDisable () {
        attachedInventory.onDrop -= OnDrop;
    }

    void OnDrop (Inventory inventory, ItemBehavior itemBehavior, int count, int newCount) {

        if (newCount == 0) {
            if (ItemIsEquipped(-1, itemBehavior)) {
                UnequipItem(itemBehavior, false);// getScene);
                // hasModel= true;
            }
        }
    }


    public const string equippedItemLayer = "EquippedItem";


    
    
    //maybe switch these to interaction system (stash and trade will be secondary/terciary action....)
    // when used BY inventory
    public const int GRAB_ACTION = 0;
    public const int STASH_ACTION = 1;
    
    //when used ON inventory
    public const int TRADE_ACTION = 2;


    public void OnUseStart (int equipSlot, int useIndex) {
        if (equipSlot < 0 || equipSlot >= equippedSlots.Length) {
            Debug.LogError("Equip slot " + equipSlot + " is out of range on inventory " + name);
            return;
        }
        
        if (equippedSlots[equipSlot] != null) {
            equippedSlots[equipSlot].sceneItem.OnEquippedUseStart(attachedInventory, useIndex);
        }
    }

    public void OnUseEnd (int equipSlot, int useIndex) {
        if (equipSlot < 0 || equipSlot >= equippedSlots.Length) {
            Debug.LogError("Equip slot " + equipSlot + " is out of range on inventory " + name);
            return;
        }
        
        EquipSlot slot = equippedSlots[equipSlot];
        if (slot != null) {
            bool isQuickEquipped = false;
            if (slot.isQuickEquipped) {

                if (useIndex == GRAB_ACTION) {
                    UnequipItem(equipSlot, true);
                    isQuickEquipped = true;
                }
            }
            if (!isQuickEquipped) {
                slot.sceneItem.OnEquippedUseEnd(attachedInventory, useIndex);
            }
        }

        }
        public void OnUseUpdate (int equipSlot, int useIndex) {

            if (equipSlot < 0 || equipSlot >= equippedSlots.Length) {
                Debug.LogError("Equip slot " + equipSlot + " is out of range on inventory " + name);
                return;
            }
        
            if (equippedSlots[equipSlot] != null) {
                equippedSlots[equipSlot].sceneItem.OnEquippedUseEnd(attachedInventory, useIndex);
            }
        }


    public class EquipInfo
    {
        public bool originallyActive;
        public CollisionDetectionMode collisionDetectionMode;
        public RigidbodyInterpolation rbInterpolation;
        public bool attachedRigidbodyWasKinematic;
        public bool attachedRigidbodyUsedGravity;
        public Transform originalParent;


        public EquipInfo( Item sceneItem ) {
            originallyActive = sceneItem.gameObject.activeSelf;
            originalParent = sceneItem.transform.parent;

            Rigidbody rb = sceneItem.rigidbody;
            if (rb != null) {

                attachedRigidbodyWasKinematic = rb.isKinematic;
                attachedRigidbodyUsedGravity = rb.useGravity;
                rbInterpolation = rb.interpolation;
                rb.interpolation = RigidbodyInterpolation.None;
                
                if (sceneItem.itemBehavior.equipType == EquipType.Normal) {
                    collisionDetectionMode = rb.collisionDetectionMode;
                    if (rb.collisionDetectionMode == CollisionDetectionMode.Continuous) {
                        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
                    }
                    rb.isKinematic = true;
                }
    
                if (sceneItem.itemBehavior.equipType == EquipType.Physics) {
                    rb.useGravity = false;
                }
            }

            if (!originallyActive) {
                sceneItem.gameObject.SetActive(true);
            }

        }

        public void ReturnItemToOriginalStateBeforeEquip (Item sceneItem) {
            sceneItem.transform.parent = originalParent;
            Rigidbody rb = sceneItem.rigidbody;
            if (rb != null)
            {
                rb.interpolation = rbInterpolation;

                if (sceneItem.itemBehavior.equipType == EquipType.Normal)
                {
                    rb.isKinematic = attachedRigidbodyWasKinematic;
                    rb.collisionDetectionMode = collisionDetectionMode;
                }
                if (sceneItem.itemBehavior.equipType == EquipType.Physics)
                {   
                    rb.useGravity = attachedRigidbodyUsedGravity;       
                }
            }
            sceneItem.gameObject.SetActive(originallyActive);
        }
    }
    

    public event System.Action<Inventory, Item, int, bool> onUnequip, onEquip, onEquipUpdate;
    
    
    public EquipPoint[] equipPoints = new EquipPoint[2];
    public void SetEquipPoint (int atIndex, EquipPoint equipPoint) {
        equipPoints[atIndex] = equipPoint;
    }


    void Update () {

        for (int i = 0 ; i < equippedSlots.Length; i++) {
            if (equippedSlots[i] != null) {

                equippedSlots[i].sceneItem.OnEquippedUpdate(attachedInventory);
                if (onEquipUpdate != null) {
                    onEquipUpdate(attachedInventory, equippedSlots[i].sceneItem, i, equippedSlots[i].isQuickEquipped);
                }
            }
        }
    }

    
public enum EquipType {
    Static, // item stays where it is, isnt parented or moved around to hand
    Normal, // item parented to hand
    Physics, // item follows hand wiht velocities
};



[System.Serializable] public class EquipSlot {

    public EquipSlot() {
        
    }
    [HideInInspector] public Item sceneItem;
    [HideInInspector] public EquipInfo equipInfo;
    [HideInInspector] public bool isQuickEquipped;
}


[HideInInspector] [System.NonSerialized] public EquipSlot[] equippedSlots = new EquipSlot[2];


public bool ItemIsEquipped(int slotIndex, ItemBehavior item) {
    if (slotIndex < 0) {
        for (int i =0 ; i < equippedSlots.Length; i++) {
            if (equippedSlots[i] != null && equippedSlots[i].sceneItem.itemBehavior == item) {        
                return true;
            }
        }
        return false;
    }
    else {
        return equippedSlots[slotIndex] != null && equippedSlots[slotIndex].sceneItem.itemBehavior == item;
    }
}

        public bool ItemIsEquipped (int slotIndex, Item item) {
            return ItemIsEquipped(slotIndex, item.itemBehavior);
        }

     

        EquipSlot BuildEquippedInventorySlot (Item sceneItem, bool quickEquip) {
            // we need to save the initial state of the scene item to restore it when it's dropped
            EquipInfo sceneItemEquipInfo = new EquipInfo( sceneItem );

            EquipSlot inventorySlot = new EquipSlot();//sceneItem.itemBehavior, sceneItem.itemCount);
            inventorySlot.sceneItem = sceneItem;
            inventorySlot.equipInfo = sceneItemEquipInfo;// quickEquipInfo;
            inventorySlot.isQuickEquipped = quickEquip;
            return inventorySlot;
        }

        // equip from item behavior
        public void EquipItem (ItemBehavior itemBehavior, int equipSlot, Item sceneItem) {

            // if (equipSlot == -1) {
            //     equipSlot = mainEquipPointIndex;
            // }

            if (equipSlot == -1) {
                Debug.LogError("problem with equi slot not set, cant equip " + itemBehavior.itemName);
                return;
            }
                
            EquipSlot equippedInventorySlot = null;
            int oldIndex = -1;
                
            // quick equipping
            if (sceneItem != null) {                
                if (sceneItem.linkedInventory != null && sceneItem.linkedInventory != this) {
                    Debug.LogError("Scene item :: " + sceneItem.name + " is already quick equipped to " + sceneItem.linkedInventory.name + " cant quick equip to "+ name);
                    return;
                }

                for (int i = 0; i < equippedSlots.Length; i++) {

                    if (equippedSlots[i] != null && equippedSlots[i].sceneItem == sceneItem) {
                        //already equipped scene item here
                        if (i == equipSlot) return;
                        
                        // item is already quick equipped at another slot
                        if (equippedSlots[i].isQuickEquipped) {
                            equippedInventorySlot = equippedSlots[i];
                            oldIndex = i;   
                        }
                        else {
                            // scene item is equipped as not quick equip, this shouldnt happen
                            Debug.LogError("Scene item :: " + sceneItem.name + " is already equipped to " + name + " normally, cant quick equip" );
                            return;
                        }
                    }
                }

                if (equippedInventorySlot == null) {
                    equippedInventorySlot = BuildEquippedInventorySlot (sceneItem, true);
                }
                else {
                    equippedSlots[oldIndex] = null;
                }

                equipPoints[equipSlot].GetComponent<InteractionPoint>().HoverLock(null);

            }
            else {
                //equipping , we need to get an available scene item
                for (int i = 0; i < equippedSlots.Length; i++) {
                    if (equippedSlots[i] != null && equippedSlots[i].sceneItem.itemBehavior == itemBehavior) {
                        
                        //already equipped item here
                        if (i == equipSlot) return;
                        
                        // item is already equipped at another slot
                        // unequip it there
                        if (!equippedSlots[i].isQuickEquipped){
                            equippedInventorySlot = equippedSlots[i];
                            oldIndex = i;    
                        }
                    }
                }

                if (equippedInventorySlot == null) {
                    equippedInventorySlot = BuildEquippedInventorySlot (Item.GetSceneItem(itemBehavior), false);
                }
                else {
                    equippedSlots[oldIndex] = null;
                }
            }


            //unequip our current equip slot
            if (equippedSlots[equipSlot] != null) {
                UnequipItem(equipSlot, equippedSlots[equipSlot].isQuickEquipped);
            }
            
            equippedSlots[equipSlot] = equippedInventorySlot;
            equippedSlots[equipSlot].sceneItem.linkedInventory = attachedInventory;
            equippedSlots[equipSlot].sceneItem.myEquipPoint = equipPoints[equipSlot];
            
            if (equippedSlots[equipSlot].sceneItem.itemBehavior.equipType != EquipType.Static) 
            {
                SnapItemToPosition ( equipSlot );
            }
            
            equippedSlots[equipSlot].sceneItem.OnEquipped (attachedInventory);
                
            if (onEquip != null) {
                onEquip(attachedInventory, equippedSlots[equipSlot].sceneItem, equipSlot, equippedSlots[equipSlot].isQuickEquipped);
            }
        }


        void SnapItemToPosition (int equipSlot) {
            Transform itemTransform = equippedSlots[equipSlot].sceneItem.transform;
            ItemBehavior item = equippedSlots[equipSlot].sceneItem.itemBehavior;
            Transform oldParent = itemTransform.parent;

            TransformBehavior.AdjustTransform(itemTransform, equipPoints[equipSlot].transform, item.equipTransform, equipSlot);
                
            if (item.equipType != EquipType.Normal) {
                itemTransform.parent = oldParent;
            }
        }
           
        public void UnequipItem(Item item, bool showScene) {
            for (int i =0 ; i < equippedSlots.Length; i++) {
                if (equippedSlots[i] != null) {
                    if (equippedSlots[i].sceneItem == item) {
                        UnequipItem(i, showScene);
                        return;
                    }
                }
            }
        }
        public void UnequipItem(ItemBehavior item, bool showScene) {
            for (int i =0 ; i < equippedSlots.Length; i++) {
                if (equippedSlots[i] != null) {
                    if (equippedSlots[i].sceneItem.itemBehavior == item) {
                        UnequipItem(i, showScene);
                        return;
                    }
                }
            }
        }

        public void UnequipItem(int slotIndex, bool showScene)
        {

            if (slotIndex < 0 || slotIndex >= equippedSlots.Length) {
                Debug.LogError("Equip slot " + slotIndex + " is out of range on inventory " + name);
                return;
            }
            if (equippedSlots[slotIndex] == null) {
                return;
            }
            
            EquipSlot slot = equippedSlots[slotIndex];
            equippedSlots[slotIndex] = null;
            equipPoints[slotIndex].GetComponent<InteractionPoint>().HoverUnlock(null);

            slot.sceneItem.linkedInventory = null;
            slot.sceneItem.myEquipPoint = null;
            
            slot.sceneItem.OnUnequipped (attachedInventory);
            if (onUnequip != null) {
                onUnequip(attachedInventory, slot.sceneItem, slotIndex, slot.isQuickEquipped);
            }

            slot.equipInfo.ReturnItemToOriginalStateBeforeEquip(slot.sceneItem);
            slot.sceneItem.gameObject.SetActive(showScene);
        }
    }
}

