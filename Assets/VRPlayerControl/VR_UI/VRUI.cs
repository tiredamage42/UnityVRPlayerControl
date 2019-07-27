using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using InventorySystem;
using SimpleUI;
using Valve.VR;
using GameUI;
using GameBase;


namespace VRPlayer {

    public class VRUI : MonoBehaviour
    {
        void Start()
        {
            SetUpMessageCenter();
        }

        void Update()
        {
            
            SetUpMessageCenter();

            UpdateQuickInventory();
            UpdateNormalInventory();
        }

        InventoryUI inventoryUI;



        void OnEnable () {
        
            GameManager.onPauseRoutineStart += OnGamePaused;
            
            UIManager.onUISelect += OnUISelection;
            UIManager.onShowGameMessage += OnShowGameMessage;

            inventoryUI = Player.instance.GetComponent<InventoryUI>();
            inventoryUI.onUIClose += OnInventoryUIClose;
            inventoryUI.onUIOpen += OnInventoryUIOpen;

            inventoryUI.SetAlternateSubmitCallback(GetAlternativeSubmits);
            
        }
            
            
        void OnDisable () {
            GameManager.onPauseRoutineStart -= OnGamePaused;

            UIManager.onUISelect -= OnUISelection;
            UIManager.onShowGameMessage -= OnShowGameMessage;

        }



        bool CheckHandForWristRadialOpen (SteamVR_Input_Sources hand) {
			if (quickInventoryToggle.GetStateDown(hand)) {
                Debug.LogError("YO");
				inventoryUI.OpenQuickInventoryUI(VRManager.Hand2Int(hand));
                
                // VRUIInput.SetUIHand(hand);
                // TransformBehavior.AdjustTransform(inventoryUI.quickInventory.baseObject.transform, Player.instance.GetHand(hand).transform, quickInventoryEquip, 0);

				return true;
			}
			return false;
		}
        void UpdateQuickInventory () {
            if (GameManager.isPaused) 
                return;
            
            if (inventoryUI.IsOpen(InventoryUI.UIType.FullInventory)) 
                return;
            if (inventoryUI.IsOpen(InventoryUI.UIType.FullTrade)) 
                return;
            if (inventoryUI.IsOpen(InventoryUI.UIType.QuickInventory)) 
                return;
            
			if (Player.instance.handsTogether) {

                StandardizedVRInput.MarkActionOccupied(quickInventoryToggle, SteamVR_Input_Sources.Any);

                if (!CheckHandForWristRadialOpen(SteamVR_Input_Sources.LeftHand)) {
                    CheckHandForWristRadialOpen(SteamVR_Input_Sources.RightHand);
                }
            }
            else {
                StandardizedVRInput.MarkActionUnoccupied(quickInventoryToggle);
            }   
        } 
     

        // [Header("Main Menu")]
        // public SteamVR_Action_Boolean mainMenuToggle;
        
        // [Space]

        [Header("Inventory")]
        public SteamVR_Action_Boolean quickInventoryToggle, fullInventoryToggle;
        public SteamVR_Input_Sources inventoryToggleHand = SteamVR_Input_Sources.RightHand;
        
        [Space]
        [Header("UI ACTIONS")]
        [Space]
        [Header("Quick Inventory")]
        public SteamVR_Action_Boolean QUICK_INVENTORY_CONSUME_ACTION;
        
        [Header("Full Inventory")]
        public SteamVR_Action_Boolean FULL_INVENTORY_CONSUME_ACTION;
        public SteamVR_Action_Boolean FULL_INVENTORY_DROP_ACTION;
        public SteamVR_Action_Boolean FULL_INVENTORY_FAVORITE_ACTION;
        
        [Header("Quick Trade")]
        public SteamVR_Action_Boolean QUICK_TRADE_SINGLE_TRADE_ACTION;
        public SteamVR_Action_Boolean QUICK_TRADE_TRADE_ALL_ACTION;
        public SteamVR_Action_Boolean QUICK_TRADE_SWITCH_TO_FULL_TRADE_ACTION;
        
        [Header("Full Trade")]
        public SteamVR_Action_Boolean FULL_TRADE_SINGLE_TRADE_ACTION;
        public SteamVR_Action_Boolean FULL_TRADE_TRADE_ALL_ACTION;
        public SteamVR_Action_Boolean FULL_TRADE_CONSUME_ACTION;
        


