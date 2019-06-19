using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SimpleUI{
    [CustomEditor(typeof(UIPage))]
    public class UIPageEditor : Editor
    {
        UIPage page;

        void OnEnable () {
            page = target as UIPage;
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            if (GUILayout.Button("Add UI Button")) {
                page.AddNewButton("New Button");
                // SceneView.RepaintAll(); 

                
            }
            // EditorUtility.SetDirty(target);
        }
        
    }
}
