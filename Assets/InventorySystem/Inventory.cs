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
    public const string equippedItemLayer = "EquippedItem";






    // // Create a layer at the next available index. Returns silently if layer already exists.
    // public static void CreateLayer(string name)
    // {
    //     if (string.IsNullOrEmpty(name))
    //         throw new System.ArgumentNullException("name", "New layer name string is either null or empty.");

    //     var tagManager = new UnityEditor.SerializedObject(UnityEditor.AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
    //     var layerProps = tagManager.FindProperty("layers");
    //     var propCount = layerProps.arraySize;

    //     UnityEditor.SerializedProperty firstEmptyProp = null;

    //     for (var i = 0; i < propCount; i++)
    //     {
    //         var layerProp = layerProps.GetArrayElementAtIndex(i);
    //         var stringValue = layerProp.stringValue;

    //         if (stringValue == name) return;

    //         if (i < 8 || stringValue != string.Empty) 
    //             continue;

    //         if (firstEmptyProp == null) {
    //             firstEmptyProp = layerProp;
    //             break;
    //         }
    //     }

    //     if (firstEmptyProp == null)
    //     {
    //         Debug.LogError("Maximum limit of " + propCount + " layers exceeded. Layer \"" + name + "\" not created.");
    //         return;
    //     }

    //     firstEmptyProp.stringValue = name;
    //     tagManager.ApplyModifiedProperties();
    // }














































    public bool allowQuickTrade;

    public void OnInspectedStart (Interactor interactor) {
        if (allowQuickTrade) {
            Inventory interactorInventory = interactor.GetComponentInParent<Inventory>();
            if (interactorInventory != null) {
                interactorInventory.SuggestQuickTrade(this, interactor.interactorID);
            }
        }
    }

    public void OnInspectedEnd (Interactor interactor) {
        if (allowQuickTrade) {
            Inventory interactorInventory = interactor.GetComponentInParent<Inventory>();
            if (interactorInventory != null) {
                interactorInventory.ForgetQuickTrade(this, interactor.interactorID);
            }
        }
    }
    public void OnInspectedUpdate (Interactor interactor) { }

    public void OnUsedStart (Interactor interactor, int useAction) {
        if (useAction == TRADE_ACTION) {
            Inventory interactorInventory = interactor.GetComponentInParent<Inventory>();
            if (interactorInventory != null) {
                interactorInventory.ForgetQuickTrade(this, interactor.interactorID);
                interactorInventory.InitiateTrade(this, interactor.interactorID);
            }
        }
    }
    public void OnUsedEnd (Interactor interactor, int useAction) { }
    public void OnUsedUpdate (Interactor interactor, int useAction) { }

    public event System.Action<Inventory, Inventory, int> onQuickTradeEnd, onTradeStart;
    public event System.Action<Inventory, Inventory, int> onQuickTradeStart;

    public void ForgetQuickTrade(Inventory withInventory, int throughInteractor) {
        if (onQuickTradeEnd != null) {
            // Debug.LogError("forget quick trade");
            onQuickTradeEnd(this, withInventory, throughInteractor);
        }
    }
    public void SuggestQuickTrade(Inventory withInventory, int throughInteractor) {
        Debug.LogError("suggesting quick trade");
        if (onQuickTradeStart != null) {

            onQuickTradeStart(this, withInventory, throughInteractor);
        }
    }
    public void InitiateTrade(Inventory withInventory, int throughInteractor) {
        if (onTradeStart != null) {
            onTradeStart(this, withInventory, throughInteractor);
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
    
    //when used ON inventory
    public int TRADE_ACTION = 2;


    public void OnUseStart (int equipSlot, int useIndex) {
        if (equipSlot < 0 || equipSlot >= equippedSlots.Length) {
            Debug.LogError("Equip slot " + equipSlot + " is out of range on inventory " + name);
            return;
        }
        // SetMainEquipPointIndex(equipSlot);
                

        if (equippedSlots[equipSlot] != null) {
            equippedSlots[equipSlot].sceneItem.OnEquippedUseStart(this, useIndex);
        }

    }
    public void OnUseEnd (int equipSlot, int useIndex) {
        if (equipSlot < 0 || equipSlot >= equippedSlots.Length) {
            Debug.LogError("Equip slot " + equipSlot + " is out of range on inventory " + name);
            return;
        }
        // SetMainEquipPointIndex(equipSlot);
        
        InventorySlot slot = equippedSlots[equipSlot];
        if (slot != null) {
            bool isQuickEquipped = false;
            if (slot.isQuickEquipped) {
                if (slot.item.equipActions.Contains(useIndex)) {
                    Debug.LogError("unequipping quick");
                    UnequipItem(equipSlot, true);
                    isQuickEquipped = true;
                }
            }
            if (!isQuickEquipped) {
                slot.sceneItem.OnEquippedUseEnd(this, useIndex);
            }
        }

            // if (onUseEnd != null) {
            //     onUseEnd (this, useIndex, hoveringInteractable);
            // }
        }
        public void OnUseUpdate (int equipSlot, int useIndex) {

            if (equipSlot < 0 || equipSlot >= equippedSlots.Length) {
                Debug.LogError("Equip slot " + equipSlot + " is out of range on inventory " + name);
                return;
            }
            // SetMainEquipPointIndex(equipSlot);
        

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



[HideInInspector] [System.NonSerialized] public InventorySlot[] equippedSlots = new InventorySlot[2];
public int favoritesCount = 8;
public List<InventorySlot> allInventory = new List<InventorySlot>();

public bool ItemIsEquipped(int slotIndex, ItemBehavior item) {
    if (slotIndex < 0) {
        for (int i =0 ; i < equippedSlots.Length; i++) {
            if (equippedSlots[i] != null && equippedSlots[i].item == item) {        
                return true;
            }
        }
        return false;
    }
    else {
        return equippedSlots[slotIndex] != null && equippedSlots[slotIndex].item == item;

    }
}

public bool ItemIsEquipped (int slotIndex, Item item) {
    return ItemIsEquipped(slotIndex, item.itemBehavior);
}

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
                
                if (itemBehavior.keepOnStash) {
                    allInventory.Add(slotInInventory);
                }
            }

            if (itemBehavior.stashedItemBehavior != null) {
                itemBehavior.stashedItemBehavior.OnItemStashed(this, itemBehavior, count);
            }

            if (onStash != null) {
                onStash(this, itemBehavior, count);
            }
        }

        public void StashItem(Item item)
        {
            ItemBehavior itemBehavior = item.itemBehavior;

            if (CanStashItem(itemBehavior)) {

                StashItem(itemBehavior, itemBehavior.stackable ? item.itemCount : 1);

                //disable the scene item (frees it up for pool)
                item.gameObject.SetActive(false);
            }
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
                bool hasModel = false;
                if (ItemIsEquipped(-1, itemBehavior)) {
                    UnequipItem(itemBehavior, getScene);
                    hasModel= true;
                }

                count = Mathf.Min(count, slotInInventory.count);

                slotInInventory.count -= count;
                if (slotInInventory.count <= 0) {
                    slotInInventory.count = 0;
                }
                
                // remove buffs
                if (itemBehavior.stashedItemBehavior != null) {
                    itemBehavior.stashedItemBehavior.OnItemDropped(this, itemBehavior, count);   
                }


                if (slotInInventory.count == 0) {
                    allInventory.Remove(slotInInventory);
                }

                if (onDrop != null) {
                    onDrop(this, itemBehavior, count);
                }


                if (!hasModel) {

                    if (getScene) {

                        Item sceneItem = Item.GetSceneItem(itemBehavior);
                        sceneItem.transform.position = transform.TransformPoint(dropLocalPoint);
                        sceneItem.transform.rotation = Quaternion.Euler(UnityEngine.Random.Range(0,360), UnityEngine.Random.Range(0,360), UnityEngine.Random.Range(0,360));
                        sceneItem.gameObject.SetActive(true);
                    }
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

            // if (equipSlot == -1) {
            //     equipSlot = mainEquipPointIndex;
            // }

            if (equipSlot == -1) {
                Debug.LogError("problem with equi slot not set, cant equip " + itemBehavior.itemName);
                return;
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
        public void UnequipItem(ItemBehavior item, bool showScene) {
            for (int i =0 ; i < equippedSlots.Length; i++) {
                if (equippedSlots[i] != null) {
                    if (equippedSlots[i].item == item) {
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
        }
    }
}
