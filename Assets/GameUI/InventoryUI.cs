using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// using VRPlayer;

using GameBase;
using SimpleUI;
using InventorySystem;
namespace GameUI {


public class InventoryUI : MonoBehaviour
{
    public Inventory inventory;
    public UIElementHolder quickInventory;
    public UIElementHolder normalInventory;


    public bool quickInventoryOpen { get { return quickInventory.gameObject.activeInHierarchy; } }
    public bool normalInventoryOpen { get { return normalInventory.gameObject.activeInHierarchy; } }

    void Awake () {
        if (inventory == null) {
            inventory = GetComponent<Inventory>();
        }
    }

    

        void OnEnable () {
        
            // VRManager.onUISelection += OnUISelection;
            // UIManager.onUISelect += OnUISelection;
            
            GameManager.onPauseRoutineStart += OnGamePaused;            
            quickInventory.onBaseCancel += CloseQuickInventory;
            normalInventory.onBaseCancel += CloseNormalInventory;

            inventory.onStash += OnStash;
            inventory.onDrop += OnDrop;
            inventory.onEquip += OnEquip;
            inventory.onUnequip += OnUnequip;

            UIManager.HideUI(quickInventory);
            UIManager.HideUI(normalInventory);
            
        }
        void OnDisable () {
            // UIManager.onUISelect -= OnUISelection;
            // VRManager.onUISelection -= OnUISelection;

            
            GameManager.onPauseRoutineStart -= OnGamePaused;
            quickInventory.onBaseCancel -= CloseQuickInventory;
            normalInventory.onBaseCancel -= CloseNormalInventory;
            
            inventory.onStash -= OnStash;
            inventory.onDrop -= OnDrop;
			inventory.onEquip -= OnEquip;
            inventory.onUnequip -= OnUnequip;
        }

        void OnStash (Inventory inventory, ItemBehavior item, int count) {
            UIManager.ShowGameMessage("Stashed " + item.itemName + " (x" + count+")", 0);
        }
        void OnDrop (Inventory inventory, ItemBehavior item, int count) {
            UIManager.ShowGameMessage("Dropped " + item.itemName + " (x" + count+")", 0);
        }
        void OnEquip (Inventory inventory, Item item, int slot, bool quickEquip) {
            UIManager.ShowGameMessage("Equipped " + item.itemBehavior.itemName + " to slot " + slot + (quickEquip ? "*quick*" : ""), 0);
        }
        void OnUnequip (Inventory inventory, Item item, int slot, bool quickEquip) {
            UIManager.ShowGameMessage("Unequipped " + item.itemBehavior.itemName + " from slot " + slot + (quickEquip ? "*quick*" : ""), 0);
        }



        System.Func<int> getWorkingInventorySlot;

        void OnQuickInventorySubmit (GameObject[] data, object[] customData, int submitType) {
			// Debug.LogError("on submit");
            if (customData != null) {

                // Debug.LogError("as cistom data");

                int slot = -1;
                if (getWorkingInventorySlot != null) {
                    slot = getWorkingInventorySlot();
                    getWorkingInventorySlot = null;
                }
                else {
                    Debug.LogError("quick inventory wasnt supplied callback to get working inventory slot, defaulting to -1");
                }

                // SteamVR_Input_Sources hand = VRUIInput.GetUIHand();
                // int slot = Player.instance.GetHand(hand).GetComponent<EquipPoint>().equipSlotOnBase;
                ItemBehavior item = (ItemBehavior)customData[0];

                if (item.stashedItemBehavior != null) {
                    item.stashedItemBehavior.OnItemConsumed(inventory, item, slot);
                }

                // if (item.stashUseBehavior != null) {
                //     // Debug.LogError("stash use!");
                //     item.stashUseBehavior.OnStashedUse (inventory, item, Inventory.UI_USE_ACTION, slot, 1, null);
                // }
                
                // inventory.EquipItem(item, slot, null);
            }


            CloseQuickInventory();
            
		}


    

        void BuildQuickInventory () {
            SelectableElement[] allElements = quickInventory.GetAllElements(inventory.favoritesCount);

            for (int i =0 ; i< allElements.Length; i++) {
                SelectableElement element = allElements[i];
                if (i < inventory.allInventory.Count) {
                    ItemBehavior inventoryItem = inventory.allInventory[i].item;
                    int count = inventory.allInventory[i].count;
                    string display = inventoryItem.itemName + " ( x"+count+" )";
                    element.elementText = display;
                    element.uiText.SetText(display);
                    element.customData = new object[] { inventoryItem };
                }
                else {
                    element.elementText = "Empty";
                    element.uiText.SetText("Empty");
                    element.customData = null;
                }
            }
        }




