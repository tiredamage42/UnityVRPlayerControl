using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VRPlayer;


using Valve.VR;

using System;


namespace InventorySystem {

public class Inventory : MonoBehaviour
{
    [Flags]
        public enum AttachmentFlags
        {
            SnapOnAttach = 1 << 0, // The object should snap to the position of the specified attachment point on the hand.
            DetachOthers = 1 << 1, // Other objects attached to this hand will be detached.
            DetachFromOtherHand = 1 << 2, // This object will be detached from the other hand.
            ParentToHand = 1 << 3, // The object will be parented to the hand.
            VelocityMovement = 1 << 4, // The object will attempt to move to match the position and rotation of the hand.
            TurnOnKinematic = 1 << 5, // The object will not respond to external physics.
            TurnOffGravity = 1 << 6, // The object will not respond to external physics.
            // AllowSidegrade = 1 << 7, // The object is able to switch from a pinch grab to a grip grab. Decreases likelyhood of a good throw but also decreases likelyhood of accidental drop
        };

    public void OnUseStart (int useIndex) {
            if (equippedItem != null) {
                // bool isUseable = hoveringInteractable.useType != Interactable.UseType.Scripted;
                // if (isUseable) {
                    equippedItem.item.OnEquippedUseStart(this, useIndex);
                // }
            }
            // if (onUseStart != null) {
            //     onUseStart (this, useIndex, hoveringInteractable);
            // }
        }
        public void OnUseEnd (int useIndex) {
            if (equippedItem != null) {
                // bool isUseable = hoveringInteractable.useType != Interactable.UseType.Scripted;
                // if (isUseable) {
                
                equippedItem.item.OnEquippedUseEnd(this, useIndex);
                // }
            }
            // if (onUseEnd != null) {
            //     onUseEnd (this, useIndex, hoveringInteractable);
            // }
        }
        public void OnUseUpdate (int useIndex) {
            if (equippedItem != null) {
                // bool isUseable = hoveringInteractable.useType != Interactable.UseType.Scripted;
                // if (isUseable) {
                
                equippedItem.item.OnEquippedUseUpdate(this, useIndex);
                // }
            }
            // if (onUseUpdate != null) {
            //     onUseUpdate (this, useIndex, hoveringInteractable);
            // }
        }



    public Inventory otherInventory;
    public class EquippedItem
    {
        public Item item;
        // public Interactable interactable;
        public Rigidbody attachedRigidbody;
        public CollisionDetectionMode collisionDetectionMode;
        public RigidbodyInterpolation rbInterpolation;
        public bool attachedRigidbodyWasKinematic;
        public bool attachedRigidbodyUsedGravity;
        public Transform originalParent;
        public bool isParentedToInventory;
        public AttachmentFlags attachFlags;
        public Transform equipPoint;
        // public Vector3 targetLocalPos;
        // public Quaternion initialRotationalOffset;
        // public Transform attachedOffsetTransform;
        // public float attachTime;

        public EquipType equipType;
        
        public bool HasAttachFlag(AttachmentFlags flag)
        {
            return (attachFlags & flag) == flag;
        }
    }
    

    public event System.Action<Inventory, Item> onUnequip, onEquip, onEquipUpdate;

    public Transform alternateEquipPoint;

    public EquippedItem equippedItem;

    protected const float MaxVelocityChange = 10f;
        protected const float VelocityMagic = 6000f;
        protected const float AngularVelocityMagic = 50f;
        protected const float MaxAngularVelocityChange = 20f;



    void Update () {
        if (equippedItem != null)
        {
            equippedItem.item.OnEquippedUpdate(this);
        if (onEquipUpdate != null) {
            onEquipUpdate(this, equippedItem.item);

        }
        }
    
    }



    void GetLocalEquippedPositionTargets (Item item, out Vector3 localPosition, out Quaternion localRotation) {
        localPosition = Vector3.zero;
        localRotation = Quaternion.identity;
        if (item.equipBehavior != null && item.equipBehavior.equipSettings.Length > 0) {
            localPosition = item.equipBehavior.equipSettings[0].position;
            localRotation = Quaternion.Euler(item.equipBehavior.equipSettings[0].rotation);
        }
    }

