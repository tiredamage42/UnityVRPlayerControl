// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

#ifndef PC_GRASS_INCLUDED
#define PC_GRASS_INCLUDED

// vertex = pos
// normal = ground normal
// color = tint color

// texcoord0 = atlasUVMain
// texcoord1 = atlasUVBump

// texcoord2 = (cutoff, shadowCutoff, 0, 0)
// texcoord3 = hueVariation
// texcoord4 = (width, height, bumpstrength, 0)


// VERTEX
struct VertexBuffer
{
    fixed4 grass_point : POSITION;
    fixed3 ground_normal : NORMAL;
    fixed4 tint_color : COLOR;
    fixed4 uv_offset_main : TEXCOORD0;
    fixed4 uv_offset_bump : TEXCOORD1;
    fixed4 cutoff_shadowcutoff : TEXCOORD2;
    fixed4 hue_variation : TEXCOORD3;
    fixed4 width_height_bumpStrength : TEXCOORD4;
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

#if defined (USE_WIND) 
    #include "../CustomWind.cginc"

    fixed4 _PCGRASS_WIND_SPEED_FREQUENCY_RANGES;
    fixed4 _PCGRASS_WIND_SCALE_MIN;
    fixed4 _PCGRASS_WIND_SCALE_MAX;

    fixed3 WaveGrass (fixed3 vertex, fixed waveAmount)
    {
        return WaveBase (
            vertex, 
            _PCGRASS_WIND_SPEED_FREQUENCY_RANGES.xy, 
            _PCGRASS_WIND_SPEED_FREQUENCY_RANGES.zw, 
            _PCGRASS_WIND_SCALE_MIN, 
            _PCGRASS_WIND_SCALE_MAX, 
            waveAmount
        );
    }

#else
    fixed3 WaveGrass (fixed3 vertex, fixed waveAmount) {
        return vertex;
    }
#endif

    struct g2f
    {

        fixed4 uv_cutoff_distancemod : TEXCOORD0;

#if defined (COLOR_PASS)
        UNITY_POSITION(pos);

        fixed4 bump_uv : TEXCOORD9;
        
        fixed4 tint_bumpstrength : COLOR;

#if defined( FORWARD_LIGHTING )
        UNITY_LIGHTING_COORDS(1,2)
        UNITY_FOG_COORDS(3)
            
#endif

        fixed3 lightDir : TEXCOORD4;
        fixed4 hueVariation : TEXCOORD5;
        
        // these three vectors will hold a 3x3 rotation matrix that transforms from tangent to world space
        fixed4 tSpace0 : TEXCOORD6; // tangent.x, bitangent.x, normal.x, worldPos.x
        fixed4 tSpace1 : TEXCOORD7; // tangent.y, bitangent.y, normal.y, worldPos.y
        fixed4 tSpace2 : TEXCOORD8; // tangent.z, bitangent.z, normal.z, worldPos.z

//      //vertex lighting
// #if defined (DEFERRED_LIGHTING) && UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL 
#if defined (FORWARD_BASE_LIGHTING)

    fixed3 sh : TEXCOORD10; 
#endif

        UNITY_VERTEX_OUTPUT_STEREO
#else
        V2F_SHADOW_CASTER;
#endif

    };

#if defined (COLOR_PASS)
    #define ALPHA_CUTOFF v.cutoff_shadowcutoff.x
#else 
    #define ALPHA_CUTOFF v.cutoff_shadowcutoff.y
#endif

