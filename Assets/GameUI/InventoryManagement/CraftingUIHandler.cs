// using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using InventorySystem;
using ActorSystem;

namespace GameUI {
    public class CraftingUIHandler : InventoryManagementUIHandler
    {
        public override string[] GetInputNames () { return new string[] { "Craft" }; }
        public override bool EquipIDSpecific() { return false; }
        protected override bool UsesRadial() { return false; }
        public override string ContextKey() { return "Crafting"; }

        protected override void OnUISelect (GameObject[] data, object[] customData) {
            //cehck for empty....
            if (customData == null)
                return;


            Inventory.InventorySlot slot;
            Inventory forInventory, linkedInventory;
            int uiIndex, otherUIIndex;
            UnpackButtonData (customData, out slot, out forInventory, out linkedInventory, out uiIndex, out otherUIIndex);
            if (slot == null) {
                return;
            }

            CraftingRecipeBehavior recipe = null;

            for (int i = 0; i < slot.item.stashedItemBehaviors.Length; i++) {
                recipe = slot.item.stashedItemBehaviors[i] as CraftingRecipeBehavior;
                if (recipe != null)
                    break;
            }

            if (recipe == null) {
                uiObject.textPanel.SetText(slot.item.itemName + " ISNT A RECIPE!!!");
                return;
            }


            string text = slot.item.itemDescription;

            text += "\n\nRequires:\n";

            Item_Composition[] requires = recipe.requires;

            

            for (int i = 0; i < requires.Length; i++) {

                Item_Composition c = requires[i];
                if (GameValueCondition.ConditionsMet(c.conditions, inventory.actor.actorValues, inventory.actor.actorValues)) {
                    int hasCount = inventory.crafter.GetItemCount(c.item, true, inventory.actor.actorValues, inventory.actor.actorValues);
                    text += c.item.itemName + "\t" + hasCount + " / " + c.amount + "\n";
                }
            }

            uiObject.textPanel.SetText(text);            
        }

        protected override List<Inventory.InventorySlot> BuildInventorySlotsForDisplay (Inventory forInventory, List<int> categoryFilter) {
            return forInventory.GetFilteredInventory(categoryFilter);
        }

        protected override void OnInventoryManagementInitiate(Inventory inventory, int usingEquipPoint, Inventory otherInventory) {
            SetUpButtons ( inventory, null, 0, 0, true, null);
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

                    if (inventory.crafter != null && inventory.actor != null) {

                        CraftingRecipeBehavior recipe = null;
                        for (int i = 0; i < slot.item.stashedItemBehaviors.Length; i++) {
                            recipe = slot.item.stashedItemBehaviors[i] as CraftingRecipeBehavior;
                            if (recipe != null)
                                break;
                        }

                        if (recipe == null) {
                            return;
                        }
                        
                        
                        if (inventory.crafter.ItemCompositionAvailableInInventory (recipe.requires, true, inventory.actor.actorValues, inventory.actor.actorValues)) {

                            if (slot.item.OnConsume(forInventory, 1, input.y)){
                                updateButtons = true;
                            }
                        }
                    }
                }

                if (updateButtons){
                    UpdateUIButtons((Inventory)customData[1], (Inventory)customData[2], (int)customData[3], (int)customData[4], usingCategoryFilter);
                }                
            }
		}
    }
}