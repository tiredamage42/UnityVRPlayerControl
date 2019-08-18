using System.Collections.Generic;
using UnityEngine;
using System;
using InteractionSystem;
namespace Game.InventorySystem {

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



    [System.Serializable] public class InventorySlot {
        public InventorySlot(ItemBehavior item, int count) {
            this.item = item;
            this.count = count;
        }
        public ItemBehavior item;
        public int count;
    }

    public class Inventory : MonoBehaviour
    {
        public string displayName;

        Actor _actor;
        public Actor actor {
            get {
                if (_actor == null) _actor = GetComponent<Actor>();
                return _actor;
            }
        }
        InventoryEquipper _equipper;
        public InventoryEquipper equipper {
            get {
                if (_equipper == null) _equipper = GetComponent<InventoryEquipper>();
                return _equipper;
            }
        }
        
        public event Action<SceneItem, int, int> onSceneItemActionStart;


        public int stashAction = 1;
        
        public void OnSceneItemActionStart (SceneItem sceneItem, int interactorID, int actionIndex) {

            // TODO: move this to player actor script with stash aciton option
            if (actionIndex == stashAction) {
                StashItem(sceneItem, interactorID, true);
            }

            if (onSceneItemActionStart != null) {
                onSceneItemActionStart(sceneItem, interactorID, actionIndex);
            }
        }

        public string GetDisplayName () {
            if (actor != null) 
                return actor.actorName;
            return displayName;
        }
                
        //TODO: move to inventory game settings
        public List<int> scrappableCategories = new List<int> () { 0, 1, 3 };
        public List<int> autoScrapCategories = new List<int> () { 3 };
        // weapons, armor, meds
        public List<int> favoriteAbleCategories = new List<int>() { 0, 1, 2 };
            
        //TODO: move to equipper
        public const string equippedItemLayer = "EquippedItem";
        
        // TODO: move this to ui handling
        public const string quickTradeContext = "QuickTrade";
        public const string fullTradeContext  = "FullTrade";
        public const string quickInventoryContext = "QuickInventory";
        public const string fullInventoryContext = "FullInventory";


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

