// using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// using VRPlayer;


// using Valve.VR;
using System;


using InteractionSystem;
using ActorSystem;

namespace InventorySystem {


/*


    recipes create items

    item getes stashed

    for workshop


        instantiate scen items for item returns on current recipe

            NOW HOVERLOCK to keep hovered over
            highlight on hover true
            keephighlighted


        workshop item script:
            bool isSelected = true; // wheneber instantiated tehy're the selected ones
            current recipe

            onuseDown () {

                if (selected) {
                    do whatever rigidibdy tstuff to place the object

                    
                    if (current recipe != null) {
                        current recipe create (
                            inventory, at position
                        ) ;

                        that gives items (
                            workshop stashed items should be consume on equip
                            then on consume gives permanent xp buff
                            also drop on consume
                         )


                    }


                        
                }
                else {

                }



            }

        

        prefab to instantiate

        OnStash (inventory, item, count) {
            //instantiate scene item

            //drop item

        }







 */


public class Inventory : MonoBehaviour
{

    public int stashAction = 1;

    ActorSystem.Actor _actor;
    [HideInInspector] public ActorSystem.Actor actor {
        get {
            if (_actor == null) _actor = GetComponent<ActorSystem.Actor>();
            return _actor;
        }
    }
    [HideInInspector] public InventoryEqupping equipping;
    // public InventoryCrafter crafter;
    void InitializeComponents () {
        // actor = GetComponent<ActorSystem.Actor>();
        equipping = GetComponent<InventoryEqupping>();
        // crafter = GetComponent<InventoryCrafter> ();
    }

    void Awake () {
        InitializeComponents();
    }

    public event System.Action<Item, int, int> onSceneItemActionStart;

        public void OnSceneItemActionStart (Item sceneItem, int interactorID, int actionIndex) {
            

            if (actionIndex == stashAction) {
                StashItem(sceneItem, interactorID, true);
            }
            if (onSceneItemActionStart != null) {
                onSceneItemActionStart(sceneItem, interactorID, actionIndex);
            }
        }

        public string GetDisplayName () {
            if (actor != null) {
                return actor.actorName;
            }
            return name;
        }


    public List<int> scrappableCategories = new List<int> () { 0, 1, 3 };
    
    public List<int> autoScrapCategories = new List<int> () { 3 };
        
    public const string equippedItemLayer = "EquippedItem";

    public event System.Action<Inventory, int, Inventory, string, List<int>> onInventoryManagementInitiate;
    public event System.Action<Inventory, int, string> onEndInventoryManagement;

    public void InitiateInventoryManagement (string context, int equipID, Inventory secondaryInventory, List<int> categoryFilter) {
        if (onInventoryManagementInitiate != null) {
            // Debug.LogError("opwninf " + context);
            onInventoryManagementInitiate(this, equipID, secondaryInventory, context, categoryFilter);
        }
    }
    public void EndInventoryManagement (string context, int equipID) {
        if (onEndInventoryManagement != null) {
            onEndInventoryManagement(this, equipID, context);
        }
    }
    


    
    public const string quickTradeContext = "QuickTrade";
    public const string fullTradeContext  = "FullTrade";
    public const string quickInventoryContext = "QuickInventory";
    public const string fullInventoryContext = "FullInventory";

    
    
    // weapons, armor, meds
    public List<int> favoriteAbleCategories = new List<int>() {
        0, 1, 2
    };




    // empty filters will return all inventory with categories >= 0
    public List<InventorySlot> GetFilteredInventory( List<int> categoryFilter ) {
        
        List<InventorySlot> r = new List<InventorySlot>();
        for (int i = 0; i < allInventory.Count; i++) {
            if (categoryFilter == null || categoryFilter.Count == 0) {
                if (allInventory[i].item.category >= 0) {
                    r.Add(allInventory[i]);
                }
            }
            else {
                if (categoryFilter.Contains(allInventory[i].item.category)) {
                    r.Add(allInventory[i]);
                }
            }
        }
        return r;
    }
    

    public void TransferInventoryContentsTo (Inventory otherInventory, bool sendMessage) {
        for (int i = allInventory.Count -1 ; i >= 0; i--) {
            ItemBehavior item = allInventory[i].item;
            int count = allInventory[i].count;

            DropItemAlreadyInInventory(allInventory[i], count, false, i, true, sendMessage);
            otherInventory.StashItem(item, count, -1, sendMessage);
        }
    }
    
