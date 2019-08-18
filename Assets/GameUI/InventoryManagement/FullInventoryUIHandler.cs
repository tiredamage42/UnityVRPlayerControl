using System.Collections.Generic;
using UnityEngine;

using Game.InventorySystem;
using SimpleUI;
namespace Game.GameUI {

    public class FullInventoryUIHandler : InventoryManagementUIHandler
    {
        
        protected override void OnUISelect (GameObject[] data, object[] customData) {
            if (customData != null) {
                InventorySlot slot = customData[0] as InventorySlot;
                if (slot != null) (uiObject as SimpleUI.UIPage).textPanel.SetText(slot.item.itemDescription);
            }   
        }

        protected override List<InventorySlot> BuildInventorySlotsForDisplay (int uiIndex, Inventory shownInventory, List<int> categoryFilter) {
            return shownInventory.GetFilteredInventory(categoryFilter);
        }
        
        protected override void OnOpenInventoryUI(Inventory inventory, int usingEquipPoint, Inventory otherInventory, List<int> categoryFilter) {
            BuildButtons("Inventory", true, 0, new object[] { inventory, null, categoryFilter });
        }
        
        const int consumeAction = 0, dropAction = 1, favoriteAction = 2;

        protected override void OnUIInput (GameObject[] data, object[] customData, Vector2Int input, int actionOffset) {
            if (customData != null) {

                InventorySlot item = customData[0] as InventorySlot;
                
                if (item != null) {
                    object[] updateButtonsParameters = customData[2] as object[];

                    Inventory shownInventory = updateButtonsParameters[0] as Inventory;
                    
                    bool updateButtons = false;
                    if (input.x == consumeAction+actionOffset) {
                        if (item.item.OnConsume(shownInventory, count: 1, input.y)){
                            updateButtons = true;
                        }
                    }
                    // drop
                    else if (input.x == dropAction+actionOffset) {
                        shownInventory.DropItem(item, 1, true, -1, true, sendMessage: false);
                        updateButtons = true;
                    }
                    // favorite
                    else if (input.x == favoriteAction+actionOffset) {
                        shownInventory.FavoriteItem(item.item);
                        updateButtons = true;
                    }
                    if (updateButtons) {
                        UpdateUIButtons(0, updateButtonsParameters);
                    }
                }
            }
		}
    }
}