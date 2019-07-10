// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Nature/Tree Creator Leaves Optimized Custom" {
Properties {
    _Color ("Main Color", Color) = (1,1,1,1)
    _Cutoff ("Alpha cutoff", Range(0,1)) = 0.3
    _MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
    _BumpSpecMap ("Normalmap (GA) Spec (R) Shadow Offset (B)", 2D) = "bump" {}
    
}

CGINCLUDE

#include "Lighting.cginc"
#include "TerrainEngine_Custom.cginc"
fixed4 _Color;
sampler2D _MainTex;

void TreeVertLeaf (inout appdata_full v)
{
    v.vertex = AnimateVertex (v.vertex,v.normal, fixed4(v.color.xy, v.texcoord1.xy));
    v.normal = normalize(v.normal);
    
    v.color.rgb = _Color.rgb;
    // v.tangent.xyz = normalize(v.tangent.xyz);
}

ENDCG

SubShader {
    Cull Off
    CGPROGRAM


#pragma surface surf Lambert alphatest:_Cutoff vertex:TreeVertLeaf fullforwardshadows nolightmap nodynlightmap nodirlightmap nometa nolppv noshadowmask interpolateview halfasview addshadow
#pragma fragmentoption ARB_precision_hint_fastest

sampler2D _BumpSpecMap;

struct Input {
    fixed2 uv_MainTex;
    fixed4 color : COLOR; // color.a = AO
};

void surf (Input IN, inout SurfaceOutput o) {

    fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
    o.Albedo = c.rgb * IN.color.rgb * IN.color.a;

    o.Alpha = c.a;
    fixed4 norspc = tex2D (_BumpSpecMap, IN.uv_MainTex);
    o.Specular = norspc.r;
    o.Normal = UnpackNormalDXT5nm(norspc);
}
ENDCG

    // Pass to render object as a shadow caster
    Pass {
        Name "ShadowCaster"
        Tags { "LightMode" = "ShadowCaster" }

        CGPROGRAM
        #pragma vertex vert_surf
        #pragma fragment frag_surf
        #pragma fragmentoption ARB_precision_hint_fastest
        #pragma multi_compile_shadowcaster
        
        struct Input {
            fixed2 uv_MainTex;
        };

        struct v2f_surf {
            V2F_SHADOW_CASTER;
            fixed2 hip_pack0 : TEXCOORD1;
            UNITY_VERTEX_OUTPUT_STEREO
        };
        
        fixed4 _MainTex_ST;

        v2f_surf vert_surf (appdata_full v) {
            v2f_surf o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
            TreeVertLeaf (v);
            o.hip_pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
            TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
            return o;
        }
        fixed _Cutoff;
        fixed4 frag_surf (v2f_surf IN) : SV_Target {
            fixed alpha = tex2D(_MainTex, IN.hip_pack0.xy).a;
            clip (alpha - _Cutoff);
            SHADOW_CASTER_FRAGMENT(IN)
        }
        ENDCG
    }

}
}
