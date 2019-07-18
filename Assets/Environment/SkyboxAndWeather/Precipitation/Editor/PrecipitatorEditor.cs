using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public abstract class EnvironmentParticlesEditor : Editor
{
    




    protected abstract Mesh MakeMesh ();


    string assetName;
    protected float fieldFrequency = .1f;
    protected EnvironmentParticles ep;
    protected virtual void OnEnable () {
        ep = target as EnvironmentParticles;
    }
    protected const int amountThresholds = 4;
    
    protected void DrawMeshBuildOptions () {

        fieldFrequency = EditorGUILayout.FloatField("Field Frequency", fieldFrequency);
        assetName = EditorGUILayout.TextField("Asset Name", assetName);
        bool stringEmpty = string.IsNullOrEmpty(assetName) || string.IsNullOrWhiteSpace(assetName);
        GUI.enabled = !stringEmpty;
        
        if (GUILayout.Button("Create Mesh")) {
            Mesh mesh = MakeMesh();
            ep.meshToDraw = mesh;

            EditorUtility.SetDirty(ep);
            
            AssetDatabase.CreateAsset(mesh, "Assets/" + assetName + ".asset");
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = mesh;

            
        }
        GUI.enabled = true;

        if (stringEmpty){
            EditorGUILayout.HelpBox("Specity an asset name...", MessageType.Warning);
        }
    }
}

[CustomEditor(typeof(Precipitator))]
public class PrecipitatorEditor : EnvironmentParticlesEditor {


    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Editor Only:");

        DrawMeshBuildOptions();
    }

    
    protected override Mesh MakeMesh () {
        Mesh mesh = new Mesh ();
        Vector2 halfRes = new Vector2(ep.gridSize * .5f, ep.gridSize * .5f);
        List<int> indicies = new List<int>();
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> uvs = new List<Vector3>();
        int k  = 0;
        for (float i = 0.0f; i <= ep.gridSize; i+=fieldFrequency) {
            for (float j = 0.0f; j <= ep.gridSize; j+=fieldFrequency) {
                vertices.Add(new Vector3((i-halfRes.x), 0, (j-halfRes.y)));
                uvs.Add(new Vector3(i / ep.gridSize, j / ep.gridSize, Mathf.Max((float)(i % amountThresholds) / amountThresholds, (float)(j % amountThresholds) / amountThresholds)));
                indicies.Add(k);
                k++;
            }    
        }
        mesh.SetVertices(vertices);
        mesh.SetUVs(0,uvs);
        mesh.SetIndices(indicies.ToArray(), MeshTopology.Points, 0);
        mesh.bounds = new Bounds(Vector3.zero, new Vector3(500, 500, 500));
        // mesh.RecalculateBounds();
        return mesh;
    }

    
}

[CustomEditor(typeof(GroundFog))]
public class GroundFogEditor : EnvironmentParticlesEditor {

    float fieldHeight = 5;
    float fieldHeightFrequency = .25f;
    

    public override void OnInspectorGUI() {


        base.OnInspectorGUI();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Editor Only:");

        fieldHeight = EditorGUILayout.FloatField("Field Height", fieldHeight);
        
        fieldHeightFrequency = EditorGUILayout.FloatField("Field Height Frequency", fieldHeightFrequency);
        
        DrawMeshBuildOptions();
    }

    protected override Mesh MakeMesh () {
        Mesh mesh = new Mesh ();
        Vector2 halfRes = new Vector2(ep.gridSize * .5f, fieldHeight * .5f);
        List<int> indicies = new List<int>();
        List<Vector3> vertices = new List<Vector3>();
        List<Vector4> uvs = new List<Vector4>();
        int k  = 0;

        for (float i = 0.0f; i <= ep.gridSize; i+=fieldFrequency) {
            for (float j = 0.0f; j <= fieldHeight; j+=fieldHeightFrequency) {
                vertices.Add(new Vector3((i-halfRes.x), (j), -halfRes.x));
                uvs.Add(new Vector4(i / ep.gridSize, j / fieldHeight, Mathf.Max((float)(i % amountThresholds) / amountThresholds, (float)(j % amountThresholds) / amountThresholds), halfRes.x));
                indicies.Add(k);
                k++;
            }    
        }
        mesh.SetVertices(vertices);
        mesh.SetUVs(0,uvs);
        mesh.SetIndices(indicies.ToArray(), MeshTopology.Points, 0);
        mesh.bounds = new Bounds(Vector3.zero, new Vector3(25, 25, 25));
        // mesh.RecalculateBounds();
        return mesh;
    }


    
}
