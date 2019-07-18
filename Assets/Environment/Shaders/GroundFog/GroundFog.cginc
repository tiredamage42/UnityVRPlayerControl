#ifndef GROUNDFOG_INCLUDED
#define GROUNDFOG_INCLUDED


// // VERTEX
// struct VertexBuffer
// {
//     fixed4 vertex : POSITION;
//     fixed4 uv : TEXCOORD0;
//     UNITY_VERTEX_INPUT_INSTANCE_ID
// };
    
// VertexBuffer vert(VertexBuffer v) { 
//     return v; 
// }

#include "Lighting.cginc"
#include "../Environment.cginc"
#include "../GeometryShaderUtils.cginc"

#define SOFT_PARTICLE

#define MOVE_COMPONENT z
#define SECONDARY_COMPONENT_0 x
#define SECONDARY_COMPONENT_1 y

#include "../MockParticles.cginc"

void AddVertex (
    float2x2 rotationMatrix, fixed3 billboardTangent, fixed3 basePoint, 
    inout g2f OUT, inout TriangleStream<g2f> stream, fixed3 vertex, fixed2 uv, fixed hueVariation, fixed distFade, fixed3 diffuseLighting, fixed3 ambientLighting)
{       

    fixed3 lVertex = vertex - basePoint;
    fixed3 billboardPos =  lVertex.x * billboardTangent;
    billboardPos.y = lVertex.y;
    
    vertex = billboardPos + basePoint;

    uv = (mul ( uv, rotationMatrix ));
    uv += .5;
    
    UNITY_INITIALIZE_OUTPUT(g2f, OUT);

    // OUT.diffLighting.xyz = diffuseLighting;
    // OUT.ambientLighting.xyz = ambientLighting;

    OUT.uv = uv;
    // OUT.diffLighting.a = uv.x;
    // OUT.ambientLighting.a = uv.y;

    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    
    OUT.pos = UnityObjectToClipPos(vertex);      

    OUT.projPos = ComputeScreenPos (OUT.pos);    
    OUT.projPos.z = -UnityObjectToViewPos( vertex ).z;

    OUT.hueVariation_distFade = fixed2(hueVariation, distFade);

// #if defined( FORWARD_LIGHTING )
//     TRANSFER_VERTEX_TO_FRAGMENT(OUT, fixed4(vertex, 1));              
//     UNITY_TRANSFER_FOG(OUT, OUT.pos);
// #endif

    stream.Append(OUT);
}


void DrawQuad (fixed3 basePos, fixed2x2 rotationMatrix, fixed3 up, inout g2f OUT, inout TriangleStream<g2f> stream, fixed3 base, fixed3 top, fixed3 perpDir, fixed hueVariation, fixed distFade, fixed3 diffuseLighting, fixed3 ambientLighting) {
    fixed3 worldPos = base + basePos;// fixed3(unity_ObjectToWorld[0].w, unity_ObjectToWorld[1].w, unity_ObjectToWorld[2].w);
    fixed3 eyeVec = normalize(_WorldSpaceCameraPos - worldPos);
    fixed3 billboardTangent = normalize(fixed3(-eyeVec.z, 0, eyeVec.x));            // cross(eyeVec, {0,1,0})

    AddVertex (rotationMatrix, billboardTangent, base, OUT, stream, base - perpDir, fixed2(-.5, -.5), hueVariation, distFade, diffuseLighting, ambientLighting);
    AddVertex (rotationMatrix, billboardTangent, base, OUT, stream, base + perpDir, fixed2(.5, -.5), hueVariation, distFade, diffuseLighting, ambientLighting);        
    AddVertex (rotationMatrix, billboardTangent, base, OUT, stream, top - perpDir, fixed2(-.5, .5), hueVariation, distFade, diffuseLighting, ambientLighting);
    AddVertex (rotationMatrix, billboardTangent, base, OUT, stream, top + perpDir, fixed2(.5, .5), hueVariation, distFade, diffuseLighting, ambientLighting);
    
    // stream.RestartStrip();
}


fixed3 _HeightRange_Steepness;
fixed2 _CloseCamRange;
fixed _StartEndFade;
fixed _RotateSpeed;


