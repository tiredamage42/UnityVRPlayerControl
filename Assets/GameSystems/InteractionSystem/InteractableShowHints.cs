using System.Collections.Generic;
using UnityEngine;
using InteractionSystem;
using Game.InventorySystem;
using Game.UI;
// namespace InteractionSystem {}



/*
    interactable that lets player user quick trade (trhough ui) with attached inventory
*/
public class InteractableShowHints : MonoBehaviour, IInteractable
{
    public int interactionMode;
    public string interactionMessage;

    string getInteractionMessage {
        get {
            if (sceneItem != null) {
                return sceneItem.itemBehavior.itemName;
            }
            return interactionMessage;
        }
    }
    
    public int GetInteractionMode() { return interactionMode; }

    [Header("EG: 'Drag-0'")]
    [NeatArray] public NeatStringArray nameAndAction;


    SceneItem sceneItem;

    List<int> actions = new List<int>();
    List<string> names = new List<string>();
    void Awake () {
        sceneItem = GetComponent<SceneItem>();
        

        for (int i = 0; i < nameAndAction.list.Length; i++) {
            string[] split = nameAndAction.list[i].Split('-');
            if (split == null || split.Length != 2) {
                Debug.LogError(name + " InteractableShowHints: problem with input name " + i);
                return;
            }
            names.Add(split[0]);
            actions.Add(int.Parse(split[1]));
        }
    }

    
    public void OnInteractableAvailabilityChange(bool available) { }
    public void OnInteractableInspectedStart (InteractionPoint interactor) {
        GameUI.ShowInteractionHint (interactor.interactorID, getInteractionMessage, actions, names);
    }
    public void OnInteractableInspectedEnd (InteractionPoint interactor) {
        GameUI.HideInteractionHint (interactor.interactorID);
    }

    public void OnInteractableInspectedUpdate (InteractionPoint interactor) { }
    public void OnInteractableUsedStart (InteractionPoint interactor, int useAction) { }
    public void OnInteractableUsedEnd (InteractionPoint interactor, int useAction) { }
    public void OnInteractableUsedUpdate (InteractionPoint interactor, int useAction) { }
}

