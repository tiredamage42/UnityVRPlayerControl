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
        // public SteamVR_Action_Boolean mainMenuToggle, quickInventoryToggle, inventoryToggle;
        // public SteamVR_Input_Sources inventoryToggleHand = SteamVR_Input_Sources.RightHand;
        
        // Start is called before the first frame update
        
        void Start()
        {
            SetUpMessageCenter();
        }

        // Update is called once per frame
        void Update()
        {
            if (mainMenuToggle.GetStateDown(SteamVR_Input_Sources.LeftHand)) {
                VRUIInput.SetUIHand(SteamVR_Input_Sources.Any);
                GameManager.TogglePause();
            }
            
            SetUpMessageCenter();

            UpdateQuickInventory();
            UpdateNormalInventory();
        }

        InventoryUI inventoryUI;



        void OnEnable () {
        
            // VRManager.onUISelection += OnUISelection;
            // GameManager.onPauseRoutineStart += OnGamePaused;
            

            UIManager.onUISelect += OnUISelection;
            UIManager.onShowGameMessage += OnShowGameMessage;

            // quickInventory.onBaseCancel += CloseQuickInventory;


            inventoryUI = Player.instance.GetComponent<InventoryUI>();
            inventoryUI.onUIClose += OnInventoryUIClose;
            inventoryUI.onUIOpen += OnInventoryUIOpen;

            inventoryUI.SetAlternateSubmitCallback(GetAlternativeSubmits);
            // inventoryUI.SetGetWorkingInventorySlotCallback(GetWorkingInventorySlot);


            // Player.instance.GetComponent<Inventory>().onStash += OnStash;
            // Player.instance.GetComponent<Inventory>().onDrop += OnDrop;
            
			// Player.instance.GetComponent<Inventory>().onEquip += OnEquip;
            // Player.instance.GetComponent<Inventory>().onUnequip += OnUnequip;

            // UIManager.HideUI(quickInventory);
            
            
        }
        void OnDisable () {
            // GameManager.onPauseRoutineStart -= OnGamePaused;


            // VRManager.onUISelection -= OnUISelection;
            UIManager.onUISelect -= OnUISelection;
            UIManager.onShowGameMessage -= OnShowGameMessage;



            // quickInventory.onBaseCancel -= CloseQuickInventory;


            
            // Player.instance.GetComponent<Inventory>().onStash -= OnStash;
            // Player.instance.GetComponent<Inventory>().onDrop -= OnDrop;
            
			// Player.instance.GetComponent<Inventory>().onEquip -= OnEquip;
            // Player.instance.GetComponent<Inventory>().onUnequip -= OnUnequip;

        }


        // void OnStash (Inventory inventory, ItemBehavior item, int count) {
        //     VRManager.ShowGameMessage("Stashed " + item.itemName + " (x" + count+")", 0);
        // }
        // void OnDrop (Inventory inventory, ItemBehavior item, int count) {
        //     VRManager.ShowGameMessage("Dropped " + item.itemName + " (x" + count+")", 0);
        // }

        // void OnEquip (Inventory inventory, Item item, int slot, bool quickEquip) {
        //     // Debug.LogError("should show message");
        //     VRManager.ShowGameMessage("Equipped " + item.itemBehavior.itemName + " to slot " + slot + (quickEquip ? "*quick*" : ""), 0);
        // }
        // void OnUnequip (Inventory inventory, Item item, int slot, bool quickEquip) {
        //     // Debug.LogError("should show message");
        //     VRManager.ShowGameMessage("Unequipped " + item.itemBehavior.itemName + " from slot " + slot + (quickEquip ? "*quick*" : ""), 0);
        // }







        
        // System.Func<int> getWorkingInventorySlot;

        // void OnQuickInventorySubmit (GameObject[] data, object[] customData) {
		// 	// Debug.LogError("on submit");
        //     if (customData != null) {

        //         // Debug.LogError("as cistom data");

        //         int slot = -1;
        //         if (getWorkingInventorySlot != null) {
        //             slot = getWorkingInventorySlot();
        //         }
        //         else {
        //             Debug.LogError("quick inventory wasnt supplied callback to get working inventory slot, defaulting to -1");
        //         }

        //         // SteamVR_Input_Sources hand = VRUIInput.GetUIHand();
        //         // int slot = Player.instance.GetHand(hand).GetComponent<EquipPoint>().equipSlotOnBase;
        //         ItemBehavior item = (ItemBehavior)customData[0];

        //         Inventory inventory = Player.instance.GetComponent<Inventory>();

        //         if (item.stashedItemBehavior != null) {
        //             item.stashedItemBehavior.OnItemConsumed(inventory, item, slot);
        //         }

        //         // if (item.stashUseBehavior != null) {
        //         //     // Debug.LogError("stash use!");
        //         //     item.stashUseBehavior.OnStashedUse (inventory, item, Inventory.UI_USE_ACTION, slot, 1, null);
        //         // }
                
        //         // inventory.EquipItem(item, slot, null);

        //         CloseQuickInventory();
        //     }
            
		// }


        // void BuildQuickInventory () {
        //     Inventory inventory = Player.instance.GetComponent<Inventory>();
        //     SelectableElement[] allElements = quickInventory.GetAllElements(inventory.favoritesCount);

        //     for (int i =0 ; i< allElements.Length; i++) {
        //         if (i < inventory.allInventory.Count) {
        //             allElements[i].elementText = inventory.allInventory[i].item.itemName + " ("+inventory.allInventory[i].count+")";
        //             allElements[i].uiText.SetText(inventory.allInventory[i].item.itemName + " ("+inventory.allInventory[i].count+")");
        //             allElements[i].customData = new object[] { inventory.allInventory[i].item };
        //         }
        //         else {
        //             allElements[i].elementText = "Empty";
        //             allElements[i].uiText.SetText("Empty");
        //             allElements[i].customData = null;
        //         }
        //     }

        // }

		// void CloseQuickInventory () {
		// 	UIManager.HideUI(quickInventory);
		// 	UIManager.onUISubmit -= OnQuickInventorySubmit;
		// }

		// void OpenQuickInventory () {

		// 	UIManager.ShowUI(quickInventory, true, false);
		// 	UIManager.onUISubmit += OnQuickInventorySubmit;
        //     BuildQuickInventory();
		// }

        // int GetWorkingInventorySlot (InventoryUI.UIType uIType) {
        //     SteamVR_Input_Sources hand = VRUIInput.GetUIHand();
        //     return Player.instance.GetHand(hand).GetComponent<EquipPoint>().equipSlotOnBase;
        // }



        bool CheckHandForWristRadialOpen (SteamVR_Input_Sources hand) {
			if (quickInventoryToggle.GetStateDown(hand)) {
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
     

        // int GetAlternativeSubmitsForNormalInventory () {
        //     VRUIInput.SetUIHand(SteamVR_Input_Sources.Any);

        //     // StandardizedVRInput.MarkActionOccupied(quickInventoryAction, SteamVR_Input_Sources.Any);

        //     if (inventoryDropAction.GetStateDown(SteamVR_Input_Sources.Any)) {
        //         // VRUIInput.SetUIHand(SteamVR_Input_Sources.Any);
        //         return 1;
        //     }
        //     return -1;
        // }

           /*
         
        
        public const int QUICK_INVENTORY_CONSUME_ACTION = 0;
        public const int FULL_INVENTORY_CONSUME_ACTION = 0;
        public const int FULL_INVENTORY_DROP_ACTION = 1;
        public const int FULL_INVENTORY_FAVORITE_ACTION = 2;
        
        public const int QUICK_TRADE_SINGLE_TRADE_ACTION = 0;
        public const int QUICK_TRADE_TRADE_ALL_ACTION = 1;
        public const int QUICK_TRADE_SWITCH_TO_FULL_TRADE_ACTION = 2;
        
        public const int FULL_TRADE_SINGLE_TRADE_ACTION = 0;
        public const int FULL_TRADE_TRADE_ALL_ACTION = 1;
        public const int FULL_TRADE_CONSUME_ACTION = 2;
        
        
         */

        [Header("Main Menu")]
        public SteamVR_Action_Boolean mainMenuToggle;
        
        [Space]

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
                    
            SteamVR_Input_Sources hand = VRManager.Int2Hand( inventoryUI.quickTradeInteractorID );
            // SteamVR_Input_Sources hand = VRUIInput.GetUIHand();
            if (QUICK_INVENTORY_CONSUME_ACTION.GetStateDown(hand)) {
                return new Vector2Int(InventoryUI.QUICK_INVENTORY_CONSUME_ACTION, inventoryUI.quickTradeInteractorID);//VRManager.Hand2Int(hand));
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
        /*
        
        [Header("Quick Trade")]
        public SteamVR_Action_Boolean QUICK_TRADE_SINGLE_TRADE_ACTION;
        public SteamVR_Action_Boolean QUICK_TRADE_TRADE_ALL_ACTION;
        public SteamVR_Action_Boolean QUICK_TRADE_SWITCH_TO_FULL_TRADE_ACTION;
        
        [Header("Full Trade")]
        public SteamVR_Action_Boolean FULL_TRADE_SINGLE_TRADE_ACTION;
        public SteamVR_Action_Boolean FULL_TRADE_TRADE_ALL_ACTION;
        public SteamVR_Action_Boolean FULL_TRADE_CONSUME_ACTION;
        

        
         */

        void OnInventoryUIOpen(InventoryUI.UIType type, UIElementHolder uiHolder) {
            SteamVR_Input_Sources hand;
            switch (type) {
                // quick inventory
                case InventoryUI.UIType.QuickInventory:
                    hand = VRManager.Int2Hand( inventoryUI.quickInventoryInteractorID );


                    StandardizedVRInput.MarkActionOccupied(QUICK_INVENTORY_CONSUME_ACTION, VRUIInput.GetUIHand());
                    StandardizedVRInput.instance.ShowHint(hand, QUICK_INVENTORY_CONSUME_ACTION, "Use");    

                    TransformBehavior.AdjustTransform(uiHolder.baseObject.transform, Player.instance.GetHand(hand).transform, quickInventoryEquip, 0);
                    VRUIInput.SetUIHand(hand);
                
                    break;
                // full inventory
                case InventoryUI.UIType.FullInventory:
                    VRUIInput.SetUIHand(SteamVR_Input_Sources.Any);
                    StandardizedVRInput.MarkActionOccupied(FULL_INVENTORY_CONSUME_ACTION, SteamVR_Input_Sources.Any);
                    StandardizedVRInput.MarkActionOccupied(FULL_INVENTORY_DROP_ACTION, SteamVR_Input_Sources.Any);
                    // StandardizedVRInput.MarkActionOccupied(FULL_INVENTORY_FAVORITE_ACTION, SteamVR_Input_Sources.Any);
                    
                    
                    
                    StandardizedVRInput.instance.ShowHint(SteamVR_Input_Sources.RightHand, FULL_INVENTORY_CONSUME_ACTION, "Use Right Hand");
                    StandardizedVRInput.instance.ShowHint(SteamVR_Input_Sources.LeftHand, FULL_INVENTORY_CONSUME_ACTION, "Use Left Hand");
                    StandardizedVRInput.instance.ShowHint(SteamVR_Input_Sources.Any, FULL_INVENTORY_DROP_ACTION, "Drop");
                    // StandardizedVRInput.instance.ShowHint(SteamVR_Input_Sources.Any, FULL_INVENTORY_FAVORITE_ACTION, "Favorite");
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

                    VRUIInput.SetUIHand(SteamVR_Input_Sources.Any);
                    

                    StandardizedVRInput.MarkActionOccupied(FULL_TRADE_CONSUME_ACTION, SteamVR_Input_Sources.Any);
                    StandardizedVRInput.MarkActionOccupied(FULL_TRADE_TRADE_ALL_ACTION, SteamVR_Input_Sources.Any);
                    StandardizedVRInput.MarkActionOccupied(FULL_TRADE_SINGLE_TRADE_ACTION, SteamVR_Input_Sources.Any);
                    
                    StandardizedVRInput.instance.ShowHint(SteamVR_Input_Sources.RightHand, FULL_TRADE_CONSUME_ACTION, "Use Right Hand");
                    StandardizedVRInput.instance.ShowHint(SteamVR_Input_Sources.LeftHand, FULL_TRADE_CONSUME_ACTION, "Use Left Hand");
                    StandardizedVRInput.instance.ShowHint(SteamVR_Input_Sources.Any, FULL_TRADE_SINGLE_TRADE_ACTION, "Trade Item");
                    StandardizedVRInput.instance.ShowHint(SteamVR_Input_Sources.Any, FULL_TRADE_TRADE_ALL_ACTION, "Trade All");
                    break;
            }

        }
        void OnInventoryUIClose(InventoryUI.UIType type, UIElementHolder uiHolder) {
            switch (type) {
                // quick inventory
                case InventoryUI.UIType.QuickInventory:
                    StandardizedVRInput.instance.HideHint(SteamVR_Input_Sources.Any, QUICK_INVENTORY_CONSUME_ACTION);
                    break;
                // full inventory
                case InventoryUI.UIType.FullInventory:
                    StandardizedVRInput.instance.HideHint(SteamVR_Input_Sources.Any, FULL_INVENTORY_CONSUME_ACTION);
                    StandardizedVRInput.instance.HideHint(SteamVR_Input_Sources.Any, FULL_INVENTORY_DROP_ACTION);
                    // StandardizedVRInput.instance.ShowHint(SteamVR_Input_Sources.Any, FULL_INVENTORY_FAVORITE_ACTION,);
                    break;
                // quick trade
                case InventoryUI.UIType.QuickTrade:
                    StandardizedVRInput.instance.HideHint(SteamVR_Input_Sources.Any, QUICK_TRADE_SINGLE_TRADE_ACTION);
                    StandardizedVRInput.instance.HideHint(SteamVR_Input_Sources.Any, QUICK_TRADE_TRADE_ALL_ACTION);
                    StandardizedVRInput.instance.HideHint(SteamVR_Input_Sources.Any, QUICK_TRADE_SWITCH_TO_FULL_TRADE_ACTION);
                    break;
                // full trade
                case InventoryUI.UIType.FullTrade: 
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
                {
                    inventoryUI.CloseFullInventoryUI();
                }
                else {
                    inventoryUI.OpenFullInventoryUI();
                }
                
                // if (!inventoryUI.normalInventoryOpen) {
                //     VRUIInput.SetUIHand(SteamVR_Input_Sources.Any);
                //     StandardizedVRInput.MarkActionOccupied(inventoryDropAction, SteamVR_Input_Sources.Any);
                //     // StandardizedVRInput.MarkActionOccupied(inventoryFavoriteAction, SteamVR_Input_Sources.Any);

                //     StandardizedVRInput.instance.ShowHint(SteamVR_Input_Sources.RightHand, inventoryDropAction, "Drop");
                //     StandardizedVRInput.instance.ShowHint(SteamVR_Input_Sources.RightHand, VRUIInput.instance.submitButton, "Use");

                //     Debug.LogError("openin ui");
                //     inventoryUI.OpenNormalInventory(() => { return 0; }, GetAlternativeSubmitsForNormalInventory);
                // }
                // else {
                    // check for canceled as well
                    // StandardizedVRInput.instance.HideHint(SteamVR_Input_Sources.RightHand, inventoryDropAction);
                    // StandardizedVRInput.instance.HideHint(SteamVR_Input_Sources.RightHand, VRUIInput.instance.submitButton);

            
                    // StandardizedVRInput.MarkActionUnoccupied(inventoryDropAction);
                    // // StandardizedVRInput.MarkActionUnoccupied(inventoryFavoriteAction);
                    // inventoryUI.CloseNormalInventory();
                // }
            }
        } 

		
			



        public SteamVR_Input_Sources messagesHand = SteamVR_Input_Sources.LeftHand;

        public TransformBehavior messagesEquip;

        public TransformBehavior quickInventoryEquip, quickTradeEquip;

        // public UIElementHolder quickInventory;
        public UIMessageCenter messageCenter;


        // public bool quickInventoryOpen { get { return quickInventory.gameObject.activeInHierarchy; } }
		// public SteamVR_Action_Boolean quickInventoryAction;



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

        
        // void OnGamePaused(bool isPaused, float routineTime) {
        //     if (isPaused) {
        //         // if (quickInventoryOpen) {
		// 		// 	CloseQuickInventory();
		// 		// }
			
        //         VRUIInput.SetUIHand(SteamVR_Input_Sources.Any);
        //     }
        // }

        
    }
}
