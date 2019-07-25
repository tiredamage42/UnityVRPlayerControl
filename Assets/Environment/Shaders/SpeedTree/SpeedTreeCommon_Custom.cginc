#ifndef SPEEDTREE_COMMON_INCLUDED
#define SPEEDTREE_COMMON_INCLUDED


#ifdef EFFECT_BUMP
    #define BUMP_MAP
#endif
#if defined(GEOM_TYPE_FROND) || defined(GEOM_TYPE_LEAF) || defined(GEOM_TYPE_FACING_LEAF)
    #define SPEEDTREE_ALPHATEST
    fixed _Cutoff;
#endif

#define TREE_WIND


#include "../CustomWind.cginc"
#include "../Environment.cginc"
#include "../ShaderHelp.cginc"

        
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

sampler2D _MainTex;
sampler2D _BumpMap;
fixed _BumpScale;
fixed4 _Color;
// float4 _MainTex_ST;


struct v2f {
    UNITY_POSITION(pos);
    UNITY_VERTEX_OUTPUT_STEREO

#if !defined(SHADOWCASTER) || defined(SPEEDTREE_ALPHATEST)
    float4 uv : TEXCOORD0; 
#endif

#ifndef SHADOWCASTER
    fixed4 color : TEXCOORD7;
    MY_LIGHTING_COORDS(1, 2, 3, 4, 5, 6)
#else 
    V2F_SHADOW_CASTER_NOPOS(1)                       
#endif
                

};


#ifndef SHADOWCASTER

    v2f vert(SpeedTreeVB v)    
    {

        INITIALIZE_FRAGMENT_IN(v2f, o, v)      
        o.uv.xy = v.texcoord.xy;
        o.color = _Color;
        o.color.rgb *= v.color.r;

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
        #ifdef SPEEDTREE_ALPHATEST
            clip(tex.a - _Cutoff);
        #endif
        fixed3 Albedo = tex.rgb * IN.color.rgb;
        AddHueVariation(Albedo, fixed4(_HueVariation.rgb, IN.uv.w));
        fixed3 Normal = UnpackNormalWithScale(tex2D(_BumpMap, IN.uv.xy), _BumpScale);
        FINISH_FRAGMENT_CALC(IN, Albedo, Normal, IN.uv.z)
    }

#else

    v2f vert(SpeedTreeVB v)
    {
        UNITY_SETUP_INSTANCE_ID(v);
        v2f o = (v2f)0;
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
        #ifdef SPEEDTREE_ALPHATEST
            o.uv.xy = v.texcoord.xy;
        #endif
        OffsetSpeedTreeVertex(v);
        TRANSFER_SHADOW_CASTER_NOPOS(o, v.vertex, v.normal)    
        return o;
    }

    fixed frag(v2f i) : SV_Target
    {
        #ifdef SPEEDTREE_ALPHATEST
            clip(tex2D(_MainTex, i.uv).a - _Cutoff);
        #endif
        SHADOW_CASTER_FRAGMENT(i)
    }

#endif


#endif // SPEEDTREE_COMMON_INCLUDED
