Shader "VR/Optimized Bump Diffuse"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _BumpMap ("Normalmap", 2D) = "bump" {}
        _BumpScale ("Bump Scale", Range(0,50)) = 2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_instancing
            #pragma multi_compile_fog
            #pragma multi_compile_fwdbase
            #include "OptimizedBumpDiffuse.cginc"
            ENDCG
        }
        Pass {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardAdd" }
            ZWrite Off Blend One One
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_instancing
            #pragma multi_compile_fog
            #pragma multi_compile_fwdadd_fullshadows
            #include "OptimizedBumpDiffuse.cginc"
            ENDCG
        }
        Pass {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On ZTest LEqual //Cull Off
            CGPROGRAM
            #pragma vertex vertexSHADOW
            #pragma fragment fragSHADOW
            #pragma fragmentoption ARB_precision_hint_fastest   
            #pragma multi_compile_shadowcaster      
            #pragma target 2.0
                    
            #define SHADOWCASTER
            #include "ShaderHelp.cginc"
            ENDCG
        } 
    }
}