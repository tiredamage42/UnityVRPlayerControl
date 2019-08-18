using System.Collections.Generic;
using UnityEngine;

using Game.InventorySystem;
namespace Game.GameUI {

    public class FullTradeUIHandler : InventoryManagementUIHandler
    {
        
        // TODO: limit equip ID for consume action to 0 when equipping on ai for instance
        protected override void OnUISelect (GameObject[] data, object[] customData) { }
        
        protected override List<InventorySlot> BuildInventorySlotsForDisplay (int uiIndex, Inventory shownInventory, List<int> categoryFilter) {
            //maybe show all inventory for ui index == 1 (other guy)
            return shownInventory.GetFilteredInventory(categoryFilter);
        }
        
        protected override void OnOpenInventoryUI(Inventory inventory, int usingEquipPoint, Inventory otherInventory, List<int> categoryFilter) {
            Debug.LogError("opening full trade");
            BuildButtons(inventory.GetDisplayName(), true, 0, new object[] { inventory, otherInventory, categoryFilter });
            BuildButtons(otherInventory.GetDisplayName(), false, 1, new object[] { otherInventory, inventory, categoryFilter });
        }

        const int singleTradeAction = 0, tradeAllAction = 1, consumeAction = 2;

        protected override void OnUIInput (GameObject[] data, object[] customData, Vector2Int input, int actionOffset) {
    
        	if (customData != null) {
                InventorySlot item = customData[0] as InventorySlot;

                object[] updateParameters = customData[2] as object[];
                    
                bool updateButtons = false;

                Inventory highlightedInventory = updateParameters[0] as Inventory;
                Inventory otherInventory = updateParameters[1] as Inventory;
                
                // single trade
                if (input.x == singleTradeAction+actionOffset) {
                    if (item != null) {
                        highlightedInventory.TransferItemAlreadyInInventoryTo(item, 1, otherInventory, sendMessage: false);
                        updateButtons = true;
                    }
                }
                // take all
                else if (input.x == tradeAllAction+actionOffset) {   
                    //TODO: check if any actually transfeerrrd
                    highlightedInventory.TransferInventoryContentsTo(otherInventory, sendMessage: false);
                    updateButtons = true;
                }
                //consume on selected inventory
                else if (input.x == consumeAction+actionOffset) {
                    if (item != null) {
                        int count = 1;
                        if (item.item.OnConsume(highlightedInventory, count, input.y)){
                            updateButtons = true;
                        }
                    }
                }
                
                if (updateButtons){
                    UpdateUIButtons( (int)customData[1], updateParameters );
                    UpdateUIButtons( 1-(int)customData[1], new object[] { updateParameters[1], updateParameters[0], updateParameters[2] } );
                }
            }
		}    
    }
}