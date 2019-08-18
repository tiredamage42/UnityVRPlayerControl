using System.Collections.Generic;
using UnityEngine;

namespace InteractionSystem {
    
    public class Interactor : MonoBehaviour
    {
        List<string> interactionTags = new List<string>();
        public bool usePositionCheck = true;
        public float positionRadius = .1f;
        public bool useRayCheck = true;
        public float rayCheckDistance = 1f;
        public bool godModeInteractor;
       

        public bool HasTag (string tag) {
            return interactionTags.Contains(tag);
        }
        
        void Awake () {
            InteractionPoint[] childInteractors = GetComponentsInChildren<InteractionPoint>();
            for (int i =0 ; i < childInteractors.Length; i++) {
                if (childInteractors[i] != this) 
                    childInteractors[i].SetBaseInteractor(this);
            }
        }
        
        public void RemoveInteractionTags (List<string> tags) {
            for (int i = 0; i < tags.Count; i++) {
                interactionTags.Remove(tags[i]);
            }
        }
        
        public void AddInteractionTags (List<string> tags) {
            interactionTags.AddRange(tags);
            if (interactionTags.Count > 25) {
                Debug.LogError(name + " interaction tags getting bloated");
            }
        }
    }
}
