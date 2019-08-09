// using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using InventorySystem;

namespace GameUI {
    public class CraftingUIHandler : InventoryManagementUIHandler
    {
        public string context;
        // public List<int> itemCategories = new List<int>();

        public override string[] GetInputNames () { return new string[] { "Craft" }; }
        public override bool EquipIDSpecific() { return false; }
        protected override bool UsesRadial() { return false; }
        public override string ContextKey() { return context; }

        protected override void OnUISelect (GameObject[] data, object[] customData) {
            
            Inventory.InventorySlot slot;
            Inventory forInventory, linkedInventory;
            int uiIndex, otherUIIndex;
            UnpackButtonData (customData, out slot, out forInventory, out linkedInventory, out uiIndex, out otherUIIndex);
            
            CraftingRecipeBehavior recipe = null;

            for (int i = 0; i < slot.item.stashedItemBehaviors.Length; i++) {
                recipe = (CraftingRecipeBehavior)slot.item.stashedItemBehaviors[i];
                if (recipe != null)
                    break;
            }

            if (recipe == null) {
                uiObject.textPanel.SetText(slot.item.itemName + " ISNT A RECIPE!!!");
                return;
            }


            string text = slot.item.itemDescription;

            text += "\n\nRequires:\n";

            ItemComposition[] requires = recipe.requires;

            for (int i = 0; i < requires.Length; i++) {
                ItemComposition c = requires[i];
                int hasCount = forInventory.GetItemCount(c.item);
                text += c.item.itemName + "\t" + hasCount + " / " + c.amount + "\n";
            }

            uiObject.textPanel.SetText(text);            
        }

        protected override List<Inventory.InventorySlot> BuildInventorySlotsForDisplay (Inventory forInventory, List<int> categoryFilter) {
            return forInventory.GetFilteredInventory(categoryFilter);
        }

        protected override void OnInventoryManagementInitiate(Inventory inventory, int usingEquipPoint, Inventory otherInventory) {
            // CloseAllUIs();

            SetUpButtons ( inventory, null, 0, 0, true, null);
            
            // SetUpButtons (0, true, null);
            // UpdateUIButtons(inventory, null, 0, 0);
        }
        
        public const int craftAction = 0;
        protected override void OnUIInput (GameObject[] data, object[] customData, Vector2Int input) {
        
    		if (customData != null) {


                Inventory.InventorySlot slot;
                Inventory forInventory, linkedInventory;
                int uiIndex, otherUIIndex;
                UnpackButtonData (customData, out slot, out forInventory, out linkedInventory, out uiIndex, out otherUIIndex);



                bool updateButtons = false;
                if (input.x == craftAction) {

                    CraftingRecipeBehavior recipe = null;

                    for (int i = 0; i < slot.item.stashedItemBehaviors.Length; i++) {
                        recipe = (CraftingRecipeBehavior)slot.item.stashedItemBehaviors[i];
                        if (recipe != null)
                            break;
                    }

                    if (recipe == null) {
                        return;
                    }

                    ItemComposition[] requires = recipe.requires;
                    for (int i = 0; i < requires.Length; i++) {

                        ItemComposition c = requires[i];
                        int hasCount = forInventory.GetItemCount(c.item);

                        if (hasCount < c.amount) {
                            forInventory.GetComponent<GameMessageInbox>().ShowMessage("Not Enough Ingredients");
                            return;
                        }
                        
                    }

                    if (slot.item.OnConsume(forInventory, 1, input.y)){
                        updateButtons = true;
                    }
                }

                if (updateButtons){
                    UpdateUIButtons((Inventory)customData[1], (Inventory)customData[2], (int)customData[3], (int)customData[4], usingCategoryFilter);
                }                
            }
		}
    }
}