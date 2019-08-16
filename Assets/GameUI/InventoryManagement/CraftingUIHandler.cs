using System.Collections.Generic;
using UnityEngine;

using InventorySystem;
using ActorSystem;
using SimpleUI;

namespace GameUI {
    public class CraftingUIHandler : InventoryManagementUIHandler
    {

        protected override bool UsesRadial() { return false; }
        protected override int MaxUIPages () { return 2; }

        // TODO: check if item is in fact scrappable (not base components)


        // public override string[] GetInputNames () { return new string[] { "Craft" }; }

        CraftingRecipeBehavior FindCraftingRecipeOnItem (ItemBehavior item) {
            for (int i = 0; i < item.stashedItemBehaviors.Length; i++) {
                CraftingRecipeBehavior recipe = item.stashedItemBehaviors[i] as CraftingRecipeBehavior;
                if (recipe != null) return recipe;
            }
            return null;
        }
        
        protected override void OnUISelect (GameObject[] data, object[] customData) {
            scrapAttempts = 0;

            Inventory.InventorySlot oldSelectedSlot = selectedSlot;
            selectedSlot = null;

            
            uiObject.textPanel.SetText("");
            
            //cehck for empty....
            if (customData == null) {
                // scrapAttempts = 0;
                return;
            }

            Inventory shownInventory, linkedInventory;
            int uiIndex;//, otherUIIndex;
            UnpackButtonData (customData, out selectedSlot, out shownInventory, out linkedInventory, out uiIndex);//, out otherUIIndex);
            if (selectedSlot == null) {
                // scrapAttempts = 0;
                return;
            }

            // if (selectedSlot != oldSelectedSlot) {
            //     scrapAttempts = 0;
            // }
        

            //selected on the recipes page...
            if (uiIndex == 0) {
                CraftingRecipeBehavior recipe = FindCraftingRecipeOnItem(selectedSlot.item);
                if (recipe == null) {
                    // Debug.LogError(selectedSlot.item.itemName + " doesnt have a recipe behavior,\nbut is in a crafting category...");
                    return;
                }
            
                string text = selectedSlot.item.itemDescription;
                text += "\n\nRequires:\n";
            
                List<Item_Composition> required = Inventory.FilterItemComposition (recipe.requires, shownInventory.actor.actorValues, shownInventory.actor.actorValues);
                for (int i = 0; i < required.Count; i++) {
                    int hasCount = shownInventory.GetItemCount(required[i].item, true, shownInventory.actor.actorValues, shownInventory.actor.actorValues);
                    text += required[i].item.itemName + ":\t" + hasCount + " / " + required[i].amount + "\n";
                }
                uiObject.textPanel.SetText(text);            
            }
            //selected on scrappable categories page
            else {

                string text = selectedSlot.item.itemDescription;
                text += "\n\nScrap For:\n";
            
                List<Item_Composition> composedOf = Inventory.FilterItemComposition (selectedSlot.item.composedOf, shownInventory.actor.actorValues, shownInventory.actor.actorValues);
                for (int i = 0; i < composedOf.Count; i++) {
                    text += composedOf[i].item.itemName + ":\t" + composedOf[i].amount + "\n";
                }
                uiObject.textPanel.SetText(text);            

            }
        }

        // protected override int GetUnpaginatedShowCount(object[] updateButtonsParameters) { return lastSlotsCount[(int)updateButtonsParameters[0]]; } 

        // int[] lastSlotsCount = new int[2];
        protected override List<Inventory.InventorySlot> BuildInventorySlotsForDisplay (int uiIndex, Inventory shownInventory, List<int> categoryFilter) {
            if (uiIndex == 0) {
                List<Inventory.InventorySlot> slots = shownInventory.GetFilteredInventory(categoryFilter);
                // lastSlotsCount[0] = slots.Count;
                return slots;
            }
            else {
                List<Inventory.InventorySlot> r = shownInventory.GetFilteredInventory(categoryFilter);
                for (int i = r.Count - 1; i >= 0; i--) {
                    if (r[i].item.composedOf.list.Length == 0) {
                        r.Remove(r[i]);
                    }
                } 
                // lastSlotsCount[1] = r.Count;
                return r;
            }
        }
        




        // protected override List<Inventory.InventorySlot> BuildInventorySlotsForDisplay (Inventory shownInventory, int uiIndex, List<int> categoryFilter) {
        //     if (uiIndex == 0) {

        //         return shownInventory.GetFilteredInventory(categoryFilter);
        //     }
        //     else {
        //         List<Inventory.InventorySlot> r = shownInventory.GetFilteredInventory(categoryFilter);
        //         for (int i = r.Count - 1; i >= 0; i--) {
        //             if (r[i].item.composedOf.list.Length == 0) {
        //                 r.Remove(r[i]);
        //             }
        //         } 
        //         return r;
        //     }
        // }

