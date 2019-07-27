using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Environment.DetailsAndBillboards {

    [CustomEditor(typeof(DetailRenderer))]
    public class DetailRendererEditor : Editor
    {
        DetailRenderer renderer;
        void OnEnable () {
            renderer = target as DetailRenderer;
        }

        DetailDefenition defenitionToBake;
        
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            // EditorGUILayout.Space();

            // EditorGUILayout.LabelField("Billboard Baking :: ");
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            defenitionToBake = (DetailDefenition)EditorGUILayout.ObjectField(defenitionToBake, typeof(DetailDefenition), false);
            GUI.enabled = defenitionToBake != null;
            
            if (GUILayout.Button("Create Billboard Map Mesh", EditorStyles.miniButton)) {
                var path = EditorUtility.SaveFilePanelInProject("Billboard Map Mesh", defenitionToBake.name + "_BBMap.asset", "asset", "");
                if (path.Length != 0)
                {
                    Mesh mesh = BakeBillboards(defenitionToBake);    
                    if (mesh != null) {
                        AssetDatabase.CreateAsset(mesh, path);//"Assets/" + assetName + ".asset");
                        AssetDatabase.SaveAssets();
                        EditorUtility.FocusProjectWindow();
                        Selection.activeObject = mesh;
                    }                
                }
                defenitionToBake = null;
            }
            
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            // if (defenitionToBake != null) {

            // }
        }

        public Mesh BakeBillboards (DetailDefenition detail) {
            if (renderer.detailMap == null) {
                Debug.LogWarning("Renderer has no detail map to bake billboards");
                return null;
            }

            int bbIndex = -1;
            for (int i = 0; i < detail.lods.Length; i++) {
                if (detail.lods[i].isBillboard) {
                    bbIndex = i;
                    break;
                }
            }
            if (bbIndex == -1) {
                Debug.Log(detail.name + " does not have any billboards to bake");
                return null;
            }

            BillboardAsset billboardAsset = detail.lods[bbIndex].billboardAsset;

            if (billboardAsset == null) {
                Debug.Log(detail.name + " billboard lod doesnt have billboard asset... need use case for later");
            }

            float billboardBaseWidth = billboardAsset.width;
            float billboardBaseHeight = billboardAsset.height;
            float billboardBaseBottom = billboardAsset.bottom;

            // Debug.Log("Billboard info: width " + billboardBaseWidth + " / height " + billboardBaseHeight + " / bottom " + billboardBaseBottom);

            Vector4[] bbTexcoords = billboardAsset.GetImageTexCoords();

            // vertex = pos
            // texcoord0 = billboardUVs
            // texcoord1 = width, height, bottom, scale
            
            List<int> indicies = new List<int>();        
            List<Vector3> vertexPositions = new List<Vector3>();
            List<List<Vector4>> uvs = new List<List<Vector4>>(2);
            for (int i = 0; i < 2; i++) {
                uvs.Add(new List<Vector4>(0));
            }

            int instances = 0;
            
            for (int i = 0; i < renderer.detailMap.details.Length; i++) {
                DetailMap.Detail d = renderer.detailMap.details[i];

                int defIndex = Mathf.Min(d.defenitionIndex, renderer.allDefenitions.Length-1);
                if (renderer.allDefenitions[defIndex] != detail) {
                    continue;
                }
                
                Vector3 worldPos = d.worldPosition;
                Vector3 scale = d.scale;

                vertexPositions.Add(worldPos);

                uvs[0].Add(new Vector4(billboardBaseWidth, billboardBaseHeight, billboardBaseBottom, scale.x));
                uvs[1].Add(bbTexcoords[Random.Range(0, bbTexcoords.Length)]);

                indicies.Add(instances);
                instances++;

                if (instances >= MeshUtils.MeshUtils.maxVerts) {
                    Debug.LogWarning("too many trees for mesh...");
                    return null;
                }
            }

            if (instances <= 0)
                return null;

            Mesh mesh = new Mesh ();
            mesh.SetVertices(vertexPositions);
            mesh.SetIndices(indicies.ToArray(), MeshTopology.Points, 0);
            for (int i = 0; i < uvs.Count; i++) {
                mesh.SetUVs(i, uvs[i]);
            }

            mesh.RecalculateBounds();
            
            Debug.Log("made Billboard map mesh with " + instances + " vertices");
            return mesh;
        }
    }
}
