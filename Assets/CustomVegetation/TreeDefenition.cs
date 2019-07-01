using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CustomVegetation {
	[CreateAssetMenu()]
	public class TreeDefenition : ScriptableObject {
		[System.Serializable] public class LODInfo {
			public Mesh mesh;
			public Material[] materials;
			public int gridDistance;
			public BillboardAsset billboardAsset;

            public bool isBillboard;

            public void InitializeMaterials () {
                if (billboardAsset != null) {
                    for (int i = 0; i < materials.Length; i++) {
                        materials[i].SetVectorArray("_BillboardSliceCoords", billboardAsset.GetImageTexCoords());
                        materials[i].SetFloat("_BillboardSlices", billboardAsset.imageCount);
                    }
                }
            }
		}
		public LODInfo[] lods;

        public void InitializeMaterials () {
            for (int i = 0; i < lods.Length; i++) {
                lods[i].InitializeMaterials();
            }
        }
	}
}