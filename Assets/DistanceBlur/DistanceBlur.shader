Shader "Hidden/DistanceBlur" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
	}
	CGINCLUDE
        #pragma vertex VertexProgram
        #pragma fragment FragmentProgram
        #pragma shader_feature BOKEH_KERNEL_SMALL
        #pragma shader_feature DEBUG_VISUAL
        #pragma fragmentoption ARB_precision_hint_fastest
		#include "UnityCG.cginc"

		sampler2D _MainTex, _CameraDepthTexture, _CoCTex, _DoFTex;
		float4 _MainTex_TexelSize;
        // float _BokehRadius, _FocusDistance, _FocusRange, _COCSteepness;
		float _FocusDistance, _FocusRange, _COCSteepness;

		struct VertexData {
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
		};

		struct Interpolators {
			float4 pos : SV_POSITION;
			float2 uv : TEXCOORD0;
		};

		Interpolators VertexProgram (VertexData v) {
			Interpolators i;
			i.pos = UnityObjectToClipPos(v.vertex);
			i.uv = v.uv;
			return i;
		}

	ENDCG

// 	SubShader {
// 		Cull Off
// 		ZTest Always
// 		ZWrite Off

// 		Pass {// 0 circleOfConfusionPass
// 			CGPROGRAM
// 				half FragmentProgram (Interpolators i) : SV_Target {
// 					half depth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv));
// 					float coc = (depth - _FocusDistance) / (_FocusRange * _BokehRadius);
//                     coc = (saturate(coc)) * _BokehRadius;
// 					return coc;
// 				}
// 			ENDCG
// 		}
//         Pass { // 1 preFilterPass
// 			CGPROGRAM
// 				half4 FragmentProgram (Interpolators i) : SV_Target {
// 					float4 o = _MainTex_TexelSize.xyxy * float2(-0.5, 0.5).xxyy;
					
//                     half coc0 = tex2D(_CoCTex, i.uv + o.xy).r;
// 					half coc1 = tex2D(_CoCTex, i.uv + o.zy).r;
// 					half coc2 = tex2D(_CoCTex, i.uv + o.xw).r;
// 					half coc3 = tex2D(_CoCTex, i.uv + o.zw).r;
					
// 					half coc = max(max(max(coc0, coc1), coc2), coc3);

// 					return half4(tex2D(_MainTex, i.uv).rgb, coc);
// 				}
// 			ENDCG
// 		}
//         Pass { // 1 bokehPass
// 			CGPROGRAM
// 				#if defined(BOKEH_KERNEL_SMALL)
//                 // From https://github.com/Unity-Technologies/PostProcessing/
// 				// blob/v2/PostProcessing/Shaders/Builtins/DiskKernels.hlsl
// 				static const int kernelSampleCount = 16;
// 				static const float2 kernel[kernelSampleCount] = {
// 					float2(0, 0),
// 					float2(0.54545456, 0),
// 					float2(0.16855472, 0.5187581),
// 					float2(-0.44128203, 0.3206101),
// 					float2(-0.44128197, -0.3206102),
// 					float2(0.1685548, -0.5187581),
// 					float2(1, 0),
// 					float2(0.809017, 0.58778524),
// 					float2(0.30901697, 0.95105654),
// 					float2(-0.30901703, 0.9510565),
// 					float2(-0.80901706, 0.5877852),
// 					float2(-1, 0),
// 					float2(-0.80901694, -0.58778536),
// 					float2(-0.30901664, -0.9510566),
// 					float2(0.30901712, -0.9510565),
// 					float2(0.80901694, -0.5877853),
// 				};
//                 #else
//                 // #elif defined (BOKEH_KERNEL_MEDIUM)
//                 static const int kernelSampleCount = 22;
//                 static const float2 kernel[kernelSampleCount] = {
//                     float2(0, 0),
//                     float2(0.53333336, 0),
//                     float2(0.3325279, 0.4169768),
//                     float2(-0.11867785, 0.5199616),
//                     float2(-0.48051673, 0.2314047),
//                     float2(-0.48051673, -0.23140468),
//                     float2(-0.11867763, -0.51996166),
//                     float2(0.33252785, -0.4169769),
//                     float2(1, 0),
//                     float2(0.90096885, 0.43388376),
//                     float2(0.6234898, 0.7818315),
//                     float2(0.22252098, 0.9749279),
//                     float2(-0.22252095, 0.9749279),
//                     float2(-0.62349, 0.7818314),
//                     float2(-0.90096885, 0.43388382),
//                     float2(-1, 0),
//                     float2(-0.90096885, -0.43388376),
//                     float2(-0.6234896, -0.7818316),
//                     float2(-0.22252055, -0.974928),
//                     float2(0.2225215, -0.9749278),
//                     float2(0.6234897, -0.7818316),
//                     float2(0.90096885, -0.43388376),
//                 };
// 				#endif

