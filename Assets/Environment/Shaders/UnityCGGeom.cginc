// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

#ifndef UNITY_CG_GEOM_INCLUDED
#define UNITY_CG_GEOM_INCLUDED

#define UNITY_PI            3.14159265359f
#define UNITY_TWO_PI        6.28318530718f
#define UNITY_FOUR_PI       12.56637061436f
#define UNITY_INV_PI        0.31830988618f
#define UNITY_INV_TWO_PI    0.15915494309f
#define UNITY_INV_FOUR_PI   0.07957747155f
#define UNITY_HALF_PI       1.57079632679f
#define UNITY_INV_HALF_PI   0.636619772367f

// Should SH (light probe / ambient) calculations be performed?
// - When both static and dynamic lightmaps are available, no SH evaluation is performed
// - When static and dynamic lightmaps are not available, SH evaluation is always performed
// - For low level LODs, static lightmap and real-time GI from light probes can be combined together
// - Passes that don't do ambient (additive, shadowcaster etc.) should not do SH either.
#define UNITY_SHOULD_SAMPLE_SH (defined(LIGHTPROBE_SH) && !defined(UNITY_PASS_FORWARDADD) && !defined(UNITY_PASS_PREPASSBASE) && !defined(UNITY_PASS_SHADOWCASTER) && !defined(UNITY_PASS_META))

#include "UnityShaderVariables.cginc"
#include "UnityShaderUtilities.cginc"
#include "UnityInstancing.cginc"


#ifdef UNITY_COLORSPACE_GAMMA
#define unity_ColorSpaceGrey fixed4(0.5, 0.5, 0.5, 0.5)
#define unity_ColorSpaceDouble fixed4(2.0, 2.0, 2.0, 2.0)
#define unity_ColorSpaceDielectricSpec fixed4(0.220916301, 0.220916301, 0.220916301, 1.0 - 0.220916301)
#define unity_ColorSpaceLuminance fixed4(0.22, 0.707, 0.071, 0.0) // Legacy: alpha is set to 0.0 to specify gamma mode
#else // Linear values
#define unity_ColorSpaceGrey fixed4(0.214041144, 0.214041144, 0.214041144, 0.5)
#define unity_ColorSpaceDouble fixed4(4.59479380, 4.59479380, 4.59479380, 2.0)
#define unity_ColorSpaceDielectricSpec fixed4(0.04, 0.04, 0.04, 1.0 - 0.04) // standard dielectric reflectivity coef at incident angle (= 4%)
#define unity_ColorSpaceLuminance fixed4(0.0396819152, 0.458021790, 0.00609653955, 1.0) // Legacy: alpha is set to 1.0 to specify linear mode
#endif

// -------------------------------------------------------------------
//  helper functions and macros used in many standard shaders


#if defined (DIRECTIONAL) || defined (DIRECTIONAL_COOKIE) || defined (POINT) || defined (SPOT) || defined (POINT_NOATT) || defined (POINT_COOKIE)
#define USING_LIGHT_MULTI_COMPILE
#endif

#if defined(SHADER_API_D3D11) || defined(SHADER_API_PSSL) || defined(SHADER_API_METAL) || defined(SHADER_API_GLCORE) || defined(SHADER_API_GLES3) || defined(SHADER_API_VULKAN) || defined(SHADER_API_SWITCH) // D3D11, D3D12, XB1, PS4, iOS, macOS, tvOS, glcore, gles3, webgl2.0, Switch
// Real-support for depth-format cube shadow map.
#define SHADOWS_CUBE_IN_DEPTH_TEX
#endif

#define SCALED_NORMAL v.normal


// These constants must be kept in sync with RGBMRanges.h
#define LIGHTMAP_RGBM_SCALE 5.0
#define EMISSIVE_RGBM_SCALE 97.0

