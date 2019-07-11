using System.Collections.Generic;
using UnityEngine;

namespace CustomVegetation {
    public class GrassMeshBuilder {
        public const int maxVerts = 65534;

        const int uvCount = 5;

        // vertex = pos
        // normal = ground normal
        // color = tint color

        // texcoord0 = atlasUVMain
        // texcoord1 = atlasUVBump
        
        // texcoord2 = (cutoff, shadowCutoff, 0, 0)
        // texcoord3 = hueVariation
        // texcoord4 = (width, height, bumpstrength, 0)
        
        List<int> indicies;
        List<Vector3> vertexPositions, normals;
        List<Color> tintColors;
        List<List<Vector4>> uvs;
        int instances;

        public void ResetBuilder () {
            instances = 0;
            vertexPositions.Clear();
            normals.Clear();
            tintColors.Clear();
            for (int i = 0; i < uvs.Count; i++) {
                uvs[i].Clear();
            }
        }
        
        public GrassMeshBuilder (int count = 0) {
            
            indicies = new List<int>(count);        
            vertexPositions = new List<Vector3>(count);
            normals = new List<Vector3>(count);
            tintColors = new List<Color>(count);

            uvs = new List<List<Vector4>>(uvCount);
            for (int i = 0; i < uvCount; i++) {
                uvs.Add(new List<Vector4>(count));
            }
        }

        public Mesh AddGrassInstance(GrassDefenition.GrassPrototype prototype, Vector3 worldPosition, Vector3 worldNormal) {
            
            vertexPositions.Add(worldPosition);
            
            normals.Add(worldNormal);

            tintColors.Add( prototype.tintColor);
            
            //uv offset
            Rect offsetMain = prototype.atlasUVMain;
            uvs[0].Add( new Vector4(offsetMain.x, offsetMain.y, offsetMain.width, offsetMain.height));
            
            Rect offsetBump = prototype.atlasUVBump;
            uvs[1].Add( new Vector4(offsetBump.x, offsetBump.y, offsetBump.width, offsetBump.height));

            //cutoff shadow cutoff
            uvs[2].Add( new Vector4(prototype.cutoffs.x, prototype.cutoffs.y, 0, 0) );

            //hue variation
            uvs[3].Add( prototype.hueVariation);
            
            //width, height, bump strength
            uvs[4].Add( new Vector4(
                Random.Range(prototype.widthRange.x, prototype.widthRange.y), 
                Random.Range(prototype.heightRange.x, prototype.heightRange.y),
                prototype.bumpStrength, 0
            ));
            
            indicies.Add(instances);
            instances++;

            if (instances >= maxVerts) {
                return BakeToMesh();
            }
            return null;
        }

        public Mesh BakeToMesh () {
            
            if (instances <= 0)
                return null;

            Mesh mesh = new Mesh ();
            // mesh.Clear();
        
            mesh.SetVertices(vertexPositions);
            mesh.SetIndices(indicies.ToArray(), MeshTopology.Points, 0);
            
            mesh.SetNormals(normals);
            mesh.SetColors(tintColors);

            for (int i = 0; i < uvs.Count; i++) {
                mesh.SetUVs(i, uvs[i]);
            }

            mesh.RecalculateBounds();

            ResetBuilder();

            return mesh;
        }
    }
}