//                 half Weigh (half coc, half radius) {
//                     return saturate((coc - radius + 2) / 2);
// 				}


// 				half4 FragmentProgram (Interpolators i) : SV_Target {
// 					half coc = tex2D(_MainTex, i.uv).a;
                    
//                     half3 bgColor = 0;
// 					half bgWeight = 0;

//                     for (int k = 0; k < kernelSampleCount; k++) {
// 						float2 o = kernel[k] * _BokehRadius;
                        
//                         half radius = length(o);
// 						o *= _MainTex_TexelSize.xy;

//                         half4 s = tex2D(_MainTex, i.uv + o);
//                         half bgw = Weigh(min(s.a, coc), radius);
                        						
//                         bgColor += s.rgb * bgw;
// 						bgWeight += bgw;
// 					}

//                     bgColor *= 1 / (bgWeight);// + (bgWeight == 0));

//                     return half4(bgColor, 1);
// 				}
// 			ENDCG
// 		}

//         Pass { // 2 postFilterPass (blur bokeh)
// 			CGPROGRAM
// 				half4 FragmentProgram (Interpolators i) : SV_Target {
//                     float4 o = _MainTex_TexelSize.xyxy * float2(-0.5, 0.5).xxyy;
// 					half4 s =
// 						tex2D(_MainTex, i.uv + o.xy) +
// 						tex2D(_MainTex, i.uv + o.zy) +
// 						tex2D(_MainTex, i.uv + o.xw) +
// 						tex2D(_MainTex, i.uv + o.zw);
// 					return s * 0.25;
// 				}
// 			ENDCG
// 		}
//         Pass { // 4 combinePass
// 			CGPROGRAM
// 				half4 FragmentProgram (Interpolators i) : SV_Target {
					
// 					// return fixed4(Linear01Depth((tex2D(_CameraDepthTexture, i.uv))).xxx, 1);
				
// 					half coc = tex2D(_CoCTex, i.uv).r;
//                     half dofStrength = pow(smoothstep(0, 1, coc), _COCSteepness);

// #if defined(DEBUG_VISUAL)
//                     if (coc == 0) return fixed4(0,0,1,1);
//                     if (coc >= 1) return fixed4(1,0,0,1);
//                     return dofStrength;
// #else

//                     half4 source = tex2D(_MainTex, i.uv);
// 					half4 dof = tex2D(_DoFTex, i.uv);
// 					half3 color = lerp(source.rgb, dof.rgb, dofStrength);
//     				return half4( color, source.a );
// #endif
// 				}
// 			ENDCG
// 		}
// 	}




	SubShader {
		Cull Off
		ZTest Always
		ZWrite Off

		Pass {// 0 circleOfConfusionPass
			CGPROGRAM
				half FragmentProgram (Interpolators i) : SV_Target {
					return (LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv)) - _FocusDistance) / (_FocusRange);
				}
			ENDCG
		}
        Pass { // 4 combinePass
			CGPROGRAM
				half4 FragmentProgram (Interpolators i) : SV_Target {
					
					half coc = tex2D(_CoCTex, i.uv).r;
					
					half dofStrength = pow(smoothstep(0, 1, coc), _COCSteepness);

#if defined(DEBUG_VISUAL)
                    if (coc <= 0) return fixed4(0,0,1,1);
                    if (coc >= 1) return fixed4(1,0,0,1);
                    return dofStrength;
#else

                    half4 source = tex2D(_MainTex, i.uv);
					half4 dof = tex2D(_DoFTex, i.uv);
					
					half3 color = lerp(source.rgb, dof.rgb, dofStrength);
    				return half4( color, source.a );
#endif
				}
			ENDCG
		}
	}
}