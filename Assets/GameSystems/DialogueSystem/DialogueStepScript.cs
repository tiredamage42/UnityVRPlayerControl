using UnityEngine;
// using ActorSystem;

namespace Game.DialogueSystem {
    public interface IDialogueStepScript {
        bool ClosesConversation();
        bool StepAvailable(Actor dialoguePlayer, Actor speaker);
        void OnDialogueStep (Actor dialoguePlayer, Actor speaker, float stepTime);
    }
    [System.Serializable] public class DialogueStepScriptArray : NeatArrayWrapper<DialogueStepScript> { }
    public class DialogueStepScript : MonoBehaviour
    {
        IDialogueStepScript[] attachedBehaviors;
        public bool StepAvailable(Actor dialoguePlayer, Actor speaker) {
            if (attachedBehaviors == null) attachedBehaviors = GetComponents<IDialogueStepScript>();
            for (int i = 0; i < attachedBehaviors.Length; i++) {
                if (!attachedBehaviors[i].StepAvailable( dialoguePlayer, speaker)) {
                    return false;
                }
            }
            return true;
        }

        public bool ClosesConversation () {
            if (attachedBehaviors == null) attachedBehaviors = GetComponents<IDialogueStepScript>();
            for (int i = 0; i < attachedBehaviors.Length; i++) {
                if (attachedBehaviors[i].ClosesConversation( )) {
                    return true;
                }
            }
            return false;   
        }

        public void OnDialogueStep (Actor dialoguePlayer, Actor speaker, float stepTime) {
            if (attachedBehaviors == null) attachedBehaviors = GetComponents<IDialogueStepScript>();
            for (int i = 0; i < attachedBehaviors.Length; i++) {
                attachedBehaviors[i].OnDialogueStep( dialoguePlayer, speaker, stepTime);
            }
        }
    }
}
