using System.Collections.Generic;
using UnityEngine;

using InventorySystem;
using SimpleUI;
namespace GameUI {

    public class FullTradeUIHandler : InventoryManagementUIHandler
    {

        // public override string[] GetInputNames () { return new string[] { "Trade", "Trade All", "Use" }; }
        
        // TODO: limit equip ID for consume action to 0 when equipping on ai for instance
        protected override void OnUISelect (GameObject[] data, object[] customData) { }
        protected override List<Inventory.InventorySlot> BuildInventorySlotsForDisplay (Inventory shownInventory, int uiIndex, List<int> categoryFilter) {
            return shownInventory.GetFilteredInventory(categoryFilter);
        }

        protected override void OnInventoryManagementInitiate(Inventory inventory, int usingEquipPoint, Inventory otherInventory, List<int> categoryFilter) {
            
            (uiObject.subHolders[0] as UIPage).SetTitle(inventory.GetDisplayName());
            (uiObject.subHolders[1] as UIPage).SetTitle(otherInventory.GetDisplayName());

            // CloseAllUIs();            
            SetUpButtons ( inventory, otherInventory, 0, 1, true, uiObject.subHolders[0], categoryFilter);
            SetUpButtons ( otherInventory, inventory, 1, 0, false, uiObject.subHolders[1], categoryFilter);
        }

        const int singleTradeAction = 0, tradeAllAction = 1, consumeAction = 2;

        protected override void OnUIInput (GameObject[] data, object[] customData, Vector2Int input) {
    
        	if (customData != null) {
                Inventory.InventorySlot item = (Inventory.InventorySlot)customData[0];
                    
                bool updateButtons = false;
                Inventory highlightedInventory = (Inventory)customData[1];
                Inventory otherInventory = (Inventory)customData[2];
                
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
                    UpdateUIButtons(highlightedInventory, otherInventory, (int)customData[3], (int)customData[4], null);
                    UpdateUIButtons(otherInventory, highlightedInventory, (int)customData[4], (int)customData[3], null);
                }
            }
		}    
    }
}