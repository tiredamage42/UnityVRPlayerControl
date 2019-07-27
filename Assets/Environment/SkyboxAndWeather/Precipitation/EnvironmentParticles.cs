using UnityEngine;
using UnityEngine.Rendering;


[ExecuteInEditMode]
public abstract class EnvironmentParticles : EnvironmentTools.GridHandler
{

    const int instanceCount = 9;

    

    protected override void OnEnable () {
        base.OnEnable();
        currentParticlesAmount = GetParticlesAmount();
        
    }

    public Mesh meshToDraw;
    public Vector3 particleRotation;

    protected override void Update()
    {
        base.Update();

        if (currentParticlesAmount > 0) {

            particleMaterial.SetMatrix("_RotationMatrix", Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(particleRotation), Vector3.one));
            
            if (meshToDraw != null) {
                DrawMesh(meshToDraw, particleMaterial, renderMatrices, renderMatrices.Length, null);
            }
        }
        
    }

    void DrawMesh (
        Mesh mesh, Material material, Matrix4x4[] instances, int count, MaterialPropertyBlock properties,
        ShadowCastingMode shadowMode=ShadowCastingMode.Off, bool receiveShadows=true, int layer=0, Camera camera=null, LightProbeUsage lightProbes = LightProbeUsage.Off
    ) {
       
        Graphics.DrawMeshInstanced(mesh, 0, material, instances, count, properties, shadowMode, receiveShadows, layer, camera, lightProbes);
    }

    
    protected abstract float GetYPositionOffset();
                
    Matrix4x4[] renderMatrices = new Matrix4x4[instanceCount];
    
    protected override void OnPlayerGridChange(Vector2Int playerGrid, Vector3 playerPosition, float cellSize) {
        if (currentParticlesAmount == 0)
            return;
        
        int i = 0;
        for (int x = -1; x <= 1; x++) {
            for (int y = -1; y <= 1; y++) {
                Vector2Int grid = new Vector2Int(playerGrid.x + x, playerGrid.y + y);
                Vector3 gridCenter = GridCenterPosition(grid);

                if (Terrain.activeTerrain != null) {
                    gridCenter.y = Terrain.activeTerrain.SampleHeight(gridCenter);
                }
                else {
                    gridCenter.y = 0;
                }


                gridCenter.y += GetYPositionOffset();
                
                renderMatrices[i].SetTRS(gridCenter, Quaternion.identity, Vector3.one);
                i++;

            }
        }
    }

    protected override float GetGridSize() {
        return gridSize;
    }

    public float gridSize = 10;
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
}
