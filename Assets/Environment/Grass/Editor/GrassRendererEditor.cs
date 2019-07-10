using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CustomVegetation {
    
    [CustomEditor(typeof(GrassRenderer))]
    public class GrassRendererEditor : Editor
    {
        GrassRenderer grassRenderer;
        void OnEnable () {
            grassRenderer = target as GrassRenderer;
        }

        public override void OnInspectorGUI () {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Editor Grass Building:");
            EditorGUILayout.Space();
            squareSize = EditorGUILayout.FloatField("Grass Cell Size", squareSize);
            density = EditorGUILayout.FloatField("Density", density);

            int used = VertsUsed();
            bool tooMany = used > GrassMeshBuilder.maxVerts;
            EditorGUILayout.HelpBox("Vertex count per cell:\n" + used + " / " + GrassMeshBuilder.maxVerts, tooMany ? MessageType.Error : MessageType.None);
            
            if (!tooMany) {
                if (grassRenderer.grassDef != null) {
                    if (GUILayout.Button("Visualize Cell In Scene")) {
                        Terrain terrain = Terrain.activeTerrain;
                        if (terrain != null) {
                            DebugDensity(terrain, grassRenderer.grassDef);
                        }
                        else {
                            Debug.LogWarning("Cant visualize grass cell, no terrain in scene");
                        }
                    }
                }
            }
            
            if (grassRenderer.grassMap != null && grassRenderer.grassDef != null) {

                EditorGUILayout.Space();
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Build From Terrain:");
                Terrain terrain = (Terrain)EditorGUILayout.ObjectField("Terrain", null, typeof(Terrain), true);
                terrainDensityOffset = EditorGUILayout.IntField("Detail Density Offset", terrainDensityOffset);
                
                if (terrain != null) {
                    CopyFromTerrainWithDensity(terrain, grassRenderer.grassMap, grassRenderer.grassDef);
                }
            }
        }

        int VertsUsed () {
            int d = (int)(squareSize / density) + (squareSize % density != 0 ? 1 : 0);
            return d * d;
        }

        void DebugDensity (Terrain terrain, GrassDefenition grassDef) {
            GrassMap grassMap = null;
            if (grassRenderer.grassMap != null && grassRenderer.grassMap.name == "VisualizeGrassCell") {
                grassMap = grassRenderer.grassMap;
            }
            
            if (grassMap == null) {
                grassMap = ScriptableObject.CreateInstance<GrassMap>();
                grassMap.name = "VisualizeGrassCell";
                grassRenderer.grassMap = grassMap;
            }
            
            grassMap.InitializeMap(squareSize);
            GrassDefenitionEditor.BakeGrassDef(grassDef, 4096*2);
                
            TerrainData data = terrain.terrainData;
            
            GrassMeshBuilder builder = new GrassMeshBuilder(VertsUsed());

            Vector2 midPoint = new Vector3(grassRenderer.transform.position.x, grassRenderer.transform.position.z);

            Vector3 startPosition = grassRenderer.transform.position + new Vector3(squareSize * -.5f, 0, squareSize * -.5f);
            
            for (float x = 0; x < squareSize; x+=density) {
                for (float y = 0; y < squareSize; y+=density) {

                    Vector3 pos = startPosition + new Vector3(x, 0, y);
                    pos.y = terrain.SampleHeight(pos);
                        
                    Vector3 norm = data.GetInterpolatedNormal(pos.x / data.size.x, pos.z / data.size.x);

                    if (builder.AddGrassInstance(grassDef.grassPrototypes[Random.Range(0, grassDef.grassPrototypes.Length)], pos, norm) != null) Debug.LogError("Max Verts...");
                }   
            }

            Mesh mesh = builder.BakeToMesh();

            if (mesh != null) grassMap.AddGrassBatch(midPoint, mesh);
            
            EditorUtility.SetDirty(grassRenderer);
            EditorUtility.SetDirty(grassMap);            
        }


        float density = .2f;
        float squareSize = 50;
        int terrainDensityOffset = -2;

        void CopyFromTerrainWithDensity (Terrain terrain, GrassMap grassMap, GrassDefenition grassDef) {
            int maxDensity = 14;

            grassMap.InitializeMap(squareSize);
            GrassDefenitionEditor.BakeGrassDef(grassDef, 4096*2);
            
            TerrainData data = terrain.terrainData;
            Vector3 terrainPos = terrain.transform.position;
            
            Dictionary<Vector2Int, GrassMeshBuilder> worldGrid2MeshInfo = new Dictionary<Vector2Int, GrassMeshBuilder>();

            float terrainRes = data.size.x;
            float detailRes = (float)data.detailWidth;
            float detailUnit = terrainRes / detailRes;
            
            DetailPrototype[] detailPrototypes = data.detailPrototypes;
            int prototypesCount = detailPrototypes.Length;

            int grassDefCount = grassDef.grassPrototypes.Length;

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
                                
                                int i = Random.Range(0, grassDefCount);
                                
                                Vector3 wPos = worldPos + new Vector3(x2, 0, y2);
                                wPos.y = terrain.SampleHeight(wPos);

                                Vector3 norm = data.GetInterpolatedNormal(wPos.x / terrainRes, wPos.z / terrainRes);

                                if (meshInfo.AddGrassInstance(grassDef.grassPrototypes[i], wPos, norm) != null) Debug.LogError("Max Verts reached");
                            }
                        }
                    }
                }
            }

            foreach (var k in worldGrid2MeshInfo.Keys) {
                Mesh mesh = worldGrid2MeshInfo[k].BakeToMesh();
                if (mesh != null) {
                    Vector3 cellMidPoint = EnvironmentTools.WorldGrid.GridCenterPosition(k, squareSize);
                    grassMap.AddGrassBatch(new Vector2(cellMidPoint.x, cellMidPoint.z), mesh);
                }
            }

            EditorUtility.SetDirty(grassRenderer);
            EditorUtility.SetDirty(grassMap);
            
        }
    }
}
