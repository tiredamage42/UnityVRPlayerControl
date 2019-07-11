#ifndef SPEEDTREE_VERTEX_INCLUDED
#define SPEEDTREE_VERTEX_INCLUDED

struct SpeedTreeVB
{
    fixed4 vertex       : POSITION;
    fixed4 tangent      : TANGENT;
    fixed3 normal       : NORMAL;
    fixed4 texcoord     : TEXCOORD0;
    fixed4 color         : COLOR;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

#include "../CustomWind.cginc"

void OffsetSpeedTreeVertex(inout SpeedTreeVB data)
{
    #if defined(GEOM_TYPE_LEAF)
        data.vertex.xyz = WaveLeaf (data.vertex.xyz);
    #endif
    data.vertex.xyz = WaveBranch (data.vertex.xyz);
}

#endif // SPEEDTREE_VERTEX_INCLUDED
