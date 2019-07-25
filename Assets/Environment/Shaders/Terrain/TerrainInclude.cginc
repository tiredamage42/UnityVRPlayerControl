#ifndef TERRAIN_INCLUDED
#define TERRAIN_INCLUDED

#define NO_ROTATION_SCALE
#define BUMP_MAP
#include "../ShaderHelp.cginc"

 struct v2f {
    UNITY_POSITION(pos);
    float3 uv : TEXCOORD0; 
    MY_LIGHTING_COORDS(1, 2, 3, 4, 5, 6)
};

v2f vert (appdata_full v) {

    INITIALIZE_FRAGMENT_IN(v2f, o, v)
            
    v.tangent = fixed4(cross(v.normal, float3(0,0,1)), -1);
    o.uv.xy = v.texcoord.xy;
    o.pos = UnityObjectToClipPos(v.vertex);
    
    FINISH_VERTEX_CALC(o, v.vertex, v.normal, v.tangent.xyz, v.tangent.w, o.pos, o.uv.z)
    return o;
}

sampler2D _Control;
sampler2D _Splat0, _Splat1, _Splat2, _Splat3;
float4 _Splat0_ST, _Splat1_ST, _Splat2_ST, _Splat3_ST;
sampler2D _Normal0, _Normal1, _Normal2, _Normal3;
float _NormalScale0, _NormalScale1, _NormalScale2, _NormalScale3;

fixed4 frag (v2f IN) : SV_Target {
    
    _NormalScale0 = _NormalScale1 = _NormalScale2 = _NormalScale3 = 1;

    half4 splat_control = tex2D(_Control, IN.uv.xy);
    float2 uvSplat0 = TRANSFORM_TEX(IN.uv.xy, _Splat0);
    float2 uvSplat1 = TRANSFORM_TEX(IN.uv.xy, _Splat1);
    float2 uvSplat2 = TRANSFORM_TEX(IN.uv.xy, _Splat2);
    float2 uvSplat3 = TRANSFORM_TEX(IN.uv.xy, _Splat3);
    
    fixed3 Albedo = 0;
    Albedo += splat_control.r * tex2D(_Splat0, uvSplat0).rgb;
    Albedo += splat_control.g * tex2D(_Splat1, uvSplat1).rgb;
    Albedo += splat_control.b * tex2D(_Splat2, uvSplat2).rgb;
    Albedo += splat_control.a * tex2D(_Splat3, uvSplat3).rgb;

    fixed3 Normal = 0;
    Normal += splat_control.r * UnpackNormalWithScale(tex2D(_Normal0, uvSplat0), _NormalScale0);
    Normal += splat_control.g * UnpackNormalWithScale(tex2D(_Normal1, uvSplat1), _NormalScale1);
    Normal += splat_control.b * UnpackNormalWithScale(tex2D(_Normal2, uvSplat2), _NormalScale2);
    Normal += splat_control.a * UnpackNormalWithScale(tex2D(_Normal3, uvSplat3), _NormalScale3);
    Normal.z += 1e-5; // to avoid nan after normalizing

    FINISH_FRAGMENT_CALC(IN, Albedo, Normal, IN.uv.z)
}

#endif 
