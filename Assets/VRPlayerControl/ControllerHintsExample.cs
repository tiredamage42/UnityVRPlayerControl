//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Demonstrates the use of the controller hint system
//
//=============================================================================

using UnityEngine;
using System.Collections;
using VRPlayer;

using Valve.VR.InteractionSystem;

using InteractionSystem;

namespace Demo
{
	public class ControllerHintsExample : MonoBehaviour
	{
		public Interactable showButton, hideButton;

		void OnEnable () {
			if (showButton != null)
				showButton.onUseStart += ShowTextHints;
			if (hideButton != null)
				hideButton.onUseStart += DisableHints;
		}
		void OnDisable () {
			if (showButton != null)
				showButton.onUseStart -= ShowTextHints;
			if (hideButton != null)
				hideButton.onUseStart -= DisableHints;
		}
			


		void ShowTextHints( Interactor user, int useIndex )
		{
			StandardizedVRInput.instance.PlayDebugRoutine();
		}
		void DisableHints( Interactor user, int useIndex )
		{
			StandardizedVRInput.instance.StopHintRoutine();
		}
	}
}
