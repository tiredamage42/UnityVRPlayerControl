using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RenderTricks;
using InteractionSystem;

[RequireComponent(typeof(Interactable))]
public class HighlightOnWorkshopHover : MonoBehaviour, IInteractable
{

    // highlighting interactables with outline when in workshop mode
    public int GetInteractionMode() { return 1; }
    
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
    }
    
    void Update()
    {
        if (!interactable.isHovering)
            UnHighlight();
        
    }
    

    public void OnInteractableAvailabilityChange(bool available) {
        
    }

    public void OnInteractableInspectedStart (InteractionPoint interactor) {
        SubmitForHighlight();
    }

    public void OnInteractableInspectedEnd(InteractionPoint interactor) { }
    public void OnInteractableInspectedUpdate(InteractionPoint interactor) { }
    public void OnInteractableUsedStart(InteractionPoint interactor, int useIndex) { }
    public void OnInteractableUsedEnd(InteractionPoint interactor, int useIndex) { }
    public void OnInteractableUsedUpdate(InteractionPoint interactor, int useIndex) { }
    
   
    void OnDestroy()
    {
        UnHighlight();
    }

    void OnDisable()
    {
        UnHighlight();
    }

    
}