    void AddVertex (fixed distFade, inout g2f OUT, inout TriangleStream<g2f> stream, VertexBuffer v, fixed3 vertex, fixed2 uv, fixed3 normal, fixed3 tangent, fixed3 bitangent, fixed4 hueVariation) 
    {

        UNITY_INITIALIZE_OUTPUT(g2f, OUT);

        fixed2 mainUV = v.uv_offset_main.xy + uv * v.uv_offset_main.zw;
        
        OUT.uv_cutoff_distancemod = fixed4(mainUV, ALPHA_CUTOFF, distFade); 
        

#if defined (COLOR_PASS)

        
        fixed2 bumpUV = v.uv_offset_bump.xy + uv * v.uv_offset_bump.zw;
        OUT.bump_uv = fixed4(bumpUV, 0, 0);
        
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
        
        OUT.pos = UnityObjectToClipPos(vertex);
        
        OUT.tint_bumpstrength = fixed4(v.tint_color.rgb, v.width_height_bumpStrength.z);
        OUT.hueVariation = hueVariation;

        // output the tangent space matrix
        OUT.tSpace0 = fixed4(tangent.x, bitangent.x, normal.x, vertex.x);
        OUT.tSpace1 = fixed4(tangent.y, bitangent.y, normal.y, vertex.y);
        OUT.tSpace2 = fixed4(tangent.z, bitangent.z, normal.z, vertex.z);
        
        OUT.lightDir = normalize(UnityWorldSpaceLightDir (vertex));

// #if defined (DEFERRED_LIGHTING)
            // #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL

#if defined (FORWARD_BASE_LIGHTING)

                // OUT.sh = ShadeSHPerVertex (normal, OUT.sh);
                OUT.sh = ShadeSHPerVertex (fixed3(0,1,0), OUT.sh);
            #endif
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

    void DrawQuadWind (inout g2f OUT, inout TriangleStream<g2f> stream, fixed3 base, fixed3 top, fixed distFade, fixed3 normal, VertexBuffer v, fixed3 perpDir, fixed3 wTangent, fixed3 wBitangent) {
        
        fixed3 quadCorner = base - perpDir;
        fixed quadRandom = frac(abs(quadCorner.x + quadCorner.y + quadCorner.z) * 2);

        fixed flipUV = floor(quadRandom+0.5); //flip uv randomly per quad

        fixed4 hueVariation = 0;
#if defined (COLOR_PASS)
        hueVariation = fixed4(v.hue_variation.rgb, saturate( quadRandom * v.hue_variation.a));
#endif

        AddVertex (distFade, OUT, stream, v, quadCorner, fixed2(flipUV, 0), normal, wTangent, wBitangent, hueVariation);
        
        quadCorner = base + perpDir;
        AddVertex (distFade, OUT, stream, v, quadCorner, fixed2(1-flipUV, 0), normal, wTangent, wBitangent, hueVariation);
        
        quadCorner = WaveGrass (top - perpDir, distFade);
        AddVertex (distFade, OUT, stream, v, quadCorner, fixed2(flipUV, 1), normal, wTangent, wBitangent, hueVariation);
        
        quadCorner = WaveGrass (top + perpDir, distFade);
        AddVertex (distFade, OUT, stream, v, quadCorner, fixed2(1-flipUV, 1), normal, wTangent, wBitangent, hueVariation);
        
        stream.RestartStrip();
    }

    fixed4 _PCGRASS_CAMERA_RANGE;

    [maxvertexcount(16)]
    void geom(point VertexBuffer IN[1], inout TriangleStream<g2f> stream)
    {
        VertexBuffer vertBuffer = IN[0];
        
        fixed3 grass_point = vertBuffer.grass_point.xyz;


        fixed camDistance;
        fixed distFade = CalculateDistanceFade (grass_point, _PCGRASS_CAMERA_RANGE.xy, camDistance);
        if (distFade <= 0)
            return;
        
        float3 camForward = mul((float3x3)unity_CameraToWorld, float3(0,0,1));
        camForward.y = 0;
        camForward = normalize(camForward);

        fixed3 camDir = grass_point - _WorldSpaceCameraPos;
        camDir.y = 0;
        camDir = normalize(camDir);
        if (dot(camForward, camDir) <= 0.25)
            return;



        fixed3 groundNorm = IN[0].ground_normal;
        fixed width = IN[0].width_height_bumpStrength.x;
        fixed height = IN[0].width_height_bumpStrength.y;

        #if defined(NO_HEIGHT_FADE)
            fixed3 top = grass_point + groundNorm * (height);
        #else 
            fixed3 top = grass_point + groundNorm * (height * distFade);
        #endif

        fixed3 perpDir = cross(groundNorm, cross(fixed3(0, 0, 1), groundNorm));

        g2f OUT;

        fixed3 wTangent = 0, wBitangent = 0;
#if defined (COLOR_PASS)            
        fixed3 quad1Norm = cross(perpDir, groundNorm);
        wTangent = cross(quad1Norm, perpDir);
        // compute bitangent from cross product of normal and tangent
        wBitangent = cross(groundNorm, wTangent) * unity_WorldTransformParams.w;
#endif

        fixed quads = 4 * distFade;

        // Quad 0
        fixed3 quad1Perp = perpDir * .5 * width;
        DrawQuadWind (OUT, stream, grass_point, top, distFade, groundNorm, vertBuffer, quad1Perp, wTangent, wBitangent);
        
        if (quads >= 1) {
            // Quad 1
            fixed4x4 quad1Matrix = rotationMatrix(groundNorm, SIN90, COS90);
            DrawQuadWind (OUT, stream, grass_point, top, distFade, groundNorm, vertBuffer, mul(quad1Matrix, quad1Perp), wTangent, wBitangent );

            if (quads >= 2) {
                // Quad 2
                fixed4x4 quad2Matrix = rotationMatrix(groundNorm, -SIN45, COS45);
                DrawQuadWind (OUT, stream, grass_point, top, distFade, groundNorm, vertBuffer, mul(quad2Matrix, quad1Perp), wTangent, wBitangent);

                if (quads >= 3) {
                    // Quad 3
                    fixed4x4 quad3Matrix = rotationMatrix(groundNorm, SIN45, COS45);
                    DrawQuadWind (OUT, stream, grass_point, top, distFade, groundNorm, vertBuffer, mul(quad3Matrix, quad1Perp), wTangent, wBitangent);
                }
            }
        }
    }


#if defined(COLOR_PASS)

    sampler2D _BumpMap;


    fixed4 frag(g2f IN) : SV_Target
    {
        fixed2 uv = IN.uv_cutoff_distancemod.xy;
        fixed cutoff = IN.uv_cutoff_distancemod.z;

        fixed3 tint = IN.tint_bumpstrength.rgb;
        
        fixed4 diffuseColor = tex2D(_MainTex, uv);
        clip(diffuseColor.a - cutoff);

        // add hue variation
        AddHueVariation(diffuseColor.rgb, IN.hueVariation);

        // sample the normal map, and decode from the Unity encoding
        fixed3 tnormal = UnpackScaleNormal(tex2D(_BumpMap, IN.bump_uv.xy), IN.tint_bumpstrength.a);
        // transform normal from tangent to world space
        fixed3 worldNormal = fixed3(
            dot(IN.tSpace0.xyz, tnormal), dot(IN.tSpace1.xyz, tnormal), dot(IN.tSpace2.xyz, tnormal)
        );

        diffuseColor.rgb *= tint;
                                    
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
