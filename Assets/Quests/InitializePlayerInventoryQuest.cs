// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;
using QuestSystem;
using InventorySystem;
using ActorSystem;

public class InitializePlayerInventoryQuest : Quest
{
    public LevelledList levelledList;

        void InitializePlayerInventory () {
            Actor.playerActor.inventory.AddInventory(levelledList.SpawnItems(Actor.playerActor.actorValues, Actor.playerActor.actorValues), sendMessage: false);
        }
        public override void OnQuestInitialize () {
            InitializePlayerInventory();
            CompleteQuest();
        }
        public override void OnUpdateQuest (float deltaTime) { }
        public override string GetHint () { return null; }
        public override void OnQuestComplete () { }
    
}
