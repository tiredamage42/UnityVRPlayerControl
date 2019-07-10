using UnityEngine;

namespace RenderTools {

    [CreateAssetMenu()] public class DetailMap : ScriptableObject {
		[System.Serializable] public struct Detail {
			public int defenitionIndex;
			public Vector3 worldPosition;

            public Vector3 scale;
            public Quaternion rotation;
			
            public Detail (int defenitionIndex, Vector3 worldPosition, Vector3 scale, Quaternion rotation) {
                this.defenitionIndex = defenitionIndex;
                this.worldPosition = worldPosition;
                this.scale = scale;
                this.rotation = rotation;
            }
		}

		[HideInInspector] public Detail[] details;
	}
}

