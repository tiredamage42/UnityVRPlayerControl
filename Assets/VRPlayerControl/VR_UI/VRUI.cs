// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

using InventorySystem;
using SimpleUI;
using Valve.VR;
using GameUI;
using GameBase;
using ActorSystem;

namespace VRPlayer.UI {

    public class VRUI : MonoBehaviour
    {
        // public GameObject playerObject;

        public float buildRotationOffset = 25;


        public TransformBehavior pageWithPanelTransform, fullTradeTransform, subTitlesTransform, dialogueOptionsTransform, messageCenterTransform;

        [Header("Normal Page Params")]
        public UIPageParameters normalPageParams = new UIPageParameters(new Vector2(3, .25f), .5f, .01f, TextAnchor.MiddleLeft, .05f, .0025f);
        
        [Header("Text Panel Params")]
        public TextPanelParameters textPanelParameters = new TextPanelParameters(new Vector2(3, 4), new Vector2(.1f, .1f), .0025f, TextAnchor.UpperLeft, 64);
        
        [Header("Quick Inventory Radial Params")]
        public UIRadialParameters radialParams = new UIRadialParameters(.8f, .1f, .0035f);

        [Header("Quick Trade Page Params")]
        public UIPageParameters quickTradeParams = new UIPageParameters(new Vector2(3, .25f), .5f, .01f, TextAnchor.MiddleCenter, 0, .0025f);

        [Header("Dialogue Options Page Params")]
        public UIPageParameters dialogueParams = new UIPageParameters(new Vector2(4, .25f), .5f, .01f, TextAnchor.MiddleLeft, 0.05f, .0025f);

        [Header("Subtitiles Params")]
        public UISubtitlesParameters subtitlesParameters = new UISubtitlesParameters(.005f, new Vector2(9, .4f), 3, 64);        

        [Header("Message Center Params")]
        public UIMessageCenterParameters messageCenterParameters = new UIMessageCenterParameters(TextAnchor.MiddleLeft, 1, new Vector2(4, .5f), .005f, .05f, 3, new Vector2(.1f, .1f), .05f, 1);
        
        //TODO: get ui stuff out of actor script...
        void BuildUIs (Actor playerActor) {
            // if (playerObject == null)
            // {
            //     Debug.LogError("no inventory uis object specified");
            //     return;
            // }

            UIElementHolder fullTradeHolder = VRUIBuilder.MakeFullTradeUI("FullTrade", normalPageParams, textPanelParameters, fullTradeTransform, buildRotationOffset);

            playerActor.GetComponent<CraftingUIHandler>().SetUIObject(fullTradeHolder);
            playerActor.GetComponent<FullTradeUIHandler>().SetUIObject(fullTradeHolder);
            playerActor.GetComponent<QuickInventoryUIHandler>().SetUIObject(VRUIBuilder.MakeRadial("QuickInvRadial", radialParams));
            
            playerActor.GetComponent<FullInventoryUIHandler>().SetUIObject(VRUIBuilder.MakeButtonsPage("PageWPanel", normalPageParams, textPanelParameters, true, pageWithPanelTransform, buildRotationOffset));
            
            dialoguePlayerUIHandler.SetUIObject(VRUIBuilder.MakeButtonsPage("Dialogue", dialogueParams, true, dialogueOptionsTransform));
            
            playerActor.GetComponent<QuickTradeUIHandler>().SetUIObject(VRUIBuilder.MakeButtonsPage("QuickTrade", quickTradeParams, false, null));

            subtitles = VRUIBuilder.MakeSubtitles("Subtitles", subtitlesParameters, subTitlesTransform);
            msgCenter = VRUIBuilder.MakeMessageCenter ("Message Center", messageCenterParameters, messageCenterTransform);

            //link the subtitles and msg center showers to the player
            playerActor.onShowMessage += msgCenter.ShowMessage;
            playerActor.onShowSubtitles += subtitles.ShowSubtitles;

            dialoguePlayerUIHandler.onUIOpen += OnOpenDialogueUI;


            // msgCenter.onShowMessage += OnShowGameMessage;

        }

        UISubtitles subtitles;
        UIMessageCenter msgCenter;
        DialoguePlayerUIHandler dialoguePlayerUIHandler;

