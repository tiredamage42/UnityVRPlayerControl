using System.Collections;
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

        public Actor actorValuesToUse;
        
        void Awake () {
            inventory = GetComponent<Inventory>();
        }

        void Start () {
            if (actorValuesToUse == null){
                actorValuesToUse = Actor.playerActor;
            }
            if (actorValuesToUse != null) {
                Respawn(actorValuesToUse.GetValueDictionary());
            }
        }

        public void Respawn (Dictionary<string, GameValue> gameValues) {
            inventory.ClearInventory();

            List<Inventory.InventorySlot> spawnList = new List<Inventory.InventorySlot>();
            GetSpawnedList(gameValues, levelledList, spawnList);

            inventory.AddInventory(spawnList);
        }

        static void GetSpawnedList (Dictionary<string, GameValue> gameValues, LevelledList levelledList, List<Inventory.InventorySlot> spawnList) {
            
            if (GameValueCondition.ConditionsMet (levelledList.conditions, gameValues)) {

                bool didSpawn = false;

                for (int i =0 ; i < levelledList.listItems.Length; i++) {
                    LevelledList.ListItem listItem = levelledList.listItems[i];
                    if (Random.value > listItem.chanceForNone) {
                        int count = Random.Range(listItem.min, listItem.max+1);

                        if (count > 0) {
                            spawnList.Add(new Inventory.InventorySlot(listItem.item, count));
                            didSpawn = true;

                            if (levelledList.singleSpawn) {
                                break;
                            }    
                        }
                    }
                }

                if (!didSpawn) {
                    for (int i =0 ; i < levelledList.fallBacks.Length; i++) {
                        LevelledList.ListItem listItem = levelledList.fallBacks[i];
                        spawnList.Add(new Inventory.InventorySlot(listItem.item, Random.Range(Mathf.Max(listItem.min, 1), listItem.max + 1)));
                            
                    }
                }

                for (int i = 0; i < levelledList.subLists.Length; i++) {
                    GetSpawnedList (gameValues, levelledList.subLists[i], spawnList);
                }
            }
        }
    }
}
