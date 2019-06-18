//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using UnityEngine;
using System.Collections;

using InteractionSystem;

namespace Valve.VR.InteractionSystem.Sample
{
    public class RenderModelChangerUI : MonoBehaviour// UIElement
    {
       
		void OnInspectStart(Interactor interactor) {

		}
        void OnInspectEnd(Interactor interactor){

		}
        void OnInspectUpdate(Interactor interactor){

		}
        void OnUseStart(Interactor interactor, int useIndex){
            if (ui != null)
            {
                ui.SetRenderModel(this);
            }

		}
        void OnUseEnd(Interactor interactor, int useIndex){
			
		}
        void OnUseUpdate(Interactor interactor, int useIndex){

		}


        // Interactable interactable;


        public GameObject leftPrefab;
        public GameObject rightPrefab;

        protected SkeletonUIOptions ui;


        // void OnEnable () {
        //     interactable.onEquipped += OnButtonClick;
            
        // }
        // void OnDisable () {
        //     interactable.onEquipped -= OnButtonClick;
        // }

        // protected override 
        void Awake()
        {
            // base.Awake();

            // interactable = GetComponent<Interactable>();

            ui = this.GetComponentInParent<SkeletonUIOptions>();
        }

        // protected override 
        // void OnButtonClick(Object clicker)
        // {
        //     // base.OnButtonClick();

        //     if (ui != null)
        //     {
        //         ui.SetRenderModel(this);
        //     }
        // }
    }
}