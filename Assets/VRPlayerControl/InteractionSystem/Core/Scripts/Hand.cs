//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: The hands used by the player in the vr interaction system
//
//=============================================================================

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine.Events;
using System.Threading;


using Valve.VR;
using Valve.VR.InteractionSystem;

using InteractionSystem;
using InventorySystem;

namespace VRPlayer
{
    //-------------------------------------------------------------------------
    // Links with an appropriate SteamVR controller and facilitates
    // interactions with objects in the virtual world.
    //-------------------------------------------------------------------------
    public class Hand : MonoBehaviour
    {

        [HideInInspector] public VelocityEstimator velocityEstimator;


        void AdditionalInitialization () {
            velocityEstimator = GetComponent<VelocityEstimator>();
            if (velocityEstimator == null) {
                Debug.LogError("Attach velocity estimator to: " + name);
            }
        }

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

        public const AttachmentFlags defaultAttachmentFlags = AttachmentFlags.ParentToHand |
                                                            //   AttachmentFlags.DetachOthers |
                                                              AttachmentFlags.DetachFromOtherHand |
                                                              AttachmentFlags.TurnOnKinematic |
                                                              AttachmentFlags.SnapOnAttach;

        public Hand otherHand;
        public SteamVR_Input_Sources handType;

        SteamVR_Behaviour_Pose trackedObject;



        public SteamVR_Action_Boolean useAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabPinch");
        
        
        public bool useHoverSphere = true;
        public Transform hoverSphereTransform;
        public float hoverSphereRadius = 0.05f;
        public LayerMask hoverLayerMask = -1;
        public float hoverUpdateInterval = 0.1f;

        public bool useControllerHoverComponent = true;
        public string controllerHoverComponent = "tip";
        public float controllerHoverRadius = 0.075f;

        public bool useFingerJointHover = true;
        public SteamVR_Skeleton_JointIndexEnum fingerJointHover = SteamVR_Skeleton_JointIndexEnum.indexTip;
        public float fingerJointHoverRadius = 0.025f;

        [Tooltip("A transform on the hand to center attached objects on")]
        public Transform objectAttachmentPoint;

        public GameObject renderModelPrefab;
        protected List<RenderModel> renderModels = new List<RenderModel>();
        protected RenderModel mainRenderModel;
        // protected RenderModel hoverhighlightRenderModel;

        public bool spewDebugText = false;
        public bool showDebugInteractables = false;

        // public class EquippedObject
        // {
        //     public GameObject attachedObject;
        //     public Interactable interactable;
        //     public Rigidbody attachedRigidbody;
        //     public CollisionDetectionMode collisionDetectionMode;
        //     public bool attachedRigidbodyWasKinematic;
        //     public bool attachedRigidbodyUsedGravity;
        //     public GameObject originalParent;
        //     public bool isParentedToHand;
        //     public AttachmentFlags attachmentFlags;
        //     public Vector3 initialPositionalOffset;
        //     public Quaternion initialRotationalOffset;
        //     public Transform attachedOffsetTransform;
        //     public Transform handAttachmentPointTransform;
        //     public Vector3 easeSourcePosition;
        //     public Quaternion easeSourceRotation;
        //     public float attachTime;

        //     public bool HasAttachFlag(AttachmentFlags flag)
        //     {
        //         return (attachmentFlags & flag) == flag;
        //     }
        // }


        // public EquippedObject currentAttached;
        
        // public bool hoverLocked { get; private set; }

        // private Interactable _hoveringInteractable;

        private TextMesh debugText;
        // private int prevOverlappingColliders = 0;

        // private const int ColliderArraySize = 16;
        // private Collider[] overlappingColliders;

        // private Player playerInstance;

        // private GameObject applicationLostFocusObject;

        // private SteamVR_Events.Action inputFocusAction;

        public bool isActive
        {
            get
            {
                return trackedObject.isActive;
                
            }
        }

        public bool isPoseValid
        {
            get
            {
                return trackedObject.isValid;
            }
        }


        // //-------------------------------------------------
        // // The Interactable object this Hand is currently hovering over
        // //-------------------------------------------------
        // public Interactable hoveringInteractable
        // {
        //     get { return _hoveringInteractable; }
        //     set
        //     {
        //         Interactable oldInteractable = _hoveringInteractable;
        //         Interactable newInteractable = value;
        //         if (oldInteractable != value)
        //         {
        //             if (oldInteractable != null)
        //             {
        //                 if (spewDebugText)
        //                     HandDebugLog("HoverEnd " + oldInteractable.gameObject);

                        
        //                 oldInteractable.OnInspectEnd(this);
        //                 // oldInteractable.SendMessage("OnHandHoverEnd", this, SendMessageOptions.DontRequireReceiver);
                        
        //                 StandardizedVRInput.instance.HideHint(this, useAction);

        //                 //if its a ui element
        //                 InputModule.instance.HoverEnd( oldInteractable.gameObject );
        //             }

        //             _hoveringInteractable = newInteractable;

        //             if (newInteractable != null)
        //             {
        //                 if (spewDebugText)
        //                     HandDebugLog("HoverBegin " + newInteractable.gameObject);


        //                 newInteractable.OnInspectStart(this);
        //                 // newInteractable.SendMessage("OnHandHoverBegin", this, SendMessageOptions.DontRequireReceiver);
        //                 //if it's useable... cant think of any time it wouldnt be
        //                 StandardizedVRInput.instance.ShowHint(this, useAction);

        //                 //if its a ui element
        //                 InputModule.instance.HoverBegin( newInteractable.gameObject );
                        
                        
        //             }
        //         }
        //     }
        // }


        /*

        when use down:
            if ui element

            InputModule.instance.Submit( uielement.gameObject )
        
        
        
        
        
        
        
        
        
        
        
        
        
         */


        
        public SteamVR_Behaviour_Skeleton skeleton
        {
            get
            {
                if (mainRenderModel != null)
                    return mainRenderModel.GetSkeleton();

                return null;
            }
        }

        public void ShowController(bool permanent = false)
        {
            if (mainRenderModel != null)
                mainRenderModel.SetControllerVisibility(true, permanent);

            // if (hoverhighlightRenderModel != null)
            //     hoverhighlightRenderModel.SetControllerVisibility(true, permanent);
        }

        public void HideController(bool permanent = false)
        {
            if (mainRenderModel != null)
                mainRenderModel.SetControllerVisibility(false, permanent);

            // if (hoverhighlightRenderModel != null)
            //     hoverhighlightRenderModel.SetControllerVisibility(false, permanent);
        }

        public void ShowSkeleton(bool permanent = false)
        {
            if (mainRenderModel != null)
                mainRenderModel.SetHandVisibility(true, permanent);

            // if (hoverhighlightRenderModel != null)
            //     hoverhighlightRenderModel.SetHandVisibility(true, permanent);
        }

        public void HideSkeleton(bool permanent = false)
        {
            if (mainRenderModel != null)
                mainRenderModel.SetHandVisibility(false, permanent);

            // if (hoverhighlightRenderModel != null)
            //     hoverhighlightRenderModel.SetHandVisibility(false, permanent);
        }

        public bool HasSkeleton()
        {
            return mainRenderModel != null && mainRenderModel.GetSkeleton() != null;
        }

        public void Show()
        {
            SetVisibility(true);
        }

        public void Hide()
        {
            SetVisibility(false);
        }

        public void SetVisibility(bool visible)
        {
            if (mainRenderModel != null)
                mainRenderModel.SetVisibility(visible);
        }

        public void SetSkeletonRangeOfMotion(EVRSkeletalMotionRange newRangeOfMotion, float blendOverSeconds = 0.1f)
        {
            for (int renderModelIndex = 0; renderModelIndex < renderModels.Count; renderModelIndex++)
            {
                renderModels[renderModelIndex].SetSkeletonRangeOfMotion(newRangeOfMotion, blendOverSeconds);
            }
        }

