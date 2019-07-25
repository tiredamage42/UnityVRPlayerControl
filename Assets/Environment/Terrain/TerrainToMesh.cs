// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// using MeshUtils;

// namespace CustomEnvironment {
    
//     [ExecuteInEditMode]
//     public class TerrainToMesh : MonoBehaviour
//     {
//         public int resolutionToCheck = 512;
        
//         void Update () {
//             int r = resolutionToCheck+1;
//             if (r * r > MeshUtils.MeshUtils.maxVerts) {
//                 Debug.LogError("Over count");
//             }
//             else {
//                 Debug.Log("count okay");
//             }
//         }



       
//         public static void ExportMeshes (TerrainData terrain, int lod, Vector2Int grid) {
//             int w = terrain.heightmapWidth;
//             int h = terrain.heightmapHeight;
//             int tRes = (int) Mathf.Pow(2, lod );
      
//             Vector3 meshScale = new Vector3(terrain.size.x / (w - 1) * tRes, terrain.size.y, terrain.size.z / (h - 1) * tRes);
//             Vector2 uvScale = new Vector2(1.0f / (w - 1), 1.0f / (h - 1));

//             w = (w - 1) / tRes + 1;
//             h = (h - 1) / tRes + 1;

//             int vertCount = w * h;

//             if (vertCount > MeshUtils.MeshUtils.maxVerts) {
//                 Debug.Log("mesh verts would be :: " + vertCount + " / " + MeshUtils.MeshUtils.maxVerts);
//                 return;
//             }

//             return;

//             Vector3[] tVertices = new Vector3[vertCount];
//             Vector2[] tUV = new Vector2[vertCount];
        
//             int[] tPolys;
//             tPolys = new int[(w - 1) * (h - 1) * 6];

//             float[,] tData = terrain.GetHeights(0, 0, w, h);
//             // Build vertices and UVs
//             for (int y = 0; y < h; y++)
//             {
//                 for (int x = 0; x < w; x++)
//                 {
//                     tVertices[y * w + x] = Vector3.Scale(meshScale, new Vector3(-y, tData[x * tRes, y * tRes], x));
                    
//                     Vector2 uv = Vector2.Scale( new Vector2(x * tRes, y * tRes), uvScale);
//                     tUV[y * w + x] = uv;
//                 }
//             }
        
//             int index = 0;
//             for (int y = 0; y < h - 1; y++)
//             {
//                 for (int x = 0; x < w - 1; x++)
//                 {
//                     // For each grid cell output two triangles
//                     tPolys[index++] = (y * w) + x;
//                     tPolys[index++] = ((y + 1) * w) + x;
//                     tPolys[index++] = (y * w) + x + 1;

//                     tPolys[index++] = ((y + 1) * w) + x;
//                     tPolys[index++] = ((y + 1) * w) + x + 1;
//                     tPolys[index++] = (y * w) + x + 1;
//                 }
//             }
//         }
//     }
// }