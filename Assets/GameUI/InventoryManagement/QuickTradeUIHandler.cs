using System.Collections.Generic;
using UnityEngine;

using InventorySystem;
using SimpleUI;
namespace GameUI {
    public class QuickTradeUIHandler : InventoryManagementUIHandler
    {
        protected override bool UsesRadial() { return false; }
        

        protected override int MaxUIPages () { return 1; }

        // public override string[] GetInputNames () { return new string[] { "Take", "Take All", "Open Trade" }; }
        protected override void OnUISelect (GameObject[] data, object[] customData) { }
        // protected override int GetUnpaginatedShowCount(object[] updateButtonsParameters) { return (updateButtonsParameters[1] as Inventory).allInventory.Count; }
        protected override List<Inventory.InventorySlot> BuildInventorySlotsForDisplay (int uiIndex, Inventory shownInventory, List<int> categoryFilter) {
            // Debug.LogError("quick trade here");
            return shownInventory.allInventory;
        }
        protected override void OnInventoryManagementInitiate(Inventory inventory, int usingEquipPoint, Inventory otherInventory, List<int> categoryFilter) {
            // SetUpButtons (otherInventory, inventory, 0, true, null, categoryFilter);
            BuildButtons(null, true, new object[] { 0, otherInventory, inventory, categoryFilter });
            (uiObject as UIPage).SetTitle(otherInventory.GetDisplayName());
        }

        const int singleTradeAction = 0, tradeAllAction = 1, switchToFullTradeAction = 2;
        protected override void OnUIInput (GameObject[] data, object[] customData, Vector2Int input) {
    		if (customData != null) {
                bool updateButtons = false;

                object[] updateButtonsParameters = customData[1] as object[];

                Inventory shownInventory = updateButtonsParameters[1] as Inventory;
                Inventory taker = updateButtonsParameters[2] as Inventory;
                
                // single trade
                if (input.x == singleTradeAction) {
                    Inventory.InventorySlot item = customData[0] as Inventory.InventorySlot;
                    if (item != null) {
                        shownInventory.TransferItemAlreadyInInventoryTo(item, 1, taker, sendMessage: false);
                    }
                }
                // take all
                else if (input.x == tradeAllAction) {

                    // TODO: check if shown inventory has anything
                    shownInventory.TransferInventoryContentsTo(taker, sendMessage: false);
                    updateButtons = true;
                }
                else if (input.x == switchToFullTradeAction) {
                    CloseUI();
                    // taker.EndInventoryManagement(context, input.y);
                    taker.InitiateInventoryManagement(Inventory.fullTradeContext, input.y, shownInventory, null);
                }
                if (updateButtons){

                    UpdateUIButtons(updateButtonsParameters);//shownInventory, taker, 0, 0, null);   
                }
            }
		}
    }
}