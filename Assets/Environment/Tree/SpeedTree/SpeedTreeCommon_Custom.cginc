#ifndef SPEEDTREE_COMMON_INCLUDED
#define SPEEDTREE_COMMON_INCLUDED

#include "UnityCG.cginc"

// Define Input structure
struct Input
{
    fixed4 color;
    fixed3 interpolator1;
};

// Define uniforms

#define mainTexUV interpolator1.xy
sampler2D _MainTex;

#if defined(GEOM_TYPE_FROND) || defined(GEOM_TYPE_LEAF) || defined(GEOM_TYPE_FACING_LEAF)
    #define SPEEDTREE_ALPHATEST
    fixed _Cutoff;
#endif

#define HueVariationAmount interpolator1.z
fixed4 _HueVariation;

#if defined(EFFECT_BUMP)
    sampler2D _BumpMap;
#endif

fixed4 _Color;

void SpeedTreeFrag(Input IN, inout SurfaceOutput OUT)
{
    fixed4 diffuseColor = tex2D(_MainTex, IN.mainTexUV);

    OUT.Alpha = diffuseColor.a * _Color.a;
    #ifdef SPEEDTREE_ALPHATEST
        clip(OUT.Alpha - _Cutoff);
    #endif

    fixed3 shiftedColor = lerp(diffuseColor.rgb, _HueVariation.rgb, IN.HueVariationAmount);
    fixed maxBase = max(diffuseColor.r, max(diffuseColor.g, diffuseColor.b));
    fixed newMaxBase = max(shiftedColor.r, max(shiftedColor.g, shiftedColor.b));
    maxBase /= newMaxBase;
    maxBase = maxBase * 0.5 + 0.5;
    // preserve vibrance
    shiftedColor.rgb *= maxBase;
    diffuseColor.rgb = saturate(shiftedColor);
    

    OUT.Albedo = diffuseColor.rgb * IN.color.rgb;

    #if defined(EFFECT_BUMP)
        OUT.Normal = UnpackNormal(tex2D(_BumpMap, IN.mainTexUV));
    #endif
}

#endif // SPEEDTREE_COMMON_INCLUDED
