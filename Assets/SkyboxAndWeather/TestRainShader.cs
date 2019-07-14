using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class TestRainShader : MonoBehaviour
{

    public Vector3 rainRotation;

    public bool makeMesh;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (makeMesh) {

            MakeMesh();
            makeMesh = false;
        }
        


        GetComponent<Renderer>().sharedMaterial.SetMatrix("_RainRotationMatrix", Matrix4x4.TRS(Vector3.one * 10, Quaternion.Euler(rainRotation), Vector3.one));
        

    }

    void MakeMesh () {
             Mesh mesh = new Mesh ();
            // mesh.Clear();

            int [] indicies = new int[100*100];
            List<Vector3> vertices = new List<Vector3>();
            int k  = 0;
            for (int i = 0; i < 100; i++) {
            for (int j = 0; j < 100; j++) {

                vertices.Add(new Vector3(
                    (i-50) * .25f,
                    0,
                    (j-50) * .25f)
                );
                
                indicies[k] = k;
                k++;
            
            }    
            }
        
            mesh.SetVertices(vertices);
            mesh.SetIndices(indicies, MeshTopology.Points, 0);
            
            // mesh.SetIndices(indicies.ToArray(), MeshTopology.Points, 0);
            
            // mesh.SetNormals(normals);
            // mesh.SetColors(tintColors);

            // for (int i = 0; i < uvs.Count; i++) {
            //     mesh.SetUVs(i, uvs[i]);
            // }

            mesh.RecalculateBounds();

            GetComponent<MeshFilter>().sharedMesh = mesh;

       
    }
}
