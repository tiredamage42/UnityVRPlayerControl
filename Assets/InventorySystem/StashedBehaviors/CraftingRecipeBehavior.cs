using UnityEngine;
using System.Collections.Generic;
using ActorSystem;
namespace InventorySystem {
    
    [CreateAssetMenu(menuName="Stashed Item Behaviors/Crafting Recipe Behavior")]
    public class CraftingRecipeBehavior : StashedItemBehavior
    {   
        [NeatArray] public ItemCompositionArray requires;
        [Space][Space] [NeatArray] public ItemCompositionArray yields;

        public override void OnItemStashed (Inventory inventory, ItemBehavior item, int count, int equipSlot, bool manual) {
            
        }
        public override void OnItemDropped (Inventory inventory, ItemBehavior item, int count) {

        }
        public override void OnItemConsumed (Inventory inventory, ItemBehavior item, int count, int equipSlot) {
            // InventoryCrafter crafter = inventory.crafter;
            // if (crafter != null) {
                Dictionary<string, GameValue> values = inventory.actor != null ? inventory.actor.actorValues : null;
                inventory.RemoveItemComposition(requires, true, true, values, values);
                inventory.AddItemComposition (yields, true, values, values);               
            // }
        }
    }
}
