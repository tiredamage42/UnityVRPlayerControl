using UnityEngine;

using SimpleUI;
using Valve.VR;
using GameBase;

using Game;
using Game.InventorySystem;
using Game.UI;

namespace VRPlayer.UI {

    public class VRUI : MonoBehaviour
    {
        public float buildRotationOffset = 25;
        public float textPanelRotationZOffset = .1f;

        public TransformBehavior pageWithPanelTransform, fullTradeTransform, subTitlesTransform, dialogueOptionsTransform, messageCenterTransform;

        public VRMenuFollowerParameters normalVRMenuFollowParams = new VRMenuFollowerParameters();
        public VRMenuFollowerParameters messagesVRMenuFollowParams = new VRMenuFollowerParameters();
        
        [Header("Full Trade Controls Panel")]
        public Vector3 controlsPanelPositionTrade;
        public ControllerHintsPanelParameters controlsPanelParamsTrade = new ControllerHintsPanelParameters();

        [Header("Page Panel Controls Panel")]
        public Vector3 controlsPanelPositionPagePanel;
        public ControllerHintsPanelParameters controlsPanelParamsPagePanel = new ControllerHintsPanelParameters();
        
        [Header("Quick Trade Controls Panel")]
        public Vector3 controlsPanelPositionQuickTrade;
        public ControllerHintsPanelParameters controlsPanelParamsQuickTrade = new ControllerHintsPanelParameters();
        
        [Header("Workshop Controls Panel")]
        public Vector3 controlsPanelPositionWorkshop;
        public ControllerHintsPanelParameters controlsPanelParamsWorkshop = new ControllerHintsPanelParameters();
        
        [Header("Dialogue Controls Panel")]
        public Vector3 controlsPanelPositionDialogue;
        public ControllerHintsPanelParameters controlsPanelParamsDialogue = new ControllerHintsPanelParameters();
        
        [Header("Quick Inventory Controls Panel")]
        public Vector3 controlsPanelPositionQuickInv;
        public ControllerHintsPanelParameters controlsPanelParamsQuickInv = new ControllerHintsPanelParameters();
        
        [Header("Interactor Controls Panel")]
        public ControllerHintsPanelParameters controlsPanelParamsInteractor = new ControllerHintsPanelParameters();

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
        
        [Header("Workshop UI Params")]
        public UIPageParameters workshopPageParams = new UIPageParameters(new Vector2(3, .25f), .5f, .01f, TextAnchor.MiddleCenter, 0, .0025f);
        public TextPanelParameters workshopPanelParams = new TextPanelParameters(new Vector2(3, 4), new Vector2(.1f, .1f), .0025f, TextAnchor.UpperLeft, 64);
        
        //TODO: get ui stuff out of actor script...
        void BuildUIs () {

            
            UIElementHolder fullTradeHolder = VRUIBuilder.MakeFullTradeUI("Page2Panel", normalPageParams, textPanelParameters, fullTradeTransform, normalVRMenuFollowParams, buildRotationOffset, textPanelRotationZOffset, controlsPanelPositionTrade, controlsPanelParamsTrade);// = new ControllerHintsPanelParameters();

            
            
            UIElementHolder pagepanelui = VRUIBuilder.MakeButtonsPage("PageWPanel", normalPageParams, textPanelParameters, pageWithPanelTransform, normalVRMenuFollowParams, buildRotationOffset, controlsPanelPositionTrade, controlsPanelParamsTrade);// = new ControllerHintsPanelParameters();






            GameUI.craftingUI.SetUIObject(fullTradeHolder);
            GameUI.tradeUI.SetUIObject(fullTradeHolder);
            
            GameUI.quickInventoryUI.SetUIObject(VRUIBuilder.MakeRadial("QuickInvRadial", radialParams, controlsPanelPositionQuickInv, controlsPanelParamsQuickInv));// = new ControllerHintsPanelParameters();
        
            
            
            
            GameUI.inventoryManagementUI.SetUIObject(pagepanelui);
            
            GameUI.quickTradeUI.SetUIObject(VRUIBuilder.MakeButtonsPage("QuickTrade", quickTradeParams, null, null, controlsPanelPositionQuickTrade, controlsPanelParamsQuickTrade));// = new ControllerHintsPanelParameters();
        
            GameUI.workshopUI.SetUIObject(VRUIBuilder.MakeButtonsPage("Workshop", workshopPageParams, workshopPanelParams, null, null, 0, controlsPanelPositionWorkshop, controlsPanelParamsWorkshop));// = new ControllerHintsPanelParameters();
        
            
            
            GameUI.dialogueResponseUI.SetUIObject(VRUIBuilder.MakeButtonsPage("Dialogue", dialogueParams, dialogueOptionsTransform, normalVRMenuFollowParams, controlsPanelPositionDialogue, controlsPanelParamsDialogue));//
        
            for (int i = 0; i < 2; i++) {
                GameUI.AddInteractorControllerHintsPanel(VRUIBuilder.InstantiateControllerPanelFull ("InteractorHints" + i, controlsPanelParamsInteractor));
            }
        


            GameUI.gameValuesUI.SetUIObject(pagepanelui);
            GameUI.perksUI.SetUIObject(fullTradeHolder);
            GameUI.questsUI.SetUIObject(fullTradeHolder);

            GameUI.SetUIMessageCenterInstance ( VRUIBuilder.MakeMessageCenter ("Message Center", messageCenterParameters, messageCenterTransform, messagesVRMenuFollowParams) );
            GameUI.SetUISubtitlesInstance( VRUIBuilder.MakeSubtitles("Subtitles", subtitlesParameters, subTitlesTransform, normalVRMenuFollowParams) );
            GameUI.SetUISelectionPopupInstance( VRUIBuilder.MakeSelectionPopup("SelectionPopup", selectionPopupParameters, selectionPopupTransform, normalVRMenuFollowParams));
            GameUI.SetUISliderPopupInstance( VRUIBuilder.MakeSliderPopup("SliderPopup", sliderPopupParameters, sliderPopupTransform, normalVRMenuFollowParams));
        }


