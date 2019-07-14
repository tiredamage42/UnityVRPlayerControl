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
    // fixed3 ground_normal : NORMAL;
    // fixed4 tint_color : COLOR;
    // fixed4 uv_offset_main : TEXCOORD0;
    // fixed4 uv_offset_bump : TEXCOORD1;
    // fixed4 cutoff_shadowcutoff : TEXCOORD2;
    // fixed4 hue_variation : TEXCOORD3;
    // fixed4 width_height_bumpStrength : TEXCOORD4;
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

        fixed3 diffLighting : TEXCOORD4;
        fixed3 ambientLighting : TEXCOORD9;
        fixed4 hueVariation : TEXCOORD5;
        
        // these three vectors will hold a 3x3 rotation matrix that transforms from tangent to world space
        // fixed4 tSpace0 : TEXCOORD6; // tangent.x, bitangent.x, normal.x, worldPos.x
        // fixed4 tSpace1 : TEXCOORD7; // tangent.y, bitangent.y, normal.y, worldPos.y
        // fixed4 tSpace2 : TEXCOORD8; // tangent.z, bitangent.z, normal.z, worldPos.z

//      //vertex lighting
// #if defined (DEFERRED_LIGHTING) && UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL 
//     fixed3 sh : TEXCOORD9; 
// #endif

        UNITY_VERTEX_OUTPUT_STEREO
#else
        V2F_SHADOW_CASTER;
#endif

    };

#if defined (COLOR_PASS)
    #define ALPHA_CUTOFF 0//v.cutoff_shadowcutoff.x
#else 
    #define ALPHA_CUTOFF 0//v.cutoff_shadowcutoff.y
#endif






// void SpeedTreeBillboardVert(inout SpeedTreeBillboardData IN, out Input OUT)
// {
//     UNITY_INITIALIZE_OUTPUT(Input, OUT);

//     // assume no scaling & rotation TODO: ADD SCALING IF BILLBOARD POPS TOO MUCH
    
//     fixed3 worldPos = mul((float3x3)unity_ObjectToWorld, v.vertex.xyz);
//     fixed3 eyeVec = normalize(_WorldSpaceCameraPos - worldPos);
//     fixed3 billboardTangent = normalize(fixed3(-eyeVec.z, 0, eyeVec.x));            // cross(eyeVec, {0,1,0})
//     fixed3 billboardNormal = fixed3(billboardTangent.z, 0, -billboardTangent.x);    // cross({0,1,0},billboardTangent)
    
//     fixed3 billboardPos =  IN.vertex.x * billboardTangent;
//     billboardPos.y = IN.vertex.y;
    
//     IN.vertex.xyz = billboardPos;
//     IN.vertex.w = 1.0f;
    
//     IN.normal = billboardNormal.xyz;
//     IN.tangent = fixed4(billboardTangent.xyz,-1);

//     fixed randFactor = abs(frac(worldPos.x + worldPos.y + worldPos.z));

//     fixed4 imageTexCoords = _BillboardSliceCoords[int(randFactor * _BillboardSlices)];
    
//     // if (imageTexCoords.w < 0)
//     // {
//     //     OUT.mainTexUV = imageTexCoords.xy - imageTexCoords.zw * IN.texcoord.yx;
//     // }
//     // else
//     // {
//         OUT.mainTexUV = imageTexCoords.xy + imageTexCoords.zw * IN.texcoord.xy;
//     // }

//     OUT.color = _Color;

