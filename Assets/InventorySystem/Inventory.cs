using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VRPlayer;


using Valve.VR;

using System;

using InteractionSystem;


namespace InventorySystem {


/*


    recipes create items

    item getes stashed

    for workshop


        instantiate scen items for item returns on current recipe

            NOW HOVERLOCK to keep hovered over
            highlight on hover true
            keephighlighted


        workshop item script:
            bool isSelected = true; // wheneber instantiated tehy're the selected ones
            current recipe

            onuseDown () {

                if (selected) {
                    do whatever rigidibdy tstuff to place the object

                    
                    if (current recipe != null) {
                        current recipe create (
                            inventory, at position
                        ) ;

                        that gives items (
                            workshop stashed items should be consume on equip
                            then on consume gives permanent xp buff
                            also drop on consume
                         )


                    }


                        
                }
                else {

                }



            }

        

        prefab to instantiate

        OnStash (inventory, item, count) {
            //instantiate scene item

            //drop item

        }







 */


public class Inventory : MonoBehaviour
{
    public ActorSystem.Actor actor;
    public InventoryEqupping equipping;
    public InventoryCrafter crafter;
    void InitializeComponents () {
        actor = GetComponent<ActorSystem.Actor>();
        equipping = GetComponent<InventoryEqupping>();
        crafter = GetComponent<InventoryCrafter> ();
    }

    void Awake () {
        InitializeComponents();
    }



    public const string equippedItemLayer = "EquippedItem";

    public event System.Action<Inventory, int, Inventory, string, List<int>> onInventoryManagementInitiate;
    public event System.Action<Inventory, int, string> onEndInventoryManagement;

    public void InitiateInventoryManagement (string context, int equipID, Inventory secondaryInventory, List<int> categoryFilter) {
        if (onInventoryManagementInitiate != null) {
            onInventoryManagementInitiate(this, equipID, secondaryInventory, context, categoryFilter);
        }
    }
    public void EndInventoryManagement (string context, int equipID) {
        if (onEndInventoryManagement != null) {
            onEndInventoryManagement(this, equipID, context);
        }
    }
    
    // // Create a layer at the next available index. Returns silently if layer already exists.
    // public static void CreateLayer(string name)
    // {
    //     if (string.IsNullOrEmpty(name))
    //         throw new System.ArgumentNullException("name", "New layer name string is either null or empty.");

    //     var tagManager = new UnityEditor.SerializedObject(UnityEditor.AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
    //     var layerProps = tagManager.FindProperty("layers");
    //     var propCount = layerProps.arraySize;

    //     UnityEditor.SerializedProperty firstEmptyProp = null;

    //     for (var i = 0; i < propCount; i++)
    //     {
    //         var layerProp = layerProps.GetArrayElementAtIndex(i);
    //         var stringValue = layerProp.stringValue;

    //         if (stringValue == name) return;

    //         if (i < 8 || stringValue != string.Empty) 
    //             continue;

    //         if (firstEmptyProp == null) {
    //             firstEmptyProp = layerProp;
    //             break;
    //         }
    //     }

    //     if (firstEmptyProp == null)
    //     {
    //         Debug.LogError("Maximum limit of " + propCount + " layers exceeded. Layer \"" + name + "\" not created.");
    //         return;
    //     }

    //     firstEmptyProp.stringValue = name;
    //     tagManager.ApplyModifiedProperties();
    // }


    
    public const string quickTradeContext = "QuickTrade";
    public const string fullTradeContext  = "FullTrade";

    
    
    // weapons, armor, meds
    public List<int> favoriteAbleCategories = new List<int>() {
        0, 1, 2
    };

    public List<Inventory.InventorySlot> GetFilteredInventory( List<int> categoryFilter ) {
        if (categoryFilter == null || categoryFilter.Count == 0)
            return allInventory;
        
        List<Inventory.InventorySlot> r = new List<Inventory.InventorySlot>();
        for (int i = 0; i < allInventory.Count; i++) {
            if (categoryFilter == null || categoryFilter.Count == 0) {
                if (allInventory[i].item.category >= 0) {
                    r.Add(allInventory[i]);
                }
            }
            else {
                if (categoryFilter.Contains(allInventory[i].item.category)) {
                    r.Add(allInventory[i]);
                }
            }
        }
        return r;
    }
    

    public bool TransferInventoryContentsTo (Inventory otherInventory) {
        bool didAnyTransfer = false;
        for (int i = allInventory.Count -1 ; i >= 0; i--) {
            
            ItemBehavior item = allInventory[i].item;
            int count = allInventory[i].count;

            if (CanDropItem(item, out _, out _, false)) {
                if (otherInventory.CanStashItem(item)) {
                    DropItem(item, count, false, i);
                    otherInventory.StashItem(item, count, -1);
                    didAnyTransfer = true;
                }
            }
        }
        return didAnyTransfer;

    }
    
    public bool TransferItemTo (ItemBehavior item, int count, Inventory otherInventory) {
        int itemIndex;
        if (CanDropItem(item, out itemIndex, out _, false)) {
            if (otherInventory.CanStashItem(item)) {
                DropItem(item, count, false, itemIndex);
                otherInventory.StashItem(item, count, -1);
                return true;
            }
        }
        return false;
    }
        
        


    public bool CanStashItem (ItemBehavior item) {
        if (item.allowMultipleStashed) 
            return true;
        //check if it's already in inventory
        return !ItemIsInInventory(item, out _, out _);
    }
        
    public bool CanDropItem (ItemBehavior item, out int atIndex, out InventorySlot slot, bool checkInventory) {
        slot = null;
        atIndex = -1;
        if (item.permanentStash)
            return false;

        //check if it's already in inventory
        if (checkInventory)
            return ItemIsInInventory(item, out atIndex, out slot);
        
        return true;
    }

