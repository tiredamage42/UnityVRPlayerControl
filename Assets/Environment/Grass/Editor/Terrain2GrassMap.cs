using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Environment.Grass {
    
    public class Terrain2GrassMap : ScriptableWizard {

        [MenuItem("GameObject/Environment/Terrain 2 Grass Map")]
        static void OpenWindow () {
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
            ScriptableWizard.DisplayWizard<Terrain2GrassMap>("Terrain 2 Grass Map", "Create").terrain = terrain;
        }


        Terrain terrain;
        GrassDefenition grassDefenition;
            
        float density = .2f;
        float squareSize = 50;
        int terrainDensityOffset = -2;
        const int maxSpawnTries = 1000;

        void OnGUI () {
            if (GrassRendererEditor.DrawDensityAndSizeOptions(ref squareSize, ref density)) {
                grassDefenition = (GrassDefenition)EditorGUILayout.ObjectField("Grass Defenition", null, typeof(GrassDefenition), false);
                terrainDensityOffset = EditorGUILayout.IntField("Detail Density Offset", terrainDensityOffset);

            }
            else {
                GUI.enabled = false;
            }

            bool shouldClose = false;
            if (GUILayout.Button("Export To Grass Map")) {
                ExportToGrassMap();
                shouldClose = true;
            }

            GUI.enabled = true;
            
            if (shouldClose)
                Close();

        }
     
        void ExportToGrassMap () {
            if (grassDefenition == null)
            {
                Debug.LogError("Need a grass definition object to export terrain grass to grass map");
                return;
            }

            string directory = EditorUtility.SaveFolderPanel("Export To Grass Map", "", "");//, "Terrain", "obj");
            if (directory.Length != 0) {
                directory = directory.Substring(Application.dataPath.Length-6);
            
                if (!directory.EndsWith("/"))
                    directory += "/";


                GrassMap grassMap = ScriptableObject.CreateInstance<GrassMap>();
                grassMap.grassDef = grassDefenition;
                grassMap.batchSize = squareSize;

                ExportTerrainGrass (directory, terrain, grassMap, squareSize, density, terrainDensityOffset);



                AssetDatabase.CreateAsset(grassMap, directory + "GrassMap_" + terrain.name + ".asset");



                AssetDatabase.SaveAssets();
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = grassMap;

            }
        }


        static int GetRandomGrassPrototype (GrassDefenition grassDef) {
            int grassDefCount = grassDef.grassPrototypes.Length;
            int chosen = -1;

            int tries = 0;
            while (chosen == -1 && tries <= maxSpawnTries) {
                int randomChoice = Random.Range(0, grassDefCount);
                if (Random.value <= grassDef.grassPrototypes[randomChoice].rarity) {
                    chosen = randomChoice;
                    break;
                }
                tries++;
            }
            return chosen;       
        }




        static void ExportTerrainGrass (string directory, Terrain terrain, GrassMap grassMap, float squareSize, float density, float terrainDensityOffset) {
            int maxDensity = 14;

            TerrainData data = terrain.terrainData;
            Vector3 terrainPos = terrain.transform.position;
            
            Dictionary<Vector2Int, GrassMeshBuilder> worldGrid2MeshInfo = new Dictionary<Vector2Int, GrassMeshBuilder>();

            float terrainRes = data.size.x;
            float detailRes = (float)data.detailWidth;
            float detailUnit = terrainRes / detailRes;
            
            DetailPrototype[] detailPrototypes = data.detailPrototypes;
            int prototypesCount = detailPrototypes.Length;

            int grassDefCount = grassMap.grassDef.grassPrototypes.Length;

            int[][,] detailMaps = new int [prototypesCount][,];

            for (int i = 0; i < prototypesCount; i++) {
                detailMaps[i] = data.GetDetailLayer(0, 0, data.detailWidth, data.detailHeight, i);
            }

            for (int y = 0; y < data.detailHeight; y++) {
                //xy switched in detail map
                float worldX = y * detailUnit;

                for (int x = 0; x < data.detailWidth; x++) {
                    float worldZ = x * detailUnit;

                    Vector3 worldPos = terrainPos + new Vector3(worldX, 0, worldZ);
                    Vector2Int grid = EnvironmentTools.WorldGrid.GetGrid(worldPos, squareSize);

                    GrassMeshBuilder meshInfo;
                    if (!worldGrid2MeshInfo.TryGetValue(grid, out meshInfo)) {
                        meshInfo = new GrassMeshBuilder();
                        worldGrid2MeshInfo[grid] = meshInfo;
                    }

                    int maxDensityAtGrid = 0;
                    for (int i = 0; i < prototypesCount; i++) {
                        int detailDensity = detailMaps[i][x,y];
                        if (detailDensity > maxDensityAtGrid) {
                            maxDensityAtGrid = detailDensity;
                        }
                    }

                    if (maxDensityAtGrid > 0) {
                  
                        for (float x2 = 0; x2 < detailUnit; x2 += density) {
                            for (float y2 = 0; y2 < detailUnit; y2 += density) {
                                
                                if (Random.Range(0, (maxDensity+1)-(terrainDensityOffset)) >= maxDensityAtGrid)
                                    continue;
                                
                                int i = GetRandomGrassPrototype(grassMap.grassDef);// Random.Range(0, grassDefCount);
                                if (i != -1) {

                                    Vector3 wPos = worldPos + new Vector3(x2, 0, y2);
                                    wPos.y = terrain.SampleHeight(wPos);

                                    Vector3 norm = data.GetInterpolatedNormal(wPos.x / terrainRes, wPos.z / terrainRes);

                                    if (meshInfo.AddGrassInstance(grassMap.grassDef.grassPrototypes[i], wPos, norm) != null) Debug.LogError("Max Verts reached");
                                }
                            }
                        }
                    }
                }
            }

            int e = 0;

            foreach (var k in worldGrid2MeshInfo.Keys) {
                Mesh mesh = worldGrid2MeshInfo[k].BakeToMesh();
                if (mesh != null) {

                    AssetDatabase.CreateAsset(mesh, directory + "zBatchMesh" + e + ".asset");
                    e++;
                    Vector3 cellMidPoint = EnvironmentTools.WorldGrid.GridCenterPosition(k, squareSize);
                    grassMap.batches.Add(new GrassMap.Batch (new Vector2(cellMidPoint.x, cellMidPoint.z), mesh));
                    
                
                }
            }
        }
    }
}

