// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;


using ActorSystem;

namespace InventorySystem {


[CreateAssetMenu(menuName="Stashed Item Behaviors/Give Buffs")]
public class GiveBuffs : StashedItemBehavior
{
    [NeatArray] public GameValueModifierArray stashBuffs;
    // [Header("Should be permanent")]
    [Space] [Space] [Space] 
    [NeatArray] public GameValueModifierArray consumeBuffs;
        
    public override void OnItemStashed (Inventory inventory, ItemBehavior item, int count, int equipSlot, bool manual) {
        GameValueModifier[] buffs = stashBuffs;

        if (buffs.Length == 0)
            return;

        Actor actor = inventory.GetComponent<Actor>();
        if (actor != null) {
            actor.AddBuffs(buffs, count, item.GetInstanceID(), 0, false, actor.GetValueDictionary(), actor.GetValueDictionary());
        }
    }
    public override void OnItemDropped (Inventory inventory, ItemBehavior item, int count) {
        GameValueModifier[] buffs = stashBuffs;

        if (buffs.Length == 0)
            return;
        
        Actor actor = inventory.GetComponent<Actor>();
        if (actor != null) {
            actor.RemoveBuffs(buffs, count, item.GetInstanceID(), 0);
        }
    }

    public override void OnItemConsumed (Inventory inventory, ItemBehavior item, int count, int equipSlot) {
        GameValueModifier[] buffs = consumeBuffs;

        if (buffs.Length == 0)
            return;
        
        Actor actor = inventory.GetComponent<Actor>();

        if (actor != null) {
            //assert permanent
            actor.AddBuffs(buffs, count, item.GetInstanceID(), 0, true, actor.GetValueDictionary(), actor.GetValueDictionary());
        }
    }
}

}