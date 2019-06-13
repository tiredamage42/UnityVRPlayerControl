// using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// [ExecuteInEditMode]
public class ObjectOutlines : MonoBehaviour 
{
    /*
        for flicker:
            try increase depth buffer
            lower near clip plane
    
     */
    public Renderer debugRenderer;
    public string outlineLayer = "Outline";
    
    Shader DrawSimple;
    [SerializeField] Camera AttachedCamera, TempCam;
    [SerializeField] Material Post_Mat;
    [SerializeField] Texture defaultBlackTexture;
    int stagedLayer;

    const int maskOutPass = 0;
    const int finalPass = 1;

    [Range(0,25)] public float intensity = 2;
    [Range(0,5)] public float heaviness = 1;
    
    [Tooltip("raise this if depth tested highlight colors bleed into overlay areas")]
    [Range(0.1f, 5)] public float overlayAlphaHelper = 1;

    
    [Header("Blur")]
    [Range(0, 2)] public int downsample = 1;
    [Range(0.0f, 10.0f)] public float blurSize = 3.0f;
    [Range(1, 4)] public int blurIterations = 2;


    public List<HighlightGroup> highlightGroups = new List<HighlightGroup>();

    static ObjectOutlines _instance;
    public static ObjectOutlines instance {
        get {
            if (_instance == null) {
                _instance = GameObject.FindObjectOfType<ObjectOutlines>();
                if (_instance == null) {
                    Debug.LogWarning("no instance of object outline in the scene");
                }
            }
            return _instance;
            
        }
    }


    void Awake () {
        InitializeAwake();
    }

    void InitializeDefaultBlackTexture () {
        //might need to adjust alphas on this...
        Texture2D defaultBlackTexture = new Texture2D(2, 2, TextureFormat.R8, false, false);
        Color c = Color.clear;
        defaultBlackTexture.SetPixels( new Color[] {c,c,c,c} );
        this.defaultBlackTexture = defaultBlackTexture;
    }

    void InitializeAwake () {
        AttachedCamera = GetComponent<Camera>();
        // AttachedCamera.depthTextureMode = DepthTextureMode.Depth;
        TempCam = new GameObject().AddComponent<Camera>();
        TempCam.enabled = false;
        Post_Mat = new Material(Shader.Find("Hidden/PostOutline"));
        DrawSimple = Shader.Find("Custom/DrawSimple");

        InitializeDefaultBlackTexture();
        InitializeShaderIDs();
    }


        


    void OnEnable () {
        _instance = this;
        
        stagedLayer = LayerMask.NameToLayer(outlineLayer);
        
        for (int i = 0; i < highlightGroups.Count; i++) {
            highlightGroups[i].OnEnable();
        }

        if (debugRenderer != null) {
            // Highlight_Renderer(debugRenderer, 0);
        }

        bool needsDepth, needsOverlay;
        bool renderingHighlighted = HasAny(out needsDepth, out needsOverlay);
        enabled = renderingHighlighted;
    }

    void OnDisable () {
        if (!Application.isPlaying) {
            DestroyImmediate(TempCam.gameObject);
            DestroyImmediate(Post_Mat);
            DestroyImmediate(defaultBlackTexture);
        }
    }

    void InitializeTemporaryCamera () {
        //set up a temporary camera
        TempCam.CopyFrom(AttachedCamera);
        TempCam.renderingPath = RenderingPath.VertexLit;
        TempCam.allowDynamicResolution = false;
        TempCam.allowHDR = false;
        TempCam.allowMSAA = false;        
        TempCam.farClipPlane = 25;
        TempCam.nearClipPlane = .1f;
        TempCam.useOcclusionCulling = true; 
        TempCam.backgroundColor = Color.clear;
    }

    int _Color, _MaskOut, _AddOverlay, _AddHighlight, _MaskAlphaSubtractMult, _OverlayMask, _Intensity_Heaviness_OverlayAlphaHelper;

    void InitializeShaderIDs () {
        _Color = Shader.PropertyToID("_OutColor");
        _MaskOut = Shader.PropertyToID("_MaskOut");
        _AddOverlay = Shader.PropertyToID("_OverlayHighlights");
        _AddHighlight = Shader.PropertyToID("_DepthHighlights");
        _MaskAlphaSubtractMult = Shader.PropertyToID("_MaskAlphaSubtractMult");
        _OverlayMask = Shader.PropertyToID("_OverlayMask");
        _Intensity_Heaviness_OverlayAlphaHelper = Shader.PropertyToID("_Intensity_Heaviness_OverlayAlphaHelper");
    }

    void MaskOutInsides (RenderTexture image, RenderTexture mask, RenderTexture target, float maskAlphaSubtractMult) {
        Post_Mat.SetTexture(_MaskOut, mask);
        Post_Mat.SetFloat(_MaskAlphaSubtractMult, maskAlphaSubtractMult);
        Graphics.Blit(image, target, Post_Mat, maskOutPass);
    }
    public enum SortingType { Overlay = 1, DepthFilter = 2 }

