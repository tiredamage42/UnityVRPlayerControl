using UnityEngine;

namespace CustomVegetation {

    [CreateAssetMenu()] public class DetailMap : ScriptableObject {
		[System.Serializable] public struct Detail {
			public int defenitionIndex;
			public Vector3 worldPosition;

            public Vector3 scale;
            public Quaternion rotation;
			// public float sizeScale, yRotation;
            
            // public Detail (int defenitionIndex, Vector3 worldPosition, float sizeScale, float yRotation) {
            public Detail (int defenitionIndex, Vector3 worldPosition, 
            Vector3 scale, Quaternion rotation
            // float sizeScale, float yRotation
        ) {
            
                this.defenitionIndex = defenitionIndex;
                this.worldPosition = worldPosition;
                this.scale = scale;
                this.rotation = rotation;
            }
		}

		[HideInInspector] public Detail[] details;
	}
}

