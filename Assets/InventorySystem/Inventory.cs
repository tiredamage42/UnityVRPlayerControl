using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VRPlayer;


using Valve.VR;

using System;

using InteractionSystem;


namespace InventorySystem {


/*


    recipes create items

    item getes stashed




    for workshop




        instantiate scen items for item returns on current recipe

            NOW HOVERLOCK to keep hovered over
            highlight on hover true
            keephighlighted


        workshop item script:
            bool isSelected = true; // wheneber instantiated tehy're the selected ones
            current recipe

            onuseDown () {

                if (selected) {
                    do whatever rigidibdy tstuff to place the object

                    
                    if (current recipe != null) {
                        current recipe create (
                            inventory, at position
                        ) ;

                        that gives items (
                            workshop stashed items should be consume on equip
                            then on consume gives permanent xp buff
                            also drop on consume
                         )


                    }


                        
                }
                else {

                }



            }

        

        prefab to instantiate

        OnStash (inventory, item, count) {
            //instantiate scene item

            //drop item

        }







 */


[RequireComponent(typeof(Interactable))]
public class Inventory : MonoBehaviour, IInteractable
{

    
    public bool allowQuickTrade;

    public void OnInspectedStart (Interactor interactor) {
        if (allowQuickTrade) {
            Inventory interactorInventory = interactor.GetComponent<Inventory>();
            if (interactorInventory != null) {
                interactorInventory.SuggestQuickTrade(this, interactor.interactorID);
            }
        }
    }

    public void OnInspectedEnd (Interactor interactor) {
        if (allowQuickTrade) {
            Inventory interactorInventory = interactor.GetComponent<Inventory>();
            if (interactorInventory != null) {
                interactorInventory.ForgetQuickTrade(this);
            }
        }
    }
    public void OnInspectedUpdate (Interactor interactor) { }

    public void OnUsedStart (Interactor interactor, int useAction) {
        if (useAction == TRADE_ACTION) {
            Debug.LogError("Trading with " + name);
            Inventory interactorInventory = interactor.GetComponent<Inventory>();
            if (interactorInventory != null) {
                interactorInventory.ForgetQuickTrade(this);
                interactorInventory.InitiateTrade(this);
            }
        }
    }
    public void OnUsedEnd (Interactor interactor, int useAction) { }
    public void OnUsedUpdate (Interactor interactor, int useAction) { }

    public event System.Action<Inventory, Inventory> onQuickTradeEnd, onTradeStart;
    public event System.Action<Inventory, Inventory, int> onQuickTradeStart;

    public void ForgetQuickTrade(Inventory withInventory) {
        if (onQuickTradeEnd != null) {
            onQuickTradeEnd(this, withInventory);
        }
    }
    public void SuggestQuickTrade(Inventory withInventory, int throughInteractor) {
        if (onQuickTradeStart != null) {
            onQuickTradeStart(this, withInventory, throughInteractor);
        }
    }
    public void InitiateTrade(Inventory withInventory) {
        if (onTradeStart != null) {
            onTradeStart(this, withInventory);
        }
    }

    public bool TransferInventoryContentsTo (Inventory otherInventory) {
        bool didAnyTransfer = false;
        for (int i = allInventory.Count -1 ; i >= 0; i--) {
            
            ItemBehavior item = allInventory[i].item;
            int count = allInventory[i].count;

            if (CanDropItem(item, out _, out _, false)) {
                if (otherInventory.CanStashItem(item)) {
                    DropItem(item, count, false, i);
                    otherInventory.StashItem(item, count);
                    didAnyTransfer = true;
                }
            }
        }
        return didAnyTransfer;

    }
    
    public bool TransferItemTo (ItemBehavior item, int count, Inventory otherInventory) {
        int itemIndex;
        if (CanDropItem(item, out itemIndex, out _, false)) {
            if (otherInventory.CanStashItem(item)) {
                DropItem(item, count, false, itemIndex);
                otherInventory.StashItem(item, count);
                return true;
            }
        }
        return false;
        
        

    }

