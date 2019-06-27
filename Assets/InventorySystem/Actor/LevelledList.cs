using System.Collections.Generic;
using UnityEngine;
using ActorSystem;

namespace InventorySystem {

    [CreateAssetMenu()]
    public class LevelledList : ScriptableObject
    {
        [System.Serializable] public class ListItem {
            public ItemBehavior item;
            public int min=2, max=5;
            [Range(0,1)] public float chanceForNone = .5f;
        }

        public GameValueCondition[] conditions;

        [Header("Only One List Item Spawned")]
        public bool singleSpawn;
        public ListItem[] listItems;

        [Header("ListItems spawned 100% if no original ListItems spawned")]
        public ListItem[] fallBacks;

        //check conditons per sublist
        public LevelledList[] subLists;
    }
}
