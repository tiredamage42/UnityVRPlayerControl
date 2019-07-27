using UnityEngine;

namespace Environment.Grass {
    [CreateAssetMenu(menuName="Environment/Grass/Grass Defenition")]
    public class GrassDefenition : ScriptableObject
    {
        [System.Serializable] public class GrassPrototype {
            public Texture2D texture, normal;
            public Vector2 widthRange = new Vector2(.25f, 5f), heightRange = new Vector2(.25f, 5f);
            public Color tintColor = Color.white, hueVariation = new Color(1,0.5f,0,0.1f);
            
            [Header("X-Albedo, Y-Shadow")]
            public Vector2 cutoffs = new Vector2(.25f, .25f);
            public float bumpStrength = 1;
            [Header("Spawning")]
            [Range(0,1)] public float rarity = .5f;
            [HideInInspector] public Rect atlasUVMain, atlasUVBump;

            public GrassPrototype () {
                widthRange = new Vector2(.25f, 5f);
                heightRange = new Vector2(.25f, 5f);
                tintColor = Color.white;
                hueVariation = new Color(1,0.5f,0,0.1f);
                bumpStrength = 1;
                rarity = 1;
            }
        }
        public GrassPrototype[] grassPrototypes;
        public Texture2D atlasedTexture, atlasedNormal;
    }
}