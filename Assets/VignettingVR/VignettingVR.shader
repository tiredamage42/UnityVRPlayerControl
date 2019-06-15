Shader "Hidden/VignettingVR"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			
			struct appdata
			{
				fixed4 vertex : POSITION;
				fixed2 uv : TEXCOORD0;
			};

			struct v2f
			{
				fixed2 uv : TEXCOORD0;
				fixed4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			fixed4 _MainTex_ST;
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				// o.uv = v.uv;
				o.uv = UnityStereoScreenSpaceUVAdjust(v.uv, _MainTex_ST);
				return o;
			}


			fixed4 _VignetteColor;
			fixed2 _vignetteViewportSpaceOffset[2];

			half4 frag (v2f i) : SV_Target
			{
				fixed2 screenUV = i.uv;// UnityStereoScreenSpaceUVAdjust(i.uv, _MainTex_ST);
#if (UNITY_SINGLE_PASS_STEREO)
				// in single pass, we need to figure out what eye we're rendering to by our selfs
				unity_StereoEyeIndex = screenUV > 0.5;		
#endif
				half2 vignetteCenter = i.uv;

				vignetteCenter += _vignetteViewportSpaceOffset[unity_StereoEyeIndex].xy; // shift to center
				
				half mask = dot(vignetteCenter, vignetteCenter); // compute square length from center
				
				// inverse mask, so we can use it for composition; apply intensity
				mask = (1.0 - mask * _VignetteColor.w); 
				half4 col = tex2D(_MainTex, screenUV);

				return half4(lerp(_VignetteColor.xyz, col.xyz, mask), 1);		
			}
			ENDCG
		}
	}
}
