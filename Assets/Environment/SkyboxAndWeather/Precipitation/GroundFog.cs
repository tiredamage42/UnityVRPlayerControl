using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundFog : EnvironmentParticles
{
    public void SetRotateSpeed (float rotateSpeed) {
        particleMaterial.SetFloat("_RotateSpeed", rotateSpeed);
    }
    public void SetSoftParticleFactor (float softParticleFactor) {
        particleMaterial.SetFloat("_SoftParticleFactor", softParticleFactor);
    }
    public void SetStartEndFadeRange (float startEndFadeRange) {
        particleMaterial.SetFloat("_StartEndFade", startEndFadeRange);
    }
    public void SetCloseCamRange (Vector2 closeCameRange) {
        particleMaterial.SetVector("_CloseCamRange", closeCameRange);
    }
    public void SetHeightFadeAndSteepness (Vector3 heightFadeAndSteepness) {
        particleMaterial.SetVector("_HeightRange_Steepness", heightFadeAndSteepness);
    }
    void SetMaxTravelDistance (float maxTravelDistance){
        particleMaterial.SetFloat("_MaxTravelDistance", maxTravelDistance);
    }
    
    //ground fog max travel distance depends on grid size since it moves along z axis...
    void OnEnable () {
        SetMaxTravelDistance (gridSize);
    }

    
    protected override float GetYPositionOffset (){//Vector2Int grid, Vector3 gridCenter) {
        return 0 ; //standard field height
    }

    // public float meshHeight = 5;
    
    // protected override Mesh MakeMesh () {
    //     Mesh mesh = new Mesh ();
    //     Vector2 halfRes = new Vector2(gridSize * .5f, meshHeight * .5f);
    //     List<int> indicies = new List<int>();
    //     List<Vector3> vertices = new List<Vector3>();
    //     List<Vector4> uvs = new List<Vector4>();
    //     int k  = 0;
    //     for (float i = 0.0f; i <= gridSize; i+=fieldFrequency.x) {
    //         for (float j = 0.0f; j <= meshHeight; j+=fieldFrequency.y) {
    //             vertices.Add(new Vector3((i-halfRes.x), (j-halfRes.y), -halfRes.x));
    //             uvs.Add(new Vector4(i / gridSize, j / meshHeight, Mathf.Max((float)(i % amountThresholds) / amountThresholds, (float)(j % amountThresholds) / amountThresholds), halfRes.x));
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
}
