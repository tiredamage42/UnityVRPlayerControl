using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ActorSystem;

namespace InventorySystem {
    
    [CreateAssetMenu(menuName="Stashed Item Behaviors/Crafting Recipe Behavior")]
    public class CraftingRecipeBehavior : StashedItemBehavior// MonoBehaviour, IStashedItem
    {

        public ItemComposition[] requires;
        public ItemComposition[] yields;
        
    
        public override void OnItemStashed (Inventory inventory, ItemBehavior item, int count, int equipSlot, bool manual) {
            
        }
        public override void OnItemDropped (Inventory inventory, ItemBehavior item, int count) {

        }
        public override void OnItemConsumed (Inventory inventory, ItemBehavior item, int count, int equipSlot) {
            
            //cehck if we have teh components in inventory
            
            //if not send message to inventory "missing required components"

            //else send message for each item composition yield

            //DO ABOVE IN UI to prevent on item sonsumed form happening if not enough components


            //foreach item composition yield, inventory.Stash(yields[i])




               
        }
    }
}
