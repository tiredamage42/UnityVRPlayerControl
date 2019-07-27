// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

namespace ActorSystem {

    // [CreateAssetMenu()]
    [System.Serializable]
    public class ActorBuff //: ScriptableObject
    {
        public GameValueCondition[] conditions;
        public GameValueModifier[] buffs;
    }

}