
Shader "Custom/PCGrass_Shadows" {
	Properties{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Cutoff("Cutoff", Range(0,1)) = 0.25
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
            
            fixed _Cutoff;

            fixed frag(g2f IN) : SV_Target
            {
                return GRASS_SHADOW_CASTER_FRAG (IN, _Cutoff * IN.uv_cutoff_distancemod.w);
            }
		    ENDCG
        }	
    }
}