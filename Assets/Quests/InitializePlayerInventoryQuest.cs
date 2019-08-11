// using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuestSystem;
using InventorySystem;
using ActorSystem;

public class InitializePlayerInventoryQuest : Quest
{
    public LevelledList levelledList;

        void InitializePlayerInventory () {
            List<Inventory.InventorySlot> spawnList = new List<Inventory.InventorySlot>();
            LevelledListSpawner.GetSpawnedList(Actor.playerActor.GetValueDictionary(), Actor.playerActor.GetValueDictionary(), levelledList, spawnList);
            Actor.playerActor.GetComponent<Inventory>().AddInventory(spawnList);
        }
        public override void OnQuestInitialize () {
            InitializePlayerInventory();
            CompleteQuest();
        }
        public override void OnUpdateQuest (float deltaTime) { }
        public override string GetHint () { return null; }
        public override void OnQuestComplete () { }
    
}
