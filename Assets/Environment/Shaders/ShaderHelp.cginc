// Upgrade NOTE: replaced 'unity_World2Shadow' with 'unity_WorldToShadow'


#ifndef SHADER_HELP_INCLUDED
#define SHADER_HELP_INCLUDED

#include "UnityCG.cginc" 



#define WORLDPOS_NAME i_VertexWorld
#define WORLDNORM_NAME i_NormalWorld
#define WORLDTANG_NAME i_TangentWorld
#define LIGHTDIR_NAME i_LightDir

#ifndef NO_ROTATION_SCALE
    #define CALCULATE_WORLDPOS(lVert) (mul(unity_ObjectToWorld, fixed4(lVert.xyz, 1))).xyz
#else 
    #define CALCULATE_WORLDPOS(lVert) (lVert.xyz + fixed3(unity_ObjectToWorld[0].w, unity_ObjectToWorld[1].w, unity_ObjectToWorld[2].w))
#endif


#ifdef SHADOWCASTER

    #ifdef SHADOWS_CUBE
        // Rendering into point light (cubemap) shadows
        #define V2F_SHADOW_CASTER_NOPOS(TCID) float3 vec : TEXCOORD##TCID;

        #define TRANSFER_SHADOW_CASTER_NOPOS(o, lVert, lNorm) \
            o.vec = CALCULATE_WORLDPOS(lVert) - _LightPositionRange.xyz; \
            o.pos = UnityObjectToClipPos(lVert);

        #define SHADOW_CASTER_FRAGMENT(i) return UnityEncodeCubeShadowDepth ((length(i.vec) + unity_LightShadowBias.x) * _LightPositionRange.w);
    #else
        // Rendering into directional or spot light shadows
        #define V2F_SHADOW_CASTER_NOPOS(TCID) float4 hpos : TEXCOORD##TCID;

        #define TRANSFER_SHADOW_CASTER_NOPOS(o, lVert, lNorm) \
            o.pos = UnityClipSpaceShadowCasterPos(lVert.xyz, lNorm); \
            o.pos = UnityApplyLinearShadowBias(o.pos); \
            o.hpos = o.pos;
        
        #define SHADOW_CASTER_FRAGMENT(i) UNITY_OUTPUT_DEPTH(i.hpos.zw);
    #endif

    #define FRAGMENT_IN v2fSHADOW
    #define VERTEX_IN appdata_full
    
    struct FRAGMENT_IN {    
        float4 pos : SV_POSITION;     
        V2F_SHADOW_CASTER_NOPOS(0)                       
        UNITY_VERTEX_OUTPUT_STEREO                      
    };        


    FRAGMENT_IN vertexSHADOW (VERTEX_IN v) {               
        UNITY_SETUP_INSTANCE_ID(v);
                                
        FRAGMENT_IN o = (FRAGMENT_IN)0;       
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);    
        TRANSFER_SHADOW_CASTER_NOPOS(o, v.vertex, v.normal)          
        return o;                                       
    }                                                   
    fixed fragSHADOW (FRAGMENT_IN i) : SV_Target {         
        SHADOW_CASTER_FRAGMENT(i)                      
    }                                                   

#else // NOT SHADOWCASTER
    #include "Lighting.cginc"
    #include "AutoLight.cginc"

#ifdef POINT
#   define COMPUTE_LIGHT_COORDS_c(a) a._LightCoord = mul(unity_WorldToLight, float4(WORLDPOS_NAME.xyz, 1)).xyz;
#endif
#ifdef SPOT
#   define COMPUTE_LIGHT_COORDS_c(a) a._LightCoord = mul(unity_WorldToLight, float4(WORLDPOS_NAME.xyz, 1));
#endif
#ifdef DIRECTIONAL
#   define COMPUTE_LIGHT_COORDS_c(a)
#endif
#ifdef POINT_COOKIE
#   define COMPUTE_LIGHT_COORDS_c(a) a._LightCoord = mul(unity_WorldToLight, float4(WORLDPOS_NAME.xyz, 1)).xyz;
#endif
#ifdef DIRECTIONAL_COOKIE
#   define COMPUTE_LIGHT_COORDS_c(a) a._LightCoord = mul(unity_WorldToLight, float4(WORLDPOS_NAME.xyz, 1)).xy;
#endif

#if defined(UNITY_NO_SCREENSPACE_SHADOWS)
#define TRANSFER_SHADOW_c(a) a._ShadowCoord = mul( unity_WorldToShadow[0], float4(WORLDPOS_NAME.xyz, 1) );
#else // UNITY_NO_SCREENSPACE_SHADOWS
#define TRANSFER_SHADOW_c(a) a._ShadowCoord = ComputeScreenPos(a.pos);
#endif
// ---- Depth map shadows
#if defined (SHADOWS_DEPTH) && defined (SPOT)
#define TRANSFER_SHADOW_c(a) a._ShadowCoord = mul (unity_WorldToShadow[0], float4(WORLDPOS_NAME.xyz, 1) );
#endif
// ---- Point light shadows
#if defined (SHADOWS_CUBE)
#define TRANSFER_SHADOW_c(a) a._ShadowCoord = (WORLDPOS_NAME).xyz - _LightPositionRange.xyz;
#endif
// ---- Shadows off
#if !defined (SHADOWS_SCREEN) && !defined (SHADOWS_DEPTH) && !defined (SHADOWS_CUBE)
#define TRANSFER_SHADOW_c(a)
#endif

