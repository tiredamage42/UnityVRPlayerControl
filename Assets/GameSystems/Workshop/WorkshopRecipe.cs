using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Game.InventorySystem.CraftingSystem;
namespace Game.InventorySystem.WorkshopSystem {
    
    
    /*
        add this behavior to an item so it acts like a recipe

        when consumed it takes away the "requires" items from an invnetory

        same as crafting recipie, just returns one item though
    */
    
    [CreateAssetMenu(menuName="Workshop System/Workshop Recipe")]
    public class WorkshopRecipe : StashedItemBehavior {
        [NeatArray] public ItemCompositionArray requires;
        public ItemBehavior returnItem;
        public override void OnItemUnequipped(Inventory inventory, ItemBehavior item, int equipSlot) { }
        public override void OnItemEquipped(Inventory inventory, ItemBehavior item, int equipSlot) { }
        public override void OnItemDropped(Inventory inventory, ItemBehavior item, int count) { }
        public override void OnItemStashed(Inventory inventory, ItemBehavior item, int count, int equipSlot, bool manual){ }
        public override void OnItemConsumed (Inventory inventory, ItemBehavior item, int count, int equipSlot) {
            if (inventory.actor == null) {
                Debug.LogError("Inventory " + inventory.GetDisplayName() + " requires an actor script to craft");
                return;
            }
            inventory.RemoveItemCompositionWithAutoScrap(requires, true, inventory.actor, inventory.actor);
        }

    }




}
