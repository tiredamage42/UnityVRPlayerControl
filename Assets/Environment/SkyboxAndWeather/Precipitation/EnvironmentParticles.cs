using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;



[ExecuteInEditMode]
public abstract class EnvironmentParticles : EnvironmentTools.GridHandler
{













    const int instanceCount = 9;

    
    // private ComputeBuffer positionBuffer;
    // private ComputeBuffer argsBuffer;
    // private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };



    // void UpdateBuffers(Vector3[] positions) {

    //     // Positions
    //     if (positionBuffer != null)
    //         positionBuffer.Release();

    //     if (meshToDraw == null)
    //         return;

    //     positionBuffer = new ComputeBuffer(instanceCount, 12);
    //     // Vector3[] positions = new Vector3[instanceCount];

    //     // for (int i = 0; i < instanceCount; i++) {
    //     //     positions[i] = 

    //     //     float angle = Random.Range(0.0f, Mathf.PI * 2.0f);
    //     //     float distance = Random.Range(20.0f, 100.0f);
    //     //     float height = Random.Range(-2.0f, 2.0f);
    //     //     float size = Random.Range(0.05f, 0.25f);
    //     //     positions[i] = new Vector4(Mathf.Sin(angle) * distance, height, Mathf.Cos(angle) * distance, size);
    //     // }


    //     positionBuffer.SetData(positions);
    //     particleMaterial.SetBuffer("positionBuffer", positionBuffer);

    //     args[0] = (uint)meshToDraw.GetIndexCount(0);
    //     args[1] = (uint)instanceCount;
    //     args[2] = (uint)meshToDraw.GetIndexStart(0);
    //     args[3] = (uint)meshToDraw.GetBaseVertex(0);

    //     if (argsBuffer == null)
    //         argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        
    //     argsBuffer.SetData(args);
    // }


    void OnEnable () {
        currentParticlesAmount = GetParticlesAmount();
        // instancePositions = new Vector4[instanceCount];
        // renderMatrices = new Matrix4x4[instanceCount];
    
        // argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        // UpdateBuffers(new Vector3[instanceCount]);
    }

    // void OnDisable() {
    //     if (positionBuffer != null)
    //         positionBuffer.Release();
    //     positionBuffer = null;

    //     if (argsBuffer != null)
    //         argsBuffer.Release();
    //     argsBuffer = null;
    // }






    // [Range(0,8)] public int drawInstance = 0;
  

    public Mesh meshToDraw;

    public Vector3 particleRotation;

    // public bool makeMesh;

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        // if (makeMesh) {
        //     GetComponent<MeshFilter>().sharedMesh = MakeMesh();
        //     makeMesh = false;
        // }

        if (currentParticlesAmount > 0) {

            particleMaterial.SetMatrix("_RotationMatrix", Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(particleRotation), Vector3.one));
            
            if (meshToDraw != null) {
            //     // Debug.Log("rendering " + name);
                DrawMesh(meshToDraw, particleMaterial, renderMatrices, renderMatrices.Length, null);//mpb);//renderMatrices.Length);
            // Render
            // Graphics.DrawMeshInstancedIndirect(meshToDraw, 0, particleMaterial, new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f)), argsBuffer);
            }
        }
        
    }

    void DrawMesh (
        Mesh mesh, Material material, Matrix4x4[] instances, int count, MaterialPropertyBlock properties,
        ShadowCastingMode shadowMode=ShadowCastingMode.Off, bool receiveShadows=true, int layer=0, Camera camera=null, LightProbeUsage lightProbes = LightProbeUsage.Off
    ) {
        // for (int i = 0; i < count; i++) {
            // renderList[0] = instances[i];
            // if (i == drawInstance) {

                // mesh, matrix, material, layer, camera, subMesh, materialPropBlock, shadowMode, receiveShadows
                // Graphics.DrawMesh(mesh, instances[i], material, 0, null, 0, null, shadowMode, receiveShadows);
                // Graphics.DrawMeshInstanced(mesh, 0, material, renderList, 1, properties, shadowMode, receiveShadows, layer, camera, lightProbes);
            // }
        // }
        


        Graphics.DrawMeshInstanced(mesh, 0, material, instances, count, properties, shadowMode, receiveShadows, layer, camera, lightProbes);
    }

    
    protected abstract float GetYPositionOffset();
        
// MaterialPropertyBlock mpb;// = new MaterialPropertyBlock();
        
    // Vector4[] instancePositions = new Vector4[instanceCount];
    Matrix4x4[] renderMatrices = new Matrix4x4[instanceCount];
    // Matrix4x4[] renderList = new Matrix4x4[1]; //idk why instanced rendering doesnt work on these...

    protected override void OnPlayerGridChange(Vector2Int playerGrid, Vector3 playerPosition, float cellSize) {
        // Vector3[] positions = new Vector3[instanceCount];
        int i = 0;
        for (int x = -1; x <= 1; x++) {
            for (int y = -1; y <= 1; y++) {
                Vector2Int grid = new Vector2Int(playerGrid.x + x, playerGrid.y + y);
                Vector3 gridCenter = GridCenterPosition(grid);

                if (Terrain.activeTerrain != null) {
                    gridCenter.y = Terrain.activeTerrain.SampleHeight(gridCenter);
                }
                else {
                    // Debug.LogWarning(name + " ::  found no terrain in scene, setting base y to 0");

                    gridCenter.y = 0;
                }


                gridCenter.y += GetYPositionOffset();
                Debug.Log(gridCenter.y + " / " + name);

                // Debug.Log(i);
                // instancePositions[i] = gridCenter;
                renderMatrices[i].SetTRS(gridCenter, Quaternion.identity, Vector3.one);
                // Debug.Log("making matrix at grid center " + gridCenter);
                i++;

            }
        }
        // if (mpb == null) {
        //     mpb = new MaterialPropertyBlock();
        // }
        
        // mpb.SetVectorArray("_WorldPos", instancePositions);

        // UpdateBuffers(positions);
    }

    protected override float GetGridSize() {
        return gridSize;
    }

    public float gridSize = 10;


    // public Vector2 fieldSize = new Vector2( 10, 10 );
    // public Vector2 fieldFrequency = new Vector2(.1f, .1f);


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
    public void SetHueVariation (Color hueVariation) {
        particleMaterial.SetColor("_HueVariation", hueVariation);
    }
    public void SetMoveSpeed (float moveSpeed) {
        particleMaterial.SetFloat("_MoveSpeed", moveSpeed);
    }
    public void SetQuadSize (Vector2 quadSize) {
        particleMaterial.SetVector("_QuadSize", quadSize);
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
    public void SetParticlesAmount (float amount) {
        currentParticlesAmount = amount;
        particleMaterial.SetFloat("_ParticlesAmount", amount);
    }

    float currentParticlesAmount;

    float GetParticlesAmount () {
        return particleMaterial.GetFloat("_ParticlesAmount");
    }

    public virtual void SetWindDirection (float windYRotation, float strength) {
        particleRotation.y = windYRotation;
    }



    // protected abstract Mesh MakeMesh();

    // protected const int amountThresholds = 4;
        

}
