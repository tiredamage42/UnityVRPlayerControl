using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using InteractionSystem;
using Game.InventorySystem;
using Game.UI;
public class TradeUITrigger : MonoBehaviour, IInteractable
{
    public int useAction = 0;
    public Inventory suppliedInventory;
    [NeatArray] public NeatIntList categoryFilter;
    void Awake () {
        if (suppliedInventory == null) 
            suppliedInventory = GetComponent<Inventory>();
    }

    public void OnInteractableAvailabilityChange(bool available) { }
    public void OnInteractableInspectedStart (InteractionPoint interactor) { }
    public void OnInteractableInspectedEnd (InteractionPoint interactor) { }
    public void OnInteractableInspectedUpdate (InteractionPoint interactor) { }

    public void OnInteractableUsedStart (InteractionPoint interactor, int useAction) {
        if (useAction == this.useAction && interactor.inventory != null) {
            GameUI.tradeUI.OpenTradUI( interactor.inventory, suppliedInventory, categoryFilter );

            // UIHandler.GetUIHandlerByContext(GameObject.FindObjectOfType<UIObjectInitializer>().gameObject, contextName).OpenUI(
            //     new object[] { interactor.interactorID, suppliedInventory, categoryFilter.list }
            // );
        }
    }

    public void OnInteractableUsedEnd (InteractionPoint interactor, int useAction) { }
    public void OnInteractableUsedUpdate (InteractionPoint interactor, int useAction) { }

}

