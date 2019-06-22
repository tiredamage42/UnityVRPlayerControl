//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Triggers haptic pulses based on a linear mapping
//
//=============================================================================

using UnityEngine;
using UnityEngine.Events;
using System.Collections;


using InteractionSystem;
using VRPlayer;
namespace Valve.VR.InteractionSystem
{
	//-------------------------------------------------------------------------
	[RequireComponent( typeof( Interactable ) )]
	public class HapticRack : MonoBehaviour
	{
		
		void OnInspectStart(Interactor interactor) {

		}
        void OnInspectEnd(Interactor interactor){

		}
        void OnInspectUpdate(Interactor interactor){

		}
        void OnUseStart(Interactor interactor, int useIndex){

		}
        void OnUseEnd(Interactor interactor, int useIndex){
			
		}
        void OnUseUpdate(Interactor interactor, int useIndex){


			Hand hand = interactor.GetComponent<Hand>();

			int currentToothIndex = Mathf.RoundToInt( linearMapping.value * teethCount - 0.5f );
			if ( currentToothIndex != previousToothIndex )
			{
				// Pulse();
				// if ( hand && (hand.isActive) && ( hand.IsGrabbing() ) )
				if ( hand && (hand.isActive) )
			
				{
					ushort duration = (ushort)Random.Range( minimumPulseDuration, maximumPulseDuration + 1 );
					StandardizedVRInput.instance.TriggerHapticPulse( hand.handType, duration );

					onPulse.Invoke();
				}

				previousToothIndex = currentToothIndex;
			}

		}



		[Tooltip( "The linear mapping driving the haptic rack" )]
		public LinearMapping linearMapping;

		[Tooltip( "The number of haptic pulses evenly distributed along the mapping" )]
		public int teethCount = 128;

		[Tooltip( "Minimum duration of the haptic pulse" )]
		public int minimumPulseDuration = 500;

		[Tooltip( "Maximum duration of the haptic pulse" )]
		public int maximumPulseDuration = 900;

		[Tooltip( "This event is triggered every time a haptic pulse is made" )]
		public UnityEvent onPulse;

		// private Hand hand;
		private int previousToothIndex = -1;

		//-------------------------------------------------
		void Awake()
		{
			Debug.LogError(name + "  is using haptic rack");
			if ( linearMapping == null )
			{
				linearMapping = GetComponent<LinearMapping>();
			}
		}


		//-------------------------------------------------
		// private void OnHandHoverBegin( Hand hand )
		// {
		// 	this.hand = hand;
		// }


		// //-------------------------------------------------
		// private void OnHandHoverEnd( Hand hand )
		// {
		// 	this.hand = null;
		// }




		//-------------------------------------------------
		// void Update()
		// {
		// 	int currentToothIndex = Mathf.RoundToInt( linearMapping.value * teethCount - 0.5f );
		// 	if ( currentToothIndex != previousToothIndex )
		// 	{
		// 		Pulse();
		// 		previousToothIndex = currentToothIndex;
		// 	}
		// }


		//-------------------------------------------------
		// private void Pulse()
		// {

		// 	// if ( hand && (hand.isActive) && ( hand.GetBestGrabbingType() != GrabTypes.None ) )
		// 	if ( hand && (hand.isActive) && ( hand.IsGrabbing() ) )
			
		// 	{
		// 		ushort duration = (ushort)Random.Range( minimumPulseDuration, maximumPulseDuration + 1 );
		// 		hand.TriggerHapticPulse( duration );

		// 		onPulse.Invoke();
		// 	}
		// }
	}
}
