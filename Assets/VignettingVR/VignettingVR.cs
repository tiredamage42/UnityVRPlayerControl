// http://www.julianheinken.com/
using UnityEngine;
using System.Collections;

/// <summary>
/// Post-Effect for Vignetting in VR.
/// </summary>
[RequireComponent(typeof(Camera))]
// [ExecuteInEditMode]
public class VignettingVR : MonoBehaviour
{
    // const settings
    const string DEBUG_VIGNETTING_MASK = "DEBUG_SHOW_VIGNETTE";
    const float VIGNETTING_DEPTH = 10f;
    //---------------------------------------
    public bool vignettingEnable = true;
    public float vignettingIntensity;
    
    public bool debugMode;
    Vector3 vignetteWorldSpaceDirection;
    Vector4[] viewportSpaceOffset = new Vector4[2];
    Camera currentCamera;

    private Material material;
    [HideInInspector]
    public Shader m_Shader;
    void Start()
    {
        vignetteWorldSpaceDirection = transform.forward;
        currentCamera = GetComponent<Camera>();
        material = new Material(m_Shader);
        material.hideFlags = HideFlags.HideAndDontSave;
        viewportSpaceOffset = new Vector4[2];
    }

    Matrix4x4 GetWorldToClipMatrix(Camera.StereoscopicEye eye)
    {
        return currentCamera.GetStereoProjectionMatrix(eye) * currentCamera.GetStereoViewMatrix(eye);  // combine view and projection matrix
    }

    static Vector4 NormalizeScreenSpaceCords(Vector4 coords)
    {
        //normalize viewport coords from [-1, 1] [0, 1]
        //we need to do this, since we are refereing to screenspace UV coords in 
        return (coords * 0.5f) + Vector4.one * 0.5f;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        material.DisableKeyword(DEBUG_VIGNETTING_MASK);

        if (debugMode)
        {
            material.EnableKeyword(DEBUG_VIGNETTING_MASK);
        }

        Vector3 currentCameraForward = transform.forward;

        //translate vignetting direction to worldspace position
        Vector3 vignetteCenterWorldSpace = transform.position + currentCameraForward * VIGNETTING_DEPTH;
        Matrix4x4 worldToClipSpace_left = GetWorldToClipMatrix(Camera.StereoscopicEye.Left);
        viewportSpaceOffset[0] =  worldToClipSpace_left.MultiplyPoint(vignetteCenterWorldSpace);// get world-space position as clip-space position 
        viewportSpaceOffset[0] = -NormalizeScreenSpaceCords(viewportSpaceOffset[0]);// convert clip-space [-1,1] to viewport space [0,1]. also negate the offset.

        Matrix4x4 worldToClipSpace_right = GetWorldToClipMatrix(Camera.StereoscopicEye.Right);
        viewportSpaceOffset[1] = worldToClipSpace_right.MultiplyPoint(vignetteCenterWorldSpace);// get world-space position as clip-space position 
        viewportSpaceOffset[1] = -NormalizeScreenSpaceCords(viewportSpaceOffset[1]); // convert clip-space [-1,1] to viewport space [0,1]. also negate the offset.

        // pass values to shader
        material.SetVectorArray("_vignetteViewportSpaceOffset", viewportSpaceOffset);
        material.SetFloat("_vignettingIntensity", vignettingEnable ? vignettingIntensity * 0.1f : 0f);
        
        Graphics.Blit(source, destination, material);
    }
}