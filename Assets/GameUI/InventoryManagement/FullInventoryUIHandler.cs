using System.Collections.Generic;
using UnityEngine;

using InventorySystem;
using SimpleUI;
namespace GameUI {

    public class FullInventoryUIHandler : InventoryManagementUIHandler
    {
        protected override bool UsesRadial() { return false; }
        
        protected override int MaxUIPages () { return 1; }

        // public override string[] GetInputNames () { return new string[] { "Use", "Drop", "Favorite" }; }
        
        
        protected override void OnUISelect (GameObject[] data, object[] customData) {
            if (customData != null) {
                Inventory.InventorySlot slot;
                Inventory shownInventory, linkedInventory;
                int uiIndex;//, otherUIIndex;
                UnpackButtonData (customData, out slot, out shownInventory, out linkedInventory, out uiIndex);//, out otherUIIndex);
                if (slot != null) uiObject.textPanel.SetText(slot.item.itemDescription);
            }   
        }

        // protected override int GetUnpaginatedShowCount(object[] updateButtonsParameters) { return lastSlotsCount; } 

        // int lastSlotsCount;
        protected override List<Inventory.InventorySlot> BuildInventorySlotsForDisplay (int uiIndex, Inventory shownInventory, List<int> categoryFilter) {
            List<Inventory.InventorySlot> slots = shownInventory.GetFilteredInventory(categoryFilter);
            // lastSlotsCount = slots.Count;
            return slots;
        }
        // protected override List<Inventory.InventorySlot> BuildInventorySlotsForDisplay (Inventory shownInventory, int uiIndex, List<int> categoryFilter) {
        //     return shownInventory.GetFilteredInventory(categoryFilter);
        // }
        


        protected override void OnInventoryManagementInitiate(Inventory inventory, int usingEquipPoint, Inventory otherInventory, List<int> categoryFilter) {
            // SetUpButtons ( inventory, null, 0, true, null, categoryFilter);
            BuildButtons(null, true, new object[] { 0, inventory, null, categoryFilter });


            (uiObject as UIPage).SetTitle("Inventory");
        }
        
        const int consumeAction = 0, dropAction = 1, favoriteAction = 2, openPreviousUIHandler = 3, openNextUIHandler = 4;

        public UIHandler nextUIHandler, previousUIHandler;
        
        protected override void OnUIInput (GameObject[] data, object[] customData, Vector2Int input) {
        
            if (input.x == openPreviousUIHandler) {
                CloseUI();
                previousUIHandler.OpenUI();
                return;
            }
            else if (input.x == openNextUIHandler) {
                CloseUI();
                nextUIHandler.OpenUI();
                return;
            }
    		
            if (customData != null) {

                Inventory.InventorySlot item = customData[0] as Inventory.InventorySlot;
                
                if (item != null) {
                    object[] updateButtonsParameters = customData[1] as object[];

                    Inventory shownInventory = updateButtonsParameters[1] as Inventory;
                    
                    bool updateButtons = false;
                    if (input.x == consumeAction) {
                        if (item.item.OnConsume(shownInventory, count: 1, input.y)){
                            updateButtons = true;
                        }
                    }
                    // drop
                    else if (input.x == dropAction) {
                        shownInventory.DropItemAlreadyInInventory(item, 1, true, -1, true, sendMessage: false);
                        updateButtons = true;
                    }
                    // favorite
                    else if (input.x == favoriteAction) {
                        shownInventory.FavoriteItem(item.item);//) {
                        updateButtons = true;
                    }
                    
                    if (updateButtons) {
                        UpdateUIButtons(updateButtonsParameters);//(Inventory)customData[1], (Inventory)customData[2], (int)customData[3], (int)customData[4], null);
                    }
                }
            }
		}
    }
}