[maxvertexcount(4)]
void geom(point VertexBuffer IN[1], inout TriangleStream<g2f> stream)
{    
    UNITY_SETUP_INSTANCE_ID(IN[0]);

    VertexBuffer vertBuffer = IN[0];

    fixed3 vertex = vertBuffer.vertex.xyz;
    fixed2 uv = vertBuffer.uv.xy;
    
    fixed noiseSample1, noiseSample2, precipitationLevelFade;
    if (VertexBelowThreshold (vertex, uv, vertBuffer.uv.z, noiseSample1, noiseSample2, precipitationLevelFade)) {
        return;
    }
    
    // float3x3 rotMatrix = (float3x3)_RotationMatrix;

    vertex.z += vertBuffer.uv.w;

    MoveAlongVector (vertex, noiseSample2);
        
    fixed simY = vertex.z;

    AddFallFlutter ( vertex, noiseSample1, noiseSample2 );

    fixed modulated = fmod(vertex.z, _MaxTravelDistance + noiseSample1 * 2 + _StartEndFade * 2 );

    vertex.z = modulated - noiseSample1 * 2 - _StartEndFade;

    // fade out towards the end of travel to prevent popping out
    fixed endFade = 1.0 - saturate(max((modulated + _StartEndFade) - (_MaxTravelDistance + _StartEndFade * 2), 0) / _StartEndFade);
    
    // fade in at beggining to prevent popping in
    fixed startFade = 1;
    if (vertex.z <= 0) {
        fixed startDist = abs(vertex.z);
        startFade = 1.0 - saturate(startDist / (noiseSample1 * 2 + _StartEndFade));
    }

    vertex.z -= vertBuffer.uv.w;

    vertex = mul((float3x3)_RotationMatrix, vertex);





    // unity_ObjectToWorldArray[unity_InstanceID]
    fixed3 baseObjectPos = fixed3(unity_ObjectToWorld[0].w, unity_ObjectToWorld[1].w, unity_ObjectToWorld[2].w);
    // fixed3 baseObjectPos = fixed3(unity_ObjectToWorldArray[unity_InstanceID][0].w, unity_ObjectToWorldArray[unity_InstanceID][1].w, unity_ObjectToWorldArray[unity_InstanceID][2].w);
    
    fixed3 worldPos = baseObjectPos + vertex;
    
    float3 camForward = mul((float3x3)unity_CameraToWorld, float3(0,0,1));
    camForward.y = 0;
    camForward = normalize(camForward);

    fixed3 camDir = worldPos - _WorldSpaceCameraPos;
    camDir.y = 0;
    camDir = normalize(camDir);
    if (dot(camForward, camDir) <= 0.25)
        return;

    


    
    fixed camDistance;
    fixed distFade = CalculateDistanceFade (worldPos, _CameraRange, camDistance);

    // fade in out based on distance from yPosition
    fixed groundY = baseObjectPos.y;
    fixed dist = abs(worldPos.y - baseObjectPos.y);

    fixed heightFade = 1.0 - saturate(max(dist - _HeightRange_Steepness.x, 0) / (_HeightRange_Steepness.y - _HeightRange_Steepness.x));
    heightFade = pow(heightFade, _HeightRange_Steepness.z);

    //fade out if too close to the camera
    fixed closeCamFade = saturate(max(camDistance - _CloseCamRange.x, 0) / (_CloseCamRange.y - _CloseCamRange.x));
    
    // distFade = 1;
    distFade *= precipitationLevelFade;
    distFade *= heightFade;
    distFade *= closeCamFade;
    distFade *= endFade;
    distFade *= startFade;
    if (distFade <= 0)
        return;

    fixed width = .25, height = .25;
    CalculateWidthAndHeight (noiseSample1, noiseSample2, width, height);

    fixed3 groundNorm = fixed3(0,1,0);    
    vertex -= groundNorm * (height * .5);
    fixed3 top = vertex + groundNorm * (height);
    
    g2f OUT;
    
    fixed randomHueFactor = (sin(noiseSample1 * (vertex.x + vertex.y + vertex.z * noiseSample2 + _Time.y * 2)) * .5 + .5);
    fixed hueVariation = saturate( randomHueFactor * _HueVariation.a);

    fixed3 ambientLighting = 0;
    fixed3 diffuseLighting = 0;//CalculateLighting(ambientLighting, vertex);
    


    
    fixed a = noiseSample2 + _Time.y * _RotateSpeed * (1 + (noiseSample1));
    float sinX = sin ( a );
    float cosX = cos ( a );
    float2x2 rotationMatrix = float2x2( cosX, -sinX, sinX, cosX);
    rotationMatrix *=0.5;
    rotationMatrix +=0.5;
    rotationMatrix = rotationMatrix * 2-1;
    


    DrawQuad (baseObjectPos, rotationMatrix, groundNorm, OUT, stream, vertex, top, fixed3(.5 * width, 0, 0), hueVariation, distFade, diffuseLighting, ambientLighting);
    
}

#endif // SPEEDTREE_COMMON_INCLUDED