    public bool GetUpdatedEquippedVelocities(out Vector3 velocityTarget, out Vector3 angularTarget)
        {
            bool realNumbers = false;


            float velocityMagic = VelocityMagic;
            float angularVelocityMagic = AngularVelocityMagic;



            Vector3 localPosition;
            Quaternion localRotation;
            GetLocalEquippedPositionTargets (equippedItem.item, out localPosition, out localRotation);


            Vector3 targetItemPosition = equippedItem.equipPoint.TransformPoint(localPosition);//equippedItem.targetLocalPos);
            
            // Vector3 targetItemPosition = TargetEquippedItemWorldPosition();//equippedItem);




            Vector3 positionDelta = (targetItemPosition - equippedItem.attachedRigidbody.position);
            velocityTarget = (positionDelta * velocityMagic * Time.deltaTime);

            if (float.IsNaN(velocityTarget.x) == false && float.IsInfinity(velocityTarget.x) == false)
            {
                realNumbers = true;
            }
            else
                velocityTarget = Vector3.zero;






            Quaternion targetItemRotation = equippedItem.equipPoint.rotation * localRotation;
            // Quaternion targetItemRotation = TargetItemRotation();//equippedItem);
            
            Quaternion rotationDelta = targetItemRotation * Quaternion.Inverse(equippedItem.item.transform.rotation);


            float angle;
            Vector3 axis;
            rotationDelta.ToAngleAxis(out angle, out axis);

            if (angle > 180)
                angle -= 360;

            if (angle != 0 && float.IsNaN(axis.x) == false && float.IsInfinity(axis.x) == false)
            {
                angularTarget = angle * axis * angularVelocityMagic * Time.deltaTime;

                realNumbers &= true;
            }
            else
                angularTarget = Vector3.zero;

            return realNumbers;
        }







const AttachmentFlags defaultAttachmentFlags = AttachmentFlags.ParentToHand |
                                                              AttachmentFlags.DetachFromOtherHand |
                                                              AttachmentFlags.TurnOnKinematic |
                                                              AttachmentFlags.SnapOnAttach;


const AttachmentFlags physicsEquipFlags = AttachmentFlags.VelocityMovement | AttachmentFlags.TurnOffGravity | AttachmentFlags.SnapOnAttach;
const AttachmentFlags normalEquipFlags = AttachmentFlags.ParentToHand | AttachmentFlags.TurnOnKinematic | AttachmentFlags.SnapOnAttach;
const AttachmentFlags staticEquipFlags = 0;

AttachmentFlags GetFlags (EquipType equipType) {
    switch (equipType) {
        case EquipType.Static:
            return staticEquipFlags;
        case EquipType.Normal:
            return normalEquipFlags;
        case EquipType.Physics:
            return physicsEquipFlags;
    }
    return 0;
}

public enum EquipType {
    Static, // item stays where it is, isnt parented or moved around to hand
    Normal, // item parented to hand
    Physics, // item follows hand wiht velocities
};

/*

    // The flags used to determine how an object is attached to the hand.
        [Flags]
        public enum AttachmentFlags
        {
            SnapOnAttach = 1 << 0, // The object should snap to the position of the specified attachment point on the hand.
            DetachOthers = 1 << 1, // Other objects attached to this hand will be detached.
            DetachFromOtherHand = 1 << 2, // This object will be detached from the other hand.
            ParentToHand = 1 << 3, // The object will be parented to the hand.
            VelocityMovement = 1 << 4, // The object will attempt to move to match the position and rotation of the hand.
            TurnOnKinematic = 1 << 5, // The object will not respond to external physics.
            TurnOffGravity = 1 << 6, // The object will not respond to external physics.
            // AllowSidegrade = 1 << 7, // The object is able to switch from a pinch grab to a grip grab. Decreases likelyhood of a good throw but also decreases likelyhood of accidental drop
        };



 */

