using UnityEditor;
using UnityEngine;
using System.Collections.Generic;


namespace Environment.DetailsAndBillboards {

    static class DetailsAndBillboardsUtils {
        public static void CreateEditorAsset<T> (T asset, string path) where T : Object {
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }

        [MenuItem("GameObject/Environment/Terrain Details 2 Detail Map")]
        static void ExportTerrainDetails() {
            Terrain terrain = null;
            if (Selection.activeGameObject != null) {
                terrain = Selection.activeGameObject.GetComponent<Terrain>();
            }

            if (terrain == null) {
                Debug.LogWarning("Couldnt find selected Unity Terrain, searching for active Terrain...");
                terrain = Terrain.activeTerrain;
            }

            if (terrain == null) {
                Debug.LogError("Couldnt Find any Terrains, cant export detail map");
                return;
            }

            var path = EditorUtility.SaveFilePanelInProject("New Detail Map", terrain.name + "_DetailMap.asset", "asset", "");
            if (path.Length != 0)
            {
                DetailMap detailMap = ScriptableObject.CreateInstance<DetailMap>();

                TerrainData data = terrain.terrainData;
                Vector3 terrainPos = terrain.transform.position;
                
                TreeInstance[] treeInstances = data.treeInstances;

                int l = treeInstances.Length;
                System.Array.Resize(ref detailMap.details, l);

                for (int i = 0; i < l; i++) {
                    TreeInstance tree = treeInstances[i];
                    
                    detailMap.details[i] = new DetailMap.Detail (
                        tree.prototypeIndex,
                        Vector3.Scale(tree.position, data.size) + terrainPos,
                        Vector3.one * tree.heightScale,
                        Quaternion.Euler(0, Random.Range(0,360), 0)
                    );
                }
                CreateEditorAsset<DetailMap>(detailMap, path);
            }
        }
        [MenuItem("GameObject/Environment/Billboard Asset 2 Mesh")]
        static void ConvertBillboardAssetToMesh()
        {
            BillboardAsset billboardAsset = null;
            if (Selection.activeObject != null) {
                billboardAsset = (BillboardAsset)Selection.activeObject;
            }

            if (billboardAsset == null) {
                Debug.LogError("No Billboard Asset Selected");
                return;
            }

            var path = EditorUtility.SaveFilePanelInProject("New Billboard Asset Mesh", billboardAsset.name + "_BBMesh.asset", "asset", "");
            if (path.Length != 0)
            {
                Mesh newMesh = new Mesh();
            
                Vector2[] assetVerts = billboardAsset.GetVertices();
                Vector3[] meshVerts = new Vector3[assetVerts.Length];

                List<Vector2> texCoords = new List<Vector2>();
        
                for (int i =0 ; i < assetVerts.Length; i++) {
                    float roundedX = Mathf.Round(assetVerts[i].x);
                    float yVertRaw = assetVerts[i].y;

                    //maybe no rounding here
                    float xVert = (roundedX - 0.5f) * billboardAsset.width;
                    float yVert = yVertRaw * billboardAsset.height + billboardAsset.bottom;

                    meshVerts[i] = new Vector3(xVert, yVert, 0);
                    texCoords.Add(new Vector2(roundedX, yVertRaw));
                }
             
                newMesh.vertices = meshVerts;
                newMesh.SetUVs(0, texCoords);

                ushort[] tris = billboardAsset.GetIndices();
                int[] triangles = new int[tris.Length];
                for (int i =0 ; i < tris.Length; i++) {
                    triangles[i] = (int)tris[i];
                }
                newMesh.triangles = triangles;

                newMesh.RecalculateBounds();
                CreateEditorAsset<Mesh>(newMesh, path);
            }
        }
    }
}