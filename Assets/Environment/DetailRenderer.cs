
using System.Collections.Generic;
using UnityEngine;
using EnvironmentTools;

namespace RenderTools {

    [ExecuteInEditMode]
    public class DetailRenderer : MonoBehaviour
    {
        public DetailDefenition[] allDefenitions;
        public DetailMap detailMap;
        Dictionary<Vector2Int, InstancedMeshRenderList> meshRenderLists = new Dictionary<Vector2Int, InstancedMeshRenderList>();

        
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


        void OnEnable () {
            for (int i =0 ; i < allDefenitions.Length; i++) {
                InitializeMaterials(allDefenitions[i]);
            }

            FlushRenderList();

            if (Application.isPlaying) {
                WorldGrid.instance.onPlayerGridChange += UpdateLODsPerDetail;
				
            }
        }
        void OnDisable () {
            if (Application.isPlaying) {
                WorldGrid.instance.onPlayerGridChange -= UpdateLODsPerDetail;
				
            }
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
                    
                int cellDistance = WorldGrid.GetDistance(detail.worldPosition, cameraCell, worldGridSize);
                
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
        void Update()
        {
            if (detailMap == null)
                return;

            int defenitionsCount = allDefenitions.Length;
            if (defenitionsCount == 0)
                return;

            if (!Application.isPlaying) {
                Vector3 cameraPos = transform.position;
                UpdateLODsPerDetail(WorldGrid.GetGrid(cameraPos), cameraPos, WorldGrid.instance.cellSize);
            }

            foreach (var k in meshRenderLists.Keys) {
                meshRenderLists[k].Render();
            }       
        }
    }
}