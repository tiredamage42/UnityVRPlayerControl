
Shader "Custom Environment/Billboard Map" {

	Properties{
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _BumpMap("Bump", 2D) = "bump" {}
        _Color ("Main Color", Color) = (1,1,1,1)
        _HueVariation ("Hue Variation", Color) = (1.0,0.5,0.0,0.1)
        _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
	}
    CGINCLUDE
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


#pragma fragmentoption ARB_precision_hint_fastest
    #pragma vertex vert
    #pragma fragment frag
    #pragma geometry geom
    #pragma target 4.0

    #define USE_NORMAL_CG

    
            #define COLOR_PASS;
            // #define USE_WIND;
            #define FORWARD_LIGHTING
            #define FORWARD_BASE_LIGHTING
            #include "BillboardMap.cginc"
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

#pragma fragmentoption ARB_precision_hint_fastest
    #pragma vertex vert
    #pragma fragment frag
    #pragma geometry geom
    #pragma target 4.0
    
            #define COLOR_PASS;
            // #define USE_WIND;
            #define FORWARD_LIGHTING
            #define FORWARD_ADD_LIGHTING

            #define USE_NORMAL_CG

            #include "BillboardMap.cginc"
            ENDCG
        }

        Pass {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            CGPROGRAM

            #pragma multi_compile_shadowcaster
            
            #pragma fragmentoption ARB_precision_hint_fastest
    #pragma vertex vert
    #pragma fragment frag
    #pragma geometry geom
    #pragma target 4.0
    
            // #define USE_WIND

            #include "BillboardMap.cginc"

            fixed frag(g2f IN) : SV_Target
            {
                return GRASS_SHADOW_CASTER_FRAG (IN, IN.uv_cutoff_distancemod.z * IN.uv_cutoff_distancemod.w);
            }
            ENDCG
        }	
    }
}