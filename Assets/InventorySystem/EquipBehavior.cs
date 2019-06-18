// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem {

    [CreateAssetMenu()]
    public class EquipBehavior : ScriptableObject
    {
        [System.Serializable] public class EquipSetting {
            public Vector3 position, rotation;
        }
        public EquipSetting[] equipSettings;
    }
}
