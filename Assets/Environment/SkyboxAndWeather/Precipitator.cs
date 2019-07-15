using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Precipitator : EnvironmentParticles
{

    protected override Mesh MakeMesh () {
        Mesh mesh = new Mesh ();
        Vector2 halfRes = new Vector2(fieldSize.x * .5f, fieldSize.y * .5f);
        List<int> indicies = new List<int>();
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> uvs = new List<Vector3>();
        int k  = 0;
        for (float i = 0.0f; i <= fieldSize.x; i+=fieldFrequency.x) {
            for (float j = 0.0f; j <= fieldSize.y; j+=fieldFrequency.y) {
                vertices.Add(new Vector3((i-halfRes.x), 0, (j-halfRes.y)));
                uvs.Add(new Vector3(i / fieldSize.x, j / fieldSize.y, Mathf.Max((float)(i % amountThresholds) / amountThresholds, (float)(j % amountThresholds) / amountThresholds)));
                indicies.Add(k);
                k++;
            }    
        }
        mesh.SetVertices(vertices);
        mesh.SetUVs(0,uvs);
        mesh.SetIndices(indicies.ToArray(), MeshTopology.Points, 0);
        mesh.RecalculateBounds();
        return mesh;
    }

    public override void SetWindDirection (float windYRotation, float strength) {
        base.SetWindDirection(windYRotation, strength);
        particleRotation.x = -Mathf.Lerp(0, 45, strength);
    }

}
