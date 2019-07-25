using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MeshUtils{

    // [RequireComponent(typeof(MeshFilter))]
    // [RequireComponent(typeof(MeshRenderer))]
    public class MeshCombiner : MonoBehaviour
    {
        public Mesh BuildMeshFromCombines (List<CombineInstance> combines, Transform copyDestination, int meshIndex, Material combineMaterial) {
            Mesh newMesh = new Mesh();
            newMesh.CombineMeshes(combines.ToArray());

            Transform newDestination = new GameObject("Combined " + meshIndex.ToString()).transform;
            newDestination.SetParent(copyDestination);
            newDestination.localPosition = Vector3.zero;
            newDestination.localRotation = Quaternion.identity;
            newDestination.localScale = Vector3.one;

            newDestination.gameObject.AddComponent<MeshFilter>().sharedMesh = newMesh;
            newDestination.gameObject.AddComponent<MeshRenderer>().sharedMaterial = combineMaterial;

            return newMesh;
        }

        public List<Mesh> CombineChildrenMeshes (Transform copyDestination) {
            List<Mesh> meshesBuilt = new List<Mesh>();

            MeshFilter myFilter = GetComponent<MeshFilter>();
            MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();


            List<CombineInstance> combines = new List<CombineInstance>();


            List<MeshRenderer> renderersToDisable = new List<MeshRenderer>();
            
            // CombineInstance[] combine = new CombineInstance[meshFilters.Length - 1];
            
            Material combineMaterial = null;

            int verts = 0;
            for (int i = 0; i < meshFilters.Length; i++) {
                if (meshFilters[i] == myFilter)
                    continue;


                MeshRenderer mr = meshFilters[i].GetComponent<MeshRenderer>();
                if (combineMaterial == null) {
                    combineMaterial = mr.sharedMaterial;
                }
                else {
                    if (mr.sharedMaterial != combineMaterial) {
                        Debug.LogWarning("Can Only Combine with same material, " + mr.name + " has material " + mr.sharedMaterial.name);
                        return null;
                    }
                }
                Mesh m = meshFilters[i].sharedMesh;

                int c = m.vertexCount;
                if (verts + c > MeshUtils.maxVerts) {

                    meshesBuilt.Add(BuildMeshFromCombines(combines, copyDestination, meshesBuilt.Count, combineMaterial));
                    verts = 0;
                    combines.Clear();

                    // Debug.LogWarning("Mesh vertex count is above " + MeshUtils.maxVerts);
                    
                    
                    // return null;
                }

                CombineInstance ci = new CombineInstance();
                ci.mesh = m;
                ci.transform = meshFilters[i].transform.localToWorldMatrix;
                combines.Add(ci);

                verts += c;

                renderersToDisable.Add(meshFilters[i].GetComponent<MeshRenderer>());

                // meshFilters[i].GetComponent<MeshRenderer>().enabled = false;
            }

            if (verts > 0) {
                meshesBuilt.Add(BuildMeshFromCombines(combines, copyDestination, meshesBuilt.Count, combineMaterial));
            }

            for (int i = 0; i < renderersToDisable.Count; i++) {
                if (renderersToDisable[i] != null) {
                    renderersToDisable[i].enabled = false;
                }
            }

            return meshesBuilt;
        }
    }
}
