using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Rendering;
using EnvironmentTools;

namespace CustomVegetation {




[ExecuteInEditMode]
public class TreeRenderer : MonoBehaviour
{
    public TreeDefenition[] allDefenitions;
    public TreeMap treeMap;
    public Terrain terrain;

    public bool copyTerrain;


    void OnEnable () {
        for (int i =0 ; i < allDefenitions.Length; i++) {
            allDefenitions[i].InitializeMaterials();
        }
        FlushRenderList();
        if (treeMap != null) {
            lodsPerTree = new int[treeMap.trees.Length];
            ResetLODs();
        }


    }

    		int[] lodsPerTree;

		void ResetLODs () {
			for (int i =0 ; i < lodsPerTree.Length; i++) {
				lodsPerTree[i] = -1;
			}
		}



    const int maxRender = 1023;
    const int maxStack = 8;
    public class MeshRenderList {
        public Mesh mesh;
        public Material[] materials;
        public Matrix4x4[][] instances;
        public int count;
        public MeshRenderList(Mesh mesh, Material[] materials) {
            this.mesh = mesh;
            this.materials = materials;
            instances = new Matrix4x4[maxStack][];
            for (int i =0 ; i< maxStack; i++) {
                instances[i] = new Matrix4x4[maxRender];
            }

            ResetList();
        }
        public void AddInstance (Matrix4x4 instance) {
            int stack = count/maxRender;

            instances[stack][count - (maxRender * stack)] = instance;
            // if (count < maxRender) {
                // instances[count] = instance;
                count++;
            // }

        }

        public void ResetList () {
            count = 0;
        }

        
    static void DrawMesh (Mesh mesh, Material[] materials, Matrix4x4[] instances, int count, 
        MaterialPropertyBlock properties=null,
        ShadowCastingMode shadowMode=ShadowCastingMode.On, bool receiveShadows=true, int layer=0, Camera camera=null, 
        LightProbeUsage lightProbes = LightProbeUsage.Off

    ) {

        for (int i = 0; i < materials.Length; i++) {

            Graphics.DrawMeshInstanced (
                mesh, i, materials[i], instances, count, properties, shadowMode, receiveShadows, layer, camera, lightProbes
            );
        }

    }



        public void RenderList () {

            int stack = 0;
            while (count > 0) {
                DrawMesh (mesh, materials, 
                
                instances[stack], 
                Mathf.Min(count, maxRender)
                    // MaterialPropertyBlock properties=null,
                    // ShadowCastingMode shadowMode=ShadowCastingMode.On, bool receiveShadows=true, int layer=0, Camera camera=null, 
                    // LightProbeUsage lightProbes = LightProbeUsage.Off

                );

                count -= maxRender;
                stack++;
            }


            ResetList();

        }
    }

    public int showLOD = 0;


    Dictionary<Vector2Int, MeshRenderList> meshRenderLists = new Dictionary<Vector2Int, MeshRenderList>();

    void FlushRenderList () {
        meshRenderLists.Clear();
    }
    public bool makeDebug;


    void MakeDebugTreeMap () {

        float resolution = 100;
        float spacing = 5;
        int res = (int)(resolution / spacing);
        int l = res * res;

        System.Array.Resize(ref treeMap.trees, l);





        for (int x = 0; x < res; x ++) {
        for (int y = 0; y < res; y ++) {
            Vector3 location = new Vector3((x*spacing) + Random.Range(-1f,1f), 0, (y*spacing) + Random.Range(-1f,1f));
            

            treeMap.trees[x + y * res] = new TreeMap.Tree(

                Random.Range(0, allDefenitions.Length), location, Random.Range(.75f, 1.25f), Random.Range(0, 360)

            );


        }    
        }

    }

    void UpdateLODsPerTree (Vector2Int newCell) {
        if (treeMap == null) {
            return;
        }
        int defenitionsCount = allDefenitions.Length;
        

        Dictionary<Vector2Int, int> cellDistances = new Dictionary<Vector2Int, int>();


        for (int i = 0; i < treeMap.trees.Length; i++) {

            TreeMap.Tree tree = treeMap.trees[i];

            Vector2Int treeCell = WorldGrid.GetGrid(tree.worldPosition);
				
            int cellDistance;
            if (!cellDistances.TryGetValue(treeCell, out cellDistance)) {
                cellDistances[treeCell] = WorldGrid.GetDistance(treeCell, newCell);
                cellDistance = cellDistances[treeCell];
            }



            int defenitionIndex = tree.defenitionIndex;
            TreeDefenition treeDef = allDefenitions[Mathf.Min(tree.defenitionIndex, defenitionsCount-1)];
            int lod = -1;
            for (int x = 0; x < treeDef.lods.Length; x++) {
                if (cellDistance <= treeDef.lods[x].gridDistance) {
                    lod = x;
                    break;
                }
            }
            lodsPerTree[i] = lod;
        }












		}