        public int maxPerPageInventory = 16;
        int currentPageOffset;

        SelectableElement[] normalInventoryUIButtons;
        void UpdateNormalInventoryUIButtons () 
        {

            // while (currentPageOffset > inventory.allInventory.Count - maxPerPageInventory) {
            //     currentPageOffset--;
            // }

            bool isAtEnd = currentPageOffset >= inventory.allInventory.Count - maxPerPageInventory;
            bool isAtBeginning = currentPageOffset == 0;
            if (!isAtBeginning) {
                normalInventoryUIButtons = normalInventory.GetAllElements(maxPerPageInventory);
                normalInventoryUIButtons[0].elementText = "** [ Page Up ] **";
                normalInventoryUIButtons[0].uiText.SetText(normalInventoryUIButtons[0].elementText);
                normalInventoryUIButtons[0].customData = new object[]{"BACK"};
            }

            int start = isAtBeginning ? 0 : 1;
            int end = isAtEnd ? maxPerPageInventory : maxPerPageInventory - 1;

            for (int i = start ; i< end; i++) {
                SelectableElement element = normalInventoryUIButtons[i];

                int inventoryIndex = ((i-start)) + (currentPageOffset);
                
                if (inventoryIndex < inventory.allInventory.Count) {
                    ItemBehavior inventoryItem = inventory.allInventory[inventoryIndex].item;
                    int count = inventory.allInventory[inventoryIndex].count;
                    
                    string display = inventoryItem.itemName + " ( x"+count+" )";
                    element.elementText = display;
                    element.uiText.SetText(display);
                    element.customData = new object[] { inventoryItem };
                }
                else {
                    element.elementText = "Empty";
                    element.uiText.SetText(element.elementText);
                    element.customData = null;
                }
            }
            if (!isAtEnd) {
                normalInventoryUIButtons[maxPerPageInventory-1].elementText = "[ Page Down ]";
                normalInventoryUIButtons[maxPerPageInventory-1].uiText.SetText(normalInventoryUIButtons[maxPerPageInventory-1].elementText);
                normalInventoryUIButtons[maxPerPageInventory-1].customData = new object[]{"FWD"};

            }


        }

        void OnNormalInventorySubmit (GameObject[] data, object[] customData, int submitType) {
			// Debug.LogError("on submit");
            if (customData != null) {
                bool updateButtons = false;
                // string asString = (string)customData[0];
                // if (asString != null) {
                    // if (asString == "BACK") {
                    //     currentPageOffset--;
                    //     updateButtons = true;
                    // } 
                    // else if (asString == "FWD") {
                    //     currentPageOffset++;
                    //     updateButtons = true;
                    // }

                // }
                // else {
                    // Debug.LogError("as cistom data");
                    // SteamVR_Input_Sources hand = VRUIInput.GetUIHand();
                    // int slot = Player.instance.GetHand(hand).GetComponent<EquipPoint>().equipSlotOnBase;
                    ItemBehavior item = (ItemBehavior)customData[0];

                    if (item != null) {


                    if (submitType == 0) {

                        int slot = -1;
                        if (getWorkingInventorySlot != null) {
                            slot = getWorkingInventorySlot();
                            getWorkingInventorySlot = null;
                        }
                        else {
                            Debug.LogError("quick inventory wasnt supplied callback to get working inventory slot, defaulting to -1");
                        }

                        if (item.OnConsume(inventory, slot)) {
                            updateButtons = true;
                        }
                    }
                    //drop
                    else if (submitType == 1) {

                        if (inventory.DropItem(item, 1, true)){

                            updateButtons = true;
                        }

                    }

                    //favorite
                    // else if (submitType == 2) {
                    //     if (
                    //     inventory.FavoriteItem(item)) {
                    //         updateButtons = true;

                    //     }
                    // }

                    // if (item.stashUseBehavior != null) {
                    //     // Debug.LogError("stash use!");
                    //     item.stashUseBehavior.OnStashedUse (inventory, item, Inventory.UI_USE_ACTION, slot, 1, null);
                    // }
                    
                    // inventory.EquipItem(item, slot, null);
                // }

                    }
                if (updateButtons){

                    UpdateNormalInventoryUIButtons();
                }


            }


            // CloseQuickInventory();
            
		}

