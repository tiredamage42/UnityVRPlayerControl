using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Environment.Grass {

    [CreateAssetMenu(menuName="Environment/Grass/GrassMap")]
    public class GrassMap : ScriptableObject
    {
        [System.Serializable] public class Batch {
            public Vector2 centerPosition;
            public Mesh mesh;

            public Batch (Vector2 centerPosition, Mesh mesh) {
                this.centerPosition = centerPosition;
                this.mesh = mesh;
            }
        }

        public GrassDefenition grassDef;

        [HideInInspector] public float batchSize;
        [HideInInspector] public List<Batch> batches = new List<Batch>();
    }
}