    public bool CanStashItem (ItemBehavior item) {
        if (item.allowMultipleStashed) 
            return true;
        //check if it's already in inventory
        return !ItemIsInInventory(item, out _, out _);
    }
        
    public bool CanDropItem (ItemBehavior item, out int atIndex, out InventorySlot slot, bool checkInventory) {
        slot = null;
        atIndex = -1;
        if (item.permanentStash)
            return false;

        //check if it's already in inventory
        if (checkInventory)
            return ItemIsInInventory(item, out atIndex, out slot);
        
        return true;
    }

    bool ItemIsInInventory (ItemBehavior item, out int atIndex, out InventorySlot slot) {
        slot = null;
        atIndex = -1;
        for (int i = 0; i < allInventory.Count; i++) {
            if (allInventory[i].item == item) {
                atIndex = i;
                slot = allInventory[i];
                return true;
            }
        }
        return false;
    }



    // when used BY inventory
    public int GRAB_ACTION = 0;
    public int STASH_ACTION = 1;
    // public const int DROP_ACTION = 2;

    //when used ON inventory
    public int TRADE_ACTION = 2;


    int mainEquipPointIndex = -1;

    public void SetMainEquipPointIndex(int index) {
        mainEquipPointIndex = index;
    }
    public void SwitchMainUsedEquipPoint() {
        mainEquipPointIndex = 1-mainEquipPointIndex;
    }