        Vector2Int GetAlternativeSubmitsQI () {
                    
            if (QUICK_INVENTORY_CONSUME_ACTION.GetStateDown(VRManager.Int2Hand( inventoryUI.QI_EquipPointID ))) {
                return new Vector2Int(InventoryUI.QUICK_INVENTORY_CONSUME_ACTION, inventoryUI.QI_EquipPointID);//VRManager.Hand2Int(hand));
            }
            
            return new Vector2Int(-1, 1);
            
        
        }
        Vector2Int GetAlternativeSubmitsFI () {
            if (FULL_INVENTORY_DROP_ACTION.GetStateDown(SteamVR_Input_Sources.Any)) {
                        // VRUIInput.SetUIHand(SteamVR_Input_Sources.Any);
                        return new Vector2Int(InventoryUI.FULL_INVENTORY_DROP_ACTION, 0);
                    }
                    
                    
                    StandardizedVRInput.ButtonState[] buttonStates;
                    StandardizedVRInput.instance.GetInputActionInfo(FULL_INVENTORY_CONSUME_ACTION, out buttonStates);
                    for (int i =0 ; i < buttonStates.Length; i++) {
                        if (buttonStates[i] == StandardizedVRInput.ButtonState.Down) {
                            return new Vector2Int(InventoryUI.FULL_INVENTORY_CONSUME_ACTION, i);
                        }
                    }
                    
            return new Vector2Int(-1, 1);
        
        }
        Vector2Int GetAlternativeSubmitsQT () {
            
                    SteamVR_Input_Sources hand = VRManager.Int2Hand( inventoryUI.quickTradeInteractorID );
                    if (QUICK_TRADE_SINGLE_TRADE_ACTION.GetStateDown(hand)) {
                        return new Vector2Int(InventoryUI.QUICK_TRADE_SINGLE_TRADE_ACTION, inventoryUI.quickTradeInteractorID);
                    }
                    if (QUICK_TRADE_TRADE_ALL_ACTION.GetStateDown(hand)) {
                        return new Vector2Int(InventoryUI.QUICK_TRADE_TRADE_ALL_ACTION, inventoryUI.quickTradeInteractorID);
                    }
                    if (QUICK_TRADE_SWITCH_TO_FULL_TRADE_ACTION.GetStateDown(hand)) {
                        return new Vector2Int(InventoryUI.QUICK_TRADE_SWITCH_TO_FULL_TRADE_ACTION, inventoryUI.quickTradeInteractorID);
                    }
                    
            return new Vector2Int(-1, 1);
                    
        
        }
        Vector2Int GetAlternativeSubmitsFT () {
            
                    if (FULL_TRADE_SINGLE_TRADE_ACTION.GetStateDown(SteamVR_Input_Sources.Any)) {
                        return new Vector2Int(InventoryUI.FULL_TRADE_SINGLE_TRADE_ACTION, 0);
                    }
                    if (FULL_TRADE_TRADE_ALL_ACTION.GetStateDown(SteamVR_Input_Sources.Any)) {
                        return new Vector2Int(InventoryUI.FULL_TRADE_TRADE_ALL_ACTION, 0);
                    }

                    StandardizedVRInput.ButtonState[] buttonStates;
                    StandardizedVRInput.instance.GetInputActionInfo(FULL_TRADE_CONSUME_ACTION, out buttonStates);
                    for (int i =0 ; i < buttonStates.Length; i++) {
                        if (buttonStates[i] == StandardizedVRInput.ButtonState.Down) {
                            return new Vector2Int(InventoryUI.FULL_TRADE_CONSUME_ACTION, i);
                        }
                    }
                    
            return new Vector2Int(-1, 1);
        
        }





        System.Func<Vector2Int> GetAlternativeSubmits (InventoryUI.UIType type) {
            switch (type) {
                // quick inventory
                case InventoryUI.UIType.QuickInventory:
                    return GetAlternativeSubmitsQI;
                    
                // full inventory
                case InventoryUI.UIType.FullInventory:
                return GetAlternativeSubmitsFI;
                
                // quick trade
                case InventoryUI.UIType.QuickTrade:
                return GetAlternativeSubmitsQT;
                    
                
                case InventoryUI.UIType.FullTrade: 
                return GetAlternativeSubmitsFT;
            }
            return null;

        }
       
