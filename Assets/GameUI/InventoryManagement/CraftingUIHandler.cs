
using UnityEngine;
using System.Collections.Generic;
using Game.InventorySystem;
using Game.InventorySystem.CraftingSystem;
using SimpleUI;
namespace Game.UI {
    public class CraftingUIHandler : UISelectableElementHandler
    {
        Inventory showingInventory;
        // dont allow cold open
        protected override object[] GetDefaultColdOpenParams () { return null; }
        
        protected override string GetDisplayForButtonObject(object buttonObject) {
            InventorySlot slot = buttonObject as InventorySlot;
            return slot.item.itemName + " ( x"+slot.count+" )";
        }


        CraftingRecipeBehavior FindCraftingRecipeOnItem (ItemBehavior item) {
            for (int i = 0; i < item.stashedItemBehaviors.Length; i++) {
                CraftingRecipeBehavior recipe = item.stashedItemBehaviors[i] as CraftingRecipeBehavior;
                if (recipe != null) return recipe;
            }
            return null;
        }
        
        protected override void OnUISelect (GameObject selectedObject, GameObject[] data, object[] customData) {
            string txt = "";
            Actor actor = showingInventory.actor;
            
            
            //cehck for empty....
            if (customData != null) {
                InventorySlot selectedSlot = customData[0] as InventorySlot;
                if (selectedSlot != null) {
                
                    int uiIndex = (int)customData[1];
                    //selected on the recipes page...
                    if (uiIndex == 0) {
                        CraftingRecipeBehavior recipe = FindCraftingRecipeOnItem(selectedSlot.item);
                        if (recipe != null) {
                            txt = selectedSlot.item.itemDescription + "\n\nRequires:\n";
                            
                            List<ItemComposition> required = Inventory.FilterItemComposition (recipe.requires, actor, actor);
                            for (int i = 0; i < required.Count; i++) {
                                int hasCount = showingInventory.GetItemCountAfterAutoScrap(required[i].item, actor, actor);
                                txt += required[i].item.itemName + ":\t" + hasCount + " / " + required[i].amount + "\n";
                            }
                        }
                    }
                    //selected on scrappable categories page
                    else {

                        txt = selectedSlot.item.itemDescription + "\n\nScrap For:\n";
                        
                        List<ItemComposition> composedOf = Inventory.FilterItemComposition (selectedSlot.item.composedOf, actor, actor);
                        for (int i = 0; i < composedOf.Count; i++) {
                            txt += composedOf[i].item.itemName + ":\t" + composedOf[i].amount + "\n";
                        }
                    }
                }
            }
            (uiObject as SimpleUI.ElementHolderCollection).textPanel.SetText(txt);
        }

        List<InventorySlot> BuildInventorySlotsForDisplay (int panelIndex) {
            if (panelIndex == 0) {
                return showingInventory.GetFilteredInventory(openedWithParams[1] as List<int>);
            }
            else {
                List<InventorySlot> r = showingInventory.GetFilteredInventory(showingInventory.scrappableCategories);
                for (int i = r.Count - 1; i >= 0; i--) {
                    // check if item is in fact scrappable (not base components)
                    if (r[i].item.composedOf.list.Length == 0) {
                        r.Remove(r[i]);
                    }
                } 
                return r;
            }
        }
        protected override List<object> BuildButtonObjectsListForDisplay(int panelIndex){
            return ToObjectList(BuildInventorySlotsForDisplay(panelIndex));
        }

        static CraftingUIHandler _instance;
        public static CraftingUIHandler instance {
            get {
                if (_instance == null) _instance = GameObject.FindObjectOfType<CraftingUIHandler>();
                return _instance;
            }
        }
        public void OpenCraftingUI (Inventory craftingInventory, List<int> categoryFilter) {
            OpenUI (  0, new object[] { craftingInventory, categoryFilter } );
        }


        protected override void OnOpenUI() {
            showingInventory = openedWithParams[0] as Inventory;
            BuildButtons("Recipes", true, 0);
            BuildButtons("Scrappable", false, 1);
        }
        
        const int craftAction = 0;

        void OnConfirmationSelection(bool used, int selectedOption) {
            if (used && selectedOption == 0) {
                //scrap
                if (currentPanelIndex == 1) {
                    showingInventory.ScrapItem(currentSlot.item, 1, -1, true, sendMessage: true, showingInventory.actor, showingInventory.actor);
                }
                // craft recipe
                else {
                    currentSlot.item.OnConsume(showingInventory, 1, 0);
                }
                UpdateUIButtons( );
            }
        }


        int currentPanelIndex;
        InventorySlot currentSlot;


        string BuildConfirmationText(string craftOrSrap) {
            Actor actor = (openedWithParams[0] as Inventory).actor;

            string msgText = "\n " + craftOrSrap + currentSlot.item.itemName + "?\n";
            List<ItemComposition> composedOf = Inventory.FilterItemComposition (currentSlot.item.composedOf, actor, actor);

            for (int i = 0; i < composedOf.Count; i++) {
                msgText += composedOf[i].item.itemName + ":\t" + composedOf[i].amount + "\n";
            }
            return msgText;
        }
        
        protected override void OnUIInput (GameObject selectedObject, GameObject[] data, object[] customData, Vector2Int input, int actionOffset) {
        
    		if (customData != null) {
                currentSlot = customData[0] as InventorySlot;
                currentPanelIndex = (int)customData[1];

                if (input.x == craftAction+actionOffset) {
                
                    // showign crafting recipes
                    if (currentPanelIndex == 0) {
                        CraftingRecipeBehavior recipe = FindCraftingRecipeOnItem(currentSlot.item);
                        if (recipe != null) {
                            if (showingInventory.ItemCompositionAvailableInInventoryAfterAutoScrap (recipe.requires, showingInventory.actor, showingInventory.actor)) {
                                UIManager.ShowSelectionPopup(true, BuildConfirmationText("Craft "), new string[] {"Yes", "No"}, OnConfirmationSelection);
                            }
                        }
                    }
                    else {
                        UIManager.ShowSelectionPopup(true, BuildConfirmationText("Scrap "), new string[] {"Yes", "No"}, OnConfirmationSelection);
                    }
                }                
            }
		}
    }
}