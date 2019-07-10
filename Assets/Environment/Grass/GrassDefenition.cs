using UnityEngine;

namespace CustomVegetation {
    [CreateAssetMenu()]
    public class GrassDefenition : ScriptableObject
    {
        [System.Serializable] public class GrassPrototype {
            public Texture2D texture, normal;
            public Vector2 widthRange = new Vector2(.25f, 5f), heightRange = new Vector2(.25f, 5f);
            public Color tintColor = Color.white, hueVariation = new Color(1,0.5f,0,0.1f);
            public float cutoff = .5f;
            public float shadowCutoff = .25f;
            public float bumpStrength = .75f;
            [HideInInspector] public Rect uvOffsetInAtlasTexture;
        }
        public GrassPrototype[] grassPrototypes;
        public Material material, shadowMaterial;
        public Texture2D atlasedTexture, atlasedNormal;
    }
}