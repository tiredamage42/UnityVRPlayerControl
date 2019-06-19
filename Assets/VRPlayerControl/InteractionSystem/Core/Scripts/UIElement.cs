//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: UIElement that responds to VR hands and generates UnityEvents
//
//=============================================================================

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using InteractionSystem;
namespace Valve.VR.InteractionSystem
{
	//-------------------------------------------------------------------------
	[RequireComponent( typeof( Interactable ) )]
	public class UIElement : MonoBehaviour, IInteractable
	{

		
		public void OnInspectStart(Interactor interactor) {
			InputModule.instance.HoverBegin( gameObject );
		}
        public void OnInspectEnd(Interactor interactor){
			InputModule.instance.HoverEnd( gameObject );
		}
        public void OnInspectUpdate(Interactor interactor){

		}
        public void OnUseStart(Interactor interactor, int useIndex){
			InputModule.instance.Submit( gameObject );
		}

        public void OnUseEnd(Interactor interactor, int useIndex){
			
		}
        public void OnUseUpdate(Interactor interactor, int useIndex){

		}




		// public CustomEvents.UnityEventHand onHandClick;

        // protected Hand currentHand;

		//-------------------------------------------------
		// protected virtual void Awake()
		// {
		// 	Button button = GetComponent<Button>();
		// 	if ( button )
		// 	{
		// 		button.onClick.AddListener( OnButtonClick );
		// 	}
		// }


		// //-------------------------------------------------
		// protected virtual void OnHandHoverBegin( Hand hand )
		// {
		// 	currentHand = hand;
		// 	InputModule.instance.HoverBegin( gameObject );
		// }


        // //-------------------------------------------------
        // protected virtual void OnHandHoverEnd( Hand hand )
		// {
		// 	InputModule.instance.HoverEnd( gameObject );
		// 	currentHand = null;
		// }


        // //-------------------------------------------------
        // protected virtual void HandHoverUpdate( Hand hand )
		// {
		// 	if ( hand.uiInteractAction != null && hand.uiInteractAction.GetStateDown(hand.handType) )
		// 	{
		// 		InputModule.instance.Submit( gameObject );
		// 	}
		// }


        //-------------------------------------------------
        // protected virtual void OnButtonClick()
		// {
		// 	// onHandClick.Invoke( currentHand );
		// }
	}

// #if UNITY_EDITOR
// 	//-------------------------------------------------------------------------
// 	[UnityEditor.CustomEditor( typeof( UIElement ) )]
// 	public class UIElementEditor : UnityEditor.Editor
// 	{
// 		//-------------------------------------------------
// 		// Custom Inspector GUI allows us to click from within the UI
// 		//-------------------------------------------------
// 		public override void OnInspectorGUI()
// 		{
// 			DrawDefaultInspector();

// 			UIElement uiElement = (UIElement)target;
// 			if ( GUILayout.Button( "Click" ) )
// 			{
// 				InputModule.instance.Submit( uiElement.gameObject );
// 			}
// 		}
// 	}
// #endif
}
