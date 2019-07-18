
Shader "Custom Environment/Rain" {

	Properties{
        _ParticlesAmount ("Precipitation Amount", Range(0,1)) = 1
        
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _NoiseMap("Fall Noise", 2D) = "white" {}
        
        _Color ("Color", Color) = (1,1,1,.5)
        _HueVariation ("Hue Variation", Color) = (1,.5,0, .1)

        _MoveSpeed ("Fall Speed", float) = -5
        _QuadDimensions("Quad Size", vector) = (.0025, .25, 0, 0)
        _SizeRange("Size Range", vector) = (.5,1,0,0)

        _MaxTravelDistance ("Max Travel Distance", float) = 15

        _CameraRange ("Camera Range", vector) = (0, 15, 0,0)

        _FlutterFrequency ("Flutter Frequency", vector) = (0.988, 1.234, 0,0)
        _FlutterSpeed ("Flutter Speed", vector) = (.01, .01, 0,0)
        _FlutterMagnitude ("Flutter Magnitude", vector) = (.35, .25, 0,0)
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("BlendDestination", int) = 0
	}

    CGINCLUDE
    #pragma multi_compile_instancing
            
    // #pragma multi_compile_fog
    #pragma fragmentoption ARB_precision_hint_fastest
    #pragma vertex vert
    #pragma fragment frag
    #pragma geometry geom
    #pragma target 4.0
    ENDCG
        

    SubShader{
        
        Tags{ 
            // "DisableBatching" = "True" 
            "Queue" = "Transparent" 
            "RenderType" = "Transparent" 
            "IgnoreProjector" = "True" 
        }
 
        CULL OFF
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
            #include "Precipitation.cginc"            
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
        //     #include "Precipitation.cginc"   
        //     ENDCG
        // }
    }
}