        void OnNormalInventorySelect (GameObject[] data, object[] customData) {
			// Debug.LogError("on submit");
            if (customData != null) {
                bool updateButtons = false;
                string asString = (string)customData[0];
                SelectableElement newSelection = null;
                if (asString != null) {
                    if (asString == "BACK") {
                        currentPageOffset--;
                        if (currentPageOffset != 0) {

                            newSelection = normalInventoryUIButtons[1];
                        }

                        updateButtons = true;
                    } 
                    else if (asString == "FWD") {
                        currentPageOffset++;
                        bool isAtEnd = currentPageOffset >= inventory.allInventory.Count - maxPerPageInventory;
                        
                        if (!isAtEnd) {
                            newSelection = normalInventoryUIButtons[maxPerPageInventory - 2];
                        }
                        
                        updateButtons = true;
                    }
                }
                
                if (updateButtons){

                    UpdateNormalInventoryUIButtons();
                    if (newSelection != null) {

                        UIManager.SetSelection(newSelection.gameObject);
                    }
                }




            }


            // CloseQuickInventory();
            
		}

        void BuildNormalInventory () {
            // normalInventoryUIButtons = normalInventory.GetAllElements(maxPerPageInventory);

            // normalInventoryUIButtons[0].elementText = "** [ Page Up ] **";
            // normalInventoryUIButtons[0].uiText.SetText(normalInventoryUIButtons[0].elementText);
            // normalInventoryUIButtons[0].customData = new object[]{"BACK"};


            // for (int i = 1 ; i< normalInventoryUIButtons.Length - 1; i++) {
            //     SelectableElement element = normalInventoryUIButtons[i];

            //     int inventoryIndex = (i-1) + currentPageOffset;
                
            //     if (inventoryIndex < inventory.allInventory.Count) {
            //         ItemBehavior inventoryItem = inventory.allInventory[inventoryIndex].item;
            //         int count = inventory.allInventory[inventoryIndex].count;
                    
            //         string display = inventoryItem.itemName + " ( x"+count+" )";
            //         element.elementText = display;
            //         element.uiText.SetText(display);
            //         element.customData = new object[] { inventoryItem };
            //     }
            //     else {
            //         element.elementText = "Empty";
            //         element.uiText.SetText(element.elementText);
            //         element.customData = null;
            //     }
            // }

            // normalInventoryUIButtons[maxPerPageInventory-1].elementText = "[ Page Down ]";
            // normalInventoryUIButtons[maxPerPageInventory-1].uiText.SetText(normalInventoryUIButtons[maxPerPageInventory-1].elementText);
            // normalInventoryUIButtons[maxPerPageInventory-1].customData = new object[]{"FWD"};

            UpdateNormalInventoryUIButtons();
        }

        public void CloseNormalInventory () {
            getWorkingInventorySlot = null;
            UIManager.HideUI(normalInventory);

            // normalInventory.onSubmitEvent -= OnNormalInventorySubmit;
            // UIManager.onUISubmit -= OnNormalInventorySubmit;
        }
            
        public void OpenNormalInventory (System.Func<int> getWorkingInventorySlot, System.Func<int> getAlternateSubmits) {
            this.getWorkingInventorySlot = getWorkingInventorySlot;
            currentPageOffset = 0;

			UIManager.ShowUI(normalInventory, true, false);
			// UIManager.onUISubmit += OnNormalInventorySubmit;

            normalInventory.onSelectEvent += OnNormalInventorySelect;
            normalInventory.onSubmitEvent += OnNormalInventorySubmit;
            normalInventory.alternativeSubmit += getAlternateSubmits;
            
            BuildNormalInventory();
		}


        public void CloseQuickInventory () {
            getWorkingInventorySlot = null;
            UIManager.HideUI(quickInventory);

            // UIManager.onUISubmit -= OnQuickInventorySubmit;
            // quickInventory.onSubmitEvent -= OnQuickInventorySubmit;
            
        }

		public void OpenQuickInventory (System.Func<int> getWorkingInventorySlot) {
            this.getWorkingInventorySlot = getWorkingInventorySlot;
			UIManager.ShowUI(quickInventory, true, false);
			
            // UIManager.onUISubmit += OnQuickInventorySubmit;
            quickInventory.onSubmitEvent += OnQuickInventorySubmit;
            
            BuildQuickInventory();
		}


    void OnGamePaused(bool isPaused, float routineTime) {
        if (isPaused) {
            if (quickInventoryOpen) {
                CloseQuickInventory();
            }
            if (normalInventoryOpen) {
                CloseNormalInventory();
            }
        
         
        }
    }
    
}
}
