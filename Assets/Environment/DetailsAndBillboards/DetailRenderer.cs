
using System.Collections.Generic;
using UnityEngine;
using EnvironmentTools;

namespace RenderTools {

    [ExecuteInEditMode]
    public class DetailRenderer : GridHandler
    {
        public DetailDefenition[] allDefenitions;
        public DetailMap detailMap;
        Dictionary<Vector2Int, InstancedMeshRenderList> meshRenderLists = new Dictionary<Vector2Int, InstancedMeshRenderList>();

        protected override float GetGridSize() {
			return WorldGrid.instance.gridSize;
		}

        protected override void OnPlayerGridChange(Vector2Int playerGrid, Vector3 playerPosition, float cellSize) {
            UpdateLODsPerDetail (playerGrid, playerPosition, cellSize);
        
        }

        
        void InitializeMaterials (DetailDefenition.LODInfo lodInfo) {
            if (lodInfo.billboardAsset != null) {
                for (int i = 0; i < lodInfo.materials.Length; i++) {
                    lodInfo.materials[i].SetVectorArray("_BillboardSliceCoords", lodInfo.billboardAsset.GetImageTexCoords());
                    lodInfo.materials[i].SetFloat("_BillboardSlices", lodInfo.billboardAsset.imageCount);
                }
            }
        }
        void InitializeMaterials (DetailDefenition detailDef) {
            for (int i = 0; i < detailDef.lods.Length; i++) {
                InitializeMaterials(detailDef.lods[i]);
            }
        }


        protected override void OnEnable () {
            for (int i =0 ; i < allDefenitions.Length; i++) {
                InitializeMaterials(allDefenitions[i]);
            }
            SetCameraRangeForBillboardMaterials();
            FlushRenderList();

            base.OnEnable();

            // if (Application.isPlaying) {
            //     WorldGrid.instance.onPlayerGridChange += UpdateLODsPerDetail;
				
            // }
        }
        void OnDisable () {
            // if (Application.isPlaying) {
            //     WorldGrid.instance.onPlayerGridChange -= UpdateLODsPerDetail;
				
            // }
        }


        void FlushRenderList () {
            meshRenderLists.Clear();
        }

        void UpdateLODsPerDetail (Vector2Int cameraCell, Vector3 cameraPos, float worldGridSize) {
            if (detailMap == null) {
                return;
            }

            foreach (var k in meshRenderLists.Keys) {
                meshRenderLists[k].ResetList();
            }

            int defenitionsCount = allDefenitions.Length;
            
            for (int i = 0; i < detailMap.details.Length; i++) {

                DetailMap.Detail detail = detailMap.details[i];
                    
                int cellDistance = GetDistance(detail.worldPosition, cameraCell, worldGridSize);
                
                DetailDefenition detailDef = allDefenitions[Mathf.Min(detail.defenitionIndex, defenitionsCount-1)];
                
                int lod = -1;
                for (int x = 0; x < detailDef.lods.Length; x++) {
                    if (cellDistance <= detailDef.lods[x].gridDistance) {
                        lod = x;
                        break;
                    }
                }
                if (lod != -1) {
                    DetailDefenition.LODInfo lodInfo = detailDef.lods[lod];

                    if (lodInfo.isBillboard)
                        continue;

                    Matrix4x4 transformMatrix = GetTransformMatrix(detail, lodInfo.isBillboard);
                    Mesh mesh = lodInfo.mesh;
                    Material[] materials = lodInfo.materials;

                    Vector2Int meshInstanceID_material0ID = new Vector2Int(mesh.GetInstanceID(), materials[0].GetInstanceID());

                    InstancedMeshRenderList renderList;
                    if (!meshRenderLists.TryGetValue(meshInstanceID_material0ID, out renderList)) {
                        renderList = new InstancedMeshRenderList(mesh, materials, lodInfo.receiveShadows, lodInfo.castShadows);
                        meshRenderLists[meshInstanceID_material0ID] = renderList;
                    }

                    renderList.AddInstance(transformMatrix);
                }
            }
        }

        Matrix4x4 GetTransformMatrix(DetailMap.Detail detail, bool billBoard) {
            return Matrix4x4.TRS(detail.worldPosition, billBoard ? Quaternion.identity : detail.rotation, detail.scale);
        }


        // Update is called once per frame
        protected override void Update()
        {
            base.Update();
            
            if (detailMap == null)
                return;

            int defenitionsCount = allDefenitions.Length;
            if (defenitionsCount == 0)
                return;

            // if (!Application.isPlaying) {
            //     Vector3 cameraPos = transform.position;
            //     UpdateLODsPerDetail(WorldGrid.GetGrid(cameraPos), cameraPos, WorldGrid.instance.cellSize);
            // }

            foreach (var k in meshRenderLists.Keys) {
                meshRenderLists[k].Render();
            }      

            RenderBillboardMaps(); 
        }


        public Mesh[] billboardMaps;
        public Material[] billboardMapMaterials;

        public Vector2 billboardFadeRange = new Vector2(10, 20);

        static readonly Matrix4x4 identityMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);

        void RenderBillboardMaps () {
            if (billboardMaps == null || billboardMapMaterials == null)
                return;
            for (int i = 0; i < billboardMaps.Length; i++) {
                if (i < billboardMapMaterials.Length) {
                    Graphics.DrawMesh(billboardMaps[i], identityMatrix, billboardMapMaterials[i], 0, null, 0, null, true, true, true);
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


    }
}