    [System.Serializable] public class HighlightGroup {

        public void OnEnable () {
            if (renderers == null) {
                renderers = new Dictionary<Renderer, int>();
            }
            if (rKeys == null) {
                rKeys = new HashSet<Renderer>();
            }
        }

        public HighlightGroup (Color color, SortingType sortingType) {
            this.highlightColor = color;
            this.sortingType = sortingType;
            OnEnable();   
        }
        
        public Color highlightColor = Color.red;
        public SortingType sortingType;
        public Dictionary<Renderer, int> renderers = new Dictionary<Renderer, int>();
        public HashSet<Renderer> rKeys = new HashSet<Renderer>();
        public void Remove (Renderer renderer) {
            renderers.Remove(renderer);
            rKeys.Remove(renderer);
        }
        public void Add (Renderer renderer) {
            renderers.Add(renderer, renderer.gameObject.layer);
            rKeys.Add(renderer);
        }
        public void Stage (int stagedLayer) {
            foreach (var renderer in rKeys) {
                renderers[renderer] = renderer.gameObject.layer;
                renderer.gameObject.layer = stagedLayer;
            }
        }
        public void Unstage () {
            foreach (var renderer in rKeys) {
                renderer.gameObject.layer = renderers[renderer];   
            }   
        }
    }

    public void UnHighlightRenderers(List<Renderer> renderers) {
        for (int i = 0; i < renderers.Count; i++) {
            UnHighlightRenderer(renderers[i]);
        }
    }
    public void HighlightRenderers(List<Renderer> renderers, int highlightGroupIndex) {
        for (int i = 0; i < renderers.Count; i++) {
            HighlightRenderer(renderers[i], highlightGroupIndex);
        }
    }

    public void HighlightRenderer(Renderer renderer, int highlightGroupIndex) {

        for (int i = 0; i < highlightGroups.Count; i++) {
            HighlightGroup group = highlightGroups[i];
            if (group.rKeys.Contains(renderer)) {
                if (i != highlightGroupIndex) {
                    group.Remove(renderer);
                }
            }
            else {
                if (i == highlightGroupIndex) {
                    group.Add(renderer);
                    enabled = true;
                }
            }
        }
    }
    public void UnHighlightRenderer(Renderer renderer) {

        for (int i = 0; i < highlightGroups.Count; i++) {
            HighlightGroup group = highlightGroups[i];
            if (group.rKeys.Contains(renderer)) {
                group.Remove(renderer);
            }
        }
        bool needsDepth, needsOverlay;
        bool renderingHighlighted = HasAny(out needsDepth, out needsOverlay);
        enabled = renderingHighlighted;        
    }

    public static void Highlight_Renderer (Renderer renderer, int highlightGroupIndex) {
       if (instance == null) return;
       instance.HighlightRenderer(renderer, highlightGroupIndex);
    }
    public static void UnHighlight_Renderer(Renderer renderer) {
        if (instance == null) return;
        instance.UnHighlightRenderer(renderer);
    }
    public static void Highlight_Renderers (List<Renderer> renderers, int highlightGroupIndex) {
       if (instance == null) return;
       instance.HighlightRenderers(renderers, highlightGroupIndex);
    }
    public static void UnHighlight_Renderers(List<Renderer> renderers) {
        if (instance == null) return;
        instance.UnHighlightRenderers(renderers);
    }
    public static void AddHighlightGroup (Color color, SortingType sortingType) {
        if (instance == null) return;
        instance.highlightGroups.Add(new HighlightGroup(color, sortingType));
    }


    void StageAll (int stagedLayer) {
        for (int i = 0; i <highlightGroups.Count; i++) {
            highlightGroups[i].Stage(stagedLayer);
        }
    }
    void UnstageAll () {
        for (int i = 0; i < highlightGroups.Count; i++) {
            highlightGroups[i].Unstage();
        }
    }



    bool HasAny(out bool needsDepth, out bool needsOverlay) {
        needsDepth = needsOverlay = false;
        for (int i = 0; i < highlightGroups.Count; i++) {
            HighlightGroup group = highlightGroups[i];
            if (group.renderers.Count > 0) {
                if (group.sortingType == SortingType.DepthFilter) needsDepth = true;
                else needsOverlay = true;
            }
            if (needsDepth && needsOverlay) return true;
        }
        return needsDepth || needsOverlay;
    }


    
    

    // we need this for the depth mask
    void RenderSceneWithoutHighlights(int stagedLayer) {

        //clear before we start renedering
        TempCam.clearFlags = CameraClearFlags.Color;  
        
        //cull all highlighted renderers
        TempCam.cullingMask = ~(1<<stagedLayer);

        Shader.SetGlobalColor(_Color, Color.clear);
        
        StageAll (stagedLayer);
        TempCam.RenderWithShader(DrawSimple,"");
        UnstageAll();

        //cull eveyrthing but highlighted renderers
        TempCam.cullingMask = 1<<stagedLayer;
    }