        public void SetTemporarySkeletonRangeOfMotion(SkeletalMotionRangeChange temporaryRangeOfMotionChange, float blendOverSeconds = 0.1f)
        {
            for (int renderModelIndex = 0; renderModelIndex < renderModels.Count; renderModelIndex++)
            {
                renderModels[renderModelIndex].SetTemporarySkeletonRangeOfMotion(temporaryRangeOfMotionChange, blendOverSeconds);
            }
        }

        public void ResetTemporarySkeletonRangeOfMotion(float blendOverSeconds = 0.1f)
        {
            for (int renderModelIndex = 0; renderModelIndex < renderModels.Count; renderModelIndex++)
            {
                renderModels[renderModelIndex].ResetTemporarySkeletonRangeOfMotion(blendOverSeconds);
            }
        }

        public void SetAnimationState(int stateValue)
        {
            for (int renderModelIndex = 0; renderModelIndex < renderModels.Count; renderModelIndex++)
            {
                renderModels[renderModelIndex].SetAnimationState(stateValue);
            }
        }

        public void StopAnimation()
        {
            for (int renderModelIndex = 0; renderModelIndex < renderModels.Count; renderModelIndex++)
            {
                renderModels[renderModelIndex].StopAnimation();
            }
        }


        //-------------------------------------------------
        // Attach a GameObject to this GameObject
        //
        // objectToAttach - The GameObject to attach
        // flags - The flags to use for attaching the object
        // attachmentPoint - Name of the GameObject in the hierarchy of this Hand which should act as the attachment point for this GameObject
        //-------------------------------------------------
        // public void AttachObject(GameObject objectToAttach, AttachmentFlags flags = defaultAttachmentFlags, Transform attachmentOffset = null)
        // {
        //     EquippedObject attachedObject = new EquippedObject();
        //     attachedObject.attachmentFlags = flags;
        //     attachedObject.attachedOffsetTransform = attachmentOffset;
        //     attachedObject.attachTime = Time.time;

        //     if (flags == 0)
        //     {
        //         flags = defaultAttachmentFlags;
        //     }

        //     //Make sure top object on stack is non-null
        //     // CleanUpAttachedObjectStack();

        //     //Detach the object if it is already attached so that it can get re-attached at the top of the stack
        //     if(ObjectIsAttached(objectToAttach))
        //         DetachObject(objectToAttach);

        //     //Detach from the other hand if requested
        //     if (attachedObject.HasAttachFlag(AttachmentFlags.DetachFromOtherHand))
        //     {
        //         if (otherHand != null)
        //             otherHand.DetachObject(objectToAttach);
        //     }


        //     if (currentAttached != null) {
        //         DetachObject(currentAttached.attachedObject);
        //     }

        //     attachedObject.attachedObject = objectToAttach;
        //     attachedObject.interactable = objectToAttach.GetComponent<Interactable>();
        //     attachedObject.handAttachmentPointTransform = this.transform;

        //     if (attachedObject.interactable != null)
        //     {
        //         if (attachedObject.interactable.attachEaseIn)
        //         {
        //             attachedObject.easeSourcePosition = attachedObject.attachedObject.transform.position;
        //             attachedObject.easeSourceRotation = attachedObject.attachedObject.transform.rotation;
        //             attachedObject.interactable.snapAttachEaseInCompleted = false;  
        //         }

        //         if (attachedObject.interactable.useHandObjectAttachmentPoint)
        //             attachedObject.handAttachmentPointTransform = objectAttachmentPoint;

        //         if (attachedObject.interactable.hideHandOnAttach)
        //             Hide();

        //         if (attachedObject.interactable.hideSkeletonOnAttach && mainRenderModel != null && mainRenderModel.displayHandByDefault)
        //             HideSkeleton();

        //         if (attachedObject.interactable.hideControllerOnAttach && mainRenderModel != null && mainRenderModel.displayControllerByDefault)
        //             HideController();

        //         if (attachedObject.interactable.handAnimationOnPickup != 0)
        //             SetAnimationState(attachedObject.interactable.handAnimationOnPickup);

        //         if (attachedObject.interactable.setRangeOfMotionOnPickup != SkeletalMotionRangeChange.None)
        //             SetTemporarySkeletonRangeOfMotion(attachedObject.interactable.setRangeOfMotionOnPickup);

        //     }

        //     attachedObject.originalParent = objectToAttach.transform.parent != null ? objectToAttach.transform.parent.gameObject : null;

        //     attachedObject.attachedRigidbody = objectToAttach.GetComponent<Rigidbody>();
            
        //     if (attachedObject.attachedRigidbody != null)
        //     {
        //         if (attachedObject.interactable.attachedToHand != null) //already attached to another hand
        //         {
        //             //if it was attached to another hand, get the flags from that hand
                    
        //             // for (int attachedIndex = 0; attachedIndex < attachedObject.interactable.attachedToHand.attachedObjects.Count; attachedIndex++)
        //             // {
        //                 EquippedObject attachedObjectInList = attachedObject.interactable.attachedToHand.currentAttached;//.attachedObjects[attachedIndex];
        //                 if (attachedObjectInList.interactable == attachedObject.interactable)
        //                 {
        //                     attachedObject.attachedRigidbodyWasKinematic = attachedObjectInList.attachedRigidbodyWasKinematic;
        //                     attachedObject.attachedRigidbodyUsedGravity = attachedObjectInList.attachedRigidbodyUsedGravity;
        //                     attachedObject.originalParent = attachedObjectInList.originalParent;
        //                 }
        //             // }
        //         }
        //         else
        //         {
        //             attachedObject.attachedRigidbodyWasKinematic = attachedObject.attachedRigidbody.isKinematic;
        //             attachedObject.attachedRigidbodyUsedGravity = attachedObject.attachedRigidbody.useGravity;
        //         }
        //     }

        //     // attachedObject.grabbedWithType = grabbedWithType;

        //     if (attachedObject.HasAttachFlag(AttachmentFlags.ParentToHand))
        //     {
        //         //Parent the object to the hand
        //         objectToAttach.transform.parent = this.transform;
        //         attachedObject.isParentedToHand = true;
        //     }
        //     else
        //     {
        //         attachedObject.isParentedToHand = false;
        //     }

        //     if (attachedObject.HasAttachFlag(AttachmentFlags.SnapOnAttach))
        //     {
        //         if (attachedObject.interactable != null && attachedObject.interactable.skeletonPoser != null && HasSkeleton())
        //         {
        //             SteamVR_Skeleton_PoseSnapshot pose = attachedObject.interactable.skeletonPoser.GetBlendedPose(skeleton);

        //             //snap the object to the center of the attach point
        //             objectToAttach.transform.position = this.transform.TransformPoint(pose.position);
        //             objectToAttach.transform.rotation = this.transform.rotation * pose.rotation;

        //             attachedObject.initialPositionalOffset = attachedObject.handAttachmentPointTransform.InverseTransformPoint(objectToAttach.transform.position);
        //             attachedObject.initialRotationalOffset = Quaternion.Inverse(attachedObject.handAttachmentPointTransform.rotation) * objectToAttach.transform.rotation;
        //         }
        //         else
        //         { 
        //             if (attachmentOffset != null)
        //             {
        //                 //offset the object from the hand by the positional and rotational difference between the offset transform and the attached object
        //                 Quaternion rotDiff = Quaternion.Inverse(attachmentOffset.transform.rotation) * objectToAttach.transform.rotation;
        //                 objectToAttach.transform.rotation = attachedObject.handAttachmentPointTransform.rotation * rotDiff;

        //                 Vector3 posDiff = objectToAttach.transform.position - attachmentOffset.transform.position;
        //                 objectToAttach.transform.position = attachedObject.handAttachmentPointTransform.position + posDiff;
        //             }
        //             else
        //             {
        //                 //snap the object to the center of the attach point
        //                 objectToAttach.transform.rotation = attachedObject.handAttachmentPointTransform.rotation;
        //                 objectToAttach.transform.position = attachedObject.handAttachmentPointTransform.position;
        //             }

        //             Transform followPoint = objectToAttach.transform;

