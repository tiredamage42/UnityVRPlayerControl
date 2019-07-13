

Shader "AC/LSky/Skybox"
{

	Properties
	{

		// _Clouds0 ("_Clouds0", 2D) = "white" 
		// _Clouds1 ("_Clouds1", 2D) = "white"

		// _MageneticField("_MageneticField0", 2D) = "white"

	
	}

	SubShader
	{

		Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
		ZWrite Off

		Pass
		{

			// float4 RotateAroundYInDegrees (float4 vertex, float degrees)
			// {
			// 	float alpha = degrees * UNITY_PI / 180.0;
			// 	float sina, cosa;
			// 	sincos(alpha, sina, cosa);
			// 	float2x2 m = float2x2(cosa, -sina, sina, cosa);
			// 	return float4(mul(m, vertex.xz), vertex.yw).xzyw;
			// }
			// o.vertex = UnityObjectToClipPos(RotateAroundYInDegrees(v.vertex, _Rotation + (_Time * _RotSpeed)));
				

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			//-------------------------------------------------------
			#pragma multi_compile __ LSKY_HDR
			#pragma multi_compile __ LSKY_GAMMA_COLOR_SPACE
			#pragma target 3.0
			
			#include "UnityCG.cginc"


			fixed4 _Clouds0Color;// = fixed4(0, 1, 0, 0);
			fixed4 _Clouds1Color;// = fixed4(1, 0, 0, 0);
			//pre power, post power, steepness
			fixed3 _Clouds0NoiseParams;// = fixed4(1, 1, 1, 2);
			fixed3 _Clouds1NoiseParams;// = fixed4(1, 1, 1, 2);
			fixed4 _Clouds0TileAndShiftSpeed;// = fixed4(.1, .1, .1, .1);
			fixed4 _Clouds1TileAndShiftSpeed;// = fixed4(.1, .1, .1, .1);


			fixed4 _MagFieldColor0;// = fixed4(0,1,0,.5);
			fixed4 _MagFieldColor1;// = fixed4(0,1,1,.5);
			fixed4 _MagneticFieldNoiseTileAndShiftSpeed;// = fixed4(.15, .15, .01, .01);
			fixed3 _MagFieldColorNoiseParams;// = fixed3(2, 1, 2);
			
			
			fixed4 _MagneticFieldTileAndShiftSpeed;// = fixed4(.25,.3,.001, .002);
			fixed3 _MagFieldNoiseParams;// = fixed3(.75, 3, 1);

			fixed3 _MageneticFieldSwimSizeAndHeightFadeSteepness;// = fixed3(.01, .025, 1);
			fixed4 _MagneticFieldSwimFrequencyAndSpeed;// = fixed4(10, 8, .05, .025);
			sampler2D _Clouds0, _Clouds1;
			sampler2D _MageneticField;
			
			
			float _HorizonFade;
			half _NebulaExponent;
			half _GroundFade;
			half _GroundAltitude;


			//-------------------------------------------------------

			struct appdata
			{
				float4 vertex : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID 
			};

			struct v2f
			{
				float3 worldPos             : TEXCOORD0;

				half3  inScatter            : TEXCOORD1;
				half4  outScatter           : TEXCOORD2;

				float2 moonCoords           : TEXCOORD3;
				float3 outerSpaceCoords     : TEXCOORD4;

				fixed4 twinkleFactor 		: TEXCOORD5;
				fixed4 cloudUVs 			: TEXCOORD6;
				fixed3 uv2D_extinction 		: TEXCOORD7;
				fixed4 magneticFieldFactor : TEXCOORD8;
				fixed3 magneticFieldSwimAndFade : TEXCOORD9;

				float4 vertex               : SV_POSITION;

				UNITY_VERTEX_OUTPUT_STEREO 
			};





/////////////////////////
/// Global variables. ///
/////////////////////////

// Celestials directions.
uniform float3 LSky_SunDir;
uniform float3 LSky_MoonDir;
//---------------------------------------

// Defautl atmosphere.
uniform float  LSky_kCameraHeight;
uniform float  LSky_kInnerRadius;
uniform float  LSky_kInnerRadius2;
uniform float  LSky_kOuterRadius;
uniform float  LSky_kOuterRadius2;
uniform float  LSky_kScale;
uniform float  LSky_kScaleOverScaleDepth;
uniform float  LSky_kKmESun;
uniform float  LSky_kKm4PI;
uniform float  LSky_kKrESun;
uniform float  LSky_kKr4PI;
uniform float3 LSky_InvWavelength;
//---------------------------------------

// Atmosphere tint.

uniform half3 LSky_AtmosphereTint;
uniform half3 LSky_DayAtmosphereTint;
uniform half3 LSky_NightAtmosphereTint;
//---------------------------------------

// Sun.
uniform float3 LSky_SunBetaMiePhase;
uniform half3  LSky_SunMieColor;
uniform half   LSky_SunMieScattering;
//---------------------------------------

// Moon.
uniform float3 LSky_MoonBetaMiePhase;
uniform half3  LSky_MoonMieColor;
uniform half   LSky_MoonMieScattering;
//---------------------------------------

// Matrices.
uniform float4x4 LSky_MoonMatrix;
//---------------------------------------

// Reflection.
uniform half3 LSky_GroundColor;
//---------------------------------------

// HDR.
uniform half LSky_Exposure;
//---------------------------------------



#define LSky_PI14 0.07957747  // (1 / (4*PI))
//-------------------------------------------------------------------------------------------

inline half Desaturate(half3 color)
{
 	return (color.r + color.g + color.b) * 0.3333333h;
}
//-------------------------------------------------------------------------------------------


// Color correction
//#define DARKNESS(color) pow(color, LSky_Darkness);
#define FAST_TONEMAPING(color) 1.0 - exp(LSky_Exposure * -color)
#define GAMMA_TO_LINEAR(color) pow(color, 0.45454545) // aproximate.

inline void ColorCorrection(inout half3 color, inout half3 groundColor)
{
	color = clamp(color, 0.01, color);   // Prevent divide by zero.
	color = sqrt(color * color * color); // color ^ 1.5.
	//color *= color; // color ^ 2.

	#ifndef LSKY_HDR
		color = FAST_TONEMAPING(color);
	#else
		color *= LSky_Exposure;
	#endif

	#ifdef LSKY_GAMMA_COLOR_SPACE
		color = GAMMA_TO_LINEAR(color);
	#else
		groundColor *= groundColor;
	#endif
}


//-------------------------------------------------------------------------------------------

// Sun.
uniform half _SunDiscSize;
// uniform half3 _SunDiscColor;
// inline half3 SunDisc(float3 dir) {
// 		half    dist = length(dir);
// 		return 100 * (1.0-step(_SunDiscSize, dist)) * _SunDiscColor;	
// }
//-------------------------------------------------------------------------------------------

// Moon.
uniform sampler2D _MoonTexture;
uniform half      _MoonSize;
uniform half4     _MoonColor;

// Outer space.
uniform samplerCUBE _OuterSpaceCube;
uniform float4x4 _OuterSpaceMatrix;

uniform half4 _StarsColor;
uniform half  _StarsScintillation;
uniform half4 _NebulaColor;


#define TWINKLE_FREQUENCY 150
float _TwinkleSpeed;


#include "AtmosphericScattering.cginc"


//---------------------------------------------------------------------------------------------------------------------------------------------------

// Original Henyey Greenstein phase function with small changes.
/*inline half3 MiePhase(float cosTheta, float3 betaMiePhase, half scattering, half3 color)
{
	return (LSky_PI14 * (betaMiePhase.x * pow(betaMiePhase.y - (betaMiePhase.z * cosTheta), -1.5))) * scattering * color;
}*/

// Simplified Henyey Greenstein phase function for moon.
inline half3 MiePhaseSimplified(float cosTheta, float3 betaMiePhase, half scattering, half3 color)
{
	return (LSky_PI14 * scattering * (betaMiePhase.x / (betaMiePhase.y - (betaMiePhase.z * cosTheta)))) * color;
}
// Cornette Sharks Henyey Greenstein phase function with small changes.
inline half3 MiePhase(float cosTheta, float3 betaMiePhase, half scattering,  half3 color)
{
	return (betaMiePhase.x * scattering * ((1.0 + cosTheta*cosTheta) * pow(betaMiePhase.y - (betaMiePhase.z * cosTheta), -_SunDiscSize))) * color;
}


//---------------------------------------------------------------------------------------------------------------------------------------------------


			v2f vert (appdata v)
			{
				v2f o;

			
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				//------------------------------------------------------------------------------

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = normalize(mul((float3x3)unity_ObjectToWorld, v.vertex.xyz));
				//------------------------------------------------------------------------------

				AtmosphericScattering(o.worldPos, o.inScatter, o.outScatter);//, true);
				//------------------------------------------------------------------------------

				o.moonCoords = (mul((float3x3)LSky_MoonMatrix, v.vertex.xyz) / _MoonSize + 0.5).xy;
				//------------------------------------------------------------------------------


				float3 sunCoords = v.vertex.xyz;
				// to have outer space rotate with sun
				// float3 sunCoords = mul((float3x3)LSky_MoonMatrix, v.vertex.xyz);
			
				o.outerSpaceCoords = mul((float3x3)_OuterSpaceMatrix, sunCoords);
		
				//------------------------------------------------------------------------------

				// o.extinction = saturate((o.worldPos.y-_HorizonFade)*5);
				fixed extinction = saturate((o.worldPos.y-_HorizonFade)*5);
				
				//------------------------------------------------------------------------------


				fixed3 posWorld = o.worldPos;
                posWorld.y = saturate(posWorld.y);

                o.uv2D_extinction = fixed3(posWorld.xz+(posWorld.xz*(1.0 - posWorld.y)), extinction);
                        
				// pow(posWorld.y, 1) //heightFade


				// _TwinkleSpeed = 20;
				
				fixed2 twinkleTime = (o.uv2D_extinction.xy * TWINKLE_FREQUENCY);// + _Time.y * _TwinkleSpeed;
                fixed twinkleSpeed = _Time.y * _TwinkleSpeed;
				o.twinkleFactor = fixed4(
                    twinkleTime + twinkleSpeed,
                    1.0-_StarsScintillation,
					sin(twinkleTime.y + twinkleTime.x + twinkleSpeed * .25) * .5 + .5 // a bit of randomness
                );




				
				
				o.magneticFieldFactor = fixed4(
					(o.uv2D_extinction.xy + (_MagneticFieldNoiseTileAndShiftSpeed.zw * _Time.y)) * _MagneticFieldNoiseTileAndShiftSpeed.xy,
					(o.uv2D_extinction.xy + (_MagneticFieldTileAndShiftSpeed.zw * _Time.y)) * _MagneticFieldTileAndShiftSpeed.xy 
				);

				o.magneticFieldSwimAndFade = fixed3(
					o.uv2D_extinction.xy * _MagneticFieldSwimFrequencyAndSpeed.xy + _MagneticFieldSwimFrequencyAndSpeed.zw * _Time.y,

					pow(posWorld.y, _MageneticFieldSwimSizeAndHeightFadeSteepness.z)
				);



				
				o.cloudUVs = fixed4(
					(o.uv2D_extinction.xy + (_Clouds0TileAndShiftSpeed.zw * _Time.y)) * _Clouds0TileAndShiftSpeed.xy,
					(o.uv2D_extinction.xy + (_Clouds1TileAndShiftSpeed.zw * _Time.y)) * _Clouds1TileAndShiftSpeed.xy
				);
					




				return o;
			}

			// #define UV_2D i.uv2D_extinction.xy
			#define EXTINCTION i.uv2D_extinction.z


			// #define UV_SPEEDS uvOffsets.xy
			// #define UV_TILES uvOffsets.zw
			#define PRE_POWER noiseParams.x
			#define STEEPNESS noiseParams.y
			#define POST_POWER noiseParams.z
			// #define DRAW_POWER cloudParameters.w
			// #define INTENSITY cloudColor.a


			fixed SampleNoise (fixed s, fixed3 noiseParams) {
				return saturate(pow(s * PRE_POWER, STEEPNESS) * POST_POWER);
			}

			

			void AddMieSun (v2f i, inout fixed3 color, inout fixed totalDraw, fixed3 ray) {

				float  sunCosTheta  = dot(ray, LSky_SunDir.xyz);
				
				fixed3 miePhase = saturate(MiePhase(sunCosTheta, LSky_SunBetaMiePhase, LSky_SunMieScattering, LSky_SunMieColor) * i.outScatter.rgb);
				miePhase *= 1.0-totalDraw;
				
				
				color += miePhase;

			}

			void AddMoonHalo (v2f i, inout fixed3 color, inout fixed totalDraw, fixed moonCosTheta) {
				
				
				fixed3 moonMie = saturate(MiePhaseSimplified(moonCosTheta, LSky_MoonBetaMiePhase, LSky_MoonMieScattering,  LSky_MoonMieColor) * i.outScatter.a);
				moonMie *= 1.0-totalDraw;

				color.rgb += moonMie;
				
			}

			
			inline half4 Moon(float2 coords, float cosTheta)
			{

				half4 color  = tex2D(_MoonTexture, coords.xy) * saturate(cosTheta);
				color.rgb *= _MoonColor.rgb * _MoonColor.a;
				color.a *= _MoonColor.a;


				// half mask    = (1.0 - color.a);

				return color;// half4(color.rgb, mask); // RGB = Moon, Alpha = Mask.
			}

			void AddMoonTexture (v2f i, inout fixed3 color, inout fixed totalDraw, fixed moonCosTheta) {
				
				half4 moon  = Moon(i.moonCoords, moonCosTheta) * i.outScatter.a;

				moon *= 1.0-totalDraw;

				totalDraw += moon.a;// 1.0-moon.a;

				totalDraw = saturate(totalDraw);

				
				color += moon.rgb * EXTINCTION;
			}




			inline half3 OuterSpace(fixed4 twinkleFactor, float3 coords, half nebulaExponent)
			{
				half4 cube = texCUBE(_OuterSpaceCube, coords);
				cube.rgb = pow(cube.rgb, nebulaExponent);
				half3 nebula =  cube.rgb * _NebulaColor.rgb  * _NebulaColor.a;


				fixed2 f = cos(twinkleFactor.xy);
				fixed noise = clamp(((f.x + f.y) * .5 + .5) * twinkleFactor.w, twinkleFactor.z, 1.0);				
				half field = cube.a * _StarsColor.a;

				half3 starsField = (field * noise) * _StarsColor.rgb;

				return starsField + nebula;
			}

			void AddNightBox (v2f i, inout fixed3 color, inout fixed totalDraw) {


				half3 outerSpace = OuterSpace(i.twinkleFactor, i.outerSpaceCoords, _NebulaExponent) * i.outScatter.a;

				// outerSpace *= saturate(1.0 - sunDisc.r);
				outerSpace *= 1.0-totalDraw;
				
				// outerSpace *= moon.a;
				color += outerSpace * EXTINCTION;
				
			}
				
				
			#define CLOUD_DRAW_POWER 2
				
			void AddCloudCover(v2f i, inout fixed3 color, inout fixed totalDraw, sampler2D cloudTex, fixed2 uv, fixed3 cloudNoiseParams, fixed4 cloudColor) {

				fixed cloud = tex2D(cloudTex, uv).r;

				cloud = SampleNoise(cloud, cloudNoiseParams);
				
				cloud = cloud * cloudColor.a * EXTINCTION;

				cloud *= 1.0-totalDraw;

				totalDraw += cloud * CLOUD_DRAW_POWER;
				totalDraw = saturate(totalDraw);

                fixed3 cloudPaint = cloud * cloudColor.rgb;
				
				color *= 1.0-cloud;
				color += cloudPaint;
			}

			void AddMagneticField (v2f i, inout fixed3 color, inout fixed totalDraw,
				fixed4 color0, fixed4 color1,
				sampler2D colorNoiseMap, 
				fixed3 colorNoiseParameters, //pre power, steepness, post power

				fixed2 swimSize,
				fixed3 magneticFieldNoiseParameters
				
			) {
				
				
				fixed magSwitch = tex2D(colorNoiseMap, i.magneticFieldFactor.xy).r;
				magSwitch = SampleNoise(magSwitch, colorNoiseParameters);
				fixed4 magColor = lerp(color0, color1, magSwitch);

				fixed magneticField = tex2D(_MageneticField, i.magneticFieldFactor.zw + cos(i.magneticFieldSwimAndFade.xy) * swimSize).r;
				magneticField = SampleNoise(magneticField, magneticFieldNoiseParameters);
				magneticField = magneticField * EXTINCTION * magColor.a * i.magneticFieldSwimAndFade.z; // intensity

				magneticField *= 1.0 - totalDraw;

				color += magneticField * magColor.rgb;
			}

			
			half4 frag (v2f i) : SV_Target
			{
				
				float3 ray = normalize(i.worldPos);
				
				half3 color = saturate(i.inScatter);

				fixed totalDraw = 0;


				


				AddCloudCover(i, color, totalDraw, _Clouds0, i.cloudUVs.xy, _Clouds0NoiseParams, _Clouds0Color);
				AddCloudCover(i, color, totalDraw, _Clouds1, i.cloudUVs.zw, _Clouds1NoiseParams, _Clouds1Color);
				
				AddMagneticField (i, color, totalDraw, _MagFieldColor0, _MagFieldColor1, _Clouds0, _MagFieldColorNoiseParams, _MageneticFieldSwimSizeAndHeightFadeSteepness.xy, _MagFieldNoiseParams);
				
				AddMieSun (i, color, totalDraw, ray);

				float  moonCosTheta = dot(ray, LSky_MoonDir.xyz); 
				AddMoonHalo (i, color, totalDraw, moonCosTheta);
				AddMoonTexture(i, color, totalDraw, moonCosTheta);

				AddNightBox(i, color, totalDraw);


				ColorCorrection(color, LSky_GroundColor);
				color = lerp(color, LSky_GroundColor, saturate((-ray.y + _GroundAltitude) * _GroundFade));
				return half4(color,1);
			}
			ENDCG
		}

	}
}
