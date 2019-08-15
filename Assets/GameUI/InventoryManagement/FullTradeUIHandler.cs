using System.Collections.Generic;
using UnityEngine;

using InventorySystem;
using SimpleUI;
namespace GameUI {

    public class FullTradeUIHandler : InventoryManagementUIHandler
    {
        protected override bool UsesRadial() { return false; }
        
        protected override int MaxUIPages () { return 2; }


        // public override string[] GetInputNames () { return new string[] { "Trade", "Trade All", "Use" }; }
        
        // TODO: limit equip ID for consume action to 0 when equipping on ai for instance
        protected override void OnUISelect (GameObject[] data, object[] customData) { }
        
        // int[] lastSlotsCount = new int[2];
        protected override List<Inventory.InventorySlot> BuildInventorySlotsForDisplay (int uiIndex, Inventory shownInventory, List<int> categoryFilter) {
            List<Inventory.InventorySlot> slots = shownInventory.GetFilteredInventory(categoryFilter);
            // lastSlotsCount[uiIndex] = slots.Count;
            return slots;
        }
        // protected override int GetUnpaginatedShowCount(object[] updateButtonsParameters) { return lastSlotsCount[(int)updateButtonsParameters[0]]; } 

        protected override void OnInventoryManagementInitiate(Inventory inventory, int usingEquipPoint, Inventory otherInventory, List<int> categoryFilter) {
            
            (uiObject.subHolders[0] as UIPage).SetTitle(inventory.GetDisplayName());
            (uiObject.subHolders[1] as UIPage).SetTitle(otherInventory.GetDisplayName());

            // CloseAllUIs();            
            // SetUpButtons ( inventory, otherInventory, 0, true, uiObject.subHolders[0], categoryFilter);
            // SetUpButtons ( otherInventory, inventory, 1, false, uiObject.subHolders[1], categoryFilter);


            BuildButtons(uiObject.subHolders[0], true, new object[] { 0, inventory, otherInventory, categoryFilter });
            BuildButtons(uiObject.subHolders[1], false, new object[] { 1, otherInventory, inventory, categoryFilter });
        }

        const int singleTradeAction = 0, tradeAllAction = 1, consumeAction = 2;

        protected override void OnUIInput (GameObject[] data, object[] customData, Vector2Int input) {
    
        	if (customData != null) {
                Inventory.InventorySlot item = customData[0] as Inventory.InventorySlot;

                object[] updateButtonsParameters = customData[1] as object[];
                    
                bool updateButtons = false;

                Inventory highlightedInventory = updateButtonsParameters[1] as Inventory;
                Inventory otherInventory = updateButtonsParameters[2] as Inventory;
                
                // single trade
                if (input.x == singleTradeAction) {
                    if (item != null) {
                        highlightedInventory.TransferItemAlreadyInInventoryTo(item, 1, otherInventory, sendMessage: false);
                        updateButtons = true;
                    }
                }
                // take all
                else if (input.x == tradeAllAction) {   

                    //TODO: check if any actually transfeerrrd
                    highlightedInventory.TransferInventoryContentsTo(otherInventory, sendMessage: false);
                    updateButtons = true;
                }
                //consume on selected inventory
                else if (input.x == consumeAction) {
                    if (item != null) {
                    
                        int count = 1;
                
                        if (item.item.OnConsume(highlightedInventory, count, input.y)){
                
                            updateButtons = true;
                        }
                    }
                }
                
                if (updateButtons){
                    UpdateUIButtons(
                        updateButtonsParameters
                    //     highlightedInventory, otherInventory, (int)customData[3], (int)customData[4], null);
                    );
                    UpdateUIButtons(
                        new object[] { 1 - (int)updateButtonsParameters[0], updateButtonsParameters[2], updateButtonsParameters[1], updateButtonsParameters[3] }
                    );


                    // UpdateUIButtons(
                    //     otherInventory, highlightedInventory, (int)customData[4], (int)customData[3], null);
                }
            }
		}    
    }
}