using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using InventorySystem;

using SimpleUI;

using Valve.VR;

namespace VRPlayer {

    public class VRUI : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            SetUpMessageCenter();
        }

        // Update is called once per frame
        void Update()
        {
            
            SetUpMessageCenter();

            UpdateQuickInventory();
        }



        void OnEnable () {
        
            VRManager.onUISelection += OnUISelection;
            VRManager.onGamePaused += OnGamePaused;
            VRManager.onShowGameMessage += OnShowGameMessage;

            quickInventory.onBaseCancel += CloseQuickInventory;


            Player.instance.GetComponent<Inventory>().onStash += OnStash;
            Player.instance.GetComponent<Inventory>().onDrop += OnDrop;
            
			Player.instance.GetComponent<Inventory>().onEquip += OnEquip;
            Player.instance.GetComponent<Inventory>().onUnequip += OnUnequip;
            
            
        }
        void OnDisable () {
            VRManager.onGamePaused -= OnGamePaused;
            VRManager.onUISelection -= OnUISelection;
            VRManager.onShowGameMessage -= OnShowGameMessage;

                        quickInventory.onBaseCancel -= CloseQuickInventory;


            
            Player.instance.GetComponent<Inventory>().onStash -= OnStash;
            Player.instance.GetComponent<Inventory>().onDrop -= OnDrop;
            
			Player.instance.GetComponent<Inventory>().onEquip -= OnEquip;
            Player.instance.GetComponent<Inventory>().onUnequip -= OnUnequip;

        }


        void OnStash (Inventory inventory, ItemBehavior item, int count) {
            VRManager.ShowGameMessage("Stashed " + item.itemName + " (x" + count+")", 0);
        }
        void OnDrop (Inventory inventory, ItemBehavior item, int count) {
            VRManager.ShowGameMessage("Dropped " + item.itemName + " (x" + count+")", 0);
        }

        void OnEquip (Inventory inventory, Item item, int slot, bool quickEquip) {
            Debug.LogError("should show message");
            VRManager.ShowGameMessage("Equipped " + item.itemBehavior.itemName + " to slot " + slot + (quickEquip ? "*quick*" : ""), 0);
        }
        void OnUnequip (Inventory inventory, Item item, int slot, bool quickEquip) {
            Debug.LogError("should show message");
            VRManager.ShowGameMessage("Unequipped " + item.itemBehavior.itemName + " from slot " + slot + (quickEquip ? "*quick*" : ""), 0);
        }


        void OnQuickInventorySubmit (GameObject[] data, object[] customData) {
			Debug.LogError("on submit");
            if (customData != null) {

                Debug.LogError("as cistom data");

                SteamVR_Input_Sources hand = VRUIInput.GetUIHand();
                int slot = Player.instance.GetHand(hand).GetComponent<EquipPoint>().equipSlotOnBase;
                ItemBehavior item = (ItemBehavior)customData[0];

                Inventory inventory = Player.instance.GetComponent<Inventory>();

                if (item.stashUseBehavior != null) {
                    Debug.LogError("stash use!");
                    item.stashUseBehavior.OnStashedUse (inventory, item, Inventory.UI_USE_ACTION, slot, 1, null);
                }
                
                // inventory.EquipItem(item, slot, null);

                CloseQuickInventory();
            }
            
		}


        void BuildQuickInventory () {
            Inventory inventory = Player.instance.GetComponent<Inventory>();
            SelectableElement[] allElements = quickInventory.GetAllElements(inventory.favoritesCount);

            for (int i =0 ; i< allElements.Length; i++) {
                if (i < inventory.allInventory.Count) {
                    allElements[i].elementText = inventory.allInventory[i].item.itemName + " ("+inventory.allInventory[i].count+")";
                    allElements[i].uiText.SetText(inventory.allInventory[i].item.itemName + " ("+inventory.allInventory[i].count+")");
                    allElements[i].customData = new object[] { inventory.allInventory[i].item };
                }
                else {
                    allElements[i].elementText = "Empty";
                    allElements[i].uiText.SetText("EMPTY");
                    
                    allElements[i].customData = null;
                }
            }

        }

		void CloseQuickInventory () {
			UIManager.HideUI(quickInventory);
			VRManager.onUISubmit -= OnQuickInventorySubmit;
		}

		void OpenQuickInventory (SteamVR_Input_Sources hand) {

            
			UIManager.ShowUI(quickInventory, true, false);

            TransformBehavior.AdjustTransform(quickInventory.baseObject.transform, Player.instance.GetHand(hand).transform, quickInventoryEquip, 0);
			VRUIInput.SetUIHand(hand);
			VRManager.onUISubmit += OnQuickInventorySubmit;

            BuildQuickInventory();
		}

        bool CheckHandForWristRadialOpen (SteamVR_Input_Sources hand) {
			if (quickInventoryAction.GetStateDown(hand)) {
				OpenQuickInventory(hand);
				return true;
			}
			return false;
		}
        void UpdateQuickInventory () {
			if (!VRManager.gamePaused && !quickInventoryOpen && Player.instance.handsTogether) {

                StandardizedVRInput.MarkActionOccupied(quickInventoryAction, SteamVR_Input_Sources.Any);

                if (!CheckHandForWristRadialOpen(SteamVR_Input_Sources.LeftHand)) {
                    CheckHandForWristRadialOpen(SteamVR_Input_Sources.RightHand);
                }
            }
            else {
                StandardizedVRInput.MarkActionUnoccupied(quickInventoryAction);
            }   
        } 

		
			



        public SteamVR_Input_Sources messagesHand = SteamVR_Input_Sources.LeftHand;

        public TransformBehavior messagesEquip, quickInventoryEquip;

        public UIElementHolder quickInventory;
        public UIMessageCenter messageCenter;


        public bool quickInventoryOpen { get { return quickInventory.gameObject.activeInHierarchy; } }
		public SteamVR_Action_Boolean quickInventoryAction;



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

        
        void OnGamePaused(bool isPaused) {
            if (isPaused) {
                if (quickInventoryOpen) {
					CloseQuickInventory();
				}
			
                VRUIInput.SetUIHand(SteamVR_Input_Sources.Any);
            }
        }

        
    }
}
