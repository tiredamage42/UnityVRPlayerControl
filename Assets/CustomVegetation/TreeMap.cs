using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomVegetation {

    [CreateAssetMenu()]
	public class TreeMap : ScriptableObject {
		[System.Serializable] public struct Tree {
			public int defenitionIndex;
			public Vector3 worldPosition;
			public float sizeScale, yRotation;
            
            public Tree (int defenitionIndex, Vector3 worldPosition, float sizeScale, float yRotation) {
                this.defenitionIndex = defenitionIndex;
                this.worldPosition = worldPosition;
                this.sizeScale = sizeScale;
                this.yRotation = yRotation;
            }

            public Matrix4x4 GetTransformMatrix(bool billBoard) {
                if (!billBoard)
                    return Matrix4x4.TRS(worldPosition, Quaternion.Euler(0,yRotation,0), Vector3.one * sizeScale);
                return Matrix4x4.TRS(worldPosition, Quaternion.identity, Vector3.one * sizeScale);
            }
		}

		[HideInInspector] public Tree[] trees;






        public void CopyFromTerrain (Terrain terrain) {
            TerrainData data = terrain.terrainData;
            TreeInstance[] treeInstances = data.treeInstances;

            int l = treeInstances.Length;
            System.Array.Resize(ref trees, l);
            for (int i = 0; i < l; i++) {
                TreeInstance treeInstance = treeInstances[i];

                Vector3 worldPos = Vector3.Scale(treeInstance.position, data.size) + terrain.transform.position;
                // Vector3 worldPos = TreeInstanceToWorldPos(treeInstance.position);

                trees[i] = new Tree (
                    treeInstance.prototypeIndex,
                    worldPos,
                    treeInstance.heightScale,
                    treeInstance.rotation * Mathf.Rad2Deg

                );



            }
        }
	}
}
