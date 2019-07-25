
#ifndef PRECIPITATION_INCLUDED
#define PRECIPITATION_INCLUDED

#include "../Environment.cginc"
#include "../GeometryShaderUtils.cginc"

#define MOVE_COMPONENT y
#define SECONDARY_COMPONENT_0 x
#define SECONDARY_COMPONENT_1 z

#include "../MockParticles.cginc"

void AddVertex (inout g2f OUT, inout TriangleStream<g2f> stream, fixed3 vertex, fixed2 uv, fixed hueVariation, fixed distFade)
{        
    UNITY_INITIALIZE_OUTPUT(g2f, OUT);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    OUT.uv.xy = uv;    
    OUT.pos = UnityObjectToClipPos(vertex);        
    PACK_HUE_VARIATION_AND_FADE(OUT, hueVariation, distFade)
    stream.Append(OUT);
}

void DrawQuad (inout g2f OUT, inout TriangleStream<g2f> stream, fixed3 base, fixed3 top, fixed3 perpDir, fixed hueVariation, fixed distFade) {
    
    AddVertex (OUT, stream, base - perpDir, fixed2(0, 0), hueVariation, distFade);
    AddVertex (OUT, stream, base + perpDir, fixed2(1, 0), hueVariation, distFade);
    AddVertex (OUT, stream, top - perpDir, fixed2(0, 1), hueVariation, distFade);
    AddVertex (OUT, stream, top + perpDir, fixed2(1, 1), hueVariation, distFade);
    stream.RestartStrip();
}

#if defined (BOTTOM_QUAD)
[maxvertexcount(12)]
void geom(point VertexBuffer IN[1], inout TriangleStream<g2f> stream)
#else
[maxvertexcount(8)]
void geom(point VertexBuffer IN[1], inout TriangleStream<g2f> stream)
#endif

{    
    UNITY_SETUP_INSTANCE_ID(IN[0]);

    VertexBuffer vertBuffer = IN[0];
    fixed2 uv = vertBuffer.uv.xy;
    fixed3 vertex = vertBuffer.vertex.xyz;

    fixed noiseSample1, noiseSample2, precipitationLevelFade;
    if (VertexBelowThreshold (vertex, uv, vertBuffer.uv.z, noiseSample1, noiseSample2, precipitationLevelFade)) {
        return;
    }

    float3x3 rotMatrix = (float3x3)_RotationMatrix;

    fixed3 rotatedVertex = mul(rotMatrix, vertex);    
    fixed3 rotatedVertexOffset = rotatedVertex - vertex;
    

    MoveAlongVector (vertex, noiseSample2);
    fixed simY = vertex.y;

    AddFallFlutter ( vertex, noiseSample1, noiseSample2 );

    vertex.y = fmod(vertex.y, -_MaxTravelDistance) + noiseSample1;
        
    vertex = mul(rotMatrix, vertex);
    vertex -= rotatedVertexOffset;

    fixed3 baseObjectPos = fixed3(unity_ObjectToWorld[0].w, unity_ObjectToWorld[1].w, unity_ObjectToWorld[2].w);
    


    CALCULATE_CAMERA_VARIABLES(baseObjectPos + vertex)

    if (dot(CAMFWD, CAMDIR) < 0.5)
        return;

    fixed distFade = 1.0 - InverseLerp (_CameraRange.xy, CAMDIST);

    distFade *= precipitationLevelFade;

    if (distFade <= 0)
        return;
    

    fixed width, height;
    CalculateWidthAndHeight (noiseSample1, noiseSample2, width, height);

    fixed3 groundNorm = mul(rotMatrix, fixed3(0,1,0));

    fixed3 top = vertex + groundNorm * (height);
    

    g2f OUT;
    // Quad 0
    fixed3 quad1Perp = fixed3(.5 * width, 0, 0);


#if defined (SPIN_QUAD)
    fixed a = simY + noiseSample2 + _Time.y * (1 + (noiseSample1 * 2 - 1));
    fixed3x3 spinMatrix = (fixed3x3)rotationMatrix(groundNorm, sin(a), cos(a));
    quad1Perp = mul(spinMatrix, quad1Perp);
#endif

    fixed randomHueFactor = (sin(noiseSample1 * (vertex.x + vertex.y * noiseSample2 + vertex.z + _Time.y * 2)) * .5 + .5);
    fixed hueVariation = saturate( randomHueFactor * _HueVariation.a);

    DrawQuad (OUT, stream, vertex, top, quad1Perp, hueVariation, distFade);//, diffuseLighting, ambientLighting);
    
    fixed3x3 quad1Matrix = (fixed3x3)rotationMatrix(groundNorm, SIN90, COS90);
    DrawQuad (OUT, stream, vertex, top, mul(quad1Matrix, quad1Perp), hueVariation, distFade);//, diffuseLighting, ambientLighting);
    
#if defined (BOTTOM_QUAD)
        //Snow needs quad facing down to not look so much like a cross

        fixed halfHeight = height * .5;
        fixed3 origGroundNorm = groundNorm;

        fixed3 right = fixed3(1,0,0);
#if defined (SPIN_QUAD)
        right = mul(spinMatrix, right);
#endif
        fixed3x3 quad2Matrix = (fixed3x3)rotationMatrix(right, SIN90, COS90);
        
        groundNorm = normalize(mul(quad2Matrix, groundNorm));
        top = vertex + groundNorm * height;

        fixed3 offsetA = groundNorm * halfHeight;            
        top -= offsetA;
        vertex -= offsetA;

        fixed3 offsetB = origGroundNorm * halfHeight;
        top += offsetB;
        vertex += offsetB;

        DrawQuad (OUT, stream, vertex, top, mul(quad2Matrix, quad1Perp), hueVariation, distFade);//, diffuseLighting, ambientLighting);
#endif
}






#endif // SPEEDTREE_COMMON_INCLUDED
