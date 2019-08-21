using System.Collections.Generic;
using UnityEngine;

namespace InteractionSystem {
    public class TaggerInteractable : MonoBehaviour, IInteractable
    {

        public int GetInteractionMode() { return 0; }
    
        public void OnInteractableAvailabilityChange(bool available) {
			
		}
		
        public int useActionToUse = 0;
        void AddTags (InteractionPoint interactor, List<string> tags) {
            if (tags.Count > 0) {
                interactor.AddInteractionTags(tags);
            }
        }

        void RemoveTags (InteractionPoint interactor, List<string> tags) {
            if (tags.Count > 0) {
                interactor.RemoveInteractionTags(tags);
            }
        }

        public void OnInteractableInspectedStart(InteractionPoint interactor) {
            lastInteractor = interactor;
            AddTags(interactor, inspectTags);
        }
        public void OnInteractableInspectedEnd(InteractionPoint interactor) {
            lastInteractor = null;
            RemoveTags(interactor, inspectTags);
        }
        public void OnInteractableUsedStart(InteractionPoint interactor, int useIndex) {
            if (useActionToUse == useIndex) {
                AddTags(interactor, useTags);
            }
        }
        public void OnInteractableUsedEnd(InteractionPoint interactor, int useIndex) {
            if (useActionToUse == useIndex) {
                RemoveTags(interactor, useTags);
            }
        }

        public void OnInteractableInspectedUpdate(InteractionPoint interactor) {}
        public void OnInteractableUsedUpdate(InteractionPoint interactor, int useIndex) {}

        public List<string> inspectTags, useTags;

        InteractionPoint lastInteractor;

        void OnDisable () {
            UntagLastInteractor();
        }
        void OnDestroy () {
            UntagLastInteractor();
        }

        void UntagLastInteractor () {

            if (lastInteractor != null) {

                lastInteractor.RemoveInteractionTags(inspectTags);
                lastInteractor.RemoveInteractionTags(useTags);
                
                lastInteractor = null;
            }
        }       
    }
}