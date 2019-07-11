using UnityEngine;

namespace CustomVegetation {
    [CreateAssetMenu()]
    public class GrassDefenition : ScriptableObject
    {
        [System.Serializable] public class GrassPrototype {
            public Texture2D texture, normal;
            public Vector2 widthRange = new Vector2(.25f, 5f), heightRange = new Vector2(.25f, 5f);
            public Color tintColor = Color.white, hueVariation = new Color(1,0.5f,0,0.1f);
            // public float cutoff = .5f;

            [Header("X-Albedo, Y-Shadow")]
            public Vector2 cutoffs = new Vector2(.25f, .25f);
            // public float shadowCutoff = .25f;
            public float bumpStrength = .75f;
            [Header("Spawning")]
            [Range(0,1)] public float rarity = .5f;
            [HideInInspector] public Rect atlasUVMain, atlasUVBump;
        }
        public GrassPrototype[] grassPrototypes;
        public Material material, shadowMaterial;
        public Texture2D atlasedTexture, atlasedNormal;
    }
}