using UnityEngine;
using ActorSystem;

namespace InventorySystem {
    // TODO : figure out a way to assert permanent buffs in editor...

    [CreateAssetMenu(menuName="Stashed Item Behaviors/Give Buffs")]
    public class GiveBuffs : StashedItemBehavior
    {
        [NeatArray] public GameValueModifierArray stashBuffs;
        // [Header("Should be permanent")]
        [Space] [Space] [Space] 
        [NeatArray] public GameValueModifierArray consumeBuffs;
            
        public override void OnItemStashed (Inventory inventory, ItemBehavior item, int count, int equipSlot, bool manual) {
            if (consumeBuffs.list.Length > 0 && inventory.actor != null) {
                inventory.actor.AddBuffs(stashBuffs, count, item.GetInstanceID(), 0, assertPermanent: false, inventory.actor.actorValues, inventory.actor.actorValues);
            }
        }
        public override void OnItemDropped (Inventory inventory, ItemBehavior item, int count) {
            if (stashBuffs.list.Length > 0 && inventory.actor != null) {
                inventory.actor.RemoveBuffs(stashBuffs, count, item.GetInstanceID(), 0);
            }
        }
        public override void OnItemConsumed (Inventory inventory, ItemBehavior item, int count, int equipSlot) {
            if (consumeBuffs.list.Length > 0 && inventory.actor != null) {
                inventory.actor.AddBuffs(consumeBuffs, count, item.GetInstanceID(), 0, assertPermanent: true, inventory.actor.actorValues, inventory.actor.actorValues);
            }
        }
    }
}