struct appdata_base {
    fixed4 vertex : POSITION;
    fixed3 normal : NORMAL;
    fixed4 texcoord : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct appdata_tan {
    fixed4 vertex : POSITION;
    fixed4 tangent : TANGENT;
    fixed3 normal : NORMAL;
    fixed4 texcoord : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct appdata_full {
    fixed4 vertex : POSITION;
    fixed4 tangent : TANGENT;
    fixed3 normal : NORMAL;
    fixed4 texcoord : TEXCOORD0;
    fixed4 texcoord1 : TEXCOORD1;
    fixed4 texcoord2 : TEXCOORD2;
    fixed4 texcoord3 : TEXCOORD3;
    fixed4 color : COLOR;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

// Legacy for compatibility with existing shaders
inline bool IsGammaSpace()
{
    #ifdef UNITY_COLORSPACE_GAMMA
        return true;
    #else
        return false;
    #endif
}

inline fixed GammaToLinearSpaceExact (fixed value)
{
    if (value <= 0.04045F)
        return value / 12.92F;
    else if (value < 1.0F)
        return pow((value + 0.055F)/1.055F, 2.4F);
    else
        return pow(value, 2.2F);
}

inline fixed3 GammaToLinearSpace (fixed3 sRGB)
{
    // Approximate version from http://chilliant.blogspot.com.au/2012/08/srgb-approximations-for-hlsl.html?m=1
    return sRGB * (sRGB * (sRGB * 0.305306011h + 0.682171111h) + 0.012522878h);

    // Precise version, useful for debugging.
    //return fixed3(GammaToLinearSpaceExact(sRGB.r), GammaToLinearSpaceExact(sRGB.g), GammaToLinearSpaceExact(sRGB.b));
}

inline fixed LinearToGammaSpaceExact (fixed value)
{
    if (value <= 0.0)
        return 0.0;
    else if (value <= 0.0031308)
        return 12.92 * value;
    else if (value < 1.0)
        return 1.055 * pow(value, 0.4166667) - 0.055;
    else
        return pow(value, 0.45454545);
}

inline fixed3 LinearToGammaSpace (fixed3 linRGB)
{
    linRGB = max(linRGB, fixed3(0, 0, 0));
    // An almost-perfect approximation from http://chilliant.blogspot.com.au/2012/08/srgb-approximations-for-hlsl.html?m=1
    return max(1.055 * pow(linRGB, 0.416666667) - 0.055, 0);

    // Exact version, useful for debugging.
    //return fixed3(LinearToGammaSpaceExact(linRGB.r), LinearToGammaSpaceExact(linRGB.g), LinearToGammaSpaceExact(linRGB.b));
}

// Tranforms position from world to homogenous space
inline fixed4 UnityWorldToClipPos( in fixed3 pos )
{
    return mul(UNITY_MATRIX_VP, fixed4(pos, 1.0));
}

// Tranforms position from view to homogenous space
inline fixed4 UnityViewToClipPos( in fixed3 pos )
{
    return mul(UNITY_MATRIX_P, fixed4(pos, 1.0));
}

// Tranforms position from object to camera space
inline fixed3 UnityObjectToViewPos( in fixed3 pos )
{
    return mul(UNITY_MATRIX_V, mul(unity_ObjectToWorld, fixed4(pos, 1.0))).xyz;
}
inline fixed3 UnityObjectToViewPos(fixed4 pos) // overload for fixed4; avoids "implicit truncation" warning for existing shaders
{
    return UnityObjectToViewPos(pos.xyz);
}

// Tranforms position from world to camera space
inline fixed3 UnityWorldToViewPos( in fixed3 pos )
{
    return mul(UNITY_MATRIX_V, fixed4(pos, 1.0)).xyz;
}

// Transforms direction from object to world space
inline fixed3 UnityObjectToWorldDir( in fixed3 dir )
{
    return normalize(mul((fixed3x3)unity_ObjectToWorld, dir));
}

// Transforms direction from world to object space
inline fixed3 UnityWorldToObjectDir( in fixed3 dir )
{
    return normalize(mul((fixed3x3)unity_WorldToObject, dir));
}

// Transforms normal from object to world space
inline fixed3 UnityObjectToWorldNormal( in fixed3 norm )
{
#ifdef UNITY_ASSUME_UNIFORM_SCALING
    return UnityObjectToWorldDir(norm);
#else
    // mul(IT_M, norm) => mul(norm, I_M) => {dot(norm, I_M.col0), dot(norm, I_M.col1), dot(norm, I_M.col2)}
    return normalize(mul(norm, (fixed3x3)unity_WorldToObject));
#endif
}

// Computes world space light direction, from world space position
inline fixed3 UnityWorldSpaceLightDir( in fixed3 worldPos )
{
    #ifndef USING_LIGHT_MULTI_COMPILE
        return _WorldSpaceLightPos0.xyz - worldPos * _WorldSpaceLightPos0.w;
    #else
        #ifndef USING_DIRECTIONAL_LIGHT
        return _WorldSpaceLightPos0.xyz - worldPos;
        #else
        return _WorldSpaceLightPos0.xyz;
        #endif
    #endif
}

// Computes world space light direction, from object space position
// *Legacy* Please use UnityWorldSpaceLightDir instead
inline fixed3 WorldSpaceLightDir( in fixed4 localPos )
{
    fixed3 worldPos = mul(unity_ObjectToWorld, localPos).xyz;
    return UnityWorldSpaceLightDir(worldPos);
}

// Computes object space light direction
inline fixed3 ObjSpaceLightDir( in fixed4 v )
{
    fixed3 objSpaceLightPos = mul(unity_WorldToObject, _WorldSpaceLightPos0).xyz;
    #ifndef USING_LIGHT_MULTI_COMPILE
        return objSpaceLightPos.xyz - v.xyz * _WorldSpaceLightPos0.w;
    #else
        #ifndef USING_DIRECTIONAL_LIGHT
        return objSpaceLightPos.xyz - v.xyz;
        #else
        return objSpaceLightPos.xyz;
        #endif
    #endif
}

// Computes world space view direction, from object space position
inline fixed3 UnityWorldSpaceViewDir( in fixed3 worldPos )
{
    return _WorldSpaceCameraPos.xyz - worldPos;
}

// Computes world space view direction, from object space position
// *Legacy* Please use UnityWorldSpaceViewDir instead
inline fixed3 WorldSpaceViewDir( in fixed4 localPos )
{
    fixed3 worldPos = mul(unity_ObjectToWorld, localPos).xyz;
    return UnityWorldSpaceViewDir(worldPos);
}

// Computes object space view direction
inline fixed3 ObjSpaceViewDir( in fixed4 v )
{
    fixed3 objSpaceCameraPos = mul(unity_WorldToObject, fixed4(_WorldSpaceCameraPos.xyz, 1)).xyz;
    return objSpaceCameraPos - v.xyz;
}

// Declares 3x3 matrix 'rotation', filled with tangent space basis
#define TANGENT_SPACE_ROTATION \
    fixed3 binormal = cross( normalize(v.normal), normalize(v.tangent.xyz) ) * v.tangent.w; \
    fixed3x3 rotation = fixed3x3( v.tangent.xyz, binormal, v.normal )



// Used in ForwardBase pass: Calculates diffuse lighting from 4 point lights, with data packed in a special way.
fixed3 Shade4PointLights (
    fixed4 lightPosX, fixed4 lightPosY, fixed4 lightPosZ,
    fixed3 lightColor0, fixed3 lightColor1, fixed3 lightColor2, fixed3 lightColor3,
    fixed4 lightAttenSq,
    fixed3 pos, fixed3 normal)
{
    // to light vectors
    fixed4 toLightX = lightPosX - pos.x;
    fixed4 toLightY = lightPosY - pos.y;
    fixed4 toLightZ = lightPosZ - pos.z;
    // squared lengths
    fixed4 lengthSq = 0;
    lengthSq += toLightX * toLightX;
    lengthSq += toLightY * toLightY;
    lengthSq += toLightZ * toLightZ;
    // don't produce NaNs if some vertex position overlaps with the light
    lengthSq = max(lengthSq, 0.000001);

    // NdotL
    fixed4 ndotl = 0;
    ndotl += toLightX * normal.x;
    ndotl += toLightY * normal.y;
    ndotl += toLightZ * normal.z;
    // correct NdotL
    fixed4 corr = rsqrt(lengthSq);
    ndotl = max (fixed4(0,0,0,0), ndotl * corr);
    // attenuation
    fixed4 atten = 1.0 / (1.0 + lengthSq * lightAttenSq);
    fixed4 diff = ndotl * atten;
    // final color
    fixed3 col = 0;
    col += lightColor0 * diff.x;
    col += lightColor1 * diff.y;
    col += lightColor2 * diff.z;
    col += lightColor3 * diff.w;
    return col;
}

// Used in Vertex pass: Calculates diffuse lighting from lightCount lights. Specifying true to spotLight is more expensive
// to calculate but lights are treated as spot lights otherwise they are treated as point lights.
fixed3 ShadeVertexLightsFull (fixed4 vertex, fixed3 normal, int lightCount, bool spotLight)
{
    fixed3 viewpos = UnityObjectToViewPos (vertex.xyz);
    fixed3 viewN = normalize (mul ((fixed3x3)UNITY_MATRIX_IT_MV, normal));

    fixed3 lightColor = UNITY_LIGHTMODEL_AMBIENT.xyz;
    for (int i = 0; i < lightCount; i++) {
        fixed3 toLight = unity_LightPosition[i].xyz - viewpos.xyz * unity_LightPosition[i].w;
        fixed lengthSq = dot(toLight, toLight);

        // don't produce NaNs if some vertex position overlaps with the light
        lengthSq = max(lengthSq, 0.000001);

        toLight *= rsqrt(lengthSq);

        fixed atten = 1.0 / (1.0 + lengthSq * unity_LightAtten[i].z);
        if (spotLight)
        {
            fixed rho = max (0, dot(toLight, unity_SpotDirection[i].xyz));
            fixed spotAtt = (rho - unity_LightAtten[i].x) * unity_LightAtten[i].y;
            atten *= saturate(spotAtt);
        }

        fixed diff = max (0, dot (viewN, toLight));
        lightColor += unity_LightColor[i].rgb * (diff * atten);
    }
    return lightColor;
}

fixed3 ShadeVertexLights (fixed4 vertex, fixed3 normal)
{
    return ShadeVertexLightsFull (vertex, normal, 4, false);
}

// normal should be normalized, w=1.0
fixed3 SHEvalLinearL0L1 (fixed4 normal)
{
    fixed3 x;

    // Linear (L1) + constant (L0) polynomial terms
    x.r = dot(unity_SHAr,normal);
    x.g = dot(unity_SHAg,normal);
    x.b = dot(unity_SHAb,normal);

    return x;
}

// normal should be normalized, w=1.0
fixed3 SHEvalLinearL2 (fixed4 normal)
{
    fixed3 x1, x2;
    // 4 of the quadratic (L2) polynomials
    fixed4 vB = normal.xyzz * normal.yzzx;
    x1.r = dot(unity_SHBr,vB);
    x1.g = dot(unity_SHBg,vB);
    x1.b = dot(unity_SHBb,vB);

    // Final (5th) quadratic (L2) polynomial
    fixed vC = normal.x*normal.x - normal.y*normal.y;
    x2 = unity_SHC.rgb * vC;

    return x1 + x2;
}

// normal should be normalized, w=1.0
// output in active color space
fixed3 ShadeSH9 (fixed4 normal)
{
    // Linear + constant polynomial terms
    fixed3 res = SHEvalLinearL0L1 (normal);

    // Quadratic polynomials
    res += SHEvalLinearL2 (normal);

#   ifdef UNITY_COLORSPACE_GAMMA
        res = LinearToGammaSpace (res);
#   endif

    return res;
}

// OBSOLETE: for backwards compatibility with 5.0
fixed3 ShadeSH3Order(fixed4 normal)
{
    // Quadratic polynomials
    fixed3 res = SHEvalLinearL2 (normal);

#   ifdef UNITY_COLORSPACE_GAMMA
        res = LinearToGammaSpace (res);
#   endif

    return res;
}

#if UNITY_LIGHT_PROBE_PROXY_VOLUME

// normal should be normalized, w=1.0
fixed3 SHEvalLinearL0L1_SampleProbeVolume (fixed4 normal, fixed3 worldPos)
{
    const fixed transformToLocal = unity_ProbeVolumeParams.y;
    const fixed texelSizeX = unity_ProbeVolumeParams.z;

    //The SH coefficients textures and probe occlusion are packed into 1 atlas.
    //-------------------------
    //| ShR | ShG | ShB | Occ |
    //-------------------------

    fixed3 position = (transformToLocal == 1.0f) ? mul(unity_ProbeVolumeWorldToObject, fixed4(worldPos, 1.0)).xyz : worldPos;
    fixed3 texCoord = (position - unity_ProbeVolumeMin.xyz) * unity_ProbeVolumeSizeInv.xyz;
    texCoord.x = texCoord.x * 0.25f;

    // We need to compute proper X coordinate to sample.
    // Clamp the coordinate otherwize we'll have leaking between RGB coefficients
    fixed texCoordX = clamp(texCoord.x, 0.5f * texelSizeX, 0.25f - 0.5f * texelSizeX);

    // sampler state comes from SHr (all SH textures share the same sampler)
    texCoord.x = texCoordX;
    fixed4 SHAr = UNITY_SAMPLE_TEX3D_SAMPLER(unity_ProbeVolumeSH, unity_ProbeVolumeSH, texCoord);

    texCoord.x = texCoordX + 0.25f;
    fixed4 SHAg = UNITY_SAMPLE_TEX3D_SAMPLER(unity_ProbeVolumeSH, unity_ProbeVolumeSH, texCoord);

    texCoord.x = texCoordX + 0.5f;
    fixed4 SHAb = UNITY_SAMPLE_TEX3D_SAMPLER(unity_ProbeVolumeSH, unity_ProbeVolumeSH, texCoord);

    // Linear + constant polynomial terms
    fixed3 x1;
    x1.r = dot(SHAr, normal);
    x1.g = dot(SHAg, normal);
    x1.b = dot(SHAb, normal);

    return x1;
}
#endif

// normal should be normalized, w=1.0
fixed3 ShadeSH12Order (fixed4 normal)
{
    // Linear + constant polynomial terms
    fixed3 res = SHEvalLinearL0L1 (normal);

#   ifdef UNITY_COLORSPACE_GAMMA
        res = LinearToGammaSpace (res);
#   endif

    return res;
}

// Transforms 2D UV by scale/bias property
#define TRANSFORM_TEX(tex,name) (tex.xy * name##_ST.xy + name##_ST.zw)

// Deprecated. Used to transform 4D UV by a fixed function texture matrix. Now just returns the passed UV.
#define TRANSFORM_UV(idx) v.texcoord.xy



struct v2f_vertex_lit {
    fixed2 uv   : TEXCOORD0;
    fixed4 diff : COLOR0;
    fixed4 spec : COLOR1;
};

inline fixed4 VertexLight( v2f_vertex_lit i, sampler2D mainTex )
{
    fixed4 texcol = tex2D( mainTex, i.uv );
    fixed4 c;
    c.xyz = ( texcol.xyz * i.diff.xyz + i.spec.xyz * texcol.a );
    c.w = texcol.w * i.diff.w;
    return c;
}


// Calculates UV offset for parallax bump mapping
inline fixed2 ParallaxOffset( fixed h, fixed height, fixed3 viewDir )
{
    h = h * height - height/2.0;
    fixed3 v = normalize(viewDir);
    v.z += 0.42;
    return h * (v.xy / v.z);
}

// Converts color to luminance (grayscale)
inline fixed Luminance(fixed3 rgb)
{
    return dot(rgb, unity_ColorSpaceLuminance.rgb);
}

// Convert rgb to luminance
// with rgb in linear space with sRGB primaries and D65 white point
fixed LinearRgbToLuminance(fixed3 linearRgb)
{
    return dot(linearRgb, fixed3(0.2126729f,  0.7151522f, 0.0721750f));
}

fixed4 UnityEncodeRGBM (fixed3 color, fixed maxRGBM)
{
    fixed kOneOverRGBMMaxRange = 1.0 / maxRGBM;
    const fixed kMinMultiplier = 2.0 * 1e-2;

    fixed3 rgb = color * kOneOverRGBMMaxRange;
    fixed alpha = max(max(rgb.r, rgb.g), max(rgb.b, kMinMultiplier));
    alpha = ceil(alpha * 255.0) / 255.0;

    // Division-by-zero warning from d3d9, so make compiler happy.
    alpha = max(alpha, kMinMultiplier);

    return fixed4(rgb / alpha, alpha);
}

// Decodes HDR textures
// handles dLDR, RGBM formats
inline fixed3 DecodeHDR (fixed4 data, fixed4 decodeInstructions)
{
    // Take into account texture alpha if decodeInstructions.w is true(the alpha value affects the RGB channels)
    fixed alpha = decodeInstructions.w * (data.a - 1.0) + 1.0;

    // If Linear mode is not supported we can skip exponent part
    #if defined(UNITY_COLORSPACE_GAMMA)
        return (decodeInstructions.x * alpha) * data.rgb;
    #else
    #   if defined(UNITY_USE_NATIVE_HDR)
            return decodeInstructions.x * data.rgb; // Multiplier for future HDRI relative to absolute conversion.
    #   else
            return (decodeInstructions.x * pow(alpha, decodeInstructions.y)) * data.rgb;
    #   endif
    #endif
}

// Decodes HDR textures
// handles dLDR, RGBM formats
// Called by DecodeLightmap when UNITY_NO_RGBM is not defined.
inline fixed3 DecodeLightmapRGBM (fixed4 data, fixed4 decodeInstructions)
{
    // If Linear mode is not supported we can skip exponent part
    #if defined(UNITY_COLORSPACE_GAMMA)
    # if defined(UNITY_FORCE_LINEAR_READ_FOR_RGBM)
        return (decodeInstructions.x * data.a) * sqrt(data.rgb);
    # else
        return (decodeInstructions.x * data.a) * data.rgb;
    # endif
    #else
        return (decodeInstructions.x * pow(data.a, decodeInstructions.y)) * data.rgb;
    #endif
}

// Decodes doubleLDR encoded lightmaps.
inline fixed3 DecodeLightmapDoubleLDR( fixed4 color, fixed4 decodeInstructions)
{
    // decodeInstructions.x contains 2.0 when gamma color space is used or pow(2.0, 2.2) = 4.59 when linear color space is used on mobile platforms
    return decodeInstructions.x * color.rgb;
}

inline fixed3 DecodeLightmap( fixed4 color, fixed4 decodeInstructions)
{
#if defined(UNITY_LIGHTMAP_DLDR_ENCODING)
    return DecodeLightmapDoubleLDR(color, decodeInstructions);
#elif defined(UNITY_LIGHTMAP_RGBM_ENCODING)
    return DecodeLightmapRGBM(color, decodeInstructions);
#else //defined(UNITY_LIGHTMAP_FULL_HDR)
    return color.rgb;
#endif
}

fixed4 unity_Lightmap_HDR;

inline fixed3 DecodeLightmap( fixed4 color )
{
    return DecodeLightmap( color, unity_Lightmap_HDR );
}

fixed4 unity_DynamicLightmap_HDR;

// Decodes Enlighten RGBM encoded lightmaps
// NOTE: Enlighten dynamic texture RGBM format is _different_ from standard Unity HDR textures
// (such as Baked Lightmaps, Reflection Probes and IBL images)
// Instead Enlighten provides RGBM texture in _Linear_ color space with _different_ exponent.
// WARNING: 3 pow operations, might be very expensive for mobiles!
inline fixed3 DecodeRealtimeLightmap( fixed4 color )
{
    //@TODO: Temporary until Geomerics gives us an API to convert lightmaps to RGBM in gamma space on the enlighten thread before we upload the textures.
#if defined(UNITY_FORCE_LINEAR_READ_FOR_RGBM)
    return pow ((unity_DynamicLightmap_HDR.x * color.a) * sqrt(color.rgb), unity_DynamicLightmap_HDR.y);
#else
    return pow ((unity_DynamicLightmap_HDR.x * color.a) * color.rgb, unity_DynamicLightmap_HDR.y);
#endif
}

inline fixed3 DecodeDirectionalLightmap (fixed3 color, fixed4 dirTex, fixed3 normalWorld)
{
    // In directional (non-specular) mode Enlighten bakes dominant light direction
    // in a way, that using it for half Lambert and then dividing by a "rebalancing coefficient"
    // gives a result close to plain diffuse response lightmaps, but normalmapped.

    // Note that dir is not unit length on purpose. Its length is "directionality", like
    // for the directional specular lightmaps.

    fixed halfLambert = dot(normalWorld, dirTex.xyz - 0.5) + 0.5;

    return color * halfLambert / max(1e-4h, dirTex.w);
}

// Encoding/decoding [0..1) fixeds into 8 bit/channel RGBA. Note that 1.0 will not be encoded properly.
inline fixed4 EncodefixedRGBA( fixed v )
{
    fixed4 kEncodeMul = fixed4(1.0, 255.0, 65025.0, 16581375.0);
    fixed kEncodeBit = 1.0/255.0;
    fixed4 enc = kEncodeMul * v;
    enc = frac (enc);
    enc -= enc.yzww * kEncodeBit;
    return enc;
}
inline fixed DecodefixedRGBA( fixed4 enc )
{
    fixed4 kDecodeDot = fixed4(1.0, 1/255.0, 1/65025.0, 1/16581375.0);
    return dot( enc, kDecodeDot );
}

// Encoding/decoding [0..1) fixeds into 8 bit/channel RG. Note that 1.0 will not be encoded properly.
inline fixed2 EncodefixedRG( fixed v )
{
    fixed2 kEncodeMul = fixed2(1.0, 255.0);
    fixed kEncodeBit = 1.0/255.0;
    fixed2 enc = kEncodeMul * v;
    enc = frac (enc);
    enc.x -= enc.y * kEncodeBit;
    return enc;
}
inline fixed DecodefixedRG( fixed2 enc )
{
    fixed2 kDecodeDot = fixed2(1.0, 1/255.0);
    return dot( enc, kDecodeDot );
}


// Encoding/decoding view space normals into 2D 0..1 vector
inline fixed2 EncodeViewNormalStereo( fixed3 n )
{
    fixed kScale = 1.7777;
    fixed2 enc;
    enc = n.xy / (n.z+1);
    enc /= kScale;
    enc = enc*0.5+0.5;
    return enc;
}
inline fixed3 DecodeViewNormalStereo( fixed4 enc4 )
{
    fixed kScale = 1.7777;
    fixed3 nn = enc4.xyz*fixed3(2*kScale,2*kScale,0) + fixed3(-kScale,-kScale,1);
    fixed g = 2.0 / dot(nn.xyz,nn.xyz);
    fixed3 n;
    n.xy = g*nn.xy;
    n.z = g-1;
    return n;
}

inline fixed4 EncodeDepthNormal( fixed depth, fixed3 normal )
{
    fixed4 enc;
    enc.xy = EncodeViewNormalStereo (normal);
    enc.zw = EncodefixedRG (depth);
    return enc;
}

inline void DecodeDepthNormal( fixed4 enc, out fixed depth, out fixed3 normal )
{
    depth = DecodefixedRG (enc.zw);
    normal = DecodeViewNormalStereo (enc);
}

inline fixed3 UnpackNormalDXT5nm (fixed4 packednormal)
{
    fixed3 normal;
    normal.xy = packednormal.wy * 2 - 1;
    normal.z = sqrt(1 - saturate(dot(normal.xy, normal.xy)));
    return normal;
}

// Unpack normal as DXT5nm (1, y, 1, x) or BC5 (x, y, 0, 1)
// Note neutral texture like "bump" is (0, 0, 1, 1) to work with both plain RGB normal and DXT5nm/BC5
fixed3 UnpackNormalmapRGorAG(fixed4 packednormal)
{
    // This do the trick
   packednormal.x *= packednormal.w;

    fixed3 normal;
    normal.xy = packednormal.xy * 2 - 1;
    normal.z = sqrt(1 - saturate(dot(normal.xy, normal.xy)));
    return normal;
}
inline fixed3 UnpackNormal(fixed4 packednormal)
{
#if defined(UNITY_NO_DXT5nm)
    return packednormal.xyz * 2 - 1;
#else
    return UnpackNormalmapRGorAG(packednormal);
#endif
}

fixed3 UnpackNormalWithScale(fixed4 packednormal, fixed scale)
{
#ifndef UNITY_NO_DXT5nm
    // Unpack normal as DXT5nm (1, y, 1, x) or BC5 (x, y, 0, 1)
    // Note neutral texture like "bump" is (0, 0, 1, 1) to work with both plain RGB normal and DXT5nm/BC5
    packednormal.x *= packednormal.w;
#endif
    fixed3 normal;
    normal.xy = (packednormal.xy * 2 - 1) * scale;
    normal.z = sqrt(1 - saturate(dot(normal.xy, normal.xy)));
    return normal;
}

// Z buffer to linear 0..1 depth
inline fixed Linear01Depth( fixed z )
{
    return 1.0 / (_ZBufferParams.x * z + _ZBufferParams.y);
}
// Z buffer to linear depth
inline fixed LinearEyeDepth( fixed z )
{
    return 1.0 / (_ZBufferParams.z * z + _ZBufferParams.w);
}


inline fixed2 UnityStereoScreenSpaceUVAdjustInternal(fixed2 uv, fixed4 scaleAndOffset)
{
    return uv.xy * scaleAndOffset.xy + scaleAndOffset.zw;
}

inline fixed4 UnityStereoScreenSpaceUVAdjustInternal(fixed4 uv, fixed4 scaleAndOffset)
{
    return fixed4(UnityStereoScreenSpaceUVAdjustInternal(uv.xy, scaleAndOffset), UnityStereoScreenSpaceUVAdjustInternal(uv.zw, scaleAndOffset));
}

#define UnityStereoScreenSpaceUVAdjust(x, y) UnityStereoScreenSpaceUVAdjustInternal(x, y)

#if defined(UNITY_SINGLE_PASS_STEREO)
fixed2 TransformStereoScreenSpaceTex(fixed2 uv, fixed w)
{
    fixed4 scaleOffset = unity_StereoScaleOffset[unity_StereoEyeIndex];
    return uv.xy * scaleOffset.xy + scaleOffset.zw * w;
}

inline fixed2 UnityStereoTransformScreenSpaceTex(fixed2 uv)
{
    return TransformStereoScreenSpaceTex(saturate(uv), 1.0);
}

inline fixed4 UnityStereoTransformScreenSpaceTex(fixed4 uv)
{
    return fixed4(UnityStereoTransformScreenSpaceTex(uv.xy), UnityStereoTransformScreenSpaceTex(uv.zw));
}
inline fixed2 UnityStereoClamp(fixed2 uv, fixed4 scaleAndOffset)
{
    return fixed2(clamp(uv.x, scaleAndOffset.z, scaleAndOffset.z + scaleAndOffset.x), uv.y);
}
#else
#define TransformStereoScreenSpaceTex(uv, w) uv
#define UnityStereoTransformScreenSpaceTex(uv) uv
#define UnityStereoClamp(uv, scaleAndOffset) uv
#endif

// Depth render texture helpers
#define DECODE_EYEDEPTH(i) LinearEyeDepth(i)
#define COMPUTE_EYEDEPTH(o) o = -UnityObjectToViewPos( v.vertex ).z
#define COMPUTE_DEPTH_01 -(UnityObjectToViewPos( v.vertex ).z * _ProjectionParams.w)
#define COMPUTE_VIEW_NORMAL normalize(mul((fixed3x3)UNITY_MATRIX_IT_MV, v.normal))

// Helpers used in image effects. Most image effects use the same
// minimal vertex shader (vert_img).

struct appdata_img
{
    fixed4 vertex : POSITION;
    fixed2 texcoord : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f_img
{
    fixed4 pos : SV_POSITION;
    fixed2 uv : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

fixed2 MultiplyUV (fixed4x4 mat, fixed2 inUV) {
    fixed4 temp = fixed4 (inUV.x, inUV.y, 0, 0);
    temp = mul (mat, temp);
    return temp.xy;
}

v2f_img vert_img( appdata_img v )
{
    v2f_img o;
    UNITY_INITIALIZE_OUTPUT(v2f_img, o);
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    o.pos = UnityObjectToClipPos (v.vertex);
    o.uv = v.texcoord;
    return o;
}

// Projected screen position helpers
#define V2F_SCREEN_TYPE fixed4

inline fixed4 ComputeNonStereoScreenPos(fixed4 pos) {
    fixed4 o = pos * 0.5f;
    o.xy = fixed2(o.x, o.y*_ProjectionParams.x) + o.w;
    o.zw = pos.zw;
    return o;
}

inline fixed4 ComputeScreenPos(fixed4 pos) {
    fixed4 o = ComputeNonStereoScreenPos(pos);
#if defined(UNITY_SINGLE_PASS_STEREO)
    o.xy = TransformStereoScreenSpaceTex(o.xy, pos.w);
#endif
    return o;
}

inline fixed4 ComputeGrabScreenPos (fixed4 pos) {
    #if UNITY_UV_STARTS_AT_TOP
    fixed scale = -1.0;
    #else
    fixed scale = 1.0;
    #endif
    fixed4 o = pos * 0.5f;
    o.xy = fixed2(o.x, o.y*scale) + o.w;
#ifdef UNITY_SINGLE_PASS_STEREO
    o.xy = TransformStereoScreenSpaceTex(o.xy, pos.w);
#endif
    o.zw = pos.zw;
    return o;
}

// snaps post-transformed position to screen pixels
inline fixed4 UnityPixelSnap (fixed4 pos)
{
    fixed2 hpc = _ScreenParams.xy * 0.5f;
#if  SHADER_API_PSSL
// sdk 4.5 splits round into v_floor_f32(x+0.5) ... sdk 5.0 uses v_rndne_f32, for compatabilty we use the 4.5 version
    fixed2 temp = ((pos.xy / pos.w) * hpc) + fixed2(0.5f,0.5f);
    fixed2 pixelPos = fixed2(__v_floor_f32(temp.x), __v_floor_f32(temp.y));
#else
    fixed2 pixelPos = round ((pos.xy / pos.w) * hpc);
#endif
    pos.xy = pixelPos / hpc * pos.w;
    return pos;
}

inline fixed2 TransformViewToProjection (fixed2 v) {
    return mul((fixed2x2)UNITY_MATRIX_P, v);
}

inline fixed3 TransformViewToProjection (fixed3 v) {
    return mul((fixed3x3)UNITY_MATRIX_P, v);
}

// Shadow caster pass helpers

fixed4 UnityEncodeCubeShadowDepth (fixed z)
{
    #ifdef UNITY_USE_RGBA_FOR_POINT_SHADOWS
    return EncodefixedRGBA (min(z, 0.999));
    #else
    return z;
    #endif
}

fixed UnityDecodeCubeShadowDepth (fixed4 vals)
{
    #ifdef UNITY_USE_RGBA_FOR_POINT_SHADOWS
    return DecodefixedRGBA (vals);
    #else
    return vals.r;
    #endif
}


fixed4 UnityClipSpaceShadowCasterPos(fixed4 vertex, fixed3 normal)
{
    fixed4 wPos = mul(unity_ObjectToWorld, vertex);

    if (unity_LightShadowBias.z != 0.0)
    {
        fixed3 wNormal = UnityObjectToWorldNormal(normal);
        fixed3 wLight = normalize(UnityWorldSpaceLightDir(wPos.xyz));

        // apply normal offset bias (inset position along the normal)
        // bias needs to be scaled by sine between normal and light direction
        // (http://the-witness.net/news/2013/09/shadow-mapping-summary-part-1/)
        //
        // unity_LightShadowBias.z contains user-specified normal offset amount
        // scaled by world space texel size.

        fixed shadowCos = dot(wNormal, wLight);
        fixed shadowSine = sqrt(1-shadowCos*shadowCos);
        fixed normalBias = unity_LightShadowBias.z * shadowSine;

        wPos.xyz -= wNormal * normalBias;
    }

    return mul(UNITY_MATRIX_VP, wPos);
}
// Legacy, not used anymore; kept around to not break existing user shaders
fixed4 UnityClipSpaceShadowCasterPos(fixed3 vertex, fixed3 normal)
{
    return UnityClipSpaceShadowCasterPos(fixed4(vertex, 1), normal);
}


fixed4 UnityApplyLinearShadowBias(fixed4 clipPos)

{
    // For point lights that support depth cube map, the bias is applied in the fragment shader sampling the shadow map.
    // This is because the legacy behaviour for point light shadow map cannot be implemented by offseting the vertex position
    // in the vertex shader generating the shadow map.
#if !(defined(SHADOWS_CUBE) && defined(SHADOWS_CUBE_IN_DEPTH_TEX))
    #if defined(UNITY_REVERSED_Z)
        // We use max/min instead of clamp to ensure proper handling of the rare case
        // where both numerator and denominator are zero and the fraction becomes NaN.
        clipPos.z += max(-1, min(unity_LightShadowBias.x / clipPos.w, 0));
    #else
        clipPos.z += saturate(unity_LightShadowBias.x/clipPos.w);
    #endif
#endif

#if defined(UNITY_REVERSED_Z)
    fixed clamped = min(clipPos.z, clipPos.w*UNITY_NEAR_CLIP_VALUE);
#else
    fixed clamped = max(clipPos.z, clipPos.w*UNITY_NEAR_CLIP_VALUE);
#endif
    clipPos.z = lerp(clipPos.z, clamped, unity_LightShadowBias.y);
    return clipPos;
}


#if defined(SHADOWS_CUBE) && !defined(SHADOWS_CUBE_IN_DEPTH_TEX)
    // Rendering into point light (cubemap) shadows
    #define V2F_SHADOW_CASTER_NOPOS fixed3 vec : TEXCOORD0;
    #define TRANSFER_SHADOW_CASTER_NOPOS_LEGACY(o,opos, vertex, worldPos) o.vec = worldPos - _LightPositionRange.xyz; opos = UnityObjectToClipPos(vertex);
    #define TRANSFER_SHADOW_CASTER_NOPOS(o,opos, vertex, normal, worldPos) o.vec = worldPos - _LightPositionRange.xyz; opos = UnityObjectToClipPos(vertex);
    #define SHADOW_CASTER_FRAGMENT(i) return UnityEncodeCubeShadowDepth ((length(i.vec) + unity_LightShadowBias.x) * _LightPositionRange.w);

#else
    // Rendering into directional or spot light shadows
    #define V2F_SHADOW_CASTER_NOPOS
    // Let embedding code know that V2F_SHADOW_CASTER_NOPOS is empty; so that it can workaround
    // empty structs that could possibly be produced.
    #define V2F_SHADOW_CASTER_NOPOS_IS_EMPTY
    
    #define TRANSFER_SHADOW_CASTER_NOPOS_LEGACY(o,opos, vertex) \
        opos = UnityObjectToClipPos(vertex); \
        opos = UnityApplyLinearShadowBias(opos);
    
    #define TRANSFER_SHADOW_CASTER_NOPOS(o,opos, vertex, normal, worldPos) \
        opos = UnityClipSpaceShadowCasterPos(vertex, normal); \
        opos = UnityApplyLinearShadowBias(opos);


    #define SHADOW_CASTER_FRAGMENT(i) return 0;
#endif

// Declare all data needed for shadow caster pass output (any shadow directions/depths/distances as needed),
// plus clip space position.
#define V2F_SHADOW_CASTER V2F_SHADOW_CASTER_NOPOS UNITY_POSITION(pos)

// Vertex shader part, with support for normal offset shadows. Requires
// position and normal to be present in the vertex input.
#define TRANSFER_SHADOW_CASTER_NORMALOFFSET(o, vertex, normal, worldPos) TRANSFER_SHADOW_CASTER_NOPOS(o,o.pos, vertex, normal, worldPos)


// Vertex shader part, legacy. No support for normal offset shadows - because
// that would require vertex normals, which might not be present in user-written shaders.
#define TRANSFER_SHADOW_CASTER(o) TRANSFER_SHADOW_CASTER_NOPOS_LEGACY(o,o.pos)


// ------------------------------------------------------------------
//  Alpha helper

#define UNITY_OPAQUE_ALPHA(outputAlpha) outputAlpha = 1.0


// ------------------------------------------------------------------
//  Fog helpers
//
//  multi_compile_fog Will compile fog variants.
//  UNITY_FOG_COORDS(texcoordindex) Declares the fog data interpolator.
//  UNITY_TRANSFER_FOG(outputStruct,clipspacePos) Outputs fog data from the vertex shader.
//  UNITY_APPLY_FOG(fogData,col) Applies fog to color "col". Automatically applies black fog when in forward-additive pass.
//  Can also use UNITY_APPLY_FOG_COLOR to supply your own fog color.

// In case someone by accident tries to compile fog code in one of the g-buffer or shadow passes:
// treat it as fog is off.
#if defined(UNITY_PASS_PREPASSBASE) || defined(UNITY_PASS_DEFERRED) || defined(UNITY_PASS_SHADOWCASTER)
#undef FOG_LINEAR
#undef FOG_EXP
#undef FOG_EXP2
#endif

#if defined(UNITY_REVERSED_Z)
    #if UNITY_REVERSED_Z == 1
        //D3d with reversed Z => z clip range is [near, 0] -> remapping to [0, far]
        //max is required to protect ourselves from near plane not being correct/meaningfull in case of oblique matrices.
        #define UNITY_Z_0_FAR_FROM_CLIPSPACE(coord) max(((1.0-(coord)/_ProjectionParams.y)*_ProjectionParams.z),0)
    #else
        //GL with reversed z => z clip range is [near, -far] -> should remap in theory but dont do it in practice to save some perf (range is close enough)
        #define UNITY_Z_0_FAR_FROM_CLIPSPACE(coord) max(-(coord), 0)
    #endif
#elif UNITY_UV_STARTS_AT_TOP
    //D3d without reversed z => z clip range is [0, far] -> nothing to do
    #define UNITY_Z_0_FAR_FROM_CLIPSPACE(coord) (coord)
#else
    //Opengl => z clip range is [-near, far] -> should remap in theory but dont do it in practice to save some perf (range is close enough)
    #define UNITY_Z_0_FAR_FROM_CLIPSPACE(coord) (coord)
#endif

#if defined(FOG_LINEAR)
    // factor = (end-z)/(end-start) = z * (-1/(end-start)) + (end/(end-start))
    #define UNITY_CALC_FOG_FACTOR_RAW(coord) fixed unityFogFactor = (coord) * unity_FogParams.z + unity_FogParams.w
#elif defined(FOG_EXP)
    // factor = exp(-density*z)
    #define UNITY_CALC_FOG_FACTOR_RAW(coord) fixed unityFogFactor = unity_FogParams.y * (coord); unityFogFactor = exp2(-unityFogFactor)
#elif defined(FOG_EXP2)
    // factor = exp(-(density*z)^2)
    #define UNITY_CALC_FOG_FACTOR_RAW(coord) fixed unityFogFactor = unity_FogParams.x * (coord); unityFogFactor = exp2(-unityFogFactor*unityFogFactor)
#else
    #define UNITY_CALC_FOG_FACTOR_RAW(coord) fixed unityFogFactor = 0.0
#endif

#define UNITY_CALC_FOG_FACTOR(coord) UNITY_CALC_FOG_FACTOR_RAW(UNITY_Z_0_FAR_FROM_CLIPSPACE(coord))

#define UNITY_FOG_COORDS_PACKED(idx, vectype) vectype fogCoord : TEXCOORD##idx;

#if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
    #define UNITY_FOG_COORDS(idx) UNITY_FOG_COORDS_PACKED(idx, fixed1)

    #if (SHADER_TARGET < 30) || defined(SHADER_API_MOBILE)
        // mobile or SM2.0: calculate fog factor per-vertex
        #define UNITY_TRANSFER_FOG(o,outpos) UNITY_CALC_FOG_FACTOR((outpos).z); o.fogCoord.x = unityFogFactor
        #define UNITY_TRANSFER_FOG_COMBINED_WITH_TSPACE(o,outpos) UNITY_CALC_FOG_FACTOR((outpos).z); o.tSpace1.y = tangentSign; o.tSpace2.y = unityFogFactor
        #define UNITY_TRANSFER_FOG_COMBINED_WITH_WORLD_POS(o,outpos) UNITY_CALC_FOG_FACTOR((outpos).z); o.worldPos.w = unityFogFactor
        #define UNITY_TRANSFER_FOG_COMBINED_WITH_EYE_VEC(o,outpos) UNITY_CALC_FOG_FACTOR((outpos).z); o.eyeVec.w = unityFogFactor
    #else
        // SM3.0 and PC/console: calculate fog distance per-vertex, and fog factor per-pixel
        #define UNITY_TRANSFER_FOG(o,outpos) o.fogCoord.x = (outpos).z
        #define UNITY_TRANSFER_FOG_COMBINED_WITH_TSPACE(o,outpos) o.tSpace2.y = (outpos).z
        #define UNITY_TRANSFER_FOG_COMBINED_WITH_WORLD_POS(o,outpos) o.worldPos.w = (outpos).z
        #define UNITY_TRANSFER_FOG_COMBINED_WITH_EYE_VEC(o,outpos) o.eyeVec.w = (outpos).z
    #endif
#else
    #define UNITY_FOG_COORDS(idx)
    #define UNITY_TRANSFER_FOG(o,outpos)
    #define UNITY_TRANSFER_FOG_COMBINED_WITH_TSPACE(o,outpos)
    #define UNITY_TRANSFER_FOG_COMBINED_WITH_WORLD_POS(o,outpos)
    #define UNITY_TRANSFER_FOG_COMBINED_WITH_EYE_VEC(o,outpos)
#endif

#define UNITY_FOG_LERP_COLOR(col,fogCol,fogFac) col.rgb = lerp((fogCol).rgb, (col).rgb, saturate(fogFac))


#if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
    #if (SHADER_TARGET < 30) || defined(SHADER_API_MOBILE)
        // mobile or SM2.0: fog factor was already calculated per-vertex, so just lerp the color
        #define UNITY_APPLY_FOG_COLOR(coord,col,fogCol) UNITY_FOG_LERP_COLOR(col,fogCol,(coord).x)
    #else
        // SM3.0 and PC/console: calculate fog factor and lerp fog color
        #define UNITY_APPLY_FOG_COLOR(coord,col,fogCol) UNITY_CALC_FOG_FACTOR((coord).x); UNITY_FOG_LERP_COLOR(col,fogCol,unityFogFactor)
    #endif
    #define UNITY_EXTRACT_FOG(name) fixed _unity_fogCoord = name.fogCoord
    #define UNITY_EXTRACT_FOG_FROM_TSPACE(name) fixed _unity_fogCoord = name.tSpace2.y
    #define UNITY_EXTRACT_FOG_FROM_WORLD_POS(name) fixed _unity_fogCoord = name.worldPos.w
    #define UNITY_EXTRACT_FOG_FROM_EYE_VEC(name) fixed _unity_fogCoord = name.eyeVec.w
#else
    #define UNITY_APPLY_FOG_COLOR(coord,col,fogCol)
    #define UNITY_EXTRACT_FOG(name)
    #define UNITY_EXTRACT_FOG_FROM_TSPACE(name)
    #define UNITY_EXTRACT_FOG_FROM_WORLD_POS(name)
    #define UNITY_EXTRACT_FOG_FROM_EYE_VEC(name)
#endif

#ifdef UNITY_PASS_FORWARDADD
    #define UNITY_APPLY_FOG(coord,col) UNITY_APPLY_FOG_COLOR(coord,col,fixed4(0,0,0,0))
#else
    #define UNITY_APPLY_FOG(coord,col) UNITY_APPLY_FOG_COLOR(coord,col,unity_FogColor)
#endif

// ------------------------------------------------------------------
//  TBN helpers
#define UNITY_EXTRACT_TBN_0(name) fixed3 _unity_tbn_0 = name.tSpace0.xyz
#define UNITY_EXTRACT_TBN_1(name) fixed3 _unity_tbn_1 = name.tSpace1.xyz
#define UNITY_EXTRACT_TBN_2(name) fixed3 _unity_tbn_2 = name.tSpace2.xyz

#define UNITY_EXTRACT_TBN(name) UNITY_EXTRACT_TBN_0(name); UNITY_EXTRACT_TBN_1(name); UNITY_EXTRACT_TBN_2(name)

#define UNITY_EXTRACT_TBN_T(name) fixed3 _unity_tangent = fixed3(name.tSpace0.x, name.tSpace1.x, name.tSpace2.x)
#define UNITY_EXTRACT_TBN_N(name) fixed3 _unity_normal = fixed3(name.tSpace0.z, name.tSpace1.z, name.tSpace2.z)
#define UNITY_EXTRACT_TBN_B(name) fixed3 _unity_binormal = cross(_unity_normal, _unity_tangent)
#define UNITY_CORRECT_TBN_B_SIGN(name) _unity_binormal *= name.tSpace1.y;
#define UNITY_RECONSTRUCT_TBN_0 fixed3 _unity_tbn_0 = fixed3(_unity_tangent.x, _unity_binormal.x, _unity_normal.x)
#define UNITY_RECONSTRUCT_TBN_1 fixed3 _unity_tbn_1 = fixed3(_unity_tangent.y, _unity_binormal.y, _unity_normal.y)
#define UNITY_RECONSTRUCT_TBN_2 fixed3 _unity_tbn_2 = fixed3(_unity_tangent.z, _unity_binormal.z, _unity_normal.z)

#if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
    #define UNITY_RECONSTRUCT_TBN(name) UNITY_EXTRACT_TBN_T(name); UNITY_EXTRACT_TBN_N(name); UNITY_EXTRACT_TBN_B(name); UNITY_CORRECT_TBN_B_SIGN(name); UNITY_RECONSTRUCT_TBN_0; UNITY_RECONSTRUCT_TBN_1; UNITY_RECONSTRUCT_TBN_2
#else
    #define UNITY_RECONSTRUCT_TBN(name) UNITY_EXTRACT_TBN(name)
#endif

//  LOD cross fade helpers
// keep all the old macros
#define UNITY_DITHER_CROSSFADE_COORDS
#define UNITY_DITHER_CROSSFADE_COORDS_IDX(idx)
#define UNITY_TRANSFER_DITHER_CROSSFADE(o,v)
#define UNITY_TRANSFER_DITHER_CROSSFADE_HPOS(o,hpos)

#ifdef LOD_FADE_CROSSFADE
    #define UNITY_APPLY_DITHER_CROSSFADE(vpos)  UnityApplyDitherCrossFade(vpos)
    sampler2D _DitherMaskLOD2D;
    void UnityApplyDitherCrossFade(fixed2 vpos)
    {
        vpos /= 4; // the dither mask texture is 4x4
        vpos.y = frac(vpos.y) * 0.0625 /* 1/16 */ + unity_LODFade.y; // quantized lod fade by 16 levels
        clip(tex2D(_DitherMaskLOD2D, vpos).a - 0.5);
    }
#else
    #define UNITY_APPLY_DITHER_CROSSFADE(vpos)
#endif


// ------------------------------------------------------------------
//  Deprecated things: these aren't used; kept here
//  just so that various existing shaders still compile, more or less.


// Note: deprecated shadow collector pass helpers
#ifdef SHADOW_COLLECTOR_PASS

#if !defined(SHADOWMAPSAMPLER_DEFINED)
UNITY_DECLARE_SHADOWMAP(_ShadowMapTexture);
#endif

// Note: V2F_SHADOW_COLLECTOR and TRANSFER_SHADOW_COLLECTOR are deprecated
#define V2F_SHADOW_COLLECTOR fixed4 pos : SV_POSITION; fixed3 _ShadowCoord0 : TEXCOORD0; fixed3 _ShadowCoord1 : TEXCOORD1; fixed3 _ShadowCoord2 : TEXCOORD2; fixed3 _ShadowCoord3 : TEXCOORD3; fixed4 _WorldPosViewZ : TEXCOORD4
#define TRANSFER_SHADOW_COLLECTOR(o)    \
    o.pos = UnityObjectToClipPos(v.vertex); \
    fixed4 wpos = mul(unity_ObjectToWorld, v.vertex); \
    o._WorldPosViewZ.xyz = wpos; \
    o._WorldPosViewZ.w = -UnityObjectToViewPos(v.vertex).z; \
    o._ShadowCoord0 = mul(unity_WorldToShadow[0], wpos).xyz; \
    o._ShadowCoord1 = mul(unity_WorldToShadow[1], wpos).xyz; \
    o._ShadowCoord2 = mul(unity_WorldToShadow[2], wpos).xyz; \
    o._ShadowCoord3 = mul(unity_WorldToShadow[3], wpos).xyz;

// Note: SAMPLE_SHADOW_COLLECTOR_SHADOW is deprecated
#define SAMPLE_SHADOW_COLLECTOR_SHADOW(coord) \
    fixed shadow = UNITY_SAMPLE_SHADOW(_ShadowMapTexture,coord); \
    shadow = _LightShadowData.r + shadow * (1-_LightShadowData.r);

// Note: COMPUTE_SHADOW_COLLECTOR_SHADOW is deprecated
#define COMPUTE_SHADOW_COLLECTOR_SHADOW(i, weights, shadowFade) \
    fixed4 coord = fixed4(i._ShadowCoord0 * weights[0] + i._ShadowCoord1 * weights[1] + i._ShadowCoord2 * weights[2] + i._ShadowCoord3 * weights[3], 1); \
    SAMPLE_SHADOW_COLLECTOR_SHADOW(coord) \
    fixed4 res; \
    res.x = saturate(shadow + shadowFade); \
    res.y = 1.0; \
    res.zw = EncodefixedRG (1 - i._WorldPosViewZ.w * _ProjectionParams.w); \
    return res;

// Note: deprecated
#if defined (SHADOWS_SPLIT_SPHERES)
#define SHADOW_COLLECTOR_FRAGMENT(i) \
    fixed3 fromCenter0 = i._WorldPosViewZ.xyz - unity_ShadowSplitSpheres[0].xyz; \
    fixed3 fromCenter1 = i._WorldPosViewZ.xyz - unity_ShadowSplitSpheres[1].xyz; \
    fixed3 fromCenter2 = i._WorldPosViewZ.xyz - unity_ShadowSplitSpheres[2].xyz; \
    fixed3 fromCenter3 = i._WorldPosViewZ.xyz - unity_ShadowSplitSpheres[3].xyz; \
    fixed4 distances2 = fixed4(dot(fromCenter0,fromCenter0), dot(fromCenter1,fromCenter1), dot(fromCenter2,fromCenter2), dot(fromCenter3,fromCenter3)); \
    fixed4 cascadeWeights = fixed4(distances2 < unity_ShadowSplitSqRadii); \
    cascadeWeights.yzw = saturate(cascadeWeights.yzw - cascadeWeights.xyz); \
    fixed sphereDist = distance(i._WorldPosViewZ.xyz, unity_ShadowFadeCenterAndType.xyz); \
    fixed shadowFade = saturate(sphereDist * _LightShadowData.z + _LightShadowData.w); \
    COMPUTE_SHADOW_COLLECTOR_SHADOW(i, cascadeWeights, shadowFade)
#else
#define SHADOW_COLLECTOR_FRAGMENT(i) \
    fixed4 viewZ = i._WorldPosViewZ.w; \
    fixed4 zNear = fixed4( viewZ >= _LightSplitsNear ); \
    fixed4 zFar = fixed4( viewZ < _LightSplitsFar ); \
    fixed4 cascadeWeights = zNear * zFar; \
    fixed shadowFade = saturate(i._WorldPosViewZ.w * _LightShadowData.z + _LightShadowData.w); \
    COMPUTE_SHADOW_COLLECTOR_SHADOW(i, cascadeWeights, shadowFade)
#endif

#endif // #ifdef SHADOW_COLLECTOR_PASS


// Legacy; used to do something on platforms that had to emulate depth textures manually. Now all platforms have native depth textures.
#define UNITY_TRANSFER_DEPTH(oo)
// Legacy; used to do something on platforms that had to emulate depth textures manually. Now all platforms have native depth textures.
#define UNITY_OUTPUT_DEPTH(i) return 0



#define API_HAS_GUARANTEED_R16_SUPPORT !(SHADER_API_VULKAN || SHADER_API_GLES || SHADER_API_GLES3)

fixed4 PackHeightmap(fixed height)
{
    #if (API_HAS_GUARANTEED_R16_SUPPORT)
        return height;
    #else
        uint a = (uint)(65535.0f * height);
        return fixed4((a >> 0) & 0xFF, (a >> 8) & 0xFF, 0, 0) / 255.0f;
    #endif
}

fixed UnpackHeightmap(fixed4 height)
{
    #if (API_HAS_GUARANTEED_R16_SUPPORT)
        return height.r;
    #else
        return (height.r + height.g * 256.0f) / 257.0f; // (255.0f * height.r + 255.0f * 256.0f * height.g) / 65535.0f
    #endif
}

#endif // UNITY_CG_INCLUDED
