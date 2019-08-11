using UnityEngine;

namespace InventorySystem {
    
    [CreateAssetMenu(menuName="Stashed Item Behaviors/Crafting Recipe Behavior")]
    public class CraftingRecipeBehavior : StashedItemBehavior
    {   
        [DisplayedArray] public ItemCompositionArray requires;

        [Space][Space]
        [DisplayedArray] public ItemCompositionArray yields;

        public override void OnItemStashed (Inventory inventory, ItemBehavior item, int count, int equipSlot, bool manual) {
            
        }
        public override void OnItemDropped (Inventory inventory, ItemBehavior item, int count) {

        }
        public override void OnItemConsumed (Inventory inventory, ItemBehavior item, int count, int equipSlot) {
            InventoryCrafter crafter = inventory.GetComponent<InventoryCrafter>();

            crafter.RemoveItemComposition(requires);
            crafter.AddItemComposition (yields);               
        }
    }
}
