using System.Collections.Generic;
using UnityEngine;

using InventorySystem;

namespace GameUI {

    public class FullInventoryUIHandler : InventoryManagementUIHandler
    {
        

        public override bool EquipIDSpecific() { return false; }
        protected override bool UsesRadial() { return false; }
        public override string ContextKey() { return "FullInventory"; }

        protected override void OnUISelect (GameObject[] data, object[] customData) {
            if (customData != null) {
                Inventory.InventorySlot slot;
                Inventory forInventory, linkedInventory;
                int uiIndex, otherUIIndex;
                UnpackButtonData (customData, out slot, out forInventory, out linkedInventory, out uiIndex, out otherUIIndex);
                if (slot != null)
                    uiObject.textPanel.SetText(slot.item.itemDescription);
            }
            
        }


        protected override List<Inventory.InventorySlot> BuildInventorySlotsForDisplay (Inventory forInventory, List<int> categoryFilter) {
            return forInventory.allInventory;
        }


        protected override void OnInventoryManagementInitiate(Inventory inventory, int usingEquipPoint, Inventory otherInventory) {
            // CloseAllUIs();

            SetUpButtons ( inventory, null, 0, 0, true, null);
            
            // SetUpButtons (0, true, null);
            // UpdateUIButtons(inventory, null, 0, 0);
        }
        
        public const int consumeAction = 0;
        public const int dropAction = 1;
        public const int favoriteAction = 2;

        public override string[] GetInputNames () { return new string[] { "Use", "Drop", "Favorite" }; }
        

        protected override void OnUIInput (GameObject[] data, object[] customData, Vector2Int input) {
        
    		if (customData != null) {
                Inventory.InventorySlot item = (Inventory.InventorySlot)customData[0];
                if (item != null) {
                    Inventory forInventory = (Inventory)customData[1];
                    
                    bool updateButtons = false;
                    if (input.x == consumeAction) {
                    
                        int count = 1;
                        if (item.item.OnConsume(forInventory, count, input.y)){
                            updateButtons = true;
                        }
                    }
                    // drop
                    else if (input.x == dropAction) {
                        int itemIndex;
                        if (forInventory.CanDropItem(item.item, out itemIndex, out _, false)) {
                            forInventory.DropItem(item.item, 1, true, itemIndex);
                            updateButtons = true;
                        }
                    }
                    // favorite
                    else if (input.x == favoriteAction) {
                        if (forInventory.FavoriteItem(item)) {
                            updateButtons = true;
                        }
                    }
                    
                    if (updateButtons){
                        UpdateUIButtons((Inventory)customData[1], (Inventory)customData[2], (int)customData[3], (int)customData[4], null);
                    }
                }
            }
		}
    }
}