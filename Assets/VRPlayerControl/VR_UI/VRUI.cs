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
        public SteamVR_Action_Boolean mainMenuToggle, quickInventoryToggle, inventoryToggle;
        
        public SteamVR_Action_Boolean inventoryDropAction;//, quickInventoryToggle, inventoryToggle;
        public SteamVR_Input_Sources inventoryToggleHand = SteamVR_Input_Sources.RightHand;

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

        int GetWorkingInventorySlot () {
            SteamVR_Input_Sources hand = VRUIInput.GetUIHand();
            return Player.instance.GetHand(hand).GetComponent<EquipPoint>().equipSlotOnBase;
        }



        bool CheckHandForWristRadialOpen (SteamVR_Input_Sources hand) {
			if (quickInventoryToggle.GetStateDown(hand)) {
                TransformBehavior.AdjustTransform(inventoryUI.quickInventory.baseObject.transform, Player.instance.GetHand(hand).transform, quickInventoryEquip, 0);
                VRUIInput.SetUIHand(hand);

				inventoryUI.OpenQuickInventory(GetWorkingInventorySlot);
				return true;
			}
			return false;
		}
        void UpdateQuickInventory () {
			if (!GameManager.isPaused && !inventoryUI.quickInventoryOpen && Player.instance.handsTogether) {

                StandardizedVRInput.MarkActionOccupied(quickInventoryToggle, SteamVR_Input_Sources.Any);

                if (!CheckHandForWristRadialOpen(SteamVR_Input_Sources.LeftHand)) {
                    CheckHandForWristRadialOpen(SteamVR_Input_Sources.RightHand);
                }
            }
            else {
                StandardizedVRInput.MarkActionUnoccupied(quickInventoryToggle);
            }   
        } 


        int GetAlternativeSubmitsForNormalInventory () {
            VRUIInput.SetUIHand(SteamVR_Input_Sources.Any);

            // StandardizedVRInput.MarkActionOccupied(quickInventoryAction, SteamVR_Input_Sources.Any);

            if (inventoryDropAction.GetStateDown(SteamVR_Input_Sources.Any)) {
                // VRUIInput.SetUIHand(SteamVR_Input_Sources.Any);
                return 1;
            }
            return -1;
        }

        void UpdateNormalInventory () {
            if (GameManager.isPaused)
                return;

            
            
                    
            if (inventoryToggle.GetStateDown(inventoryToggleHand)) {
                if (!inventoryUI.normalInventoryOpen) {
                    VRUIInput.SetUIHand(SteamVR_Input_Sources.Any);
                    StandardizedVRInput.MarkActionOccupied(inventoryDropAction, SteamVR_Input_Sources.Any);
                    // StandardizedVRInput.MarkActionOccupied(inventoryFavoriteAction, SteamVR_Input_Sources.Any);

                    StandardizedVRInput.instance.ShowHint(SteamVR_Input_Sources.RightHand, inventoryDropAction, "Drop");
                    StandardizedVRInput.instance.ShowHint(SteamVR_Input_Sources.RightHand, VRUIInput.instance.submitButton, "Use");

                    Debug.LogError("openin ui");
                    inventoryUI.OpenNormalInventory(() => { return 0; }, GetAlternativeSubmitsForNormalInventory);
                }
                else {
                    // check for canceled as well
                    StandardizedVRInput.instance.HideHint(SteamVR_Input_Sources.RightHand, inventoryDropAction);
                    StandardizedVRInput.instance.HideHint(SteamVR_Input_Sources.RightHand, VRUIInput.instance.submitButton);

                    StandardizedVRInput.MarkActionUnoccupied(inventoryDropAction);
                    // StandardizedVRInput.MarkActionUnoccupied(inventoryFavoriteAction);
            
                    inventoryUI.CloseNormalInventory();
                }
            }
        } 

		
			



        public SteamVR_Input_Sources messagesHand = SteamVR_Input_Sources.LeftHand;

        public TransformBehavior messagesEquip, quickInventoryEquip;

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
