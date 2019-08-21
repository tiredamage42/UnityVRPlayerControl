using System.Collections.Generic;
using UnityEngine;




using Game;
namespace InteractionSystem {
    
    public class Interactor : MonoBehaviour
    {
        public int interactionMode;
        public float defaultRaycheckDistance = 1f;

        public void SetInteractionMode (int interactionMode, float rayCheckDistance) {
            this.interactionMode = interactionMode;
            if (rayCheckDistance < 0) rayCheckDistance = defaultRaycheckDistance;
            this.rayCheckDistance = rayCheckDistance;
        }


        List<string> interactionTags = new List<string>();
        public bool usePositionCheck = true;
        public float positionRadius = .1f;
        public bool useRayCheck = true;
        [HideInInspector] public float rayCheckDistance = 1f;
        public bool godModeInteractor;
       

        public bool HasTag (string tag) {
            return interactionTags.Contains(tag);
        }
        InteractionPoint[] childInteractors;
        void Awake () {
            childInteractors = GetComponentsInChildren<InteractionPoint>();
            for (int i =0 ; i < childInteractors.Length; i++) {
                childInteractors[i].SetBaseInteractor(this);
            }
                
            SetInteractionMode(0, -1);
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

        Actor actor;
        void OnEnable () {
            actor = GetComponent<Actor>();
            InitializeActionsReceiver();
        }
        void OnDisable () {
            UninitializeActionsReceiver();
        }



        void InitializeActionsReceiver () {
            actor.onActionStart += OnActionStart;
            actor.onActionUpdate += OnActionUpdate;
            actor.onActionEnd += OnActionEnd;
        }
        void UninitializeActionsReceiver () {
            actor.onActionStart -= OnActionStart;
            actor.onActionUpdate -= OnActionUpdate;
            actor.onActionEnd -= OnActionEnd;
        }

        InteractionPoint GetInteractionPointByControllerID (int controllerIndex) {
            
            for (int i =0 ; i < childInteractors.Length; i++) {
                if (childInteractors[i].interactorID == controllerIndex) {
                    return childInteractors[i];
                }
            }
            return null;
        }
        public InteractionPoint mainInteractor {
            get {
                return GetInteractionPointByControllerID(0);
            }
        }

        void OnActionStart (int controllerIndex, int actionIndex) {
            InteractionPoint p = GetInteractionPointByControllerID(controllerIndex);
            if (p == null) return;
            p.OnActionStart(interactionMode, actionIndex);
        }
        void OnActionUpdate (int controllerIndex, int actionIndex) {
            InteractionPoint p = GetInteractionPointByControllerID(controllerIndex);
            if (p == null) return;
            p.OnActionUpdate(interactionMode, actionIndex);
        }
        void OnActionEnd (int controllerIndex, int actionIndex) {
            InteractionPoint p = GetInteractionPointByControllerID(controllerIndex);
            if (p == null) return;
            p.OnActionEnd(interactionMode, actionIndex);
        }
    }
}
