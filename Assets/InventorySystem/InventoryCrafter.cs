// using System.Collections;

using System.Collections.Generic;
using UnityEngine;
using ActorSystem;
namespace InventorySystem {
    [RequireComponent(typeof(Inventory))]
    // [RequireComponent(typeof(Actor))]
    public class InventoryCrafter : MonoBehaviour
    {
        public List<int> autoScrapCategories = new List<int> ();
        // Actor actor;
        Inventory inventory;

        // public Dictionary<string, GameValue> gameValues;

        void Awake () {
            // actor = GetComponent<Actor>();
            inventory = GetComponent<Inventory>();
        }

        void Start () {
            // gameValues = actor.GetValueDictionary();
        }

    
        public int GetItemCount (ItemBehavior itemToCheckFor, bool checkScrap, Dictionary<string, GameValue> selfGameValues, Dictionary<string, GameValue> suppliedGameValues) {
            int c = 0;
            for (int i =0 ; i < inventory.allInventory.Count; i++) {
                if (inventory.allInventory[i].item == itemToCheckFor) {
                    c += inventory.allInventory[i].count;
                    continue;
                }

                if (checkScrap) {
                    if (autoScrapCategories.Contains(inventory.allInventory[i].item.category)) {
                        int consistsOfCount;
                        if (ItemConsistsOf(inventory.allInventory[i].item, itemToCheckFor, out consistsOfCount, selfGameValues, suppliedGameValues))
                        {
                            c += consistsOfCount * inventory.allInventory[i].count;
                        }
                    }
                }
            }
            return c;
        }

        bool ItemConsistsOf(ItemBehavior item, ItemBehavior componentCheck, out int consistsOfCount, Dictionary<string, GameValue> selfGameValues, Dictionary<string, GameValue> suppliedGameValues) {
            consistsOfCount = -1;
            Item_Composition[] composedOf = item.composedOf;
            for (int i = 0; i < composedOf.Length; i++) {
                if (composedOf[i].item == componentCheck) {
                    if (GameValueCondition.ConditionsMet(composedOf[i].conditions, selfGameValues, suppliedGameValues)) {
                        consistsOfCount = composedOf[i].amount;
                        return true;
                    }
                }
            }
            return false;
        }

        public bool RemoveItemComposition (Item_Composition itemComps, bool checkScrap, Dictionary<string, GameValue> selfGameValues, Dictionary<string, GameValue> suppliedGameValues) {
            if (!GameValueCondition.ConditionsMet(itemComps.conditions, selfGameValues, suppliedGameValues)) {
                return false;
            }
            
            ItemBehavior itemToRemove = itemComps.item;
            int needsAmount = itemComps.amount;

            // bool hasEnoughBaseComponents = false;
        
            for (int i = 0; i < inventory.allInventory.Count; i++) {
                if (inventory.allInventory[i].item == itemToRemove) {
                    // int countToDrop = Mathf.Min(needsAmount, inventory.allInventory[i].count);
                    // needsAmount -= countToDrop;

                    needsAmount -= inventory.allInventory[i].count;
                    break;
                }
            }
            bool hasEnoughBaseComponents = needsAmount <= 0;

            if (hasEnoughBaseComponents) {
                inventory.DropItem(itemToRemove, itemComps.amount, false, -1);

                return true;

            }
            else {
                
                if (checkScrap) {

                    //if not enough, loop through and junk items for their base components (first level only)
                    // bool hasEnoughComponentsNow = false;
                    for (int i = 0; i < inventory.allInventory.Count; i++) {

                        ItemBehavior potentiallyScrapped = inventory.allInventory[i].item;
                        int potentiallyScrappedCountInInventory = inventory.allInventory[i].count;

                        if (autoScrapCategories.Contains(potentiallyScrapped.category)) {

                        
                            int scrappedItemSubIngredientCount;
                            if (ItemConsistsOf(potentiallyScrapped, itemToRemove, out scrappedItemSubIngredientCount, selfGameValues, suppliedGameValues)) {

                                int itemToRemoveCountWithinIngredients = potentiallyScrappedCountInInventory * scrappedItemSubIngredientCount;

                                // if we're gonna scrap every item anyways and still need some
                                if (needsAmount > itemToRemoveCountWithinIngredients) {
                                    ScrapItem(potentiallyScrapped, potentiallyScrappedCountInInventory, selfGameValues, suppliedGameValues);
                                    needsAmount -= itemToRemoveCountWithinIngredients;
                                }
                                else {

                                    // need 6, consists of 4 = scrap 2
                                    // need 4, consists of 6 = scrap 1
                                    // need 6, consists of 6 = scrap 1

                                    int countToScrap = (needsAmount / scrappedItemSubIngredientCount) + Mathf.Min(needsAmount%scrappedItemSubIngredientCount, 1);
                                    ScrapItem(inventory.allInventory[i].item, countToScrap, selfGameValues, suppliedGameValues);
                                    needsAmount = 0;

                                    hasEnoughBaseComponents = true;

                                    break;

                                }
                            }
                        }
                    }

                    //we have enough base components after scrpping to remove the item composition...
                    if (hasEnoughBaseComponents) {
                        inventory.DropItem(itemToRemove, itemComps.amount, false, -1);
                    }
                    else {
                        Debug.LogError("problem with math here....");
                    }
                }


                return false;
                
                
            }
        }

        void ScrapItem (ItemBehavior item, int count, Dictionary<string, GameValue> selfGameValues, Dictionary<string, GameValue> suppliedGameValues) {
            inventory.DropItem(item, count, false, -1);
            for (int i = 0; i< count; i++) {
                AddItemComposition(item.composedOf, true, selfGameValues, suppliedGameValues);
            }
        }
        public bool ItemCompositionAvailableInInventory (Item_Composition[] compositions, bool checkScrap, Dictionary<string, GameValue> selfGameValues, Dictionary<string, GameValue> suppliedGameValues) {
            for (int i = 0; i < compositions.Length; i++) {
                Item_Composition composition = compositions[i];
                if (GameValueCondition.ConditionsMet(composition.conditions, selfGameValues, suppliedGameValues)) {
                    if (GetItemCount(composition.item, checkScrap, selfGameValues, suppliedGameValues) < composition.amount) {
                        return false;
                    }
                }
            }
            return true;
        }
        
        public Item_Composition[] RemoveItemComposition (Item_Composition[] itemComps, bool checkScrap, Dictionary<string, GameValue> selfGameValues, Dictionary<string, GameValue> suppliedGameValues) {

            List<Item_Composition> removed = new List<Item_Composition>();
            for (int i = 0; i < itemComps.Length; i++) {
                if (RemoveItemComposition(itemComps[i], checkScrap, selfGameValues, suppliedGameValues)){
                    removed.Add(itemComps[i]);
                }
            }

            return removed.ToArray();
        }
        public void AddItemComposition(Item_Composition[] itemComps, bool checkConditions, Dictionary<string, GameValue> selfGameValues, Dictionary<string, GameValue> suppliedGameValues) {
            for (int i = 0; i < itemComps.Length; i++) {

                if (!checkConditions || GameValueCondition.ConditionsMet(itemComps[i].conditions, selfGameValues, suppliedGameValues)) {
                    inventory.StashItem(itemComps[i].item, itemComps[i].amount, -1);
                }
            }
        }
    }
}
