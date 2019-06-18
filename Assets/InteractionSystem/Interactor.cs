using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem;

namespace InteractionSystem {

    public class Interactor : MonoBehaviour
    {
        public System.Action<Interactor, Interactable> onInspectUpdate, onInspectStart, onInspectEnd;
        public System.Action<Interactor, int, Interactable> onUseUpdate, onUseStart, onUseEnd;
        
        public string extraSuffix;
        List<string> interactionTags = new List<string>();
        Interactor baseInteractor;
        public bool hoverLocked { get; private set; }
        private Interactable _hoveringInteractable;

        // private int prevOverlappingColliders = 0;

        private const int ColliderArraySize = 16;
        private Collider[] overlappingColliders;

        public void ForceHoverUnlock()
        {
            hoverLocked = false;
        }

        // -------------------------------------------------
        // Continue to hover over this object indefinitely, whether or not the Hand moves out of its interaction trigger volume.
        
        // interactable - The Interactable to hover over indefinitely.
        // -------------------------------------------------
        
        public void HoverLock(Interactable interactable)
        {
            // if (spewDebugText)
            //     HandDebugLog("HoverLock " + interactable);
            hoverLocked = true;
            hoveringInteractable = interactable;
        }


        // -------------------------------------------------
        // Stop hovering over this object indefinitely.
        
        // interactable - The hover-locked Interactable to stop hovering over indefinitely.
        // -------------------------------------------------
        public void HoverUnlock(Interactable interactable)
        {
            // if (spewDebugText)
            //     HandDebugLog("HoverUnlock " + interactable);

            if (hoveringInteractable == interactable)
            {
                hoverLocked = false;
            }
        }

        void Start () {
            // allocate array for colliders
            overlappingColliders = new Collider[ColliderArraySize];

        }







        //-------------------------------------------------
        // The Interactable object this Hand is currently hovering over
        //-------------------------------------------------
        public Interactable hoveringInteractable
        {
            get { return _hoveringInteractable; }
            set
            {
                Interactable oldInteractable = _hoveringInteractable;
                Interactable newInteractable = value;
                if (oldInteractable != value)
                {
                    if (oldInteractable != null)
                    {
                        // if (spewDebugText)
                        //     HandDebugLog("HoverEnd " + oldInteractable.gameObject);

                        
                        oldInteractable.OnInspectEnd(this);
                        // oldInteractable.SendMessage("OnHandHoverEnd", this, SendMessageOptions.DontRequireReceiver);
                        if (onInspectEnd != null) {
                            onInspectEnd(this, oldInteractable);
                        }



                        // StandardizedVRInput.instance.HideHint(this, useAction);

                        // //if its a ui element
                        // InputModule.instance.HoverEnd( oldInteractable.gameObject );
                    }

                    _hoveringInteractable = newInteractable;

                    if (newInteractable != null)
                    {
                        // if (spewDebugText)
                        //     HandDebugLog("HoverBegin " + newInteractable.gameObject);
                        //Debug.LogError("Found interactable start hover");

                        newInteractable.OnInspectStart(this);
                        if (onInspectStart != null) {
                            onInspectStart(this, newInteractable);
                        }
                        // newInteractable.SendMessage("OnHandHoverBegin", this, SendMessageOptions.DontRequireReceiver);
                        //if it's useable... cant think of any time it wouldnt be
                        // StandardizedVRInput.instance.ShowHint(this, useAction);

                        // //if its a ui element
                        // InputModule.instance.HoverBegin( newInteractable.gameObject );
                        
                        
                    }
                }
            }
        }


        public void OnUseStart (int useIndex) {
            if (hoveringInteractable != null) {
                bool isUseable = hoveringInteractable.useType != Interactable.UseType.Scripted;
                if (isUseable) {
                    hoveringInteractable.OnUseStart(this, useIndex);
                }
            }
            if (onUseStart != null) {
                onUseStart (this, useIndex, hoveringInteractable);
            }
        }
        public void OnUseEnd (int useIndex) {
            if (hoveringInteractable != null) {
                bool isUseable = hoveringInteractable.useType != Interactable.UseType.Scripted;
                if (isUseable) {
                
                hoveringInteractable.OnUseEnd(this, useIndex);
                }
            }
            if (onUseEnd != null) {
                onUseEnd (this, useIndex, hoveringInteractable);
            }
        }
        public void OnUseUpdate (int useIndex) {
            if (hoveringInteractable != null) {
                bool isUseable = hoveringInteractable.useType != Interactable.UseType.Scripted;
                if (isUseable) {
                
                hoveringInteractable.OnUseUpdate(this, useIndex);
                }
            }
            if (onUseUpdate != null) {
                onUseUpdate (this, useIndex, hoveringInteractable);
            }
        }
        



        void Update()
        {
            Interactable hoveringInteractable = this.hoveringInteractable;
            
            if (hoveringInteractable)
            {  
                hoveringInteractable.OnInspectUpdate(this);

                if (onInspectUpdate != null){
                    onInspectUpdate(this, hoveringInteractable);
                }

            }

            UpdateHovering();
        }