    public Transform editorReferenceTransform;
    // Update is called once per frame
    void Update()
    {
        if (treeMap == null)
            return;



        if (makeDebug) {

MakeDebugTreeMap();
            makeDebug = false;
        }

        if (copyTerrain) {
            if (terrain != null) {
                treeMap.CopyFromTerrain(terrain);
            }
            copyTerrain = false;
        }


        int defenitionsCount = allDefenitions.Length;
        if (defenitionsCount == 0)
            return;

        if (lodsPerTree == null) 
            return;

        if (!Application.isPlaying) {
            if (editorReferenceTransform == null)
                return;


            UpdateLODsPerTree(WorldGrid.GetGrid(editorReferenceTransform.position));
        }



        // Dictionary<int, MeshRenderList> meshInstanceLists = new Dictionary<int, MeshRenderList>();
        
        for (int i = 0; i < lodsPerTree.Length; i++) {

            if (lodsPerTree[i] == -1) {
                continue;
            }

        //     [System.Serializable] public class Tree {
		// 	public int defenitionIndex;
		// 	public Vector3 worldPosition;
		// 	public float sizeScale;
		// 	public float yRotation;
		// }
            TreeMap.Tree tree = treeMap.trees[i];
            
            int defenitionIndex = tree.defenitionIndex;
            TreeDefenition treeDef = allDefenitions[Mathf.Min(tree.defenitionIndex, defenitionsCount-1)];

    
            TreeDefenition.LODInfo lodInfo = treeDef.lods[Mathf.Min(lodsPerTree[i], treeDef.lods.Length-1)];
            // TreeDefenition.LODInfo lodInfo = treeDef.lods[lodsPerTree[i]];

            Matrix4x4 transformMatrix = tree.GetTransformMatrix(lodInfo.isBillboard);
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


        foreach (var k in meshRenderLists.Keys) {
            MeshRenderList renderList = meshRenderLists[k];
            renderList.RenderList();


            // DrawMesh (renderList.mesh, renderList.materials, renderList.instances, renderList.count
            //     // MaterialPropertyBlock properties=null,
            //     // ShadowCastingMode shadowMode=ShadowCastingMode.On, bool receiveShadows=true, int layer=0, Camera camera=null, 
            //     // LightProbeUsage lightProbes = LightProbeUsage.Off

            // );



            // renderList.ResetList();

        }
            

            
		
        
        
        // Matrix4x4[] matrixArray0 = new Matrix4x4[] {
        //     Matrix4x4.TRS(transform.position + transform.right * 10, Quaternion.identity, Vector3.one),
        //     Matrix4x4.TRS(transform.position + transform.right * -10, Quaternion.identity, Vector3.one),

        // };

        // Graphics.DrawMeshInstanced (
        //     GetComponent<MeshFilter>().sharedMesh,
        //     0,
        //     material,
        //     matrixArray0,
        //     matrixArray0.Length,
        //     null,
        //     ShadowCastingMode.On,
        //     true,
        //     0,
        //     null,
        //     LightProbeUsage.Off
        // );
      


        
    }



}
}

/*







	public class TreeCellRenderer : MonoBehaviour {
		public TreeMap treeMap;
		public TreeDefenition[] treeDefenitions;

		int[] lodsPerTree;

		void ResetLODs () {
			for (int i =0 ; i < lodsPerTree.Length; i++) {
				lodsPerTree[i] = -1;
			}
		}

		void Update () {
			if (treeMap != null) {
				if (lodsPerTree != null) {
					for (int i =0 ; i < lodsPerTree.Length; i++) {
						if (lodsPerTree[i] != -1) {



							TreeMap.Tree tree = treeMap.trees[i];
							TreeDefenition treeDef = treeDefenitions[tree.defenitionIndex];
							TreeDefenition.LODInfo lodInfo = treeDef.lods[lodsPerTree[i]];

							// Matrix4x4 treeMatrix = treeInstances[i].GetTransformMatrix();

							// InstancedRenderer.SubmitMesh(
							// 	lodInfo.mesh,
							// 	lodInfo.materials,
							// 	treeMatrix
							// );
						}
					}
				}
			}
		}

		void OnEnable () {
			
			if (treeMap != null) {
				lodsPerTree = new int[treeMap.trees.Length];
				ResetLODs();
			}

			// WorldGrid.onPlayerCellChange += OnPlayerCellChange;
		}
		void OnDisable () {
			// WorldGrid.onPlayerCellChange -= OnPlayerCellChange;
		}



		void UpdateLODsPerTree (Vector2Int newCell) {
			if (treeMap == null) {
				return;
			}

			Dictionary<Vector2Int, int> cellDistances = new Dictionary<Vector2Int, int>();
			for (int i = 0; i < treeMap.trees.Length; i++) {

				TreeMap.Tree tree = treeMap.trees[i];
				TreeDefenition treeDef = treeDefenitions[tree.defenitionIndex];
							

				Vector2Int treeCell = WorldGrid.GetGrid(tree.worldPosition);
				

				int cellDistance;
				if (!cellDistances.TryGetValue(treeCell, out cellDistance)) {
					cellDistances[treeCell] = WorldGrid.GetDistance(treeCell, newCell);
					cellDistance = cellDistances[treeCell];
				}

				int lod = -1;

				for (int x = 0; x < treeDef.lods.Length; x++) {
					if (cellDistance <= treeDef.lods[x].gridDistance) {
						lod = x;
						break;
					}
				}
				lodsPerTree[i] = lod;
				
				
			}

		}

		public void OnPlayerCellChange (Vector2Int newCell) {
			UpdateLODsPerTree(newCell);
		}
	}
 */