//     OUT.HueVariationAmount = saturate(randFactor * _HueVariation.a);
// }


























    sampler2D _FallNoise;
    fixed _FieldWidth;

    fixed4x4 _RainRotationMatrix;



    fixed3 CalculateLighting (out fixed3 ambient, fixed3 vertex) {
        
        // fixed3 c;
        ambient = 0;
    #if defined (FORWARD_BASE_LIGHTING)
        // Ambient term. Only do this in Forward Base. It only needs calculating once.
        ambient = (UNITY_LIGHTMODEL_AMBIENT.rgb * 2);         
    #endif
        return saturate(dot(fixed3(0,1,0), normalize(UnityWorldSpaceLightDir (vertex)))) * 2 * _LightColor0.rgb;

        //out.diffLighting
        // out.ambientLighting
    }



    void AddVertex (fixed3 basePoint, fixed distFade, inout g2f OUT, inout TriangleStream<g2f> stream, VertexBuffer v, fixed3 vertex, fixed2 uv, fixed3 normal, fixed3 tangent, fixed3 bitangent, fixed4 hueVariation) 
    {

        
        UNITY_INITIALIZE_OUTPUT(g2f, OUT);


        fixed3 baseObjectPos = fixed3(unity_ObjectToWorld[0].w, unity_ObjectToWorld[1].w, unity_ObjectToWorld[2].w);
        

        // fixed origY = vertex.y;
        // vertex = mul((float3x3)_RainRotationMatrix, vertex);
        // vertex.y = origY;

        // fixed3 up = fixed3(0,1,0);// mul((float3x3)_RainRotationMatrix, fixed3(0,1,0));
        fixed3 up = mul((float3x3)_RainRotationMatrix, fixed3(0,1,0));//verex.xyz);


        

        // vertex = fixed3(uv, 0);
        // vertex.x -= .5;

        // fixed3 worldPos = basePoint;//vertex;//mul((float3x3)unity_ObjectToWorld, vertex.xyz);
        // fixed3 worldPos = basePoint + baseObjectPos;// mul((float3x3)unity_ObjectToWorld, basePoint);//verex.xyz);
        // fixed3 worldPos = mul((float3x3)_RainRotationMatrix, basePoint + baseObjectPos);//verex.xyz);
        



        // fixed3 eyeVec = normalize(_WorldSpaceCameraPos - worldPos);
        
        // fixed3 billboardTangent = cross(eyeVec, up);//fixed3(0,1,0));            // cross(eyeVec, {0,1,0})
        // billboardTangent = mul((float3x3)_RainRotationMatrix, billboardTangent);//verex.xyz);
        
        // // fixed3 billboardTangent = normalize(fixed3(-eyeVec.z, 0, eyeVec.x));            // cross(eyeVec, {0,1,0})
        
        
        // // fixed3 billboardNormal = cross(fixed3(0,1,0), billboardTangent);
        // fixed3 billboardNormal = cross(up, billboardTangent);
        // // fixed3 billboardNormal = fixed3(billboardTangent.z, 0, -billboardTangent.x);    // cross({0,1,0},billboardTangent)
        

        // fixed3 localV = vertex - basePoint;//basePoint;
        // fixed3 billboardPos = localV.x * billboardTangent;
        // billboardPos.y = localV.y;

        // billboardPos.z = 1;
        
        // vertex.xyz += billboardPos;
        
        // vertex = billboardPos + basePoint;
        
        // vertex.z = -billboardPos.z;

        

        
        
        // vertex.w = 1.0f;
        
        // normal = billboardNormal.xyz;
        // tangent = fixed4(billboardTangent.xyz,-1);

        
        // _RainRotationMatrix[0].w = 100;//vertex.x;
        // _RainRotationMatrix[1].w = 100;//vertex.y;
        // _RainRotationMatrix[2].w = 100;//vertex.z;

        // float4x4 mat = float4x4(
        //     _RainRotationMatrix[0].xyz, 100,
        //     _RainRotationMatrix[1].xyz, 100,
        //     _RainRotationMatrix[2].xyz, 100,
        //     0,0,0,0

        // );
        // vertex = mul((float3x3)_RainRotationMatrix, vertex);


// fixed origY = vertex.y;
//         vertex = mul((float3x3)_RainRotationMatrix, vertex);



// fixed angle = 1;
// fixed4x4 rotMatrixRight = rotationMatrix(fixed3(1,0,0), sin(angle), cos(angle));


// fixed3 offset = baseObjectPos - basePoint;

// vertex -= offset;

// vertex = mul((float3x3)rotMatrixRight, vertex);
        
// vertex += offset;

        // vertex.y = origY;






        fixed2 mainUV = uv;// v.uv_offset_main.xy + uv * v.uv_offset_main.zw;
        
        OUT.uv_cutoff_distancemod = fixed4(mainUV, ALPHA_CUTOFF, distFade); 
        

#if defined (COLOR_PASS)

        OUT.diffLighting = CalculateLighting(OUT.ambientLighting, vertex);

        
        // fixed2 bumpUV = uv;// v.uv_offset_bump.xy + uv * v.uv_offset_bump.zw;
        // OUT.bump_uv = fixed4(bumpUV, 0, 0);
        
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
        
        OUT.pos = UnityObjectToClipPos(vertex);
        
        // OUT.tint_bumpstrength = fixed4(v.tint_color.rgb, v.width_height_bumpStrength.z);
        OUT.hueVariation = hueVariation;

        // output the tangent space matrix
        // OUT.tSpace0 = fixed4(tangent.x, bitangent.x, normal.x, vertex.x);
        // OUT.tSpace1 = fixed4(tangent.y, bitangent.y, normal.y, vertex.y);
        // OUT.tSpace2 = fixed4(tangent.z, bitangent.z, normal.z, vertex.z);
        
        // OUT.lightDir = normalize(UnityWorldSpaceLightDir (vertex));

// #if defined (DEFERRED_LIGHTING)
//             #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
//                 OUT.sh = 0;                
//                 OUT.sh = ShadeSHPerVertex (normal, OUT.sh);
//             #endif
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

    fixed4 _HueVariation;

    void DrawQuadWind (inout g2f OUT, inout TriangleStream<g2f> stream, fixed3 base, fixed3 top, fixed distFade, fixed3 normal, VertexBuffer v, fixed3 perpDir, fixed3 wTangent, fixed3 wBitangent) {
        
        fixed3 quadCorner = base - perpDir;
        fixed quadRandom = frac(abs(quadCorner.x + quadCorner.y + quadCorner.z) * 2);

        fixed flipUV = 0;// floor(quadRandom+0.5); //flip uv randomly per quad

        fixed4 hueVariation = 0;
#if defined (COLOR_PASS)
        hueVariation = fixed4(_HueVariation.rgb, saturate( quadRandom * _HueVariation.a));
#endif

        AddVertex (base, distFade, OUT, stream, v, quadCorner, fixed2(flipUV, 0), normal, wTangent, wBitangent, hueVariation);
        
        quadCorner = base + perpDir;
        AddVertex (base, distFade, OUT, stream, v, quadCorner, fixed2(1-flipUV, 0), normal, wTangent, wBitangent, hueVariation);
        
        quadCorner = (top - perpDir);
        AddVertex (base, distFade, OUT, stream, v, quadCorner,  fixed2(flipUV, 1), normal, wTangent, wBitangent, hueVariation);
        
        quadCorner = (top + perpDir);
        AddVertex (base, distFade, OUT, stream, v, quadCorner, fixed2(1-flipUV, 1), normal, wTangent, wBitangent, hueVariation);
        
        stream.RestartStrip();
    }

    fixed4 _PCGRASS_CAMERA_RANGE;

























fixed _RainSpeed;

fixed2 _RainDimensions;
fixed _MaxRainTravelDistance;

fixed _RainAmount;





    [maxvertexcount(16)]
    void geom(point VertexBuffer IN[1], inout TriangleStream<g2f> stream)
    {

        
        VertexBuffer vertBuffer = IN[0];

        fixed angle = 10;
        fixed4x4 rotMatrixRight = _RainRotationMatrix;// rotationMatrix(fixed3(1,0,0), sin(angle), cos(angle));
    
        

        fixed3 baseObjectPos = fixed3(unity_ObjectToWorld[0].w, unity_ObjectToWorld[1].w, unity_ObjectToWorld[2].w);
        
        fixed3 grass_point = vertBuffer.grass_point.xyz;

        fixed3 rotatingGrass = mul((float3x3)rotMatrixRight, grass_point);//verex.xyz);
        
        fixed3 yOffset = rotatingGrass - grass_point;
        // yOffset *= .1;
        // grass_point.y = baseObjectPos.y;
        
        fixed2 fallnoiseUV = fixed2(
            (grass_point.x + _FieldWidth * .5) / _FieldWidth,
            (grass_point.z + _FieldWidth * .5) / _FieldWidth
        );


        fixed noiseSample = frac(tex2Dlod(_FallNoise, fixed4(fallnoiseUV, 0, 0)).r + frac(
            baseObjectPos.x + baseObjectPos.y + baseObjectPos.z
            + grass_point.x + grass_point.y + grass_point.z
        ));
        
        if (noiseSample > _RainAmount) {
            return;
        }
            

        fixed noiseSample2 = frac(tex2Dlod(_FallNoise, fixed4(fallnoiseUV + _Time.y * .1, 0, 0)).r + frac(
            baseObjectPos.x * baseObjectPos.y * baseObjectPos.z
            * grass_point.x * grass_point.y * grass_point.z 
        ));
        // fixed noiseSample = textureLod(_FallNoise, fallnoiseUV, 0);


        // fixed3 rainDirection = normalize(fixed3(0,-1,0));
        // fixed3 rainDirection = fixed3(0,-1,0);



        


        grass_point.y += -1 * _Time.y * (_RainSpeed + (_RainSpeed * noiseSample));

        // grass_point.y -= _Time.y * (_RainSpeed + (noiseSample * 2));
        grass_point.y = fmod(grass_point.y, -_MaxRainTravelDistance);// - (noiseSample2);



//noise for snowfall
        grass_point.x += sin(grass_point.y * noiseSample * 2 + _Time.y * 3 * noiseSample) * .1;
        grass_point.z += cos(grass_point.y * noiseSample * 3 + _Time.y * 2 * noiseSample) * .1;
        
        



        grass_point = mul((float3x3)rotMatrixRight, grass_point);
        grass_point -= yOffset;


        // fixed3 viewDir = _WorldSpaceCameraPos - grass_point;
        // fixed cameraDistance = length(viewDir);

        // fixed fadeStart = _PCGRASS_CAMERA_RANGE.x;
        // fixed fadeEnd =_PCGRASS_CAMERA_RANGE.y;

        // if (cameraDistance > fadeEnd)
        //     return;
        
        fixed distFade = 1;//1.0 - saturate(max(cameraDistance - fadeStart, 0) / (fadeEnd - fadeStart));

        fixed3 groundNorm = fixed3(0,1,0);//IN[0].ground_normal;




        fixed width = _RainDimensions.x + (_RainDimensions.x * noiseSample);//IN[0].width_height_bumpStrength.x;
        fixed height = _RainDimensions.y + (_RainDimensions.y * noiseSample);//IN[0].width_height_bumpStrength.y;

        
        groundNorm = mul((float3x3)rotMatrixRight, groundNorm);


        // #if defined(NO_HEIGHT_FADE)
            fixed3 top = grass_point + groundNorm * (height);
        // #else 
        //     fixed3 top = grass_point + groundNorm * (height * distFade);
        // #endif

        // fixed3 perpDir = cross(groundNorm, cross(fixed3(0, 0, 1), groundNorm));
        fixed3 perpDir = fixed3(1, 0, 0);//cross(groundNorm, cross(fixed3(0, 0, 1), groundNorm));

        g2f OUT;

        fixed3 wTangent = 0, wBitangent = 0;

        



        fixed3 quad1Norm = cross(perpDir, groundNorm);
        wTangent = cross(quad1Norm, perpDir);
#if defined (COLOR_PASS)   



        // compute bitangent from cross product of normal and tangent
        wBitangent = cross(groundNorm, wTangent) * unity_WorldTransformParams.w;
#endif

        fixed quads = 4 * distFade;

        // Quad 0
        fixed3 quad1Perp = perpDir * .5 * width;

        
        




        DrawQuadWind (OUT, stream, grass_point, top, distFade, groundNorm, vertBuffer, quad1Perp, wTangent, wBitangent);
        
        fixed4x4 quad1Matrix = rotationMatrix(groundNorm, SIN90, COS90);
        DrawQuadWind (OUT, stream, grass_point, top, distFade, groundNorm, vertBuffer, mul(quad1Matrix, quad1Perp), wTangent, wBitangent );
        


        // fixed 
        angle = 90 * 0.01745329251;
        fixed4x4 quad2Matrix = rotationMatrix(fixed3(1,0,0), sin(angle), cos(angle));
        // fixed4x4 quad2Matrix = rotationMatrix(groundNorm, -SIN45, COS45);


        fixed3 origGroundNorm = groundNorm;
        // top = mul((float3x3)quad2Matrix, top);
        groundNorm = normalize(mul((float3x3)quad2Matrix, groundNorm));
        top = grass_point + groundNorm * (height);

        top -= groundNorm * (height * .5);
        grass_point -= groundNorm * (height * .5);

        top += origGroundNorm * (height * .5);
        grass_point += origGroundNorm * (height * .5);



//just snow
        
        // DrawQuadWind (OUT, stream, grass_point, top, distFade, groundNorm, vertBuffer, mul(quad2Matrix, quad1Perp), wTangent, wBitangent);
        DrawQuadWind (OUT, stream, grass_point, top, distFade, groundNorm, vertBuffer, quad1Perp, wTangent, wBitangent);

        
        
        
        
        // if (quads >= 1) {
        //     // Quad 1

        //     if (quads >= 2) {
        //         // Quad 2
        //         fixed4x4 quad2Matrix = rotationMatrix(groundNorm, -SIN45, COS45);
        //         DrawQuadWind (OUT, stream, grass_point, top, distFade, groundNorm, vertBuffer, mul(quad2Matrix, quad1Perp), wTangent, wBitangent);

        //         if (quads >= 3) {
        //             // Quad 3
        //             fixed4x4 quad3Matrix = rotationMatrix(groundNorm, SIN45, COS45);
        //             DrawQuadWind (OUT, stream, grass_point, top, distFade, groundNorm, vertBuffer, mul(quad3Matrix, quad1Perp), wTangent, wBitangent);
        //         }
        //     }
        // }
    }


#if defined(COLOR_PASS)

    // sampler2D _BumpMap;

    fixed4 _RainColor;




    fixed4 frag(g2f IN) : SV_Target
    {
        fixed2 uv = IN.uv_cutoff_distancemod.xy;

        // return fixed4(uv,0,1);
        // fixed cutoff = IN.uv_cutoff_distancemod.z;

        // fixed3 tint = IN.tint_bumpstrength.rgb;
        
        fixed4 diffuseColor = tex2D(_MainTex, uv);
        // return fixed4(0,1,0,1);
        // diffuseColor.a = 0;

        // return fixed4(diffuseColor.aaa, diffuseColor.a * .1);

        // clip(diffuseColor.a - cutoff);

        // add hue variation
        AddHueVariation(diffuseColor.rgb, IN.hueVariation);

        // sample the normal map, and decode from the Unity encoding
        // fixed3 tnormal = UnpackScaleNormal(tex2D(_BumpMap, IN.bump_uv.xy), IN.tint_bumpstrength.a);
        // transform normal from tangent to world space
        // fixed3 worldNormal = fixed3(0,1,0);// fixed3(
        //     dot(IN.tSpace0.xyz, tnormal), dot(IN.tSpace1.xyz, tnormal), dot(IN.tSpace2.xyz, tnormal)
        // );

        diffuseColor *= _RainColor;
                                    
        fixed4 c = 0;
        // c = diffuseColor;

#if defined (FORWARD_BASE_LIGHTING)
        // Ambient term. Only do this in Forward Base. It only needs calculating once.
        // c.rgb = (UNITY_LIGHTMODEL_AMBIENT.rgb * 2 * diffuseColor.rgb);     
        c.rgb = IN.ambientLighting * diffuseColor.rgb;    
#endif

        // Macro to get you the combined shadow & attenuation value.
        fixed atten = LIGHT_ATTENUATION(IN); 
        
        // fixed3 diff = saturate(dot(worldNormal, IN.lightDir)) * 2 * _LightColor0.rgb;
        
        c.rgb += (diffuseColor.rgb) * (IN.diffLighting) * atten; 
        
        c.a = diffuseColor.a;

        UNITY_APPLY_FOG(IN.fogCoord, c);

        return c;
    }



#else

    // fixed GRASS_SHADOW_CASTER_FRAG(g2f IN, fixed cutoff) {

    //     fixed2 uv = IN.uv_cutoff_distancemod.xy; 
    //     fixed4 diffuseColor = tex2D(_MainTex, uv); 
    //     clip(diffuseColor.a - cutoff); 

    //     SHADOW_CASTER_FRAGMENT(IN)                  
    // }

#endif














#endif // SPEEDTREE_COMMON_INCLUDED
