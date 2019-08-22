// using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using InteractionSystem;

namespace Game.InventorySystem {
    public interface ISceneItem {
        void OnEquipped(Inventory inventory);
        void OnUnequipped(Inventory inventory);
        void OnEquippedUpdate(Inventory inventory);   
        void OnEquippedUseStart(Inventory inventory, int useIndex);
        void OnEquippedUseEnd(Inventory inventory, int useIndex);
        void OnEquippedUseUpdate(Inventory inventory, int useIndex);
    }

    /*
        scene representation of the item
    */
    public class SceneItem : MonoBehaviour, IInteractable
    {
        public int GetInteractionMode() { return itemBehavior.onlyWorkshop ? 1 : 0; }


        public static List<SceneItem> allSceneItems = new List<SceneItem>();

        static void RemoveSceneItem (SceneItem sceneItem) {
            allSceneItems.Remove(sceneItem);
        }
        static void AddSceneItem (SceneItem sceneItem) {
            if (!allSceneItems.Contains(sceneItem)) {
                allSceneItems.Add(sceneItem);
            }
        }

        void OnEnable () {
            AddSceneItem(this);
        }
        void OnDestroy () {
            RemoveSceneItem(this);
        }
        void OnDiable () {
            RemoveSceneItem(this);
        }
    

        static Dictionary<int, HashSet<SceneItem>> itemPoolsPerPrefab = new Dictionary<int, HashSet<SceneItem>>();
        
        
        // TODO: add a way to ensure scene variation
        // TODO: add count
        public static SceneItem GetSceneItem (ItemBehavior itemBehavior) {
            SceneItem prefab = itemBehavior.scenePrefabVariations[Random.Range(0, itemBehavior.scenePrefabVariations.Length)];
            
            int instanceID = prefab.GetInstanceID();
            
            SceneItem sceneItem = null;
            bool hasKey = false;
            if (itemPoolsPerPrefab.ContainsKey(instanceID)) {
                hasKey = true;

                foreach (var it in itemPoolsPerPrefab[instanceID]) {
                    if (!it.gameObject.activeInHierarchy) {
                        sceneItem = it;

                        // turn on and unparent
                        break;
                    }
                }
            }
            
            if (sceneItem == null) {
                sceneItem = GameObject.Instantiate(prefab);

                if (hasKey) {
                    itemPoolsPerPrefab[instanceID].Add(sceneItem);
                }
                else {
                    itemPoolsPerPrefab.Add(instanceID, new HashSet<SceneItem>(){ sceneItem });
                }
            }

            sceneItem.itemBehavior = itemBehavior;
            return sceneItem;
        }


        public int itemCount = 1;
        public ItemBehavior itemBehavior;
        Interactable interactable;
        [HideInInspector] new public Rigidbody rigidbody;

        void Awake () {
            interactable = GetComponent<Interactable>();
            rigidbody = GetComponent<Rigidbody>();
            itemColliders = GetComponentsInChildren<Collider>();

            // interactable.actionNames = new string[] { "Grab", "Stash" };

            InitializeListeners();
        }
        List<ISceneItem> listeners = new List<ISceneItem>();
        void InitializeListeners() {
            ISceneItem[] listeners_ = GetComponents<ISceneItem>();
            for (int i = 0; i< listeners_.Length; i++) {
                this.listeners.Add(listeners_[i]);
            }
        }
        public void AddListener (ISceneItem listener) {
            listeners.Add(listener);
        }

        Collider[] itemColliders;
        public void OnInteractableAvailabilityChange(bool available) { }
        public void OnInteractableInspectedStart(InteractionPoint interactor) {}
        public void OnInteractableInspectedEnd(InteractionPoint interactor) {}
        public void OnInteractableInspectedUpdate(InteractionPoint interactor) {}

        // when an action is performed on the item while it's in its interactable (unequipped) state
        public void OnInteractableUsedStart(InteractionPoint interactor, int useIndex) {
            if (!itemBehavior.onlyWorkshop) { // dont allow interaction with workshop only items
                if (interactor.inventory != null) 
                    interactor.inventory.OnSceneItemActionStart (this, interactor.interactorID, useIndex);
            }
        }
        public void OnInteractableUsedEnd(InteractionPoint interactor, int useIndex) {

        }
        public void OnInteractableUsedUpdate(InteractionPoint interactor, int useIndex) {

        }

        [HideInInspector] public Inventory parentInventory;
        [HideInInspector] public EquipPoint myEquipPoint;

        void TemporarilySetCollidersToEquippedItemLayer () {
            for (int i = 0; i < itemColliders.Length; i++) {
                colliderLayers.Add(new ColliderLayerPair(itemColliders[i], Inventory.equippedItemLayer));
            }
        }
        void ResetItemColliderLayersToOriginals () {
            for (int i = colliderLayers.Count - 1; i >= 0; i--) {
                colliderLayers[i].ResetPair();
                colliderLayers.Remove(colliderLayers[i]);
            }
        }

        List<ColliderLayerPair> colliderLayers = new List<ColliderLayerPair>();

        struct ColliderLayerPair {
            Collider collider;
            int originalLayer;
            public void ResetPair () {
                collider.gameObject.layer = originalLayer;
            }

            public ColliderLayerPair (Collider collider, string newLayer) {
                this.collider = collider;
                originalLayer = collider.gameObject.layer;
                collider.gameObject.layer = LayerMask.NameToLayer(newLayer);
            }
        }
        
        public void OnEquipped (Inventory inventory) {
            interactable.SetAvailable(false);
            TemporarilySetCollidersToEquippedItemLayer();
            for (int i = 0; i < listeners.Count; i++) {
                listeners[i].OnEquipped(inventory);
            }
        }
        public void OnUnequipped (Inventory inventory) {
            interactable.SetAvailable(true);
            ResetItemColliderLayersToOriginals();
            for (int i = 0; i < listeners.Count; i++) {
                listeners[i].OnUnequipped(inventory);
            }
        }
        public void OnEquippedUpdate(Inventory inventory) {
            for (int i = 0; i < listeners.Count; i++) {
                listeners[i].OnEquippedUpdate(inventory);
            }
        }

        public void OnEquippedUseStart (Inventory interactor, int useIndex) {
            for (int i = 0; i < listeners.Count; i++) {
                listeners[i].OnEquippedUseStart(interactor, useIndex);
            }
        }
        public void OnEquippedUseEnd (Inventory inventory, int useIndex) {
            for (int i = 0; i < listeners.Count; i++) {
                listeners[i].OnEquippedUseEnd(inventory, useIndex);
            }
        }
        public void OnEquippedUseUpdate(Inventory interactor, int useIndex) {
            for (int i = 0; i < listeners.Count; i++) {
                listeners[i].OnEquippedUseUpdate(interactor, useIndex);
            }
        }
    }
}
