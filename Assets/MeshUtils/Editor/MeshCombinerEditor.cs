using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MeshUtils {
    [CustomEditor(typeof(MeshCombiner))]
    public class MeshCombinerEditor : Editor
    {

        MeshCombiner mc;
        void OnEnable () {
            mc = target as MeshCombiner;
        }

        Transform copyDestination;

        string meshPath;
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            meshPath = EditorGUILayout.TextField("Mesh Path", meshPath);
            bool stringEmpty = string.IsNullOrEmpty(meshPath) || string.IsNullOrWhiteSpace(meshPath);
        
            GUI.enabled = !stringEmpty;

            copyDestination = (Transform)EditorGUILayout.ObjectField("Copy Destination Root", copyDestination, typeof(Transform), true);

            GUI.enabled = !stringEmpty && copyDestination != null;

            if (GUILayout.Button("Combine Children Meshes")) {
                List<Mesh> meshes = mc.CombineChildrenMeshes (copyDestination);
                if (meshes != null) {
                    for (int i = 0; i < meshes.Count; i++) {
                        // Debug.Log("saving mesh");
                        AssetDatabase.CreateAsset(meshes[i], "Assets/" + meshPath + i.ToString() + ".asset");
                    }
                    AssetDatabase.SaveAssets();
                }
            }

            GUI.enabled = true;
            if (stringEmpty){
                EditorGUILayout.HelpBox("Specify a mesh path...", MessageType.Warning);
            }
            if (copyDestination == null) {
                EditorGUILayout.HelpBox("Specify a copy destination in the scene...", MessageType.Warning);
            }
        }
        
    }
}
