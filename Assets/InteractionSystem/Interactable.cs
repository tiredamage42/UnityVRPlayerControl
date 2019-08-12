
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

using RenderTricks;

namespace InteractionSystem
{

    // [System.Serializable] public class InteractableInspectEvent : UnityEvent<InteractionPoint> { }
    // [System.Serializable] public class InteractableUseEvent : UnityEvent<InteractionPoint, int> { }
    // [System.Serializable] public class InteractableAvailabilityEvent : UnityEvent<bool> { }
    public interface IInteractable {
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
        
        public string[] actionNames = new string [] {
            "Use"
        };
        public bool onlyProximityHover;
        public bool available = true;

        public void SetAvailable (bool available) {
            this.available = available;
            for (int i = 0; i < listeners.Count; i++) listeners[i].OnInteractableAvailabilityChange(available);
            // if (onAvailabilityChange != null) onAvailabilityChange.Invoke(available);
        }
        
        public enum UseType { Normal, Scripted };
        public UseType useType;
        
        // public InteractableUseEvent onUseStart, onUseUpdate, onUseEnd;
        // public InteractableInspectEvent onInspectStart, onInspectUpdate, onInspectEnd;
        // public InteractableAvailabilityEvent onAvailabilityChange;



        HashSet<int> currentHoveringIDs = new HashSet<int>();

        // public bool isDestroying { get; protected set; }
        public bool isHovering { get { return currentHoveringIDs.Count != 0; } }


        void SetInteractableElements () {
            InteractableElement[] elements = GetComponentsInChildren<InteractableElement>();
            for (int i = 0; i < elements.Length; i++) elements[i].SetInteractable(this);
        }
        
        private void Awake()
        {
            InitializeListeners();
            SetInteractableElements();
        }

        void Start () {
            SetAvailable(available);
        }

        
        public void OnInspectedStart (InteractionPoint interactor) {
            currentHoveringIDs.Add(interactor.GetInstanceID());
            for (int i = 0; i < listeners.Count; i++) listeners[i].OnInteractableInspectedStart(interactor);
            // if (onInspectStart != null) onInspectStart.Invoke(interactor);
        }
        public void OnInspectedEnd (InteractionPoint interactor) {
            currentHoveringIDs.Remove(interactor.GetInstanceID());
            for (int i = 0; i < listeners.Count; i++) listeners[i].OnInteractableInspectedEnd(interactor);
            // if (onInspectEnd != null) onInspectEnd.Invoke(interactor);
        }
        public void OnInspectedUpdate(InteractionPoint interactor) {
            for (int i = 0; i < listeners.Count; i++) listeners[i].OnInteractableInspectedUpdate(interactor);
            // if (onInspectUpdate != null) onInspectUpdate.Invoke(interactor);
        }
        public void OnUsedStart (InteractionPoint interactor, int useIndex) {
            for (int i = 0; i < listeners.Count; i++) listeners[i].OnInteractableUsedStart(interactor, useIndex);
            // if (onUseStart != null) onUseStart.Invoke(interactor, useIndex);
        }
        public void OnUsedEnd (InteractionPoint interactor, int useIndex) {
            for (int i = 0; i < listeners.Count; i++) listeners[i].OnInteractableUsedEnd(interactor, useIndex);
            // if (onUseEnd != null) onUseEnd.Invoke(interactor, useIndex);
        }
        public void OnUsedUpdate(InteractionPoint interactor, int useIndex) {
            for (int i = 0; i < listeners.Count; i++) listeners[i].OnInteractableUsedUpdate(interactor, useIndex);
            // if (onUseUpdate != null) onUseUpdate.Invoke(interactor, useIndex);
        }
        List<IInteractable> listeners = new List<IInteractable>();
        void InitializeListeners() {
            IInteractable[] listeners_ = GetComponents<IInteractable>();
            for (int i = 0; i< listeners_.Length; i++) this.listeners.Add(listeners_[i]);
        }
        public void AddListener (IInteractable listener) {
            listeners.Add(listener);
        }
    }
}
