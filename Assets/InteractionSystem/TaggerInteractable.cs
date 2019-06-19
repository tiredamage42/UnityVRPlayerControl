using System.Collections.Generic;
using UnityEngine;

namespace InteractionSystem {
    public class TaggerInteractable : MonoBehaviour, IInteractable
    {
        public int useActionToUse = 0;
        void AddTags (Interactor interactor, List<string> tags) {
            if (tags.Count > 0) {
                interactor.AddInteractionTags(tags);
            }
        }

        void RemoveTags (Interactor interactor, List<string> tags) {
            if (tags.Count > 0) {
                interactor.RemoveInteractionTags(tags);
            }
        }

        public void OnInspectStart(Interactor interactor) {
            lastInteractor = interactor;
            AddTags(interactor, inspectTags);
        }
        public void OnInspectEnd(Interactor interactor) {
            lastInteractor = null;
            RemoveTags(interactor, inspectTags);
        }
        public void OnUseStart(Interactor interactor, int useIndex) {
            if (useActionToUse == useIndex) {
                AddTags(interactor, useTags);
            }
        }
        public void OnUseEnd(Interactor interactor, int useIndex) {
            if (useActionToUse == useIndex) {
                RemoveTags(interactor, useTags);
            }
        }

        public void OnInspectUpdate(Interactor interactor) {}
        public void OnUseUpdate(Interactor interactor, int useIndex) {}

        public List<string> inspectTags, useTags;

        Interactor lastInteractor;

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