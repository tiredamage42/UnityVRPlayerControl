
using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using InteractionSystem;

// using VRPlayer;
namespace GameBase//Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    [RequireComponent(typeof(Interactable))]
    public class HoverButton : MonoBehaviour, IInteractable
    {
        public int useActionIndex = 0;
        
		public void OnInspectedStart(Interactor interactor) {

		}
        public void OnInspectedEnd(Interactor interactor){

		}
        public void OnInspectedUpdate(Interactor interactor){
            hovering = true;
            // lastHoveredHand = hand;
            lastInspector = interactor;

            Vector3 inspectorPosition = interactor.transform.position;

            bool wasEngaged = engaged;

            float currentDistance = Vector3.Distance(movingPart.parent.InverseTransformPoint(inspectorPosition), endPosition);
            float enteredDistance = Vector3.Distance(handEnteredPosition, endPosition);

            if (currentDistance > enteredDistance)
            {
                enteredDistance = currentDistance;
                handEnteredPosition = movingPart.parent.InverseTransformPoint(inspectorPosition);
            }

            float distanceDifference = enteredDistance - currentDistance;

            float lerp = Mathf.InverseLerp(0, localMoveDistance.magnitude, distanceDifference);

            if (lerp > engageAtPercent)
                engaged = true;
            else if (lerp < disengageAtPercent)
                engaged = false;

            movingPart.localPosition = Vector3.Lerp(startPosition, endPosition, lerp);

            InvokeEvents(wasEngaged, engaged);

		}
        public void OnUsedStart(Interactor interactor, int useIndex){
            Debug.LogError("hover button use start");
		}

        public void OnUsedEnd(Interactor interactor, int useIndex){
			
		}
        public void OnUsedUpdate(Interactor interactor, int useIndex){

		}


        public Transform movingPart;

        public Vector3 localMoveDistance = new Vector3(0, -0.1f, 0);

        [Range(0, 1)]
        public float engageAtPercent = 0.95f;

        [Range(0, 1)]
        public float disengageAtPercent = 0.9f;


        public bool engaged = false;
        public bool buttonDown = false;
        public bool buttonUp = false;

        Vector3 startPosition;
        Vector3 endPosition;
        Vector3 handEnteredPosition;

        bool hovering;
        Interactor lastInspector;

        void Awake () {
            interactable = GetComponent<Interactable>();
        }

        Interactable interactable;


        private void Start()
        {
            if (movingPart == null && this.transform.childCount > 0)
                movingPart = this.transform.GetChild(0);

            startPosition = movingPart.localPosition;
            endPosition = startPosition + localMoveDistance;
            handEnteredPosition = endPosition;
        }


        private void LateUpdate()
        {
            if (hovering == false)
            {
                movingPart.localPosition = startPosition;
                handEnteredPosition = endPosition;

                InvokeEvents(engaged, false);
                engaged = false;
            }

            hovering = false;
        }
        
        private void InvokeEvents(bool wasEngaged, bool isEngaged)
        {
            buttonDown = wasEngaged == false && isEngaged == true;
            buttonUp = wasEngaged == true && isEngaged == false;

            if (buttonDown)
                interactable.OnUsedStart(lastInspector, useActionIndex);
            if (isEngaged)
                interactable.OnUsedUpdate(lastInspector, useActionIndex);
            if (buttonUp)
                interactable.OnUsedEnd(lastInspector, useActionIndex);
        }
    }
}
