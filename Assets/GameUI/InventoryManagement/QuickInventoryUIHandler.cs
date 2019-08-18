using System.Collections.Generic;
using UnityEngine;
using Game.InventorySystem;

namespace Game.GameUI {
    public class QuickInventoryUIHandler : InventoryManagementUIHandler
    {
        protected override List<InventorySlot> BuildInventorySlotsForDisplay ( int uiIndex, Inventory shownInventory, List<int> categoryFilter) {
            List<InventorySlot> r = new List<InventorySlot>();
            for (int i = 0; i < shownInventory.favorites.Count; i++) {
                r.Add(shownInventory.allInventory[shownInventory.favorites[i]]);
            }
            return r;
        }

        protected override void OnOpenInventoryUI(Inventory inventory, int usingEquipPoint, Inventory otherInventory, List<int> categoryFilter) {
            BuildButtons("", false, 0, new object[] { inventory, null, categoryFilter });
        }

        protected override void OnUISelect (GameObject[] data, object[] customData) {

        }
                    
        const int consumeAction = 0;
        protected override void OnUIInput (GameObject[] data, object[] customData, Vector2Int input, int actionOffset) {
        	if (input.x == consumeAction+actionOffset){
                InventorySlot item = customData[0] as InventorySlot;
                if (item != null) {
                    int count = 1;
                    item.item.OnConsume((customData[2] as object[])[0] as Inventory, count, input.y);
                }
            }
            CloseUI();
		}
    }
}