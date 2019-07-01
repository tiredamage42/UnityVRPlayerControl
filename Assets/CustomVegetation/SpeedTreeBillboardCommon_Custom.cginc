// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

#ifndef SPEEDTREE_BILLBOARD_COMMON_INCLUDED
#define SPEEDTREE_BILLBOARD_COMMON_INCLUDED

#define SPEEDTREE_ALPHATEST
fixed _Cutoff;

// fixed _BillboardIndex;


#include "SpeedTreeCommon_Custom.cginc"

// CBUFFER_START(UnityBillboardPerCamera)
    // float3 unity_BillboardNormal;
    // float3 unity_BillboardTangent;
    // float4 unity_BillboardCameraParams;
    // #define unity_BillboardCameraPosition (unity_BillboardCameraParams.xyz)
    // #define unity_BillboardCameraXZAngle (unity_BillboardCameraParams.w)
// CBUFFER_END

// CBUFFER_START(UnityBillboardPerBatch)
    // float4 unity_BillboardInfo; // x: num of billboard slices; y: 1.0f / (delta angle between slices)
    
    // float4 unity_BillboardSize; // x: width; y: height; z: bottom (from billboard asset)
    
    float _BillboardSlices;
    float4 _BillboardSliceCoords[16];
// CBUFFER_END

