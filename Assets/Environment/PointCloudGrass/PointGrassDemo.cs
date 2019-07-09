using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class PointGrassDemo : MonoBehaviour
{

    public Vector2 cameraFadeRange = new Vector2(25, 35);
    public float windSpeed = 1;
    public float windFrequency = 1;
    public float windScale = 1;
    public Mesh mesh;
    public GrassDefenition grassDef;


    void Update()
    {
        Shader.SetGlobalVector("_PCGRASS_CAMERA_RANGE", new Vector4(cameraFadeRange.x, cameraFadeRange.y, 0, 0));
        Shader.SetGlobalVector("_PCGRASS_WINDSETTINGS", new Vector4(windSpeed, windFrequency, windScale, 0));

        Render();


        if (rebuildGrass) {
            RebuildGrass();
            rebuildGrass = false;
        }
    }


    void OnEnable () {
        
    }


    static void DrawMesh (
        Mesh mesh, Material material, Matrix4x4 transform, ShadowCastingMode shadowMode, bool receiveShadows,  
        MaterialPropertyBlock properties=null, int layer=0, Camera camera=null
    ) {
        Graphics.DrawMesh(mesh, transform, material, layer, camera, 0, properties, shadowMode, receiveShadows);
    }

    public void Render () {
        if (mesh == null)
            return;

        if (grassDef == null)
            return;

        Matrix4x4 transformMatrix = Matrix4x4.TRS(transform.position, Quaternion.identity, Vector3.one);

        /*
            need to draw in two passes:

            1 - the actual waving grass without shadow casting 
                (so the wind movement doesnt cause flickering shadows)

            2 - the shadow casting grass without movement
        */
        DrawMesh (mesh, grassDef.material, transformMatrix, ShadowCastingMode.Off, true);
        DrawMesh (mesh, grassDef.shadowMaterial, transformMatrix, ShadowCastingMode.ShadowsOnly, false);
    }

    class GrassMeshBuilder {
        Mesh mesh;
        List<int> indicies;


        // vertex = pos
        // normal = ground normal
        // color = tint color
        // texcoord0 = textureAtlasOffset
        // texcoord1 = textureAtlasOffsetNorm
        // texcoord2 = hueVariation
        // texcoord3 = (width, height, cutoff, bumpstrength)
        
        List<Vector3> vertexPositions, normals;
        List<Color> tintColors;
        List<List<Vector4>> uvs;

        Vector3 worldCenter;

        int instances;
        
        public GrassMeshBuilder (Mesh mesh, Vector3 worldCenter, int count = 0) {
            
            if (mesh == null)
                return;
            
            this.mesh = mesh;
            this.worldCenter = worldCenter;

            indicies = new List<int>(count);        
            vertexPositions = new List<Vector3>(count);
            normals = new List<Vector3>(count);
            tintColors = new List<Color>(count);

            uvs = new List<List<Vector4>>(4) {
                new List<Vector4>(count), new List<Vector4>(count), 
                new List<Vector4>(count), new List<Vector4>(count),
            };
        }

        public void AddGrassInstance(GrassDefenition.GrassPrototype prototype, Vector3 worldPosition, Vector3 worldNormal) {
            if (mesh == null)
                return;
            
            vertexPositions.Add(worldPosition - worldCenter);
            
            normals.Add(worldNormal);

            tintColors.Add( prototype.tintColor);
            
            //uv offsets
            uvs[0].Add( new Vector4(prototype.uvOffsetInAtlasTexture.x, prototype.uvOffsetInAtlasTexture.y, prototype.uvOffsetInAtlasTexture.width, prototype.uvOffsetInAtlasTexture.height));
            uvs[1].Add( new Vector4(prototype.uvOffsetInAtlasNorm.x, prototype.uvOffsetInAtlasNorm.y, prototype.uvOffsetInAtlasNorm.width, prototype.uvOffsetInAtlasNorm.height));

            //hue variation
            uvs[2].Add( prototype.hueVariation);
            
            //width, height, cutoff, bump strength
            uvs[3].Add( new Vector4(
                Random.Range(prototype.widthRange.x, prototype.widthRange.y), 
                Random.Range(prototype.heightRange.x, prototype.heightRange.y),
                prototype.cutoff, prototype.bumpStrength
            ));
            

            indicies.Add(instances);
            instances++;
        }

        public void BakeToMesh () {
            if (mesh == null)
                return;

            mesh.Clear();
        
            mesh.SetVertices(vertexPositions);

            mesh.SetNormals(normals);

            mesh.SetIndices(indicies.ToArray(), MeshTopology.Points, 0);
            
            mesh.SetColors(tintColors);

            for (int i = 0; i < uvs.Count; i++) {
                mesh.SetUVs(i, uvs[i]);
            }
        }
    }


    
    [Header("Debug Build Grass")]
    public int seed;
    public Vector2 size;
    [Range(1, 60000)] public int grassNumber;
    public bool rebuildGrass;
    


    // Update is called once per frame
    void RebuildGrass()
    {
        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null)
            return;

        if (grassDef == null)
            return;

        Vector3 terrainSize = terrain.terrainData.size;
        
        Random.InitState(seed);

        if (mesh == null) mesh = new Mesh();

        GrassMeshBuilder grassBuilder = new GrassMeshBuilder(mesh, transform.position, grassNumber);
            
        for (int i = 0; i < grassNumber; ++i)
        {
            
            Vector3 worldPos = this.transform.position + new Vector3(size.x * Random.Range(-0.5f, 0.5f), 0, size.y * Random.Range(-0.5f, 0.5f));
            worldPos.y = terrain.SampleHeight(worldPos);
            
            Vector3 worldNorm = terrain.terrainData.GetInterpolatedNormal(worldPos.x / terrainSize.x, worldPos.z / terrainSize.z);

            grassBuilder.AddGrassInstance(
                grassDef.grassPrototypes[Random.Range(0, grassDef.grassPrototypes.Length)], 
                worldPos, 
                worldNorm
            );
            
        }
        grassBuilder.BakeToMesh();
       
    }
}