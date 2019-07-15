
Shader "Custom Environment/Grass/Grass" {

	Properties{
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _BumpMap("Bump", 2D) = "bump" {}
	}
    CGINCLUDE
    #pragma fragmentoption ARB_precision_hint_fastest
    #pragma vertex vert
    #pragma fragment frag
    #pragma geometry geom
    #pragma target 4.0
    ENDCG

    SubShader{
        Tags{ 
            "RenderType" = "TransparentCutout" 
            "Queue" = "AlphaTest" 
            "IgnoreProjector" = "True" 
        }
        CULL OFF

        Pass {
            Name "FORWARD"
            Tags { "LightMode"="ForwardBase" }

            CGPROGRAM
            #pragma multi_compile_fog
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap

            #define COLOR_PASS;
            #define USE_WIND;
            #define FORWARD_LIGHTING
            #define FORWARD_BASE_LIGHTING
            #include "Grass.cginc"
            ENDCG
        }

        Pass
        {
            // Forward Add pass - this is added once per extra light source
            Name "FORWARD"
            Tags { "LightMode" = "ForwardAdd" }

            ZWrite Off Blend One One

            CGPROGRAM
            #pragma multi_compile_fog
            #pragma multi_compile_fwdadd_fullshadows nolightmap nodirlightmap nodynlightmap

            #define COLOR_PASS;
            #define USE_WIND;
            #define FORWARD_LIGHTING
            #define FORWARD_ADD_LIGHTING

            #include "Grass.cginc"
            ENDCG
        }

        Pass {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            CGPROGRAM
            #pragma multi_compile_shadowcaster
            
            #define USE_WIND

            #include "Grass.cginc"

            fixed frag(g2f IN) : SV_Target
            {
                return GRASS_SHADOW_CASTER_FRAG (IN, IN.uv_cutoff_distancemod.z);
            }
            ENDCG
        }	
    }
}