        //             attachedObject.initialPositionalOffset = attachedObject.handAttachmentPointTransform.InverseTransformPoint(followPoint.position);
        //             attachedObject.initialRotationalOffset = Quaternion.Inverse(attachedObject.handAttachmentPointTransform.rotation) * followPoint.rotation;
        //         }
        //     }
        //     else
        //     {
        //         if (attachedObject.interactable != null && attachedObject.interactable.skeletonPoser != null && HasSkeleton())
        //         {
        //             attachedObject.initialPositionalOffset = attachedObject.handAttachmentPointTransform.InverseTransformPoint(objectToAttach.transform.position);
        //             attachedObject.initialRotationalOffset = Quaternion.Inverse(attachedObject.handAttachmentPointTransform.rotation) * objectToAttach.transform.rotation;
        //         }
        //         else
        //         {
        //             if (attachmentOffset != null)
        //             {
        //                 //get the initial positional and rotational offsets between the hand and the offset transform
        //                 Quaternion rotDiff = Quaternion.Inverse(attachmentOffset.transform.rotation) * objectToAttach.transform.rotation;
        //                 Quaternion targetRotation = attachedObject.handAttachmentPointTransform.rotation * rotDiff;
        //                 Quaternion rotationPositionBy = targetRotation * Quaternion.Inverse(objectToAttach.transform.rotation);

        //                 Vector3 posDiff = (rotationPositionBy * objectToAttach.transform.position) - (rotationPositionBy * attachmentOffset.transform.position);

        //                 attachedObject.initialPositionalOffset = attachedObject.handAttachmentPointTransform.InverseTransformPoint(attachedObject.handAttachmentPointTransform.position + posDiff);
        //                 attachedObject.initialRotationalOffset = Quaternion.Inverse(attachedObject.handAttachmentPointTransform.rotation) * (attachedObject.handAttachmentPointTransform.rotation * rotDiff);
        //             }
        //             else
        //             {
        //                 attachedObject.initialPositionalOffset = attachedObject.handAttachmentPointTransform.InverseTransformPoint(objectToAttach.transform.position);
        //                 attachedObject.initialRotationalOffset = Quaternion.Inverse(attachedObject.handAttachmentPointTransform.rotation) * objectToAttach.transform.rotation;
        //             }
        //         }
        //     }



        //     if (attachedObject.HasAttachFlag(AttachmentFlags.TurnOnKinematic))
        //     {
        //         if (attachedObject.attachedRigidbody != null)
        //         {
        //             attachedObject.collisionDetectionMode = attachedObject.attachedRigidbody.collisionDetectionMode;
        //             if (attachedObject.collisionDetectionMode == CollisionDetectionMode.Continuous)
        //                 attachedObject.attachedRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;

        //             attachedObject.attachedRigidbody.isKinematic = true;
        //         }
        //     }

        //     if (attachedObject.HasAttachFlag(AttachmentFlags.TurnOffGravity))
        //     {
        //         if (attachedObject.attachedRigidbody != null)
        //         {
        //             attachedObject.attachedRigidbody.useGravity = false;
        //         }
        //     }

        //     if (attachedObject.interactable != null && attachedObject.interactable.attachEaseIn)
        //     {
        //         attachedObject.attachedObject.transform.position = attachedObject.easeSourcePosition;
        //         attachedObject.attachedObject.transform.rotation = attachedObject.easeSourceRotation;
        //     }

        //     // attachedObjects.Add(attachedObject);
        //     currentAttached = attachedObject;
        //     // hasCurrentAttached = true;
        //     Debug.Log("Setting current Attached ::" + currentAttached.attachedObject);

        //     UpdateHovering();

        //     if (spewDebugText)
        //         HandDebugLog("AttachObject " + objectToAttach);



            






        //     objectToAttach.SendMessage("OnAttachedToHand", this, SendMessageOptions.DontRequireReceiver);
        // }

        protected float blendToPoseTime = 0.1f;
        protected float releasePoseBlendTime = 0.2f;



        void OnItemEquipped (Inventory inventory, Item item){//Inventory.EquippedItem equippedItem) {
            // VR_Item item = equippedItem.item.GetComponent<VR_Item>();
            // Item item = equippedItem.item;//.GetComponent<Item>();
            
            if (!item) return;
            if (item.hideHandOnAttach)
                Hide();

            if (item.hideSkeletonOnAttach && mainRenderModel != null && mainRenderModel.displayHandByDefault)
                HideSkeleton();

            if (item.hideControllerOnAttach && mainRenderModel != null && mainRenderModel.displayControllerByDefault)
                HideController();

            if (item.handAnimationOnPickup != 0)
                SetAnimationState(item.handAnimationOnPickup);

            if (item.setRangeOfMotionOnPickup != SkeletalMotionRangeChange.None)
                SetTemporarySkeletonRangeOfMotion(item.setRangeOfMotionOnPickup);


            if (item.skeletonPoser != null && skeleton != null)
            {
                Debug.Log("blendign to poser skeleton " + name);
                skeleton.BlendToPoser(item.skeletonPoser, blendToPoseTime);
            }

            if (item.activateActionSetOnAttach != null)
                item.activateActionSetOnAttach.Activate(handType);
        }

        void OnItemUnequipped(Inventory inventory, Item item){//.EquippedItem equippedItem) {
            // Item item = equippedItem.item;//.GetComponent<VR_Item>();
            if (!item) return;
            
            if (item.hideHandOnAttach)
                        Show();

                    if (item.hideSkeletonOnAttach && mainRenderModel != null && mainRenderModel.displayHandByDefault)
                        ShowSkeleton();

                    if (item.hideControllerOnAttach && mainRenderModel != null && mainRenderModel.displayControllerByDefault)
                        ShowController();

                    if (item.handAnimationOnPickup != 0)
                        StopAnimation();

                    if (item.setRangeOfMotionOnPickup != SkeletalMotionRangeChange.None)
                        ResetTemporarySkeletonRangeOfMotion();
              
            if (mainRenderModel != null)
                mainRenderModel.MatchHandToTransform(mainRenderModel.transform);
            // if (hoverhighlightRenderModel != null)
            //     hoverhighlightRenderModel.MatchHandToTransform(hoverhighlightRenderModel.transform);





            if (item.activateActionSetOnAttach != null)
            {
                // if (hand.otherHand == null || hand.otherHand.currentAttachedObjectInfo.HasValue == false ||
                //     (hand.otherHand.currentAttachedObjectInfo.Value.interactable != null &&
                //      hand.otherHand.currentAttachedObjectInfo.Value.interactable.activateActionSetOnAttach != this.activateActionSetOnAttach))
                
                if (inventory.otherInventory.equippedItem == null || inventory.otherInventory.equippedItem.item.activateActionSetOnAttach != item.activateActionSetOnAttach)
                // if (otherHand == null || otherHand.currentAttached == null ||
                //     (hand.otherHand.currentAttached.interactable != null &&
                //      hand.otherHand.currentAttached.interactable.activateActionSetOnAttach != this.activateActionSetOnAttach))
                
                
                {
                    item.activateActionSetOnAttach.Deactivate(handType);
                }
            }

            // do callback
            // if (onDetachedFromHand != null)
            // {
            //     onDetachedFromHand.Invoke(hand);
            // }

            //move to vr interacable
            if (item.skeletonPoser != null)
            {
                if (skeleton != null) {
                    skeleton.BlendToSkeleton(releasePoseBlendTime);
                    Debug.Log(name + "releasing pose to skeleton");
                }
            }

        }

        // public bool ObjectIsAttached(GameObject go)
        // {
        //     return currentAttached != null && currentAttached.attachedObject == go;
        // }

        // public void ForceHoverUnlock()
        // {
        //     hoverLocked = false;
        // }

        //-------------------------------------------------
        // Detach this GameObject from the attached object stack of this Hand
        //
        // objectToDetach - The GameObject to detach from this Hand
        //-------------------------------------------------
        // public void DetachObject(GameObject objectToDetach, bool restoreOriginalParent = true)
        // {
        //     if (currentAttached == null) {
        //         return;
        //     }
        //     if (currentAttached.attachedObject != objectToDetach) {
        //         return;
        //     }

