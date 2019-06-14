//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: This object will get hover events and can be attached to the hands
//
//=============================================================================

using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace Valve.VR.InteractionSystem
{
    public class Interactable : MonoBehaviour
    {
        [Tooltip("Activates an action set on attach and deactivates on detach")]
        public SteamVR_ActionSet activateActionSetOnAttach;

        [Tooltip("Hide the whole hand on attachment and show on detach")]
        public bool hideHandOnAttach = true;

        [Tooltip("Hide the skeleton part of the hand on attachment and show on detach")]
        public bool hideSkeletonOnAttach = false;

        [Tooltip("Hide the controller part of the hand on attachment and show on detach")]
        public bool hideControllerOnAttach = false;

        [Tooltip("The integer in the animator to trigger on pickup. 0 for none")]
        public int handAnimationOnPickup = 0;

        [Tooltip("The range of motion to set on the skeleton. None for no change.")]
        public SkeletalMotionRangeChange setRangeOfMotionOnPickup = SkeletalMotionRangeChange.None;

        // public delegate void OnAttachedToHandDelegate(Hand hand);
        // public delegate void OnDetachedFromHandDelegate(Hand hand);

        // public event OnAttachedToHandDelegate onAttachedToHand;
        // public event OnDetachedFromHandDelegate onDetachedFromHand;

        public event System.Action<Object> onEquipped;
        public event System.Action<Object> onUnequipped;



        [Tooltip("Specify whether you want to snap to the hand's object attachment point, or just the raw hand")]
        public bool useHandObjectAttachmentPoint = true;

        public bool attachEaseIn = false;
        [HideInInspector]
        public AnimationCurve snapAttachEaseInCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);
        public float snapAttachEaseInTime = 0.15f;

        public bool snapAttachEaseInCompleted = false;


        // [Tooltip("The skeleton pose to apply when grabbing. Can only set this or handFollowTransform.")]
        [HideInInspector]
        public SteamVR_Skeleton_Poser skeletonPoser;

        [Tooltip("Should the rendered hand lock on to and follow the object")]
        public bool handFollowTransform= true;

        [Tooltip("Set whether or not you want this interactible to highlight when hovering over it")]
        public bool highlightOnHover = true;
        
        [Tooltip("An array of child gameObjects to not render a highlight for. Things like transparent parts, vfx, etc.")]
        public GameObject[] hideHighlight;


        HashSet<int> currentHoveringIDs = new HashSet<int>();
        bool hasOwner;
        int ownerID;


        [System.NonSerialized]
        public Hand attachedToHand;

        // [System.NonSerialized]
        // public Hand hoveringHand;

        // public bool isDestroying { get; protected set; }
        public bool isHovering { 
            get {
                return currentHoveringIDs.Count != 0;
            }
        // protected set; 
        }
        // public bool wasHovering { get; protected set; }
        bool isHighlighted;

        private void Awake()
        {
            skeletonPoser = GetComponent<SteamVR_Skeleton_Poser>();
        }

        protected virtual void Start()
        {
            if (skeletonPoser != null)
            {
                if (useHandObjectAttachmentPoint)
                {
                    Debug.LogWarning("<b>[SteamVR Interaction]</b> SkeletonPose and useHandObjectAttachmentPoint both set at the same time. Ignoring useHandObjectAttachmentPoint.");
                    useHandObjectAttachmentPoint = false;
                }
            }
        }

        protected virtual bool ShouldIgnoreHighlight(Component component)
        {
            return ShouldIgnore(component.gameObject);
        }

        protected virtual bool ShouldIgnore(GameObject check)
        {
            for (int ignoreIndex = 0; ignoreIndex < hideHighlight.Length; ignoreIndex++)
            {
                if (check == hideHighlight[ignoreIndex])
                    return true;
            }
            return false;
        }


        List<Renderer> _Renderers = new List<Renderer>();

        void SubmitForHighlight () {
            _Renderers.Clear();

            Renderer[] renderers = this.GetComponentsInChildren<Renderer>(true);
                        
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];

                if (ShouldIgnoreHighlight(renderer))
                    continue;

                _Renderers.Add(renderer);
            }

            ObjectOutlines.Highlight_Renderers( _Renderers, 0 );


            isHighlighted = true;
        }
        void UnHighlight() {
            if (isHighlighted) {
                isHighlighted = false;
                ObjectOutlines.UnHighlight_Renderers( _Renderers );
                _Renderers.Clear();

            }
        }


        /// <summary>
        /// Called when a Hand starts hovering over this object
        /// </summary>
        protected virtual void OnHandHoverBegin(Hand hand)
        {
            int handID = hand.GetInstanceID();
            currentHoveringIDs.Add(handID);

            // wasHovering = isHovering;
            // isHovering = true;

            // hoveringHand = hand;

            if (highlightOnHover == true)
            {
                SubmitForHighlight();
            }
        }


        /// <summary>
        /// Called when a Hand stops hovering over this object
        /// </summary>
        private void OnHandHoverEnd(Hand hand)
        {
            int handID = hand.GetInstanceID();
            currentHoveringIDs.Remove(handID);




            // wasHovering = isHovering;
            // isHovering = false;

            if (highlightOnHover)
                UnHighlight();
        }

        protected virtual void Update()
        {
            if (highlightOnHover)
            {
                if (!isHovering)
                    UnHighlight();
            }
        }
        

        protected float blendToPoseTime = 0.1f;
        protected float releasePoseBlendTime = 0.2f;


        //on equip
        protected virtual void OnAttachedToHand(Hand hand)
        {

            //move to vr interactable
            if (activateActionSetOnAttach != null)
                activateActionSetOnAttach.Activate(hand.handType);

            //do callback
            // if (onAttachedToHand != null)
            // {
            //     onAttachedToHand.Invoke(hand);
            // }

            //move to vr interactable
            if (skeletonPoser != null && hand.skeleton != null)
            {
                hand.skeleton.BlendToPoser(skeletonPoser, blendToPoseTime);
            }

            attachedToHand = hand;


            if (onEquipped != null) {
                onEquipped(hand);
            }
        }

        // on unequip
        protected virtual void OnDetachedFromHand(Hand hand)
        {

            //move to vr interactable
            if (activateActionSetOnAttach != null)
            {
                // if (hand.otherHand == null || hand.otherHand.currentAttachedObjectInfo.HasValue == false ||
                //     (hand.otherHand.currentAttachedObjectInfo.Value.interactable != null &&
                //      hand.otherHand.currentAttachedObjectInfo.Value.interactable.activateActionSetOnAttach != this.activateActionSetOnAttach))
                
                if (hand.otherHand == null || !hand.otherHand.hasCurrentAttached ||
                    (hand.otherHand.currentAttached.interactable != null &&
                     hand.otherHand.currentAttached.interactable.activateActionSetOnAttach != this.activateActionSetOnAttach))
                
                
                {
                    activateActionSetOnAttach.Deactivate(hand.handType);
                }
            }

            // do callback
            // if (onDetachedFromHand != null)
            // {
            //     onDetachedFromHand.Invoke(hand);
            // }

            //move to vr interacable
            if (skeletonPoser != null)
            {
                if (hand.skeleton != null)
                    hand.skeleton.BlendToSkeleton(releasePoseBlendTime);
            }

            attachedToHand = null;

            if (onUnequipped != null) {
                onUnequipped(hand);
            }
        }

        protected virtual void OnDestroy()
        {
            // isDestroying = true;

            if (attachedToHand != null)
            {
                attachedToHand.DetachObject(this.gameObject, false);
                attachedToHand.skeleton.BlendToSkeleton(0.1f);
            }

            
            UnHighlight();
        }


        protected virtual void OnDisable()
        {
            // isDestroying = true;

            if (attachedToHand != null)
            {
                attachedToHand.ForceHoverUnlock();
            }

            UnHighlight();
        }
    }
}
