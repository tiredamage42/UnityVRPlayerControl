Shader "Custom/Highlight" {

	Properties {
		_MainTex("Main Texture", 2D) = "black" {}
	}

	CGINCLUDE

	fixed4 _Color;

	float4 simple_vert(float4 v:POSITION) : POSITION{
		return UnityObjectToClipPos(v);
	}

	fixed4 simple_frag() : COLOR{
		return _Color * 2;
	}

	ENDCG

	SubShader {

		ZWrite Off
		ZTest Always 
		Cull Off 		
			
		// OVERLAY GLOW		
		Pass {
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
		
			sampler2D _MainTex;
			sampler2D _SecondaryTex;
			
			// fixed _ControlValue;
		
			fixed4 frag(v2f_img IN) : COLOR 
			{
				fixed4 mCol = tex2D (_MainTex, IN.uv);

				#if !UNITY_UV_STARTS_AT_TOP
				IN.uv.y = 1.0 - IN.uv.y;
				#endif

				fixed3 overCol = tex2D(_SecondaryTex, IN.uv) * 2;
				return mCol + fixed4(overCol, 1.0);
			}
			ENDCG
		}
		
		// OCCLUSION	
		Pass {
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
		
			sampler2D _MainTex;
			sampler2D _SecondaryTex;

			fixed4 frag(v2f_img IN) : COLOR 
			{
				fixed4 occludeCol = tex2D(_SecondaryTex, IN.uv);
				return tex2D (_MainTex, IN.uv) - occludeCol.aaaa;
			}
			ENDCG
		}

		// Draw to render texture, overlayed.
		Pass {
            CGPROGRAM
            #pragma vertex simple_vert
            #pragma fragment simple_frag
            ENDCG
        }	

		// 4 Draw to render texture, depth filtered.
        Pass {        	
			// ZTest LEqual 
			CGPROGRAM
			#pragma vertex simple_vert
			#pragma fragment simple_frag
			ENDCG
        }
	} 
}