        //         if (spewDebugText)
        //             HandDebugLog("DetachObject " + objectToDetach);

            
        //         EquippedObject ao = currentAttached;
        //         if (ao.interactable != null)
        //         {
        //             if (ao.interactable.hideHandOnAttach)
        //                 Show();

        //             if (ao.interactable.hideSkeletonOnAttach && mainRenderModel != null && mainRenderModel.displayHandByDefault)
        //                 ShowSkeleton();

        //             if (ao.interactable.hideControllerOnAttach && mainRenderModel != null && mainRenderModel.displayControllerByDefault)
        //                 ShowController();

        //             if (ao.interactable.handAnimationOnPickup != 0)
        //                 StopAnimation();

        //             if (ao.interactable.setRangeOfMotionOnPickup != SkeletalMotionRangeChange.None)
        //                 ResetTemporarySkeletonRangeOfMotion();
        //         }

        //         Transform parentTransform = null;
        //         if (ao.isParentedToHand)
        //         {
        //             if (restoreOriginalParent && (ao.originalParent != null))
        //             {
        //                 parentTransform = ao.originalParent.transform;
        //             }

        //             if (ao.attachedObject != null)
        //             {
        //                 ao.attachedObject.transform.parent = parentTransform;
        //             }
        //         }

        //         if (ao.HasAttachFlag(AttachmentFlags.TurnOnKinematic))
        //         {
        //             if (ao.attachedRigidbody != null)
        //             {
        //                 ao.attachedRigidbody.isKinematic = ao.attachedRigidbodyWasKinematic;
        //                 ao.attachedRigidbody.collisionDetectionMode = ao.collisionDetectionMode;
        //             }
        //         }

        //         if (ao.HasAttachFlag(AttachmentFlags.TurnOffGravity))
        //         {
        //             if (ao.attachedObject != null)
        //             {
        //                 if (ao.attachedRigidbody != null)
        //                     ao.attachedRigidbody.useGravity = ao.attachedRigidbodyUsedGravity;
        //             }
        //         }

        //         if (ao.interactable != null && ao.interactable.handFollowTransform && HasSkeleton())
        //         {
        //             skeleton.transform.localPosition = Vector3.zero;
        //             skeleton.transform.localRotation = Quaternion.identity;
        //         }

        //         if (ao.attachedObject != null)
        //         {
        //             if (ao.interactable == null || (ao.interactable != null && ao.interactable.isDestroying == false))
        //                 ao.attachedObject.SetActive(true);

        //             ao.attachedObject.SendMessage("OnDetachedFromHand", this, SendMessageOptions.DontRequireReceiver);
        //         }

        //         currentAttached = null;

                
        //         hoverLocked = false;


                

            
        //     if (mainRenderModel != null)
        //         mainRenderModel.MatchHandToTransform(mainRenderModel.transform);
        //     if (hoverhighlightRenderModel != null)
        //         hoverhighlightRenderModel.MatchHandToTransform(hoverhighlightRenderModel.transform);
        // }


        //-------------------------------------------------
        // Get the world velocity of the VR Hand.
        //-------------------------------------------------
        public Vector3 GetTrackedObjectVelocity(float timeOffset = 0)
        {
            // if (trackedObject == null)
            // {
            //     Vector3 velocityTarget, angularTarget;
            //     inventory.GetUpdatedEquippedVelocities(out velocityTarget, out angularTarget);
            //     Debug.LogError("Getting tracked object velocity here");
            //     return velocityTarget;
            // }

                if (isActive)
            {
                if (timeOffset == 0)
                    return VRManager.trackingOrigin.TransformVector(trackedObject.GetVelocity());
                else
                {
                    Vector3 velocity;
                    Vector3 angularVelocity;

                    trackedObject.GetVelocitiesAtTimeOffset(timeOffset, out velocity, out angularVelocity);
                    return VRManager.trackingOrigin.TransformVector(velocity);
                }
            }

            return Vector3.zero;
        }
        

        //-------------------------------------------------
        // Get the world space angular velocity of the VR Hand.
        //-------------------------------------------------
        public Vector3 GetTrackedObjectAngularVelocity(float timeOffset = 0)
        {
            

            if (isActive)
            {
                if (timeOffset == 0)
                    return VRManager.trackingOrigin.TransformDirection(trackedObject.GetAngularVelocity());
                else
                {
                    Vector3 velocity;
                    Vector3 angularVelocity;

                    trackedObject.GetVelocitiesAtTimeOffset(timeOffset, out velocity, out angularVelocity);
                    return VRManager.trackingOrigin.TransformDirection(angularVelocity);
                }
            }

            return Vector3.zero;
        }

        public void GetEstimatedPeakVelocities(out Vector3 velocity, out Vector3 angularVelocity)
        {
            Debug.LogError("heiyyy!");
            trackedObject.GetEstimatedPeakVelocities(out velocity, out angularVelocity);
            velocity = VRManager.trackingOrigin.TransformVector(velocity);
            angularVelocity = VRManager.trackingOrigin.TransformDirection(angularVelocity);
        }



        protected virtual void Awake()
        {
            AdditionalInitialization();
            // inputFocusAction = SteamVR_Events.InputFocusAction(OnInputFocus);

            if (hoverSphereTransform == null)
                hoverSphereTransform = this.transform;

            if (objectAttachmentPoint == null)
                objectAttachmentPoint = this.transform;

            // applicationLostFocusObject = new GameObject("_application_lost_focus");
            // applicationLostFocusObject.transform.parent = transform;
            // applicationLostFocusObject.SetActive(false);

            trackedObject = GetComponent<SteamVR_Behaviour_Pose>();
            trackedObject.onTransformUpdatedEvent += OnTransformUpdated;

            inventory = GetComponent<Inventory>();
            interactor = GetComponent<Interactor>();
        }

        Inventory inventory;
        Interactor interactor;

        protected virtual void OnDestroy()
        {
            trackedObject.onTransformUpdatedEvent -= OnTransformUpdated;
        }

        protected virtual void OnTransformUpdated(SteamVR_Behaviour_Pose updatedPose, SteamVR_Input_Sources updatedSource)
        {
            // HandFollowUpdate();
        }

        //-------------------------------------------------
        protected virtual IEnumerator Start()
        {
            // // allocate array for colliders
            // overlappingColliders = new Collider[ColliderArraySize];

            //Debug.Log( "<b>[SteamVR Interaction]</b> Hand - initializing connection routine" );

            while (true)
            {
                if (isPoseValid)
                {
                    InitController();
                    break;
                }

                yield return null;
            }
        }

        void SetHoverPositions () {
            List<Vector4> setPosChecks = new List<Vector4>();

            if (useHoverSphere)
            {
                Vector3 pos = hoverSphereTransform.position;
                setPosChecks.Add (new Vector4 (
                    pos.x, pos.y, pos.z,

                hoverSphereRadius * Mathf.Abs(SteamVR_Utils.GetLossyScale(hoverSphereTransform))
                ));
                // CheckHoveringForTransform(hoverSphereTransform.position, scaledHoverRadius, ref closestDistance, ref closestInteractable, Color.green);
            }

            if (useControllerHoverComponent && mainRenderModel != null && mainRenderModel.IsControllerVisibile())
            {
                Vector3 pos = mainRenderModel.GetControllerPosition(controllerHoverComponent);
                setPosChecks.Add (new Vector4 (
pos.x, pos.y, pos.z,
                controllerHoverRadius * Mathf.Abs(SteamVR_Utils.GetLossyScale(this.transform)) / 2f
                ));
                
                // CheckHoveringForTransform(mainRenderModel.GetControllerPosition(controllerHoverComponent), scaledHoverRadius / 2f, ref closestDistance, ref closestInteractable, Color.blue);
            }

            if (useFingerJointHover && mainRenderModel != null && mainRenderModel.IsHandVisibile())
            {
                Vector3 pos = mainRenderModel.GetBonePosition((int)fingerJointHover);
                setPosChecks.Add (new Vector4 (
pos.x, pos.y, pos.z,
                    fingerJointHoverRadius * Mathf.Abs(SteamVR_Utils.GetLossyScale(this.transform)) / 2f
                ));
                
                // CheckHoveringForTransform(mainRenderModel.GetBonePosition((int)fingerJointHover), scaledHoverRadius / 2f, ref closestDistance, ref closestInteractable, Color.yellow);
            }





            interactor.SetHoverCheckPositionsAndRadii(setPosChecks.ToArray());
        }
       


