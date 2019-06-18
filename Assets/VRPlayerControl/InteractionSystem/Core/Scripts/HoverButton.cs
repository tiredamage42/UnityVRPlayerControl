//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Drives a linear mapping based on position between 2 positions
//
//=============================================================================

using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using InteractionSystem;

using VRPlayer;
namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    [RequireComponent(typeof(Interactable))]
    public class HoverButton : MonoBehaviour, IInteractable
    {
        public int useActionIndex = 0;
        
		public void OnInspectStart(Interactor interactor) {

		}
        public void OnInspectEnd(Interactor interactor){

		}
        public void OnInspectUpdate(Interactor interactor){
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
        public void OnUseStart(Interactor interactor, int useIndex){

            Debug.LogError("hover button use start");

		}
        public void OnUseEnd(Interactor interactor, int useIndex){
			
		}
        public void OnUseUpdate(Interactor interactor, int useIndex){

		}


        public Transform movingPart;

        public Vector3 localMoveDistance = new Vector3(0, -0.1f, 0);

        [Range(0, 1)]
        public float engageAtPercent = 0.95f;

        [Range(0, 1)]
        public float disengageAtPercent = 0.9f;


        


        public HandEvent onButtonDown;
        public HandEvent onButtonUp;
        public HandEvent onButtonIsPressed;

        public bool engaged = false;
        public bool buttonDown = false;
        public bool buttonUp = false;

        private Vector3 startPosition;
        private Vector3 endPosition;

        private Vector3 handEnteredPosition;

        private bool hovering;

        // private Hand lastHoveredHand;

        Interactor lastInspector;

        void Awake () {
            interactable = GetComponent<Interactable>();
            

        }

        // void OnEnable () {
        //     interactable.onInspectUpdate += OnInspectUpdate;
            
        // }
        Interactable interactable;


        private void Start()
        {
            if (movingPart == null && this.transform.childCount > 0)
                movingPart = this.transform.GetChild(0);

            startPosition = movingPart.localPosition;
            endPosition = startPosition + localMoveDistance;
            handEnteredPosition = endPosition;
        }

        // private void HandHoverUpdate(Hand hand)
        private void OnInspectUpdate(Object inspector)
        
        {
            // hovering = true;
            // // lastHoveredHand = hand;
            // lastInspector = inspector;

            // Vector3 inspectorPosition = ((Hand)inspector).transform.position;

            // bool wasEngaged = engaged;

            // float currentDistance = Vector3.Distance(movingPart.parent.InverseTransformPoint(inspectorPosition), endPosition);
            // float enteredDistance = Vector3.Distance(handEnteredPosition, endPosition);

            // if (currentDistance > enteredDistance)
            // {
            //     enteredDistance = currentDistance;
            //     handEnteredPosition = movingPart.parent.InverseTransformPoint(inspectorPosition);
            // }

            // float distanceDifference = enteredDistance - currentDistance;

            // float lerp = Mathf.InverseLerp(0, localMoveDistance.magnitude, distanceDifference);

            // if (lerp > engageAtPercent)
            //     engaged = true;
            // else if (lerp < disengageAtPercent)
            //     engaged = false;

            // movingPart.localPosition = Vector3.Lerp(startPosition, endPosition, lerp);

            // InvokeEvents(wasEngaged, engaged);
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



            if (buttonDown)// && onButtonDown != null)
                interactable.OnUseStart(lastInspector, useActionIndex);
                // onButtonDown.Invoke(lastHoveredHand);
            if (isEngaged)// && onButtonIsPressed != null)
                interactable.OnUseUpdate(lastInspector, useActionIndex);
                // onButtonIsPressed.Invoke(lastHoveredHand);
            if (buttonUp)// && onButtonUp != null)
                interactable.OnUseEnd(lastInspector, useActionIndex);
                // onButtonUp.Invoke(lastHoveredHand);
        }
    }
}