struct SpeedTreeBillboardData
{
    float4 vertex       : POSITION;
    float2 texcoord     : TEXCOORD0;
    // float4 texcoord1    : TEXCOORD1;
    float3 normal       : NORMAL;
    float4 tangent      : TANGENT;
    // float4 color        : COLOR;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

void SpeedTreeBillboardVert(inout SpeedTreeBillboardData IN, out Input OUT)
{
    UNITY_INITIALIZE_OUTPUT(Input, OUT);

    // assume no scaling & rotation
    // float3 worldPos = IN.vertex.xyz + float3(unity_ObjectToWorld[0].w, unity_ObjectToWorld[1].w, unity_ObjectToWorld[2].w);
    float3 worldPos = float3(unity_ObjectToWorld[0].w, unity_ObjectToWorld[1].w, unity_ObjectToWorld[2].w);
    
    // float3 worldPos =  mul(unity_ObjectToWorld, IN.vertex.xyz);// float3(unity_ObjectToWorld[0].w, unity_ObjectToWorld[1].w, unity_ObjectToWorld[2].w);

// #ifdef BILLBOARD_FACE_CAMERA_POS
    float3 eyeVec = normalize(_WorldSpaceCameraPos - worldPos);
    // float3 eyeVec = normalize( - worldPos );
    
    // float3 eyeVec = normalize(unity_BillboardCameraPosition - worldPos);

    float3 billboardTangent = normalize(float3(-eyeVec.z, 0, eyeVec.x));            // cross(eyeVec, {0,1,0})
    float3 billboardNormal = float3(billboardTangent.z, 0, -billboardTangent.x);    // cross({0,1,0},billboardTangent)
    // float3 angle = atan2(billboardNormal.z, billboardNormal.x);                     // signed angle between billboardNormal to {0,0,1}
    // angle += angle < 0 ? 2 * UNITY_PI : 0;
// #else
    // float3 billboardTangent = unity_BillboardTangent;
    // float3 billboardNormal = unity_BillboardNormal;
    // float angle = unity_BillboardCameraXZAngle;
// #endif


    //per instance
    // float widthScale = 1;//IN.texcoord1.x;
    // float heightScale = 1;//IN.texcoord1.y;
    
    
    // float rotation = IN.texcoord1.z;
    
    // float2 percent = IN.texcoord.xy;

    //will be baked into mesh when exporting speed tree custom mesh
    // float billboardAssetWidth = 1;//13.55;//unity_BillboardSize.x;
    // float billboardAssetHeight = 1;//30.658;//unity_BillboardSize.y;
    // float billboardAssetBottom = 0;//-.7317;//unity_BillboardSize.z;
    // float billboardAssetWidth = unity_BillboardSize.x;
    // float billboardAssetHeight = unity_BillboardSize.y;
    // float billboardAssetBottom = unity_BillboardSize.z;
    

    // float3 billboardPos = (percent.x - 0.5) * billboardAssetWidth * widthScale * billboardTangent;
    // billboardPos.y = (percent.y * billboardAssetHeight + billboardAssetBottom) * heightScale;
    
    float3 billboardPos =  IN.vertex.x * billboardTangent;
    billboardPos.y = IN.vertex.y;
    
    // float3 billboardPos = float3((percent.x - 0.5) * widthScale, 0, 0);// * billboardTangent;
    // billboardPos.y += (percent.y) * heightScale;
    
    
    
    
    // float3 billboardPos = float3(
    //     IN.vertex.x , 
    //     IN.vertex.y, 
    //     0
    // )* billboardTangent;// (percent.x - 0.5f) * billboardAssetWidth * widthScale * billboardTangent;
    // billboardPos.y += (percent.y * billboardAssetHeight + billboardAssetBottom) * heightScale;
    
    // float3 billboardPos = (percent.x - 0.5f) * widthScale * billboardTangent;// * unity_BillboardSize.x * widthScale * billboardTangent;
    // billboardPos.y += (percent.y);// * unity_BillboardSize.y + unity_BillboardSize.z) * heightScale;
    

// #ifdef ENABLE_WIND
    // if (_WindQuality * _WindEnabled > 0)
    //     billboardPos = GlobalWind(billboardPos, worldPos, true, _ST_WindVector.xyz, IN.texcoord1.w);
// #endif

    // OUT.color = half4(IN.vertex.x, IN.vertex.y, IN.vertex.z, 1);

    IN.vertex.xyz = billboardPos;
    // IN.vertex.xz *= billboardTangent;
    
    IN.vertex.w = 1.0f;
    IN.normal = billboardNormal.xyz;
    IN.tangent = float4(billboardTangent.xyz,-1);

    // float slices = _BillboardSlices;// unity_BillboardInfo.x;

    
    // float invDelta = unity_BillboardInfo.y;
    // angle += rotation;

    // .0 .0 .2 .5
    // .2 .0 .2 .5
    // .4 .0 .2 .5
    // .6 .0 .2 .5
    // .8 .0 .2 .5
    
    // .0 .5 .2 .5
    // .2 .5 .2 .5
    // .4 .5 .2 .5

    float randFactor = frac(worldPos.x + worldPos.y + worldPos.z);

    float indexChose = abs(randFactor);
    indexChose *= _BillboardSlices;
    
    int imageIndex = int(indexChose);// fmod(_BillboardIndex, slices);// fmod(floor(angle * invDelta + 0.5f), slices);

    // float4 imageTexCoords = float4(percent.x, percent.y, 0, 0);// float4((1/slices) * (imageIndex/2),imageIndex%2,0,0);// 
    // float4 imageTexCoords = float4(.0, .5 ,.2, .45);// unity_BillboardImageTexCoords_custom[imageIndex];
    float4 imageTexCoords = _BillboardSliceCoords[imageIndex];
    
    // if (imageTexCoords.w < 0)
    // {
    //     OUT.mainTexUV = imageTexCoords.xy - imageTexCoords.zw * percent.yx;
    // }
    // else
    // {
        OUT.mainTexUV = imageTexCoords.xy + imageTexCoords.zw * IN.texcoord.xy;
    // }

    OUT.color = _Color;

// #ifdef EFFECT_HUE_VARIATION
    // float hueVariationAmount = frac(worldPos.x + worldPos.y + worldPos.z);
    OUT.HueVariationAmount = saturate(randFactor * _HueVariation.a);
// #endif
}

#endif // SPEEDTREE_BILLBOARD_COMMON_INCLUDED
