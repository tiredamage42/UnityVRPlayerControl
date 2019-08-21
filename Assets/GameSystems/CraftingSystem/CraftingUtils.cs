// using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// using ActorSystem;
// using Game.InventorySystem;


namespace Game.InventorySystem.CraftingSystem {
    public static class CraftingUtils
    {

        public static void ScrapItem (this Inventory inventory, ItemBehavior item, int count, bool sendMessage, Actor selfActor, Actor suppliedActor) {
            for (int i = 0; i< count; i++) {
                inventory.AddItemComposition(item.composedOf, sendMessage, selfActor, suppliedActor);
            }
        }     

        public static void ScrapItem (this Inventory inventory, ItemBehavior item, int count, int slotIndex, bool removeEmpty, bool sendMessage, Actor selfActor, Actor suppliedActor) {
            
            if (slotIndex == -1) slotIndex = inventory.GetSlotIndex(item);
            if (slotIndex != -1) {
                inventory.DropItem(item, count, false, slotIndex, removeEmpty, sendMessage);
                inventory.ScrapItem(item, count, sendMessage, selfActor, suppliedActor);
            }
        }     

        public static bool ItemConsistsOf(ItemBehavior item, ItemBehavior componentCheck, out int consistsOfCount, Actor selfActor, Actor suppliedActor) {
            consistsOfCount = -1;
            ItemComposition[] composedOf = item.composedOf;
            for (int i = 0; i < composedOf.Length; i++) {
                if (composedOf[i].item == componentCheck) {
                    if (ActorValueCondition.ConditionsMet(composedOf[i].conditions, selfActor, suppliedActor)) {
                        consistsOfCount = composedOf[i].amount;
                        return true;
                    }
                }
            }
            return false;
        } 

        public static int GetItemCountAfterAutoScrap (this Inventory inventory, ItemBehavior itemToCheckFor, Actor selfActor, Actor suppliedActor) {
            int c = 0;
            for (int i =0 ; i < inventory.allInventory.Count; i++) {
                if (inventory.allInventory[i].item == itemToCheckFor) {
                    c += inventory.allInventory[i].count;
                    continue;
                }

                if (inventory.allInventory[i].count > 0) {
                    if (inventory.autoScrapCategories.Contains(inventory.allInventory[i].item.category)) {
                        int consistsOfCount;
                        if (ItemConsistsOf(inventory.allInventory[i].item, itemToCheckFor, out consistsOfCount, selfActor, suppliedActor))
                        {
                            c += consistsOfCount * inventory.allInventory[i].count;
                        }
                    }
                }
            }
            return c;
        }


        public static bool RemoveItemCompositionWithAutoScrap (this Inventory inventory, ItemComposition itemComps, bool sendMessage, Actor selfActor, Actor suppliedActor) {
            
            if (!ActorValueCondition.ConditionsMet(itemComps.conditions, selfActor, suppliedActor)) {
                return false;
            }

            //just to be sure
            int itemCompositionAmount = Mathf.Max(1, itemComps.amount);
            
            ItemBehavior itemToRemove = itemComps.item;
            int needsAmount = itemCompositionAmount;

            int baseSlot = inventory.GetSlotIndex(itemToRemove);

            if (baseSlot != -1) {
                needsAmount -= inventory.allInventory[baseSlot].count;
            }

            bool hasEnoughBaseComponents = needsAmount <= 0;

            if (hasEnoughBaseComponents) {
                inventory.DropItem(itemToRemove, itemCompositionAmount, false, baseSlot, true, sendMessage);
                return true;
            }
            else {
                    
                // if not enough, loop through and junk items for their base components (first level only)

                // dont clear empties while looping, in order to keep indicies consistent
                for (int i = 0; i < inventory.allInventory.Count; i++) {

                    if (inventory.allInventory[i].count <= 0) continue;

                    ItemBehavior potentiallyScrapped = inventory.allInventory[i].item;
                    int potentiallyScrappedCountInInventory = inventory.allInventory[i].count;

                    if (inventory.autoScrapCategories.Contains(potentiallyScrapped.category)) {

                    
                        int scrappedItemSubIngredientCount;
                        if (ItemConsistsOf(potentiallyScrapped, itemToRemove, out scrappedItemSubIngredientCount, selfActor, suppliedActor)) {

                            int itemToRemoveCountWithinIngredients = potentiallyScrappedCountInInventory * scrappedItemSubIngredientCount;

                            // if we're gonna scrap every item anyways and still need some
                            if (needsAmount > itemToRemoveCountWithinIngredients) {
                                //need to check for inventory slot, since slot indicies could change during scrapping
                                inventory.ScrapItem(potentiallyScrapped, potentiallyScrappedCountInInventory, i, false, sendMessage, selfActor, suppliedActor);
                                needsAmount -= itemToRemoveCountWithinIngredients;
                            }
                            // we only need to scrap some of the item we have...
                            else {
                                /*
                                    just checking math by hand...
                                    // need 6, consists of 4 = scrap 2
                                    // need 4, consists of 6 = scrap 1
                                    // need 6, consists of 6 = scrap 1
                                */

                                int countToScrap = (needsAmount / scrappedItemSubIngredientCount) + Mathf.Min(needsAmount%scrappedItemSubIngredientCount, 1);
                                inventory.ScrapItem(potentiallyScrapped, countToScrap, i, false, sendMessage, selfActor, suppliedActor);
                                
                                needsAmount = 0;

                                hasEnoughBaseComponents = true;
                                break;
                            }
                        }
                    }
                }

                //we have enough base components after scrpping to remove the item composition...
                if (hasEnoughBaseComponents) {
                    //in case we couldnt find a slot for our base component before
                    if (baseSlot == -1) baseSlot = inventory.GetSlotIndex(itemToRemove);
                    inventory.DropItem(itemToRemove, itemCompositionAmount, false, baseSlot, true, sendMessage);
                    return true;
                }
                else {
                    Debug.LogError("problem with math here....");
                    return false;
                }
            }
        }


        public static ItemComposition[] RemoveItemCompositionWithAutoScrap (this Inventory inventory, ItemComposition[] itemComps, bool sendMessage, Actor selfActor, Actor suppliedActor) {

            List<ItemComposition> removed = new List<ItemComposition>();
            for (int i = 0; i < itemComps.Length; i++) {
                if (inventory.RemoveItemCompositionWithAutoScrap(itemComps[i], sendMessage, selfActor, suppliedActor)) {
                    removed.Add(itemComps[i]);
                }
            }
            inventory.RemoveEmpties();
            return removed.ToArray();
        }

        public static bool ItemCompositionAvailableInInventoryAfterAutoScrap (this Inventory inventory, ItemComposition[] compositions, Actor selfActor, Actor suppliedActor) {
            for (int i = 0; i < compositions.Length; i++) {
                ItemComposition composition = compositions[i];
                if (ActorValueCondition.ConditionsMet(composition.conditions, selfActor, suppliedActor)) {
                    if (inventory.GetItemCountAfterAutoScrap(composition.item, selfActor, suppliedActor) < composition.amount) {
                        return false;
                    }
                }
            }
            return true;
        }
        

    }
}
