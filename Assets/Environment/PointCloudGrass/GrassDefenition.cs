using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu()]
public class GrassDefenition : ScriptableObject
{
   
    [System.Serializable] public class GrassPrototype {
        public Texture2D texture, normal;
        public Vector2 widthRange = new Vector2(.25f, 5f), heightRange = new Vector2(.25f, 5f);
        public Color tintColor = Color.white, hueVariation = new Color(1,0.5f,0,0.1f);
        [HideInInspector] public Rect uvOffsetInAtlasTexture, uvOffsetInAtlasNorm;

        public GrassPrototype () {
            widthRange = new Vector2(.25f, 5f);
            heightRange = new Vector2(.25f, 5f);
            tintColor = Color.white; 
            hueVariation = new Color(1,0.5f,0,0.1f);
        }
    }
    public GrassPrototype[] grassPrototypes;
    public Material material;
    public Texture2D atlasedTexture, atlasedNormal;



}
