// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;


using ActorSystem;

namespace InventorySystem {


[CreateAssetMenu(menuName="Stashed Item Behaviors/Give Buffs")]
public class GiveBuffs : StashedItemBehavior//MonoBehaviour, IStashedItem
{

    [System.Serializable] public class GVMArray : NeatArrayWrapper<GameValueModifier> { }

    [DisplayedArray(new float[] {0,0,0,.1f}, true)] 
    public GVMArray stashBuffs;
    
    // [Header("Should be permanent")]
    [Space] [Space] [Space] 
    [DisplayedArray(new float[] {0,0,0,.1f}, true)] 
    public GVMArray consumeBuffs;
        

    
    public override void OnItemStashed (Inventory inventory, ItemBehavior item, int count, int equipSlot, bool manual) {
        GameValueModifier[] buffs = stashBuffs;

        if (buffs.Length == 0)
            return;

        Actor actor = inventory.GetComponent<Actor>();
        if (actor != null) {
            
            // for (int i = 0; i < stashBuffs.Length; i++) {
                // actor.AddBuffs(stashBuffs[i], count, item.GetInstanceID(), i, false);
            // }   

             actor.AddBuffs(buffs, count, item.GetInstanceID(), 0, false);
        }
    }
    public override void OnItemDropped (Inventory inventory, ItemBehavior item, int count) {
        GameValueModifier[] buffs = stashBuffs;

        if (buffs.Length == 0)
            return;
        
        Actor actor = inventory.GetComponent<Actor>();
        if (actor != null) {
            // for (int i = 0; i < stashBuffs.Length; i++) {
            //     actor.RemoveBuffs(stashBuffs[i], count, item.GetInstanceID(), i);
            // }

            actor.RemoveBuffs(buffs, count, item.GetInstanceID(), 0);
        }
    }

    public override void OnItemConsumed (Inventory inventory, ItemBehavior item, int count, int equipSlot) {
        GameValueModifier[] buffs = consumeBuffs;

        if (buffs.Length == 0)
            return;
        
        Actor actor = inventory.GetComponent<Actor>();

        if (actor != null) {
            // for (int i = 0; i < consumeBuffs.Length; i++) {


                // for (int x = 0; x < consumeBuffs[i].buffs.Length; x++) {
                //     GameValueModifier mod = consumeBuffs[i].buffs[x];

                //     if (
                //         mod.modifyValueComponent == GameValue.GameValueComponent.Value ||
                //         mod.modifyValueComponent == GameValue.GameValueComponent.MinValue ||
                //         mod.modifyValueComponent == GameValue.GameValueComponent.MaxValue
                //     ) {
                //         Debug.LogError("non permanent stacked buff found in OnItemConsumed. Consume Buff Index: " + i.ToString() + " on " + name);
                //         // Debug.LogError("non permanent stacked buff found in OnItemConsumed. Consume Buff Index: " + consumeBuffs[i].name + " on " + name);
                        
                //         break;
                //     }
                // }
                //assert permanent
                
            //     actor.AddBuffs(consumeBuffs[i], count, item.GetInstanceID(), i, true);
            // }   

            actor.AddBuffs(buffs, count, item.GetInstanceID(), 0, true);
        }
    }
}

}