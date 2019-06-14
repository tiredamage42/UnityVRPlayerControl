using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class ControllerHints : MonoBehaviour
{

    /*


    //-------------------------------------------------
		// Cycles through all the text hints on the controller
		//-------------------------------------------------
		private IEnumerator TestTextHints( Hand hand )
		{
			ControllerButtonHints.HideAllTextHints( hand );

			while ( true )
            {
                for (int actionIndex = 0; actionIndex < SteamVR_Input.actionsIn.Length; actionIndex++)
                {
                    ISteamVR_Action_In action = SteamVR_Input.actionsIn[actionIndex];
                    if (action.GetActive(hand.handType))
                    {
                        ControllerButtonHints.ShowTextHint(hand, action, action.GetShortName());
                        yield return new WaitForSeconds(3.0f);
                        ControllerButtonHints.HideTextHint(hand, action);
                        yield return new WaitForSeconds(0.5f);
                    }
                    yield return null;
                }

                ControllerButtonHints.HideAllTextHints(hand);
                yield return new WaitForSeconds(3.0f);
			}
		}
     */
}
