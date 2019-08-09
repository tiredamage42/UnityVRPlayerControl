using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace InteractionSystem {

public class InteractionPoint : MonoBehaviour
{
    Interactable _hoveringInteractable;
        const int ColliderArraySize = 16;
        Collider[] overlappingColliders;
        public Transform referenceTransform;
        
        public int interactorID = 0;

        
        public Interactor baseInteractor;
        [HideInInspector] public Vector3 lastInteractionPoint;
        

        

        void Awake () {
            overlappingColliders = new Collider[ColliderArraySize];
        }

        void FindInteractables () {
            if (hoverLocked)
                return;

            float closestDistance = float.MaxValue;
            Interactable closestInteractable = null;

            if (referenceTransform != null) {
                if (( baseInteractor.usePositionCheck )) {
                    UpdateHovering(
                        referenceTransform.position, 
                        baseInteractor.positionRadius, 
                        ref closestDistance, ref closestInteractable
                    );

                    if (closestInteractable != null) {
                        lastInteractionPoint = referenceTransform.position;
                    }
                }
                bool rayEnabled = closestInteractable == null && baseInteractor.useRayCheck;
                Vector3 hitPos = Vector3.zero;
                
                if (rayEnabled) {

                    UpdateRayCheck(referenceTransform.position, referenceTransform.forward, 
                    baseInteractor.rayCheckDistance , 
                    out hitPos, ref closestInteractable);
                    
                    if (closestInteractable != null) {
                        lastInteractionPoint = hitPos;
                    }

                    // VisualizeInteractionRay (referenceTransform.position, hitPos, closestInteractable != null);
                }
                else {
                    // DisableInteractionRay ();
                }

                if (onRayCheckUpdate != null) {
                    onRayCheckUpdate(rayEnabled, referenceTransform.position, hitPos, closestInteractable != null, interactorID);
                }

            }
            else {
                Debug.LogError("no interactor reference transform supplied for " + name);
            }
                
            // Hover on this one
            hoveringInteractable = closestInteractable;
        }


        public System.Action<bool, Vector3, Vector3, bool, int> onRayCheckUpdate;
        void UpdateRayCheck(Vector3 origin, Vector3 direction, float distance, out Vector3 hitPos, ref Interactable closestInteractable) {
            Ray ray = new Ray(origin, direction);
            RaycastHit hit;
            //maybe raycast all
            if (Physics.Raycast(ray, out hit, distance, Interactable.interactTriggerMask, QueryTriggerInteraction.Collide)){// baseInteractor.hoverLayerMask)) {
                
                hitPos = hit.point;
                InteractableElement c = hit.collider.GetComponent<InteractableElement>();
                if (c == null)
                    return;

                Interactable contacting = c.interactable;
                // Interactable contacting = hit.collider.GetComponentInParent<Interactable>();

                if (contacting == null || !contacting.available)
                    return;

                if (contacting.onlyProximityHover && !baseInteractor.godModeInteractor)
                    return;
                
                closestInteractable = contacting;
            }
            else {
                hitPos = origin + direction * distance;
            }
        }
        void UpdateHovering(Vector3 hoverPosition, float hoverRadius, ref float closestDistance, ref Interactable closestInteractable)
        {
           
            // null out old vals
            for (int i = 0; i < overlappingColliders.Length; ++i)
            {
                overlappingColliders[i] = null;
            }

            int numColliding = Physics.OverlapSphereNonAlloc(hoverPosition, hoverRadius, overlappingColliders, Interactable.interactTriggerMask, QueryTriggerInteraction.Collide);// baseInteractor.hoverLayerMask.value);

            if (numColliding == ColliderArraySize)
                Debug.LogWarning("<b>[SteamVR Interaction]</b> This hand is overlapping the max number of colliders: " + ColliderArraySize + ". Some collisions may be missed. Increase ColliderArraySize on Hand.cs");

            // Pick the closest hovering
            for (int colliderIndex = 0; colliderIndex < numColliding; colliderIndex++)
            {
                InteractableElement c = overlappingColliders[colliderIndex].GetComponent<InteractableElement>();
                if (c == null)
                    continue;

                Interactable contacting = c.interactable;
                
                // Interactable contacting = overlappingColliders[colliderIndex].GetComponentInParent<Interactable>();

                // Yeah, it's null, skip
                if (contacting == null || !contacting.available)
                    continue;

                // Best candidate so far...
                float distance = Vector3.Distance(contacting.transform.position, hoverPosition);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInteractable = contacting;
                }
            }
        }
        public bool findInteractables;
        void Update()
        {         
            if (findInteractables) {

                FindInteractables();
            }   
         
            if (hoveringInteractable)
            {  
                hoveringInteractable.OnInspectedUpdate(this);

                if (onInspectUpdate != null){
                    onInspectUpdate(this, hoveringInteractable);
                }
            }


        }

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
                        oldInteractable.OnInspectedEnd(this);
                        if (onInspectEnd != null) {
                            onInspectEnd(this, oldInteractable);
                        }
                    }

                    _hoveringInteractable = newInteractable;

                    if (newInteractable != null)
                    {
                        newInteractable.OnInspectedStart(this);
                        if (onInspectStart != null) {
                            onInspectStart(this, newInteractable);
                        }
                    }
                }
            }
        }

        public void ForceHoverUnlock()
        {
            hoverLocked = false;
        }

        // Continue to hover over this object indefinitely, whether or not the Hand moves out of its interaction trigger volume.
        public void HoverLock(Interactable interactable)
        {
            hoverLocked = true;
            hoveringInteractable = interactable;
        }

        // Stop hovering over this object indefinitely.
        public void HoverUnlock(Interactable interactable)
        {
            if (hoveringInteractable == interactable)
                hoverLocked = false;
        }

        public void OnUseStart (int useIndex) {
            if (hoveringInteractable != null) {
                bool isUseable = hoveringInteractable.useType != Interactable.UseType.Scripted;
                if (isUseable) {
                    hoveringInteractable.OnUsedStart(this, useIndex);
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
                    hoveringInteractable.OnUsedEnd(this, useIndex);
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
                
                hoveringInteractable.OnUsedUpdate(this, useIndex);
                }
            }
            if (onUseUpdate != null) {
                onUseUpdate (this, useIndex, hoveringInteractable);
            }
        }
        public void SetBaseInteractor (Interactor baseInteractor) {
            this.baseInteractor = baseInteractor;
        }

        public void RemoveInteractionTags (List<string> tags) {
            baseInteractor.RemoveInteractionTags(BuildSuffixedTags(tags));   
            
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
            baseInteractor.AddInteractionTags(BuildSuffixedTags(tags));   
            
        }
        public bool hoverLocked { get; private set; }

public string extraSuffix;
        public System.Action<InteractionPoint, Interactable> onInspectUpdate, onInspectStart, onInspectEnd;
        public System.Action<InteractionPoint, int, Interactable> onUseUpdate, onUseStart, onUseEnd;
        
        



}

}