        void OnInventoryUIOpen(InventoryUI.UIType type, UIElementHolder uiHolder) {
            SteamVR_Input_Sources hand;
            switch (type) {
                // quick inventory
                case InventoryUI.UIType.QuickInventory:
                    hand = VRManager.Int2Hand( inventoryUI.QI_EquipPointID );

                    StandardizedVRInput.MarkActionOccupied(QUICK_INVENTORY_CONSUME_ACTION, VRUIInput.GetUIHand());
                    StandardizedVRInput.instance.ShowHint(hand, QUICK_INVENTORY_CONSUME_ACTION, "Use");    

                    TransformBehavior.AdjustTransform(uiHolder.baseObject.transform, Player.instance.GetHand(hand).transform, quickInventoryEquip, 0);
                    VRUIInput.SetUIHand(hand);
                    break;
                // full inventory
                case InventoryUI.UIType.FullInventory:
                    StandardizedVRInput.MarkActionOccupied(FULL_INVENTORY_CONSUME_ACTION, SteamVR_Input_Sources.Any);
                    StandardizedVRInput.MarkActionOccupied(FULL_INVENTORY_DROP_ACTION, SteamVR_Input_Sources.Any);
                    // StandardizedVRInput.MarkActionOccupied(FULL_INVENTORY_FAVORITE_ACTION, SteamVR_Input_Sources.Any);
                    
                    StandardizedVRInput.instance.ShowHint(SteamVR_Input_Sources.RightHand, FULL_INVENTORY_CONSUME_ACTION, "Use Right Hand");
                    StandardizedVRInput.instance.ShowHint(SteamVR_Input_Sources.LeftHand, FULL_INVENTORY_CONSUME_ACTION, "Use Left Hand");
                    StandardizedVRInput.instance.ShowHint(SteamVR_Input_Sources.Any, FULL_INVENTORY_DROP_ACTION, "Drop");
                    // StandardizedVRInput.instance.ShowHint(SteamVR_Input_Sources.Any, FULL_INVENTORY_FAVORITE_ACTION, "Favorite");
                    
                    VRUIInput.SetUIHand(SteamVR_Input_Sources.Any);
                    break;
                // quick trade
                case InventoryUI.UIType.QuickTrade:
                    hand = VRManager.Int2Hand( inventoryUI.quickTradeInteractorID );

                    StandardizedVRInput.MarkActionOccupied(QUICK_TRADE_SINGLE_TRADE_ACTION, hand);
                    StandardizedVRInput.MarkActionOccupied(QUICK_TRADE_TRADE_ALL_ACTION, hand);
                    StandardizedVRInput.MarkActionOccupied(QUICK_TRADE_SWITCH_TO_FULL_TRADE_ACTION, hand);
                    
                    StandardizedVRInput.instance.ShowHint(hand, QUICK_TRADE_SINGLE_TRADE_ACTION, "Take Item");
                    StandardizedVRInput.instance.ShowHint(hand, QUICK_TRADE_TRADE_ALL_ACTION, "Take All");
                    StandardizedVRInput.instance.ShowHint(hand, QUICK_TRADE_SWITCH_TO_FULL_TRADE_ACTION, "Open Trade");
                    
                    TransformBehavior.AdjustTransform(uiHolder.baseObject.transform, Player.instance.GetHand(hand).transform, quickTradeEquip, 0);
                    
                    VRUIInput.SetUIHand(hand);
                
                    break;
                // full trade
                case InventoryUI.UIType.FullTrade: 

                    StandardizedVRInput.MarkActionOccupied(FULL_TRADE_CONSUME_ACTION, SteamVR_Input_Sources.Any);
                    StandardizedVRInput.MarkActionOccupied(FULL_TRADE_TRADE_ALL_ACTION, SteamVR_Input_Sources.Any);
                    StandardizedVRInput.MarkActionOccupied(FULL_TRADE_SINGLE_TRADE_ACTION, SteamVR_Input_Sources.Any);
                    
                    StandardizedVRInput.instance.ShowHint(SteamVR_Input_Sources.RightHand, FULL_TRADE_CONSUME_ACTION, "Use Right Hand");
                    StandardizedVRInput.instance.ShowHint(SteamVR_Input_Sources.LeftHand, FULL_TRADE_CONSUME_ACTION, "Use Left Hand");
                    StandardizedVRInput.instance.ShowHint(SteamVR_Input_Sources.Any, FULL_TRADE_SINGLE_TRADE_ACTION, "Trade Item");
                    StandardizedVRInput.instance.ShowHint(SteamVR_Input_Sources.Any, FULL_TRADE_TRADE_ALL_ACTION, "Trade All");
                    
                    VRUIInput.SetUIHand(SteamVR_Input_Sources.Any);
                    break;
            }
        }
        void OnInventoryUIClose(InventoryUI.UIType type, UIElementHolder uiHolder) {
            switch (type) {
                // quick inventory
                case InventoryUI.UIType.QuickInventory:
                    StandardizedVRInput.MarkActionUnoccupied(QUICK_INVENTORY_CONSUME_ACTION);
                    StandardizedVRInput.instance.HideHint(SteamVR_Input_Sources.Any, QUICK_INVENTORY_CONSUME_ACTION);
                    break;
                // full inventory
                case InventoryUI.UIType.FullInventory:
                    StandardizedVRInput.MarkActionUnoccupied(FULL_INVENTORY_CONSUME_ACTION);
                    StandardizedVRInput.MarkActionUnoccupied(FULL_INVENTORY_DROP_ACTION);
                    // StandardizedVRInput.MarkActionUnoccupied(FULL_INVENTORY_FAVORITE_ACTION);
                    
                    StandardizedVRInput.instance.HideHint(SteamVR_Input_Sources.Any, FULL_INVENTORY_CONSUME_ACTION);
                    StandardizedVRInput.instance.HideHint(SteamVR_Input_Sources.Any, FULL_INVENTORY_DROP_ACTION);
                    // StandardizedVRInput.instance.ShowHint(SteamVR_Input_Sources.Any, FULL_INVENTORY_FAVORITE_ACTION,);
                    break;
                // quick trade
                case InventoryUI.UIType.QuickTrade:
                    StandardizedVRInput.MarkActionUnoccupied(QUICK_TRADE_SINGLE_TRADE_ACTION);
                    StandardizedVRInput.MarkActionUnoccupied(QUICK_TRADE_TRADE_ALL_ACTION);
                    StandardizedVRInput.MarkActionUnoccupied(QUICK_TRADE_SWITCH_TO_FULL_TRADE_ACTION);
                    
                    StandardizedVRInput.instance.HideHint(SteamVR_Input_Sources.Any, QUICK_TRADE_SINGLE_TRADE_ACTION);
                    StandardizedVRInput.instance.HideHint(SteamVR_Input_Sources.Any, QUICK_TRADE_TRADE_ALL_ACTION);
                    StandardizedVRInput.instance.HideHint(SteamVR_Input_Sources.Any, QUICK_TRADE_SWITCH_TO_FULL_TRADE_ACTION);
                    break;
                // full trade
                case InventoryUI.UIType.FullTrade: 
                    StandardizedVRInput.MarkActionUnoccupied(FULL_TRADE_CONSUME_ACTION);
                    StandardizedVRInput.MarkActionUnoccupied(FULL_TRADE_TRADE_ALL_ACTION);
                    StandardizedVRInput.MarkActionUnoccupied(FULL_TRADE_SINGLE_TRADE_ACTION);
                    
                    StandardizedVRInput.instance.HideHint(SteamVR_Input_Sources.Any, FULL_TRADE_CONSUME_ACTION);
                    StandardizedVRInput.instance.HideHint(SteamVR_Input_Sources.Any, FULL_TRADE_SINGLE_TRADE_ACTION);
                    StandardizedVRInput.instance.HideHint(SteamVR_Input_Sources.Any, FULL_TRADE_TRADE_ALL_ACTION);
                    break;
            }

        }