        //-------------------------------------------------
        // protected virtual void UpdateHovering()
        // {
        //     if (hoverLocked)
        //         return;

        //     if (applicationLostFocusObject.activeSelf) {
        //         Debug.Log("this is why");
        //         return;
        //     }

        //     float closestDistance = float.MaxValue;
        //     Interactable closestInteractable = null;

        //     if (useHoverSphere)
        //     {
        //         float scaledHoverRadius = hoverSphereRadius * Mathf.Abs(SteamVR_Utils.GetLossyScale(hoverSphereTransform));
        //         CheckHoveringForTransform(hoverSphereTransform.position, scaledHoverRadius, ref closestDistance, ref closestInteractable, Color.green);
        //     }

        //     if (useControllerHoverComponent && mainRenderModel != null && mainRenderModel.IsControllerVisibile())
        //     {
        //         float scaledHoverRadius = controllerHoverRadius * Mathf.Abs(SteamVR_Utils.GetLossyScale(this.transform));
        //         CheckHoveringForTransform(mainRenderModel.GetControllerPosition(controllerHoverComponent), scaledHoverRadius / 2f, ref closestDistance, ref closestInteractable, Color.blue);
        //     }

        //     if (useFingerJointHover && mainRenderModel != null && mainRenderModel.IsHandVisibile())
        //     {
        //         float scaledHoverRadius = fingerJointHoverRadius * Mathf.Abs(SteamVR_Utils.GetLossyScale(this.transform));
        //         CheckHoveringForTransform(mainRenderModel.GetBonePosition((int)fingerJointHover), scaledHoverRadius / 2f, ref closestDistance, ref closestInteractable, Color.yellow);
        //     }
        //     if (closestInteractable != null) {
        //         Debug.Log(closestInteractable);
        //     }

        //     // Hover on this one
        //     hoveringInteractable = closestInteractable;
        // }

        // protected virtual bool CheckHoveringForTransform(Vector3 hoverPosition, float hoverRadius, ref float closestDistance, ref Interactable closestInteractable, Color debugColor)
        // {
        //     bool foundCloser = false;

        //     // null out old vals
        //     for (int i = 0; i < overlappingColliders.Length; ++i)
        //     {
        //         overlappingColliders[i] = null;
        //     }

        //     int numColliding = Physics.OverlapSphereNonAlloc(hoverPosition, hoverRadius, overlappingColliders, hoverLayerMask.value);

        //     if (numColliding == ColliderArraySize)
        //         Debug.LogWarning("<b>[SteamVR Interaction]</b> This hand is overlapping the max number of colliders: " + ColliderArraySize + ". Some collisions may be missed. Increase ColliderArraySize on Hand.cs");

        //     // DebugVar
        //     int iActualColliderCount = 0;

        //     // Pick the closest hovering
        //     for (int colliderIndex = 0; colliderIndex < overlappingColliders.Length; colliderIndex++)
        //     {
        //         Collider collider = overlappingColliders[colliderIndex];

        //         if (collider == null)
        //             continue;

        //         Interactable contacting = collider.GetComponentInParent<Interactable>();

        //         // Yeah, it's null, skip
        //         if (contacting == null)
        //             continue;

        //         // Can't hover over the object if it's attached
        //         Inventory myInventory = GetComponent<Inventory>();
        //         if (myInventory.equippedItem != null && myInventory.equippedItem.item == contacting.gameObject)
        //             continue;
                
        //         // Occupied by another hand, so we can't touch it
        //         // if (otherHand && otherHand.hoveringInteractable == contacting)
        //         //     continue;

        //         // Best candidate so far...
        //         float distance = Vector3.Distance(contacting.transform.position, hoverPosition);
        //         if (distance < closestDistance)
        //         {
        //             closestDistance = distance;
        //             closestInteractable = contacting;
        //             foundCloser = true;
        //         }
        //         iActualColliderCount++;
        //     }

        //     if (showDebugInteractables && foundCloser)
        //     {
        //         Debug.DrawLine(hoverPosition, closestInteractable.transform.position, debugColor, .05f, false);
        //     }

        //     if (iActualColliderCount > 0 && iActualColliderCount != prevOverlappingColliders)
        //     {
        //         prevOverlappingColliders = iActualColliderCount;

        //         if (spewDebugText)
        //             HandDebugLog("Found " + iActualColliderCount + " overlapping colliders.");
        //     }

        //     return foundCloser;
        // }


        //-------------------------------------------------
        private void UpdateDebugText()
        {
                if (debugText == null)
                {
                    debugText = new GameObject("_debug_text").AddComponent<TextMesh>();
                    debugText.fontSize = 120;
                    debugText.characterSize = 0.001f;
                    debugText.transform.parent = transform;

                    debugText.transform.localRotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
                }

                if (handType == SteamVR_Input_Sources.RightHand)
                {
                    debugText.transform.localPosition = new Vector3(-0.05f, 0.0f, 0.0f);
                    debugText.alignment = TextAlignment.Right;
                    debugText.anchor = TextAnchor.UpperRight;
                }
                else
                {
                    debugText.transform.localPosition = new Vector3(0.05f, 0.0f, 0.0f);
                    debugText.alignment = TextAlignment.Left;
                    debugText.anchor = TextAnchor.UpperLeft;
                }

                debugText.text = string.Format(
                    "Hovering: {0}\n" +
                    "Hover Lock: {1}\n" +
                    "Attached: {2}\n" +
                    "Type: {3}\n",
                    (interactor.hoveringInteractable ? interactor.hoveringInteractable.gameObject.name : "null"),
                    interactor.hoverLocked,
                    (inventory.equippedItem != null ? inventory.equippedItem.item.name : "null"),
                    
                    handType.ToString());
           
        }


        //-------------------------------------------------
        protected virtual void OnEnable()
        {
            // inputFocusAction.enabled = true;

            // Stagger updates between hands
            float hoverUpdateBegin = ((otherHand != null) && (otherHand.GetInstanceID() < GetInstanceID())) ? (0.5f * hoverUpdateInterval) : (0.0f);
            // InvokeRepeating("UpdateHovering", hoverUpdateBegin, hoverUpdateInterval);

            InvokeRepeating("UpdateDebugText", hoverUpdateBegin, hoverUpdateInterval);


            Inventory inventory = GetComponent<Inventory>();
            inventory.onEquip += OnItemEquipped;
            inventory.onUnequip += OnItemUnequipped;
            inventory.onEquipUpdate += OnEquippedUpdate;

            Interactor interactor = GetComponent<Interactor>();
            interactor.onInspectUpdate += OnInspectUpdate;
            interactor.onInspectStart += OnInspectStart;
            interactor.onInspectEnd += OnInspectEnd;

        }


        //-------------------------------------------------
        protected virtual void OnDisable()
        {
            // inputFocusAction.enabled = false;

            CancelInvoke();

            
            Inventory inventory = GetComponent<Inventory>();
            inventory.onEquip -= OnItemEquipped;
            inventory.onUnequip -= OnItemUnequipped;
            inventory.onEquipUpdate -= OnEquippedUpdate;

            Interactor interactor = GetComponent<Interactor>();
            interactor.onInspectUpdate -= OnInspectUpdate;
            interactor.onInspectStart -= OnInspectStart;
            interactor.onInspectEnd -= OnInspectEnd;
        }



