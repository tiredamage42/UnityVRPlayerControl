using UnityEngine;

using Valve.VR;
using Game.UI;

using Game;
using System.Collections.Generic;

namespace VRPlayer.UI {

    public class VRUIInputHandler : MonoBehaviour 
    {
        public TransformBehavior equipBehavior;
        public string context;
        UIHandler myUIHandler;
        
        
        void OnEnable () {

            myUIHandler = UIHandler.GetUIHandlerByContext(context);

            if (myUIHandler != null) {
                // myUIHandler.onUIClose += OnCloseUI;
                myUIHandler.onUIOpen += OnOpenUI;
            }
            else {
                Debug.LogError(context + " handler is null");
            }
        }

        void OnDisable () {
            if (myUIHandler != null) {
                // myUIHandler.onUIClose -= OnCloseUI;
                myUIHandler.onUIOpen -= OnOpenUI;
            }
        }

    // bool needsSingleHandInput { get { return myUIHandler.controllerIndex != -1; } }

        void OnOpenUI (GameObject uiObject, int interactorID) {

            SteamVR_Input_Sources hand = myUIHandler.controllerIndex != -1 ? VRManager.Int2Hand( myUIHandler.controllerIndex ) : SteamVR_Input_Sources.Any;

            // List<int> inputActions = myUIHandler.allActions;
            // for (int i = 0; i < inputActions.Count; i++) {

            //     ControlsManager.MarkActionOccupied(inputActions[i], myUIHandler.controllerIndex);
            //     // StandardizedVRInput.MarkActionOccupied(Player.instance.actions[inputActions[i]], hand);
            // }

            if (equipBehavior != null) {
                TransformBehavior.AdjustTransform(uiObject.transform, Player.instance.GetHand(hand).transform, equipBehavior, 0);
            }
            // VRUIInput.SetUIHand(hand);
        }

        // void OnCloseUI (GameObject uiObject) {
        //     List<int> inputActions = myUIHandler.allActions;
        //     for (int i = 0; i < inputActions.Count; i++) {
        //         ControlsManager.MarkActionUnoccupied(inputActions[i]);
        //         // StandardizedVRInput.MarkActionUnoccupied(Player.instance.actions[inputActions[i]]);
                
        //     }
        // }
    }
}

