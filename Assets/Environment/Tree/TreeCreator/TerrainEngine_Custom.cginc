// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

#ifndef TERRAIN_ENGINE_INCLUDED
#define TERRAIN_ENGINE_INCLUDED

// Terrain engine shader helpers

CBUFFER_START(UnityTerrain)
    // grass
    fixed4 _WavingTint;
    float4 _WaveAndDistance;    // wind speed, wave size, wind amount, max sqr distance
    float4 _CameraPosition;     // .xyz = camera position, .w = 1 / (max sqr distance)
    float3 _CameraRight, _CameraUp;

CBUFFER_END


// ---- Vertex input structures

struct appdata_tree {
    float4 vertex : POSITION;       // position
    float4 tangent : TANGENT;       // directional AO
    float3 normal : NORMAL;         // normal
    fixed4 color : COLOR;           // .w = bend factor
    float4 texcoord : TEXCOORD0;    // UV
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

// ---- Grass helpers

// Calculate a 4 fast sine-cosine pairs
// val:     the 4 input values - each must be in the range (0 to 1)
// s:       The sine of each of the 4 values
// c:       The cosine of each of the 4 values
void FastSinCos (float4 val, out float4 s, out float4 c) {
    val = val * 6.408849 - 3.1415927;
    // powers for taylor series
    float4 r5 = val * val;                  // wavevec ^ 2
    float4 r6 = r5 * r5;                        // wavevec ^ 4;
    float4 r7 = r6 * r5;                        // wavevec ^ 6;
    float4 r8 = r6 * r5;                        // wavevec ^ 8;

    float4 r1 = r5 * val;                   // wavevec ^ 3
    float4 r2 = r1 * r5;                        // wavevec ^ 5;
    float4 r3 = r2 * r5;                        // wavevec ^ 7;


    //Vectors for taylor's series expansion of sin and cos
    float4 sin7 = {1, -0.16161616, 0.0083333, -0.00019841};
    float4 cos8  = {-0.5, 0.041666666, -0.0013888889, 0.000024801587};

    // sin
    s =  val + r1 * sin7.y + r2 * sin7.z + r3 * sin7.w;

    // cos
    c = 1 + r5 * cos8.x + r6 * cos8.y + r7 * cos8.z + r8 * cos8.w;
}

fixed4 TerrainWaveGrass (inout float4 vertex, float waveAmount, fixed4 color)
{
    float4 _waveXSize = float4(0.012, 0.02, 0.06, 0.024) * _WaveAndDistance.y;
    float4 _waveZSize = float4 (0.006, .02, 0.02, 0.05) * _WaveAndDistance.y;
    float4 waveSpeed = float4 (0.3, .5, .4, 1.2) * 4;

    float4 _waveXmove = float4(0.012, 0.02, -0.06, 0.048) * 2;
    float4 _waveZmove = float4 (0.006, .02, -0.02, 0.1);

    float4 waves;
    waves = vertex.x * _waveXSize;
    waves += vertex.z * _waveZSize;

    // Add in time to model them over time
    waves += _WaveAndDistance.x * waveSpeed;

    float4 s, c;
    waves = frac (waves);
    FastSinCos (waves, s,c);

    s = s * s;

    s = s * s;

    float lighting = dot (s, normalize (float4 (1,1,.4,.2))) * .7;

    s = s * waveAmount;

    float3 waveMove = float3 (0,0,0);
    waveMove.x = dot (s, _waveXmove);
    waveMove.z = dot (s, _waveZmove);

    vertex.xz -= waveMove.xz * _WaveAndDistance.z;

    // apply color animation

    // fix for dx11/etc warning
    fixed3 waveColor = lerp (fixed3(0.5,0.5,0.5), _WavingTint.rgb, fixed3(lighting,lighting,lighting));

    // Fade the grass out before detail distance.
    // Saturate because Radeon HD drivers on OS X 10.4.10 don't saturate vertex colors properly.
    float3 offset = vertex.xyz - _CameraPosition.xyz;
    color.a = saturate (2 * (_WaveAndDistance.w - dot (offset, offset)) * _CameraPosition.w);

    return fixed4(2 * waveColor * color.rgb, color.a);
}

void TerrainBillboardGrass( inout float4 pos, float2 offset )
{
    float3 grasspos = pos.xyz - _CameraPosition.xyz;
    if (dot(grasspos, grasspos) > _WaveAndDistance.w)
        offset = 0.0;
    pos.xyz += offset.x * _CameraRight.xyz;
    pos.xyz += offset.y * _CameraUp.xyz;
}

// Grass: appdata_full usage
// color        - .xyz = color, .w = wave scale
// normal       - normal
// tangent.xy   - billboard extrusion
// texcoord     - UV coords
// texcoord1    - 2nd UV coords

void WavingGrassVert (inout appdata_full v)
{
    // MeshGrass v.color.a: 1 on top vertices, 0 on bottom vertices
    // _WaveAndDistance.z == 0 for MeshLit
    float waveAmount = v.color.a * _WaveAndDistance.z;

    v.color = TerrainWaveGrass (v.vertex, waveAmount, v.color);
}

void WavingGrassBillboardVert (inout appdata_full v)
{
    TerrainBillboardGrass (v.vertex, v.tangent.xy);
    // wave amount defined by the grass height
    float waveAmount = v.tangent.y;
    v.color = TerrainWaveGrass (v.vertex, waveAmount, v.color);
}

float4 _Wind;

float4 SmoothCurve( float4 x ) {
    return x * x *( 3.0 - 2.0 * x );
}
float4 TriangleWave( float4 x ) {
    return abs( frac( x + 0.5 ) * 2.0 - 1.0 );
}
float4 SmoothTriangleWave( float4 x ) {
    return SmoothCurve( TriangleWave( x ) );
}

// Detail bending
inline float4 AnimateVertex(float4 pos, float3 normal, float4 animParams)
{
    // animParams stored in color
    // animParams.x = branch phase
    // animParams.y = edge flutter factor
    // animParams.z = primary factor
    // animParams.w = secondary factor

    float fDetailAmp = 0.1f;
    float fBranchAmp = 0.3f;

    // Phases (object, vertex, branch)
    float fObjPhase = dot(unity_ObjectToWorld._14_24_34, 1);
    float fBranchPhase = fObjPhase + animParams.x;

    float fVtxPhase = dot(pos.xyz, animParams.y + fBranchPhase);

    // x is used for edges; y is used for branches
    float2 vWavesIn = _Time.yy + float2(fVtxPhase, fBranchPhase );

    // 1.975, 0.793, 0.375, 0.193 are good frequencies
    float4 vWaves = (frac( vWavesIn.xxyy * float4(1.975, 0.793, 0.375, 0.193) ) * 2.0 - 1.0);

    vWaves = SmoothTriangleWave( vWaves );
    float2 vWavesSum = vWaves.xz + vWaves.yw;

    // Edge (xz) and branch bending (y)
    float3 bend = animParams.y * fDetailAmp * normal.xyz;
    bend.y = animParams.w * fBranchAmp;
    pos.xyz += ((vWavesSum.xyx * bend) + (_Wind.xyz * vWavesSum.y * animParams.w)) * _Wind.w;

    // Primary bending
    // Displace position
    pos.xyz += animParams.z * _Wind.xyz;

    return pos;
}

#endif
