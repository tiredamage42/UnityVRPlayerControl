using System.Collections.Generic;
using UnityEngine;

using InventorySystem;

namespace GameUI {
    public class QuickInventoryUIHandler : InventoryManagementUIHandler
    {
        public override bool EquipIDSpecific() { return true; }
        protected override bool UsesRadial() { return true; }
        public override string ContextKey() { return "QuickInventory"; }
        protected override void OnUISelect (GameObject[] data, object[] customData) { }


        protected override List<Inventory.InventorySlot> BuildInventorySlotsForDisplay (Inventory forInventory, List<int> categoryFilter) {
            return forInventory.favorites;
        }

        protected override void OnInventoryManagementInitiate(Inventory inventory, int usingEquipPoint, Inventory otherInventory) {
            SetUpButtons (inventory, null, 0, 0, false, null);
        }
                    
        public const int consumeAction = 0;
        public override string[] GetInputNames () { return new string[] { "Use" }; }
        

        protected override void OnUIInput (GameObject[] data, object[] customData, Vector2Int input) {
        	if (input.x == consumeAction){
            // if (customData != null) {
                Inventory.InventorySlot item = customData[0] as Inventory.InventorySlot;
                if (item != null) {
                    int count = 1;
                    item.item.OnConsume((Inventory)customData[1], count, input.y);
                }
            }
            CloseUI();
		}
    }
}