    void RenderLoop (int stagedLayer, SortingType sortType, bool clear, bool useGroupColor, Color overrideColor) {
        if (!useGroupColor) {
            Shader.SetGlobalColor(_Color, overrideColor);
        }

        bool clearedCamera = false;
            
        for (int i = 0; i < highlightGroups.Count; i++) {
            HighlightGroup group = highlightGroups[i];
            if (group.sortingType == sortType) {
                
                if (clear) {
                    TempCam.clearFlags = clearedCamera || !clear ? CameraClearFlags.Nothing : CameraClearFlags.Color;
                }
                
                clearedCamera = true;
                    
                if (useGroupColor) {
                    Shader.SetGlobalColor(_Color, group.highlightColor);
                }

                group.Stage(stagedLayer);
                TempCam.RenderWithShader(DrawSimple,"");
                group.Unstage();
            }
        }
    }

    

 
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        bool needsDepth, needsOverlay;
        bool renderingHighlighted = HasAny(out needsDepth, out needsOverlay);

        if (!renderingHighlighted) {
            // Graphics.Blit(source, destination);
            return;
        }
        
        int w = source.width;
        int h = source.height;

        //make the temporary rendertexture
        InitializeTemporaryCamera();

        RenderTexture cameraTarget = RenderTexture.GetTemporary(w, h, needsDepth ? 24 : 0, RenderTextureFormat.Default);
        //set the camera's target texture when rendering
        TempCam.targetTexture = cameraTarget;
        
        RenderTexture blurredTarget = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.Default);
        
        RenderTexture finalDepthTestedHighlighted = null;
        if (needsDepth) {
            finalDepthTestedHighlighted = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.Default);
            
            // draw the scene without highlighted objects to mask out the depth tested
            // highlights we draw later
            RenderSceneWithoutHighlights(stagedLayer);
            
            // render only the highlited objects that are depth tested
            // no camera clearint yet, we need the depth buffer from the last pass
            RenderLoop(stagedLayer, SortingType.DepthFilter, false, true, Color.magenta);
            
            // blur the render texture we've been drawing to with the tempCamera
            BlurUtils.BlurImage(cameraTarget, blurredTarget, downsample, blurSize, blurIterations);
        
            // make a mask to remove the inside to make it an outline
            // render all outlined objects again wihtout any occlusion (this render loop clears the camera)
            // to filter out all possible glow on overlapping objects
            RenderLoop(stagedLayer, SortingType.DepthFilter, true, false, Color.white);
            
            // now mask out the insides (as well as the alpha)
            MaskOutInsides (blurredTarget, cameraTarget, finalDepthTestedHighlighted, 1);
        }

        //do overlay highlights
        RenderTexture finalOverlayedHighLighted = null;
        if (needsOverlay) {
            finalOverlayedHighLighted = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.Default);
            
            RenderLoop(stagedLayer, SortingType.Overlay, true, true, Color.magenta);
            
            BlurUtils.BlurImage(cameraTarget, blurredTarget, downsample, blurSize, blurIterations);
            
            // mask out the insides, except for alpha (maskAlphaSubtractMult == 0)
            // we need the alpha of the overlay inside to cancel out any 
            // highlight within it that's been drawn by the depth tested passes
            MaskOutInsides (blurredTarget, cameraTarget, finalOverlayedHighLighted, 0);
        }

        // the final pass
        Post_Mat.SetTexture(_AddHighlight, finalDepthTestedHighlighted != null ? finalDepthTestedHighlighted : defaultBlackTexture);
        
        Post_Mat.SetTexture(_AddOverlay, finalOverlayedHighLighted != null ? finalOverlayedHighLighted : defaultBlackTexture);
        
        // we need to pass in the original overlay mask to cancel out the alpha
        // after it cancels out any overlapped depth tested highlights
        // the mask is contained in the camera target
        Post_Mat.SetTexture(_OverlayMask, finalOverlayedHighLighted != null ? cameraTarget : defaultBlackTexture);
        
        Post_Mat.SetVector(_Intensity_Heaviness_OverlayAlphaHelper, new Vector3(intensity, heaviness, overlayAlphaHelper));
        
        Graphics.Blit(source, destination, Post_Mat, finalPass);

        // release memory
        RenderTexture.ReleaseTemporary(cameraTarget);
        RenderTexture.ReleaseTemporary(blurredTarget);    
        
        if (finalDepthTestedHighlighted != null) RenderTexture.ReleaseTemporary(finalDepthTestedHighlighted);
        if (finalOverlayedHighLighted != null) RenderTexture.ReleaseTemporary(finalOverlayedHighLighted);
    }
}