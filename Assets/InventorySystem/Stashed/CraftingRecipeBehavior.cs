using UnityEngine;

namespace InventorySystem {
    
    [CreateAssetMenu(menuName="Stashed Item Behaviors/Crafting Recipe Behavior")]
    public class CraftingRecipeBehavior : StashedItemBehavior
    {   
        [NeatArray] public ItemCompositionArray requires;

        [Space][Space]
        [NeatArray] public ItemCompositionArray yields;

        public override void OnItemStashed (Inventory inventory, ItemBehavior item, int count, int equipSlot, bool manual) {
            
        }
        public override void OnItemDropped (Inventory inventory, ItemBehavior item, int count) {

        }
        public override void OnItemConsumed (Inventory inventory, ItemBehavior item, int count, int equipSlot) {
            InventoryCrafter crafter = inventory.GetComponent<InventoryCrafter>();
            ActorSystem.Actor actor = inventory.GetComponent<ActorSystem.Actor>();
            crafter.RemoveItemComposition(requires, true, actor.GetValueDictionary(), actor.GetValueDictionary());
            crafter.AddItemComposition (yields, true, actor.GetValueDictionary(), actor.GetValueDictionary());               
        }
    }
}
