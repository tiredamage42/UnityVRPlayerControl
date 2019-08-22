
using UnityEngine;
// using UnityEngine.Events;
using System.Collections.Generic;

namespace InteractionSystem
{

    public interface IInteractable {
        int GetInteractionMode ();

        void OnInteractableInspectedStart(InteractionPoint interactor);
        void OnInteractableInspectedEnd(InteractionPoint interactor);
        void OnInteractableInspectedUpdate(InteractionPoint interactor);
        void OnInteractableUsedStart(InteractionPoint interactor, int useIndex);
        void OnInteractableUsedEnd(InteractionPoint interactor, int useIndex);
        void OnInteractableUsedUpdate(InteractionPoint interactor, int useIndex);
        void OnInteractableAvailabilityChange(bool available);
    }
    public class Interactable : MonoBehaviour
    {
        public const string interactableLayerName = "InteractableTrigger";
        public static int interactTriggerMask { get { return 1 << LayerMask.NameToLayer(interactableLayerName); } }
        // public string[] actionNames = new string [] { "Use" };
        public bool onlyProximityHover;
        public bool isAvailable = true;        
        public enum UseType { Normal, Scripted };
        public UseType useType;
        HashSet<int> currentHoveringIDs = new HashSet<int>();
        public bool isHovering { get { return currentHoveringIDs.Count != 0; } }
        
        List<IInteractable> listeners = new List<IInteractable>();

        public void AddListener (IInteractable listener) {
            listeners.Add(listener);
        }
        void InitializeListeners() {
            IInteractable[] listeners_ = GetComponents<IInteractable>();
            for (int i = 0; i< listeners_.Length; i++) this.listeners.Add(listeners_[i]);
        }
        void SetInteractableElements () {
            InteractableElement[] elements = GetComponentsInChildren<InteractableElement>();
            for (int i = 0; i < elements.Length; i++) elements[i].interactable = this;
        }

        public int enforceInteractorID = -1;
        
        
        void Awake() {
            InitializeListeners();
            SetInteractableElements();
        }

        void Start () {
            SetAvailable(isAvailable);
        }
        public bool IsAvailable (int currentInteractionMode) {
            if (!isAvailable) {
                return false;
            }
            for (int i = 0; i < listeners.Count; i++) {
                if (listeners[i].GetInteractionMode() == currentInteractionMode) {
                    return true;
                }
            }
            return false;
        }

        public void SetAvailable (bool available) {
            this.isAvailable = available;
            for (int i = 0; i < listeners.Count; i++) listeners[i].OnInteractableAvailabilityChange(available);
        }
        public void OnInspectedStart (InteractionPoint interactor, int currentInteractionMode) {
            if (enforceInteractorID != -1 && enforceInteractorID != interactor.interactorID) return;
            currentHoveringIDs.Add(interactor.GetInstanceID());
            for (int i = 0; i < listeners.Count; i++) if (listeners[i].GetInteractionMode() == currentInteractionMode) listeners[i].OnInteractableInspectedStart(interactor);
        }
        public void OnInspectedEnd (InteractionPoint interactor, int currentInteractionMode) {
            if (enforceInteractorID != -1 && enforceInteractorID != interactor.interactorID) return;
            currentHoveringIDs.Remove(interactor.GetInstanceID());
            for (int i = 0; i < listeners.Count; i++) if (listeners[i].GetInteractionMode() == currentInteractionMode) listeners[i].OnInteractableInspectedEnd(interactor);
        }
        public void OnInspectedUpdate(InteractionPoint interactor, int currentInteractionMode) {
            if (enforceInteractorID != -1 && enforceInteractorID != interactor.interactorID) return;
            for (int i = 0; i < listeners.Count; i++) if (listeners[i].GetInteractionMode() == currentInteractionMode) listeners[i].OnInteractableInspectedUpdate(interactor);
        }


        










        public void OnUsedStart (InteractionPoint interactor, int currentInteractionMode, int useIndex) {
            if (enforceInteractorID != -1 && enforceInteractorID != interactor.interactorID) return;
            for (int i = 0; i < listeners.Count; i++) if (listeners[i].GetInteractionMode() == currentInteractionMode) listeners[i].OnInteractableUsedStart(interactor, useIndex);
        }
        public void OnUsedEnd (InteractionPoint interactor, int currentInteractionMode, int useIndex) {
            if (enforceInteractorID != -1 && enforceInteractorID != interactor.interactorID) return;
            for (int i = 0; i < listeners.Count; i++) if (listeners[i].GetInteractionMode() == currentInteractionMode) listeners[i].OnInteractableUsedEnd(interactor, useIndex);
        }
        public void OnUsedUpdate(InteractionPoint interactor, int currentInteractionMode, int useIndex) {
            if (enforceInteractorID != -1 && enforceInteractorID != interactor.interactorID) return;
            for (int i = 0; i < listeners.Count; i++) if (listeners[i].GetInteractionMode() == currentInteractionMode) listeners[i].OnInteractableUsedUpdate(interactor, useIndex);
        }
    }
}
