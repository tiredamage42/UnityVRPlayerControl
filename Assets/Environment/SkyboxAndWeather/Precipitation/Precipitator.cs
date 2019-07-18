using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Precipitator : EnvironmentParticles
{
    public float yOffset = 10;
    protected override float GetYPositionOffset (){//Vector2Int grid, Vector3 gridCenter) {
        return yOffset;
    }

    // protected override Mesh MakeMesh () {
    //     Mesh mesh = new Mesh ();
    //     Vector2 halfRes = new Vector2(gridSize * .5f, gridSize * .5f);
    //     List<int> indicies = new List<int>();
    //     List<Vector3> vertices = new List<Vector3>();
    //     List<Vector3> uvs = new List<Vector3>();
    //     int k  = 0;
    //     for (float i = 0.0f; i <= gridSize; i+=fieldFrequency.x) {
    //         for (float j = 0.0f; j <= gridSize; j+=fieldFrequency.y) {
    //             vertices.Add(new Vector3((i-halfRes.x), 0, (j-halfRes.y)));
    //             uvs.Add(new Vector3(i / gridSize, j / gridSize, Mathf.Max((float)(i % amountThresholds) / amountThresholds, (float)(j % amountThresholds) / amountThresholds)));
    //             indicies.Add(k);
    //             k++;
    //         }    
    //     }
    //     mesh.SetVertices(vertices);
    //     mesh.SetUVs(0,uvs);
    //     mesh.SetIndices(indicies.ToArray(), MeshTopology.Points, 0);
    //     mesh.RecalculateBounds();
    //     return mesh;
    // }

    public override void SetWindDirection (float windYRotation, float strength) {
        base.SetWindDirection(windYRotation, strength);
        particleRotation.x = -Mathf.Lerp(0, 45, strength);
    }

    public void SetMaxTravelDistance (float maxTravelDistance){
        particleMaterial.SetFloat("_MaxTravelDistance", maxTravelDistance);
    }

    

}