#define UNITY_TRANSFER_LIGHTING(a) COMPUTE_LIGHT_COORDS_c(a) TRANSFER_SHADOW_c(a)


#if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
    #define UNITY_FOG_LERP_COLOR(col, fogCol, fogFac) col.rgb = lerp(fogCol, col.rgb, fogFac)
    
    #ifndef UNITY_PASS_FORWARDADD
        #define APPLY_FOG(coord, col) UNITY_FOG_LERP_COLOR(col, unity_FogColor.rgb, coord)
    #else
        #define APPLY_FOG(coord, col) UNITY_FOG_LERP_COLOR(col, fixed3(0,0,0), coord)
    #endif
    
    #define CALCULATE_FOG(clipPos, outFogFactor) UNITY_CALC_FOG_FACTOR(clipPos.z); outFogFactor = saturate(unityFogFactor);
#else
    #define APPLY_FOG(coord,col)
    #define CALCULATE_FOG(clipPos, outFogFactor)
#endif

#ifdef BUMP_MAP
    #define TSPACE_MATRIX(TCID0, TCID1, TCID2) \
        float4 tSpace0 : TEXCOORD##TCID0; \
        float4 tSpace1 : TEXCOORD##TCID1; \
        float4 tSpace2 : TEXCOORD##TCID2;
    
    #define TRANSFER_TSPACEMATRIX(a, b) \
        b.tSpace0 = a.tSpace0; \
        b.tSpace1 = a.tSpace1; \
        b.tSpace2 = a.tSpace2; 
#else
    #define TSPACE_MATRIX(TCID0, TCID1, TCID2) \
        float3 wPos : TEXCOORD##TCID0; \
        float3 wNorm : TEXCOORD##TCID1;
    
    #define TRANSFER_TSPACEMATRIX(a, b) \
        b.wPos = a.wPos; \
        b.wNorm = a.wNorm; 
        
#endif

#ifndef UNITY_PASS_FORWARDADD
    #define AMBIENT_LIGHTING_COORDS(TCID) float3 ambLighting : TEXCOORD##TCID; 
#else
    #define AMBIENT_LIGHTING_COORDS(TCID) 
#endif
    
#define INTERPOLATOR_DATA UNITY_VERTEX_INPUT_INSTANCE_ID UNITY_VERTEX_OUTPUT_STEREO



#define MY_LIGHTING_COORDS(TCID0, TCID1, TCID2, LID0, LID1, ALID) \
    TSPACE_MATRIX(TCID0, TCID1, TCID2) \
    LIGHTING_COORDS(LID0, LID1) \
    AMBIENT_LIGHTING_COORDS(ALID) \
    INTERPOLATOR_DATA


#ifndef NO_ROTATION_SCALE
    #define CALCULATE_WORLD_SPACE_VALUES(lVert, lNorm, lTang) \
        fixed3 WORLDPOS_NAME = CALCULATE_WORLDPOS(lVert); \
        fixed3 WORLDNORM_NAME = UnityObjectToWorldNormal(lNorm.xyz); \
        fixed3 WORLDTANG_NAME = UnityObjectToWorldDir(lTang.xyz); 
#else 
    #define CALCULATE_WORLD_SPACE_VALUES(lVert, lNorm, lTang) \
        fixed3 WORLDPOS_NAME = CALCULATE_WORLDPOS(lVert); \
        fixed3 WORLDNORM_NAME = lNorm.xyz; \
        fixed3 WORLDTANG_NAME = lTang.xyz;


#endif

#ifdef BUMP_MAP
    
    #define PACK_TSPACE_MATRIX(o, sign) \
        fixed3 worldBinormal = cross(WORLDNORM_NAME, WORLDTANG_NAME) * (sign * unity_WorldTransformParams.w); \
        o.tSpace0 = float4(WORLDTANG_NAME.x, worldBinormal.x, WORLDNORM_NAME.x, WORLDPOS_NAME.x); \
        o.tSpace1 = float4(WORLDTANG_NAME.y, worldBinormal.y, WORLDNORM_NAME.y, WORLDPOS_NAME.y); \
        o.tSpace2 = float4(WORLDTANG_NAME.z, worldBinormal.z, WORLDNORM_NAME.z, WORLDPOS_NAME.z);

#else 
    #define PACK_TSPACE_MATRIX(o, sign) \
        o.wPos = WORLDPOS_NAME.xyz; \
        o.wNorm = WORLDNORM_NAME.xyz;
    


#endif


#ifdef BUMP_MAP

