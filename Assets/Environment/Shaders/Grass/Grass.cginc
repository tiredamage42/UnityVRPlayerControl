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

#define NO_ROTATION_SCALE
#define BUMP_MAP

#include "../ShaderHelp.cginc"


#include "../GeometryShaderUtils.cginc"


#ifndef SHADOWCASTER
    #include "../Environment.cginc"    
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


        UNITY_POSITION(pos);
#ifndef SHADOWCASTER
        MY_LIGHTING_COORDS(1, 2, 3, 4, 5, 6)

        fixed4 bump_uv_strength : TEXCOORD9;
        
        fixed4 tint_fog : COLOR;

        fixed4 hueVariation : TEXCOORD7;
       
#else

        V2F_SHADOW_CASTER_NOPOS(1)                       
        
#endif

        UNITY_VERTEX_OUTPUT_STEREO
    };


#ifndef SHADOWCASTER
    #define ALPHA_CUTOFF v.cutoff_shadowcutoff.x
#else 
    #define ALPHA_CUTOFF v.cutoff_shadowcutoff.y
#endif

    void AddVertex (fixed distFade, inout g2f OUT, inout TriangleStream<g2f> stream, VertexBuffer v, fixed3 vertex, fixed2 uv, fixed3 normal, fixed3 tangent, fixed3 bitangent, fixed4 hueVariation) 
    {

        UNITY_INITIALIZE_OUTPUT(g2f, OUT);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT); 


        fixed2 mainUV = v.uv_offset_main.xy + uv * v.uv_offset_main.zw;
        
        OUT.uv_cutoff_distancemod = fixed4(mainUV, ALPHA_CUTOFF, distFade); 
        

// #if defined (COLOR_PASS)
#ifndef SHADOWCASTER

        
        fixed2 bumpUV = v.uv_offset_bump.xy + uv * v.uv_offset_bump.zw;
        OUT.bump_uv_strength = fixed4(bumpUV, v.width_height_bumpStrength.z, 0);
        
        OUT.pos = UnityObjectToClipPos(vertex);
        
        OUT.tint_fog = fixed4(v.tint_color.rgb, 0);
        OUT.hueVariation = hueVariation;

        CALCULATE_WORLD_SPACE_VALUES(vertex, normal, tangent.xyz) 
        

        // output the tangent space matrix
        OUT.tSpace0 = fixed4(tangent.x, bitangent.x, normal.x, vertex.x);
        OUT.tSpace1 = fixed4(tangent.y, bitangent.y, normal.y, vertex.y);
        OUT.tSpace2 = fixed4(tangent.z, bitangent.z, normal.z, vertex.z);
        
        CALCULATE_AMBIENT_LIGHTING(OUT)
        CALCULATE_FOG(OUT.pos, OUT.tint_fog.a)
        UNITY_TRANSFER_LIGHTING(OUT)
#else       
        TRANSFER_SHADOW_CASTER_NOPOS(OUT, vertex, normal)          
#endif

        stream.Append(OUT);
    }





    void DrawQuadWind (inout g2f OUT, inout TriangleStream<g2f> stream, fixed3 base, fixed3 top, fixed distFade, fixed3 normal, VertexBuffer v, fixed3 perpDir, fixed3 wTangent, fixed3 wBitangent) {
        
        fixed3 quadCorner = base - perpDir;
        fixed quadRandom = frac(abs(quadCorner.x + quadCorner.y + quadCorner.z) * 2);

        fixed flipUV = floor(quadRandom+0.5); //flip uv randomly per quad

        fixed4 hueVariation = 0;

#ifndef SHADOWCASTER

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



        CALCULATE_CAMERA_VARIABLES(grass_point)

    fixed fadeStart = _PCGRASS_CAMERA_RANGE.x;
    fixed fadeEnd = _PCGRASS_CAMERA_RANGE.y;

    if (CAMDIST > fadeEnd)
        return;

    if (dot(CAMFWD, CAMDIR) < 0.5)
        return;

        fixed distFade = 1.0 - InverseLerp(_PCGRASS_CAMERA_RANGE.xy, CAMDIST);

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
// #if defined (COLOR_PASS)  
#ifndef SHADOWCASTER

        fixed3 quad1Norm = cross(perpDir, groundNorm);
        wTangent = cross(quad1Norm, perpDir);
        // compute bitangent from cross product of normal and tangent
        wBitangent = cross(groundNorm, wTangent) * unity_WorldTransformParams.w * -1;
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


#ifndef SHADOWCASTER

    sampler2D _BumpMap;

    fixed4 frag(g2f IN) : SV_Target
    {
        fixed4 diffuseColor = tex2D(_MainTex, IN.uv_cutoff_distancemod.xy);
        
        clip(diffuseColor.a - IN.uv_cutoff_distancemod.z);

        diffuseColor.rgb *= IN.tint_fog.rgb;
        
        // add hue variation
        AddHueVariation(diffuseColor.rgb, IN.hueVariation);

        // sample the normal map, and decode from the Unity encoding
        fixed3 tnormal = UnpackNormalWithScale(tex2D(_BumpMap, IN.bump_uv_strength.xy), IN.bump_uv_strength.z);

        FINISH_FRAGMENT_CALC(IN, diffuseColor.rgb, tnormal, IN.tint_fog.w)
    }

#else

    fixed GRASS_SHADOW_CASTER_FRAG(g2f IN, fixed cutoff) {
        fixed2 uv = IN.uv_cutoff_distancemod.xy; 
        fixed4 diffuseColor = tex2D(_MainTex, uv); 
        clip(diffuseColor.a - cutoff); 

        SHADOW_CASTER_FRAGMENT(IN)                  
    }

#endif














#endif // SPEEDTREE_COMMON_INCLUDED
