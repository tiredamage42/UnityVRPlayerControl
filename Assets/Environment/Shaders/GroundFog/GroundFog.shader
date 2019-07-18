
Shader "Custom Environment/Ground Fog" {

	Properties{
        _ParticlesAmount ("Fog Amount", Range(0,1)) = 1
        
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _NoiseMap("Noise", 2D) = "white" {}
        
        _Color ("Color", Color) = (1,1,1,.5)
        _HueVariation ("Hue Variation", Color) = (1,.5,0, .1)

        _MoveSpeed ("Fall Speed", float) = .2

        _QuadDimensions("Quad Size", vector) = (1, 1, 0, 0)
        _SizeRange("Size Range", vector) = (1,2.5,0,0)
        
        _MaxTravelDistance ("Max Travel Distance", float) = 10

        _RotateSpeed ("Rotate Speed", float) = .1

        _SoftParticleFactor ("Soft Particle Factor", float) = 1
        _StartEndFade ("Start End Fade Range", float) = 2

        _CameraRange ("Camera Range", vector) = (0, 10, 0,0)
        _CloseCamRange ("Close Cam Range", vector) = (1,4, 0,0)
        _HeightRange_Steepness("Height Fade Range (XY) Steepness (Z)", vector) = (0,2,4, 0)

        _FlutterFrequency ("Flutter Frequency", vector) = (0.1, .1, 0,0)
        _FlutterSpeed ("Flutter Speed", vector) = (.1, .1, 0,0)
        _FlutterMagnitude ("Flutter Magnitude", vector) = (.35, .25, 0,0)


        // _WorldPos ("World Pos", vector) = (0,0,0,0)
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("BlendDestination", int) = 0
	}

    CGINCLUDE
    #pragma multi_compile_instancing
    // #pragma instancing_options procedural:setup

            
    // #pragma multi_compile_fog
    #pragma fragmentoption ARB_precision_hint_fastest
    #pragma vertex vert
    #pragma fragment frag
    #pragma geometry geom
    #pragma target 4.0
    // #pragma target 4.5


// #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
        // StructuredBuffer<float3> positionBuffer;
    // #endif

    //  void setup()
    //     {
        // #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            // float3 data = positionBuffer[unity_InstanceID];

            // unity_ObjectToWorld[0].w = data.x;
            // unity_ObjectToWorld[1].w = data.y;
            // unity_ObjectToWorld[2].w = data.z;

            // unity_ObjectToWorld._11_21_31_41 = float4(data.w, 0, 0, 0);
            // unity_ObjectToWorld._12_22_32_42 = float4(0, data.w, 0, 0);
            // unity_ObjectToWorld._13_23_33_43 = float4(0, 0, data.w, 0);
            // unity_ObjectToWorld._14_24_34_44 = float4(data.xyz, 1);
            // unity_WorldToObject = unity_ObjectToWorld;
            // unity_WorldToObject._14_24_34 *= -1;
            // unity_WorldToObject._11_22_33 = 1.0f / unity_WorldToObject._11_22_33;
        // #endif
        // }
    ENDCG
        

    SubShader{
        
        Tags{ 
            // "DisableBatching" = "True" 
            "Queue" = "Transparent" 
            // "Queue" = "AlphaTest" 
            
            "RenderType" = "Transparent" 
            "IgnoreProjector" = "True" 
        }
 
        // CULL FRONT
        Cull Off
        Blend SrcAlpha [_DstBlend]
        // Blend SrcAlpha OneMinusSrcAlpha
        // Blend SrcAlpha One //for additive
        ZWrite Off
        Pass {
            // Name "FORWARD"
            // Tags { "LightMode"="ForwardBase" }
            CGPROGRAM
            // #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap
            // #pragma nolightmap nodirlightmap nodynlightmap
            
            // #define FORWARD_LIGHTING
            // #define FORWARD_BASE_LIGHTING
            #include "GroundFog.cginc"            
            ENDCG
        }

        // Pass
        // {
        //     Name "FORWARD"
        //     Tags { "LightMode" = "ForwardAdd" }
        //     // ZWrite Off 
        //     Blend One One
        //     CGPROGRAM
        //     #pragma multi_compile_fwdadd_fullshadows nolightmap nodirlightmap nodynlightmap
        //     #define FORWARD_LIGHTING
        //     #define FORWARD_ADD_LIGHTING
        //     #include "GroundFog.cginc"   
        //     ENDCG
        // }
    }
}