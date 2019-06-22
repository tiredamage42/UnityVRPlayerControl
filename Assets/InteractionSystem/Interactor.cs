using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InteractionSystem {

    public class Interactor : MonoBehaviour
    {
        public System.Action<Interactor, Interactable> onInspectUpdate, onInspectStart, onInspectEnd;
        public System.Action<Interactor, int, Interactable> onUseUpdate, onUseStart, onUseEnd;
        
        public string extraSuffix;
        List<string> interactionTags = new List<string>();
        Interactor baseInteractor;
        public bool hoverLocked { get; private set; }

        Interactable _hoveringInteractable;
        const int ColliderArraySize = 16;
        Collider[] overlappingColliders;
        public Transform referenceTransform;
        
        [Header("The following are ignored when using a base interactor")]
        public float interactRayWidth = .1f;
        public Material interactRayMaterial;
        public Color interactRayColor = Color.green, interactRayNullColor = new Color(.5f, .5f, .5f, .25f);
        public bool usePositionCheck = true;
        public float positionRadius = .1f;
        public bool useRayCheck = true;
        public float rayCheckDistance = 1f;
        LineRenderer interactRay;

        
        void BuildLineRenderer () {
            interactRay = gameObject.AddComponent<LineRenderer>();
            interactRay.sharedMaterial = baseInteractor != null ? baseInteractor.interactRayMaterial : interactRayMaterial;
           
        }


        void VisualizeInteractionRay (Vector3 start, Vector3 end, bool isHitting) {
            if (interactRay == null) {
                BuildLineRenderer();
            }
            interactRay.enabled = true;
            float width = baseInteractor != null ? baseInteractor.interactRayWidth : interactRayWidth;
            interactRay.startWidth = width;
            interactRay.endWidth = width;

            Color color = isHitting ? (baseInteractor != null ? baseInteractor.interactRayColor : interactRayColor) : (baseInteractor != null ? baseInteractor.interactRayNullColor : interactRayNullColor);

            interactRay.startColor = color;
            interactRay.endColor = color;

            interactRay.SetPosition(0, start);
            interactRay.SetPosition(1, end);
        }
        void DisableInteractionRay() {
            interactRay.enabled = false;
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

        void Start () {
            // allocate array for colliders
            overlappingColliders = new Collider[ColliderArraySize];
        }

        void FindInteractables () {
            if (hoverLocked)
                return;

            float closestDistance = float.MaxValue;
            Interactable closestInteractable = null;

            if (referenceTransform != null) {
                if ((baseInteractor != null ? baseInteractor.usePositionCheck : usePositionCheck)) {
                    UpdateHovering(
                        referenceTransform.position, 
                        baseInteractor != null ? baseInteractor.positionRadius : positionRadius, 
                        ref closestDistance, ref closestInteractable
                    );

                    if (closestInteractable != null) {
                        lastInteractionPoint = referenceTransform.position;
                    }
                }

                if (closestInteractable == null && (baseInteractor != null ? baseInteractor.useRayCheck : useRayCheck)) {

                    Vector3 hitPos;
                    UpdateRayCheck(referenceTransform.position, referenceTransform.forward, 
                    baseInteractor != null ? baseInteractor.rayCheckDistance : rayCheckDistance, 
                    out hitPos, ref closestInteractable);
                    
                    if (closestInteractable != null) {
                        lastInteractionPoint = hitPos;
                    }

                    VisualizeInteractionRay (referenceTransform.position, hitPos, closestInteractable != null);
                }
                else {
                    DisableInteractionRay ();
                }

            }
            else {
                Debug.LogError("no interactor reference transform supplied for " + name);
            }
                
            // Hover on this one
            hoveringInteractable = closestInteractable;
        }

        void UpdateRayCheck(Vector3 origin, Vector3 direction, float distance, out Vector3 hitPos, ref Interactable closestInteractable) {
            Ray ray = new Ray(origin, direction);

            RaycastHit hit;
            hitPos = origin + direction * distance;

            //maybe raycast all
            if (Physics.Raycast(ray, out hit, distance, hoverLayerMask)) {
                
                hitPos = hit.point;

                Interactable contacting = hit.collider.GetComponentInParent<Interactable>();
                if (contacting == null || !contacting.isAvailable)
                    return;

            
                closestInteractable = contacting;
            }
        }

        void UpdateHovering(Vector3 hoverPosition, float hoverRadius, ref float closestDistance, ref Interactable closestInteractable)
        {
           
            // null out old vals
            for (int i = 0; i < overlappingColliders.Length; ++i)
            {
                overlappingColliders[i] = null;
            }

            int numColliding = Physics.OverlapSphereNonAlloc(hoverPosition, hoverRadius, overlappingColliders, hoverLayerMask.value);

            if (numColliding == ColliderArraySize)
                Debug.LogWarning("<b>[SteamVR Interaction]</b> This hand is overlapping the max number of colliders: " + ColliderArraySize + ". Some collisions may be missed. Increase ColliderArraySize on Hand.cs");

            // Pick the closest hovering
            for (int colliderIndex = 0; colliderIndex < numColliding; colliderIndex++)
            {
                Interactable contacting = overlappingColliders[colliderIndex].GetComponentInParent<Interactable>();

                // Yeah, it's null, skip
                if (contacting == null || !contacting.isAvailable)
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




        // The Interactable object this Hand is currently hovering over
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
                        oldInteractable.OnInspectEnd(this);
                        if (onInspectEnd != null) {
                            onInspectEnd(this, oldInteractable);
                        }
                    }

                    _hoveringInteractable = newInteractable;

                    if (newInteractable != null)
                    {
                        newInteractable.OnInspectStart(this);
                        if (onInspectStart != null) {
                            onInspectStart(this, newInteractable);
                        }
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
        public bool findInteractables;
        void Update()
        {         
            if (findInteractables) {

                FindInteractables();
            }   
         
            if (hoveringInteractable)
            {  
                hoveringInteractable.OnInspectUpdate(this);

                if (onInspectUpdate != null){
                    onInspectUpdate(this, hoveringInteractable);
                }
            }


        }

        public Vector3 lastInteractionPoint;
        public LayerMask hoverLayerMask = -1;

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
