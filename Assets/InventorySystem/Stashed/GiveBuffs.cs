using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using InventorySystem;

namespace ActorSystem {

public class GiveBuffs : MonoBehaviour, IStashedItem
{
    public ActorBuff[] stashBuffs, consumeBuffs;

    
    public void OnItemStashed (Inventory inventory, ItemBehavior item, int count) {
        Actor actor = inventory.GetComponent<Actor>();
        if (actor != null) {
            for (int i = 0; i < stashBuffs.Length; i++) {
                actor.AddBuffs(stashBuffs[i], count, item.GetInstanceID());
            }
            
        }

    }
    public void OnItemDropped (Inventory inventory, ItemBehavior item, int count) {
        Actor actor = inventory.GetComponent<Actor>();
        if (actor != null) {
            for (int i = 0; i < stashBuffs.Length; i++) {
                actor.RemoveBuffs(stashBuffs[i], count, item.GetInstanceID());
            }
        }

    }
    public void OnItemConsumed (Inventory inventory, ItemBehavior item, int count, int equipSlot) {

        Actor actor = inventory.GetComponent<Actor>();

        if (actor != null) {
            for (int i = 0; i < consumeBuffs.Length; i++) {
                for (int x = 0; x < consumeBuffs[i].buffs.Length; x++) {
                    GameValueModifier mod = consumeBuffs[i].buffs[x];

                    if (
                        mod.modifyValueComponent == GameValue.GameValueComponent.Value ||
                        mod.modifyValueComponent == GameValue.GameValueComponent.MinValue ||
                        mod.modifyValueComponent == GameValue.GameValueComponent.MaxValue
                    ) {
                        Debug.LogError("non permanent stacked buff found in OnItemConsumed. Buff: " + consumeBuffs[i].name + " on " + name);
                        break;
                    }
                }
                
                actor.AddBuffs(consumeBuffs[i], count, item.GetInstanceID());
            }


            
        }

        
    }
}

}