using UnityEngine;
using UnityEditor;

namespace Environment.Grass {
    
    [CustomEditor(typeof(GrassRenderer))]
    public class GrassRendererEditor : Editor
    {
        GrassRenderer grassRenderer;
        void OnEnable () {
            grassRenderer = target as GrassRenderer;
        }


        public static bool DrawDensityAndSizeOptions (ref float squareSize, ref float density) {
            squareSize = EditorGUILayout.FloatField("Grass Cell Size", squareSize);
            density = EditorGUILayout.FloatField("Density", density);

            int used = VertsUsed(squareSize, density);
            bool tooMany = used > MeshUtils.MeshUtils.maxVerts;
            EditorGUILayout.HelpBox("Vertex count per cell:\n" + used + " / " + MeshUtils.MeshUtils.maxVerts, tooMany ? MessageType.Error : MessageType.None);

            return !tooMany;
        }
            


        public override void OnInspectorGUI () {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Editor Grass Building:");
            EditorGUILayout.Space();

            if (DrawDensityAndSizeOptions(ref squareSize, ref density)) {

                    if (GUILayout.Button("Visualize Cell In Scene")) {
                        DebugDensity(Terrain.activeTerrain);
                    }
            }
                   
        }

        static int VertsUsed (float squareSize, float density) {
            int d = (int)(squareSize / density) + (squareSize % density != 0 ? 1 : 0);
            return d * d;
        }

        void DebugDensity (Terrain terrain){
            Vector3 refPos = grassRenderer.transform.position;

            GrassMap grassMap = null;
            if (grassRenderer.grassMap != null) {
                if (grassRenderer.grassMap.name == "VisualizeGrassCell") {
                    grassMap = grassRenderer.grassMap;
                }
                else {
                    grassRenderer.grassMap = null;
                }
            }
            
            if (grassMap == null) {
                grassMap = ScriptableObject.CreateInstance<GrassMap>();
                grassMap.name = "VisualizeGrassCell";

                GrassDefenition grassDef = ScriptableObject.CreateInstance<GrassDefenition>();
                grassDef.grassPrototypes = new GrassDefenition.GrassPrototype[] {
                    new GrassDefenition.GrassPrototype()
                };
                grassMap.grassDef = grassDef;

                grassRenderer.grassMap = grassMap;
            }

            
            grassMap.batchSize = squareSize;
            grassMap.batches.Clear();

                
            GrassMeshBuilder builder = new GrassMeshBuilder(VertsUsed(squareSize, density));

            Vector2 midPoint = new Vector3(refPos.x, refPos.z);

            Vector3 startPosition = refPos + new Vector3(squareSize * -.5f, 0, squareSize * -.5f);
            
            for (float x = 0; x < squareSize; x+=density) {
                for (float y = 0; y < squareSize; y+=density) {

                    Vector3 pos = startPosition + new Vector3(x, refPos.y, y);
                    Vector3 norm = Vector3.up;
                    
                    if (terrain != null) {
                        pos.y = terrain.SampleHeight(pos);
                        norm = terrain.terrainData.GetInterpolatedNormal(pos.x / terrain.terrainData.size.x, pos.z / terrain.terrainData.size.x);
                    }
                    
                    // int i = GetRandomGrassPrototype(grassMap.grassDef);
                                
                    if (builder.AddGrassInstance(grassMap.grassDef.grassPrototypes[0], pos, norm) != null) 
                        Debug.LogError("Max Verts...");
                }   
            }

            Mesh mesh = builder.BakeToMesh();

            if (mesh != null) 
                grassMap.batches.Add(new GrassMap.Batch (midPoint, mesh));
                
            EditorUtility.SetDirty(grassRenderer);
            EditorUtility.SetDirty(grassMap);            
        }


        float density = .2f;
        float squareSize = 50;
        
    }

}