        void OnInspectUpdate (Interactor interactor, Interactable hoveringInteractable) {
            if (hoveringInteractable)
            {
                // bool useDown = useAction.GetStateDown(handType);
                // bool useUp = useAction.GetStateUp(handType);
                // bool useHeld = useAction.GetState(handType);
                
                // if (useDown) {
                //     StandardizedVRInput.instance.HideHint(this, useAction);
                //     interactor.OnUseStart(0);
                // }
                // if (useUp) {
                //     interactor.OnUseEnd(0);
                // }
                // if (useHeld) {
                //     interactor.OnUseUpdate(0);
                // }
            }

        }
        void OnInspectStart (Interactor interactor, Interactable hoveringInteractable) {

            // newInteractable.SendMessage("OnHandHoverBegin", this, SendMessageOptions.DontRequireReceiver);
            //if it's useable... cant think of any time it wouldnt be
            StandardizedVRInput.instance.ShowHint(this, useAction);

            //if its a ui element
            InputModule.instance.HoverBegin( hoveringInteractable.gameObject );

        }
        void OnInspectEnd (Interactor interactor, Interactable hoveringInteractable) {
             StandardizedVRInput.instance.HideHint(this, useAction);

            //if its a ui element
            InputModule.instance.HoverEnd( hoveringInteractable.gameObject );

        }




        
        //-------------------------------------------------
        protected virtual void Update()
        {
            bool useDown = useAction.GetStateDown(handType);
                bool useUp = useAction.GetStateUp(handType);
                bool useHeld = useAction.GetState(handType);
                
                if (useDown) {
                    StandardizedVRInput.instance.HideHint(this, useAction);
                    interactor.OnUseStart(0);
                    inventory.OnUseStart(0);
                }
                if (useUp) {
                    interactor.OnUseEnd(0);
                    inventory.OnUseEnd(0);
                }
                if (useHeld) {
                    interactor.OnUseUpdate(0);
                                        inventory.OnUseUpdate(0);

                }
            SetHoverPositions();
            
            // if (currentAttached != null)
            // {
            //     currentAttached.
            //     attachedObject.SendMessage("HandAttachedUpdate", this, SendMessageOptions.DontRequireReceiver);
            // }

            // if (hoveringInteractable)
            // {
            //     bool isUseable = hoveringInteractable.useType != Interactable.UseType.Scripted;
                
            //     bool useDown = useAction.GetStateDown(handType);
            //     bool useUp = useAction.GetStateUp(handType);
            //     bool useHeld = useAction.GetState(handType);

            //     if (useDown) {
            //         StandardizedVRInput.instance.HideHint(this, useAction);
            //     }

            //     hoveringInteractable.OnInspectUpdate(this);
                
            //     // hoveringInteractable.SendMessage("HandHoverUpdate", this, SendMessageOptions.DontRequireReceiver);
            //     if (isUseable) {
            //         if (useDown)
            //             hoveringInteractable.OnUseStart(this);
            //         if (useHeld)
            //             hoveringInteractable.OnUseUpdate(this);
            //         if (useUp)
            //             hoveringInteractable.OnUseEnd(this);
            //     }
            // }

                


                
        }

        protected virtual void OnEquippedUpdate( Inventory inventory, Item item)//.EquippedItem currentAttached)
        {
            // Inventory.EquippedItem currentAttached = inventory.equippedItem;

            // if (currentAttached != null)
            // {
                // Item item = currentAttached.item;//.GetComponent<VR_Item>();

                if (item != null)//currentAttached.interactable != null)
                
                {
                    SteamVR_Skeleton_PoseSnapshot pose = null;

                    if (item.skeletonPoser != null && HasSkeleton())
                    {
                        pose = item.skeletonPoser.GetBlendedPose(skeleton);
                    }

                    if (item.handFollowTransform)                    
                    {
                        Quaternion targetHandRotation;
                        Vector3 targetHandPosition;

                        if (pose == null)
                        {
                            Transform equipPoint = inventory.equippedItem.equipPoint;
                            // Quaternion offset = Quaternion.Inverse(this.transform.rotation) * item.handAttachmentPointTransform.rotation;
                            Quaternion offset = Quaternion.Inverse(this.transform.rotation) * equipPoint.rotation;
                            
                            targetHandRotation = item.transform.rotation * Quaternion.Inverse(offset);

                            // Vector3 worldOffset = (this.transform.position - currentAttached.handAttachmentPointTransform.position);
                            Vector3 worldOffset = (this.transform.position - equipPoint.position);
                            
                            Quaternion rotationDiff = mainRenderModel.GetHandRotation() * Quaternion.Inverse(this.transform.rotation);
                            Vector3 localOffset = rotationDiff * worldOffset;
                            targetHandPosition = item.transform.position + localOffset;
                        }
                        else
                        {
                            Transform objectT = item.transform;
                            
                            Vector3 oldItemPos = objectT.position;
                            Quaternion oldItemRot = objectT.transform.rotation;
                            objectT.position = inventory.TargetItemPosition();//currentAttached);
                            objectT.rotation = inventory.TargetItemRotation();//currentAttached);
                            
                            Vector3 localSkelePos = objectT.InverseTransformPoint(transform.position);
                            Quaternion localSkeleRot = Quaternion.Inverse(objectT.rotation) * transform.rotation;
                            objectT.position = oldItemPos;
                            objectT.rotation = oldItemRot;

                            targetHandPosition = objectT.TransformPoint(localSkelePos);
                            targetHandRotation = objectT.rotation * localSkeleRot;
                        }

                        if (mainRenderModel != null)
                            mainRenderModel.SetHandRotation(targetHandRotation);
                        // if (hoverhighlightRenderModel != null)
                        //     hoverhighlightRenderModel.SetHandRotation(targetHandRotation);

                        if (mainRenderModel != null)
                            mainRenderModel.SetHandPosition(targetHandPosition);
                        // if (hoverhighlightRenderModel != null)
                        //     hoverhighlightRenderModel.SetHandPosition(targetHandPosition);
                    }
                }
            // }
        }


        // protected virtual void HandFollowUpdate()
        // {
        //     Inventory.EquippedItem currentAttached = inventory.equippedItem;

        //     if (currentAttached != null)
        //     {
        //         VR_Item item = currentAttached.item.GetComponent<VR_Item>();

        //         if (item != null)//currentAttached.interactable != null)
                
        //         {
        //             SteamVR_Skeleton_PoseSnapshot pose = null;

        //             if (item.skeletonPoser != null && HasSkeleton())
        //             {
        //                 pose = item.skeletonPoser.GetBlendedPose(skeleton);
        //             }

        //             if (item.handFollowTransform)                    
        //             {
        //                 Quaternion targetHandRotation;
        //                 Vector3 targetHandPosition;

        //                 if (pose == null)
        //                 {
        //                     Quaternion offset = Quaternion.Inverse(this.transform.rotation) * currentAttached.handAttachmentPointTransform.rotation;
        //                     targetHandRotation = item.transform.rotation * Quaternion.Inverse(offset);

        //                     Vector3 worldOffset = (this.transform.position - currentAttached.handAttachmentPointTransform.position);
        //                     Quaternion rotationDiff = mainRenderModel.GetHandRotation() * Quaternion.Inverse(this.transform.rotation);
        //                     Vector3 localOffset = rotationDiff * worldOffset;
        //                     targetHandPosition = item.transform.position + localOffset;
        //                 }
        //                 else
        //                 {
        //                     Transform objectT = item.transform;
                            
        //                     Vector3 oldItemPos = objectT.position;
        //                     Quaternion oldItemRot = objectT.transform.rotation;
        //                     objectT.position = TargetItemPosition(currentAttached);
        //                     objectT.rotation = TargetItemRotation(currentAttached);
                            
        //                     Vector3 localSkelePos = objectT.InverseTransformPoint(transform.position);
        //                     Quaternion localSkeleRot = Quaternion.Inverse(objectT.rotation) * transform.rotation;
        //                     objectT.position = oldItemPos;
        //                     objectT.rotation = oldItemRot;

        //                     targetHandPosition = objectT.TransformPoint(localSkelePos);
        //                     targetHandRotation = objectT.rotation * localSkeleRot;
        //                 }

