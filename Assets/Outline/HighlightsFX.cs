using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

// namespace Valve.VR.InteractionSystem
// {

[RequireComponent(typeof(Camera))]

public class HighlightsFX : MonoBehaviour 
{
	public enum HighlightType { Glow = 0, Solid = 1 }
	public enum SortingType { Overlay = 2, DepthFiltered = 3, }
	public enum RTResolution { Quarter = 4, Half = 2, Full = 1 }

    struct OutlineData
    {
        public Color color;
        public SortingType sortingType;
    }

    [Header("Outline Settings")]
    public RTResolution m_resolution = RTResolution.Full;
    public CameraEvent BufferDrawEvent = CameraEvent.BeforeImageEffects;

    [Header("BlurOptimized Settings")]
    [Range(0, 2)] public int downsample = 0;
    [Range(0.0f, 10.0f)] public float blurSize = 3.0f;
    [Range(1, 4)] public int blurIterations = 2;
    
    private CommandBuffer m_commandBuffer;

    private int m_highlightRTID, m_blurredRTID, m_temporaryRTID;

    // private Dictionary<List<Renderer>, OutlineData> m_objectRenderers;
    private Dictionary<Renderer, OutlineData> m_objectRenderers = new Dictionary<Renderer, OutlineData>();
    private List<Renderer> m_objectExcluders = new List<Renderer>();
    // private List<List<Renderer>> m_objectExcluders;

    private Material m_highlightMaterial, m_blurMaterial;		
    private Camera m_camera;

	private int m_RTWidth = 512;
	private int m_RTHeight = 512;

    // private RenderTexture m_highlightRT, m_blurredRT, m_temporaryRT;
    


    // public void AddRenderers(List<Renderer> renderers, Color col, SortingType sorting)
    // {
    //     var data = new OutlineData() { 
    //         color = col, 
    //         sortingType = sorting 
    //     };

    //     m_objectRenderers.Add(renderers, data);      
    //     RecreateCommandBuffer();
    // }
    public void AddRenderers(List<Renderer> renderers, Color col, SortingType sorting)
    {
        bool changed = false;
        var data = new OutlineData() { color = col, sortingType = sorting };
        foreach (var r in renderers) {
            if (AddRenderer(r, data, false)) changed = true;
        }
        if (changed) RecreateCommandBuffer();
        
    }
    bool AddRenderer(Renderer renderer, OutlineData data, bool rebuild=true)
    {
        if (!m_objectRenderers.ContainsKey(renderer)) {
            m_objectRenderers.Add(renderer, data);     
            if (rebuild) RecreateCommandBuffer();
            return true;
        }
        return false;
    }
    public void AddRenderer(Renderer renderer, Color col, SortingType sorting)
    {
        AddRenderer(renderer, new OutlineData() { color = col, sortingType = sorting });
    }

    public void RemoveRenderers(List<Renderer> renderers)
    {
        bool changed = false;
        foreach (var r in renderers) {
            if (RemoveRenderer(r, false)) changed = true;
        } 
        if (changed) RecreateCommandBuffer();
    }
        
    public bool RemoveRenderer(Renderer renderer, bool rebuild=true)
    {
        if (m_objectRenderers.ContainsKey(renderer)) {
            m_objectRenderers.Remove(renderer);      
            if (rebuild) RecreateCommandBuffer();
            return true;
        }
        return false;
    }




    public void AddExcluders(List<Renderer> renderers)
    {
        bool changed = false;
        foreach (var r in renderers) {
            if (AddExcluder(r, false)) changed = true;
        }
        if (changed) RecreateCommandBuffer();
    }
    public bool AddExcluder(Renderer renderer, bool rebuild=true)
    {
        if (!m_objectExcluders.Contains(renderer)) {
            m_objectExcluders.Add(renderer);     
            if (rebuild) RecreateCommandBuffer();
            return true;
        }
        return false;
    }

    public void RemoveExcluders(List<Renderer> renderers)
    {
        bool changed = false;
        foreach (var r in renderers) {
            if (RemoveExcluder(r, false)) changed = true;
        } 
        if (changed) RecreateCommandBuffer();
    }
    public bool RemoveExcluder(Renderer renderer, bool rebuild=true)
    {
        if (m_objectExcluders.Contains(renderer)) {
            m_objectExcluders.Remove(renderer);      
            if (rebuild) RecreateCommandBuffer();
            return true;
        }
        return false;
    }






    public void ClearOutlineData()
    {
        m_objectRenderers.Clear();
        m_objectExcluders.Clear();
        RecreateCommandBuffer();
    }

    public static HighlightsFX instance;

