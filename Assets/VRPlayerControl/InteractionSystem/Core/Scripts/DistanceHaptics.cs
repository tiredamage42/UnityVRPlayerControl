//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Triggers haptic pulses based on distance between 2 positions
//
//=============================================================================

using UnityEngine;
using System.Collections;
using VRPlayer;


namespace Valve.VR.InteractionSystem
{
	//-------------------------------------------------------------------------
	public class DistanceHaptics : MonoBehaviour
	{
		public Transform firstTransform;
		public Transform secondTransform;

		public AnimationCurve distanceIntensityCurve = AnimationCurve.Linear( 0.0f, 800.0f, 1.0f, 800.0f );
		public AnimationCurve pulseIntervalCurve = AnimationCurve.Linear( 0.0f, 0.01f, 1.0f, 0.0f );

		//-------------------------------------------------
		IEnumerator Start()
		{
			while ( true )
			{
				float distance = Vector3.Distance( firstTransform.position, secondTransform.position );

                Hand hand = GetComponentInParent<Hand>();
                if (hand != null)
                { 
					float pulse = distanceIntensityCurve.Evaluate( distance );

					StandardizedVRInput.instance.TriggerHapticPulse(hand.handType,
					// hand.TriggerHapticPulse(
							(ushort)pulse
					);

                    //SteamVR_Controller.Input( (int)trackedObject.index ).TriggerHapticPulse( (ushort)pulse );
				}

				float nextPulse = pulseIntervalCurve.Evaluate( distance );

				yield return new WaitForSeconds( nextPulse );
			}

		}
	}
}