        void UninitializeUIs (Actor playerActor) {
            playerActor.onShowMessage -= msgCenter.ShowMessage;
            playerActor.onShowSubtitles -= subtitles.ShowSubtitles;
            dialoguePlayerUIHandler.onUIOpen -= OnOpenDialogueUI;

            // msgCenter.onShowMessage -= OnShowGameMessage;

        }

        Inventory inventory;
        public SteamVR_Input_Sources dialogueHand = SteamVR_Input_Sources.RightHand;
        void OnOpenDialogueUI (GameObject uiObject, object[] parameters) {
        // void OnOpenDialogueUI (UIElementHolder uiObject) {
        
            // VRUIInput.SetUIHand(dialogueHand);
        }

        void OnEnable () {
            inventory = Player.instance.GetComponent<Inventory>();
            dialoguePlayerUIHandler = inventory.GetComponent<DialoguePlayerUIHandler>();
            
            BuildUIs(inventory.actor);

            
            GameManager.onPauseRoutineStart += OnGamePaused;
            UIManager.onAnyUISelect += OnUISelection;
            
        }
            
            
        void OnDisable () {

            GameManager.onPauseRoutineStart -= OnGamePaused;
            UIManager.onAnyUISelect -= OnUISelection;
            
            UninitializeUIs(inventory.actor);
        }

        
        void OnUISelection (GameObject[] data, object[] customData) {
            // float duration,  float frequency, float amplitude
            StandardizedVRInput.instance.TriggerHapticPulse( VRUIInput.GetUIHand (), .1f, 1.0f, 1.0f );   
        }
        
        void OnGamePaused(bool isPaused, float routineTime) {
            if (isPaused) {
                VRUIInput.SetUIHand(SteamVR_Input_Sources.Any);
            }
        }        

        
        

        // public SteamVR_Input_Sources messagesHand = SteamVR_Input_Sources.LeftHand;
        // void OnShowGameMessage (string message) {            
        //     StandardizedVRInput.instance.TriggerHapticPulse( messagesHand, .1f, 1.0f, 1.0f );   
        // }


        
       
        void Update()
        { 
            if (GameManager.isPaused) return;
            UpdateNormalInventory();
            UpdateQuickInventory();
       
        }
         
        [Space] public SteamVR_Action_Boolean uiInvToggleAction;
        // public string invContext = "FullInventory";
        // InventoryManagementUIHandler invUIHandler;
        FullInventoryUIHandler invUIHandler;

        // open full inventory logic
        void UpdateNormalInventory () {
            if (uiInvToggleAction.GetStateDown(VRManager.instance.mainHand)) {
                if (invUIHandler == null) invUIHandler = GameObject.FindObjectOfType<FullInventoryUIHandler>();// InventoryManagementUIHandler.GetUIHandlerByContext(Inventory.fullInventoryContext);
                
                if (invUIHandler.UIObjectActive())
                    inventory.EndInventoryManagement (Inventory.fullInventoryContext, 0);
                else
                    inventory.InitiateInventoryManagement (Inventory.fullInventoryContext, 0, null, null);
                    
            }
        } 

        [Space] public SteamVR_Action_Boolean uiQuickInvToggleAction;
        // public string quickInvContext = "QuickInventory";
       
        // Opening quick invnetory logic
        bool CheckHandForWristRadialOpen (SteamVR_Input_Sources hand) {
			if (uiQuickInvToggleAction.GetStateDown(hand)) {
                inventory.InitiateInventoryManagement (Inventory.quickInventoryContext, VRManager.Hand2Int(hand), null, null);
				return true;
			}
			return false;
		}
        void UpdateQuickInventory () {

            if (UIManager.AnyUIOpen())//-1))
                return;
            
			if (Player.instance.handsTogether) {
                
                StandardizedVRInput.MarkActionOccupied(uiQuickInvToggleAction, SteamVR_Input_Sources.Any);

                if (!CheckHandForWristRadialOpen(SteamVR_Input_Sources.LeftHand)) {
                    CheckHandForWristRadialOpen(SteamVR_Input_Sources.RightHand);
                }
            }
            else {
                StandardizedVRInput.MarkActionUnoccupied(uiQuickInvToggleAction);
            }   
        }   


    }
}
