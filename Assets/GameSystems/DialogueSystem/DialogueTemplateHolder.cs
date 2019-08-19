using UnityEngine;
using InteractionSystem;

namespace Game.DialogueSystem 
{
    //interactable to start conversations...
    public class DialogueTemplateHolder : MonoBehaviour, IInteractable {
        public int useAction = 0;
        public DialogueTemplate template;
        public Actor actor;
        public AudioSource audioSource;

        void Awake () {
            if ( actor == null )
                actor = GetComponent<Actor>();
        }

        public void OnInteractableAvailabilityChange(bool available) { }
        public void OnInteractableInspectedStart (InteractionPoint interactor) { }
        public void OnInteractableInspectedEnd (InteractionPoint interactor) { }
        public void OnInteractableInspectedUpdate (InteractionPoint interactor) { }

        public void OnInteractableUsedStart (InteractionPoint interactor, int useAction) {
            if ( useAction == this.useAction ) 
                Actor.playerActor.StartDialogue(template, actor, audioSource);
        }

        public void OnInteractableUsedEnd (InteractionPoint interactor, int useAction) { }
        public void OnInteractableUsedUpdate (InteractionPoint interactor, int useAction) { }
    }
}