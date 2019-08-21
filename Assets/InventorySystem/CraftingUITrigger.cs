using UnityEngine;

using InteractionSystem;
using Game.InventorySystem;
using Game.UI;

/*
    interactable that opens inventory ui context
*/
public class CraftingUITrigger : MonoBehaviour, IInteractable
{
    public int GetInteractionMode() { return 0; }
    
    public int useAction = 0;
    [NeatArray] public NeatIntList categoryFilter;
    
    public void OnInteractableAvailabilityChange(bool available) { }
    public void OnInteractableInspectedStart (InteractionPoint interactor) { }
    public void OnInteractableInspectedEnd (InteractionPoint interactor) { }
    public void OnInteractableInspectedUpdate (InteractionPoint interactor) { }

    public void OnInteractableUsedStart (InteractionPoint interactor, int useAction) {
        if (useAction == this.useAction && interactor.inventory != null) {
            GameUI.craftingUI.OpenCraftingUI(interactor.inventory, categoryFilter);
        }
    }

    public void OnInteractableUsedEnd (InteractionPoint interactor, int useAction) { }
    public void OnInteractableUsedUpdate (InteractionPoint interactor, int useAction) { }

}
