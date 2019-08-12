using UnityEngine;
using ActorSystem;
using InventorySystem;
using InteractionSystem;

namespace DialogueSystem 
{

    //make interactable to start conversations...
    public class DialogueTemplateHolder : MonoBehaviour, IInteractable {
        public DialogueTemplate template;
        public AudioSource audioSource;
        public Actor actor;
        public Inventory inventory;

        void Awake () {
            if (actor == null) actor = GetComponent<Actor>();
            if (inventory == null) inventory = GetComponent<Inventory>();
        }

        public int useAction = 0;
        
        public void OnInteractableAvailabilityChange(bool available) { }
        public void OnInteractableInspectedStart (InteractionPoint interactor) { }
        public void OnInteractableInspectedEnd (InteractionPoint interactor) { }
        public void OnInteractableInspectedUpdate (InteractionPoint interactor) { }

        public void OnInteractableUsedStart (InteractionPoint interactor, int useAction) {
            if ( useAction != this.useAction ) return;
            DialoguePlayer dialoguePlayer = interactor.GetComponentInParent<DialoguePlayer>();
            if (dialoguePlayer != null) dialoguePlayer.StartDialogueWith(this);
        }

        public void OnInteractableUsedEnd (InteractionPoint interactor, int useAction) { }
        public void OnInteractableUsedUpdate (InteractionPoint interactor, int useAction) { }
    }
}



