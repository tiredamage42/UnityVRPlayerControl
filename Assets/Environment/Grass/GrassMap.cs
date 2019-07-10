using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomVegetation {

    [CreateAssetMenu()]
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

        public float batchSize;
        public List<Batch> batches = new List<Batch>();

        public void InitializeMap (float batchSize) {
            this.batchSize = batchSize;
            batches.Clear();
        }
        public void AddGrassBatch (Vector2 centerPosition, Mesh mesh) {
            batches.Add(new Batch (centerPosition, mesh));
        }
    }
}