    bool ItemIsInInventory (ItemBehavior item, out int atIndex, out InventorySlot slot) {
        slot = null;
        atIndex = -1;
        for (int i = 0; i < allInventory.Count; i++) {
            if (allInventory[i].item == item) {
                atIndex = i;
                slot = allInventory[i];
                return true;
            }
        }
        return false;
    }

    //maybe switch these to interaction system (stash and trade will be secondary/terciary action....)
    // when used BY inventory
    public const int GRAB_ACTION = 0;
    public const int STASH_ACTION = 1;
    
    //when used ON inventory
    // public const int TRADE_ACTION = 2;

    public event System.Action<Inventory, ItemBehavior, int, int> onStash, onDrop;
    

    
[System.Serializable] public class InventorySlot {
    public InventorySlot(ItemBehavior item, int count) {
        this.item = item;
        this.count = count;
    }
    public ItemBehavior item;
    public int count;
}

public int favoritesMaxCount = 8;
[HideInInspector] public List<InventorySlot> allInventory = new List<InventorySlot>();
[HideInInspector] public List<InventorySlot> favorites = new List<InventorySlot>();


public bool FavoriteItem (InventorySlot slot) {
    if (favoriteAbleCategories.Contains(slot.item.category)) {
    // if (slot.item.canBeFavorite) {
        if (!favorites.Contains(slot)) {
            favorites.Add(slot);
        }
        else {
            favorites.Remove(slot);
        }
        return true;
    }
    return false;
}


        public Vector3 dropLocalPoint = new Vector3 (0,1,1);


        public void StashItem(ItemBehavior itemBehavior, int count, int equipSlot) {
            //check if it's already in inventory
            InventorySlot slotToUse = null;

            if (itemBehavior.keepOnStash) {
                for (int i = 0; i < allInventory.Count; i++) {
                    if (allInventory[i].item == itemBehavior) {
                        slotToUse = allInventory[i];
                        break;
                    }
                }
            }
            bool wasInInventory = slotToUse != null;

            if (wasInInventory) {
                if (!itemBehavior.allowMultipleStashed) {
                    return;// false;
                }
                slotToUse.count += count;
            }
            else {

                
                if (itemBehavior.keepOnStash) {
                    slotToUse = new InventorySlot(itemBehavior, count);
                    allInventory.Add(slotToUse);
                }
            }

            int newCount = slotToUse.count;

            for (int i = 0; i < itemBehavior.stashedItemBehaviors.Length; i++) {
                itemBehavior.stashedItemBehaviors[i].OnItemStashed(this, itemBehavior, count, equipSlot, equipSlot != -1);
            }

            if (onStash != null) {
                onStash(this, itemBehavior, count, newCount);
            }

            if (!wasInInventory) {
                if (slotToUse != null) {
                    if (favorites.Count < favoritesMaxCount) {
                        FavoriteItem(slotToUse);
                    }
                }
            }


        }

        public void StashItem(Item item, int equipSlot)
        {
            ItemBehavior itemBehavior = item.itemBehavior;

            if (CanStashItem(itemBehavior)) {

                StashItem(itemBehavior, itemBehavior.stackable ? item.itemCount : 1, equipSlot);

                //disable the scene item (frees it up for pool)
                item.gameObject.SetActive(false);
            }
        }


        public void ClearInventory () {
            for (int i = allInventory.Count -1; i >= 0; i--) {
                DropItem(allInventory[i].item, allInventory[i].count, false, i);
            }
        }
        public void AddInventory (List<InventorySlot> slots) {
            for (int i =0 ; i < slots.Count; i++) {
                StashItem(slots[i].item, slots[i].count, -1);
            }
        }

        public void DropItem (ItemBehavior itemBehavior, int count, bool getScene, int inventoryIndex) {
            if (itemBehavior.permanentStash) {
                return;// false;
            }

            if (inventoryIndex < 0) {
                //check if it's already in inventory
                for (int i = 0; i < allInventory.Count; i++) {
                    if (allInventory[i].item == itemBehavior) {
                        inventoryIndex = i;
                        break;
                    }
                }
            }
            
            InventorySlot slotInInventory = null;
            if (inventoryIndex >= 0) {
                slotInInventory = allInventory[inventoryIndex];
            }

            bool wasInInventory = slotInInventory != null;

            if (wasInInventory) {

                count = Mathf.Min(count, slotInInventory.count);

                slotInInventory.count -= count;
                if (slotInInventory.count <= 0) {
                    slotInInventory.count = 0;
                }

                int newCount = slotInInventory.count;

                bool hasModel = false;

                // if (newCount == 0) {
                //     if (ItemIsEquipped(-1, itemBehavior)) {
                //         UnequipItem(itemBehavior, getScene);
                //         hasModel= true;
                //     }
                // }
                
                // remove buffs
                for (int i = 0; i < itemBehavior.stashedItemBehaviors.Length; i++) {
                    itemBehavior.stashedItemBehaviors[i].OnItemDropped(this, itemBehavior, count);
                }


                if (slotInInventory.count == 0) {
                    allInventory.Remove(slotInInventory);
                }

                if (onDrop != null) {
                    onDrop(this, itemBehavior, count, newCount);
                }


                if (!hasModel) {

                    if (getScene) {

                        Item sceneItem = Item.GetSceneItem(itemBehavior);
                        sceneItem.transform.position = transform.TransformPoint(dropLocalPoint);
                        sceneItem.transform.rotation = Quaternion.Euler(UnityEngine.Random.Range(0,360), UnityEngine.Random.Range(0,360), UnityEngine.Random.Range(0,360));
                        sceneItem.gameObject.SetActive(true);
                    }
                }
            }
        }

     
    }
}
