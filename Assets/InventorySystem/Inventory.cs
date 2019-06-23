using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VRPlayer;


using Valve.VR;

using System;

using InteractionSystem;


namespace InventorySystem {

public class Inventory : MonoBehaviour
{


    public const int UI_USE_ACTION = 0;
    public const int UI_TRADE_ACTION = 1;
    public const int UI_DROP_ACTION = 2;
    public const int UI_FAVORITE_ACTION = 3;

    public const int GRAB_ACTION = 0;
    public const int STASH_ACTION = 1;
    public const int DROP_ACTION = 2;

    



    // [Flags]
    //     public enum AttachmentFlags
    //     {
    //         SnapOnAttach = 1 << 0, // The object should snap to the position of the specified attachment point on the hand.
    //         DetachOthers = 1 << 1, // Other objects attached to this hand will be detached.
    //         DetachFromOtherHand = 1 << 2, // This object will be detached from the other hand.
    //         ParentToHand = 1 << 3, // The object will be parented to the hand.
    //         VelocityMovement = 1 << 4, // The object will attempt to move to match the position and rotation of the hand.
    //         TurnOnKinematic = 1 << 5, // The object will not respond to external physics.
    //         TurnOffGravity = 1 << 6, // The object will not respond to external physics.
    //         // AllowSidegrade = 1 << 7, // The object is able to switch from a pinch grab to a grip grab. Decreases likelyhood of a good throw but also decreases likelyhood of accidental drop
    //     };



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


