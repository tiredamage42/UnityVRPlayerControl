using System.Collections.Generic;
using UnityEngine;

using InventorySystem;

namespace GameUI {

    public class FullTradeUIHandler : InventoryManagementUIHandler
    {


        public override bool EquipIDSpecific() { return false; }
        protected override bool UsesRadial() { return false; }
        public override string ContextKey() { return Inventory.fullTradeContext; }

        protected override void OnUISelect (GameObject[] data, object[] customData) {

            // Inventory.InventorySlot slot;
            // Inventory forInventory, linkedInventory;
            // int uiIndex, otherUIIndex;
            // UnpackButtonData (customData, out slot, out forInventory, out linkedInventory, out uiIndex, out otherUIIndex);
            // uiObject.textPanel.SetText(slot.item.itemDescription);
            
        }


        protected override List<Inventory.InventorySlot> BuildInventorySlotsForDisplay (Inventory forInventory, List<int> categoryFilter) {
            return forInventory.allInventory;
        }

        protected override void OnInventoryManagementInitiate(Inventory inventory, int usingEquipPoint, Inventory otherInventory) {
            
            // CloseAllUIs();            
            SetUpButtons ( inventory, otherInventory, 0, 1, true, uiObject.subHolders[0]);
            SetUpButtons ( otherInventory, inventory, 1, 0, false, uiObject.subHolders[1]);
        }

        public const int singleTradeAction = 0, tradeAllAction = 1, consumeAction = 2;

        public override string[] GetInputNames () { return new string[] { "Trade", "Trade All", "Use" }; }
        
        
        protected override void OnUIInput (GameObject[] data, object[] customData, Vector2Int input) {
    
        	if (customData != null) {
                Inventory.InventorySlot item = (Inventory.InventorySlot)customData[0];
                    
                bool updateButtons = false;
                Inventory trader = (Inventory)customData[1];
                Inventory tradee = (Inventory)customData[2];
                
                // single trade
                if (input.x == singleTradeAction) {
                    if (item != null) {
                        if (trader.TransferItemTo(item.item, 1, tradee)) {
                            updateButtons = true;
                        }
                    }
                }
                // take all
                else if (input.x == tradeAllAction) {   
                    if (trader.TransferInventoryContentsTo(tradee)) {
                        updateButtons = true;
                    }
                }
                //consume on selected inventory
                else if (input.x == consumeAction) {
                    if (item != null) {
                    
                        int count = 1;
                
                        if (item.item.OnConsume(trader, count, input.y)){
                        
                            updateButtons = true;
                        }
                    }
                }
                
                if (updateButtons){
                    UpdateUIButtons(trader, tradee, (int)customData[3], (int)customData[4], null);
                    UpdateUIButtons(tradee, trader, (int)customData[4], (int)customData[3], null);
                }
            }
		}    
    }
}