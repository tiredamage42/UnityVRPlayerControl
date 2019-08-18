

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
namespace Game.InventorySystem {
    public static class InventorySystemEditor 
    {
        static AssetSelector<ItemBehavior> _itemSelector;
        public static AssetSelector<ItemBehavior> itemSelector {
            get {
                if (_itemSelector == null) {
                    Debug.Log("Buildign asset selector for items");
                    _itemSelector = new AssetSelector<ItemBehavior>(o => "[" + o.category + "] " + o.name, o => o.category);
                }
                return _itemSelector;
            }
        }
    }

    

    [CustomEditor(typeof(SceneItem))]
    [CanEditMultipleObjects]
    public class ItemEditor : Editor
    {   
        SceneItem item;
        void OnEnable () {
            item = target as SceneItem;
        }

        void SaveLocalPositionAndRotation () {
            TransformBehavior.SetValues(item.itemBehavior.equipTransform, setIndex, item.transform);
        }


        static int setIndex;
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            EditorGUILayout.Space();

            GUILayout.Label("Playtime Editing:");

            setIndex = EditorGUILayout.IntField( "Equip Settings Index", setIndex);

            if (GUILayout.Button("Save Local Position And Rotation")) {
                SaveLocalPositionAndRotation();
                EditorUtility.SetDirty(item.itemBehavior.equipTransform);
            }
        }
    }
}

#endif