        List<int> categoryFilter;

        protected override void OnInventoryManagementInitiate(Inventory inventory, int usingEquipPoint, Inventory otherInventory, List<int> categoryFilter) {
            this.categoryFilter = categoryFilter;
            (uiObject.subHolders[0] as UIPage).SetTitle("Recipes");
            (uiObject.subHolders[1] as UIPage).SetTitle("Scrappable Items");

            // SetUpButtons ( inventory, null, 0, 0, true, null);
            // SetUpButtons ( inventory, inventory, 0, true, uiObject.subHolders[0], categoryFilter);
            // SetUpButtons ( inventory, inventory, 1, false, uiObject.subHolders[1], inventory.scrappableCategories);


            BuildButtons(uiObject.subHolders[0], true, new object[] { 0, inventory, inventory, categoryFilter });
            BuildButtons(uiObject.subHolders[1], false, new object[] { 1, inventory, inventory, inventory.scrappableCategories });

        }
        
        const int craftAction = 0;

        int scrapAttempts;
        Inventory.InventorySlot selectedSlot;

        protected override void OnUIInput (GameObject[] data, object[] customData, Vector2Int input) {
        
    		if (customData != null) {

                Inventory.InventorySlot slot;
                Inventory shownInventory, linkedInventory;
                int uiIndex;//, otherUIIndex;
                UnpackButtonData (customData, out slot, out shownInventory, out linkedInventory, out uiIndex);//, out otherUIIndex);

                bool updateButtons = false;
                if (input.x == craftAction) {

                    if (uiIndex == 0) {
                        if (shownInventory.actor != null) {

                            CraftingRecipeBehavior recipe = FindCraftingRecipeOnItem(slot.item);

                            if (recipe != null) {
                                if (shownInventory.ItemCompositionAvailableInInventory (recipe.requires, true, shownInventory.actor.actorValues, shownInventory.actor.actorValues)) {
                                    if (slot.item.OnConsume(shownInventory, 1, input.y)){
                                        updateButtons = true;

                                         object[] updateButtonsParameters = customData[1] as object[];

                    UpdateUIButtons(updateButtonsParameters); // (Inventory)customData[1], (Inventory)customData[2], (int)customData[3], (int)customData[4], usingCategoryFilter);
                    UpdateUIButtons(
                        new object[] { 1 - (int)updateButtonsParameters[0], updateButtonsParameters[1], updateButtonsParameters[2], inventory.scrappableCategories }
                    );
                                    }
                                }
                            }
                        }
                    }
                    else {

                        if (scrapAttempts == 1) {
                            scrapAttempts = 0;
                            Debug.Log("scrpping");
                            shownInventory.ScrapItemAlreadyInInventory(slot.item, 1, -1, true, sendMessage: true, shownInventory.actor.actorValues, shownInventory.actor.actorValues);
                        updateButtons = true;


                         object[] updateButtonsParameters = customData[1] as object[];

                         UpdateUIButtons(
                        new object[] { (int)updateButtonsParameters[0], updateButtonsParameters[1], updateButtonsParameters[2], inventory.scrappableCategories }
                    );



                    // UpdateUIButtons(updateButtonsParameters); // (Inventory)customData[1], (Inventory)customData[2], (int)customData[3], (int)customData[4], usingCategoryFilter);
                    UpdateUIButtons(
                        new object[] { 1 - (int)updateButtonsParameters[0], updateButtonsParameters[1], updateButtonsParameters[2], categoryFilter }
                    );
                        }
                        else {
                            uiObject.textPanel.SetText("\n\nAre you sure you want to scrap " + slot.item.itemName + " ?\n\nPress 'Scrap' again to confirm.\nSelect another item to cancel...");
                        scrapAttempts = 1;
                        }
                    }

                }

                if (updateButtons){
                    // object[] updateButtonsParameters = customData[1] as object[];

                    // UpdateUIButtons(updateButtonsParameters); // (Inventory)customData[1], (Inventory)customData[2], (int)customData[3], (int)customData[4], usingCategoryFilter);
                    // UpdateUIButtons(
                    //     new object[] { 1 - (int)updateButtonsParameters[0], updateButtonsParameters[1], updateButtonsParameters[2], updateButtonsParameters[3] }
                    // );

                    // UpdateUIButtons((Inventory)customData[1], (Inventory)customData[2], (int)customData[3], (int)customData[4], usingCategoryFilter);
                }                
            }
		}
    }
}