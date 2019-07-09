// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

#ifndef PC_GRASS_INCLUDED
#define PC_GRASS_INCLUDED

// VERTEX
struct VertexBuffer
{
    fixed4 grass_point : POSITION;
    fixed3 ground_normal : NORMAL;
    fixed4 tint_color : COLOR;
    fixed4 uv_offset_main : TEXCOORD0;
    fixed4 uv_offset_bump : TEXCOORD1;
    fixed4 hue_variation : TEXCOORD2;
    fixed4 width_height_cutoff_bumpStrength : TEXCOORD3;
};
    
VertexBuffer vert(VertexBuffer v) { 
    return v; 
}

sampler2D _MainTex;

// QUAD ROTATION TOOLS

static const fixed SIN45 = 0.70710678087;
static const fixed COS45 = 0.70710678087;

static const fixed COS90 = 0;
static const fixed SIN90 = 1;

fixed4x4 rotationMatrix(fixed3 axis, fixed sinAngle, fixed cosAngle)
{
    axis = normalize(axis);

    fixed3 oc = (1.0 - cosAngle) * axis;

    fixed ocxy = oc.x * axis.y;
    fixed oczx = oc.z * axis.x;
    fixed ocyz = oc.y * axis.z;

    fixed3 aSin = axis * sinAngle;

    return fixed4x4(
        oc.x * axis.x + cosAngle,           
        ocxy - aSin.z,
        oczx + aSin.y,  
        0.0,
        
        ocxy + aSin.z,  
        oc.y * axis.y + cosAngle,           
        ocyz - aSin.x,  
        0.0,
        
        oczx - aSin.y,  
        ocyz + aSin.x,  
        oc.z * axis.z + cosAngle,           
        0.0,
        
        0.0, 0.0, 0.0, 1.0
    );
}


#if defined (COLOR_PASS)
    void AddHueVariation (inout fixed3 diffuseColor, fixed4 hueVariation) {
        fixed3 shiftedColor = lerp(diffuseColor.rgb, hueVariation.rgb, hueVariation.a);

        fixed maxBase = max(diffuseColor.r, max(diffuseColor.g, diffuseColor.b));
        fixed newMaxBase = max(shiftedColor.r, max(shiftedColor.g, shiftedColor.b));
        // preserve vibrance
        diffuseColor = saturate(shiftedColor * ((maxBase/newMaxBase) * 0.5 + 0.5));
    }
#endif



#if defined (USE_WIND) 
    // WIND TOOLS

    // Calculate a 4 fast sine-cosine pairs
    // val:     the 4 input values - each must be in the range (0 to 1)
    // s:       The sine of each of the 4 values
    // c:       The cosine of each of the 4 values
    void FastSinCos (fixed4 val, out fixed4 s, out fixed4 c) {
        val = val * 6.408849 - 3.1415927;
        // powers for taylor series
        fixed4 r5 = val * val;                  // wavevec ^ 2
        fixed4 r6 = r5 * r5;                        // wavevec ^ 4;
        fixed4 r7 = r6 * r5;                        // wavevec ^ 6;
        fixed4 r8 = r6 * r5;                        // wavevec ^ 8;

        fixed4 r1 = r5 * val;                   // wavevec ^ 3
        fixed4 r2 = r1 * r5;                        // wavevec ^ 5;
        fixed4 r3 = r2 * r5;                        // wavevec ^ 7;

        //Vectors for taylor's series expansion of sin and cos
        fixed4 sin7 = {1, -0.16161616, 0.0083333, -0.00019841};
        fixed4 cos8  = {-0.5, 0.041666666, -0.0013888889, 0.000024801587};
        // sin
        s =  val + r1 * sin7.y + r2 * sin7.z + r3 * sin7.w;
        // cos
        c = 1 + r5 * cos8.x + r6 * cos8.y + r7 * cos8.z + r8 * cos8.w;
    }

    fixed4 _PCGRASS_WINDSETTINGS;

    fixed3 WaveGrass (fixed3 vertex, fixed waveAmount)
    {
        fixed windSpeed = _PCGRASS_WINDSETTINGS.x;
        fixed windFrequency = _PCGRASS_WINDSETTINGS.y;
        fixed windScale = _PCGRASS_WINDSETTINGS.z;

        fixed4 waves;
        waves = vertex.x * fixed4(0.012, 0.02, 0.06, 0.024) * windFrequency;
        waves += vertex.z * fixed4 (0.006, .02, 0.02, 0.05) * windFrequency;

        // Add in time to model them over time
        waves += _Time.x * windSpeed * fixed4 (0.3, .5, .4, 1.2) * 4;

        fixed4 s, c;
        waves = frac (waves);
        FastSinCos (waves, s,c);
        s = s * s;
        s = s * s;
        s = s * waveAmount;

        fixed3 waveMove = fixed3 (
            dot (s, fixed4(0.012, 0.02, -0.06, 0.048) * 2), 
            0, 
            dot (s, fixed4 (0.006, .02, -0.02, 0.1))
        );
        
        vertex.xz -= waveMove.xz * windScale;
        return vertex;
    }

