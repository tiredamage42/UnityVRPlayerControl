using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlurUtils : MonoBehaviour
{
    static Material _material;
    static Material material {
        get {
            if (_material == null) {
                _material = new Material(Shader.Find("Hidden/_FastBlur"));
            }
            return _material;
        }
    }
    static int _Parameter = Shader.PropertyToID("_Parameter");

    public static void BlurImage (RenderTexture source, RenderTexture destination, int downsample, float size, int iterations) {
        if (iterations < 1) {
            Debug.LogWarning("Need at least one blur iteration... setting to one");
        }
        float widthMod = 1.0f / (1.0f * (1<<downsample));

        material.SetVector (_Parameter, new Vector4 (size * widthMod, -size * widthMod, 0.0f, 0.0f));
        source.filterMode = FilterMode.Bilinear;

        int rtW = source.width >> downsample;
        int rtH = source.height >> downsample;

        // downsample
        RenderTexture rt = RenderTexture.GetTemporary (rtW, rtH, 0, source.format);
        rt.filterMode = FilterMode.Bilinear;
        Graphics.Blit (source, rt, material, 0);

        RenderTexture rt2 = RenderTexture.GetTemporary (rtW, rtH, 0, source.format);
        rt2.filterMode = FilterMode.Bilinear;
            
        for(int i = 0; i < iterations; i++) {
            float iterationOffs = (i*1.0f);
            material.SetVector (_Parameter, new Vector4 (size * widthMod + iterationOffs, -size * widthMod - iterationOffs, 0.0f, 0.0f));
            // vertical blur
            Graphics.Blit (rt, rt2, material, 1);
            // horizontal blur
            Graphics.Blit (rt2, i == iterations - 1 ? destination : rt, material, 2);
        }

        RenderTexture.ReleaseTemporary (rt);
        RenderTexture.ReleaseTemporary (rt2);
        
    }
}