                DropItem(allInventory[i], count, false, i, true, sendMessage);
                otherInventory.StashItem(item, count, -1, sendMessage);
            }
        }
        
        public void TransferItemAlreadyInInventoryTo (InventorySlot slot, int count, Inventory otherInventory, bool sendMessage) {
            DropItem(slot, count, false, -1, true, sendMessage);
            otherInventory.StashItem(slot.item, count, -1, sendMessage);
        }


        public bool ItemCompositionAvailableInInventory (ItemComposition[] compositions, Actor selfActor, Actor suppliedActor) {
            for (int i = 0; i < compositions.Length; i++) {
                ItemComposition composition = compositions[i];
                if (ActorValueCondition.ConditionsMet(composition.conditions, selfActor, suppliedActor)) {
                    if (GetItemCount(composition.item) < composition.amount) {
                        return false;
                    }
                }
            }
            return true;
        }
        
        bool RemoveItemComposition (ItemComposition itemComps, bool sendMessage, Actor selfActor, Actor suppliedActor) {
            
            if (!ActorValueCondition.ConditionsMet(itemComps.conditions, selfActor, suppliedActor)) {
                return false;
            }

            int baseSlot = GetSlotIndex(itemComps.item);
            if (baseSlot < 0) {
                return false;
            }
            
            //just to be sure
            int itemCompositionAmount = Mathf.Max(1, itemComps.amount);
            
            if (allInventory[baseSlot].count >= itemCompositionAmount) {
                DropItem(itemComps.item, itemCompositionAmount, false, baseSlot, true, sendMessage);
                return true;
            }
            return false;
        }


        public ItemComposition[] RemoveItemComposition (ItemComposition[] itemComps, bool sendMessage, Actor selfActor, Actor suppliedActor) {

            List<ItemComposition> removed = new List<ItemComposition>();
            for (int i = 0; i < itemComps.Length; i++) {
                if (RemoveItemComposition(itemComps[i], sendMessage, selfActor, suppliedActor)) {
                    removed.Add(itemComps[i]);
                }
            }
            RemoveEmpties();
            return removed.ToArray();
        }
        

                    
        public static List<ItemComposition> FilterItemComposition (ItemComposition[] compositions, Actor selfActor, Actor suppliedActor) {
            List<ItemComposition> filtered = new List<ItemComposition>();
            for (int i = 0; i < compositions.Length; i++) {
                if (ActorValueCondition.ConditionsMet(compositions[i].conditions, selfActor, suppliedActor)) {
                    filtered.Add(compositions[i]);
                }
            }
            return filtered;
        }

        public void AddItemComposition(ItemComposition[] itemComps, bool sendMessage, Actor selfActor, Actor suppliedActor) {
            bool checkConditions = selfActor != null && suppliedActor != null;
            for (int i = 0; i < itemComps.Length; i++) {
                if (!checkConditions || ActorValueCondition.ConditionsMet(itemComps[i].conditions, selfActor, suppliedActor)) {
                    StashItem(itemComps[i].item, itemComps[i].amount, -1, sendMessage);
                }
            }
        }


        public int GetItemCount (ItemBehavior itemToCheckFor) {
            int slot = GetSlotIndex(itemToCheckFor);
            if (slot != -1) {
                return allInventory[slot].count;
            }
            return 0;
        }
        
        
        public event Action<Inventory, ItemBehavior, int, int> onStash, onDrop;
        
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

        public void StashItem(SceneItem item, int equipSlot, bool sendMessage)
        {
            ItemBehavior itemBehavior = item.itemBehavior;
            StashItem(itemBehavior, itemBehavior.stackable ? item.itemCount : 1, equipSlot, sendMessage);
            //disable the scene item (frees it up for pool)
            item.gameObject.SetActive(false);
            
        }


        public void ClearInventory (bool sendMessage) {
            for (int i = allInventory.Count -1; i >= 0; i--) {
                DropItem(allInventory[i], allInventory[i].count, false, i, true, sendMessage);
            }
        }
        public void AddInventory (List<InventorySlot> slots, bool sendMessage) {
            for (int i =0 ; i < slots.Count; i++) {
                StashItem(slots[i].item, slots[i].count, -1, sendMessage);
            }
        }

        public void DropItem (ItemBehavior itemBehavior, int countDropped, bool getScene, int slotIndex, bool removeEmpty, bool sendMessage) {
            if (slotIndex < 0) slotIndex = GetSlotIndex(itemBehavior);
            if (slotIndex < 0) return;
            DropItem(allInventory[slotIndex], countDropped, getScene, slotIndex, removeEmpty, sendMessage);
        }
            
        public int GetSlotIndex (InventorySlot slot) {
            for (int i = 0; i < allInventory.Count; i++) {
                if (allInventory[i] == slot) return i;
            }
            return -1;
        }
        public int GetSlotIndex (ItemBehavior item) {
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

        public void DropItem (InventorySlot slot, int countDropped, bool getScene, int slotIndex, bool removeEmpty, bool sendMessage) {
            if (slotIndex < 0) slotIndex = GetSlotIndex(slot);
            if (slotIndex < 0) return;
            
            countDropped = Mathf.Min(countDropped, slot.count);
            slot.count -= countDropped;
            
            int newCount = slot.count;
            if (newCount == 0) {
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
                    SceneItem sceneItem = SceneItem.GetSceneItem(slot.item);
                    sceneItem.transform.position = transform.TransformPoint(dropLocalPoint);
                    sceneItem.transform.rotation = Quaternion.Euler(UnityEngine.Random.Range(0,360), UnityEngine.Random.Range(0,360), UnityEngine.Random.Range(0,360));
                    sceneItem.gameObject.SetActive(true);
                }
            }
        }
    }
}
