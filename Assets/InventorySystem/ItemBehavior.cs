using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
    recipe:


        Composition[] requires

        [SHOULD BE PERMANENT]
        actorbuffs[] // to give xp for instance

        Composition[] yields

        
 */


/*



SEPERATE STASHED ITEM TO HAVE 
    ON STASH 
    ON CONSUME
    ON UNSTASH

CALLBACKS

CAN SEPERATE BUFFS INTO OTHER COMPONENTS


CONSUME ON STASH OPTION FOR ITEM MAYBE


 */














namespace InventorySystem {
    [CreateAssetMenu(menuName="Inventory System/Item Behavior")]
    public class ItemBehavior : ScriptableObject
    {
        public string itemName;
        [Header("Set to false for utility items")]
        public bool keepOnStash = true;
        
        
        // -1 if set by where equipped from
                // else equip point index (overwritten if quick equipped)
                
        public bool allowMultipleStashed = true;


        [Header("Cant be dropped from inventory accidentally")]
        public bool permanentStash;
        public int category;

        // stash use logic needs to happen on scriptable objects
        // because runtime inventory slots only contain a refrence to the item behavior
        // (to prevent having multiple gameObjects of the same item in bloated inventories)
        public StashedItemBehavior[] stashedItemBehaviors;

        public float weight = 1;
        [Range(0,100)] public float value = 50;

        public ItemComposition[] composedOf;

        
        [Header("Scene Behavior")]
        public bool canQuickEquip = true;
        public Item[] scenePrefabVariations;

        [Header("'Armor Slot', -1 for anything that isnt wearable")]
        public int equipSlot = -1;

        [Header("Can one scene instance hold multiple of items ?")]
        public bool stackable;

        public Inventory.EquipType equipType = Inventory.EquipType.Normal;
        public bool hoverLockOnEquip = true;

        public TransformBehavior equipTransform;

        // public List<int> stashActions = new List<int> () {
        //     1
        // };
        // public List<int> equipActions = new List<int> () {
        //     0
        // };




        // [Header("Stashing")]
        // public Buffs onStashBuffs;

        // public StashUseBehavior stashUseBehavior;
        // public StashedItem stashedItemBehavior;

        

        // [Header("Equipping")]
        // public Buffs onEquipBuffs;

        
        /*
            TODO: add multiplier values
        */
        public bool OnConsume (Inventory inventory, int count, int slot)
        {
            bool hasBehaviors = stashedItemBehaviors.Length != 0;
            for (int i = 0; i < stashedItemBehaviors.Length; i++) {
                stashedItemBehaviors[i].OnItemConsumed(inventory, this, count, slot);
            }
            return hasBehaviors;
            // if (stashedItemBehavior != null) {
            //     stashedItemBehavior.OnItemConsumed(inventory, this, count, slot);
            //     return true;
            // }
            // return false;
        }
    }

    [System.Serializable] public class ItemComposition {
        public ItemBehavior item;
        public int amount = 1;
    }



    // public abstract class StashUseBehavior : ScriptableObject {
    //     public void OnStashedUse (Inventory inventory, ItemBehavior item, int useAction, int slotIndex, int affectingCount, Inventory secondaryInventory) {
    //         if (useAction == Inventory.UI_DROP_ACTION) {
    //             inventory.DropItem(item, affectingCount, true);
    //         }
    //         else if (useAction == Inventory.UI_TRADE_ACTION) {
    //             inventory.DropItem(item, affectingCount, false);
    //             secondaryInventory.StashItem(item, affectingCount);
    //         }
    //         else {
    //             _OnStashedUse ( inventory, item, useAction, slotIndex, affectingCount, secondaryInventory);
    //         }

    //     }
    //     protected abstract void _OnStashedUse (Inventory inventory, ItemBehavior item, int useAction, int slotIndex, int affectingCount, Inventory secondaryInventory);
    // }
        
        
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

//  [System.Serializable] public class ActorValue {

//  }

//  [System.Serializable] public class Buff {

//  }


//     public class Buffs : ScriptableObject {

//     }
}
