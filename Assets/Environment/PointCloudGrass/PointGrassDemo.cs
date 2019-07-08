using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PointGrassDemo : MonoBehaviour
{

    public Vector2 cameraFadeRange = new Vector2(25, 35);
    
    public bool rebuildGrass;
    void Update()
    {

        Shader.SetGlobalVector("_PCGRASS_CAMERA_RANGE", new Vector4(cameraFadeRange.x, cameraFadeRange.y, 0, 0));

        if (rebuildGrass) {

            RebuildGrass();
            rebuildGrass = false;
        }
        
    }


    void OnEnable () {
        InitializeMesh();
    }

    // static readonly string shaderName = "Custom/PCGrass";


    void InitializeMesh () {
        filter = GetComponent<MeshFilter>();
        if (filter == null)
            filter = gameObject.AddComponent<MeshFilter>();

        if (filter.sharedMesh == null) 
            filter.sharedMesh = new Mesh();
        
        
        renderer = GetComponent<MeshRenderer>();
        if (renderer == null)
            renderer = gameObject.AddComponent<MeshRenderer>();

        // renderer.sharedMaterial = new Material(Shader.Find(shaderName));
        
    }

    MeshFilter filter;
    MeshRenderer renderer;

    public int seed;
    public Vector2 size;

    [Range(1, 60000)]
    public int grassNumber;





    // vertex = pos
    // normal = ground normal
    // color = tint color
    // texcoord0 = textureAtlasOffset
    // texcoord1 = textureAtlasOffsetNorm
    // texcoord2 = hueVariation
    // texcoord3 = (width, height, 0, 0)
    
    
    void AddGrassInstance(GrassDefenition grassDef, int grassPrototype, 
        List<Color> colors, 
        List<Vector4> uvs0, 
        List<Vector4> uvs1,
        List<Vector4> uvs2, 
        List<Vector4> uvs3
    ) {
        //position handled
        //normal handled

        GrassDefenition.GrassPrototype prototype = grassDef.grassPrototypes[grassPrototype];

        colors.Add( prototype.tintColor);
        
        uvs0.Add( new Vector4(prototype.uvOffsetInAtlasTexture.x, prototype.uvOffsetInAtlasTexture.y, prototype.uvOffsetInAtlasTexture.width, prototype.uvOffsetInAtlasTexture.height));
        uvs1.Add( new Vector4(prototype.uvOffsetInAtlasNorm.x, prototype.uvOffsetInAtlasNorm.y, prototype.uvOffsetInAtlasNorm.width, prototype.uvOffsetInAtlasNorm.height));
        
        uvs2.Add( prototype.hueVariation);
        uvs3.Add( new Vector4(
            Random.Range(prototype.widthRange.x, prototype.widthRange.y), 
            Random.Range(prototype.heightRange.x, prototype.heightRange.y),
            0,0
        ));
    }

    public GrassDefenition grassDef;


    // Update is called once per frame
    void RebuildGrass()
    {
        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null)
            return;

        if (grassDef == null)
            return;

        Debug.Log("Rebuilding");

        Vector3 terrainSize = terrain.terrainData.size;
        
        Random.InitState(seed);
            
        List<Vector3> positions = new List<Vector3>(grassNumber);
        int[] indicies = new int[grassNumber];
        List<Color> colors = new List<Color>(grassNumber);
        List<Vector3> normals = new List<Vector3>(grassNumber);
        List<Vector4> uvs0 = new List<Vector4>(grassNumber);
        List<Vector4> uvs1 = new List<Vector4>(grassNumber);
        List<Vector4> uvs2 = new List<Vector4>(grassNumber);
        List<Vector4> uvs3 = new List<Vector4>(grassNumber);
        
        for (int i = 0; i < grassNumber; ++i)
        {
            
            Vector3 worldPos = this.transform.position;
            worldPos.x += size.x * Random.Range(-0.5f, 0.5f);
            worldPos.z += size.y * Random.Range(-0.5f, 0.5f);
            worldPos.y = terrain.SampleHeight(worldPos);

            var grassPosition = worldPos;
            grassPosition -= this.transform.position;
            positions.Add(grassPosition);

            Vector3 normal = terrain.terrainData.GetInterpolatedNormal(worldPos.x / terrainSize.x, worldPos.z / terrainSize.z);
            normals.Add(normal);
            indicies[i] = i;

            AddGrassInstance(grassDef, Random.Range(0, grassDef.grassPrototypes.Length), 
                colors, 
                uvs0, 
                uvs1,
                uvs2, 
                uvs3
            );
            
            // colors.Add(new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1));

            
        }

        Mesh mesh = filter.sharedMesh;

        mesh.Clear();
        
        mesh.SetVertices(positions);
        mesh.SetNormals(normals);
        mesh.SetIndices(indicies, MeshTopology.Points, 0);
        mesh.SetColors(colors);
        
        mesh.SetUVs(0, uvs0);
        mesh.SetUVs(1, uvs1);
        mesh.SetUVs(2, uvs2);
        mesh.SetUVs(3, uvs3);
        
        UnityEditor.EditorUtility.SetDirty(mesh);   

        renderer.sharedMaterial = grassDef.material;
    }
}