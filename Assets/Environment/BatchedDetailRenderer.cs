
using System.Collections.Generic;
using UnityEngine;

// using System.Collections;
// using UnityEngine.Rendering;
using EnvironmentTools;
using RenderTools;
namespace CustomVegetation {

    [ExecuteInEditMode]
    public class BatchedDetailRenderer : MonoBehaviour
    {
        public DetailDefenition[] allDefenitions;
        public DetailMap detailMap;

        public int gridDistace = 2;
        public bool castShadows = true;
        public bool receiveShadows = true;
        

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
        }




    Dictionary<Vector2Int, MeshRenderList> meshRenderLists = new Dictionary<Vector2Int, MeshRenderList>();

    void FlushRenderList () {
        meshRenderLists.Clear();
    }
    

    void UpdateLODsPerDetail (Vector2Int newCell) {
        if (detailMap == null) {
            return;
        }

        int defenitionsCount = allDefenitions.Length;
        
        Dictionary<Vector2Int, int> cellDistances = new Dictionary<Vector2Int, int>();

        for (int i = 0; i < detailMap.details.Length; i++) {

            DetailMap.Detail detail = detailMap.details[i];

            Vector2Int detailCell = WorldGrid.GetGrid(detail.worldPosition);
				
            int cellDistance;
            if (!cellDistances.TryGetValue(detailCell, out cellDistance)) {
                cellDistance = WorldGrid.GetDistance(detailCell, newCell);
                cellDistances[detailCell] = cellDistance;
            }

            DetailDefenition detailDef = allDefenitions[Mathf.Min(detail.defenitionIndex, defenitionsCount-1)];
            

            if (cellDistance <= gridDistace) {

                DetailDefenition.LODInfo lodInfo = detailDef.lods[0];
                Matrix4x4 transformMatrix = GetTransformMatrix(detail);
                Mesh mesh = lodInfo.mesh;
                Material[] materials = lodInfo.materials;


                Vector2Int meshInstanceID_material0ID = new Vector2Int(mesh.GetInstanceID(), materials[0].GetInstanceID());


                MeshRenderList renderList;
                if (!meshRenderLists.TryGetValue(meshInstanceID_material0ID, out renderList)) {
                    renderList = new MeshRenderList(mesh, materials);
                    meshRenderLists[meshInstanceID_material0ID] = renderList;
                }
                renderList.AddInstance(transformMatrix);
            }



        }
    }


    public Transform editorReferenceTransform;

    Matrix4x4 GetTransformMatrix(DetailMap.Detail detail){
        // if (!billBoard)
        //     return Matrix4x4.TRS(detail.worldPosition, Quaternion.Euler(0, detail.yRotation, 0), Vector3.one * detail.sizeScale);
        // return Matrix4x4.TRS(detail.worldPosition, Quaternion.identity, Vector3.one * detail.sizeScale);

        return Matrix4x4.TRS(detail.worldPosition, detail.rotation, detail.scale);
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
            if (editorReferenceTransform == null)
                return;


            UpdateLODsPerDetail(WorldGrid.GetGrid(editorReferenceTransform.position));
        }

        // for (int i = 0; i < lodsPerDetail.Length; i++) {

            // if (lodsPerDetail[i] == -1) {
            //     continue;
            // }

            // DetailMap.Detail detail = detailMap.details[i];
            
            // DetailDefenition detailDef = allDefenitions[Mathf.Min(detail.defenitionIndex, defenitionsCount-1)];
    
            // DetailDefenition.LODInfo lodInfo = detailDef.lods[Mathf.Min(lodsPerDetail[i], detailDef.lods.Length-1)];

            // Matrix4x4 transformMatrix = GetTransformMatrix(detail, lodInfo.isBillboard);
            // Mesh mesh = lodInfo.mesh;
            // Material[] materials = lodInfo.materials;


            // Vector2Int meshInstanceID_material0ID = new Vector2Int(mesh.GetInstanceID(), materials[0].GetInstanceID());

            // InstancedMeshRenderList renderList;
            // if (!meshRenderLists.TryGetValue(meshInstanceID_material0ID, out renderList)) {
            //     renderList = new InstancedMeshRenderList(mesh, materials, lodInfo.receiveShadows, lodInfo.castShadows);
            //     meshRenderLists[meshInstanceID_material0ID] = renderList;
            // }

            // renderList.AddInstance(transformMatrix);
        // }

        foreach (var k in meshRenderLists.Keys) {
            meshRenderLists[k].Render(castShadows, receiveShadows);
        }
            
    }



}
}


