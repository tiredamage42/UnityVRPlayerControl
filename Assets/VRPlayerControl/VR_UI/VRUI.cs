// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

using SimpleUI;
using Valve.VR;
using GameBase;
using Game;
using Game.GameUI;
using Game.InventorySystem;
namespace VRPlayer.UI {

    public class VRUI : MonoBehaviour
    {
        // public GameObject playerObject;
        public float buildRotationOffset = 25;
        public float textPanelRotationZOffset = .1f;

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

        [Header("Slider Popup")]
        public TransformBehavior sliderPopupTransform;
        public UISliderPopupParameters sliderPopupParameters = new UISliderPopupParameters();
        
        [Header("Selection Popup")]
        public TransformBehavior selectionPopupTransform;
        public UISelectionPopupParameters selectionPopupParameters = new UISelectionPopupParameters();
        



        //TODO: get ui stuff out of actor script...
        void BuildUIs (Actor playerActor, GameObject uiObject) {

            quickInvHandler = uiObject.GetComponent<QuickInventoryUIHandler>();
            invUIHandler = uiObject.GetComponent<FullInventoryUIHandler>();
            
            UIElementHolder fullTradeHolder = VRUIBuilder.MakeFullTradeUI("FullTrade", normalPageParams, textPanelParameters, fullTradeTransform, buildRotationOffset, textPanelRotationZOffset);

            uiObject.GetComponent<CraftingUIHandler>().SetUIObject(fullTradeHolder);
            uiObject.GetComponent<FullTradeUIHandler>().SetUIObject(fullTradeHolder);
            
            quickInvHandler.SetUIObject(VRUIBuilder.MakeRadial("QuickInvRadial", radialParams));
            invUIHandler.SetUIObject(VRUIBuilder.MakeButtonsPage("PageWPanel", normalPageParams, textPanelParameters, true, pageWithPanelTransform, buildRotationOffset));
            
            uiObject.GetComponent<DialoguePlayerUIHandler>().SetUIObject(VRUIBuilder.MakeButtonsPage("Dialogue", dialogueParams, true, dialogueOptionsTransform));
            uiObject.GetComponent<QuickTradeUIHandler>().SetUIObject(VRUIBuilder.MakeButtonsPage("QuickTrade", quickTradeParams, false, null));
        

            subtitles = VRUIBuilder.MakeSubtitles("Subtitles", subtitlesParameters, subTitlesTransform);
            msgCenter = VRUIBuilder.MakeMessageCenter ("Message Center", messageCenterParameters, messageCenterTransform);

            //link the subtitles and msg center showers to the player
            playerActor.onShowMessage += msgCenter.ShowMessage;
            playerActor.onShowSubtitles += subtitles.ShowSubtitles;

            UIManager.SetUISelectionPopupInstance(VRUIBuilder.MakeSelectionPopup("SelectionPopup", selectionPopupParameters, selectionPopupTransform));
            UIManager.SetUISliderPopupInstance(VRUIBuilder.MakeSliderPopup("SliderPopup", sliderPopupParameters, sliderPopupTransform));
        }

        UISubtitles subtitles;
        UIMessageCenter msgCenter;
        void UninitializeUIs (Actor playerActor) {
            playerActor.onShowMessage -= msgCenter.ShowMessage;
            playerActor.onShowSubtitles -= subtitles.ShowSubtitles;
        }


        Inventory inventory;
        
        void OnEnable () {
            inventory = Player.instance.GetComponent<Inventory>();    
            BuildUIs(inventory.actor, GameObject.FindObjectOfType<UIObjectInitializer>().gameObject);

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

        void Update()
        { 
            if (GameManager.isPaused) return;
            UpdateNormalInventory();
            UpdateQuickInventory();
        }
         
        [Space] public SteamVR_Action_Boolean uiInvToggleAction;
        FullInventoryUIHandler invUIHandler;
        QuickInventoryUIHandler quickInvHandler;

        // open full inventory logic
        void UpdateNormalInventory () {
            if (uiInvToggleAction.GetStateDown(VRManager.instance.mainHand)) {
                if (invUIHandler.UIObjectActive())
                    invUIHandler.CloseUI();
                else
                    invUIHandler.OpenUI();
            }
        } 

        [Space] public SteamVR_Action_Boolean uiQuickInvToggleAction;
        
        // Opening quick invnetory logic
        bool CheckHandForWristRadialOpen (SteamVR_Input_Sources hand) {
			if (uiQuickInvToggleAction.GetStateDown(hand)) {
                quickInvHandler.OpenUI(new object[] { inventory, VRManager.Hand2Int(hand), null, null });
                return true;
			}
			return false;
		}
        void UpdateQuickInventory () {
            if (UIManager.AnyUIOpen())
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
