

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
namespace InventorySystem {
    public static class InventorySystemEditorUtils 
    {
        public static AssetSelector<ItemBehavior> itemSelector;

        public static void UpdateItemSelector () {
            if (itemSelector == null)
                itemSelector = new AssetSelector<ItemBehavior>(o => "[" + o.category + "] " + o.name, o => o.category);
            else 
                itemSelector.UpdateAssetReferences();
        }
    }

    

    [CustomEditor(typeof(Item))]
    [CanEditMultipleObjects]
    public class ItemEditor : Editor
    {   
        Item item;
        void OnEnable () {
            item = target as Item;
        }

        void SaveLocalPositionAndRotation () {
            TransformBehavior.SetValues(item.itemBehavior.equipTransform, setIndex, item.transform);
        }


        static int setIndex;
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            // if (Application.isPlaying) {
                EditorGUILayout.Space();

                GUILayout.Label("Playtime Editing:");

                setIndex = EditorGUILayout.IntField( "Equip Settings Index", setIndex);

                if (GUILayout.Button("Save Local Position And Rotation")) {
                    SaveLocalPositionAndRotation();
                    EditorUtility.SetDirty(item.itemBehavior.equipTransform);
                }
            // }

        }
    }



    [CustomEditor(typeof(ItemBehavior))]
    [CanEditMultipleObjects]
    public class ItemBehaviorEditor : Editor
    {   
        void OnEnable () {
            InventorySystemEditorUtils.UpdateItemSelector();
        }
    }

    [CustomEditor(typeof(CraftingRecipeBehavior))]
    [CanEditMultipleObjects]
    public class CraftingRecipeBehaviorEditor : Editor
    {   
        void OnEnable () {
            InventorySystemEditorUtils.UpdateItemSelector();
        }
    }
    
}

#endif