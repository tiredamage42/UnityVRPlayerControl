Shader "Custom Environment/Terrain" {
    Properties { }

    SubShader {
        Tags {
            "Queue" = "Geometry-100"
            "RenderType" = "Opaque"
        }
        Pass {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_fog
            #pragma multi_compile_fwdbase
            #include "TerrainInclude.cginc"
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
            #pragma multi_compile_fog 
            #pragma multi_compile_fwdadd_fullshadows
            #include "TerrainInclude.cginc"
            ENDCG
        }
        Pass {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On ZTest LEqual // Cull Off
            CGPROGRAM
            #pragma vertex vertexSHADOW
            #pragma fragment fragSHADOW
            #pragma fragmentoption ARB_precision_hint_fastest   
            #pragma multi_compile_shadowcaster                  
            #define SHADOWCASTER
            #define NO_ROTATION_SCALE
            #include "../ShaderHelp.cginc"
            ENDCG
        }
    }
    Dependency "BaseMapShader"    = "Hidden/TerrainEngine/Splatmap/Standard-Base_Custom"
}