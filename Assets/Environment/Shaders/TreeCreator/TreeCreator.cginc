#ifndef TREE_CREATOR_INCLUDED
#define TREE_CREATOR_INCLUDED


        
#define TREE_WIND

#define BUMP_MAP
#include "../CustomWind.cginc"
#include "../Environment.cginc"
#include "../ShaderHelp.cginc"


struct v2f {
    UNITY_POSITION(pos);
    UNITY_VERTEX_OUTPUT_STEREO

#if !defined(SHADOWCASTER) || !defined(BARK)
    float4 uv : TEXCOORD0; 
#endif

#ifndef SHADOWCASTER
    fixed4 color : TEXCOORD7;
    MY_LIGHTING_COORDS(1, 2, 3, 4, 5, 6)
#else 

    V2F_SHADOW_CASTER_NOPOS(1)                       

#endif
                

};

fixed4 _Color;
sampler2D _MainTex;
sampler2D _BumpSpecMap;
fixed _Cutoff;
        
void OffsetSpeedTreeVertex(inout appdata_full data)
{
    #if !defined(BARK)
        data.vertex.xyz = WaveLeaf (data.vertex.xyz);
    #endif
    data.vertex.xyz = WaveBranch (data.vertex.xyz);
}


#ifndef SHADOWCASTER

  v2f vert(appdata_full v)    
    {

        INITIALIZE_FRAGMENT_IN(v2f, o, v)      
        o.uv.xy = v.texcoord.xy;
        o.color.rgb = _Color.rgb;
        o.color.a = v.color.a;

        fixed hueVariationAmount = frac(unity_ObjectToWorld[0].w + unity_ObjectToWorld[1].w + unity_ObjectToWorld[2].w);
        hueVariationAmount += frac(v.vertex.x + v.normal.y + v.normal.x) * 0.5 - 0.3;
        
        o.uv.w = saturate(hueVariationAmount * _HueVariation.a);
        
        
        OffsetSpeedTreeVertex(v);
        
        o.pos = UnityObjectToClipPos(v.vertex);

        FINISH_VERTEX_CALC(o, v.vertex, v.normal, v.tangent.xyz, v.tangent.w, o.pos, o.uv.z)
        return o;
    }



    fixed4 frag (v2f IN) : SV_Target {

        fixed4 tex = tex2D (_MainTex, IN.uv.xy);
        #ifndef BARK
            clip(tex.a - _Cutoff);
        #endif
        fixed3 Albedo = tex.rgb * IN.color.rgb * IN.color.a;
        AddHueVariation(Albedo, fixed4(_HueVariation.rgb, IN.uv.w));
        
        // fixed3 Normal = UnpackNormalWithScale(tex2D(_BumpSpecMap, IN.uv.xy), 1);//_BumpScale);
        fixed3 Normal = UnpackNormalDXT5nm(tex2D(_BumpSpecMap, IN.uv.xy));
        
        FINISH_FRAGMENT_CALC(IN, Albedo, Normal, IN.uv.z)
    }

#else

v2f vert(appdata_full v)
{
    UNITY_SETUP_INSTANCE_ID(v);
    v2f o = (v2f)0;
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    #ifndef BARK
    o.uv.xy = v.texcoord.xy;
    #endif
    OffsetSpeedTreeVertex(v);

    TRANSFER_SHADOW_CASTER_NOPOS(o, v.vertex, v.normal)    
    return o;
}

fixed frag(v2f i) : SV_Target
{
    #ifndef BARK
    clip(tex2D(_MainTex, i.uv).a - _Cutoff);
    #endif

    SHADOW_CASTER_FRAGMENT(i)
}

#endif


#endif // SPEEDTREE_COMMON_INCLUDED
