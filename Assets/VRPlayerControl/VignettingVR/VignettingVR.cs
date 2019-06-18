using UnityEngine;

[RequireComponent(typeof(Camera))]
public class VignettingVR : MonoBehaviour
{
    Color vignetteColor = Color.black;
    public void SetIntensity(float intensity) {
        vignettingIntensity = intensity;
        if (vignettingIntensity == 0) {
            enabled = false;
        }
        else {
            enabled = true;
        }
    }
    public void SetColor (Color newColor) {
        vignetteColor = newColor;
    }
    
    const float VIGNETTING_DEPTH = 10f;
    float vignettingIntensity = .5f;
    Vector4[] viewportSpaceOffset = new Vector4[2];
    Camera currentCamera;

    static int _vignetteViewportSpaceOffset = Shader.PropertyToID("_vignetteViewportSpaceOffset");
    static int _VignetteColor = Shader.PropertyToID("_VignetteColor");
    

    private Material material;
    [HideInInspector] public Shader m_Shader;
    void Start()
    {
        currentCamera = GetComponent<Camera>();
        material = new Material(m_Shader);
        material.hideFlags = HideFlags.HideAndDontSave;
        viewportSpaceOffset = new Vector4[2];
        SetIntensity(0);
    }

    Matrix4x4 GetWorldToClipMatrix(Camera.StereoscopicEye eye)
    {
        // combine view and projection matrix
        return currentCamera.GetStereoProjectionMatrix(eye) * currentCamera.GetStereoViewMatrix(eye);  
    }

    static Vector4 NormalizeScreenSpaceCords(Vector4 coords)
    {
        //normalize viewport coords from [-1, 1] [0, 1]
        //we need to do this, since we are refereing to screenspace UV coords in 
        return (coords * 0.5f) + Vector4.one * 0.5f;
    }

    Vector4 GetViewportSpaceOffset(Vector3 vignetteCenterWorldSpace, Camera.StereoscopicEye eye) {
        Matrix4x4 worldToClipSpace = GetWorldToClipMatrix(eye);
        
        // get world-space position as clip-space position 
        Vector4 viewportSpaceOffset =  worldToClipSpace.MultiplyPoint(vignetteCenterWorldSpace);
        // convert clip-space [-1,1] to viewport space [0,1]. also negate the offset.
        viewportSpaceOffset = -NormalizeScreenSpaceCords(viewportSpaceOffset);
        return viewportSpaceOffset;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Vector3 vignetteCenterWorldSpace = transform.position + transform.forward * VIGNETTING_DEPTH;
        
        viewportSpaceOffset[0] = GetViewportSpaceOffset(vignetteCenterWorldSpace, Camera.StereoscopicEye.Left);
        viewportSpaceOffset[1] = GetViewportSpaceOffset(vignetteCenterWorldSpace, Camera.StereoscopicEye.Right);

        // pass values to shader
        material.SetVectorArray(_vignetteViewportSpaceOffset, viewportSpaceOffset);
                    
        material.SetColor(_VignetteColor, new Color(vignetteColor.r, vignetteColor.g, vignetteColor.b, vignettingIntensity));
        
        Graphics.Blit(source, destination, material);
        
    }
}