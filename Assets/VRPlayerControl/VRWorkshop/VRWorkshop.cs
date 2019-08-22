using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;
using Game.UI;
using Valve.VR;
using InteractionSystem;

using Game.InventorySystem.WorkshopSystem;

namespace VRPlayer {
    public class VRWorkshop : MonoBehaviour
    {
        

        public SteamVR_Action_Boolean newTeleportButton;
    //     // public SteamVR_Input_Sources newTeleportHand;

        SteamVR_Action_Boolean oldTeleportButton;
        SteamVR_Input_Sources oldTeleportHand;

        WorkshopMode workshop;

        public TransformBehavior workshopUITransform;    
        UIHandler myUIHandler;
        void OnEnable () {
            workshop = Player.instance.GetComponent<WorkshopMode>();
            myUIHandler = WorkshopUIHandler.instance;
            myUIHandler.onUIClose += OnCloseUI;
            myUIHandler.onUIOpen += OnOpenUI;
        }
        void OnDisable () {
            myUIHandler.onUIClose -= OnCloseUI;
            myUIHandler.onUIOpen -= OnOpenUI;
        }

        bool isOpen;

        void OnOpenUI (GameObject uiObject, int interactorID) {
            isOpen = true;

//         hide hands, show controllers
            for (int i =0 ; i < Player.instance.hands.Length; i++) {
                Player.instance.hands[i].ShowController(true);   
            }

//         uncrouch on workshop enter
            Player.instance.GetComponent<TouchpadLocomotion>().isCrouched = false;

            


            SteamVR_Input_Sources uiHand = SteamVR_Input_Sources.LeftHand;
            VRUIInput.SetUIHand(uiHand);

            TransformBehavior.AdjustTransform(uiObject.transform, Player.instance.GetHand(uiHand).transform, workshopUITransform, 0);
        
            // SteamVR_Input_Sources inputHand = SteamVR_Input_Sources.RightHand;
        
            // StandardizedVRInput.MarkActionOccupied(controls.list[i].action, hand);
            // StandardizedVRInput.instance.ShowHint(inputHand, Player.instance.actions[Player.instance.GetComponent<ControlsManager>().inGameMenuAction.x], "Exit");
            
            // StandardizedVRInput.instance.ShowHint(inputHand, Player.instance.actions[workshop.cancelAction], "Scrap / Store / Cancel");
            // StandardizedVRInput.instance.ShowHint(inputHand, Player.instance.actions[workshop.submitAction], "Build / Select");



            if (Teleport.instance != null) {
                oldTeleportButton = Teleport.instance.teleportAction;
                oldTeleportHand = Teleport.instance.teleportHand;

                Teleport.instance.teleportAction = newTeleportButton;
                Teleport.instance.teleportHand = uiHand;
            }

    //         //disable left hand
            Player.instance.GetHand(uiHand).GetComponent<InteractionPoint>().findInteractables = false;


            
        }



        void OnCloseUI (GameObject uiObject) {
            isOpen = false;
            for (int i =0 ; i < Player.instance.hands.Length; i++) {
                Player.instance.hands[i].HideController(true);   
            }


            // StandardizedVRInput.MarkActionUnoccupied(controls.list[i].action);
            // StandardizedVRInput.instance.HideHint(SteamVR_Input_Sources.Any, Player.instance.actions[Player.instance.GetComponent<ControlsManager>().inGameMenuAction.x]);    
            // StandardizedVRInput.instance.HideHint(SteamVR_Input_Sources.Any, Player.instance.actions[workshop.cancelAction]);    
            // StandardizedVRInput.instance.HideHint(SteamVR_Input_Sources.Any, Player.instance.actions[workshop.submitAction]);    
        
            if (Teleport.instance != null) {
                Teleport.instance.teleportAction = oldTeleportButton;
                Teleport.instance.teleportHand = oldTeleportHand;
            }
            Player.instance.GetHand(SteamVR_Input_Sources.LeftHand).GetComponent<InteractionPoint>().findInteractables = true;            
        }

        void Update () {
            if (isOpen) {
                workshop.ProvideAxisDeltas(StandardizedVRInput.instance.GetScrollDelta(SteamVR_Input_Sources.RightHand));
            }
        }


    }
}




//     // TODO: handle dpad input and trackpad button (seperate by magnitude on trackpad axis)

//     // TODO: inventory (pip boy) menu brought up when button up is before a certain time

//     /*

//         menu with panel to left hand, teleport switched to trackpad button left hand
//             (let teleport input regardless of ui input occupied)

//         right hand:
//             smooth turn should still work if active

//         send axis deltas from scroll motion (get from vr ui input)
//         normally would send axis deltas as raw axis values

//     */


    // public class VRWorkshopHandler : MonoBehaviour {

    // }
