#ifndef GROUNDFOG_INCLUDED
#define GROUNDFOG_INCLUDED


#include "../Environment.cginc"
#include "../GeometryShaderUtils.cginc"

#define SOFT_PARTICLE

#define MOVE_COMPONENT z
#define SECONDARY_COMPONENT_0 x
#define SECONDARY_COMPONENT_1 y

#include "../MockParticles.cginc"

void AddVertex (
    float2x2 rotationMatrix, fixed3 billboardTangent, fixed3 basePoint, 
    inout g2f OUT, inout TriangleStream<g2f> stream, fixed3 vertex, fixed2 uv, fixed hueVariation, fixed distFade
)
{       
    fixed3 lVertex = vertex - basePoint;
    fixed3 billboardPos =  lVertex.x * billboardTangent;
    billboardPos.y = lVertex.y;
    vertex = billboardPos + basePoint;
    
    uv = (mul ( uv, rotationMatrix ));
    uv += .5;
    
    UNITY_INITIALIZE_OUTPUT(g2f, OUT);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    OUT.uv.xy = uv;
    
    OUT.pos = UnityObjectToClipPos(vertex);      

    OUT.projPos = ComputeScreenPos (OUT.pos);    
    OUT.projPos.z = -UnityObjectToViewPos( vertex ).z;

    PACK_HUE_VARIATION_AND_FADE(OUT, hueVariation, distFade)
    stream.Append(OUT);
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
    
    vertex.z += vertBuffer.uv.w;

    MoveAlongVector (vertex, noiseSample2);
        
    AddFallFlutter ( vertex, noiseSample1, noiseSample2 );

    fixed modulated = fmod(vertex.z, _MaxTravelDistance + (noiseSample1 * 2) + (_StartEndFade * 2) );

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

    fixed3 baseObjectPos = fixed3(unity_ObjectToWorld[0].w, unity_ObjectToWorld[1].w, unity_ObjectToWorld[2].w);
    
    fixed3 worldPos = baseObjectPos + vertex;

    CALCULATE_CAMERA_VARIABLES(worldPos)

    if (dot(CAMFWD, CAMDIR) < 0.5)
        return;

    fixed distFade = 1.0 - InverseLerp (_CameraRange.xy, CAMDIST);

    // fade in out based on distance from yPosition
    fixed dist = abs(worldPos.y - baseObjectPos.y);

    fixed heightFade = 1.0 - InverseLerp (_HeightRange_Steepness.xy, dist);
    heightFade = pow(heightFade, _HeightRange_Steepness.z);

    //fade out if too close to the camera
    fixed closeCamFade = InverseLerp (_CloseCamRange.xy, CAMDIST);

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

    fixed3 normal = fixed3(0,1,0);    
    vertex -= normal * (height * .5);
    fixed3 top = vertex + normal * (height);
    
    g2f OUT;
    
    fixed randomHueFactor = (sin(noiseSample1 * (vertex.x + vertex.y + vertex.z * noiseSample2 + _Time.y * 2)) * .5 + .5);
    fixed hueVariation = saturate( randomHueFactor * _HueVariation.a);

    
    fixed a = noiseSample2 + _Time.y * _RotateSpeed * (1 + (noiseSample1));
    float sinX = sin ( a );
    float cosX = cos ( a );
    float2x2 rotationMatrix = float2x2( cosX, -sinX, sinX, cosX);
    rotationMatrix *=0.5;
    rotationMatrix +=0.5;
    rotationMatrix = rotationMatrix * 2 - 1;
    

    fixed3 eyeVec = normalize(_WorldSpaceCameraPos - worldPos);
    fixed3 billboardTangent = normalize(fixed3(-eyeVec.z, 0, eyeVec.x));            // cross(eyeVec, {0,1,0})    
    fixed3 perpDir = fixed3(.5 * width, 0, 0);


    AddVertex (rotationMatrix, billboardTangent, vertex, OUT, stream, vertex - perpDir, fixed2(-.5, -.5), hueVariation, distFade);
    AddVertex (rotationMatrix, billboardTangent, vertex, OUT, stream, vertex + perpDir, fixed2(.5, -.5), hueVariation, distFade);        
    AddVertex (rotationMatrix, billboardTangent, vertex, OUT, stream, top - perpDir, fixed2(-.5, .5), hueVariation, distFade);
    AddVertex (rotationMatrix, billboardTangent, vertex, OUT, stream, top + perpDir, fixed2(.5, .5), hueVariation, distFade);
    
    // stream.RestartStrip();
}

#endif // SPEEDTREE_COMMON_INCLUDED
