using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

using RenderTools;

namespace CustomVegetation {

    static class TerrainEditorUtils {
        public static void CreateEditorAsset<T> (T asset, string path) where T : Object {
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
        public static void OpenWindow (EditorWindow window) {
            var size = 300;
            var hSize = size*.5f;
            
            window.position = new Rect(Screen.width*.5f-hSize, Screen.height*.5f+hSize, size, size);
        }
    }

    
    public class Terrain2TreeMap : ScriptableWizard
    {
        [MenuItem("GameObject/Custom Vegetation/Terrain to TreeMap")]
        static void CreateWizard()
        {
            var window = ScriptableWizard.DisplayWizard<Terrain2TreeMap>("Terrain to TreeMap", "Create");
            TerrainEditorUtils.OpenWindow(window);
        }

        public Terrain terrain;
        public DetailMap treeMap;

        void OnWizardCreate()
        {
            if (terrain == null) {
                return;
            }

            if (treeMap != null) {
                CopyFromTerrain(terrain, treeMap);
                EditorUtility.SetDirty(treeMap);
                return;
            }

            var path = EditorUtility.SaveFilePanelInProject("New Tree Map", terrain.name + "_TreeMap.asset", "asset", "");
            if (path.Length != 0)
            {
                DetailMap asset = ScriptableObject.CreateInstance<DetailMap>();
                
                CopyFromTerrain(terrain, asset);

                TerrainEditorUtils.CreateEditorAsset<DetailMap>(asset, path);
            }
        }

        static void CopyFromTerrain (Terrain terrain, DetailMap treeMap) {
            TerrainData data = terrain.terrainData;
            Vector3 terrainPos = terrain.transform.position;
            
            TreeInstance[] treeInstances = data.treeInstances;

            int l = treeInstances.Length;
            System.Array.Resize(ref treeMap.details, l);

            for (int i = 0; i < l; i++) {

                TreeInstance treeInstance = treeInstances[i];
                
                treeMap.details[i] = new DetailMap.Detail (
                    treeInstance.prototypeIndex,
                    Vector3.Scale(treeInstance.position, data.size) + terrainPos,
                  
                    // treeInstance.heightScale,
                    Vector3.one * treeInstance.heightScale,

                    Quaternion.Euler(0, Random.Range(0,360), 0)
                    // treeInstance.rotation * Mathf.Rad2Deg
                );

            }
        }
    }


    public class BillboardAsset2Mesh : ScriptableWizard
    {
        public BillboardAsset asset;

        [MenuItem("GameObject/Custom Vegetation/Billboard Asset To Mesh")]
        static void CreateWizard()
        {
            var window = ScriptableWizard.DisplayWizard<BillboardAsset2Mesh>("Billboard Asset To Mesh", "Create");
            TerrainEditorUtils.OpenWindow(window);
        }


        void SetTriangles (Mesh mesh) {
            ushort[] tris = asset.GetIndices();
            int[] triangles = new int[tris.Length];
            for (int i =0 ; i < tris.Length; i++) {
                triangles[i] = (int)tris[i];
            }
            mesh.triangles = triangles;
        }

        void OnWizardCreate()
        {

            if (asset == null) {
                return;
            }

            var path = EditorUtility.SaveFilePanelInProject("New Mesh", asset.name + "_BBMesh.asset", "asset", "");
            if (path.Length != 0)
            {
                Mesh newMesh = new Mesh();
            
                Vector2[] assetVerts = asset.GetVertices();
                Vector3[] meshVerts = new Vector3[assetVerts.Length];

                List<Vector2> texCoords = new List<Vector2>();
        
                for (int i =0 ; i < assetVerts.Length; i++) {
                    float roundedX = Mathf.Round(assetVerts[i].x);
                    float yVertRaw = assetVerts[i].y;

                    //maybe no rounding here
                    float xVert = (roundedX - 0.5f) * asset.width;
                    float yVert = yVertRaw * asset.height + asset.bottom;

                    meshVerts[i] = new Vector3(xVert, yVert, 0);
                    texCoords.Add(new Vector2(roundedX, yVertRaw));
                }
             
                
                newMesh.vertices = meshVerts;
                newMesh.SetUVs(0, texCoords);

                SetTriangles(newMesh); 
                newMesh.RecalculateBounds();
                TerrainEditorUtils.CreateEditorAsset<Mesh>(newMesh, path);
            }
        }
    }
}