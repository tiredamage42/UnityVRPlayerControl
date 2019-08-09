using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/*
    recipe:


        Composition[] requires

        [SHOULD BE PERMANENT]
        actorbuffs[] // to give xp for instance

        Composition[] yields

        
 */


/*



SEPERATE STASHED ITEM TO HAVE 
    ON STASH 
    ON CONSUME
    ON UNSTASH

CALLBACKS

CAN SEPERATE BUFFS INTO OTHER COMPONENTS


CONSUME ON STASH OPTION FOR ITEM MAYBE


 */


namespace InventorySystem {
    [CreateAssetMenu(menuName="Inventory System/Item Behavior")]
    public class ItemBehavior : ScriptableObject
    {
        public string itemName;

        [TextArea]
        public string itemDescription = "No Description Available...";
        
        [Header("Set to false for utility items")]
        public bool keepOnStash = true;
        
        // -1 if set by where equipped from
                // else equip point index (overwritten if quick equipped)
                
        public bool allowMultipleStashed = true;


        [Header("Cant be dropped from inventory accidentally")]
        public bool permanentStash;

        public int category;

        // stash use logic needs to happen on scriptable objects
        // because runtime inventory slots only contain a refrence to the item behavior
        // (to prevent having multiple gameObjects of the same item in bloated inventories)
        public StashedItemBehavior[] stashedItemBehaviors;

        public float weight = 1;
        [Range(0,100)] public float value = 50;

        [DisplayedArray(new float[] {0,0,0,.25f}, false)] 
        public ItemCompositionArray composedOf;

        
        [Header("Scene Behavior")]
        public bool canQuickEquip = true;
        public Item[] scenePrefabVariations;

        [Header("'Armor Slot', -1 for anything that isnt wearable")]
        public int equipSlot = -1;

        [Header("Can one scene instance hold multiple of items ?")]
        public bool stackable;

        public InventoryEqupping.EquipType equipType = InventoryEqupping.EquipType.Normal;
        public bool hoverLockOnEquip = true;

        public TransformBehavior equipTransform;

        // public List<int> stashActions = new List<int> () {
        //     1
        // };
        // public List<int> equipActions = new List<int> () {
        //     0
        // };




        // [Header("Stashing")]
        // public Buffs onStashBuffs;

        // public StashUseBehavior stashUseBehavior;
        // public StashedItem stashedItemBehavior;

        

        // [Header("Equipping")]
        // public Buffs onEquipBuffs;

        
        /*
            TODO: add multiplier values
        */
        public bool OnConsume (Inventory inventory, int count, int slot)
        {
            bool hasBehaviors = stashedItemBehaviors.Length != 0;
            for (int i = 0; i < stashedItemBehaviors.Length; i++) {
                stashedItemBehaviors[i].OnItemConsumed(inventory, this, count, slot);
            }
            return hasBehaviors;
        }
    }

    [System.Serializable] public class ItemComposition {
        public ItemBehavior item;
        public int amount = 1;
    }

[System.Serializable] public class ItemCompositionArray : NeatArrayWrapper<ItemComposition> { }
        
    
    #if UNITY_EDITOR

[CustomPropertyDrawer(typeof(ItemComposition))]
public class GameValueConditionDrawer : PropertyDrawer
{
    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        float offset = -5;

        float x = position.x;
            
        var amountRect = new Rect(x, position.y, 175, EditorGUIUtility.singleLineHeight);
        x += 175-offset;

        InventorySystemEditorUtils.itemSelector.Draw(amountRect, property.FindPropertyRelative("item"), GUIContent.none);
        // EditorGUI.PropertyField(amountRect, property.FindPropertyRelative("item"), GUIContent.none);
        
        amountRect = new Rect(x, position.y, 50, EditorGUIUtility.singleLineHeight);
        x+= 50-offset;
        EditorGUI.LabelField(amountRect, "Amount:");

        amountRect = new Rect(x, position.y, 135, EditorGUIUtility.singleLineHeight);
        x+= 135-offset;
        EditorGUI.PropertyField(amountRect, property.FindPropertyRelative("amount"), GUIContent.none);



            
        EditorGUI.EndProperty();
    }


    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }
}
#endif


/*



items have modifiers that modify owner values


Set | Add | Multiply

Base | Max     

Variable Name

Value

isOneOff  
    (modifier cant be removed, and is permanent 
        i.e level up adds 100 to max health, 
        or health pack adds health but then is let go (so cant remove modifier)
    )

gameMessage 

 */

//  [System.Serializable] public class ActorValue {

//  }

//  [System.Serializable] public class Buff {

//  }


//     public class Buffs : ScriptableObject {

//     }
}
