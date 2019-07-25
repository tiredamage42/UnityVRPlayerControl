// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Custom Environment/Tree/SpeedTree"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _HueVariation ("Hue Variation", Color) = (1.0,0.5,0.0,0.1)
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.333
        [MaterialEnum(Off,0,Front,1,Back,2)] _Cull ("Cull", Int) = 2
        
        [Header(Bark Wind)]
        _Bark_Wind_Speed_Range ("Speed Range", Vector) = (.2, .2, 0, 0)
        _Bark_Wind_Frequency_Range ("Frequency Range", Vector) = (.1, .1, 0, 0)
        _Bark_Wind_Scale_Min ("Scale Min", Vector) = (50, 0, 25, 0)
        _Bark_Wind_Scale_Max ("Scale Max", Vector) = (100, 0, 50, 0)
        _Bark_Wind_Height_Range ("Tree Height Range", Vector) = (50, 50, 0, 0)
        _Bark_Wind_Height_Steepness_Range ("Tree Height Steepness Range", Vector) = (2, 1, 0, 0)
        
        [Header(Leaf Wind)]
        _Leaf_Wind_Speed_Range ("Speed Range", Vector) = (1, 1, 0, 0)
        _Leaf_Wind_Frequency_Range ("Frequency Range", Vector) = (32, 32, 0, 0)
        _Leaf_Wind_Scale_Min ("Scale Min", Vector) = (1, 2, 1, 0)
        _Leaf_Wind_Scale_Max ("Scale Max", Vector) = (1, 2, 1, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull [_Cull]
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
            #pragma shader_feature GEOM_TYPE_BRANCH GEOM_TYPE_FROND GEOM_TYPE_LEAF GEOM_TYPE_MESH
            #pragma shader_feature EFFECT_BUMP
            #include "SpeedTreeCommon_Custom.cginc"
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
            #pragma shader_feature GEOM_TYPE_BRANCH GEOM_TYPE_FROND GEOM_TYPE_LEAF GEOM_TYPE_MESH
            #pragma shader_feature EFFECT_BUMP
            #include "SpeedTreeCommon_Custom.cginc"
            ENDCG
        }
        Pass
        {

            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            
            ZWrite On ZTest LEqual
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma shader_feature GEOM_TYPE_BRANCH GEOM_TYPE_FROND GEOM_TYPE_LEAF GEOM_TYPE_MESH
            #pragma multi_compile_shadowcaster
            #define SHADOWCASTER
            #include "SpeedTreeCommon_Custom.cginc"
            ENDCG
        }
    }
    CustomEditor "SpeedTreeMaterialInspector"
}
