using UnityEngine;
using System;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class DistanceBlur : MonoBehaviour {
	[NonSerialized] Material material;
	const int cocPass = 0;
    const int preFilterPass = 1;
    const int bokehPass = 2;
    const int postFilterPass = 3;
    const int combinePass = 4;
    [Range(.1f, 20f)] public float bokehRadius = 5f;
    [Range(0.1f, 100f)] public float startDistance = 10f;    
    [Range(0.1f, 100f)] public float fadeRange = 3f;
    [Range(0.001f, 10)] public float fadeSteepness = 1;

    public bool smallKernel = true;
    public bool debugVisuals = false;
	void OnRenderImage (RenderTexture source, RenderTexture destination) {
		if (material == null) {
			material = new Material(Shader.Find("Hidden/DistanceBlur"));
			material.hideFlags = HideFlags.HideAndDontSave;
		}

        int width = source.width / 2;
		int height = source.height / 2;
		RenderTextureFormat format = source.format;
		RenderTexture dof0 = RenderTexture.GetTemporary(width, height, 0, format);
		RenderTexture dof1 = RenderTexture.GetTemporary(width, height, 0, format);
        RenderTexture coc = RenderTexture.GetTemporary(
			source.width, source.height, 0,
			RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear
		);
        if (smallKernel) {
            material.EnableKeyword("BOKEH_KERNEL_SMALL");
        }
        else {
            material.DisableKeyword("BOKEH_KERNEL_SMALL");
        }
        if (debugVisuals) {
            material.EnableKeyword("DEBUG_VISUAL");
        }
        else {
            material.DisableKeyword("DEBUG_VISUAL");
        }

        material.SetFloat("_COCSteepness", fadeSteepness);
        material.SetFloat("_BokehRadius", bokehRadius);
        material.SetFloat("_FocusDistance", startDistance);
		material.SetFloat("_FocusRange", fadeRange);
        material.SetTexture("_DoFTex", dof0);
        material.SetTexture("_CoCTex", coc);

		Graphics.Blit(source, coc, material, cocPass);
        Graphics.Blit(source, dof0, material, preFilterPass);
		Graphics.Blit(dof0, dof1, material, bokehPass);
        Graphics.Blit(dof1, dof0, material, postFilterPass);
        Graphics.Blit(source, destination, material, combinePass);

		RenderTexture.ReleaseTemporary(coc);
        RenderTexture.ReleaseTemporary(dof0);
		RenderTexture.ReleaseTemporary(dof1);
	}
}