#ifndef OPTIMIZEDBUMPDIFFUSE_INCLUDED
#define OPTIMIZEDBUMPDIFFUSE_INCLUDED

#define BUMP_MAP
#include "ShaderHelp.cginc"

sampler2D _MainTex;
sampler2D _BumpMap;
fixed _BumpScale;

struct v2f {
    UNITY_POSITION(pos);
    float3 uv : TEXCOORD0; 
    MY_LIGHTING_COORDS(1, 2, 3, 4, 5, 6)
};

float4 _MainTex_ST;

// vertex shader
v2f vert (appdata_full v) {
    INITIALIZE_FRAGMENT_IN(v2f, o, v)      
    o.pos = UnityObjectToClipPos(v.vertex);
    o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
    FINISH_VERTEX_CALC(o, v.vertex, v.normal, v.tangent.xyz, v.tangent.w, o.pos, o.uv.z)
    return o;
}

fixed4 frag (v2f IN) : SV_Target {
    UNITY_SETUP_INSTANCE_ID(IN);
    fixed3 Albedo = tex2D (_MainTex, IN.uv.xy).rgb;
    fixed3 Normal = UnpackNormalWithScale(tex2D(_BumpMap, IN.uv.xy), _BumpScale);
    FINISH_FRAGMENT_CALC(IN, Albedo, Normal, IN.uv.z)
}

#endif 