        //                 if (mainRenderModel != null)
        //                     mainRenderModel.SetHandRotation(targetHandRotation);
        //                 // if (hoverhighlightRenderModel != null)
        //                 //     hoverhighlightRenderModel.SetHandRotation(targetHandRotation);

        //                 if (mainRenderModel != null)
        //                     mainRenderModel.SetHandPosition(targetHandPosition);
        //                 // if (hoverhighlightRenderModel != null)
        //                 //     hoverhighlightRenderModel.SetHandPosition(targetHandPosition);
        //             }
        //         }
        //     }
        // }

        // protected virtual void FixedUpdate()
        // {
        //     if (currentAttached != null)
            
        //     {
        //         EquippedObject attachedInfo = currentAttached;
                
        //         if (attachedInfo.attachedObject != null)
        //         {
        //             if (attachedInfo.HasAttachFlag(AttachmentFlags.VelocityMovement))
        //             {
        //                 if(attachedInfo.interactable.attachEaseIn == false || attachedInfo.interactable.snapAttachEaseInCompleted)
        //                     UpdateAttachedVelocity(attachedInfo);

        //                 /*if (attachedInfo.interactable.handFollowTransformPosition)
        //                 {
        //                     skeleton.transform.position = TargetSkeletonPosition(attachedInfo);
        //                     skeleton.transform.rotation = attachedInfo.attachedObject.transform.rotation * attachedInfo.skeletonLockRotation;
        //                 }*/
        //             }
        //             else
        //             {
        //                 if (attachedInfo.HasAttachFlag(AttachmentFlags.ParentToHand))
        //                 {
        //                     attachedInfo.attachedObject.transform.position = TargetItemPosition(attachedInfo);
        //                     attachedInfo.attachedObject.transform.rotation = TargetItemRotation(attachedInfo);
        //                 }
        //             }


        //             if (attachedInfo.interactable.attachEaseIn)
        //             {
        //                 float t = Util.RemapNumberClamped(Time.time, attachedInfo.attachTime, attachedInfo.attachTime + attachedInfo.interactable.snapAttachEaseInTime, 0.0f, 1.0f);
        //                 if (t < 1.0f)
        //                 {
        //                     if (attachedInfo.HasAttachFlag(AttachmentFlags.VelocityMovement))
        //                     {
        //                         attachedInfo.attachedRigidbody.velocity = Vector3.zero;
        //                         attachedInfo.attachedRigidbody.angularVelocity = Vector3.zero;
        //                     }
        //                     t = attachedInfo.interactable.snapAttachEaseInCurve.Evaluate(t);
        //                     attachedInfo.attachedObject.transform.position = Vector3.Lerp(attachedInfo.easeSourcePosition, TargetItemPosition(attachedInfo), t);
        //                     attachedInfo.attachedObject.transform.rotation = Quaternion.Lerp(attachedInfo.easeSourceRotation, TargetItemRotation(attachedInfo), t);
        //                 }
        //                 else if (!attachedInfo.interactable.snapAttachEaseInCompleted)
        //                 {
        //                     attachedInfo.interactable.gameObject.SendMessage("OnThrowableAttachEaseInCompleted", this, SendMessageOptions.DontRequireReceiver);
        //                     attachedInfo.interactable.snapAttachEaseInCompleted = true;
        //                 }
        //             }
        //         }
        //     }
        // }

        // protected const float MaxVelocityChange = 10f;
        // protected const float VelocityMagic = 6000f;
        // protected const float AngularVelocityMagic = 50f;
        // protected const float MaxAngularVelocityChange = 20f;

        // protected void UpdateAttachedVelocity(EquippedObject attachedObjectInfo)
        // {
        //     Vector3 velocityTarget, angularTarget;
        //     bool success = GetUpdatedAttachedVelocities(attachedObjectInfo, out velocityTarget, out angularTarget);
        //     if (success)
        //     {
        //         float scale = SteamVR_Utils.GetLossyScale(currentAttached.handAttachmentPointTransform);
                
        //         float maxAngularVelocityChange = MaxAngularVelocityChange * scale;
        //         float maxVelocityChange = MaxVelocityChange * scale;

        //         attachedObjectInfo.attachedRigidbody.velocity = Vector3.MoveTowards(attachedObjectInfo.attachedRigidbody.velocity, velocityTarget, maxVelocityChange);
        //         attachedObjectInfo.attachedRigidbody.angularVelocity = Vector3.MoveTowards(attachedObjectInfo.attachedRigidbody.angularVelocity, angularTarget, maxAngularVelocityChange);
        //     }
        // }

        // protected Vector3 TargetItemPosition(EquippedObject attachedObject)
        // {
        //     if (attachedObject.interactable != null && attachedObject.interactable.skeletonPoser != null && HasSkeleton())
        //     {
        //         Vector3 tp = attachedObject.handAttachmentPointTransform.InverseTransformPoint(transform.TransformPoint(attachedObject.interactable.skeletonPoser.GetBlendedPose(skeleton).position));
        //         //tp.x *= -1;
        //         return currentAttached.handAttachmentPointTransform.TransformPoint(tp);
        //     }
        //     else
        //     {
        //         return currentAttached.handAttachmentPointTransform.TransformPoint(attachedObject.initialPositionalOffset);
        //     }
        // }

        // protected Quaternion TargetItemRotation(EquippedObject attachedObject)
        // {
        //     if (attachedObject.interactable != null && attachedObject.interactable.skeletonPoser != null && HasSkeleton())
        //     {
        //         Quaternion tr = Quaternion.Inverse(attachedObject.handAttachmentPointTransform.rotation) * (transform.rotation * attachedObject.interactable.skeletonPoser.GetBlendedPose(skeleton).rotation);
        //         return currentAttached.handAttachmentPointTransform.rotation * tr;
        //     }
        //     else
        //     {
        //         return currentAttached.handAttachmentPointTransform.rotation * attachedObject.initialRotationalOffset;
        //     }
        // }

        // protected bool GetUpdatedAttachedVelocities(EquippedObject attachedObjectInfo, out Vector3 velocityTarget, out Vector3 angularTarget)
        // {
        //     bool realNumbers = false;


        //     float velocityMagic = VelocityMagic;
        //     float angularVelocityMagic = AngularVelocityMagic;

        //     Vector3 targetItemPosition = TargetItemPosition(attachedObjectInfo);
        //     Vector3 positionDelta = (targetItemPosition - attachedObjectInfo.attachedRigidbody.position);
        //     velocityTarget = (positionDelta * velocityMagic * Time.deltaTime);

        //     if (float.IsNaN(velocityTarget.x) == false && float.IsInfinity(velocityTarget.x) == false)
        //     {
        //         realNumbers = true;
        //     }
        //     else
        //         velocityTarget = Vector3.zero;


        //     Quaternion targetItemRotation = TargetItemRotation(attachedObjectInfo);
        //     Quaternion rotationDelta = targetItemRotation * Quaternion.Inverse(attachedObjectInfo.attachedObject.transform.rotation);


        //     float angle;
        //     Vector3 axis;
        //     rotationDelta.ToAngleAxis(out angle, out axis);

        //     if (angle > 180)
        //         angle -= 360;

        //     if (angle != 0 && float.IsNaN(axis.x) == false && float.IsInfinity(axis.x) == false)
        //     {
        //         angularTarget = angle * axis * angularVelocityMagic * Time.deltaTime;

        //         realNumbers &= true;
        //     }
        //     else
        //         angularTarget = Vector3.zero;

        //     return realNumbers;
        // }


        //-------------------------------------------------
        // protected virtual void OnInputFocus(bool hasFocus)
        // {
        //     Debug.Log("FOCUSING");
        //     if (hasFocus)
        //     {
        //         DetachObject(applicationLostFocusObject, true);
        //         applicationLostFocusObject.SetActive(false);
        //         UpdateHovering();
        //         BroadcastMessage("OnParentHandInputFocusAcquired", SendMessageOptions.DontRequireReceiver);
        //     }
        //     else
        //     {
        //         applicationLostFocusObject.SetActive(true);
        //         AttachObject(applicationLostFocusObject, AttachmentFlags.ParentToHand);
        //         BroadcastMessage("OnParentHandInputFocusLost", SendMessageOptions.DontRequireReceiver);
        //     }
        // }