#else

    fixed3 WaveGrass (fixed3 vertex, fixed waveAmount)
    {
        return vertex;
    }

#endif






      struct g2f
        {

            fixed4 uv_cutoff_distancemod : TEXCOORD0;

#if defined (COLOR_PASS)
            UNITY_POSITION(pos);
            
            fixed4 tint_bumpstrength : COLOR;

        #if defined( FORWARD_LIGHTING )
            UNITY_LIGHTING_COORDS(1,2)
            UNITY_FOG_COORDS(3)
        #endif

            // //ehhhhh
            fixed3 lightDir : TEXCOORD4;
            fixed4 hueVariation : TEXCOORD5;
            
            // these three vectors will hold a 3x3 rotation matrix
            // that transforms from tangent to world space
            fixed4 tSpace0 : TEXCOORD6; // tangent.x, bitangent.x, normal.x, worldPos.x
            fixed4 tSpace1 : TEXCOORD7; // tangent.y, bitangent.y, normal.y, worldPos.y
            fixed4 tSpace2 : TEXCOORD8; // tangent.z, bitangent.z, normal.z, worldPos.z


//             //vertex lighting
// #if defined (DEFERRED_LIGHTING)
//             #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL 
//             fixed3 sh : TEXCOORD9; // SH
//             #endif
// #endif

            UNITY_VERTEX_OUTPUT_STEREO

#else
            V2F_SHADOW_CASTER;
#endif

        };




        void AddVertex (
            fixed distanceFade,
            inout g2f OUT, inout TriangleStream<g2f> triStream,
            fixed3 vertex, fixed2 uv, VertexBuffer V2G, fixed3 worldNormal
            
#if defined (COLOR_PASS)
            , fixed3 worldPos, fixed4 tangent, fixed4 hueVariation, fixed3 wTangent, fixed3 wBitangent
#endif
        ) 
            

        {

            UNITY_INITIALIZE_OUTPUT(g2f, OUT);
            
            OUT.uv_cutoff_distancemod = fixed4(uv, V2G.width_height_cutoff_bumpStrength.z, distanceFade); 

#if defined (COLOR_PASS)
            
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
            
            OUT.pos = UnityObjectToClipPos(vertex);
            
            OUT.tint_bumpstrength = fixed4(V2G.tint_color.rgb, V2G.width_height_cutoff_bumpStrength.w);
            OUT.hueVariation = hueVariation;

            // fixed3 wTangent = UnityObjectToWorldDir(tangent.xyz);
            // // compute bitangent from cross product of normal and tangent
            // fixed tangentSign = tangent.w * unity_WorldTransformParams.w;
            // fixed3 wBitangent = cross(worldNormal, wTangent) * tangentSign;
            
            // output the tangent space matrix
            OUT.tSpace0 = fixed4(wTangent.x, wBitangent.x, worldNormal.x, worldPos.x);
            OUT.tSpace1 = fixed4(wTangent.y, wBitangent.y, worldNormal.y, worldPos.y);
            OUT.tSpace2 = fixed4(wTangent.z, wBitangent.z, worldNormal.z, worldPos.z);
            
            // EEHHHHH
            OUT.lightDir = normalize(WorldSpaceLightDir (fixed4(vertex, 1)));

// #if defined (DEFERRED_LIGHTING)
//             #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
//                 OUT.sh = 0;                
//                 OUT.sh = ShadeSHPerVertex (worldNormal, OUT.sh);
//             #endif
// #endif

#if defined( FORWARD_LIGHTING )
            // Macro to send shadow & attenuation to the fragment shader.
            TRANSFER_VERTEX_TO_FRAGMENT(OUT, fixed4(worldPos, 1));              
            UNITY_TRANSFER_FOG(OUT, OUT.pos);
#endif



#else
            
            TRANSFER_SHADOW_CASTER_NORMALOFFSET(OUT, vertex, worldNormal, worldPos);

#endif



            triStream.Append(OUT);
        }

        void DrawQuadWind (
            inout g2f OUT, inout TriangleStream<g2f> triStream,
            fixed3 base, fixed3 top, 
            fixed distanceFade, fixed3 worldNormal, fixed4 uvOffset, 
            VertexBuffer V2G, fixed3 perpDir
#if defined (COLOR_PASS)
            , fixed3 obj2World, fixed4 objTangent, fixed3 wTangent, fixed3 wBitangent
#endif
        ) {
            
            fixed3 quadCorner = base - perpDir;

#if defined (COLOR_PASS)
            fixed3 quadCorner_world = quadCorner + obj2World;
            fixed4 quadHueVariation = fixed4(V2G.hue_variation.rgb, saturate( frac(abs(quadCorner_world.x + quadCorner_world.y + quadCorner_world.z)) * V2G.hue_variation.a));
#endif

            AddVertex (distanceFade, OUT, triStream, quadCorner, uvOffset.xy + fixed2(0, 0) * uvOffset.zw, V2G, worldNormal

#if defined (COLOR_PASS)
                , quadCorner_world, objTangent, quadHueVariation, wTangent, wBitangent
#endif
            );
            
            quadCorner = base + perpDir;
            AddVertex (distanceFade, OUT, triStream, quadCorner, uvOffset.xy + fixed2(1, 0) * uvOffset.zw, V2G, worldNormal

#if defined (COLOR_PASS)
                , quadCorner + obj2World, objTangent, quadHueVariation, wTangent, wBitangent
#endif
            );
            
            quadCorner = WaveGrass (top - perpDir, distanceFade);
            AddVertex (distanceFade, OUT, triStream, quadCorner, uvOffset.xy + fixed2(0, 1) * uvOffset.zw, V2G, worldNormal

#if defined (COLOR_PASS)
                , quadCorner + obj2World, objTangent, quadHueVariation, wTangent, wBitangent
#endif
            );
            
            quadCorner = WaveGrass (top + perpDir, distanceFade);
            AddVertex (distanceFade, OUT, triStream, quadCorner, uvOffset.xy + fixed2(1, 1) * uvOffset.zw, V2G, worldNormal

#if defined (COLOR_PASS)
                , quadCorner + obj2World, objTangent, quadHueVariation, wTangent, wBitangent
#endif
            );
            
            triStream.RestartStrip();
        }

        fixed4 _PCGRASS_CAMERA_RANGE;

        [maxvertexcount(16)]
        void geom(point VertexBuffer IN[1], inout TriangleStream<g2f> triStream)
        {
            fixed3 obj2World = fixed3(unity_ObjectToWorld[0].w, unity_ObjectToWorld[1].w, unity_ObjectToWorld[2].w);
            
            VertexBuffer vertBuffer = IN[0];
            
            fixed3 grass_point = vertBuffer.grass_point.xyz;

            fixed3 world_grass_point = grass_point + obj2World;

            fixed3 viewDir = _WorldSpaceCameraPos - world_grass_point;
            fixed cameraDistance = length(viewDir);


            fixed fadeStart = _PCGRASS_CAMERA_RANGE.x;
            fixed fadeEnd =_PCGRASS_CAMERA_RANGE.y;

            if (cameraDistance > fadeEnd)
                return;
            

            fixed distanceFade = 1.0 - saturate(max(cameraDistance - fadeStart, 0) / (fadeEnd - fadeStart));

            fixed3 groundNorm = IN[0].ground_normal;
            fixed width = IN[0].width_height_cutoff_bumpStrength.x;
            fixed height = IN[0].width_height_cutoff_bumpStrength.y;

            fixed4 uvOffset = IN[0].uv_offset_main;


            #if defined(NO_HEIGHT_FADE)

            fixed3 top = grass_point + groundNorm * (height);
            #else 
            fixed3 top = grass_point + groundNorm * (height * distanceFade);
            
            #endif

            fixed3 perpDir = fixed3(0, 0, 1);
            perpDir = cross(groundNorm, cross(perpDir, groundNorm));

            g2f OUT;

            // Quad 1
            fixed3 quad1Perp = perpDir * .5 * width;


#if defined (COLOR_PASS)
            fixed3 quad1Norm = cross(perpDir, groundNorm);
            fixed4 quad1Tang = fixed4(cross(quad1Norm, perpDir), 1);

            fixed3 wTangent = UnityObjectToWorldDir(quad1Tang.xyz);
            // compute bitangent from cross product of normal and tangent
            fixed tangentSign = quad1Tang.w * unity_WorldTransformParams.w;
            fixed3 wBitangent = cross(groundNorm, wTangent) * tangentSign;
            
#endif

            DrawQuadWind (OUT, triStream, grass_point, top, distanceFade, groundNorm, uvOffset, vertBuffer, quad1Perp

#if defined (COLOR_PASS)
                , obj2World, quad1Tang, wTangent, wBitangent            
#endif
            );
            
            
            fixed4x4 quad1Matrix = rotationMatrix(groundNorm, SIN90, COS90);
            DrawQuadWind (OUT, triStream, grass_point, top, distanceFade, groundNorm, uvOffset, vertBuffer, mul(quad1Matrix, quad1Perp)

#if defined (COLOR_PASS)
                , obj2World, quad1Tang, wTangent, wBitangent //mul(quad1Matrix, quad1Tang)
#endif
            );

            // Quad 2
            fixed4x4 quad2Matrix = rotationMatrix(groundNorm, -SIN45, COS45);
            DrawQuadWind (OUT, triStream, grass_point, top, distanceFade, groundNorm, uvOffset, vertBuffer, mul(quad2Matrix, quad1Perp)

#if defined (COLOR_PASS)
                , obj2World, quad1Tang, wTangent, wBitangent //mul(quad2Matrix, quad1Tang)
#endif
            );

            // // Quad 3
            fixed4x4 quad3Matrix = rotationMatrix(groundNorm, SIN45, COS45);
            DrawQuadWind (OUT, triStream, grass_point, top, distanceFade, groundNorm, uvOffset, vertBuffer, mul(quad3Matrix, quad1Perp)

#if defined (COLOR_PASS)
                , obj2World, quad1Tang, wTangent, wBitangent //mul(quad3Matrix, quad1Tang)
#endif
            );

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

                fixed3 tnormal = UnpackScaleNormal(tex2D(_BumpMap, uv), IN.tint_bumpstrength.a);
                // transform normal from tangent to world space
                fixed3 worldNormal = fixed3(
                    dot(IN.tSpace0.xyz, tnormal),
                    dot(IN.tSpace1.xyz, tnormal),
                    dot(IN.tSpace2.xyz, tnormal)
                );

                diffuseColor.rgb *= tint;
                                            
                fixed4 c = 0;

#if defined (FORWARD_BASE_LIGHTING)
                // Ambient term. Only do this in Forward Base. It only needs calculating once.
                c.rgb = (UNITY_LIGHTMODEL_AMBIENT.rgb * 2 * diffuseColor.rgb);         
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

    fixed2 uv = IN.uv_cutoff_distancemod.xy; 
    fixed4 diffuseColor = tex2D(_MainTex, uv); 
    clip(diffuseColor.a - cutoff); 

    SHADOW_CASTER_FRAGMENT(IN)                  
}

#endif














#endif // SPEEDTREE_COMMON_INCLUDED
