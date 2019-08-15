using System.Collections.Generic;
using UnityEngine;

using InventorySystem;
using SimpleUI;
namespace GameUI {
    public class QuickTradeUIHandler : InventoryManagementUIHandler
    {
        // public override string[] GetInputNames () { return new string[] { "Take", "Take All", "Open Trade" }; }
        protected override void OnUISelect (GameObject[] data, object[] customData) { }

        protected override List<Inventory.InventorySlot> BuildInventorySlotsForDisplay (Inventory shownInventory, int uiIndex, List<int> categoryFilter) {
            return shownInventory.allInventory;
        }
        protected override void OnInventoryManagementInitiate(Inventory inventory, int usingEquipPoint, Inventory otherInventory, List<int> categoryFilter) {
            SetUpButtons (otherInventory, inventory, 0, 0, true, null, categoryFilter);
            (uiObject as UIPage).SetTitle(otherInventory.GetDisplayName());
        }

        const int singleTradeAction = 0, tradeAllAction = 1, switchToFullTradeAction = 2;
        protected override void OnUIInput (GameObject[] data, object[] customData, Vector2Int input) {
    		if (customData != null) {
                bool updateButtons = false;
                Inventory shownInventory = (Inventory)customData[1];
                Inventory tradee = (Inventory)customData[2];
                
                // single trade
                if (input.x == singleTradeAction) {
                    Inventory.InventorySlot item = (Inventory.InventorySlot)customData[0];
                    if (item != null) {
                        shownInventory.TransferItemAlreadyInInventoryTo(item, 1, tradee, sendMessage: false);
                    }
                }
                // take all
                else if (input.x == tradeAllAction) {

                    // TODO: check if shown inventory has anything
                    shownInventory.TransferInventoryContentsTo(tradee, sendMessage: false);
                    updateButtons = true;
                }
                else if (input.x == switchToFullTradeAction) {
                    tradee.EndInventoryManagement(context, input.y);
                    tradee.InitiateInventoryManagement(Inventory.fullTradeContext, input.y, shownInventory, null);
                }
                if (updateButtons){
                    UpdateUIButtons(shownInventory, tradee, 0, 0, null);   
                }
            }
		}

        
    }
}