        //-------------------------------------------------
        protected virtual void OnDrawGizmos()
        {
            if (useHoverSphere && hoverSphereTransform != null)
            {
                Gizmos.color = Color.green;
                float scaledHoverRadius = hoverSphereRadius * Mathf.Abs(SteamVR_Utils.GetLossyScale(hoverSphereTransform));
                Gizmos.DrawWireSphere(hoverSphereTransform.position, scaledHoverRadius/2);
            }

            if (useControllerHoverComponent && mainRenderModel != null && mainRenderModel.IsControllerVisibile())
            {
                Gizmos.color = Color.blue;
                float scaledHoverRadius = controllerHoverRadius * Mathf.Abs(SteamVR_Utils.GetLossyScale(this.transform));
                Gizmos.DrawWireSphere(mainRenderModel.GetControllerPosition(controllerHoverComponent), scaledHoverRadius/2);
            }

            if (useFingerJointHover && mainRenderModel != null && mainRenderModel.IsHandVisibile())
            {
                Gizmos.color = Color.yellow;
                float scaledHoverRadius = fingerJointHoverRadius * Mathf.Abs(SteamVR_Utils.GetLossyScale(this.transform));
                Gizmos.DrawWireSphere(mainRenderModel.GetBonePosition((int)fingerJointHover), scaledHoverRadius/2);
            }
        }


        //-------------------------------------------------
        private void HandDebugLog(string msg)
        {
            if (spewDebugText)
            {
                Debug.Log("<b>[SteamVR Interaction]</b> Hand (" + this.name + "): " + msg);
            }
        }


        //-------------------------------------------------
        // Continue to hover over this object indefinitely, whether or not the Hand moves out of its interaction trigger volume.
        //
        // interactable - The Interactable to hover over indefinitely.
        //-------------------------------------------------
        
        // public void HoverLock(Interactable interactable)
        // {
        //     if (spewDebugText)
        //         HandDebugLog("HoverLock " + interactable);
        //     hoverLocked = true;
        //     hoveringInteractable = interactable;
        // }


        //-------------------------------------------------
        // Stop hovering over this object indefinitely.
        //
        // interactable - The hover-locked Interactable to stop hovering over indefinitely.
        //-------------------------------------------------
        // public void HoverUnlock(Interactable interactable)
        // {
        //     if (spewDebugText)
        //         HandDebugLog("HoverUnlock " + interactable);

        //     if (hoveringInteractable == interactable)
        //     {
        //         hoverLocked = false;
        //     }
        // }

        // public void TriggerHapticPulse(ushort microSecondsDuration)
        // {
        //     float seconds = (float)microSecondsDuration / 1000000f;
        //     hapticAction.Execute(0, seconds, 1f / seconds, 1, handType);
        // }

        // public void TriggerHapticPulse(float duration, float frequency, float amplitude)
        // {
        //     hapticAction.Execute(0, duration, frequency, amplitude, handType);
        // }




        // void ShowGrabHint()
        // {
        //     ControllerButtonHints.ShowButtonHint(this, useAction); //todo: assess
        // }

        // void HideGrabHint()
        // {
        //     ControllerButtonHints.HideButtonHint(this, grabGripAction); //todo: assess
        // }

        // void ShowGrabHint(string text)
        // {
        //     ControllerButtonHints.ShowTextHint(this, grabGripAction, text);
        // }


        public bool GetGrabDown ( ) {
            return useAction.GetStateDown(handType);
        }
        public bool GetGrabUp ( ) {
            return useAction.GetStateUp(handType);
        }
        public bool IsGrabbing ( ) {
            return useAction.GetState(handType);
        }

        //-------------------------------------------------
        private void InitController()
        {
            //if (spewDebugText)
                HandDebugLog("Hand " + name + " connected with type " + handType.ToString());

            bool hadOldRendermodel = mainRenderModel != null;
            EVRSkeletalMotionRange oldRM_rom = EVRSkeletalMotionRange.WithController;
            if(hadOldRendermodel)
                oldRM_rom = mainRenderModel.GetSkeletonRangeOfMotion;


            foreach (RenderModel r in renderModels)
            {
                if (r != null)
                    Destroy(r.gameObject);
            }

            renderModels.Clear();

            GameObject renderModelInstance = GameObject.Instantiate(renderModelPrefab);
            renderModelInstance.layer = gameObject.layer;
            renderModelInstance.tag = gameObject.tag;
            renderModelInstance.transform.parent = this.transform;
            renderModelInstance.transform.localPosition = Vector3.zero;
            renderModelInstance.transform.localRotation = Quaternion.identity;
            renderModelInstance.transform.localScale = renderModelPrefab.transform.localScale;

            //TriggerHapticPulse(800);  //pulse on controller init

            int deviceIndex = trackedObject.GetDeviceIndex();

            mainRenderModel = renderModelInstance.GetComponent<RenderModel>();
            renderModels.Add(mainRenderModel);

            if (hadOldRendermodel)
                mainRenderModel.SetSkeletonRangeOfMotion(oldRM_rom);

            this.BroadcastMessage("SetInputSource", handType, SendMessageOptions.DontRequireReceiver); // let child objects know we've initialized
            Debug.Log("Set Input source " + handType);
            // Debug.Log("Set Input source " + mainRenderModel);
            
            
            this.BroadcastMessage("OnHandInitialized", deviceIndex, SendMessageOptions.DontRequireReceiver); // let child objects know we've initialized
        }

        public void SetRenderModel(GameObject prefab)
        {
            renderModelPrefab = prefab;

            if (mainRenderModel != null && isPoseValid)
                InitController();
        }

        // public void SetHoverRenderModel(RenderModel hoverRenderModel)
        // {
        //     hoverhighlightRenderModel = hoverRenderModel;
        //     renderModels.Add(hoverRenderModel);
        // }

        public int GetDeviceIndex()
        {
            return trackedObject.GetDeviceIndex();
        }
    }


    [System.Serializable]
    public class HandEvent : UnityEvent<Hand> { }


#if UNITY_EDITOR
    //-------------------------------------------------------------------------
    [UnityEditor.CustomEditor(typeof(Hand))]
    public class HandEditor : UnityEditor.Editor
    {
        //-------------------------------------------------
        // Custom Inspector GUI allows us to click from within the UI
        //-------------------------------------------------
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            /*
            Hand hand = (Hand)target;

            if (hand.otherHand)
            {
                if (hand.otherHand.otherHand != hand)
                {
                    UnityEditor.EditorGUILayout.HelpBox("The otherHand of this Hand's otherHand is not this Hand.", UnityEditor.MessageType.Warning);
                }

                if (hand.handType == SteamVR_Input_Sources.LeftHand && hand.otherHand && hand.otherHand.handType != SteamVR_Input_Sources.RightHand)
                {
                    UnityEditor.EditorGUILayout.HelpBox("This is a left Hand but otherHand is not a right Hand.", UnityEditor.MessageType.Warning);
                }

                if (hand.handType == SteamVR_Input_Sources.RightHand && hand.otherHand && hand.otherHand.handType != SteamVR_Input_Sources.LeftHand)
                {
                    UnityEditor.EditorGUILayout.HelpBox("This is a right Hand but otherHand is not a left Hand.", UnityEditor.MessageType.Warning);
                }

                if (hand.handType == SteamVR_Input_Sources.Any && hand.otherHand && hand.otherHand.handType != SteamVR_Input_Sources.Any)
                {
                    UnityEditor.EditorGUILayout.HelpBox("This is an any-handed Hand but otherHand is not an any-handed Hand.", UnityEditor.MessageType.Warning);
                }
            }
            */ //removing for now because it conflicts with other input sources (trackers and such)
        }
    }
#endif
}
