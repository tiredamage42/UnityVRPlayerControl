using System.Collections;
using System.Collections.Generic;
using UnityEngine;




[ExecuteInEditMode]
public abstract class EnvironmentParticles : MonoBehaviour
{

    public Vector3 particleRotation;

    public bool makeMesh;

    // Update is called once per frame
    void Update()
    {

        if (makeMesh) {
            GetComponent<MeshFilter>().sharedMesh = MakeMesh();
            makeMesh = false;
        }
        particleMaterial.SetMatrix("_RotationMatrix", Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(particleRotation), Vector3.one));
    }


    public Vector2 fieldSize = new Vector2( 10, 10 );
    public Vector2 fieldFrequency = new Vector2(.1f, .1f);


    public Material particleMaterial;
    public void SetNoiseTexture(Texture2D noiseTexture) {
        particleMaterial.SetTexture("_NoiseMap", noiseTexture);
    }
    public void SetMainTexture(Texture2D texture) {
        particleMaterial.SetTexture("_MainTex", texture);
    }
    public void SetColor (Color color) {
        particleMaterial.SetColor("_Color", color);
    }
    public void SetMoveSpeed (float moveSpeed) {
        particleMaterial.SetFloat("_MoveSpeed", moveSpeed);
    }
    public void SetQuadSize (Vector2 quadSize) {
        particleMaterial.SetVector("_QuadSize", quadSize);
    }
    public void SetMaxTravelDistance (float maxTravelDistance){
        particleMaterial.SetFloat("_MaxTravelDistance", maxTravelDistance);
    }
    public void SetCameraRange(Vector2 cameraRange) {
        particleMaterial.SetVector("_CameraRange", cameraRange);
    }
    public void SetFlutterFrequency (Vector2 flutterFrequency) {
        particleMaterial.SetVector("_FlutterFrequency", flutterFrequency);
    }
    public void SetFlutterSpeed (Vector2 flutterSpeed) {
        particleMaterial.SetVector("_FlutterSpeed", flutterSpeed);
    }
    public void SetFlutterMagnitude (Vector2 flutterMagnitude) {
        particleMaterial.SetVector("_FlutterMagnitude", flutterMagnitude);
    }
    public void SetSizeRange(Vector2 sizeRange) {
        particleMaterial.SetVector("_SizeRange", sizeRange);
    }

    public virtual void SetWindDirection (float windYRotation, float strength) {
        particleRotation.y = windYRotation;
    }

    protected abstract Mesh MakeMesh();

    protected const int amountThresholds = 4;
        

}
