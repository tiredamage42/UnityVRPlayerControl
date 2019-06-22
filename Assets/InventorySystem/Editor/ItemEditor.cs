﻿using System.Collections;
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
            TransformBehavior.SetValues(item.equipBehavior, setIndex, item.transform);
        }


        static int setIndex;
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if (Application.isPlaying) {
                EditorGUILayout.Space();

                GUILayout.Label("Playtime Editing:");

                setIndex = EditorGUILayout.IntField( "Equip Settings Index", setIndex);

                if (GUILayout.Button("Save Local Position And Rotation")) {
                    SaveLocalPositionAndRotation();
                    EditorUtility.SetDirty(item.equipBehavior);
                }
            }

        }
    }
}