        void UpdateNormalInventory () {
            if (GameManager.isPaused)
                return;
            if (inventoryUI.IsOpen(InventoryUI.UIType.FullTrade))
                return;
            
            if (fullInventoryToggle.GetStateDown(inventoryToggleHand)) {
                if (inventoryUI.IsOpen(InventoryUI.UIType.FullInventory))
                    inventoryUI.CloseFullInventoryUI();
                else
                    inventoryUI.OpenFullInventoryUI();
            }
        } 

        public SteamVR_Input_Sources messagesHand = SteamVR_Input_Sources.LeftHand;
        public TransformBehavior messagesEquip;
        public TransformBehavior quickInventoryEquip, quickTradeEquip;
        public UIMessageCenter messageCenter;

        void SetUpMessageCenter () {
            Transform handTransform = Player.instance.GetHand(messagesHand).transform;
            TransformBehavior.AdjustTransform(messageCenter.transform, handTransform, messagesEquip, 0);
        }
        
        void OnUISelection (GameObject[] data, object[] customData) {
            // float duration,  float frequency, float amplitude
            StandardizedVRInput.instance.TriggerHapticPulse( VRUIInput.GetUIHand (), .1f, 1.0f, 1.0f );   
        }
        void OnShowGameMessage (string message, int key) {            
            StandardizedVRInput.instance.TriggerHapticPulse( messagesHand, .1f, 1.0f, 1.0f );   
        }

        void OnGamePaused(bool isPaused, float routineTime) {
            if (isPaused) {
                VRUIInput.SetUIHand(SteamVR_Input_Sources.Any);
            }
        }        
    }
}
