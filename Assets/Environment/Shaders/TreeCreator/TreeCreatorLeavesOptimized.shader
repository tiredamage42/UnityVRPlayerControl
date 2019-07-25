Shader "CustomEnvironment/Tree/Tree Creator Leaves Optimized" {
    Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
        _Cutoff ("Alpha cutoff", Range(0,1)) = 0.3
        _MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
        _BumpSpecMap ("Normalmap (GA) Spec (R) Shadow Offset (B)", 2D) = "bump" {}
        _HueVariation ("Hue Variation", Color) = (1.0,0.5,0.0,0.1)
        
        [Header(Bark Wind)]
        _Bark_Wind_Speed_Range ("Speed Range", Vector) = (.2, .2, 0, 0)
        _Bark_Wind_Frequency_Range ("Frequency Range", Vector) = (.1, .1, 0, 0)
        _Bark_Wind_Scale_Min ("Scale Min", Vector) = (50, 0, 25, 0)
        _Bark_Wind_Scale_Max ("Scale Max", Vector) = (100, 0, 50, 0)
        _Bark_Wind_Height_Range ("Tree Height Range", Vector) = (25, 25, 0, 0)
        _Bark_Wind_Height_Steepness_Range ("Tree Height Steepness Range", Vector) = (2, 1, 0, 0)
        
        [Header(Leaf Wind)]
        _Leaf_Wind_Speed_Range ("Speed Range", Vector) = (1, 1, 0, 0)
        _Leaf_Wind_Frequency_Range ("Frequency Range", Vector) = (20, 20, 0, 0)
        _Leaf_Wind_Scale_Min ("Scale Min", Vector) = (1, 2, 1, 0)
        _Leaf_Wind_Scale_Max ("Scale Max", Vector) = (1, 2, 1, 0)
        
    }
    SubShader {
        Cull Off
        Pass {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma multi_compile_fwdbase
            #include "TreeCreator.cginc"
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
            #pragma multi_compile_instancing
            #pragma multi_compile_fwdadd_fullshadows
            #include "TreeCreator.cginc"
            ENDCG
        }

        Pass {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_instancing
            #define SHADOWCASTER
            #include "TreeCreator.cginc"
            ENDCG
        }
    }
}