     //-------------------------------------------------
        // Attach a GameObject to this GameObject
        //
        // objectToAttach - The GameObject to attach
        // flags - The flags to use for attaching the object
        // attachmentPoint - Name of the GameObject in the hierarchy of this Hand which should act as the attachment point for this GameObject
        //-------------------------------------------------
        public void EquipItem(Item item)//, Hand.AttachmentFlags flags = defaultAttachmentFlags)
        {
            if(ItemIsEquipped(item))
                return;

            Rigidbody itemRB = item.GetComponent<Rigidbody>();

            Vector3 originalItemPosition = item.transform.position;
            Quaternion originalItemRotation = item.transform.rotation;
            
            EquippedItem attachedObject = new EquippedItem();
            attachedObject.attachFlags = GetFlags(item.equipType);// flags;
            attachedObject.equipType = item.equipType;

            attachedObject.item = item;
            // attachedObject.attachTime = Time.time;

            attachedObject.originalParent = item.transform.parent;

            attachedObject.attachedRigidbody = itemRB;
            
            if (itemRB != null)
            {
                if (item.parentInventory != null) //already attached to another hand
                {
                    //if it was attached to another hand, get the flags from that hand
                    
                    EquippedItem attachedObjectInList = item.parentInventory.equippedItem;
                    if (attachedObjectInList.item == attachedObject.item)
                    {
                        attachedObject.attachedRigidbodyWasKinematic = attachedObjectInList.attachedRigidbodyWasKinematic;
                        attachedObject.attachedRigidbodyUsedGravity = attachedObjectInList.attachedRigidbodyUsedGravity;
                        attachedObject.originalParent = attachedObjectInList.originalParent;

                        attachedObject.rbInterpolation = attachedObjectInList.rbInterpolation;

                        
                    }
                }
                else
                {
                    attachedObject.rbInterpolation = itemRB.interpolation;

                    attachedObject.attachedRigidbodyWasKinematic = itemRB.isKinematic;
                    attachedObject.attachedRigidbodyUsedGravity = itemRB.useGravity;
                }
                
                itemRB.interpolation = RigidbodyInterpolation.None;



                if (attachedObject.HasAttachFlag(AttachmentFlags.TurnOnKinematic)) {
                    attachedObject.collisionDetectionMode = itemRB.collisionDetectionMode;
                    if (attachedObject.collisionDetectionMode == CollisionDetectionMode.Continuous)
                        itemRB.collisionDetectionMode = CollisionDetectionMode.Discrete;
                    itemRB.isKinematic = true;
                }
                if (attachedObject.HasAttachFlag(AttachmentFlags.TurnOffGravity))
                    itemRB.useGravity = false;
            }
            
            attachedObject.equipPoint = alternateEquipPoint != null ? alternateEquipPoint : this.transform;// item.useAlternateAttachementPoint ? alternateEquipPoint : transform;


            if (attachedObject.HasAttachFlag(AttachmentFlags.ParentToHand))
            {
                //Parent the object to the hand
                item.transform.parent = attachedObject.equipPoint;// this.transform;
                attachedObject.isParentedToInventory = true;
            }
            else
            {
                attachedObject.isParentedToInventory = false;
            }

                
            //Detach from the other hand if requested
            // if (attachedObject.HasAttachFlag(Hand.AttachmentFlags.DetachFromOtherHand))
            // {
            //     if (otherHand != null)
            //         otherHand.DetachObject(objectToAttach);
            // }

            
            if (attachedObject.HasAttachFlag(AttachmentFlags.SnapOnAttach))
            {

                Vector3 localPosition;
                Quaternion localRotation;
                GetLocalEquippedPositionTargets (item, out localPosition, out localRotation);

                Transform oldParent = item.transform.parent;
                item.transform.parent = attachedObject.equipPoint;
                item.transform.localPosition = localPosition;
                item.transform.localRotation = localRotation;
                item.transform.parent = oldParent;
                
                // if (item.equipBehavior != null && item.equipBehavior.equipSettings.Length > 0) {
                //     Transform oldParent = item.transform.parent;
                //     item.transform.parent = attachedObject.equipPoint;
                //     item.transform.localPosition = item.equipBehavior.equipSettings[0].position;
                //     item.transform.localRotation = Quaternion.Euler(item.equipBehavior.equipSettings[0].rotation);
                //     item.transform.parent = oldParent;
                // }
                // else {
                //     Debug.LogError(item.name + " :: has no equip behavior, or no equip settings, resetting to just equip point origin");
                // // if (attachedObject.item.skeletonPoser != null && HasSkeleton())
                // // {
                // //     SteamVR_Skeleton_PoseSnapshot pose = attachedObject.item.skeletonPoser.GetBlendedPose(skeleton);

                // //     //snap the object to the center of the attach point
                // //     item.transform.position = this.transform.TransformPoint(pose.position);
                // //     item.transform.rotation = this.transform.rotation * pose.rotation;
                // // }
                // // else
                // // { 
                //     //snap the object to the center of the attach point
                //     item.transform.rotation = attachedObject.equipPoint.rotation;
                //     item.transform.position = attachedObject.equipPoint.position;
                // // }
                // }
            }


            // attachedObject.targetLocalPos = attachedObject.equipPoint.InverseTransformPoint(item.transform.position);
            // attachedObject.initialRotationalOffset = Quaternion.Inverse(attachedObject.equipPoint.rotation) * item.transform.rotation;


            if (equippedItem != null) {
                UnequipItem(equippedItem.item);
            }


            equippedItem = attachedObject;
            
            
            
            item.OnEquipped (this);
            // objectToAttach.SendMessage("OnAttachedToHand", this, SendMessageOptions.DontRequireReceiver);
                
            if (onEquip != null) {
                onEquip(this, equippedItem.item);
            }
        }


