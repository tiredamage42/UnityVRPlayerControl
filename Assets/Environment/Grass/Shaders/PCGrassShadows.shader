
Shader "Custom Environment/Grass/ShadowPass" {
	Properties{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
	}
    SubShader{
		CULL OFF
        Pass {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom
            #pragma multi_compile_shadowcaster    

            // Use shader model 4.0 target, we need geometry shader support
            #pragma target 4.0
            #pragma fragmentoption ARB_precision_hint_fastest
            
            // #define NO_HEIGHT_FADE
            #include "UnityCGGEOM.cginc" 
            #include "PCGrassInclude.cginc"
            
            fixed frag(g2f IN) : SV_Target
            {
                return GRASS_SHADOW_CASTER_FRAG (IN, IN.uv_cutoff_distancemod.z * IN.uv_cutoff_distancemod.w);
            }
		    ENDCG
        }	
    }
}