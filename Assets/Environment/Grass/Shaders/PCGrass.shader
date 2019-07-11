
Shader "Custom Environment/Grass/Grass" {

	Properties{
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _BumpMap("Bump", 2D) = "bump" {}
	}

    SubShader{
        Tags{ 
            "RenderType" = "TransparentCutout" 
            "Queue" = "AlphaTest" 
            "IgnoreProjector" = "True" 
        }
        CULL OFF

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
            #define USE_WIND;
            #define FORWARD_LIGHTING
            #define FORWARD_BASE_LIGHTING
            
            #include "UnityCG.cginc"
            #include "AutoLightGEOM.cginc"
            
            #include "Lighting.cginc"
            #include "PCGrassInclude.cginc"
            
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
            #define USE_WIND;
            #define FORWARD_LIGHTING
            #define FORWARD_ADD_LIGHTING
            
            #include "UnityCG.cginc"
            #include "AutoLightGEOM.cginc"
            #include "Lighting.cginc"
            #include "PCGrassInclude.cginc"
            
            ENDCG
        }

        Pass {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            CGPROGRAM
            #pragma multi_compile_shadowcaster
            // Use shader model 4.0 target, we need geometry shader support
            #pragma target 4.0            
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom
            #pragma fragmentoption ARB_precision_hint_fastest
            #define USE_WIND
            #include "UnityCGGEOM.cginc" 
            #include "PCGrassInclude.cginc"

            fixed frag(g2f IN) : SV_Target
            {
                return GRASS_SHADOW_CASTER_FRAG (IN, IN.uv_cutoff_distancemod.z);
            }
            ENDCG
        }	
    }
}