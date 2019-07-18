// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

#ifndef BILLBOARD_MAP_INCLUDED
#define BILLBOARD_MAP_INCLUDED
/*

    TODO: FADE OUT (WITH CUTOFF) IF TOO CLOSE TO CAMERA

    TODO: CULL IF BEHIND CAMERA....
*/

// vertex = pos
// texcoord0 = billboardUVs
// texcoord1 = width, height, bottom, scale

// VERTEX
struct VertexBuffer
{
    fixed4 basePoint : POSITION;
    fixed4 sizeParams : TEXCOORD0;
    fixed4 billboardUVs : TEXCOORD1;
};
    
VertexBuffer vert(VertexBuffer v) { 
    return v; 
}

sampler2D _MainTex;

#include "../GeometryShaderUtils.cginc"

#if defined (COLOR_PASS)
    #include "../Environment.cginc"    
    #include "Lighting.cginc"      
#endif



    struct g2f
    {

        fixed4 uv_cutoff_distancemod : TEXCOORD0;

#if defined (COLOR_PASS)
        UNITY_POSITION(pos);

        // fixed4 bump_uv : TEXCOORD9;
        // fixed4 tint_bumpstrength : COLOR;
        

#if defined( FORWARD_LIGHTING )
        UNITY_LIGHTING_COORDS(1,2)
        UNITY_FOG_COORDS(3)
            
#endif

        fixed3 lightDir : TEXCOORD4;
        fixed hueVariationAmt : TEXCOORD5;
        
        // these three vectors will hold a 3x3 rotation matrix that transforms from tangent to world space
        fixed4 tSpace0 : TEXCOORD6; // tangent.x, bitangent.x, normal.x, worldPos.x
        fixed4 tSpace1 : TEXCOORD7; // tangent.y, bitangent.y, normal.y, worldPos.y
        fixed4 tSpace2 : TEXCOORD8; // tangent.z, bitangent.z, normal.z, worldPos.z

//      //vertex lighting
// #if defined (DEFERRED_LIGHTING) && UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL 
// #if defined (FORWARD_BASE_LIGHTING)

    // fixed3 sh : TEXCOORD10; 
// #endif

        UNITY_VERTEX_OUTPUT_STEREO
#else
        V2F_SHADOW_CASTER;
#endif

    };

    float _Cutoff;
    // fixed4 _HueVariation;

    #define ALPHA_CUTOFF _Cutoff

    void AddVertex (fixed4 imageTexCoords, fixed3 basePoint, fixed distFade, inout g2f OUT, inout TriangleStream<g2f> stream, VertexBuffer v, fixed3 vertex, fixed2 uv, fixed3 normal, fixed3 tangent, fixed3 bitangent, fixed hueVariationAmt) 
    {

        fixed3 lVertex = vertex - basePoint;
        fixed3 billboardPos =  lVertex.x * tangent;
        billboardPos.y = lVertex.y;
        
        vertex = billboardPos + basePoint;

        




        UNITY_INITIALIZE_OUTPUT(g2f, OUT);

        // fixed2 mainUV = v.uv_offset_main.xy + uv * v.uv_offset_main.zw;
        fixed2 mainUV = imageTexCoords.xy + imageTexCoords.zw * uv;
        
        OUT.uv_cutoff_distancemod = fixed4(mainUV, ALPHA_CUTOFF, distFade); 
        

#if defined (COLOR_PASS)

        
        // fixed2 bumpUV = v.uv_offset_bump.xy + uv * v.uv_offset_bump.zw;
        // OUT.bump_uv = fixed4(bumpUV, 0, 0);
        
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
        
        OUT.pos = UnityObjectToClipPos(vertex);
        
        // OUT.tint_bumpstrength = fixed4(v.tint_color.rgb, v.width_height_bumpStrength.z);
        OUT.hueVariationAmt = hueVariationAmt;

        // output the tangent space matrix
        OUT.tSpace0 = fixed4(tangent.x, bitangent.x, normal.x, vertex.x);
        OUT.tSpace1 = fixed4(tangent.y, bitangent.y, normal.y, vertex.y);
        OUT.tSpace2 = fixed4(tangent.z, bitangent.z, normal.z, vertex.z);
        
        OUT.lightDir = normalize(UnityWorldSpaceLightDir (vertex));

// #if defined (DEFERRED_LIGHTING)
            // #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL

// #if defined (FORWARD_BASE_LIGHTING)

                // OUT.sh = ShadeSHPerVertex (normal, OUT.sh);
                // OUT.sh = ShadeSHPerVertex (fixed3(0,1,0), OUT.sh);
            // #endif
// #endif

#if defined( FORWARD_LIGHTING )
        // Macro to send shadow & attenuation to the fragment shader.
        TRANSFER_VERTEX_TO_FRAGMENT(OUT, fixed4(vertex, 1));              
        UNITY_TRANSFER_FOG(OUT, OUT.pos);
#endif

#else       
        TRANSFER_SHADOW_CASTER_NORMALOFFSET(OUT, vertex, normal, vertex);
#endif

        stream.Append(OUT);
    }

    void DrawQuad (fixed4 imageTexCoords, inout g2f OUT, inout TriangleStream<g2f> stream, fixed3 base, fixed3 top, fixed distFade, fixed3 normal, VertexBuffer v, fixed3 perpDir, fixed3 wTangent, fixed3 wBitangent) {
        
        fixed3 worldPos = base;// fixed3(unity_ObjectToWorld[0].w, unity_ObjectToWorld[1].w, unity_ObjectToWorld[2].w);
        
        #if defined(COLOR_PASS)
        fixed3 eyeVec = normalize(_WorldSpaceCameraPos - worldPos);
        #else
        fixed3 eyeVec;
        //directional
        if (_WorldSpaceLightPos0.w == 0) {
            eyeVec = _WorldSpaceLightPos0.xyz;
        }
        else {
            eyeVec = normalize(_WorldSpaceLightPos0.xyz - worldPos);
        }
        #endif


        wTangent = normalize(fixed3(-eyeVec.z, 0, eyeVec.x));            // cross(eyeVec, {0,1,0})
        // normal = fixed3(wTangent.z, 0, -wTangent.x);    // cross({0,1,0},billboardTangent)
        
        
        
        fixed quadRandom = frac(abs(base.x + base.y + base.z) * 2);

        fixed flipUV = floor(quadRandom+0.5); //flip uv randomly per quad

        fixed4 hueVariation = 0;
#if defined (COLOR_PASS)
        hueVariation = saturate( quadRandom * _HueVariation.a);


        wBitangent = cross(normal, wTangent) * unity_WorldTransformParams.w;
#endif


                
        
        fixed3 quadCorner = base - perpDir;
        AddVertex (imageTexCoords, base, distFade, OUT, stream, v, quadCorner, fixed2(flipUV, 0), normal, wTangent, wBitangent, hueVariation);
        
        quadCorner = base + perpDir;
        AddVertex (imageTexCoords, base, distFade, OUT, stream, v, quadCorner, fixed2(1-flipUV, 0), normal, wTangent, wBitangent, hueVariation);
        
        quadCorner = top - perpDir;
        AddVertex (imageTexCoords, base, distFade, OUT, stream, v, quadCorner, fixed2(flipUV, 1), normal, wTangent, wBitangent, hueVariation);
        
        quadCorner = top + perpDir;
        AddVertex (imageTexCoords, base, distFade, OUT, stream, v, quadCorner, fixed2(1-flipUV, 1), normal, wTangent, wBitangent, hueVariation);
        
        // stream.RestartStrip();
    }

    fixed4 _CameraRange;

    [maxvertexcount(4)]
    void geom(point VertexBuffer IN[1], inout TriangleStream<g2f> stream)
    {
        VertexBuffer vertBuffer = IN[0];
        
        fixed3 noraml = fixed3(0,1,0);
        fixed3 basePoint = vertBuffer.basePoint.xyz;

        fixed3 viewDir = _WorldSpaceCameraPos - basePoint;
        viewDir.y = 0;
        fixed cameraDistance = length(viewDir);

    fixed fadeStart = _CameraRange.x;
    fixed fadeEnd = _CameraRange.y;

    if (cameraDistance < fadeStart)
        return;

    float3 camForward = mul((float3x3)unity_CameraToWorld, float3(0,0,1));
    camForward.y = 0;
    camForward = normalize(camForward);

    viewDir /= cameraDistance;

    if (dot(camForward, viewDir) > 0.25)
        return;


    fixed distFade = saturate((cameraDistance - fadeStart) / (fadeEnd - fadeStart));
        
        // texcoord1 = width, height, bottom, scale
        fixed4 sizeParams = vertBuffer.sizeParams;

        fixed4 imageTexCoords = vertBuffer.billboardUVs;

        fixed width = sizeParams.x;
        fixed height = sizeParams.y;
        fixed bottom = sizeParams.z;
        fixed scale = sizeParams.w;

        basePoint += bottom;
        fixed3 top = basePoint + noraml * height * scale;

        fixed3 perpDir = fixed3(width * .5 * scale, 0, 0);

        g2f OUT;

        // fixed distFade = 1;

        fixed3 wTangent = 0, wBitangent = 0;
#if defined (COLOR_PASS)            
        fixed3 quadNorm = cross(fixed3(1,0,0), noraml);
        wTangent = cross(quadNorm, fixed3(1,0,0));
        // compute bitangent from cross product of normal and tangent
        wBitangent = cross(noraml, wTangent) * unity_WorldTransformParams.w;
#endif

        DrawQuad (imageTexCoords, OUT, stream, basePoint, top, distFade, noraml, vertBuffer, perpDir, wTangent, wBitangent);
        
    }


