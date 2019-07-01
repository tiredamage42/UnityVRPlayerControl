using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace EnvironmentTools {

    public class BillboardAssetMeshBaker : ScriptableWizard
    {

        public BillboardAsset asset;

        [MenuItem("GameObject/Billboard Asset To Mesh")]
        static void CreateWizard()
        {
            ScriptableWizard.DisplayWizard<BillboardAssetMeshBaker>("Create Light", "Create", "Apply");
            //If you don't want to use the secondary button simply leave it out:
            //ScriptableWizard.DisplayWizard<WizardCreateLight>("Create Light", "Create");
        }

    void OnWizardCreate()
    {

        if (asset == null) {
            return;
        }

     

        Mesh newMesh = new Mesh();

        Vector2[] assetVerts = asset.GetVertices();
        Vector3[] meshVerts = new Vector3[assetVerts.Length];

        List<Vector2> texCoords_ = new List<Vector2>();
        // for (int i =0 ; i < texCoords.Length; i++) {
        //     texCoords_.Add(texCoords[i]);
        // }
        


        for (int i =0 ; i < assetVerts.Length; i++) {
            meshVerts[i] = new Vector3(
                // 0,0,
                // assetVerts[i].x
                // , assetVerts[i].y,
                (Mathf.Round(assetVerts[i].x) - 0.5f) * asset.width,
                assetVerts[i].y * asset.height + asset.bottom,
                0
            );
            Debug.Log(meshVerts[i]);
            texCoords_.Add(new Vector2(Mathf.Round(assetVerts[i].x), assetVerts[i].y));
            Debug.Log(texCoords_[i]);
        }
        
        newMesh.vertices = meshVerts;
        Debug.Log("verts :" + meshVerts.Length);

        ushort[] tris = asset.GetIndices();
        
        int[] triangles = new int[tris.Length];
        for (int i =0 ; i < tris.Length; i++) {
            triangles[i] = (int)tris[i];
        }
        newMesh.triangles = triangles;
        
        Vector4[] texCoords = asset.GetImageTexCoords();
        // Debug.Log("coords :" + texCoords.Length);


        // List<Vector4> texCoords_ = new List<Vector4>();
        for (int i =0 ; i < texCoords.Length; i++) {
            Debug.Log(texCoords[i]);

            // texCoords_.Add(texCoords[i]);
        }
        newMesh.SetUVs(0, texCoords_);

        newMesh.RecalculateBounds();
        // newMesh.RecalculateNormals();

        AssetDatabase.CreateAsset(newMesh, "Assets/" + asset.name + ".mesh");
    }
    }
}
