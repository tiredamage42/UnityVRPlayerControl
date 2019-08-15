using System.Collections.Generic;
using UnityEngine;

using InventorySystem;
using SimpleUI;
namespace GameUI {

    public class FullInventoryUIHandler : InventoryManagementUIHandler
    {
        // public override string[] GetInputNames () { return new string[] { "Use", "Drop", "Favorite" }; }
        
        
        protected override void OnUISelect (GameObject[] data, object[] customData) {
            if (customData != null) {
                Inventory.InventorySlot slot;
                Inventory shownInventory, linkedInventory;
                int uiIndex, otherUIIndex;
                UnpackButtonData (customData, out slot, out shownInventory, out linkedInventory, out uiIndex, out otherUIIndex);
                if (slot != null) uiObject.textPanel.SetText(slot.item.itemDescription);
            }   
        }

        protected override List<Inventory.InventorySlot> BuildInventorySlotsForDisplay (Inventory shownInventory, int uiIndex, List<int> categoryFilter) {
            return shownInventory.GetFilteredInventory(categoryFilter);
        }

        protected override void OnInventoryManagementInitiate(Inventory inventory, int usingEquipPoint, Inventory otherInventory, List<int> categoryFilter) {
            SetUpButtons ( inventory, null, 0, 0, true, null, categoryFilter);
            (uiObject as UIPage).SetTitle("Inventory");
        }
        
        const int consumeAction = 0, dropAction = 1, favoriteAction = 2;
        
        protected override void OnUIInput (GameObject[] data, object[] customData, Vector2Int input) {
        
    		if (customData != null) {
                Inventory.InventorySlot item = (Inventory.InventorySlot)customData[0];
                
                if (item != null) {
                    Inventory shownInventory = (Inventory)customData[1];
                    
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
                    
                    if (updateButtons){
                        UpdateUIButtons((Inventory)customData[1], (Inventory)customData[2], (int)customData[3], (int)customData[4], null);
                    }
                }
            }
		}
    }
}