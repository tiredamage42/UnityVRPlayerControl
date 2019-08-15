using UnityEngine;
using ActorSystem;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace InventorySystem {

    [System.Serializable] public class LevelledListItemArray : NeatArrayWrapper<LevelledList.ListItem> { }
    
    [CreateAssetMenu()] public class LevelledList : ScriptableObject
    {
        [System.Serializable] public class ListItem {
            public ItemBehavior item;
            public Vector2Int minMax = new Vector2Int(2, 5);
            [Range(0,1)] public float chanceForNone = .5f;
            [NeatArray] public GameValueConditionArray conditions;
        }

        [Header("Only One List Item Spawned")]
        public bool singleSpawn;
        [NeatArray] public LevelledListItemArray listItems;


        [Header("ListItems spawned 100% if no original ListItems spawned")]
        [NeatArray] public LevelledListItemArray fallBacks;

        // public LevelledList[] subLists;


        public List<Inventory.InventorySlot> SpawnItems (Dictionary<string, GameValue> selfValues, Dictionary<string, GameValue> suppliedValues) {

            List<Inventory.InventorySlot> spawnList = new List<Inventory.InventorySlot>();

            bool didSpawn = false;
            //CHECK FOR INJECTED RUNTIME LISTS ASSOCIATED WITH SUPPLIED LEVELLED LIST HERE
            
            LevelledList.ListItem[] listItems = this.listItems;

            for (int i =0 ; i < listItems.Length; i++) {
                LevelledList.ListItem listItem = listItems[i];
                if (Random.value > listItem.chanceForNone) {

                    int count = Random.Range(listItem.minMax.x, listItem.minMax.y+1);
                    if (count > 0) {

                        if (GameValueCondition.ConditionsMet (listItem.conditions, selfValues, suppliedValues)) {
                            spawnList.Add(new Inventory.InventorySlot(listItem.item, count));
                            didSpawn = true;
                            if (singleSpawn) {
                                break;
                            }    
                        }
                    }
                }
            }

            if (!didSpawn) {
                LevelledList.ListItem[] fallBacks = this.fallBacks;

                for (int i =0 ; i < fallBacks.Length; i++) {
                    LevelledList.ListItem listItem = fallBacks[i];

                    if (GameValueCondition.ConditionsMet (listItem.conditions, selfValues, suppliedValues)) {
                        spawnList.Add(new Inventory.InventorySlot(listItem.item, Random.Range(Mathf.Max(listItem.minMax.x, 1), listItem.minMax.y + 1)));
                    }
                }
            }


            return spawnList;
        }
        
    }



#if UNITY_EDITOR
    [CustomEditor(typeof(LevelledList))]
    [CanEditMultipleObjects]
    public class LevelledListEditor : Editor
    {   
        void OnEnable () {
            InventorySystemEditorUtils.UpdateItemSelector();
        }
    }
    
    [CustomPropertyDrawer(typeof(LevelledList.ListItem))] public class ListItemDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            float singleLineHeight = EditorGUIUtility.singleLineHeight;
            GUIContent noContent = GUIContent.none;
            EditorGUI.BeginProperty(position, label, property);

            int oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            float x = EditorTools.DrawIndent (oldIndent, position.x);

            InventorySystemEditorUtils.itemSelector.Draw(new Rect(x, position.y, 175, singleLineHeight), property.FindPropertyRelative("item"), GUIContent.none);
            x += 175;
            
            EditorGUI.LabelField(new Rect(x, position.y, 60, singleLineHeight), new GUIContent("Min Max:"));
            EditorGUI.PropertyField(new Rect(x + 60, position.y, 100, singleLineHeight), property.FindPropertyRelative("minMax"), GUIContent.none);
            x += 60 + 100;
            
            EditorGUI.LabelField(new Rect(x, position.y, 80, singleLineHeight), new GUIContent("Chance None:"));
            EditorGUI.PropertyField(new Rect(x + 80, position.y, 50, singleLineHeight), property.FindPropertyRelative("chanceForNone"), GUIContent.none);
            
            EditorGUI.indentLevel = oldIndent + 1;
            SerializedProperty conditionsProp = property.FindPropertyRelative("conditions");
            EditorGUI.PropertyField(new Rect(position.x, position.y + singleLineHeight, position.width, (EditorGUI.GetPropertyHeight(conditionsProp, true))), conditionsProp, new GUIContent("Conditions"));
            EditorGUI.indentLevel = oldIndent;
            
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