        protected virtual void FixedUpdate()
        {
            if (equippedItem != null)
            {
                EquippedItem attachedInfo = equippedItem;

                if (attachedInfo.equipType == EquipType.Physics) {
                    UpdateAttachedVelocity(attachedInfo);

                }
                else if (attachedInfo.equipType == EquipType.Normal) {

                    Vector3 localPosition;
                Quaternion localRotation;
                GetLocalEquippedPositionTargets (attachedInfo.item, out localPosition, out localRotation);

                    attachedInfo.item.transform.localPosition = localPosition;//attachedInfo.targetLocalPos;
                    attachedInfo.item.transform.localRotation = localRotation;//attachedInfo.initialRotationalOffset;
                }
                
                
                    // if (attachedInfo.HasAttachFlag(Hand.AttachmentFlags.VelocityMovement))
                    // {
                    //     UpdateAttachedVelocity(attachedInfo);

                    //     /*if (attachedInfo.interactable.handFollowTransformPosition)
                    //     {
                    //         skeleton.transform.position = TargetSkeletonPosition(attachedInfo);
                    //         skeleton.transform.rotation = attachedInfo.attachedObject.transform.rotation * attachedInfo.skeletonLockRotation;
                    //     }*/
                    // }
                    // else
                    // {
                    //     if (attachedInfo.HasAttachFlag(Hand.AttachmentFlags.ParentToHand))
                    //     {
                            
                    //         attachedInfo.item.transform.position = TargetEquippedItemWorldPosition();//attachedInfo);
                    //         attachedInfo.item.transform.rotation = TargetItemRotation();//attachedInfo);
                    //     }
                        
                    // }

            }
        }
        protected void UpdateAttachedVelocity(EquippedItem attachedObjectInfo)
        {
            Vector3 velocityTarget, angularTarget;
            bool success = GetUpdatedEquippedVelocities(out velocityTarget, out angularTarget);
            if (success)
            {
                float scale = SteamVR_Utils.GetLossyScale(equippedItem.equipPoint);
                
                float maxAngularVelocityChange = MaxAngularVelocityChange * scale;
                float maxVelocityChange = MaxVelocityChange * scale;

                attachedObjectInfo.attachedRigidbody.velocity = Vector3.MoveTowards(attachedObjectInfo.attachedRigidbody.velocity, velocityTarget, maxVelocityChange);
                attachedObjectInfo.attachedRigidbody.angularVelocity = Vector3.MoveTowards(attachedObjectInfo.attachedRigidbody.angularVelocity, angularTarget, maxAngularVelocityChange);
            }
        }

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

        public bool ItemIsEquipped(Item item)
        {
            return equippedItem != null && equippedItem.item == item;
        }

        
        //-------------------------------------------------
        // Detach this GameObject from the attached object stack of this Hand
        //
        // objectToDetach - The GameObject to detach from this Hand
        //-------------------------------------------------
        public void UnequipItem(Item item, bool restoreOriginalParent = true)
        {
            if (equippedItem == null) {
                return;
            }
            if (equippedItem.item != item) {
                return;
            }

            // Debug.LogError("unequuiping " + name);
            
            Transform parentTransform = null;
            if (equippedItem.isParentedToInventory)
            {
                if (restoreOriginalParent)
                {
                    parentTransform = equippedItem.originalParent;
                }
                item.transform.parent = parentTransform;
            }

            Rigidbody rb = equippedItem.attachedRigidbody;
            if (rb != null)
            {
                rb.interpolation = equippedItem.rbInterpolation;

                if (equippedItem.HasAttachFlag(AttachmentFlags.TurnOnKinematic))
                {
                    rb.isKinematic = equippedItem.attachedRigidbodyWasKinematic;
                    rb.collisionDetectionMode = equippedItem.collisionDetectionMode;
                }
                if (equippedItem.HasAttachFlag(AttachmentFlags.TurnOffGravity))
                {   
                    rb.useGravity = equippedItem.attachedRigidbodyUsedGravity;       
                }
            }

            // if (ao.interactable == null || (ao.interactable != null && ao.interactable.isDestroying == false))
            item.gameObject.SetActive(true);
                
            equippedItem = null;
                
            item.OnUnequipped (this);
            // ao.attachedObject.SendMessage("OnDetachedFromHand", this, SendMessageOptions.DontRequireReceiver);
                
            if (onUnequip != null) {
                onUnequip(this, item);
            }
        }
    }
}
