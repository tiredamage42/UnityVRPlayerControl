using System.Collections.Generic;
using UnityEngine;
using InventorySystem;

namespace GameUI {
    public class QuickInventoryUIHandler : InventoryManagementUIHandler
    {
        protected override bool UsesRadial() { return true; }
        
        protected override int MaxUIPages () { return 1; }

        // public override string[] GetInputNames () { return new string[] { "Use" }; }

        protected override void OnUISelect (GameObject[] data, object[] customData) { }

        // protected override int GetUnpaginatedShowCount(object[] updateButtonsParameters) { return maxButtons; } 
        

        protected override List<Inventory.InventorySlot> BuildInventorySlotsForDisplay ( int uiIndex, Inventory shownInventory, List<int> categoryFilter) {
            List<Inventory.InventorySlot> r = new List<Inventory.InventorySlot>();
            for (int i = 0; i < shownInventory.favorites.Count; i++) {
                r.Add(shownInventory.allInventory[shownInventory.favorites[i]]);
            }
            return r;
        }

        protected override void OnInventoryManagementInitiate(Inventory inventory, int usingEquipPoint, Inventory otherInventory, List<int> categoryFilter) {
            // SetUpButtons (inventory, null, 0, false, null, categoryFilter);

            BuildButtons(null, false, new object[] { 0, inventory, null, categoryFilter });
            
        }
                    
    
        const int consumeAction = 0;
        protected override void OnUIInput (GameObject[] data, object[] customData, Vector2Int input) {
        	if (input.x == consumeAction){
                Inventory.InventorySlot item = customData[0] as Inventory.InventorySlot;
                if (item != null) {
                    int count = 1;
                    item.item.OnConsume((customData[1] as object[])[1] as Inventory, count, input.y);
                }
            }
            CloseUI();
		}
    }
}