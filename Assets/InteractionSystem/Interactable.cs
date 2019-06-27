
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

using RenderTricks;

namespace InteractionSystem
{

    [System.Serializable] public class InteractableInspectEvent : UnityEvent<Interactor> { }
    [System.Serializable] public class InteractableUseEvent : UnityEvent<Interactor, int> { }

    public class Interactable : MonoBehaviour
    {
        public string[] actionNames = new string [] {
            "Use"
        };
        public bool onlyProximityHover;
        public bool isAvailable = true;
        public enum UseType { Normal, Scripted };
        public UseType useType;
        
        [Tooltip("Set whether or not you want this interactible to highlight when hovering over it")]
        public bool highlightOnHover = true;
        
        [Tooltip("An array of child gameObjects to not render a highlight for. Things like transparent parts, vfx, etc.")]
        public GameObject[] hideHighlight;

        public InteractableUseEvent onUseStart, onUseUpdate, onUseEnd;
        public InteractableInspectEvent onInspectStart, onInspectUpdate, onInspectEnd;

        HashSet<int> currentHoveringIDs = new HashSet<int>();

        public bool isDestroying { get; protected set; }
        public bool isHovering { get { return currentHoveringIDs.Count != 0; } }
        bool isHighlighted;

        private void Awake()
        {
            InitializeListeners();
        }

        protected virtual bool ShouldIgnoreHighlight(Component component)
        {
            return ShouldIgnore(component.gameObject);
        }

        protected virtual bool ShouldIgnore(GameObject check)
        {
            for (int ignoreIndex = 0; ignoreIndex < hideHighlight.Length; ignoreIndex++)
            {
                if (check == hideHighlight[ignoreIndex])
                    return true;
            }
            return false;
        }

        List<Renderer> _Renderers = new List<Renderer>();

        void SubmitForHighlight () {
            if (!isHighlighted) {
                //Debug.Log("Highlighting " + name + " on hover");

                _Renderers.Clear();

                Renderer[] renderers = this.GetComponentsInChildren<Renderer>(true);
                            
                for (int i = 0; i < renderers.Length; i++)
                {
                    Renderer renderer = renderers[i];

                    if (ShouldIgnoreHighlight(renderer))
                        continue;

                    _Renderers.Add(renderer);
                }

                ObjectOutlines.Highlight_Renderers( _Renderers, 0 );
                isHighlighted = true;
            }
        }
        void UnHighlight() {
            if (isHighlighted) {
                isHighlighted = false;
                ObjectOutlines.UnHighlight_Renderers( _Renderers );
                _Renderers.Clear();
            }
        }

        public void OnInspectedStart (Interactor interactor) {
            currentHoveringIDs.Add(interactor.GetInstanceID());
            if (highlightOnHover) {
                SubmitForHighlight();
            }
            for (int i = 0; i < listeners.Count; i++) {
                listeners[i].OnInspectedStart(interactor);
            }
            if (onInspectStart != null) {
                onInspectStart.Invoke(interactor);
            }
        }
        
        public void OnInspectedEnd (Interactor interactor) {
            currentHoveringIDs.Remove(interactor.GetInstanceID());
            // if (highlightOnHover) {
            //     UnHighlight();
            // }
            for (int i = 0; i < listeners.Count; i++) {
                listeners[i].OnInspectedEnd(interactor);
            }
            if (onInspectEnd != null) {
                onInspectEnd.Invoke(interactor);
            }
        }
        public void OnInspectedUpdate(Interactor interactor) {
            for (int i = 0; i < listeners.Count; i++) {
                listeners[i].OnInspectedUpdate(interactor);
            }
            if (onInspectUpdate != null) {
                onInspectUpdate.Invoke(interactor);
            }
        }

        public void OnUsedStart (Interactor interactor, int useIndex) {
            for (int i = 0; i < listeners.Count; i++) {
                listeners[i].OnUsedStart(interactor, useIndex);
            }
            if (onUseStart != null) {
                onUseStart.Invoke(interactor, useIndex);
            }
        }
        public void OnUsedEnd (Interactor interactor, int useIndex) {
            for (int i = 0; i < listeners.Count; i++) {
                listeners[i].OnUsedEnd(interactor, useIndex);
            }
            if (onUseEnd != null) {
                onUseEnd.Invoke(interactor, useIndex);
            }
        }
        public void OnUsedUpdate(Interactor interactor, int useIndex) {
            for (int i = 0; i < listeners.Count; i++) {
                listeners[i].OnUsedUpdate(interactor, useIndex);
            }
            if (onUseUpdate != null) {
                onUseUpdate.Invoke(interactor, useIndex);
            }
        }

        List<IInteractable> listeners = new List<IInteractable>();
        void InitializeListeners() {
            IInteractable[] listeners_ = GetComponents<IInteractable>();
            for (int i = 0; i< listeners_.Length; i++) {
                this.listeners.Add(listeners_[i]);
            }
        }

        public void AddListener (IInteractable listener) {
            listeners.Add(listener);
        }

        protected virtual void Update()
        {
            if (highlightOnHover)
            {
                if (!isHovering)
                    UnHighlight();
            }
        }

        protected virtual void OnDestroy()
        {
            isDestroying = true;
            UnHighlight();
        }

        protected virtual void OnDisable()
        {
            isDestroying = true;
            UnHighlight();
        }
    }
}
