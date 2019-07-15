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
    CGINCLUDE


    #define TREE_WIND
    #include "../CustomWind.cginc"
    #include "UnityCG.cginc"
            

    struct SpeedTreeVB
    {
        fixed4 vertex       : POSITION;
        fixed4 tangent      : TANGENT;
        fixed3 normal       : NORMAL;
        fixed4 texcoord     : TEXCOORD0;
        fixed4 color         : COLOR;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };


    void OffsetSpeedTreeVertex(inout SpeedTreeVB data)
    {
        #if defined(GEOM_TYPE_LEAF)
            data.vertex.xyz = WaveLeaf (data.vertex.xyz);
        #endif
        data.vertex.xyz = WaveBranch (data.vertex.xyz);
    }

    ENDCG
    
    SubShader
    {
        Cull [_Cull]

        CGPROGRAM

        #pragma surface surf Lambert vertex:SpeedTreeVert fullforwardshadows nolightmap nodynlightmap nodirlightmap nometa nolppv noshadowmask interpolateview halfasview addshadow
        #pragma fragmentoption ARB_precision_hint_fastest
        
        #pragma shader_feature GEOM_TYPE_BRANCH GEOM_TYPE_FROND GEOM_TYPE_LEAF GEOM_TYPE_MESH
        #pragma shader_feature EFFECT_BUMP
        #define USEFRAG
        #include "SpeedTreeCommon_Custom.cginc"
        
        void SpeedTreeVert(inout SpeedTreeVB IN, out Input OUT)
        {
            UNITY_INITIALIZE_OUTPUT(Input, OUT);

            OUT.mainTexUV = IN.texcoord.xy;
            OUT.color = _Color;
            OUT.color.rgb *= IN.color.r; // ambient occlusion factor
            
            fixed hueVariationAmount = frac(unity_ObjectToWorld[0].w + unity_ObjectToWorld[1].w + unity_ObjectToWorld[2].w);
            hueVariationAmount += frac(IN.vertex.x + IN.normal.y + IN.normal.x) * 0.5 - 0.3;
            OUT.HueVariationAmount = saturate(hueVariationAmount * _HueVariation.a);
        
            OffsetSpeedTreeVertex(IN);
        }

        void surf(Input IN, inout SurfaceOutput OUT)
        {
            SpeedTreeFrag(IN, OUT);    
        }
        ENDCG

        Pass
        {
            Tags { "LightMode" = "ShadowCaster" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma shader_feature GEOM_TYPE_BRANCH GEOM_TYPE_FROND GEOM_TYPE_LEAF GEOM_TYPE_MESH
            
            #pragma multi_compile_shadowcaster

            struct v2f
            {
                V2F_SHADOW_CASTER;
                #ifdef SPEEDTREE_ALPHATEST
                    fixed2 uv : TEXCOORD1;
                #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(SpeedTreeVB v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                #ifdef SPEEDTREE_ALPHATEST
                    o.uv = v.texcoord.xy;
                #endif
                OffsetSpeedTreeVertex(v);
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                #ifdef SPEEDTREE_ALPHATEST
                    clip(tex2D(_MainTex, i.uv).a * _Color.a - _Cutoff);
                #endif
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    CustomEditor "SpeedTreeMaterialInspector"
}
