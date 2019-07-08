// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

#ifndef SPEEDTREE_VERTEX_INCLUDED
#define SPEEDTREE_VERTEX_INCLUDED

///////////////////////////////////////////////////////////////////////
//  struct SpeedTreeVB

// texcoord setup
//
//      BRANCHES                        FRONDS                      LEAVES
// 0    diffuse uv, branch wind xy      "                           "
// 1    lod xyz, 0                      lod xyz, 0                  anchor xyz, lod scalar
// 2    detail/seam uv, seam amount, 0  frond wind xyz, 0           leaf wind xyz, leaf group

struct SpeedTreeVB
{
    fixed4 vertex       : POSITION;
    fixed4 tangent      : TANGENT;
    fixed3 normal       : NORMAL;
    fixed4 texcoord     : TEXCOORD0;
    fixed4 texcoord1    : TEXCOORD1;
    fixed4 texcoord2    : TEXCOORD2;
    fixed4 color         : COLOR;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};


///////////////////////////////////////////////////////////////////////
//  SpeedTree winds

#define WIND_QUALITY_NONE       0
#define WIND_QUALITY_FASTEST    1
#define WIND_QUALITY_FAST       2
#define WIND_QUALITY_BETTER     3
#define WIND_QUALITY_BEST       4
#define WIND_QUALITY_PALM       5

uniform fixed _WindQuality;
// uniform fixed _WindEnabled;

#include "SpeedTreeWind_Custom.cginc"

///////////////////////////////////////////////////////////////////////
//  OffsetSpeedTreeVertex

void OffsetSpeedTreeVertex(inout SpeedTreeVB data)
{
    fixed3 finalPosition = data.vertex.xyz;

    fixed windQuality = _WindQuality * 0;// * _WindEnabled;

    fixed3 rotatedWindVector, rotatedBranchAnchor;
    if (windQuality <= WIND_QUALITY_NONE)
    {
        rotatedWindVector = fixed3(0.0f, 0.0f, 0.0f);
        rotatedBranchAnchor = fixed3(0.0f, 0.0f, 0.0f);
    }
    else
    {
        // compute rotated wind parameters
        rotatedWindVector = normalize(mul(_ST_WindVector.xyz, (fixed3x3)unity_ObjectToWorld));
        rotatedBranchAnchor = normalize(mul(_ST_WindBranchAnchor.xyz, (fixed3x3)unity_ObjectToWorld)) * _ST_WindBranchAnchor.w;
    }
    
    #if defined(GEOM_TYPE_BRANCH) || defined(GEOM_TYPE_FROND)

        // frond wind, if needed
        #if defined(GEOM_TYPE_FROND)
        
            if (windQuality == WIND_QUALITY_PALM)
                finalPosition = RippleFrond(finalPosition, data.normal, data.texcoord.x, data.texcoord.y, data.texcoord2.x, data.texcoord2.y, data.texcoord2.z);
        #endif

    #elif defined(GEOM_TYPE_LEAF)

        // remove anchor position
        finalPosition -= data.texcoord1.xyz;

        bool isFacingLeaf = data.color.a == 0;
        if (isFacingLeaf)
        {
            // face camera-facing leaf to camera
            fixed offsetLen = length(finalPosition);
            finalPosition = mul(finalPosition.xyz, (fixed3x3)UNITY_MATRIX_IT_MV); // inv(MV) * finalPosition
            finalPosition = normalize(finalPosition) * offsetLen; // make sure the offset vector is still scaled
        }

        // leaf wind
        if (windQuality > WIND_QUALITY_FASTEST && windQuality < WIND_QUALITY_PALM)
        {
            fixed leafWindTrigOffset = data.texcoord1.x + data.texcoord1.y;
            finalPosition = LeafWind(windQuality == WIND_QUALITY_BEST, data.texcoord2.w > 0.0, finalPosition, data.normal, data.texcoord2.x, fixed3(0,0,0), data.texcoord2.y, data.texcoord2.z, leafWindTrigOffset, rotatedWindVector);
        }
        
        // move back out to anchor
        finalPosition += data.texcoord1.xyz;

    #endif

    fixed3 treePos = fixed3(unity_ObjectToWorld[0].w, unity_ObjectToWorld[1].w, unity_ObjectToWorld[2].w);

    #ifndef GEOM_TYPE_MESH
        if (windQuality >= WIND_QUALITY_BETTER)
        {
            // branch wind (applies to all 3D geometry)
            finalPosition = BranchWind(windQuality == WIND_QUALITY_PALM, finalPosition, treePos, fixed4(data.texcoord.zw, 0, 0), rotatedWindVector, rotatedBranchAnchor);
        }
    #endif

    if (windQuality > WIND_QUALITY_NONE)
    {
        // global wind
        finalPosition = GlobalWind(finalPosition, treePos, true, rotatedWindVector, _ST_WindGlobal.x);
    }
    
    data.vertex.xyz = finalPosition;
}

#endif // SPEEDTREE_VERTEX_INCLUDED
