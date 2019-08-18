using UnityEngine;
using InteractionSystem;
using Game.InventorySystem;
using Game.GameUI;
/*
    interactable that lets player user quick trade (trhough ui) with attached inventory
*/
public class QuickLootable : MonoBehaviour, IInteractable
{
    public Inventory linkedInventory;
    void Awake () {
        if (linkedInventory == null) 
            linkedInventory = GetComponent<Inventory>();
    }

    public void OnInteractableAvailabilityChange(bool available) { }
    public void OnInteractableInspectedStart (InteractionPoint interactor) {
        if (linkedInventory != null && interactor.inventory != null) {
            UIHandler.GetUIHandlerByContext(GameObject.FindObjectOfType<UIObjectInitializer>().gameObject, Inventory.quickTradeContext).OpenUI(
                new object[] { interactor.inventory, interactor.interactorID, linkedInventory, null }
            );
        }
    }
    public void OnInteractableInspectedEnd (InteractionPoint interactor) {
        if (linkedInventory != null && interactor.inventory != null)  {
            UIHandler.GetUIHandlerByContext(GameObject.FindObjectOfType<UIObjectInitializer>().gameObject, Inventory.quickTradeContext).CloseUI(
                new object[] { interactor.inventory, interactor.interactorID, linkedInventory, null }
            );
        }
    }

    public void OnInteractableInspectedUpdate (InteractionPoint interactor) { }
    public void OnInteractableUsedStart (InteractionPoint interactor, int useAction) { }
    public void OnInteractableUsedEnd (InteractionPoint interactor, int useAction) { }
    public void OnInteractableUsedUpdate (InteractionPoint interactor, int useAction) { }
}
