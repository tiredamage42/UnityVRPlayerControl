using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/*

SEPERATE STASHED ITEM TO HAVE 
    ON STASH 
    ON CONSUME
    ON UNSTASH

CALLBACKS
*/


namespace Game.InventorySystem {
    [CreateAssetMenu(menuName="Inventory System/Item Behavior")]
    public class ItemBehavior : ScriptableObject
    {
        public string itemName;

        [TextArea]
        public string itemDescription = "No Description Available...";
        
        public int category;

        // stash use logic needs to happen on scriptable objects
        // because runtime inventory slots only contain a refrence to the item behavior
        // (to prevent having multiple gameObjects of the same item in bloated inventories)
        public StashedItemBehavior[] stashedItemBehaviors;

        public float weight = 1;
        [Range(0,100)] public float value = 50;

        [NeatArray] public ItemCompositionArray composedOf;

        
        [Header("Scene Behavior")]
        public bool canQuickEquip = true;
        public SceneItem[] scenePrefabVariations;


        // -1 if set by where equipped from
        // else equip point index (overwritten if quick equipped)
        [Header("'Armor Slot', -1 for anything that isnt wearable")]
        public int equipSlot = -1;

        [Header("Can one scene instance hold multiple of items ?")]
        public bool stackable;

        public InventoryEquipper.EquipType equipType = InventoryEquipper.EquipType.Normal;
        public TransformBehavior equipTransform;

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
        [NeatArray] public ActorValueConditionArray conditions;

        public ItemComposition () {
            amount = 1;
        }
    }
        
    [System.Serializable] public class ItemCompositionArray : NeatArrayWrapper<ItemComposition> { }
        
    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ItemComposition))] public class ItemCompositionDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float singleLine = EditorGUIUtility.singleLineHeight;
            EditorGUI.BeginProperty(position, label, property);

            float offset = -5;

            int oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            float x = EditorTools.DrawIndent (oldIndent, position.x);
                
            InventorySystemEditor.itemSelector.Draw(new Rect(x, position.y, 175, singleLine), property.FindPropertyRelative("item"), GUIContent.none);
            x += 175-offset;
            
            EditorGUI.LabelField(new Rect(x, position.y, 50, singleLine), "Amount:");
            x+= 50-offset;
            
            EditorGUI.PropertyField(new Rect(x, position.y, 135, singleLine), property.FindPropertyRelative("amount"), GUIContent.none);
            


            EditorGUI.indentLevel = oldIndent + 1;
            SerializedProperty conditionsProp = property.FindPropertyRelative("conditions");
            EditorGUI.PropertyField(new Rect(position.x, position.y + singleLine, position.width, (EditorGUI.GetPropertyHeight(conditionsProp, true))), conditionsProp, new GUIContent("Conditions"));
            EditorGUI.indentLevel = oldIndent;
         
                
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + (EditorGUI.GetPropertyHeight(property.FindPropertyRelative("conditions"), true));
        }
    }
#endif

}