    void Awake()
	{
        if (instance == null) {
            instance = this;
        }

        // m_objectRenderers = new Dictionary<List<Renderer>, OutlineData>();
        // m_objectExcluders = new List<List<Renderer>>();

        m_commandBuffer = new CommandBuffer();
        m_commandBuffer.name = "HighlightFX Command Buffer";

        m_highlightRTID = Shader.PropertyToID("_HighlightRT");
        m_blurredRTID = Shader.PropertyToID("_BlurredRT");
        m_temporaryRTID = Shader.PropertyToID("_TemporaryRT");

        m_highlightMaterial = new Material(Shader.Find("Custom/Highlight"));
        m_blurMaterial = new Material(Shader.Find("Hidden/FastBlur"));

        m_camera = GetComponent<Camera>();
        m_camera.depthTextureMode = DepthTextureMode.Depth;
        m_camera.AddCommandBuffer(BufferDrawEvent, m_commandBuffer);
	}
    Vector2 resolution;
    void Update () {
    if (resolution.x != Screen.width || resolution.y != Screen.height)
     {

         RecreateCommandBuffer();
         
         resolution.x = Screen.width;
         resolution.y = Screen.height;
     }
    }

    private void RecreateCommandBuffer()
    {
        m_commandBuffer.Clear();

        if (m_objectRenderers.Count == 0)
            return;

        m_RTWidth = (int)(Screen.width / (float)m_resolution);
        m_RTHeight = (int)(Screen.height / (float)m_resolution);


        // initialization

        m_commandBuffer.GetTemporaryRT(m_highlightRTID, m_RTWidth, m_RTHeight, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
        m_commandBuffer.SetRenderTarget(m_highlightRTID, BuiltinRenderTextureType.CurrentActive);
        m_commandBuffer.ClearRenderTarget(false, true, Color.clear);


        // rendering into texture
        // foreach (var collection in m_objectRenderers)
        // {
        //     m_commandBuffer.SetGlobalColor("_Color", collection.Value.color);
        //     foreach (var render in collection.Key)
        //     {
        //         m_commandBuffer.DrawRenderer(render, m_highlightMaterial, 0, (int)collection.Value.sortingType);
        //     }
        // }

        foreach (var renderer in m_objectRenderers.Keys)
        {
            OutlineData d = m_objectRenderers[renderer];
            m_commandBuffer.SetGlobalColor("_Color", d.color);
            m_commandBuffer.DrawRenderer(renderer, m_highlightMaterial, 0, (int)d.sortingType);
        }
            

        // excluding from texture 

        m_commandBuffer.SetGlobalColor("_Color", Color.clear);
        // foreach (var collection in m_objectExcluders)
        // {         
        //     foreach (var render in collection)
        //     {
        //         m_commandBuffer.DrawRenderer(render, m_highlightMaterial, 0, (int) SortingType.Overlay);
        //     }
        // }
        foreach (var renderer in m_objectExcluders)
        {         
            m_commandBuffer.DrawRenderer(renderer, m_highlightMaterial, 0, (int) SortingType.Overlay);
            
        }





        // Bluring texture

        float widthMod = 1.0f / (1.0f * (1 << downsample));

        int rtW = m_RTWidth >> downsample;
        int rtH = m_RTHeight >> downsample;
   
        m_commandBuffer.GetTemporaryRT(m_blurredRTID, rtW, rtH, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
        m_commandBuffer.GetTemporaryRT(m_temporaryRTID, rtW, rtH, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);

        if (downsample != 0) {

            m_commandBuffer.Blit(m_highlightRTID, m_temporaryRTID, m_blurMaterial, 0);
        }


        for (int i = 0; i < blurIterations; i++)
        {
            float iterationOffs = (i * 1.0f);
            var blurHorizParam = blurSize * widthMod + iterationOffs;
            var blurVertParam = -blurSize * widthMod - iterationOffs;

            m_commandBuffer.SetGlobalVector("_Parameter", new Vector4(blurHorizParam, blurVertParam));

            // m_commandBuffer.Blit(i == 0 ? m_highlightRTID : m_temporaryRTID, m_blurredRTID, m_blurMaterial, 1);// + passOffs);
            m_commandBuffer.Blit(i == 0 && downsample == 0 ? m_highlightRTID : m_temporaryRTID, m_blurredRTID, m_blurMaterial, 1);// + passOffs);
            
            m_commandBuffer.Blit(m_blurredRTID, m_temporaryRTID, m_blurMaterial, 2);// + passOffs);
        }

        // occlusion

        // if (m_fillType == FillType.Outline)
        // {
            // Excluding the original image from the blurred image, leaving out the areal alone
            m_commandBuffer.SetGlobalTexture("_SecondaryTex", m_highlightRTID);
            m_commandBuffer.Blit(m_temporaryRTID, m_blurredRTID, m_highlightMaterial, 1);
            m_commandBuffer.SetGlobalTexture("_SecondaryTex", m_blurredRTID);
        // }
        // else
        // {
        //     m_commandBuffer.SetGlobalTexture("_SecondaryTex", m_temporaryRTID);
        // }

        // back buffer
        // m_commandBuffer.Blit(BuiltinRenderTextureType.CameraTarget, m_highlightRTID);

        // overlay
        m_commandBuffer.Blit(m_highlightRTID, BuiltinRenderTextureType.CameraTarget, m_highlightMaterial, 0);


        m_commandBuffer.ReleaseTemporaryRT(m_temporaryRTID);
        m_commandBuffer.ReleaseTemporaryRT(m_blurredRTID);
        m_commandBuffer.ReleaseTemporaryRT(m_highlightRTID);
    }
}
// }
