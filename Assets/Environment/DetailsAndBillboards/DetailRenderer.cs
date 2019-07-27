
using System.Collections.Generic;
using UnityEngine;
using EnvironmentTools;

namespace Environment.DetailsAndBillboards {

    [ExecuteInEditMode]
    public class DetailRenderer : GridHandler
    {
        public DetailMap detailMap;
        [Space]
        // [Header("Corresponds with Terrain Tree Prototypes (If rendering map exported from Unity Terrain)")]
        [Tooltip("Corresponds with Terrain Tree Prototypes\n(If rendering map exported from Unity Terrain)")]
        public DetailDefenition[] allDefenitions;
        Dictionary<Vector2Int, InstancedMeshRenderList> meshRenderLists = new Dictionary<Vector2Int, InstancedMeshRenderList>();
        
        protected override float GetGridSize() {
			return WorldGrid.instance.gridSize;
		}
        protected override void OnPlayerGridChange(Vector2Int playerGrid, Vector3 playerPosition, float cellSize) {
            UpdateRenderLists (playerGrid, cellSize);
        }
    
        // void InitializeMaterials (DetailDefenition.LODInfo lodInfo) {
        //     if (lodInfo.billboardAsset != null) {
        //         for (int i = 0; i < lodInfo.materials.Length; i++) {
        //             lodInfo.materials[i].SetVectorArray("_BillboardSliceCoords", lodInfo.billboardAsset.GetImageTexCoords());
        //             lodInfo.materials[i].SetFloat("_BillboardSlices", lodInfo.billboardAsset.imageCount);
        //         }
        //     }
        // }

        // void InitializeMaterials (DetailDefenition detailDef) {
        //     for (int i = 0; i < detailDef.lods.Length; i++) {
        //         InitializeMaterials(detailDef.lods[i]);
        //     }
        // }


        protected override void OnEnable () {
            // for (int i =0 ; i < allDefenitions.Length; i++) {
            //     InitializeMaterials(allDefenitions[i]);
            // }
            SetCameraRangeForBillboardMaterials();
            meshRenderLists.Clear();
            base.OnEnable();
        }
        
        /*
            Update the render lists
            calculate LODs / Culling
        */
        void UpdateRenderLists (Vector2Int cameraCell, float cellSize) {
            if (detailMap == null) {
                return;
            }

            //reset the mesh render lists
            foreach (var k in meshRenderLists.Keys) {
                meshRenderLists[k].ResetList();
            }

            int lastDefenitionIndex = allDefenitions.Length;
            
            for (int i = 0; i < detailMap.details.Length; i++) {

                DetailMap.Detail detail = detailMap.details[i];
                    
                // get the detail defenition (if the detail on the map has detail index above our count, just use the last one)
                int detailDefenitionIndex = Mathf.Min(detail.defenitionIndex, lastDefenitionIndex);

                DetailDefenition detailDef = allDefenitions[detailDefenitionIndex];
                
                int cellDistance = GetDistance(detail.worldPosition, cameraCell, cellSize);
                
                //calculate the lod level this detail will be rendered at (depending on the cell distance from the cell the camera is in)
                int lod = -1;
                for (int x = 0; x < detailDef.lods.Length; x++) {
                    if (cellDistance <= detailDef.lods[x].gridDistance) {
                        
                        // dont render billboards (should be baked and rendered below with geometry shader)
                        if (!detailDef.lods[x].isBillboard) {
                            lod = x;
                            break;
                        }
                    }
                }

                if (lod != -1) {
                    DetailDefenition.LODInfo lodInfo = detailDef.lods[lod];
                    
                    Mesh mesh = lodInfo.mesh;
                    Material[] materials = lodInfo.materials;


                    Vector2Int meshInstanceID_material0ID = new Vector2Int(mesh.GetInstanceID(), materials[0].GetInstanceID());

                    //check if we have a list going for this mesh/material combo
                    //if not make one
                    InstancedMeshRenderList renderList;
                    if (!meshRenderLists.TryGetValue(meshInstanceID_material0ID, out renderList)) {
                        renderList = new InstancedMeshRenderList(mesh, materials, lodInfo.receiveShadows, lodInfo.castShadows);
                        meshRenderLists[meshInstanceID_material0ID] = renderList;
                    }

                    //calculate instance matrix
                    Matrix4x4 transformMatrix = Matrix4x4.TRS(detail.worldPosition, lodInfo.isBillboard ? Quaternion.identity : detail.rotation, detail.scale);
                    renderList.AddInstance(transformMatrix);
                }
            }
        }


        void RenderDetails () {
            foreach (var k in meshRenderLists.Keys) {
                meshRenderLists[k].Render();
            }      
        }


        [Header("Billboard Maps:")]
        [Space]
        public Mesh[] billboardMaps;
        public Material[] billboardMapMaterials;

        [Header("Billboard Fade In Distance")]
        public Vector2 billboardFadeRange = new Vector2(10, 20);

        static readonly Matrix4x4 identityMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);

        void RenderBillboardMaps () {
            if (billboardMaps == null || billboardMapMaterials == null)
                return;
            for (int i = 0; i < billboardMaps.Length; i++) {
                if (i < billboardMapMaterials.Length) {
                    
                    // TODO: Draw in 2 passes like grass, 
                    // actual shadow casting needs to billboard towards light direction, not camera position
                    Graphics.DrawMesh(
                        billboardMaps[i], 
                        identityMatrix, 
                        billboardMapMaterials[i], 0, null, 0, null,
                        false, // cast shadows
                        true, // receive shadows
                        true);
                }
            }

            #if UNITY_EDITOR
            SetCameraRangeForBillboardMaterials();
            #endif
        }

        void SetCameraRangeForBillboardMaterials () {
            for (int i = 0; i < billboardMapMaterials.Length; i++) {
                billboardMapMaterials[i].SetVector("_CameraRange", billboardFadeRange);
            }
        }


        protected override void Update()
        {
            base.Update();
            
            if (detailMap == null)
                return;

            if (allDefenitions.Length == 0)
                return;

            RenderDetails();
            RenderBillboardMaps(); 
        }
    }
}