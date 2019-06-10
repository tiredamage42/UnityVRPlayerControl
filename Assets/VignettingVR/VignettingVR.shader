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
			#pragma shader_feature DEBUG_SHOW_VIGNETTE //if you need to see the mask somehow in-build, change this line to "#pragma multi_compile _ DEBUG_SHOW_VIGNETTE". Otherwise, Unity will strip this shader-variant away.

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			sampler2D _MainTex;
			float4 _MainTex_ST;

			half  _vignettingIntensity;
			half2 _vignetteViewportSpaceOffset[2];

			half4 frag (v2f i) : SV_Target
			{
				float2 screenUV = UnityStereoScreenSpaceUVAdjust(i.uv, _MainTex_ST);
#if (UNITY_SINGLE_PASS_STEREO)
				// in single pass, we need to figure out what eye we're rendering to by our selfs
				unity_StereoEyeIndex = screenUV > 0.5;		
#endif
				half2 vignetteCenter = i.uv;

				vignetteCenter += _vignetteViewportSpaceOffset[unity_StereoEyeIndex].xy; // shift to center

				vignetteCenter *= 2.0; // make vignett a bit tighter
				
				half mask = dot(vignetteCenter, vignetteCenter); // compute square length from center
				
				mask = (1.0 - mask * _vignettingIntensity); // inverse mask, so we can use it for composition; apply intensity
				
#ifdef DEBUG_SHOW_VIGNETTE 
				return float4(saturate((mask).xxx > 0), 1) ;
#else
				half4 col = tex2D(_MainTex, screenUV);
				return col * mask; //compose
#endif

			}
			ENDCG
		}
	}
}
