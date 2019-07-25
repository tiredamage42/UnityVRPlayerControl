// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Custom Environment/Tree/SpeedTree Billboard"
{
    // Properties
    // {
    //     _MainTex ("Base (RGB)", 2D) = "white" {}
    //     _BumpMap ("Normalmap", 2D) = "bump" {}
    //     _Color ("Main Color", Color) = (1,1,1,1)
    //     _HueVariation ("Hue Variation", Color) = (1.0,0.5,0.0,0.1)
    //     _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    // }

    // SubShader
    // {
    //     CGPROGRAM
        
    //         #pragma surface surf Lambert vertex:SpeedTreeBillboardVert nolightmap nodynlightmap nodirlightmap nometa noforwardadd nolppv noshadowmask interpolateview halfasview
    //         #pragma fragmentoption ARB_precision_hint_fastest
    //         #pragma shader_feature EFFECT_BUMP
    //         #define USEFRAG
    //         #define SPEEDTREE_ALPHATEST
            
    //         fixed _Cutoff;
    //         #include "SpeedTreeCommon_Custom.cginc"

    //         fixed _BillboardSlices;
    //         fixed4 _BillboardSliceCoords[16];

    //         struct SpeedTreeBillboardData
    //         {
    //             fixed4 vertex       : POSITION;
    //             fixed2 texcoord     : TEXCOORD0;
    //             fixed3 normal       : NORMAL;
    //             fixed4 tangent      : TANGENT;
    //             UNITY_VERTEX_INPUT_INSTANCE_ID
    //         };

    //         void SpeedTreeBillboardVert(inout SpeedTreeBillboardData IN, out Input OUT)
    //         {
    //             UNITY_INITIALIZE_OUTPUT(Input, OUT);

    //             // assume no scaling & rotation TODO: ADD SCALING IF BILLBOARD POPS TOO MUCH
    //             fixed3 worldPos = fixed3(unity_ObjectToWorld[0].w, unity_ObjectToWorld[1].w, unity_ObjectToWorld[2].w);
                
    //             fixed3 eyeVec = normalize(_WorldSpaceCameraPos - worldPos);
    //             fixed3 billboardTangent = normalize(fixed3(-eyeVec.z, 0, eyeVec.x));            // cross(eyeVec, {0,1,0})
    //             fixed3 billboardNormal = fixed3(billboardTangent.z, 0, -billboardTangent.x);    // cross({0,1,0},billboardTangent)
                
    //             fixed3 billboardPos =  IN.vertex.x * billboardTangent;
    //             billboardPos.y = IN.vertex.y;
                
    //             IN.vertex.xyz = billboardPos;
    //             IN.vertex.w = 1.0f;
                
    //             IN.normal = billboardNormal.xyz;
    //             IN.tangent = fixed4(billboardTangent.xyz,-1);

    //             fixed randFactor = abs(frac(worldPos.x + worldPos.y + worldPos.z));

    //             fixed4 imageTexCoords = _BillboardSliceCoords[int(randFactor * _BillboardSlices)];
                
    //             // if (imageTexCoords.w < 0)
    //             // {
    //             //     OUT.mainTexUV = imageTexCoords.xy - imageTexCoords.zw * IN.texcoord.yx;
    //             // }
    //             // else
    //             // {
    //                 OUT.mainTexUV = imageTexCoords.xy + imageTexCoords.zw * IN.texcoord.xy;
    //             // }

    //             OUT.color = _Color;

    //             OUT.HueVariationAmount = saturate(randFactor * _HueVariation.a);
    //         }


    //         void surf(Input IN, inout SurfaceOutput OUT)
    //         {
    //             SpeedTreeFrag(IN, OUT);
    //         }
    //     ENDCG
    // }
}
