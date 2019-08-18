using System.Collections.Generic;
using UnityEngine;

namespace Game.InventorySystem {
    /*
        spawns a levelled list of inventory items to populate an inventory


        TODO: figure out a way for quick lootables (such as crates) to only spawn when checked
    */
    [RequireComponent(typeof(Inventory))] 
    public class LevelledListSpawner : MonoBehaviour {
        public LevelledList levelledList;

        [Header("Defaults to player actor if null and no Actor attached to this object")]
        public Actor selfActorForConditions;

        [Header("Defaults to player actor if null")]
        public Actor suppliedActorForConditions;
        
        // [Header("In Game Days, -1 for no respawn")]
        // public float respawnRate = 3.0f;
        public bool respawnDebug;
        Inventory inventory;
        
        void Awake () {
            inventory = GetComponent<Inventory>();
        }
        void Start () {
            //if no supplied actor, use player actor...
            if (suppliedActorForConditions == null) suppliedActorForConditions = Actor.playerActor;
            
            // try to get an actor attached to this object first (for self)
            if (selfActorForConditions == null) selfActorForConditions = GetComponent<Actor>();
            if (selfActorForConditions == null) selfActorForConditions = Actor.playerActor;
            
            Respawn();
        }
        void Update () {
            if (respawnDebug) {
                Respawn();
                respawnDebug = false;
            }
        }


        public void Respawn () {
            if (levelledList == null) {
                Debug.LogError(name + " levelled list spawner, cant spawn, levelled list is null");
                return;
            }
            inventory.ClearInventory(false);
            inventory.AddInventory(levelledList.SpawnItems(selfActorForConditions, suppliedActorForConditions), false);
        }
    }
}
