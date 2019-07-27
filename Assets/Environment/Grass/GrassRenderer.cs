using UnityEngine;
using UnityEngine.Rendering;

using EnvironmentTools;

namespace Environment.Grass {

    [ExecuteInEditMode]
    public class GrassRenderer : GridHandler
    {
        [Header("Grass Wind")]
        public Vector2 windSpeed_range = new Vector2(1,1);
        public Vector2 windFrequency_range = new Vector2(1,1);
        public Vector3 windScale_min = new Vector3(2, 0, 1);
        public Vector3 windScale_max = new Vector3(2, 0, 1);

        [Header("Grass Rendering")]
        public Vector2 cameraFadeRange = new Vector2(25, 35);
        public int worldGridRenderDistance = 2;
        public GrassMap grassMap;
        bool[] renderMask;

        protected override float GetGridSize() {
            return WorldGrid.instance.gridSize;
        }
        protected override void OnPlayerGridChange(Vector2Int playerGrid, Vector3 playerPosition, float cellSize) {
            UpdateRenderMask(playerGrid, playerPosition, cellSize);
        }


        void SetWindAndRenderSettings () {
            Shader.SetGlobalVector("_PCGRASS_CAMERA_RANGE", new Vector4(cameraFadeRange.x, cameraFadeRange.y, 0, 0));
            Shader.SetGlobalVector("_PCGRASS_WIND_SPEED_FREQUENCY_RANGES", new Vector4(windSpeed_range.x, windSpeed_range.y, windFrequency_range.x, windFrequency_range.y));
            Shader.SetGlobalVector("_PCGRASS_WIND_SCALE_MIN", new Vector4(windScale_min.x, windScale_min.y, windScale_min.z, 0));
            Shader.SetGlobalVector("_PCGRASS_WIND_SCALE_MAX", new Vector4(windScale_max.x, windScale_max.y, windScale_max.z, 0));

        }


        protected override void Update()
        {
            base.Update();

            #if UNITY_EDITOR
            SetWindAndRenderSettings();
            #endif
            
            RenderGrassMap();            
        }

        static string normalShader = "Custom Environment/Grass/Grass";
        static string shadowShader = "Custom Environment/Grass/ShadowPass";

        
        [System.NonSerialized] Material grassMaterial, shadowMaterial;
        void BuildMaterialIfNull(ref Material material, string shader) {
            if (material == null) {
                material = new Material(Shader.Find(shader));
                material.hideFlags = HideFlags.HideAndDontSave;
            }
        }
        void BuildMaterialsIfNull () {
            BuildMaterialIfNull(ref grassMaterial, normalShader);
            BuildMaterialIfNull(ref shadowMaterial, shadowShader);
        }
            
        protected override void OnEnable () {
            base.OnEnable();

            BuildMaterialsIfNull();

            SetWindAndRenderSettings();
            
            if (CheckErrored())
                return;

            grassMaterial.SetTexture("_MainTex", grassMap.grassDef.atlasedTexture);
            grassMaterial.SetTexture("_BumpMap", grassMap.grassDef.atlasedNormal);
            shadowMaterial.SetTexture("_MainTex", grassMap.grassDef.atlasedTexture);
            
            CheckForRenderMaskInitialize();
        }
        
        bool CheckErrored () {
            if (grassMap == null) {
                Debug.LogWarning("Needs Grass Map");
                return true;
            }
            if (grassMap.grassDef == null) {
                Debug.LogWarning("Needs Grass Defenition");
                return true;
            }
            return false;
        }

        void CheckForRenderMaskInitialize () {
            if (CheckErrored())
                return;
            
        
            if (renderMask == null) {
                renderMask = new bool[0];
            }
            if (renderMask.Length != grassMap.batches.Count) {
                System.Array.Resize(ref renderMask, grassMap.batches.Count);
            }
        }



        float Abs (float a) {
            return a >= 0.0f ? a : -a;
        }

        void UpdateRenderMask (Vector2Int cameraCell, Vector3 cameraPos, float worldGridSize) {

            if (CheckErrored())
                return;
            
            CheckForRenderMaskInitialize();

            float distCheck = (grassMap.batchSize * .5f) + (worldGridSize * worldGridRenderDistance); 
            
            for (int i = 0; i < grassMap.batches.Count; i++) {
                GrassMap.Batch batch = grassMap.batches[i];
                //check if player position is within batch or grid distance away from the edges
                renderMask[i] = Abs(cameraPos.x - batch.centerPosition.x) <= distCheck && Abs(cameraPos.z - batch.centerPosition.y) <= distCheck;            
            }
        }


        static readonly Matrix4x4 identityMatrix = Matrix4x4.TRS (Vector3.zero, Quaternion.identity, Vector3.one);

        static void DrawMesh (Mesh mesh, Material material, ShadowCastingMode shadowMode, bool receiveShadows) {
            // mesh, matrix, material, layer, camera, subMesh, materialPropBlock, shadowMode, receiveShadows
            Graphics.DrawMesh(mesh, identityMatrix, material, 0, null, 0, null, shadowMode, receiveShadows);
        }

        void RenderGrassMap () {
            if (CheckErrored())
                return;
            
            CheckForRenderMaskInitialize();

            for (int i = 0; i < grassMap.batches.Count; i++) {
                if (renderMask[i]) {
                    RenderGrassMesh (grassMap.batches[i].mesh);
                }
            }
        }
        
        void RenderGrassMesh (Mesh mesh) {
            if (mesh == null)
                return;

            /*
                need to draw in two passes:

                1 - the actual waving grass without shadow casting 
                    (so the wind movement doesnt cause flickering shadows)

                2 - the shadow casting grass without movement
            */
            DrawMesh (mesh, grassMaterial, ShadowCastingMode.Off, true);
            DrawMesh (mesh, shadowMaterial, ShadowCastingMode.ShadowsOnly, false);
        }

    }
}