            // InventorySlot slot;
            // if (ThisSubInventoryHasCurrentEquipped(out slot)) {
            // // if (equippedItem != null) {
            //     // bool isUseable = hoveringInteractable.useType != Interactable.UseType.Scripted;
            //     // if (isUseable) {
            //         equippedItem.item.OnEquippedUseStart(this, useIndex);
            //     // }
            // }
            // if (onUseStart != null) {
            //     onUseStart (this, useIndex, hoveringInteractable);
            // }
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
                    UnequipItem(equipSlot);
                    isQuickEquipped = true;
                }
            }
            if (!isQuickEquipped) {

                slot.sceneItem.OnEquippedUseEnd(this, useIndex);
                
                if (useIndex == DROP_ACTION) {
                    
                    UnequipItem(equipSlot);
                    DropItem(slot.item, 1, false);// true);
                }

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



    // public Inventory otherInventory;
    public class EquipInfo
    {
        public bool originallyActive;
        // public Item item;
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

        // public bool isParentedToInventory;
        // public AttachmentFlags attachFlags;
        // public EquipType equipType;
        
        // public bool HasAttachFlag(AttachmentFlags flag)
        // {
        //     return (attachFlags & flag) == flag;
        // }
    }
    

    public event System.Action<Inventory, Item, int, bool> onUnequip, onEquip, onEquipUpdate;
    public event System.Action<Inventory, ItemBehavior, int> onStash, onDrop;

    // public Transform alternateEquipPoint;

    // Transform equipPoint { 
    //     get {
    //         return alternateEquipPoint != null ? alternateEquipPoint : transform;
    //     }
    // }
    
    public EquipPoint[] equipPoints = new EquipPoint[2];
    public void SetEquipPoint (int atIndex, EquipPoint equipPoint) {
        equipPoints[atIndex] = equipPoint;
    }



    // public EquippedItem equippedItem;

    // protected const float MaxVelocityChange = 10f;
    //     protected const float VelocityMagic = 6000f;
    //     protected const float AngularVelocityMagic = 50f;
    //     protected const float MaxAngularVelocityChange = 20f;



    void Update () {


        for (int i = 0 ; i < equippedSlots.Length; i++) {
            if (equippedSlots[i] != null) {

                equippedSlots[i].sceneItem.OnEquippedUpdate(this);
                if (onEquipUpdate != null) {
                    onEquipUpdate(this, equippedSlots[i].sceneItem, i, equippedSlots[i].isQuickEquipped);
                }
            }
        }

        // InventorySlot slot;
        // if (ThisSubInventoryHasCurrentEquipped(out slot)) 
        // // if (equippedItem != null)
        // {
        //     slot.sceneReference.item.OnEquippedUpdate(this);
        //     if (onEquipUpdate != null) {
        //         onEquipUpdate(this, slot.sceneReference.item);
        //     }
        // }
    }

    
    

    // void GetLocalEquippedPositionTargets (ItemBehavior item, int slot, out Vector3 localPosition, out Quaternion localRotation) {
    //     TransformBehavior.GetValues(item.equipTransform, 0, out localPosition, out localRotation);
    // }


    // void GetLocalEquippedPositionTargets (ItemBehavior item, out Vector3 localPosition, out Quaternion localRotation) {
    //     TransformBehavior.GetValues(item.equipTransform, 0, out localPosition, out localRotation);
    //     // localPosition = Vector3.zero;
    //     // localRotation = Quaternion.identity;

    //     // if (item.equipBehavior != null && item.equipBehavior.transformSettings.Length > 0) {
    //     //     localPosition = item.equipBehavior.transformSettings[0].position;
    //     //     localRotation = Quaternion.Euler(item.equipBehavior.transformSettings[0].rotation);
    //     // }
    // }

    // public bool GetUpdatedEquippedVelocities(out Vector3 velocityTarget, out Vector3 angularTarget)
    //     {
    //         bool realNumbers = false;


    //         float velocityMagic = VelocityMagic;
    //         float angularVelocityMagic = AngularVelocityMagic;



    //         Vector3 localPosition;
    //         Quaternion localRotation;
    //         GetLocalEquippedPositionTargets (equippedItem.item, out localPosition, out localRotation);


    //         Vector3 targetItemPosition = equipPoint.TransformPoint(localPosition);//equippedItem.targetLocalPos);
            
    //         // Vector3 targetItemPosition = TargetEquippedItemWorldPosition();//equippedItem);




    //         Vector3 positionDelta = (targetItemPosition - equippedItem.item.rigidbody.position);
    //         velocityTarget = (positionDelta * velocityMagic * Time.deltaTime);

    //         if (float.IsNaN(velocityTarget.x) == false && float.IsInfinity(velocityTarget.x) == false)
    //         {
    //             realNumbers = true;
    //         }
    //         else
    //             velocityTarget = Vector3.zero;






    //         Quaternion targetItemRotation = equipPoint.rotation * localRotation;
    //         // Quaternion targetItemRotation = TargetItemRotation();//equippedItem);
            
    //         Quaternion rotationDelta = targetItemRotation * Quaternion.Inverse(equippedItem.item.transform.rotation);


    //         float angle;
    //         Vector3 axis;
    //         rotationDelta.ToAngleAxis(out angle, out axis);

    //         if (angle > 180)
    //             angle -= 360;

    //         if (angle != 0 && float.IsNaN(axis.x) == false && float.IsInfinity(axis.x) == false)
    //         {
    //             angularTarget = angle * axis * angularVelocityMagic * Time.deltaTime;

    //             realNumbers &= true;
    //         }
    //         else
    //             angularTarget = Vector3.zero;

    //         return realNumbers;
    //     }







// const AttachmentFlags defaultAttachmentFlags = AttachmentFlags.ParentToHand |
//                                                               AttachmentFlags.DetachFromOtherHand |
//                                                               AttachmentFlags.TurnOnKinematic |
//                                                               AttachmentFlags.SnapOnAttach;


// const AttachmentFlags physicsEquipFlags = AttachmentFlags.VelocityMovement | AttachmentFlags.TurnOffGravity | AttachmentFlags.SnapOnAttach;
// const AttachmentFlags normalEquipFlags = AttachmentFlags.ParentToHand | AttachmentFlags.TurnOnKinematic | AttachmentFlags.SnapOnAttach;
// const AttachmentFlags staticEquipFlags = 0;

// AttachmentFlags GetFlags (EquipType equipType) {
//     switch (equipType) {
//         case EquipType.Static:
//             return staticEquipFlags;
//         case EquipType.Normal:
//             return normalEquipFlags;
//         case EquipType.Physics:
//             return physicsEquipFlags;
//     }
//     return 0;
// }

public enum EquipType {
    Static, // item stays where it is, isnt parented or moved around to hand
    Normal, // item parented to hand
    Physics, // item follows hand wiht velocities
};



[System.Serializable] public class InventorySlot {
    public ItemBehavior item;
    public int count;
    // public Item sceneReference;
    public Item sceneItem;
    public EquipInfo equipInfo;
    public bool isQuickEquipped;
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
            if (equippedSlots[slotIndex] != null && equippedSlots[slotIndex].item == item) {
            
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












        public bool StashItem(ItemBehavior itemBehavior, int count) {
            //check if it's already in inventory
            InventorySlot slotInInventory = null;


            for (int i = 0; i < allInventory.Count; i++) {
                if (allInventory[i].item == itemBehavior) {
                    slotInInventory = allInventory[i];
                    break;
                }
            }
            bool wasInInventory = slotInInventory != null;

            if (wasInInventory) {
                if (!itemBehavior.allowMultipleStashed) {
                    return false;
                }
                slotInInventory.count += count;
            }
            else {

                slotInInventory = new InventorySlot();
                slotInInventory.count = count;
                slotInInventory.item = itemBehavior;
                slotInInventory.sceneItem = null;
                slotInInventory.equipInfo = null;
                allInventory.Add(slotInInventory);
            }

            // add buffs
            if (itemBehavior.onStashBuffs != null) {

                // itemBehavior.onStashBuffs.AddBuffsToActor(actor, count);
            
            }

            if (onStash != null) {
                onStash(this, itemBehavior, count);
            }

            return true;
        }

        public bool StashItem(Item item)
        {
            ItemBehavior itemBehavior = item.itemBehavior;

            if (StashItem(itemBehavior, itemBehavior.stackable ? item.itemCount : 1)) {

                //disable the scene item
                //frees it up for pool
                item.gameObject.SetActive(false);
                return true;
            }
            return false;
        }

        public void DropItem (ItemBehavior itemBehavior, int count, bool getScene) {
            if (itemBehavior.permanentStash) {
                return;
            }

            //check if it's already in inventory
            InventorySlot slotInInventory = null;
            for (int i = 0; i < allInventory.Count; i++) {
                if (allInventory[i].item == itemBehavior) {
                    slotInInventory = allInventory[i];
                    break;
                }
            }
            bool wasInInventory = slotInInventory != null;



            if (wasInInventory) {
                count = Mathf.Min(count, slotInInventory.count);

                slotInInventory.count -= count;
                if (slotInInventory.count <= 0) {
                    slotInInventory.count = 0;
                }
                
                // remove buffs
                if (itemBehavior.onStashBuffs != null) {

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
            }
        }

        // public QuickEquipInfo GetEquipInfo (Item sceneItem) {
        //     for (int i =0 ; i < equippedSlots.Length; i++) {
        //         if (equippedSlots[i] != null && equippedSlots[i].sceneItem == sceneItem) {
        //             return equippedSlots[i].quickEquip;
        //         }
        //     }
        //     return null;
        // }

        InventorySlot BuildEquippedInventorySlot (Item sceneItem, bool quickEquip) {
            // we need to save the initial state of the scene item to restore it when it's dropped
            EquipInfo sceneItemEquipInfo = new EquipInfo( sceneItem );

            InventorySlot inventorySlot = new InventorySlot();
            inventorySlot.count = sceneItem.itemCount;
            inventorySlot.item = sceneItem.itemBehavior;
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
                UnequipItem(equipSlot);
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
            if (itemBehavior.onEquipBuffs != null) {

                // itemBehavior.onEquipBuffs.AddBuffsToActor(actor, 1);
            
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
           














        // public void EquipItem(Item item)
        // {

        //     if(ItemIsEquipped(item))
        //         return;

        //     Rigidbody itemRB = item.rigidbody;

        //     Vector3 originalItemPosition = item.transform.position;
        //     Quaternion originalItemRotation = item.transform.rotation;
            
        //     EquippedItem attachedObject = new EquippedItem();
        //     // attachedObject.attachFlags = GetFlags(item.equipType);// flags;
        //     attachedObject.equipType = item.equipType;

        //     attachedObject.item = item;
        //     // attachedObject.attachTime = Time.time;

        //     attachedObject.originalParent = item.transform.parent;

        //     // attachedObject.attachedRigidbody = itemRB;
            
        //     if (itemRB != null)
        //     {
        //         if (item.parentInventory != null) //already attached to another hand
        //         {
        //             //if it was attached to another hand, get the flags from that hand
                    
        //             EquippedItem attachedObjectInList = item.parentInventory.equippedItem;
        //             if (attachedObjectInList.item == attachedObject.item)
        //             {
        //                 attachedObject.attachedRigidbodyWasKinematic = attachedObjectInList.attachedRigidbodyWasKinematic;
        //                 attachedObject.attachedRigidbodyUsedGravity = attachedObjectInList.attachedRigidbodyUsedGravity;
        //                 attachedObject.originalParent = attachedObjectInList.originalParent;

        //                 attachedObject.rbInterpolation = attachedObjectInList.rbInterpolation;

                        
        //             }
        //         }
        //         else
        //         {
        //             attachedObject.rbInterpolation = itemRB.interpolation;

        //             attachedObject.attachedRigidbodyWasKinematic = itemRB.isKinematic;
        //             attachedObject.attachedRigidbodyUsedGravity = itemRB.useGravity;
        //         }
                
        //         itemRB.interpolation = RigidbodyInterpolation.None;


        //         if (attachedObject.equipType == EquipType.Normal) {
        //         // if (attachedObject.HasAttachFlag(AttachmentFlags.TurnOnKinematic)) {
        //             attachedObject.collisionDetectionMode = itemRB.collisionDetectionMode;
        //             if (attachedObject.collisionDetectionMode == CollisionDetectionMode.Continuous)
        //                 itemRB.collisionDetectionMode = CollisionDetectionMode.Discrete;
        //             itemRB.isKinematic = true;
        //         }
                
                
        //         // if (attachedObject.HasAttachFlag(AttachmentFlags.TurnOffGravity))
        //         if (attachedObject.equipType == EquipType.Physics)
                
        //             itemRB.useGravity = false;
        //     }
            
        //     // attachedObject.equipPoint = alternateEquipPoint != null ? alternateEquipPoint : this.transform;// item.useAlternateAttachementPoint ? alternateEquipPoint : transform;


        //     // if (attachedObject.HasAttachFlag(AttachmentFlags.ParentToHand))
        //     if (attachedObject.equipType == EquipType.Normal) 
        //     {
        //         //Parent the object to the hand
        //         item.transform.parent = equipPoint;// this.transform;

        //         // attachedObject.isParentedToInventory = true;
        //     }
        //     else
        //     {
        //         // attachedObject.isParentedToInventory = false;
        //     }

                
        //     //Detach from the other hand if requested
        //     // if (attachedObject.HasAttachFlag(Hand.AttachmentFlags.DetachFromOtherHand))
        //     // {
        //     //     if (otherHand != null)
        //     //         otherHand.DetachObject(objectToAttach);
        //     // }

            
        //     // if (attachedObject.HasAttachFlag(AttachmentFlags.SnapOnAttach))
        //     if (attachedObject.equipType != EquipType.Static) 
        //     {

        //         Vector3 localPosition;
        //         Quaternion localRotation;
        //         GetLocalEquippedPositionTargets (item, out localPosition, out localRotation);

        //         Transform oldParent = item.transform.parent;
        //         item.transform.parent = equipPoint;
        //         item.transform.localPosition = localPosition;
        //         item.transform.localRotation = localRotation;
        //         item.transform.parent = oldParent;
                
        //         // if (item.equipBehavior != null && item.equipBehavior.equipSettings.Length > 0) {
        //         //     Transform oldParent = item.transform.parent;
        //         //     item.transform.parent = attachedObject.equipPoint;
        //         //     item.transform.localPosition = item.equipBehavior.equipSettings[0].position;
        //         //     item.transform.localRotation = Quaternion.Euler(item.equipBehavior.equipSettings[0].rotation);
        //         //     item.transform.parent = oldParent;
        //         // }
        //         // else {
        //         //     Debug.LogError(item.name + " :: has no equip behavior, or no equip settings, resetting to just equip point origin");
        //         // // if (attachedObject.item.skeletonPoser != null && HasSkeleton())
        //         // // {
        //         // //     SteamVR_Skeleton_PoseSnapshot pose = attachedObject.item.skeletonPoser.GetBlendedPose(skeleton);

        //         // //     //snap the object to the center of the attach point
        //         // //     item.transform.position = this.transform.TransformPoint(pose.position);
        //         // //     item.transform.rotation = this.transform.rotation * pose.rotation;
        //         // // }
        //         // // else
        //         // // { 
        //         //     //snap the object to the center of the attach point
        //         //     item.transform.rotation = attachedObject.equipPoint.rotation;
        //         //     item.transform.position = attachedObject.equipPoint.position;
        //         // // }
        //         // }
        //     }


        //     // attachedObject.targetLocalPos = attachedObject.equipPoint.InverseTransformPoint(item.transform.position);
        //     // attachedObject.initialRotationalOffset = Quaternion.Inverse(attachedObject.equipPoint.rotation) * item.transform.rotation;

            


        //     if (equippedItem != null) {
        //         UnequipItem(equippedItem.item);
        //     }


        //     equippedItem = attachedObject;
            
            
            
        //     item.OnEquipped (this);
        //     // objectToAttach.SendMessage("OnAttachedToHand", this, SendMessageOptions.DontRequireReceiver);
                
        //     if (onEquip != null) {
        //         onEquip(this, equippedItem.item);
        //     }
        // }


























































        // protected virtual void FixedUpdate()
        // {

        //     InventorySlot slot;
        //     if (ThisSubInventoryHasCurrentEquipped(out slot)) 
            
        //     // if (equippedItem != null)
        //     {
        //         // EquippedItem attachedInfo = equippedItem;

        //         // if (attachedInfo.equipType == EquipType.Physics) {
        //         if (slot.item.equipType == EquipType.Physics) {
                    
                    
        //             UpdateAttachedVelocity(slot.sceneReference.item.rigidbody);

        //         }
        //         // else if (attachedInfo.equipType == EquipType.Normal) {
        //         else if (slot.item.equipType == EquipType.Normal) {

        //             Vector3 localPosition;
        //         Quaternion localRotation;
        //         GetLocalEquippedPositionTargets (attachedInfo.item, out localPosition, out localRotation);

        //             slot.sceneReference.item.transform.localPosition = localPosition;//attachedInfo.targetLocalPos;
        //             slot.sceneReference.item.transform.localRotation = localRotation;//attachedInfo.initialRotationalOffset;
        //         }
                
                
        //             // if (attachedInfo.HasAttachFlag(Hand.AttachmentFlags.VelocityMovement))
        //             // {
        //             //     UpdateAttachedVelocity(attachedInfo);

        //             //     /*if (attachedInfo.interactable.handFollowTransformPosition)
        //             //     {
        //             //         skeleton.transform.position = TargetSkeletonPosition(attachedInfo);
        //             //         skeleton.transform.rotation = attachedInfo.attachedObject.transform.rotation * attachedInfo.skeletonLockRotation;
        //             //     }*/
        //             // }
        //             // else
        //             // {
        //             //     if (attachedInfo.HasAttachFlag(Hand.AttachmentFlags.ParentToHand))
        //             //     {
                            
        //             //         attachedInfo.item.transform.position = TargetEquippedItemWorldPosition();//attachedInfo);
        //             //         attachedInfo.item.transform.rotation = TargetItemRotation();//attachedInfo);
        //             //     }
                        
        //             // }

        //     }
        // }

        // protected void UpdateAttachedVelocity(Rigidbody attachedRigidbody)
        // {
        //     Vector3 velocityTarget, angularTarget;
        //     bool success = GetUpdatedEquippedVelocities(out velocityTarget, out angularTarget);
        //     if (success)
        //     {
        //         float scale = SteamVR_Utils.GetLossyScale(equipPoint);
                
        //         float maxAngularVelocityChange = MaxAngularVelocityChange * scale;
        //         float maxVelocityChange = MaxVelocityChange * scale;

        //         attachedRigidbody.velocity = Vector3.MoveTowards(attachedRigidbody.velocity, velocityTarget, maxVelocityChange);
        //         attachedRigidbody.angularVelocity = Vector3.MoveTowards(attachedRigidbody.angularVelocity, angularTarget, maxAngularVelocityChange);
        //     }
        // }

        // public Vector3 TargetEquippedItemWorldPosition()//EquippedItem attachedObject)
        // {
        //     // if (attachedObject.item.skeletonPoser != null && HasSkeleton())
        //     // {
        //     //     Vector3 tp = attachedObject.equipPoint.InverseTransformPoint(transform.TransformPoint(attachedObject.item.skeletonPoser.GetBlendedPose(skeleton).position));
        //     //     //tp.x *= -1;
        //     //     return equippedItem.equipPoint.TransformPoint(tp);
        //     // }
        //     // else
        //     // {
        //         return equippedItem.equipPoint.TransformPoint(equippedItem.targetLocalPos);
        //     // }
        // }

        // public Quaternion TargetItemRotation()//EquippedItem attachedObject)
        // {
        //     // if (attachedObject.item.skeletonPoser != null && HasSkeleton())
        //     // {
        //     //     Quaternion tr = Quaternion.Inverse(attachedObject.equipPoint.rotation) * (transform.rotation * attachedObject.item.skeletonPoser.GetBlendedPose(skeleton).rotation);
        //     //     return equippedItem.equipPoint.rotation * tr;
        //     // }
        //     // else
        //     // {
        //         return equippedItem.equipPoint.rotation * equippedItem.initialRotationalOffset;
        //     // }
        // }

        // public bool ItemIsEquipped(Item item)
        // {
        //     return equippedItem != null && equippedItem.item == item;
        // }

        


















        public void UnequipItem(Item item) {
            for (int i =0 ; i < equippedSlots.Length; i++) {
                if (equippedSlots[i] != null) {
                    if (equippedSlots[i].sceneItem == item) {
                        
                        UnequipItem(i);
                        return;
                    }
                }
            }
        }

        public void UnequipItem(int slotIndex)
        {

            if (slotIndex < 0 || slotIndex >= equippedSlots.Length) {
                Debug.LogError("Equip slot " + slotIndex + " is out of range on inventory " + name);
                return;
            }
            if (equippedSlots[slotIndex] == null) {
                return;
            }
            
            InventorySlot slot = equippedSlots[slotIndex];

            slot.sceneItem.linkedInventory = null;
            slot.sceneItem.myEquipPoint = null;
            
            slot.equipInfo.ReturnItemToOriginalStateBeforeEquip(slot.sceneItem);
    
            equippedSlots[slotIndex] = null;
            
                
            slot.sceneItem.OnUnequipped (this);
            
            if (onUnequip != null) {
                onUnequip(this, slot.sceneItem, slotIndex, slot.isQuickEquipped);
            }

            equipPoints[slotIndex].GetComponent<Interactor>().HoverUnlock(null);

            // equip buffs
            if (slot.item.onEquipBuffs != null) {
                // itemBehavior.onEquipBuffs.RemoveBuffsFromActor(actor, 1);
            }
        }















        //-------------------------------------------------
        // Detach this GameObject from the attached object stack of this Hand
        //
        // objectToDetach - The GameObject to detach from this Hand
        //-------------------------------------------------
        // public void UnequipItem(Item item)//, bool restoreOriginalParent = true)
        // {
        //     if (equippedItem == null) {
        //         return;
        //     }
        //     if (equippedItem.item != item) {
        //         return;
        //     }

        //     // Debug.LogError("unequuiping " + name);
            
        //     Transform parentTransform = null;
        //     // if (equippedItem.isParentedToInventory)
        //     // {
        //     //     if (restoreOriginalParent)
        //     //     {
        //             parentTransform = equippedItem.originalParent;
        //     //     }
        //         item.transform.parent = parentTransform;
        //     // }

        //     Rigidbody rb = equippedItem.item.rigidbody;
        //     if (rb != null)
        //     {
        //         rb.interpolation = equippedItem.rbInterpolation;

        //         if (equippedItem.equipType == EquipType.Normal)
        //         // if (equippedItem.HasAttachFlag(AttachmentFlags.TurnOnKinematic))
        //         {
        //             rb.isKinematic = equippedItem.attachedRigidbodyWasKinematic;
        //             rb.collisionDetectionMode = equippedItem.collisionDetectionMode;
        //         }
                
        //         // if (equippedItem.HasAttachFlag(AttachmentFlags.TurnOffGravity))
        //         if (equippedItem.equipType == EquipType.Physics)
                
        //         {   
        //             rb.useGravity = equippedItem.attachedRigidbodyUsedGravity;       
        //         }
        //     }

        //     // if (ao.interactable == null || (ao.interactable != null && ao.interactable.isDestroying == false))
        //     item.gameObject.SetActive(true);
                
        //     equippedItem = null;
                
        //     item.OnUnequipped (this);
        //     // ao.attachedObject.SendMessage("OnDetachedFromHand", this, SendMessageOptions.DontRequireReceiver);
                
        //     if (onUnequip != null) {
        //         onUnequip(this, item);
        //     }
        // }
    }



    // public class EquipPoint : MonoBehaviour {
    //     public Inventory baseInventory;
    //     public int equipSlotOnBase;


    //     void Awake () {
    //         baseInventory.SetEquipPoint(equipSlotOnBase, this);
    //     }



    //     protected const float MaxVelocityChange = 10f;
    //     protected const float VelocityMagic = 6000f;
    //     protected const float AngularVelocityMagic = 50f;
    //     protected const float MaxAngularVelocityChange = 20f;


    //     // bool ThisSubInventoryHasCurrentEquipped (out Inventory.InventorySlot slot) {
    //     //     return baseInventory.EquipSlotIsOccupied(equipSlotOnBase, out slot);
    //     // }




    //     protected virtual void FixedUpdate()
    //     {


    //         if (equipSlotOnBase < 0 || equipSlotOnBase >= baseInventory.equippedSlots.Length) {
    //         Debug.LogError("Equip slot " + equipSlotOnBase + " is out of range on inventory " + baseInventory);
    //         return;
    //     }

    //         Inventory.InventorySlot slot = baseInventory.equippedSlots[equipSlotOnBase]; 
    //         if (slot != null) {
    //             ItemBehavior item = slot.item;

    //             if (item.equipType == Inventory.EquipType.Physics) {        
    //                 UpdateAttachedVelocity(slot);
    //             }
    //             else if (item.equipType == Inventory.EquipType.Normal) {

    //                 // Vector3 localPosition;
    //                 // Quaternion localRotation;

    //                 // TransformBehavior.GetValues(item.equipTransform, equipSlotOnBase, out localPosition, out localRotation);

    //                 // slot.sceneItem.transform.localPosition = localPosition;//attachedInfo.targetLocalPos;
    //                 // slot.sceneItem.transform.localRotation = localRotation;//attachedInfo.initialRotationalOffset;


    //                 TransformBehavior.AdjustTransform(slot.sceneItem.transform, transform, item.equipTransform, equipSlotOnBase);

                    
    //             }
   
    //         }
    //     }


    //     void UpdateAttachedVelocity(Inventory.InventorySlot equippedSlot)
    //     {
    //         Vector3 velocityTarget, angularTarget;
    //         bool success = GetUpdatedEquippedVelocities(equippedSlot, out velocityTarget, out angularTarget);
    //         if (success)
    //         {
    //             float scale = SteamVR_Utils.GetLossyScale(transform);// equipPoint);
                
    //             float maxAngularVelocityChange = MaxAngularVelocityChange * scale;
    //             float maxVelocityChange = MaxVelocityChange * scale;
    //             Rigidbody attachedRigidbody = equippedSlot.sceneItem.rigidbody;
    //             attachedRigidbody.velocity = Vector3.MoveTowards(attachedRigidbody.velocity, velocityTarget, maxVelocityChange);
    //             attachedRigidbody.angularVelocity = Vector3.MoveTowards(attachedRigidbody.angularVelocity, angularTarget, maxAngularVelocityChange);
    //         }
    //     }

    //     public bool GetUpdatedEquippedVelocities(Inventory.InventorySlot equippedSlot, out Vector3 velocityTarget, out Vector3 angularTarget)
    //     {
    //         bool realNumbers = false;


    //         Vector3 localPosition;
    //         Quaternion localRotation;

    //         TransformBehavior.GetValues(equippedSlot.item.equipTransform, equipSlotOnBase, out localPosition, out localRotation);

    //         // GetLocalEquippedPositionTargets (equippedItem.item, out localPosition, out localRotation);


    //         Vector3 targetItemPosition = transform.TransformPoint(localPosition);//equippedItem.targetLocalPos);
    //         // Vector3 targetItemPosition = TargetEquippedItemWorldPosition();//equippedItem);




    //         Vector3 positionDelta = (targetItemPosition - equippedSlot.sceneItem.rigidbody.position);
    //         velocityTarget = (positionDelta * VelocityMagic * Time.deltaTime);

    //         if (float.IsNaN(velocityTarget.x) == false && float.IsInfinity(velocityTarget.x) == false)
    //         {
    //             realNumbers = true;
    //         }
    //         else
    //             velocityTarget = Vector3.zero;


    //         Quaternion targetItemRotation = transform.rotation * localRotation;
            
    //         Quaternion rotationDelta = targetItemRotation * Quaternion.Inverse(equippedSlot.sceneItem.transform.rotation);


    //         float angle;
    //         Vector3 axis;
    //         rotationDelta.ToAngleAxis(out angle, out axis);

    //         if (angle > 180)
    //             angle -= 360;

    //         if (angle != 0 && float.IsNaN(axis.x) == false && float.IsInfinity(axis.x) == false)
    //         {
    //             angularTarget = angle * axis * AngularVelocityMagic * Time.deltaTime;

    //             realNumbers &= true;
    //         }
    //         else
    //             angularTarget = Vector3.zero;

    //         return realNumbers;
    //     }

    // }
}
