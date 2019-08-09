using System.Collections.Generic;
using UnityEngine;

using InventorySystem;

namespace GameUI {
    public class QuickTradeUIHandler : InventoryManagementUIHandler
    {
        public override bool EquipIDSpecific() { return true; }
        protected override bool UsesRadial() { return false; }
        public override string ContextKey() { return Inventory.quickTradeContext; }
        protected override void OnUISelect (GameObject[] data, object[] customData) { }


        protected override List<Inventory.InventorySlot> BuildInventorySlotsForDisplay (Inventory forInventory, List<int> categoryFilter) {
            return forInventory.allInventory;
        }

        public const int singleTradeAction = 0, tradeAllAction = 1, switchToFullTradeAction = 2;

        public override string[] GetInputNames () { return new string[] { "Take", "Take All", "Open Trade" }; }
        

        protected override void OnUIInput (GameObject[] data, object[] customData, Vector2Int input) {
    		if (customData != null) {
                bool updateButtons = false;
                Inventory shownInventory = (Inventory)customData[1];
                Inventory tradee = (Inventory)customData[2];
                
                // single trade
                if (input.x == singleTradeAction) {
                    Inventory.InventorySlot item = (Inventory.InventorySlot)customData[0];
                    if (item != null) {
                        if (shownInventory.TransferItemTo(item.item, 1, tradee)) {
                            updateButtons = true;
                        }
                    }
                }
                // take all
                else if (input.x == tradeAllAction) {
                    if (shownInventory.TransferInventoryContentsTo(tradee)) {
                        updateButtons = true;
                    }
                }
                else if (input.x == switchToFullTradeAction) {
                    tradee.EndInventoryManagement(ContextKey(), input.y);
                    tradee.InitiateInventoryManagement(Inventory.fullTradeContext, input.y, shownInventory, null);
                }
                if (updateButtons){
                    UpdateUIButtons(shownInventory, tradee, 0, 0, null);   
                }
            }
		}

        protected override void OnInventoryManagementInitiate(Inventory inventory, int usingEquipPoint, Inventory otherInventory) {
            SetUpButtons (inventory, null, 0, 0, true, null);
        }
    }
}