using UnityEngine;
using UnityEditor;
namespace CustomVegetation {

    [CustomEditor(typeof(GrassDefenition))]
    public class GrassDefenitionEditor : Editor
    {
        GrassDefenition grassDef;
        void OnEnable () {
            grassDef = target as GrassDefenition;
        }

        int maxAtlasSize = 4098;

        static string normalShader = "Custom Environment/Grass/Grass";
        static string shadowShader = "Custom Environment/Grass/ShadowPass";

        public override void OnInspectorGUI() 
        {
            
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            maxAtlasSize = EditorGUILayout.IntField("Max Atlas Size", maxAtlasSize);

            if (GUILayout.Button("Bake Atlases")) {
                BakeGrassDef(grassDef, maxAtlasSize);
            }
        }

        public static void BakeGrassDef (GrassDefenition grassDef, int maxAtlasSize) {
            grassDef.atlasedTexture = AtlasPrototypes(grassDef.atlasedTexture, grassDef, false, maxAtlasSize);
            grassDef.atlasedNormal = AtlasPrototypes(grassDef.atlasedNormal, grassDef, true, maxAtlasSize);
            
            if (grassDef.material == null) {
                grassDef.material = new Material(Shader.Find(normalShader));
            }
            grassDef.material.SetTexture("_MainTex", grassDef.atlasedTexture);
            grassDef.material.SetTexture("_BumpMap", grassDef.atlasedNormal);
            
            if (grassDef.shadowMaterial == null) {
                grassDef.shadowMaterial = new Material(Shader.Find(shadowShader));
            }
            grassDef.shadowMaterial.SetTexture("_MainTex", grassDef.atlasedTexture);

            EditorUtility.SetDirty(grassDef);
            EditorUtility.SetDirty(grassDef.atlasedTexture);
            EditorUtility.SetDirty(grassDef.atlasedNormal);
            EditorUtility.SetDirty(grassDef.material);
            EditorUtility.SetDirty(grassDef.shadowMaterial);
        }

        static Texture2D AtlasPrototypes (Texture2D atlas, GrassDefenition grassDef, bool normals, int maxAtlasSize) {
            int l = grassDef.grassPrototypes.Length;

            Texture2D[] atlasTextures = new Texture2D[l];
            
            for (int i = 0; i < l; i++) {
                atlasTextures[i] = normals ? grassDef.grassPrototypes[i].normal : grassDef.grassPrototypes[i].texture;
            }
            
            if (atlas == null) {
                atlas = new Texture2D(2, 2);
                AssetDatabase.CreateAsset(atlas, "Assets/" + (normals? "GrassNorm" : "Grass") + "Atlas.asset");
            }

            Rect[] rects = atlas.PackTextures(atlasTextures, 0, maxAtlasSize, true);
            
                for (int i = 0; i < l; i++) {
                    if (normals) {
                        grassDef.grassPrototypes[i].atlasUVBump = rects[i];
                    }
                    else {
                        grassDef.grassPrototypes[i].atlasUVMain = rects[i];
                    }
            }
        
            return atlas;
        }
    }
}
