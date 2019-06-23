using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using InteractionSystem;

namespace VRPlayer {

    public class VRInteractor : MonoBehaviour
    {

        /*
            attach to the hand interactor to modify the interaction reference transform
        */
        public TransformBehavior interactorReferenceBehavior;

        Interactor interactor;

        void Awake () {
            interactor = GetComponent<Interactor>();
            UpdateInteractorReferenceTransform();
        }

        void UpdateInteractorReferenceTransform () {
            if (interactor.referenceTransform == null) {
                interactor.referenceTransform = new GameObject(name + " interactor helper transform").transform;
            }
            TransformBehavior.AdjustTransform(interactor.referenceTransform, transform, interactorReferenceBehavior, 0);
        }

        
        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
#if UNITY_EDITOR
            UpdateInteractorReferenceTransform();
#endif
        }
    }
}