    public void TransferItemAlreadyInInventoryTo (Inventory.InventorySlot slot, int count, Inventory otherInventory, bool sendMessage) {
        DropItemAlreadyInInventory(slot, count, false, -1, true, sendMessage);
        otherInventory.StashItem(slot.item, count, -1, sendMessage);
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
        
        bool RemoveItemComposition (Item_Composition itemComps, bool checkScrap, bool sendMessage, Dictionary<string, GameValue> selfGameValues, Dictionary<string, GameValue> suppliedGameValues) {
            
            if (!GameValueCondition.ConditionsMet(itemComps.conditions, selfGameValues, suppliedGameValues)) {
                return false;
            }

            //just to be sure
            int itemCompositionAmount = Mathf.Max(1, itemComps.amount);
            
            ItemBehavior itemToRemove = itemComps.item;
            int needsAmount = itemCompositionAmount;

            int baseSlot = GetSlotIndex(itemToRemove);

            if (baseSlot != -1) {
                needsAmount -= allInventory[baseSlot].count;
            }

            bool hasEnoughBaseComponents = needsAmount <= 0;

            if (hasEnoughBaseComponents) {
                DropItemAlreadyInInventory(itemToRemove, itemCompositionAmount, false, baseSlot, true, sendMessage);
                return true;
            }
            else {
                
                if (checkScrap) {
                    
                    // if not enough, loop through and junk items for their base components (first level only)

                    // dont clear empties while looping, in order to keep indicies consistent
                    for (int i = 0; i < allInventory.Count; i++) {

                        if (allInventory[i].count <= 0) continue;

                        ItemBehavior potentiallyScrapped = allInventory[i].item;
                        int potentiallyScrappedCountInInventory = allInventory[i].count;

                        if (autoScrapCategories.Contains(potentiallyScrapped.category)) {

                        
                            int scrappedItemSubIngredientCount;
                            if (ItemConsistsOf(potentiallyScrapped, itemToRemove, out scrappedItemSubIngredientCount, selfGameValues, suppliedGameValues)) {

                                int itemToRemoveCountWithinIngredients = potentiallyScrappedCountInInventory * scrappedItemSubIngredientCount;

                                // if we're gonna scrap every item anyways and still need some
                                if (needsAmount > itemToRemoveCountWithinIngredients) {
                                    //need to check for inventory slot, since slot indicies could change during scrapping
                                    ScrapItemAlreadyInInventory(potentiallyScrapped, potentiallyScrappedCountInInventory, i, false, sendMessage, selfGameValues, suppliedGameValues);
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
                                    ScrapItemAlreadyInInventory(potentiallyScrapped, countToScrap, i, false, sendMessage, selfGameValues, suppliedGameValues);
                                    
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
                        if (baseSlot == -1) baseSlot = GetSlotIndex(itemToRemove);
                        DropItemAlreadyInInventory(itemToRemove, itemCompositionAmount, false, baseSlot, true, sendMessage);
                        return true;
                    }
                    else {
                        Debug.LogError("problem with math here....");
                        return false;
                    }
                }
            }
            return false;
        }

        public Item_Composition[] RemoveItemComposition (Item_Composition[] itemComps, bool checkScrap, bool sendMessage, Dictionary<string, GameValue> selfGameValues, Dictionary<string, GameValue> suppliedGameValues) {

            List<Item_Composition> removed = new List<Item_Composition>();
            for (int i = 0; i < itemComps.Length; i++) {
                if (RemoveItemComposition(itemComps[i], checkScrap, sendMessage, selfGameValues, suppliedGameValues)){
                    removed.Add(itemComps[i]);
                }
            }
            RemoveEmpties();
            return removed.ToArray();
        }

                    
        public static List<Item_Composition> FilterItemComposition (Item_Composition[] compositions, Dictionary<string, GameValue> selfGameValues, Dictionary<string, GameValue> suppliedGameValues) {
            List<Item_Composition> filtered = new List<Item_Composition>();
            for (int i = 0; i < compositions.Length; i++) {
                if (GameValueCondition.ConditionsMet(compositions[i].conditions, selfGameValues, suppliedGameValues)) {
                    filtered.Add(compositions[i]);
                }
            }
            return filtered;
        }

        public void AddItemComposition(Item_Composition[] itemComps, bool sendMessage, Dictionary<string, GameValue> selfGameValues, Dictionary<string, GameValue> suppliedGameValues) {
            bool checkConditions = selfGameValues != null && suppliedGameValues != null;
            for (int i = 0; i < itemComps.Length; i++) {
                if (!checkConditions || GameValueCondition.ConditionsMet(itemComps[i].conditions, selfGameValues, suppliedGameValues)) {
                    StashItem(itemComps[i].item, itemComps[i].amount, -1, sendMessage);
                }
            }
        }

        void ScrapItemWithCheck (ItemBehavior item, int count, bool removeEmpty, bool sendMessage, Dictionary<string, GameValue> selfGameValues, Dictionary<string, GameValue> suppliedGameValues) {
            int slotIndex = GetSlotIndex(item);
            if (slotIndex != -1) ScrapItemAlreadyInInventory ( item, count, slotIndex, removeEmpty, sendMessage, selfGameValues, suppliedGameValues );
        }

        public void ScrapItemAlreadyInInventory (ItemBehavior item, int count, int slotIndex, bool removeEmpty, bool sendMessage, Dictionary<string, GameValue> selfGameValues, Dictionary<string, GameValue> suppliedGameValues) {
            DropItemAlreadyInInventory(item, count, false, slotIndex, removeEmpty, sendMessage);
            for (int i = 0; i< count; i++) {
                AddItemComposition(item.composedOf, sendMessage, selfGameValues, suppliedGameValues);
            }
        }

        public int GetItemCount (ItemBehavior itemToCheckFor, bool checkScrap, Dictionary<string, GameValue> selfGameValues, Dictionary<string, GameValue> suppliedGameValues) {
            int c = 0;
            for (int i =0 ; i < allInventory.Count; i++) {
                if (allInventory[i].item == itemToCheckFor) {
                    c += allInventory[i].count;
                    
                    if (!checkScrap) 
                        return c;

                    continue;
                }

                if (checkScrap) {
                    if (allInventory[i].count > 0) {
                        if (autoScrapCategories.Contains(allInventory[i].item.category)) {
                            int consistsOfCount;
                            if (ItemConsistsOf(allInventory[i].item, itemToCheckFor, out consistsOfCount, selfGameValues, suppliedGameValues))
                            {
                                c += consistsOfCount * allInventory[i].count;
                            }
                        }
                    }
                }
            }
            return c;
        }

        public static bool ItemConsistsOf(ItemBehavior item, ItemBehavior componentCheck, out int consistsOfCount, Dictionary<string, GameValue> selfGameValues, Dictionary<string, GameValue> suppliedGameValues) {
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
        
        
        public event System.Action<Inventory, ItemBehavior, int, int> onStash, onDrop;
    
        [System.Serializable] public class InventorySlot {
            public InventorySlot(ItemBehavior item, int count) {
                this.item = item;
                this.count = count;
            }
            public ItemBehavior item;
            public int count;
        }

        public int favoritesMaxCount = 8;
        [HideInInspector] public List<InventorySlot> allInventory = new List<InventorySlot>();
        [HideInInspector] public List<int> favorites = new List<int>();

        public void FavoriteItem (ItemBehavior item) {
            int slotIndex = GetSlotIndex(item);
            FavoriteItem(slotIndex);
            
        }
        public void FavoriteItem (int slotIndex) {
            if (favoriteAbleCategories.Contains(allInventory[slotIndex].item.category)) {
                if (!favorites.Contains(slotIndex)) {
                    favorites.Add(slotIndex);
                }
                else {
                    UnfavoriteItem (slotIndex);
                }
            }
        }

        void UnfavoriteItem (int slotIndex) {
            favorites.Remove(slotIndex);
        }


        public Vector3 dropLocalPoint = new Vector3 (0,1,1);

        public void StashItem(ItemBehavior itemBehavior, int count, int equipSlot, bool sendMessage) {
            
            InventorySlot slotToUse = null;

            //check if it's already in inventory
            int slotIndex = GetSlotIndex(itemBehavior);
            bool wasInInventory = slotIndex != -1;
            if (wasInInventory) {
                slotToUse = allInventory[slotIndex];
                slotToUse.count += count;
            }
            else {                
                slotToUse = new InventorySlot(itemBehavior, count);
                allInventory.Add(slotToUse);
                slotIndex = allInventory.Count - 1;
            }

            for (int i = 0; i < itemBehavior.stashedItemBehaviors.Length; i++) {
                itemBehavior.stashedItemBehaviors[i].OnItemStashed(this, itemBehavior, count, equipSlot, equipSlot != -1);
            }

            if (onStash != null) {
                onStash(this, itemBehavior, count, slotToUse.count);
            }

            if (sendMessage && actor != null && actor.isPlayer) {
                actor.ShowMessage("Stashed " + itemBehavior.itemName + " [ x" + count + " ]", UIColorScheme.Normal);
            }


            if (!wasInInventory) {
                if (favorites.Count < favoritesMaxCount) {
                    FavoriteItem(slotIndex);
                }
            }
        }

        public void StashItem(Item item, int equipSlot, bool sendMessage)
        {
            ItemBehavior itemBehavior = item.itemBehavior;
            StashItem(itemBehavior, itemBehavior.stackable ? item.itemCount : 1, equipSlot, sendMessage);
            //disable the scene item (frees it up for pool)
            item.gameObject.SetActive(false);
            
        }


        public void ClearInventory (bool sendMessage) {
            for (int i = allInventory.Count -1; i >= 0; i--) {
                DropItemAlreadyInInventory(allInventory[i], allInventory[i].count, false, i, true, sendMessage);
            }
        }
        public void AddInventory (List<InventorySlot> slots, bool sendMessage) {
            for (int i =0 ; i < slots.Count; i++) {
                StashItem(slots[i].item, slots[i].count, -1, sendMessage);
            }
        }

        public void DropItemWithInventoryCheck (InventorySlot slot, int countDropped, bool getScene, bool removeEmpty, bool sendMessage) {
            DropItemWithInventoryCheck(slot.item, countDropped, getScene, removeEmpty, sendMessage);
        }
        public void DropItemWithInventoryCheck (ItemBehavior itemBehavior, int countDropped, bool getScene, bool removeEmpty, bool sendMessage) {
            int slotIndex = GetSlotIndex(itemBehavior);
            if (slotIndex == -1) return;
            DropItemAlreadyInInventory(allInventory[slotIndex], countDropped, getScene, slotIndex, removeEmpty, sendMessage);
        }
            
        public void DropItemAlreadyInInventory (ItemBehavior itemBehavior, int countDropped, bool getScene, int slotIndex, bool removeEmpty, bool sendMessage) {    
            if (slotIndex == -1) slotIndex = GetSlotIndex(itemBehavior);
            DropItemAlreadyInInventory(allInventory[slotIndex], countDropped, getScene, slotIndex, removeEmpty, sendMessage);
        }


        int GetSlotIndex (InventorySlot slot) {
            for (int i = 0; i < allInventory.Count; i++) {
                if (allInventory[i] == slot) return i;
            }
            return -1;
        }
        int GetSlotIndex (ItemBehavior item) {
            for (int i = 0; i < allInventory.Count; i++) {
                if (allInventory[i].item == item) return i;
            }
            return -1;
        }

        public void RemoveEmpties () {
            for (int i = allInventory.Count-1; i >= 0; i--) {
                if (allInventory[i].count == 0) allInventory.Remove(allInventory[i]);
            }
        }

        public void DropItemAlreadyInInventory (InventorySlot slot, int countDropped, bool getScene, int slotIndex, bool removeEmpty, bool sendMessage) {
            
            countDropped = Mathf.Min(countDropped, slot.count);
            slot.count -= countDropped;
            
            int newCount = slot.count;
            if (newCount == 0) {
                if (slotIndex < 0) slotIndex = GetSlotIndex(slot);
                UnfavoriteItem(slotIndex);

                if (removeEmpty) {
                    allInventory.Remove(slot);
                }
            }
            
            bool hasModel = false;
            // if (newCount == 0) {
            //     if (ItemIsEquipped(-1, itemBehavior)) {
            //         UnequipItem(itemBehavior, getScene);
            //         hasModel= true;
            //     }
            // }

            // remove buffs
            for (int i = 0; i < slot.item.stashedItemBehaviors.Length; i++) {
                slot.item.stashedItemBehaviors[i].OnItemDropped(this, slot.item, countDropped);
            }

            if (onDrop != null) {
                onDrop(this, slot.item, countDropped, newCount);
            }

            if (sendMessage && actor != null && actor.isPlayer) {
                actor.ShowMessage("Dropped " + slot.item.itemName + " [ x" + countDropped + " ]", UIColorScheme.Normal);
            }

            //TODO: figure out a way to check if we unequipped an item to drop it (hasModel = true)
            if (!hasModel) { 
                if (getScene) {
                    //TODO: add count to GetSceneItem when dropping multiple...         
                    Item sceneItem = Item.GetSceneItem(slot.item);
                    sceneItem.transform.position = transform.TransformPoint(dropLocalPoint);
                    sceneItem.transform.rotation = Quaternion.Euler(UnityEngine.Random.Range(0,360), UnityEngine.Random.Range(0,360), UnityEngine.Random.Range(0,360));
                    sceneItem.gameObject.SetActive(true);
                }
            }
        }

     
    }
}
