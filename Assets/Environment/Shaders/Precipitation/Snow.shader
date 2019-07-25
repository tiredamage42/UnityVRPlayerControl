
Shader "Custom Environment/Snow" {

	Properties{
        _ParticlesAmount ("Precipitation Amount", Range(0,1)) = 1
        
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _NoiseMap("Fall Noise", 2D) = "white" {}
        
        _Color ("Color", Color) = (1,1,1,.5)
        _HueVariation ("Hue Variation", Color) = (1,.5,0, 1)

        _MoveSpeed ("Fall Speed", float) = -.25
        _QuadDimensions("Quad Size", vector) = (.5, .5, 0, 0)
        _SizeRange("Size Range", vector) = (.05,.025,0,0)
        
        _MaxTravelDistance ("Max Travel Distance", float) = 10

        _CameraRange ("Camera Range", vector) = (0, 10, 0,0)

        _FlutterFrequency ("Flutter Frequency", vector) = (0.988, 1.234, 0,0)
        _FlutterSpeed ("Flutter Speed", vector) = (1.0, .5, 0,0)
        _FlutterMagnitude ("Flutter Magnitude", vector) = (.35, .25, 0,0)
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("BlendDestination", int) = 0
	}

    
    SubShader{
        
        Tags{ 
            // "DisableBatching" = "True" 
            "Queue" = "Transparent" 
            "RenderType" = "Transparent" 
            "IgnoreProjector" = "True" 
        }
 
        CULL OFF
        Blend SrcAlpha [_DstBlend]
        ZWrite Off
        Pass {
            CGPROGRAM
            #pragma multi_compile_instancing
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom
            #pragma target 4.0        
            #define SPIN_QUAD
            #define BOTTOM_QUAD
            #include "Precipitation.cginc"            
            ENDCG
        }
    }
}