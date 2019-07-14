
Shader "Custom Environment/Snow" {

	Properties{
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _FallNoise("Fall Noise", 2D) = "white" {}
        _FieldWidth ("Field Width", float) = 10

        _RainColor ("Rain Color", Color) = (1,1,1,.5)
        _HueVariation ("Hue Variation", Color) = (1,.5,0, .1)

        _RainSpeed ("Rain Speed", float) = 3

        _RainDimensions("Rain Size", vector) = (.5, 2, 0, 0)
        _MaxRainTravelDistance ("Max Travel Distance", float) = 2
        _RainAmount ("Rain Amount", Range(0,1)) = 1
        
	}

    SubShader{
        // Tags{ 
            // "RenderType" = "Transparent" 
            // // "Queue" = "AlphaTest" 
            // "Queue" = "Transparent" 
            // "IgnoreProjector" = "True" 
        // }
        CULL OFF

        Tags{ 
            
             "DisableBatching" = "True" 
            "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
 
        Blend SrcAlpha OneMinusSrcAlpha
        
        ZWrite Off
 

        

        Pass {
            Name "FORWARD"
		
            // indicate that our pass is the "base" pass in forward
            // rendering pipeline. It gets ambient and main directional
            // light data set up; light direction in _WorldSpaceLightPos0
            // and color in _LightColor0
            Tags { "LightMode"="ForwardBase" }

            CGPROGRAM
            #pragma fragmentoption ARB_precision_hint_fastest

            #pragma multi_compile_fog
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap

            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom

            #pragma target 4.0

            #define COLOR_PASS;
            
            #define FORWARD_LIGHTING
            #define FORWARD_BASE_LIGHTING
            

            
            // #pragma glsl
            #include "UnityCG.cginc"
            #include "AutoLightGEOM.cginc"

            
            
            #include "Lighting.cginc"
            #include "SnowShaderInclude.cginc"
            
            ENDCG
        }

        Pass
        {
            // Forward Add pass - this is added once per extra light source
            Name "FORWARD"
            Tags { "LightMode" = "ForwardAdd" }

            ZWrite Off Blend One One

            CGPROGRAM

            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_fog
            #pragma multi_compile_fwdadd_fullshadows nolightmap nodirlightmap nodynlightmap

            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom

            #pragma target 4.0

            #define COLOR_PASS;
            
            #define FORWARD_LIGHTING
            #define FORWARD_ADD_LIGHTING

            // #pragma glsl
            
            #include "UnityCG.cginc"
            #include "AutoLightGEOM.cginc"
            #include "Lighting.cginc"
            #include "SnowShaderInclude.cginc"
            
            ENDCG
        }

    }
}