#if defined(COLOR_PASS)

    sampler2D _BumpMap;
    fixed4 _Color;


    fixed4 frag(g2f IN) : SV_Target
    {
        fixed2 uv = IN.uv_cutoff_distancemod.xy;
        fixed cutoff = IN.uv_cutoff_distancemod.z;
        fixed distFade = IN.uv_cutoff_distancemod.w;
        // clip(-1);
        // return fixed4(uv, 0, 1);

        fixed4 diffuseColor = tex2D(_MainTex, uv);
        clip(diffuseColor.a - (cutoff + (1-distFade)));

        // add hue variation
        AddHueVariation(diffuseColor.rgb, fixed4(_HueVariation.rgb, IN.hueVariationAmt));

        // sample the normal map, and decode from the Unity encoding
        fixed3 tnormal = UnpackScaleNormal(tex2D(_BumpMap, uv), 1);
        // transform normal from tangent to world space
        fixed3 worldNormal = fixed3(
            dot(IN.tSpace0.xyz, tnormal), dot(IN.tSpace1.xyz, tnormal), dot(IN.tSpace2.xyz, tnormal)
        );

        diffuseColor.rgb *= _Color;
                                    
        fixed4 c = 0;

#if defined (FORWARD_BASE_LIGHTING)
        // Ambient term. Only do this in Forward Base. It only needs calculating once.

        c.rgb = (UNITY_LIGHTMODEL_AMBIENT.rgb * 2 * diffuseColor.rgb);         

        // c.rgb = (IN.sh.rgb * 2 * diffuseColor.rgb);      
        // c.rgb = unity_AmbientSky.xyz * 2 * diffuseColor.rgb;
        // c.rgb = ShadeSH9(half4(worldNormal, 1.0)); 
                   
#endif

        // Macro to get you the combined shadow & attenuation value.
        fixed atten = LIGHT_ATTENUATION(IN); 
        
        fixed diff = saturate(dot(worldNormal, IN.lightDir));
        
        c.rgb += (diffuseColor.rgb * _LightColor0.rgb) * (diff * atten * 2); 
        
        c.a = 1;

        UNITY_APPLY_FOG(IN.fogCoord, c);

        return c;
    }



#else

    fixed GRASS_SHADOW_CASTER_FRAG(g2f IN, fixed cutoff) {
        // return 1;
        fixed2 uv = IN.uv_cutoff_distancemod.xy; 
        fixed4 diffuseColor = tex2D(_MainTex, uv); 
        clip(diffuseColor.a - cutoff); 

        SHADOW_CASTER_FRAGMENT(IN)                  
    }

#endif














#endif // SPEEDTREE_COMMON_INCLUDED
