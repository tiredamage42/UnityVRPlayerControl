using System.Collections.Generic;
using UnityEngine;
using Game.InventorySystem;
namespace InteractionSystem {
    public class InteractionPoint : MonoBehaviour
    {
        public Transform referenceTransform;
        public string extraSuffix;
        public int interactorID = 0;
        public Interactor baseInteractor;
        public Inventory inventory;
        public bool findInteractables;
        
        public System.Action<InteractionPoint, Interactable> onInspectUpdate, onInspectStart, onInspectEnd;
        // public System.Action<InteractionPoint, int, Interactable> onUseUpdate, onUseStart, onUseEnd;
        [HideInInspector] public Vector3 lastInteractionPoint;
        public bool hoverLocked { get; private set; }
        
        Interactable _hoveringInteractable;
        const int ColliderArraySize = 16;
        Collider[] overlappingColliders;
        
        
        void Awake () {
            overlappingColliders = new Collider[ColliderArraySize];
        }
        public void SetBaseInteractor (Interactor baseInteractor) {
            this.baseInteractor = baseInteractor;
            inventory = baseInteractor.GetComponent<Inventory>();
        }
        

        void FindInteractables () {
            if (hoverLocked)
                return;

            if (referenceTransform == null) {
                Debug.LogError("no interactor reference transform supplied for " + name);
                return;
            }

            Interactable closestInteractable = null;

            float closestDistance = float.MaxValue;
            
            // use check around the interactors position
            if (baseInteractor.usePositionCheck ) {
                UpdateHovering (referenceTransform.position, baseInteractor.positionRadius, ref closestDistance, ref closestInteractable);

                if (closestInteractable != null) {
                    lastInteractionPoint = referenceTransform.position;
                }
            }

            bool rayEnabled = closestInteractable == null && baseInteractor.useRayCheck;
            Vector3 hitPos = Vector3.zero;
            
            if (rayEnabled) {
                UpdateRayCheck(referenceTransform.position, referenceTransform.forward, baseInteractor.rayCheckDistance, out hitPos, ref closestInteractable);
                
                if (closestInteractable != null) {
                    lastInteractionPoint = hitPos;
                }
            }
            
            //for visualization....
            if (onRayCheckUpdate != null) {
                onRayCheckUpdate(rayEnabled, referenceTransform.position, hitPos, closestInteractable != null, interactorID);
            }

            // Hover on this one
            hoveringInteractable = closestInteractable;
        }

        public System.Action<bool, Vector3, Vector3, bool, int> onRayCheckUpdate;

        
        void UpdateHovering(Vector3 hoverPosition, float hoverRadius, ref float closestDistance, ref Interactable closestInteractable)
        {
           
            // null out old vals
            for (int i = 0; i < overlappingColliders.Length; ++i) overlappingColliders[i] = null;
            
            int numColliding = Physics.OverlapSphereNonAlloc(hoverPosition, hoverRadius, overlappingColliders, Interactable.interactTriggerMask, QueryTriggerInteraction.Collide);

            if (numColliding == ColliderArraySize)
                Debug.LogWarning("overlapping the max number of colliders: " + ColliderArraySize + ". Some collisions may be missed. Increase ColliderArraySize on InteractionPoint.cs");

            // Pick the closest hovering
            for (int colliderIndex = 0; colliderIndex < numColliding; colliderIndex++)
            {
                InteractableElement c = overlappingColliders[colliderIndex].GetComponent<InteractableElement>();
                if (c == null)
                    continue;

                Interactable contacting = c.interactable;
                
                if (contacting == null || !contacting.IsAvailable(baseInteractor.interactionMode))
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
        void UpdateRayCheck(Vector3 origin, Vector3 direction, float distance, out Vector3 hitPos, ref Interactable closestInteractable) {
            RaycastHit hit;
            //maybe raycast all
            if (Physics.Raycast(new Ray(origin, direction), out hit, distance, Interactable.interactTriggerMask, QueryTriggerInteraction.Collide)){
                
                hitPos = hit.point;
                InteractableElement c = hit.collider.GetComponent<InteractableElement>();
                if (c == null)
                    return;

                Interactable contacting = c.interactable;
                if (contacting == null || !contacting.IsAvailable(baseInteractor.interactionMode))
                    return;

                if (contacting.onlyProximityHover && !baseInteractor.godModeInteractor)
                    return;
                
                closestInteractable = contacting;
            }
            else {
                hitPos = origin + direction * distance;
            }
        }
        void Update()
        {         
            if (findInteractables) {
                FindInteractables();
            }   
            else {
                hoveringInteractable = null;
                hoverLocked = false;
            }
         
            if (hoveringInteractable)
            {  
                hoveringInteractable.OnInspectedUpdate(this, baseInteractor.interactionMode);

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
                        oldInteractable.OnInspectedEnd(this, baseInteractor.interactionMode);
                        if (onInspectEnd != null) {
                            onInspectEnd(this, oldInteractable);
                        }
                    }

                    _hoveringInteractable = newInteractable;

                    if (newInteractable != null)
                    {
                        newInteractable.OnInspectedStart(this, baseInteractor.interactionMode);
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

        public void OnActionStart (int interactionMode, int actionIndex) {
            if (hoveringInteractable != null) {
                if (hoveringInteractable.useType != Interactable.UseType.Scripted) {
                    hoveringInteractable.OnUsedStart(this, interactionMode, actionIndex);
                }
            }
        }
        public void OnActionEnd (int interactionMode, int actionIndex) {
            if (hoveringInteractable != null) {
                if (hoveringInteractable.useType != Interactable.UseType.Scripted) {
                    hoveringInteractable.OnUsedEnd(this, interactionMode, actionIndex);
                }
            }
        }
        public void OnActionUpdate (int interactionMode, int actionIndex) {
            if (hoveringInteractable != null) {
                if (hoveringInteractable.useType != Interactable.UseType.Scripted) {
                    hoveringInteractable.OnUsedUpdate(this, interactionMode, actionIndex);
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
            baseInteractor.AddInteractionTags(BuildSuffixedTags(tags));   
        }
        public void RemoveInteractionTags (List<string> tags) {
            baseInteractor.RemoveInteractionTags(BuildSuffixedTags(tags));      
        }   
    }
}