//looks better without normalization...
#define UNPACK_TSPACE_NORMAL(i, tNorm) ( float3 ( dot(i.tSpace0.xyz, tNorm), dot(i.tSpace1.xyz, tNorm), dot(i.tSpace2.xyz, tNorm) ) )
// #define UNPACK_TSPACE_NORMAL(i, tNorm) normalize( float3 ( dot(i.tSpace0.xyz, tNorm), dot(i.tSpace1.xyz, tNorm), dot(i.tSpace2.xyz, tNorm) ) )

#define EXTRACT_WORLDPOS(i, worldPos) float3 worldPos = fixed3(i.tSpace0.w, i.tSpace1.w, i.tSpace2.w);

#else
#define UNPACK_TSPACE_NORMAL(i, tNorm) ( i.wNorm )
#define EXTRACT_WORLDPOS(i, worldPos) float3 worldPos = i.wPos;

#endif

    
#define ADD_LAMBERT_LIGHTING(i, color, albedo, normal) \
    fixed atten = LIGHT_ATTENUATION(i); \
    color.rgb += albedo * _LightColor0.rgb * (atten * max (0, dot (normal, LIGHTDIR_NAME)))


#ifndef UNITY_PASS_FORWARDADD
    #ifdef VERTEX_LIGHTS
        // SH/ambient and vertex lights // Approximated illumination from non-important point lights
        #define CALCULATE_AMBIENT_LIGHTING(o) \
            o.ambLighting += max(half3(0,0,0), ShadeSH9 (half4(WORLDNORM_NAME, 1.0))); \
            o.ambLighting += Shade4PointLights ( \
                unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0, \
                unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb, \
                unity_4LightAtten0, \
                WORLDPOS_NAME, WORLDNORM_NAME \
            );

    #else 
        #define CALCULATE_AMBIENT_LIGHTING(o) \
            o.ambLighting += max(half3(0,0,0), ShadeSH9 (half4(WORLDNORM_NAME, 1.0))); 
    #endif

    #define ADD_AMBIENT_LIGHTING(i, color, albedo, normal) color.rgb += albedo * i.ambLighting
#else
    #define ADD_AMBIENT_LIGHTING(i, color, albedo, normal)
    #define CALCULATE_AMBIENT_LIGHTING(o) 
#endif


#ifdef CALCULATE_LIGHTDIR_VERTEX          
    #define CALCULATE_LIGHT_DIRECTION(i, worldPos) fixed3 LIGHTDIR_NAME = i.lightDir.xyz; 
#else 

    #ifndef USING_DIRECTIONAL_LIGHT
        #define CALCULATE_LIGHT_DIRECTION(i, worldPos) fixed3 LIGHTDIR_NAME = normalize(UnityWorldSpaceLightDir(worldPos));
    #else 
        #define CALCULATE_LIGHT_DIRECTION(i, worldPos) fixed3 LIGHTDIR_NAME = _WorldSpaceLightPos0.xyz; 
    #endif
#endif

#ifdef CALCULATE_LIGHTDIR_VERTEX          
    #ifndef USING_DIRECTIONAL_LIGHT
        #define CALCULATE_LIGHT_DIRECTION_VERT(o) o.lightDir = normalize(UnityWorldSpaceLightDir(WORLDPOS_NAME));
    #else 
        #define CALCULATE_LIGHT_DIRECTION_VERT(o) o.lightDir = _WorldSpaceLightPos0.xyz; 
    #endif
#else 
    #define CALCULATE_LIGHT_DIRECTION_VERT(o) 

#endif



#define INITIALIZE_FRAGMENT_IN(INTYPE, FRAG_IN_NAME, VERTEX_IN) \
    INTYPE FRAG_IN_NAME = (INTYPE)0;    \
    UNITY_SETUP_INSTANCE_ID(VERTEX_IN); \
    UNITY_TRANSFER_INSTANCE_ID(VERTEX_IN, FRAG_IN_NAME); \
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(FRAG_IN_NAME); 


#define FINISH_VERTEX_CALC(o, lVert, lNorm, lTang, lTangentSign, clipPos, fogCoord) \
    CALCULATE_WORLD_SPACE_VALUES(lVert, lNorm, lTang.xyz) \
    PACK_TSPACE_MATRIX(o, lTangentSign) \
    CALCULATE_LIGHT_DIRECTION_VERT(o) \
    CALCULATE_AMBIENT_LIGHTING(o) \
    CALCULATE_FOG(clipPos, fogCoord) \
    UNITY_TRANSFER_LIGHTING(o)
    
#define FINISH_FRAGMENT_CALC(i, albedo, tNorm, fogCoord) \
    fixed3 wNorm = UNPACK_TSPACE_NORMAL(i, tNorm); \
    fixed4 color = fixed4(0,0,0,1); \
    EXTRACT_WORLDPOS(i, wPos) \
    CALCULATE_LIGHT_DIRECTION(i, wPos) \
    ADD_LAMBERT_LIGHTING(i, color, albedo, wNorm); \
    ADD_AMBIENT_LIGHTING(i, color, albedo, wNorm); \
    APPLY_FOG(fogCoord, color); \
    return color;

#endif // SHADOWCASTER


#endif // UNITY_CG_INCLUDED