        Inventory inventory;
        
        void OnEnable () {
            inventory = Player.instance.GetComponent<Inventory>();    
            BuildUIs();

            GameManager.onPauseRoutineStart += OnGamePaused;
            GameUI.onOpenInteractionHint += OnOpenInteractorHint;
            UIManager.onAnyUISelect += OnUISelection;            
            UIManager.onPopupOpen += OnPopupOpen;
            UIManager.onPopupClose += OnPopupClose;

        }
            
        void OnDisable () {
            GameManager.onPauseRoutineStart -= OnGamePaused;
            GameUI.onOpenInteractionHint -= OnOpenInteractorHint;
            UIManager.onAnyUISelect -= OnUISelection;
            UIManager.onPopupOpen -= OnPopupOpen;
            UIManager.onPopupClose -= OnPopupClose;
        }

        // SteamVR_Input_Sources uiHandBeforePopup;
        int uiHandBeforePopup;
        
        void OnPopupOpen () {
            // uiHandBeforePopup = currentUIHand;
            uiHandBeforePopup = ControlsManager.currentUIController;

            // SetUIHand(SteamVR_Input_Sources.Any);
            ControlsManager.SetUIController(-1);
        }

        void OnPopupClose () {
            // SetUIHand(uiHandBeforePopup);
            ControlsManager.SetUIController(uiHandBeforePopup);
        }


        public TransformBehavior interactionHintTransform;

        void OnOpenInteractorHint (GameObject uiObject, int controllerIndex) {
            TransformBehavior.AdjustTransform(uiObject.transform, Player.instance.GetHand(VRManager.Int2Hand( controllerIndex )).transform, interactionHintTransform, controllerIndex);
        }
            
        void OnUISelection (GameObject selectedObject, GameObject[] data, object[] customData) {
            // float duration,  float frequency, float amplitude
            // StandardizedVRInput.instance.TriggerHapticPulse( VRUIInput.GetUIHand (), .1f, 1.0f, 1.0f );   
            Player.instance.TriggerHapticPulse( VRManager.Int2Hand( ControlsManager.GetUIController () ), .1f, 1.0f, 1.0f );   
        }
        
        void OnGamePaused(bool isPaused, float routineTime) {
            if (isPaused) {
                ControlsManager.SetUIController(-1);
                // VRUIInput.SetUIHand(SteamVR_Input_Sources.Any);
            }
        }        

        void Update()
        { 
            if (GameManager.isPaused) return;
            UpdateQuickInventory();
        }

        
        // [Space] public SteamVR_Action_Boolean uiQuickInvToggleAction;
        [Space] public int uiQuickInvToggleAction;
        
        // Opening quick invnetory logic
        bool CheckHandForWristRadialOpen (SteamVR_Input_Sources hand) {
            int handInt = VRManager.Hand2Int(hand);
            if (ControlsManager.GetActionStart(uiQuickInvToggleAction, handInt)) {
			// if (uiQuickInvToggleAction.GetStateDown(hand)) {
                GameUI.quickInventoryUI.OpenQuickInventoryUI(handInt, inventory);
                return true;
			}
			return false;
		}
        void UpdateQuickInventory () {
            if (UIManager.AnyUIOpen(out _))
                return;
            
			if (Player.instance.handsTogether) {
                
                
                ControlsManager.MarkActionOccupied(uiQuickInvToggleAction, -1);
                // StandardizedVRInput.MarkActionOccupied(uiQuickInvToggleAction, SteamVR_Input_Sources.Any);

                if (!CheckHandForWristRadialOpen(SteamVR_Input_Sources.LeftHand)) {
                    CheckHandForWristRadialOpen(SteamVR_Input_Sources.RightHand);
                }
            }
            else {
                
                ControlsManager.MarkActionUnoccupied(uiQuickInvToggleAction);
                // StandardizedVRInput.MarkActionUnoccupied(uiQuickInvToggleAction);
            }   
        }   


    }
}
