using UnityEngine;
using InteractionSystem;

namespace Game.DialogueSystem 
{
    //interactable to start conversations...
    public class DialogueTemplateHolder : MonoBehaviour, IInteractable {
        public int GetInteractionMode () { return 0; }
        public int useAction = 0;
        public DialogueTemplate template;


        DialogueTemplate overrideTemplate;

        public void SetOverrideTemplate (DialogueTemplate overrideTemplate) {
            this.overrideTemplate = overrideTemplate;
        }


        public Actor actor;
        public AudioSource audioSource;

        void Awake () {
            if ( actor == null )
                actor = GetComponent<Actor>();
        }

        public event System.Action onDialogueEnded, onDialogueStarted;
        void OnDialogueEnded () {
            if (onDialogueEnded != null) {
                onDialogueEnded();
            }

        }

        void OnDialogueStarted () {
            if (onDialogueStarted != null) {
                onDialogueStarted();
            }
        }
        void StartDialogue (Actor instigator) {
            OnDialogueStarted();
            instigator.StartDialogue(overrideTemplate != null ? overrideTemplate : template, actor, audioSource, OnDialogueEnded);
        }

        public void OnInteractableAvailabilityChange(bool available) { }
        public void OnInteractableInspectedStart (InteractionPoint interactor) { }
        public void OnInteractableInspectedEnd (InteractionPoint interactor) { }
        public void OnInteractableInspectedUpdate (InteractionPoint interactor) { }

        public void OnInteractableUsedStart (InteractionPoint interactor, int useAction) {
            if ( useAction == this.useAction ) {
                // TODO: set this to interactor actor
                StartDialogue(Actor.playerActor);
            }
        }

        public void OnInteractableUsedEnd (InteractionPoint interactor, int useAction) { }
        public void OnInteractableUsedUpdate (InteractionPoint interactor, int useAction) { }
    }
}