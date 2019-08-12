using System.Collections.Generic;
using UnityEngine;

using ActorSystem;

namespace InventorySystem {
    [RequireComponent(typeof(Inventory))]
    public class LevelledListSpawner : MonoBehaviour {
        public LevelledList levelledList;

        [Header("In Game Days, -1 for no respawn")]
        public float respawnRate = 3.0f;
        Inventory inventory;
        public Actor selfActor, suppliedActor;
        
        public bool respawnDebug;
        
        void Awake () {
            inventory = GetComponent<Inventory>();
            
            if (selfActor == null) {
                selfActor = GetComponent<Actor>();
            }
        }
        void Start () {
            if (suppliedActor == null) suppliedActor = Actor.playerActor;
            if (selfActor == null) selfActor = suppliedActor;
            
            Respawn(selfActor.actorValues, suppliedActor.actorValues);
        }
        void Update () {
            if (respawnDebug) {
                Respawn(selfActor.actorValues, suppliedActor.actorValues);
                respawnDebug = false;
            }
        }


        public void Respawn (Dictionary<string, GameValue> selfValues, Dictionary<string, GameValue> suppliedValues) {
            inventory.ClearInventory();
            List<Inventory.InventorySlot> spawnList = new List<Inventory.InventorySlot>();
            GetSpawnedList(selfValues, suppliedValues, levelledList, spawnList);
            inventory.AddInventory(spawnList);
        }

        public static void GetSpawnedList (Dictionary<string, GameValue> selfValues, Dictionary<string, GameValue> suppliedValues, LevelledList levelledList, List<Inventory.InventorySlot> spawnList) {
            bool didSpawn = false;

            //CHECK FOR INJECTED RUNTIME LISTS ASSOCIATED WITH SUPPLIED LEVELLED LIST HERE
            
            LevelledList.ListItem[] listItems = levelledList.listItems;

            for (int i =0 ; i < listItems.Length; i++) {
                LevelledList.ListItem listItem = listItems[i];
                if (Random.value > listItem.chanceForNone) {


                    int count = Random.Range(listItem.minMax.x, listItem.minMax.y+1);
                    if (count > 0) {

                        if (GameValueCondition.ConditionsMet (listItem.conditions, selfValues, suppliedValues)) {
                            spawnList.Add(new Inventory.InventorySlot(listItem.item, count));
                            didSpawn = true;

                            if (levelledList.singleSpawn) {
                                break;
                            }    
                        }
                    }
                }
            }

            if (!didSpawn) {
                LevelledList.ListItem[] fallBacks = levelledList.fallBacks;

                for (int i =0 ; i < fallBacks.Length; i++) {
                    LevelledList.ListItem listItem = fallBacks[i];

                    if (GameValueCondition.ConditionsMet (listItem.conditions, selfValues, suppliedValues)) {
                        spawnList.Add(new Inventory.InventorySlot(listItem.item, Random.Range(Mathf.Max(listItem.minMax.x, 1), listItem.minMax.y + 1)));
                    }
                }
            }
        }
    }
}
