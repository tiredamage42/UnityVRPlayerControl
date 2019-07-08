using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace CustomVegetation {

    
    
/*
Quaternion.FromToRotation(Vector3.up, normal);

myQuaternion *= Quaternion.Euler(Vector3.up * 20); //along y axis maybe quaternion's up instead of wolrd
 */
    
    static class TerrainEditorUtils {
        public static void CreateEditorAsset<T> (T asset, string path) where T : Object {
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            // Selection.activeObject = asset;
        }
        public static void OpenWindow (EditorWindow window) {
            var size = 300;
            var hSize = size*.5f;
            
            window.position = new Rect(Screen.width*.5f-hSize, Screen.height*.5f+hSize, size, size);
        }
    }




    public class Terrain2GrassMap : ScriptableWizard
    {
        [MenuItem("GameObject/Custom Vegetation/Terrain to GrassMap")]
        static void CreateWizard()
        {
            var window = ScriptableWizard.DisplayWizard<Terrain2GrassMap>("Terrain to GrassMap", "Create");
            TerrainEditorUtils.OpenWindow(window);
        }

        public Terrain terrain;
        // public DetailMap detailMap;
        public BatchedDetailRenderer detailRenderer;


        void OnWizardCreate()
        {
            if (terrain == null) {
                return;
            }

            // if (detailMap != null) {
            //     CopyFromTerrain(terrain, detailMap);
            //     EditorUtility.SetDirty(detailMap);
            //     return;
            // }

            // var path = EditorUtility.SaveFilePanelInProject("New Grass Detail Map", terrain.name + "_GrassMap.asset", "asset", "");
            // var directory = EditorUtility.SaveFolderPanel("New Grass Detail Map", "", "");
            // if (directory.StartsWith(Application.dataPath)) {
            //     directory =  "Assets" + directory.Substring(Application.dataPath.Length);
            // }
            
            // if (directory.Length != 0)
            {
                // if (!directory.EndsWith("/")) {
                //     directory += "/";
                // }
                detailRenderer.allDefenitions = new DetailDefenition[0];

                DetailMap asset = ScriptableObject.CreateInstance<DetailMap>();
                
                CopyFromTerrain(terrain, asset, 
                // directory, 
                "",
                ref detailRenderer.allDefenitions);

                // TerrainEditorUtils.CreateEditorAsset<DetailMap>(asset, directory + terrain.name + "_DetailMap.asset");
                // AssetDatabase.CreateAsset(asset, directory + terrain.name + "_DetailMap");
            
                detailRenderer.detailMap = asset;
                EditorUtility.SetDirty(detailRenderer);
            }
        }


        class MeshInfo {

            int vertCount;
            Vector3[] verts, normals;
            Vector2[] uvs;
            // public List<Vector3> verts = new List<Vector3>();
            // public List<Vector2> uvs = new List<Vector2>();
            // public List<Vector3> normals = new List<Vector3>();

            Vector2Int grid;
            Material material;
            Vector3 gridCenterPosition;

            int bakedCount;

            Vector3 ModifyVertex (Vector3 vertex, Vector3 worldPosition, Quaternion rotation, Vector3 scale) {
                vertex = Vector3.Scale(vertex, scale);
                vertex = (worldPosition - gridCenterPosition) + vertex;
                vertex = rotation * vertex;
                return vertex;
            }

            public void BakeIfAny (string directory, ref DetailDefenition[] allDefenitions, DetailMap detailMap) {
                if (vertCount > 0) {
                    // Debug.Log("baking " + grid + " material: " + material.name + " :: END");
                    BakeMeshInfo ( directory, ref allDefenitions, detailMap);
                }
            }


            public void AddMeshInstanceToBatch (Vector3[] instanceVerts, Vector2[] instanceUVs, Vector3 worldPosition, Quaternion rotation, Vector3 scale, string directory, ref DetailDefenition[] allDefenitions, DetailMap detailMap) {
                
                int count = instanceVerts.Length;

                if (vertCount + count > maxVerts) {
                    // Debug.Log("baking " + grid + " material: " + material.name + " :: too many verts");
                    BakeMeshInfo ( directory, ref allDefenitions, detailMap);
                }

                for (int i = 0; i < count; i++) {
                    
                    // verts.Add(ModifyVertex(instanceVerts[i], worldPosition, rotation, scale));
                    // uvs.Add(instanceUVs[i]);
                    // normals.Add(Vector3.up);

                    verts[vertCount] = ModifyVertex(instanceVerts[i], worldPosition, rotation, scale);
                    uvs[vertCount] = instanceUVs[i];
                    normals[vertCount] = Vector3.up;


                    vertCount++;
                }
            }



            void ResetMeshInfo () {
                bakedCount++;
                vertCount = 0;
                System.Array.Resize(ref verts, maxVerts);
                System.Array.Resize(ref uvs, maxVerts);
                System.Array.Resize(ref normals, maxVerts);
                
                // verts.Clear();
                // uvs.Clear();
                // normals.Clear();
            }

            public MeshInfo (Vector2Int grid, Material material) {
                this.grid = grid;
                this.gridCenterPosition = EnvironmentTools.WorldGrid.GridCenterPosition(grid);
                this.material = material;

                verts = new Vector3[maxVerts];
                uvs = new Vector2[maxVerts];
                normals = new Vector3[maxVerts];
                

            }

            Mesh CreateMeshAsset (string directory, string gridString) {
                Mesh mesh = new Mesh();

                System.Array.Resize(ref verts, vertCount);
                System.Array.Resize(ref uvs, vertCount);
                System.Array.Resize(ref normals, vertCount);

                mesh.vertices = verts;
                mesh.uv = uvs;
                mesh.normals = normals;
                

                // mesh.SetVertices(verts);
                // mesh.SetUVs(0, uvs);
                // mesh.SetNormals(normals);
                mesh.RecalculateBounds();
                mesh.RecalculateTangents();


                // AssetDatabase.CreateAsset(mesh, directory + "bakedMesh_"+ gridString + ".mesh");
            

                // TerrainEditorUtils.CreateEditorAsset<Mesh>(
                //     mesh, directory + "bakedMesh_"+ gridString + ".mesh"
                // );
                return mesh;
            }
            DetailDefenition CreateDetailDefenition(Mesh mesh, string directory, string gridString) {
                DetailDefenition detailDefenition = ScriptableObject.CreateInstance<DetailDefenition>();
                
                detailDefenition.lods = new DetailDefenition.LODInfo[] { new DetailDefenition.LODInfo() };

                DetailDefenition.LODInfo lODInfo = detailDefenition.lods[0];

                lODInfo.materials = new Material[] { material };
                lODInfo.mesh = mesh;



                // AssetDatabase.CreateAsset(detailDefenition, directory + "bakedMeshDef_"+ gridString + ".asset");
            

                // TerrainEditorUtils.CreateEditorAsset<DetailDefenition>(
                //     detailDefenition, 
                //     directory + "bakedMeshDef_"+ gridString + ".asset"
                // );
                return detailDefenition;
            }

            void AddToDetailMap(DetailMap detailMap, int defIndex) {
                if (detailMap.details == null) {
                    detailMap.details = new DetailMap.Detail[1];
                }
                else {
                    System.Array.Resize(ref detailMap.details, detailMap.details.Length+1);
                }

                detailMap.details[detailMap.details.Length - 1] = new DetailMap.Detail(
                    defIndex, gridCenterPosition, Vector3.one, Quaternion.identity
                );
            }
                
            void BakeMeshInfo (string directory, ref DetailDefenition[] allDefenitions, DetailMap detailMap) {
                
                string gridString = "x" + grid.x.ToString() + "y" + grid.y.ToString() + "_" + bakedCount;
                
                //bake the mesh
                Mesh mesh = CreateMeshAsset(directory, gridString);
                
                //create a detail defenition for it
                DetailDefenition detailDefenition = CreateDetailDefenition(mesh, directory, gridString);

                //add it to the master list
                System.Array.Resize(ref allDefenitions, allDefenitions.Length+1);
                
                int defIndex = allDefenitions.Length-1;
                
                allDefenitions[defIndex] = detailDefenition;


                //add it to the detail map
                AddToDetailMap(detailMap, defIndex);
                
                ResetMeshInfo();
            }
        }


        const int maxVerts = 65534;

        

        // [Header("Null if texture detail")]
        // public Mesh[] detailMeshesPerPrototype;
        // public Material[] materialsPerPrototype;



            
        void CopyFromTerrain (Terrain terrain, DetailMap detailMap, string directory, ref DetailDefenition[] allDefenitions) {
            TerrainData data = terrain.terrainData;
            Vector3 terrainPos = terrain.transform.position;
            Dictionary<Vector3Int, MeshInfo> worldGrid2MeshInfo = new Dictionary<Vector3Int, MeshInfo>();


            float terrainRes = data.size.x;
            float detailRes = (float)data.detailWidth;
            float detailUnit = terrainRes / detailRes;
            
            

            DetailPrototype[] detailPrototypes = data.detailPrototypes;
            int prototypesCount = detailPrototypes.Length;

            // List<DetailMap.Detail> allDetails = new List<DetailMap.Detail>();

            // int usedPrototypes = 0;
            for (int i = 0; i < prototypesCount; i++) {
                DetailPrototype prototype = detailPrototypes[i];

                if (prototype.prototype != null) {

                    Mesh instanceMesh = prototype.prototype.GetComponent<MeshFilter>().sharedMesh;// detailMeshesPerPrototype[i];
                    Vector3[] instanceVerts = instanceMesh.vertices;
                    Vector2[] instanceUVs = instanceMesh.uv;

                    Material instanceMaterial = prototype.prototype.GetComponent<Renderer>().sharedMaterial;// materialsPerPrototype[i];
                    int materialID = instanceMaterial.GetInstanceID();


                    int[,] dataMap = data.GetDetailLayer(0, 0, data.detailWidth, data.detailHeight, i);

                    for (int y = 0; y < data.detailHeight; y++) {
                        //xy switched in detail map
                        float worldX = y * detailUnit;

                        for (int x = 0; x < data.detailWidth; x++) {
                            float worldZ = x * detailUnit;

                            
                            
                            int detailDensity = dataMap[x,y];
                            for (int d = 0; d < detailDensity; d++) {
                                Vector3 worldPosition = terrainPos + new Vector3(worldX + ((Random.value) * (detailUnit)), 0, worldZ + ((Random.value) * (detailUnit)));
                                worldPosition.y = terrain.SampleHeight(worldPosition);
                                
                                float xzScale = Random.Range(prototype.minWidth, prototype.maxWidth);
                                float yScale = Random.Range(prototype.minHeight, prototype.maxHeight);

                                Vector3 scale = new Vector3(xzScale, yScale, xzScale);
                                    
                                Vector3 terrainNormal = Vector3.up;
                                Quaternion detailRotation = Quaternion.FromToRotation(Vector3.up, terrainNormal);
                                //along y axis maybe quaternion's up instead of wolrd
                                detailRotation *= Quaternion.Euler(Vector3.up * Random.Range(0,360)); 

                                Vector2Int grid = EnvironmentTools.WorldGrid.GetGrid(worldPosition);

                                Vector3Int key = new Vector3Int(grid.x, grid.y, materialID);

                                MeshInfo meshInfo;

                                if (!worldGrid2MeshInfo.TryGetValue(key, out meshInfo)) {
                                    meshInfo = new MeshInfo(grid, instanceMaterial);
                                    worldGrid2MeshInfo[key] = meshInfo;
                                }

                                meshInfo.AddMeshInstanceToBatch(
                                    instanceVerts, instanceUVs, worldPosition, detailRotation, scale, directory, ref allDefenitions, detailMap
                                );
                                                                        
                                }

            
                        }
                    }




                    // usedPrototypes++;




                }

            }

            // detailMap.details = allDetails.ToArray();

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
            var window = ScriptableWizard.DisplayWizard<Terrain2TreeMap>("Billboard Asset To Mesh", "Create");
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

            var path = EditorUtility.SaveFilePanelInProject("New Mesh", asset.name + "_BBMesh.mesh", "mesh", "");
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