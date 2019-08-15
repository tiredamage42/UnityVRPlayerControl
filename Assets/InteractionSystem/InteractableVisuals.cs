using System.Collections.Generic;
using UnityEngine;

using RenderTricks;
namespace InteractionSystem {
    [RequireComponent(typeof(Interactable))]
    public class InteractableVisuals : MonoBehaviour, IInteractable
    {

        /*
            getting renderer.material instantiates a new material

            which disables the ability to use gpu instancing / batching

            so instead of handling emission flashing per renderer material

            we create a second version of each material that interactables use, 
            that uses emission, and change out each renderer's .sharedMaterial

            this way we dont have multiple versions of the same material we dont need
        */

        static Dictionary<Material, Material> nonFlash2Flash = new Dictionary<Material, Material>();
        static Dictionary<Material, Material> flash2NonFlash = new Dictionary<Material, Material>();

        static void SubmitMaterial (Material material, EmissionFlasher interactableEmissionFlasher) {
            if (!nonFlash2Flash.ContainsKey(material)) {
                Material emissionVersion = new Material(material);
                emissionVersion.EnableKeyword("_EMISSION");
                interactableEmissionFlasher.AddMaterial(emissionVersion);

                nonFlash2Flash.Add(material, emissionVersion);
                flash2NonFlash.Add(emissionVersion, material);
            }
        }

        const string emissionFlasherName = "Interactable Flasher";

        void InitializeMaterialsFlasher () {
            EmissionFlasher interactableEmissionFlasher = EmissionFlasher.GetFlasherByName(emissionFlasherName);
            if (interactableEmissionFlasher == null) {
                return;
            }

            for (int i = 0; i < renderers.Length; i++) {
                SubmitMaterial ( renderers[i].sharedMaterial, interactableEmissionFlasher );
            }
        }

        public bool flashInteractable = true;
        
        [Tooltip("Set whether or not you want this interactible to highlight when hovering over it")]
        public bool highlightOnHover = true;
        

        public Renderer[] flashRenderers;
        Renderer[] renderers;
        Interactable interactable;
        bool isHighlighted;

        void Awake () {
            interactable = GetComponent<Interactable>();

            if (flashRenderers == null || flashRenderers.Length == 0) {
                renderers = GetComponentsInChildren<Renderer>();
            }
            else {
                renderers = flashRenderers;
            }

            InitializeMaterialsFlasher();
        }

        void SubmitForHighlight () {
            if (!isHighlighted) {
                ObjectOutlines.Highlight_Renderers( renderers, 0 );
                isHighlighted = true;
            }
        }
        void UnHighlight() {
            if (isHighlighted) {
                isHighlighted = false;
                ObjectOutlines.UnHighlight_Renderers( renderers );
            }
        }

        void Update()
        {
            if (highlightOnHover)
            {
                if (!interactable.isHovering)
                    UnHighlight();
            }
        }
        protected virtual void OnDestroy()
        {
            UnHighlight();
        }

        protected virtual void OnDisable()
        {
            UnHighlight();
        }


        void EnableFlashing (bool enabled) {
            for (int i = 0; i < renderers.Length; i++) {

                Material materials = renderers[i].sharedMaterial;
                if (enabled) {
                    // if it's not in the keys, we're already in our flashing state (hopefully...)
                    if (nonFlash2Flash.ContainsKey(materials)) {
                        materials = nonFlash2Flash[materials];
                    }
                }
                else {
                    if (flash2NonFlash.ContainsKey(materials)) {
                        materials = flash2NonFlash[materials];
                    }
                }
                renderers[i].sharedMaterial = materials;
            }
        }

        public void OnInteractableAvailabilityChange(bool available) {
            if (flashInteractable) {
                EnableFlashing(available);
            }
        }

        public void OnInteractableInspectedStart (InteractionPoint interactor) {
            if (highlightOnHover) {
                SubmitForHighlight();
            }
        }

        public void OnInteractableInspectedEnd(InteractionPoint interactor) { }
        public void OnInteractableInspectedUpdate(InteractionPoint interactor) { }
        public void OnInteractableUsedStart(InteractionPoint interactor, int useIndex) { }
        public void OnInteractableUsedEnd(InteractionPoint interactor, int useIndex) { }
        public void OnInteractableUsedUpdate(InteractionPoint interactor, int useIndex) { }
    }
}
