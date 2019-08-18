using System.Collections.Generic;
using UnityEngine;

using Game.InventorySystem;
using Game.InventorySystem.CraftingSystem;
using SimpleUI;
namespace Game.GameUI {
    public class CraftingUIHandler : InventoryManagementUIHandler
    {
        CraftingRecipeBehavior FindCraftingRecipeOnItem (ItemBehavior item) {
            for (int i = 0; i < item.stashedItemBehaviors.Length; i++) {
                CraftingRecipeBehavior recipe = item.stashedItemBehaviors[i] as CraftingRecipeBehavior;
                if (recipe != null) return recipe;
            }
            return null;
        }
        
        protected override void OnUISelect (GameObject[] data, object[] customData) {
            // scrapAttempts = 0;
            (uiObject as SimpleUI.ElementHolderCollection).textPanel.SetText("");

            //cehck for empty....
            if (customData == null) {
                return;
            }

            InventorySlot selectedSlot = customData[0] as InventorySlot;
            int uiIndex = (int)customData[1];
            Inventory shownInventory = (customData[2] as object[])[0] as Inventory;

            if (selectedSlot == null) {
                return;
            }

            //selected on the recipes page...
            if (uiIndex == 0) {
                CraftingRecipeBehavior recipe = FindCraftingRecipeOnItem(selectedSlot.item);
                if (recipe == null) {
                    return;
                }
            
                string text = selectedSlot.item.itemDescription + "\n\nRequires:\n";
                
                List<ItemComposition> required = Inventory.FilterItemComposition (recipe.requires, shownInventory.actor, shownInventory.actor);
                for (int i = 0; i < required.Count; i++) {
                    int hasCount = shownInventory.GetItemCountAfterAutoScrap(required[i].item, shownInventory.actor, shownInventory.actor);
                    text += required[i].item.itemName + ":\t" + hasCount + " / " + required[i].amount + "\n";
                }
                (uiObject as SimpleUI.ElementHolderCollection).textPanel.SetText(text);            
            }
            //selected on scrappable categories page
            else {

                string text = selectedSlot.item.itemDescription + "\n\nScrap For:\n";
                
                List<ItemComposition> composedOf = Inventory.FilterItemComposition (selectedSlot.item.composedOf, shownInventory.actor, shownInventory.actor);
                for (int i = 0; i < composedOf.Count; i++) {
                    text += composedOf[i].item.itemName + ":\t" + composedOf[i].amount + "\n";
                }
                (uiObject as SimpleUI.ElementHolderCollection).textPanel.SetText(text);            

            }
        }

        protected override List<InventorySlot> BuildInventorySlotsForDisplay (int uiIndex, Inventory shownInventory, List<int> categoryFilter) {
            if (uiIndex == 0) {
                return shownInventory.GetFilteredInventory(categoryFilter);
            }
            else {
                List<InventorySlot> r = shownInventory.GetFilteredInventory(categoryFilter);
                for (int i = r.Count - 1; i >= 0; i--) {

                    // check if item is in fact scrappable (not base components)
                    if (r[i].item.composedOf.list.Length == 0) {
                        r.Remove(r[i]);
                    }
                } 
                return r;
            }
        }
        
        List<int> categoryFilter;

        protected override void OnOpenInventoryUI(Inventory inventory, int usingEquipPoint, Inventory otherInventory, List<int> categoryFilter) {
            this.categoryFilter = categoryFilter;
            BuildButtons("Recipes", true, 0, new object[] { inventory, inventory, categoryFilter });
            BuildButtons("Scrappable Items", false, 1, new object[] { inventory, inventory, inventory.scrappableCategories });

        }
        
        const int craftAction = 0;

        // int scrapAttempts;
















        void OnConfirmationSelection(bool used, int selectedOption) {
            if (used && selectedOption == 0) {
                Inventory shownInventory = currentUpdateParams[0] as Inventory;

                //scrap
                if (currentPanelIndex == 1) {
                    shownInventory.ScrapItem(currentSlot.item, 1, -1, true, sendMessage: true, shownInventory.actor, shownInventory.actor);

                    UpdateUIButtons( 1, currentUpdateParams);//new object[] { currentUpdateParams[0], currentUpdateParams[1], shownInventory.scrappableCategories } );
                    UpdateUIButtons( 0, new object[] { currentUpdateParams[0], currentUpdateParams[1], categoryFilter } );
                }
                // craft recipe
                else {
                    if (currentSlot.item.OnConsume(shownInventory, 1, 0)){
                        //update with current button params
                        UpdateUIButtons(0, currentUpdateParams); 
                        //update other ui panel, other index, same inventories, scrappable categories
                        UpdateUIButtons(1, new object[] { currentUpdateParams[0], currentUpdateParams[1], shownInventory.scrappableCategories } );
                    }

                }

                
            }
        }


