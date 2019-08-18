using UnityEngine;

namespace Game.InventorySystem.CraftingSystem {
    /*
        add this behavior to an item so it acts like a recipe

        when consumed it takes away the "requires" items from an invnetory
        
        and adds teh "yields" items
    */
    
    [CreateAssetMenu(menuName="Crafting System/Crafting Recipe Item Behavior")]
    public class CraftingRecipeBehavior : StashedItemBehavior
    {   
        [NeatArray] public ItemCompositionArray requires;
        [Space][Space] [NeatArray] public ItemCompositionArray yields;

        public override void OnItemStashed (Inventory inventory, ItemBehavior item, int count, int equipSlot, bool manual) {
            
        }
        public override void OnItemDropped (Inventory inventory, ItemBehavior item, int count) {

        }
        public override void OnItemConsumed (Inventory inventory, ItemBehavior item, int count, int equipSlot) {
            if (inventory.actor == null) {
                Debug.LogError("Inventory " + inventory.GetDisplayName() + " requires an actor script to craft");
                return;
            }
            inventory.RemoveItemCompositionWithAutoScrap(requires, true, inventory.actor, inventory.actor);
            inventory.AddItemComposition (yields, true, inventory.actor, inventory.actor);               
        }

        public override void OnItemEquipped (Inventory inventory, ItemBehavior item, int equipSlot) {
            
        }
        public override void OnItemUnequipped (Inventory inventory, ItemBehavior item, int equipSlot) {
            
        }
    }
}