    public void OnUseStart (int equipSlot, int useIndex) {
        if (equipSlot < 0 || equipSlot >= equippedSlots.Length) {
            Debug.LogError("Equip slot " + equipSlot + " is out of range on inventory " + name);
            return;
        }
        SetMainEquipPointIndex(equipSlot);
                

        if (equippedSlots[equipSlot] != null) {
            equippedSlots[equipSlot].sceneItem.OnEquippedUseStart(this, useIndex);
        }

        }
        public void OnUseEnd (int equipSlot, int useIndex) {
        if (equipSlot < 0 || equipSlot >= equippedSlots.Length) {
            Debug.LogError("Equip slot " + equipSlot + " is out of range on inventory " + name);
            return;
        }
        SetMainEquipPointIndex(equipSlot);
        
        InventorySlot slot = equippedSlots[equipSlot];
        if (slot != null) {
            bool isQuickEquipped = false;
            if (slot.isQuickEquipped) {
                if (slot.item.equipActions.Contains(useIndex)) {
                    UnequipItem(equipSlot, true);
                    isQuickEquipped = true;
                }
            }
            if (!isQuickEquipped) {
                slot.sceneItem.OnEquippedUseEnd(this, useIndex);
                // if (useIndex == DROP_ACTION) {
                    
                //     UnequipItem(equipSlot, true);
                //     DropItem(slot.item, 1, false);// true);
                // }
            }
        }

            // InventorySlot slot;
            // if (ThisSubInventoryHasCurrentEquipped(out slot)) {
            
            // // if (equippedItem != null) {
            //     // bool isUseable = hoveringInteractable.useType != Interactable.UseType.Scripted;
            //     // if (isUseable) {
                
            //     equippedItem.item.OnEquippedUseEnd(this, useIndex);
            //     // }
            // }
            // if (onUseEnd != null) {
            //     onUseEnd (this, useIndex, hoveringInteractable);
            // }
        }
        public void OnUseUpdate (int equipSlot, int useIndex) {

            if (equipSlot < 0 || equipSlot >= equippedSlots.Length) {
                Debug.LogError("Equip slot " + equipSlot + " is out of range on inventory " + name);
                return;
            }
            SetMainEquipPointIndex(equipSlot);
        

        if (equippedSlots[equipSlot] != null) {
            equippedSlots[equipSlot].sceneItem.OnEquippedUseEnd(this, useIndex);
        }



            // InventorySlot slot;
            // if (ThisSubInventoryHasCurrentEquipped(out slot)) {
            
            // // if (equippedItem != null) {
            //     // bool isUseable = hoveringInteractable.useType != Interactable.UseType.Scripted;
            //     // if (isUseable) {
                
            //     equippedItem.item.OnEquippedUseUpdate(this, useIndex);
            //     // }
            // }
            // if (onUseUpdate != null) {
            //     onUseUpdate (this, useIndex, hoveringInteractable);
            // }
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
    public event System.Action<Inventory, ItemBehavior, int> onStash, onDrop;

    
    public EquipPoint[] equipPoints = new EquipPoint[2];
    public void SetEquipPoint (int atIndex, EquipPoint equipPoint) {
        equipPoints[atIndex] = equipPoint;
    }


    void Update () {

        for (int i = 0 ; i < equippedSlots.Length; i++) {
            if (equippedSlots[i] != null) {

                equippedSlots[i].sceneItem.OnEquippedUpdate(this);
                if (onEquipUpdate != null) {
                    onEquipUpdate(this, equippedSlots[i].sceneItem, i, equippedSlots[i].isQuickEquipped);
                }
            }
        }
    }

    
public enum EquipType {
    Static, // item stays where it is, isnt parented or moved around to hand
    Normal, // item parented to hand
    Physics, // item follows hand wiht velocities
};



[System.Serializable] public class InventorySlot {

    public InventorySlot(ItemBehavior item, int count) {
        this.item = item;
        this.count = count;
    }
    public ItemBehavior item;
    public int count;
    [HideInInspector] public Item sceneItem;
    [HideInInspector] public EquipInfo equipInfo;
    [HideInInspector] public bool isQuickEquipped;

    
}


// public Inventory baseInventory;

// public int equipSlotOnBase;
// public bool isSubInventory {
//     get {
//         return baseInventory != null;
//     }
// }


[HideInInspector] [System.NonSerialized] public InventorySlot[] equippedSlots = new InventorySlot[2];
public int favoritesCount = 8;
public List<InventorySlot> allInventory = new List<InventorySlot>();


// bool ItemWithinInventoryRange (ItemBehavior item, int range) {
//     // Inventory inv = isSubInventory ? baseInventory : this;
//     // int count = range <= 0 ? inv.allInventory.Count : range;
//     int count = range <= 0 ? allInventory.Count : range;

//     for (int i =0 ; i < count; i++) {
//         // if (inv.allInventory[i].item == item) {
//         if (allInventory[i].item == item) {
        
//             return true;
//         }
//     }
//     return false;
// }

// public bool ItemIsStashed (ItemBehavior item) {
//     return ItemWithinInventoryRange(item, -1);
// }
// // public bool ItemIsStashed (Item item) {
// //     return ItemIsStashed(item.itemBehavior);
// // }
// // public bool ItemIsFavorited (ItemBehavior item) {
// //     // return ItemWithinInventoryRange(item, isSubInventory ? baseInventory.favoritesCount : favoritesCount);
// //     return ItemWithinInventoryRange(item, favoritesCount);

// // }
// // public bool ItemIsFavorited (Item item) {
// //     return ItemIsFavorited(item.itemBehavior);
// // }




// bool ThisSubInventoryHasCurrentEquipped (out InventorySlot slot) {
//     return EquipSlotIsOccupied(equipSlotOnBase, out slot);
// }

// public bool EquipSlotIsOccupied (int slotIndex, out InventorySlot slot) {
//     // Inventory inv = isSubInventory ? baseInventory : this;
//     slot = inv.equippedSlots[slotIndex];
//     return slot != null;
// }
public bool ItemIsEquipped(int slotIndex, ItemBehavior item) {
    // Inventory inv = isSubInventory ? baseInventory : this;
    if (slotIndex == -1) {
        // for (int i =0 ; i < inv.equippedSlots.Length; i++) {
        for (int i =0 ; i < equippedSlots.Length; i++) {
        
            // if (inv.equippedSlots[slotIndex] != null && inv.equippedSlots[slotIndex].item == item) {
            if (equippedSlots[i] != null && equippedSlots[i].item == item) {
            
                return true;
            }
        }
        return false;
    }
    else {
        // return inv.equippedSlots[slotIndex] != null && inv.equippedSlots[slotIndex].item == item;
        return equippedSlots[slotIndex] != null && equippedSlots[slotIndex].item == item;

    }
}
// public bool ItemIsEquipped (ItemBehavior item) {
//     int slotIndex = isSubInventory ? equipSlotOnBase : -1;
//     return ItemIsEquipped(slotIndex, item); 
// }

public bool ItemIsEquipped (int slotIndex, Item item) {
    return ItemIsEquipped(slotIndex, item.itemBehavior);
}
// public bool ItemIsEquipped (Item item) {
//     return ItemIsEquipped(item.itemBehavior);
// }

        public Vector3 dropLocalPoint = new Vector3 (0,1,1);


        public void StashItem(ItemBehavior itemBehavior, int count) {
            //check if it's already in inventory
            InventorySlot slotInInventory = null;

            if (itemBehavior.keepOnStash) {
                for (int i = 0; i < allInventory.Count; i++) {
                    if (allInventory[i].item == itemBehavior) {
                        slotInInventory = allInventory[i];
                        break;
                    }
                }
            }
            bool wasInInventory = slotInInventory != null;

            if (wasInInventory) {
                if (!itemBehavior.allowMultipleStashed) {
                    return;// false;
                }
                slotInInventory.count += count;
            }
            else {

                slotInInventory = new InventorySlot(itemBehavior, count);
                slotInInventory.sceneItem = null;
                slotInInventory.equipInfo = null;

                if (itemBehavior.keepOnStash) {
                    allInventory.Add(slotInInventory);
                }
            }

            // add buffs
            if (itemBehavior.stashedItemBehavior != null) {
            // if (itemBehavior.onStashBuffs != null) {

                itemBehavior.stashedItemBehavior.OnItemStashed(this, itemBehavior, count);
                
                // itemBehavior.onStashBuffs.AddBuffsToActor(actor, count);
            }

            if (onStash != null) {
                onStash(this, itemBehavior, count);
            }

            // return true;
        }

        public void StashItem(Item item)
        {
            ItemBehavior itemBehavior = item.itemBehavior;

            if (CanStashItem(itemBehavior)) {

            
            // if (StashItem(itemBehavior, itemBehavior.stackable ? item.itemCount : 1)) {
                StashItem(itemBehavior, itemBehavior.stackable ? item.itemCount : 1);

                //disable the scene item
                //frees it up for pool
                item.gameObject.SetActive(false);
                // return true;
            }
            // return false;
        }


        public void ClearInventory () {
            for (int i = allInventory.Count -1; i >= 0; i--) {
                DropItem(allInventory[i].item, allInventory[i].count, false, i);
            }
        }
        public void AddInventory (List<InventorySlot> slots) {
            for (int i =0 ; i < slots.Count; i++) {
                StashItem(slots[i].item, slots[i].count);
            }
        }

        public void DropItem (ItemBehavior itemBehavior, int count, bool getScene, int inventoryIndex) {
            if (itemBehavior.permanentStash) {
                return;// false;
            }

            if (inventoryIndex < 0) {
                //check if it's already in inventory
                for (int i = 0; i < allInventory.Count; i++) {
                    if (allInventory[i].item == itemBehavior) {
                        inventoryIndex = i;
                        // slotInInventory = allInventory[i];
                        break;
                    }
                }
            }
            
            InventorySlot slotInInventory = null;
            if (inventoryIndex >= 0) {
                slotInInventory = allInventory[inventoryIndex];
            }

            bool wasInInventory = slotInInventory != null;



            if (wasInInventory) {

                count = Mathf.Min(count, slotInInventory.count);

                slotInInventory.count -= count;
                if (slotInInventory.count <= 0) {
                    slotInInventory.count = 0;
                }
                
                // remove buffs
                if (itemBehavior.stashedItemBehavior != null) {

                    itemBehavior.stashedItemBehavior.OnItemDropped(this, itemBehavior, count);
                    
                    // itemBehavior.onStashBuffs.RemoveBuffsFromActor(actor, count);
                
                }


                if (slotInInventory.count == 0) {
                    allInventory.Remove(slotInInventory);
                }

                if (onDrop != null) {
                    onDrop(this, itemBehavior, count);
                }

                if (getScene) {

                    Item sceneItem = Item.GetSceneItem(itemBehavior);
                    sceneItem.transform.position = transform.TransformPoint(dropLocalPoint);
                    sceneItem.transform.rotation = Quaternion.Euler(UnityEngine.Random.Range(0,360), UnityEngine.Random.Range(0,360), UnityEngine.Random.Range(0,360));
                    sceneItem.gameObject.SetActive(true);
                }
                // return true;
            }
            // return false;
        }

     

        InventorySlot BuildEquippedInventorySlot (Item sceneItem, bool quickEquip) {
            // we need to save the initial state of the scene item to restore it when it's dropped
            EquipInfo sceneItemEquipInfo = new EquipInfo( sceneItem );

            InventorySlot inventorySlot = new InventorySlot(sceneItem.itemBehavior, sceneItem.itemCount);
            inventorySlot.sceneItem = sceneItem;
            inventorySlot.equipInfo = sceneItemEquipInfo;// quickEquipInfo;
            inventorySlot.isQuickEquipped = quickEquip;
            
            return inventorySlot;
        }

        // equip from item behavior
        public void EquipItem (ItemBehavior itemBehavior, int equipSlot, Item sceneItem) {

            if (equipSlot == -1) {
                equipSlot = mainEquipPointIndex;
            }

            if (equipSlot == -1) {
                Debug.LogError("problem with equi slot not set");
            }
                
            InventorySlot equippedInventorySlot = null;
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

                equipPoints[equipSlot].GetComponent<Interactor>().HoverLock(null);

            }
            else {
                //equipping , we need to get an available scene item
                for (int i = 0; i < equippedSlots.Length; i++) {
                    if (equippedSlots[i] != null && equippedSlots[i].item == itemBehavior) {
                        
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
            equippedSlots[equipSlot].sceneItem.linkedInventory = this;
            equippedSlots[equipSlot].sceneItem.myEquipPoint = equipPoints[equipSlot];
            
            if (equippedSlots[equipSlot].item.equipType != EquipType.Static) 
            {
                SnapItemToPosition ( equipSlot );
            }
            
            equippedSlots[equipSlot].sceneItem.OnEquipped (this);
                
            if (onEquip != null) {
                onEquip(this, equippedSlots[equipSlot].sceneItem, equipSlot, equippedSlots[equipSlot].isQuickEquipped);
            }

            // equip buffs
            // if (itemBehavior.onEquipBuffs != null) {

            //     // itemBehavior.onEquipBuffs.AddBuffsToActor(actor, 1);
            
            // }





            

        }


        void SnapItemToPosition (int equipSlot) {

            Transform itemTransform = equippedSlots[equipSlot].sceneItem.transform;
            ItemBehavior item = equippedSlots[equipSlot].item;

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

        public void UnequipItem(int slotIndex, bool showScene)
        {

            if (slotIndex < 0 || slotIndex >= equippedSlots.Length) {
                Debug.LogError("Equip slot " + slotIndex + " is out of range on inventory " + name);
                return;
            }
            if (equippedSlots[slotIndex] == null) {
                return;
            }
            
            InventorySlot slot = equippedSlots[slotIndex];
            equippedSlots[slotIndex] = null;
            equipPoints[slotIndex].GetComponent<Interactor>().HoverUnlock(null);

            
    
            
                
            slot.sceneItem.linkedInventory = null;
            slot.sceneItem.myEquipPoint = null;
            
            slot.sceneItem.OnUnequipped (this);
            if (onUnequip != null) {
                onUnequip(this, slot.sceneItem, slotIndex, slot.isQuickEquipped);
            }

            slot.equipInfo.ReturnItemToOriginalStateBeforeEquip(slot.sceneItem);
            slot.sceneItem.gameObject.SetActive(showScene);


            // equip buffs
            // if (slot.item.onEquipBuffs != null) {
            //     // itemBehavior.onEquipBuffs.RemoveBuffsFromActor(actor, 1);
            // }
        }
    }
}