        int currentPanelIndex;
        // Inventory shownInventory;
        InventorySlot currentSlot;
        object[] currentUpdateParams;

        void ShowConfirmationScreen(string msg, int currentPanelIndex, 
            // Inventory shownInventory, 
            InventorySlot currentSlot, object[] currentUpdateParams) {
            this.currentPanelIndex = currentPanelIndex;
            this.currentUpdateParams = currentUpdateParams;
            // this.shownInventory = shownInventory;
            this.currentSlot = currentSlot;

            UIManager.ShowSelectionPopup(msg, new string[] {"Yes", "No"}, OnConfirmationSelection);
        }

        protected override void OnUIInput (GameObject[] data, object[] customData, Vector2Int input, int actionOffset) {
        
    		if (customData != null) {

                InventorySlot slot = customData[0] as InventorySlot;
                int uiIndex = (int)customData[1];

                object[] updateParameters = customData[2] as object[];
                Inventory shownInventory = updateParameters[0] as Inventory;

                if (input.x == craftAction+actionOffset) {
                    // showign crafting recipes
                    if (uiIndex == 0) {
                        CraftingRecipeBehavior recipe = FindCraftingRecipeOnItem(slot.item);
                        if (recipe != null) {
                            if (shownInventory.ItemCompositionAvailableInInventoryAfterAutoScrap (recipe.requires, shownInventory.actor, shownInventory.actor)) {
                                

                                string msgText = "\nCraft " + slot.item.itemName + "?\n";

                                List<ItemComposition> required = Inventory.FilterItemComposition (recipe.requires, shownInventory.actor, shownInventory.actor);
                                for (int i = 0; i < required.Count; i++) {
                                    msgText += required[i].item.itemName + ":\t" + required[i].amount + "\n";
                                }
                
                                ShowConfirmationScreen(msgText, 0, slot, updateParameters);

                                // if (slot.item.OnConsume(shownInventory, 1, input.y)){

                                //     object[] updateParameters = customData[2] as object[];
                                //     //update with current button params
                                //     UpdateUIButtons(0, updateParameters); 
                                //     //update other ui panel, other index, same inventories, scrappable categories
                                //     UpdateUIButtons(1, new object[] { updateParameters[0], updateParameters[1], shownInventory.scrappableCategories } );
                                // }
                            }
                        }
                    }
                    else {
                        
                        string msgText = "\nScrap " + slot.item.itemName + "?\n";

                        List<ItemComposition> composedOf = Inventory.FilterItemComposition (slot.item.composedOf, shownInventory.actor, shownInventory.actor);
                        for (int i = 0; i < composedOf.Count; i++) {
                            msgText += composedOf[i].item.itemName + ":\t" + composedOf[i].amount + "\n";
                        }

                        ShowConfirmationScreen(msgText, 1, slot, updateParameters);

                    
                        // if (scrapAttempts == 1) {
                        //     scrapAttempts = 0;

                        //     shownInventory.ScrapItem(slot.item, 1, -1, true, sendMessage: true, shownInventory.actor, shownInventory.actor);
                        //     object[] updateParameters = customData[2] as object[];

                        //     UpdateUIButtons( 1, updateParameters);//new object[] { updateParameters[0], updateParameters[1], shownInventory.scrappableCategories } );
                        //     UpdateUIButtons( 0, new object[] { updateParameters[0], updateParameters[1], categoryFilter } );

                        // }
                        // else {
                        //     uiObject.textPanel.SetText("\n\nAre you sure you want to scrap " + slot.item.itemName + " ?\n\nPress 'Scrap' again to confirm.\nSelect another item to cancel...");
                        //     scrapAttempts = 1;
                        // }
                    }
                }                
            }
		}
    }
}