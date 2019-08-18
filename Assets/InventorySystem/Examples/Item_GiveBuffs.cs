using UnityEngine;
using Game.InventorySystem;
using Game;

// TODO : figure out a way to assert permanent buffs in editor...

[CreateAssetMenu(menuName="Inventory System/Item Behaviors/Give Buffs")]
public class Item_GiveBuffs : StashedItemBehavior
{
    // TODO: make same ish behavior, but gives perks (sets perk levels on inventory perk holder if any)
    [NeatArray] public GameValueModifierArray equipBuffs;
    
    [Space] [Space] [Space] 
    [NeatArray] public GameValueModifierArray stashBuffs;
    [Space] [Space] [Space] 
    [Header("Consume Buffs Should Be Permanent")]
    [NeatArray] public GameValueModifierArray consumeBuffs;

    public override void OnItemEquipped (Inventory inventory, ItemBehavior item, int equipSlot) {
        if (equipBuffs.list.Length > 0 && inventory.actor != null) {
            inventory.actor.AddBuffs(equipBuffs, 1, item.GetInstanceID(), 0, assertPermanent: false, inventory.actor, inventory.actor);
        }
    }
    public override void OnItemUnequipped (Inventory inventory, ItemBehavior item, int equipSlot) {
        if (equipBuffs.list.Length > 0 && inventory.actor != null) {
            inventory.actor.RemoveBuffs(equipBuffs, 1, item.GetInstanceID(), 0);
        }
    }
        
    public override void OnItemStashed (Inventory inventory, ItemBehavior item, int count, int equipSlot, bool manual) {
        if (stashBuffs.list.Length > 0 && inventory.actor != null) {
            inventory.actor.AddBuffs(stashBuffs, count, item.GetInstanceID(), 0, assertPermanent: false, inventory.actor, inventory.actor);
        }
    }
    public override void OnItemDropped (Inventory inventory, ItemBehavior item, int count) {
        if (stashBuffs.list.Length > 0 && inventory.actor != null) {
            inventory.actor.RemoveBuffs(stashBuffs, count, item.GetInstanceID(), 0);
        }
    }
    
    public override void OnItemConsumed (Inventory inventory, ItemBehavior item, int count, int equipSlot) {
        if (consumeBuffs.list.Length > 0 && inventory.actor != null) {
            inventory.actor.AddBuffs(consumeBuffs, count, item.GetInstanceID(), 0, assertPermanent: true, inventory.actor, inventory.actor);
        }
    }
}