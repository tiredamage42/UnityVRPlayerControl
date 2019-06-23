using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem {
    [CreateAssetMenu()]
    public class ItemBehavior : ScriptableObject
    {
        // -1 if set by where equipped from
                // else equip point index (overwritten if quick equipped)
                
        public int equipSlot = -1;

        [Header("Can one scene instance hold multiple of items")]
        public bool stackable;
        public bool allowMultipleStashed = true;
        public string itemName;
        public Item[] scenePrefabVariations;


        // cant be dropped from inventory accidentally
        public bool permanentStash;



        public List<int> stashActions = new List<int> () {
            1
        };
        public List<int> equipActions = new List<int> () {
            0
        };


        public Inventory.EquipType equipType = Inventory.EquipType.Normal;
        public bool hoverLockOnEquip = true;


        [Header("Stashing")]
        public Buffs onStashBuffs;

        // stash use logic needs to happen on scriptable objects
        // because runtime inventory slots only contain a refrence to the item behavior
        // (to prevent having multiple gameObjects of the same item in bloated inventories)
        public StashUseBehavior stashUseBehavior;


        [Header("Equipping")]
        public Buffs onEquipBuffs;
        public TransformBehavior equipTransform;
    }


    public abstract class StashUseBehavior : ScriptableObject {
        public void OnStashedUse (Inventory inventory, ItemBehavior item, int useAction, int slotIndex, int affectingCount, Inventory secondaryInventory) {
            if (useAction == Inventory.UI_DROP_ACTION) {
                inventory.DropItem(item, affectingCount, true);
            }
            else if (useAction == Inventory.UI_TRADE_ACTION) {
                inventory.DropItem(item, affectingCount, false);
                secondaryInventory.StashItem(item, affectingCount);
            }
            else {
                _OnStashedUse ( inventory, item, useAction, slotIndex, affectingCount, secondaryInventory);
            }

        }
        protected abstract void _OnStashedUse (Inventory inventory, ItemBehavior item, int useAction, int slotIndex, int affectingCount, Inventory secondaryInventory);
    }
        
        
// [CreateAssetMenu()]
    
    // public class ConsumableStashUseBehavior : StashUseBehavior {
    //     public Buffs consumeBuffs;
    //     public override void OnStashedUse (Inventory inventory, Inventory.InventorySlot slot, int useAction) {
            
    //         inventory.GetComponent<Actor>().GiveBuffs(consumeBuffs);
    //         // inventory.EquipItem(slot.item);

    //     }
    // }
/*



items have modifiers that modify owner values


Set | Add | Multiply

Base | Max     

Variable Name

Value

isOneOff  
    (modifier cant be removed, and is permanent 
        i.e level up adds 100 to max health, 
        or health pack adds health but then is let go (so cant remove modifier)
    )

gameMessage 

 */

 [System.Serializable] public class ActorValue {

 }

 [System.Serializable] public class Buff {

 }


    public class Buffs : ScriptableObject {

    }
}
