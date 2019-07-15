using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class TestRainShader : MonoBehaviour
{

    public Vector3 rainRotation;

    public bool makeMesh, makeMeshFog;

    


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
        if (makeMeshFog) {
            MakeMeshGroundFog();
            makeMeshFog = false;
        }
        


        GetComponent<Renderer>().sharedMaterial.SetMatrix("_RotationMatrix", Matrix4x4.TRS(Vector3.one * 10, Quaternion.Euler(rainRotation), Vector3.one));
    }


    public Vector2 fieldSize = new Vector2( 10, 10 );
    public Vector2 fieldFrequency = new Vector2(.1f, .1f);
        

    void MakeMeshGroundFog () {
        int precipitationLevels = 4;

        
             Mesh mesh = new Mesh ();
            // mesh.Clear();

            // int fieldRes = 300;

            Vector2 halfRes = new Vector2(
                fieldSize.x * .5f,
                fieldSize.y * .5f
            );

            // float vertexFrequency = .1f;


            List<int> indicies = new List<int>();

            // int [] indicies = new int[fieldRes*fieldRes];
            List<Vector3> vertices = new List<Vector3>();
            List<Vector4> uvs = new List<Vector4>();
            int k  = 0;
            for (float i = 0.0f; i <= fieldSize.x; i+=fieldFrequency.x) {
            for (float j = 0.0f; j <= fieldSize.y; j+=fieldFrequency.y) {

                Vector3 vertex = new Vector3(
                    (i-halfRes.x), (j-halfRes.y), -halfRes.x
                );


                vertices.Add(vertex);


                float precipitationAmountThreshold = (float)(i % precipitationLevels) / precipitationLevels;
                float precipitationAmountThreshold2 = (float)(j % precipitationLevels) / precipitationLevels;

                Vector4 uv = new Vector4(
                    i / fieldSize.x,
                    j / fieldSize.y,
                    Mathf.Max(precipitationAmountThreshold, precipitationAmountThreshold2),
                    halfRes.x
                );


                uvs.Add(uv);



                // precipitationAmountThreshold
                    
                indicies.Add(k);
                // indicies[k] = k;
                k++;
            
            }    
            }

            Debug.Log(vertices.Count);
        
            mesh.SetVertices(vertices);
            mesh.SetUVs(0,uvs);
            mesh.SetIndices(indicies.ToArray(), MeshTopology.Points, 0);
            
            mesh.RecalculateBounds();

            GetComponent<MeshFilter>().sharedMesh = mesh;

       

    }

    void MakeMesh () {
        int precipitationLevels = 4;

        
             Mesh mesh = new Mesh ();
            // mesh.Clear();

            // int fieldRes = 300;

            Vector2 halfRes = new Vector2(
                fieldSize.x * .5f,
                fieldSize.y * .5f
            );

            // float vertexFrequency = .1f;


            List<int> indicies = new List<int>();

            // int [] indicies = new int[fieldRes*fieldRes];
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> uvs = new List<Vector3>();
            int k  = 0;
            for (float i = 0.0f; i <= fieldSize.x; i+=fieldFrequency.x) {
            for (float j = 0.0f; j <= fieldSize.y; j+=fieldFrequency.y) {

                Vector3 vertex = new Vector3(
                    (i-halfRes.x), 0, (j-halfRes.y)
                );


                vertices.Add(vertex);


                float precipitationAmountThreshold = (float)(i % precipitationLevels) / precipitationLevels;
                float precipitationAmountThreshold2 = (float)(j % precipitationLevels) / precipitationLevels;

                Vector3 uv = new Vector3(
                    i / fieldSize.x,
                    j / fieldSize.y,
                    Mathf.Max(precipitationAmountThreshold, precipitationAmountThreshold2)
                );


                uvs.Add(uv);



                // precipitationAmountThreshold
                    
                indicies.Add(k);
                // indicies[k] = k;
                k++;
            
            }    
            }

            Debug.Log(vertices.Count);
        
            mesh.SetVertices(vertices);
            mesh.SetUVs(0,uvs);
            mesh.SetIndices(indicies.ToArray(), MeshTopology.Points, 0);
            
            mesh.RecalculateBounds();

            GetComponent<MeshFilter>().sharedMesh = mesh;

       
    }
}
