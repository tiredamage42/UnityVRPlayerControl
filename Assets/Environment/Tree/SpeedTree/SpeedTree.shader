// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Nature/SpeedTree_Custom"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _HueVariation ("Hue Variation", Color) = (1.0,0.5,0.0,0.1)
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.333
        [MaterialEnum(Off,0,Front,1,Back,2)] _Cull ("Cull", Int) = 2
        [MaterialEnum(None,0,Fastest,1,Fast,2,Better,3,Best,4,Palm,5)] _WindQuality ("Wind Quality", Range(0,5)) = 0
    }

    SubShader
    {
        Cull [_Cull]

        CGPROGRAM

            #pragma surface surf Lambert vertex:SpeedTreeVert fullforwardshadows nolightmap nodynlightmap nodirlightmap nometa nolppv noshadowmask interpolateview halfasview addshadow
            #pragma fragmentoption ARB_precision_hint_fastest
            
            // #pragma target 3.0
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
                #include "SpeedTreeCommon_Custom.cginc"

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
                    UNITY_APPLY_DITHER_CROSSFADE(i.pos.xy);
                    SHADOW_CASTER_FRAGMENT(i)
                }
            ENDCG
        }
    }
    CustomEditor "SpeedTreeMaterialInspector"
}