        public Vector3 interactionPoint {
            get {
                return new Vector3 ( hoverCheckPositionsAndRadii[0].x, hoverCheckPositionsAndRadii[0].y, hoverCheckPositionsAndRadii[0].z );
            }
        }

        Vector4[] hoverCheckPositionsAndRadii;

        public void SetHoverCheckPositionsAndRadii (Vector4[] checks) {
            this.hoverCheckPositionsAndRadii = checks;
        }


        void UpdateHovering()
        {
            if (hoverLocked)
                return;

            
            float closestDistance = float.MaxValue;
            Interactable closestInteractable = null;

            for (int i =0 ; i < hoverCheckPositionsAndRadii.Length; i++) {
                Vector3 hoverPosition = new Vector3 ( hoverCheckPositionsAndRadii[i].x, hoverCheckPositionsAndRadii[i].y, hoverCheckPositionsAndRadii[i].z );
                float hoverRadius = hoverCheckPositionsAndRadii[i].w;

                CheckHoveringForTransform(hoverPosition, hoverRadius, ref closestDistance, ref closestInteractable);
            }

            // if (closestInteractable != null) {
            //     Debug.Log(closestInteractable);
            // }

            // Hover on this one
            hoveringInteractable = closestInteractable;
        }
        public LayerMask hoverLayerMask = -1;
        

        bool CheckHoveringForTransform(Vector3 hoverPosition, float hoverRadius, ref float closestDistance, ref Interactable closestInteractable)
        {
            bool foundCloser = false;

            // null out old vals
            for (int i = 0; i < overlappingColliders.Length; ++i)
            {
                overlappingColliders[i] = null;
            }

            int numColliding = Physics.OverlapSphereNonAlloc(hoverPosition, hoverRadius, overlappingColliders, hoverLayerMask.value);

            if (numColliding == ColliderArraySize)
                Debug.LogWarning("<b>[SteamVR Interaction]</b> This hand is overlapping the max number of colliders: " + ColliderArraySize + ". Some collisions may be missed. Increase ColliderArraySize on Hand.cs");

            // DebugVar
            // int iActualColliderCount = 0;

            // Pick the closest hovering
            for (int colliderIndex = 0; colliderIndex < overlappingColliders.Length; colliderIndex++)
            {
                Collider collider = overlappingColliders[colliderIndex];

                if (collider == null)
                    continue;

                Interactable contacting = collider.GetComponentInParent<Interactable>();

                // Yeah, it's null, skip
                if (contacting == null)
                    continue;

                // Can't hover over the object if it's attached
                Inventory myInventory = GetComponent<Inventory>();
                if (myInventory.equippedItem != null && myInventory.equippedItem.item == contacting.gameObject)
                    continue;
                
                // Occupied by another hand, so we can't touch it
                // if (otherHand && otherHand.hoveringInteractable == contacting)
                //     continue;

                // Best candidate so far...
                float distance = Vector3.Distance(contacting.transform.position, hoverPosition);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInteractable = contacting;
                    foundCloser = true;
                }
                // iActualColliderCount++;
            }

    
            // if (iActualColliderCount > 0 && iActualColliderCount != prevOverlappingColliders)
            // {
            //     prevOverlappingColliders = iActualColliderCount;

            //     if (spewDebugText)
            //         HandDebugLog("Found " + iActualColliderCount + " overlapping colliders.");
            // }

            return foundCloser;
        }

















        public bool HasTag (string tag) {
            return interactionTags.Contains(tag);
        }
        
        void Awake () {
            Interactor[] childInteractors = GetComponentsInChildren<Interactor>();
            for (int i =0 ; i < childInteractors.Length; i++) {
                if (childInteractors[i] != this) 
                    childInteractors[i].SetBaseInteractor(this);
            }
        }
        
        void SetBaseInteractor (Interactor baseInteractor) {
            this.baseInteractor = baseInteractor;
        }

        public void RemoveInteractionTags (List<string> tags) {
            if (baseInteractor != null) {
                baseInteractor.RemoveInteractionTags(BuildSuffixedTags(tags));   
            }
            else {
                for (int i = 0; i < tags.Count; i++) {
                    interactionTags.Remove(tags[i]);
                }
            }
        }
        List<string> BuildSuffixedTags(List<string> tags) {
            if (extraSuffix == null || extraSuffix == string.Empty) {
                return tags;
            }
            List<string> forBase = new List<string>();
            for (int i =0; i < tags.Count; i++) {
                forBase.Add(tags[i] + extraSuffix);
            }
            return forBase;
            
        }


        public void AddInteractionTags (List<string> tags) {
            if (baseInteractor != null) {
                baseInteractor.AddInteractionTags(BuildSuffixedTags(tags));   
            }
            else {
                interactionTags.AddRange(tags);

                if (interactionTags.Count > 25) {
                    Debug.LogError(name + " interaction tags getting bloated");
                }
            }
        }
    }
}
