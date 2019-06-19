using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

namespace InventorySystem {

    [CustomEditor(typeof(Item))]
    public class ItemEditor : Editor
    {   
        Item item;
        void OnEnable () {
            item = target as Item;
        }

        void SaveLocalPositionAndRotation () {

            if (setIndex < item.equipBehavior.equipSettings.Length && setIndex >= 0) {
                item.equipBehavior.equipSettings[setIndex].position = item.transform.localPosition;
                item.equipBehavior.equipSettings[setIndex].rotation = item.transform.localRotation.eulerAngles;
            }
        }

        static int setIndex;
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if (Application.isPlaying) {
                EditorGUILayout.Space();

                GUILayout.Label("Playtime Editing:");

                setIndex = EditorGUILayout.IntField(setIndex, "Equip Settings Index");

                if (GUILayout.Button("Save Local Position And Rotation")) {
                    SaveLocalPositionAndRotation();
                    EditorUtility.SetDirty(item.equipBehavior);
                }
            }

        }
    }
}
