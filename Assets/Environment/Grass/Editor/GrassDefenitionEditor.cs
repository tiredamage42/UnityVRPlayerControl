using UnityEngine;
using UnityEditor;
namespace Environment.Grass {

    [CustomEditor(typeof(GrassDefenition))]
    public class GrassDefenitionEditor : Editor
    {
        GrassDefenition grassDef;
        void OnEnable () {
            grassDef = target as GrassDefenition;
        }

        int maxAtlasSize = 4098;

        public override void OnInspectorGUI() 
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            maxAtlasSize = EditorGUILayout.IntField("Max Atlas Size", maxAtlasSize);

            if (GUILayout.Button("Bake Atlases")) {
                string directory = EditorUtility.SaveFolderPanel("Export Grass Atlases", "", "");//, "Terrain", "obj");
                if (directory.Length != 0) {
                    directory = directory.Substring(Application.dataPath.Length-6);
            
                    if (!directory.EndsWith("/"))
                        directory += "/";

                    grassDef.atlasedTexture = CreateAtlasAsset(directory, grassDef.name + "DiffuseAtlas");
                    grassDef.atlasedNormal = CreateAtlasAsset(directory, grassDef.name + "BumpAtlas");

                    AtlasPrototypes(grassDef.atlasedTexture, grassDef, false, maxAtlasSize);
                    AtlasPrototypes(grassDef.atlasedNormal, grassDef, true, maxAtlasSize);
                    
                    EditorUtility.SetDirty(grassDef);
                    EditorUtility.SetDirty(grassDef.atlasedTexture);
                    EditorUtility.SetDirty(grassDef.atlasedNormal);

                    AssetDatabase.SaveAssets();
                }
                else {
                    Debug.Log("Cancelled Atlasing");
                }
            }
        }

        static Texture2D CreateAtlasAsset (string directory, string name) {
            Texture2D atlas = new Texture2D(2, 2);
            AssetDatabase.CreateAsset(atlas, directory + name + ".asset");
            return atlas;
        }

        static void AtlasPrototypes (Texture2D atlas, GrassDefenition grassDef, bool normals, int maxAtlasSize) {
            int l = grassDef.grassPrototypes.Length;

            Texture2D[] atlasTextures = new Texture2D[l];
            
            for (int i = 0; i < l; i++) {
                atlasTextures[i] = normals ? grassDef.grassPrototypes[i].normal : grassDef.grassPrototypes[i].texture;
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
        }
    }
}
