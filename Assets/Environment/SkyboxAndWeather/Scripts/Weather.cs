using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu()]
public class Weather : ScriptableObject
{
    /*


		.1 range = 2:24 hours in game transition

		0

		.2 = night full (4:48am)
		.25 = peak sunrises (6am)
		.3 = day full (7:12am)

		.5 = midday

		//for consistent visuals...
		.7 = day full (4:48pm)
		.75 = peak sunset(6pm)
		.8 = night full (7:12pm)

		//for realistic tod
		.8 = day full (7:12pm)
		.85 = peak sunset(8:24pm)
		.9 = night full (9:36pm)

		1
		
		*/

		static Gradient GetTODGradient (Color fullNight, Color peakSunrise, Color peakDay, Color peakMidDay, Color peakSunset, bool realisticTOD) {

			float endDayOffset = realisticTOD ? .1f : 0f;
			return new Gradient()
			{
				colorKeys = new GradientColorKey[]
				{
					new GradientColorKey(fullNight, 0.2f),
					new GradientColorKey(peakSunrise, .25f),
					new GradientColorKey(peakDay, .3f),
					new GradientColorKey(peakMidDay, .5f),
					new GradientColorKey(peakDay, .7f + endDayOffset),
					new GradientColorKey(peakSunset, .75f + endDayOffset),
					new GradientColorKey(fullNight, .8f + endDayOffset),	
				},

				alphaKeys = new GradientAlphaKey[] 
				{
					new GradientAlphaKey(fullNight.a, 0.2f),
					new GradientAlphaKey(peakSunrise.a, .25f),
					new GradientAlphaKey(peakDay.a, .3f),
					new GradientAlphaKey(peakMidDay.a, .5f),
					new GradientAlphaKey(peakDay.a, .7f + endDayOffset),
					new GradientAlphaKey(peakSunset.a, .75f + endDayOffset),
					new GradientAlphaKey(fullNight.a, .8f + endDayOffset),
				}
			};
		}
		static Gradient GetTODGradient (Color color, bool realisticTOD) {
			return GetTODGradient(color, color, color, color, color, realisticTOD);
		}
		
		
		static AnimationCurve GetTODCurve(float nightValue, float sunriseValue, float dayValue, float midDayValue, float sunsetValue, bool realisticTOD) {
			float endDayOffset = realisticTOD ? .1f : 0f;
			
			return new AnimationCurve()
			{
				keys = new Keyframe[]
				{
					new Keyframe(0.0f, nightValue),
					
					new Keyframe(0.2f, nightValue),
					new Keyframe(.25f, sunriseValue),
					new Keyframe(0.3f, dayValue),
					new Keyframe(0.5f, midDayValue),
					new Keyframe(0.7f + endDayOffset, dayValue),
					new Keyframe(.75f + endDayOffset, sunsetValue),
					new Keyframe(0.8f + endDayOffset, nightValue),	
					
					new Keyframe(1.0f, nightValue),
					
				}
			};
		}
		static AnimationCurve GetTODCurve(float value, bool realisticTOD) {
			return GetTODCurve(value, value, value, value, value, realisticTOD);
		}


    


		public Gradient waveLengths = GetTODGradient(
			new Color(.65f, .57f, .475f, 1),
			new Color(.65f, .57f, .475f, 1),
			new Color(.65f, .57f, .475f, 1),
			new Color(.65f, .57f, .475f, 1),
			new Color(.65f, .57f, .475f, 1),
			false
		);




		public AnimationCurve atmosphereThickness = GetTODCurve(1, false);	
		public AnimationCurve sunBrightness = GetTODCurve(30f, false);
		public AnimationCurve mie = GetTODCurve(.01f, false);
		
		



		public AnimationCurve starsScintillation = GetTODCurve(.9f, false);
		public AnimationCurve starsScintillationSpeed = GetTODCurve(1f, false);
		public AnimationCurve exposure = GetTODCurve(1.3f, false);
		public AnimationCurve ambientIntensity = GetTODCurve(1, false);
		public AnimationCurve unityFogDensity = GetTODCurve(.01f, false);
		public AnimationCurve unityFogStartDistance = GetTODCurve(0f, false);
		public AnimationCurve unityFogEndDistance = GetTODCurve(300f, false);
		public AnimationCurve horizonFade = GetTODCurve(.006f, false); // -.5, .5
		public AnimationCurve groundFade = GetTODCurve(30, false); //0, 60
		public AnimationCurve groundAltitude = GetTODCurve(0, false); // -1, 1
		public AnimationCurve nebulaExponent = GetTODCurve(3, false); //.01,10
			


		
		
		#region Atmosphere

	
		public Gradient dayAtmosphereTint = GetTODGradient(
			new Color(1.0f, 1.0f, 1.0f, 1.0f),
			new Color(1.0f, 1.0f, 1.0f, 1.0f),
			new Color(1.0f, 1.0f, 1.0f, 1.0f),
			new Color(1.0f, 1.0f, 1.0f, 1.0f),
			new Color(1.0f, 1.0f, 1.0f, 1.0f),
			false
		);
		public Gradient nightAtmosphereTint = GetTODGradient(
			new Color(0.03f, 0.05f, 0.09f, 1.0f),
			new Color(0.03f, 0.05f, 0.09f, 1.0f),
			new Color(0.03f, 0.05f, 0.09f, 1.0f),
			new Color(0.03f, 0.05f, 0.09f, 1.0f),
			new Color(0.03f, 0.05f, 0.09f, 1.0f),
			false
		);

		public Gradient sunMieColor = GetTODGradient(
			new Color(1.0f, 0.95f, 0.83f, 1.0f),
			new Color(1.0f, 0.95f, 0.83f, 1.0f),
			new Color(1.0f, 0.95f, 0.83f, 1.0f),
			new Color(1.0f, 0.95f, 0.83f, 1.0f),
			new Color(1.0f, 0.95f, 0.83f, 1.0f),
			false
		);

	
		
		public AnimationCurve sunMieAnisotropy = GetTODCurve(.75f, false);
		public AnimationCurve sunMieScattering = GetTODCurve(.5f, false);
		public AnimationCurve sunDiscSize = GetTODCurve(5f, false);
		
		
		public Gradient moonMieColor = GetTODGradient(
			new Color(0.507f, 0.695f,  1.0f, 1.0f),
			new Color(0.507f, 0.695f,  1.0f, 1.0f),
			new Color(0.507f, 0.695f,  1.0f, 1.0f),
			new Color(0.507f, 0.695f,  1.0f, 1.0f),
			new Color(0.507f, 0.695f,  1.0f, 1.0f),
			false
		);

		public AnimationCurve moonMieAnisotropy = GetTODCurve(.93f, false);
		public AnimationCurve moonMieScattering = GetTODCurve(.5f, false);
		public AnimationCurve moonSize = GetTODCurve(.3f, false);

	
		#endregion

		#region Celestials		
		// public LSkyColor sunDiscColor = new LSkyColor() new Color(1.0f, 1.0f, 1.0f, 1.0f)
		
		//----------------------------------------------------------------------------------------




		public Gradient moonColor = GetTODGradient(
			new Color(1.0f, 1.0f, 1.0f, 1.0f),
			new Color(1.0f, 1.0f, 1.0f, 0.5f),
			new Color(1.0f, 1.0f, 1.0f, 0.0f),
			new Color(1.0f, 1.0f, 1.0f, 0.0f),
			new Color(1.0f, 1.0f, 1.0f, 0.5f),
			false
		);

	
		
		// maybe add max intensity of 10
		public Gradient starsColor = GetTODGradient(
			new Color(1.0f, 1.0f, 1.0f, 1.0f),
			new Color(1.0f, 1.0f, 1.0f, 0.5f),
			new Color(1.0f, 1.0f, 1.0f, 0.0f),
			new Color(1.0f, 1.0f, 1.0f, 0.0f),
			new Color(1.0f, 1.0f, 1.0f, 0.5f),
			false
		);
		


		

		// maybe add max intensity of 10
		public Gradient nebulaColor = GetTODGradient(
			new Color(1.0f, 1.0f, 1.0f, 1.0f),
			new Color(1.0f, 1.0f, 1.0f, 0.5f),
			new Color(1.0f, 1.0f, 1.0f, 0.0f),
			new Color(1.0f, 1.0f, 1.0f, 0.0f),
			new Color(1.0f, 1.0f, 1.0f, 0.5f),
			false
		);

		[Range(0,10)] public float sunLightMaxIntensity = 1;

		public Gradient sunLightColor_Intensity = GetTODGradient(
			new Color(0.0f, 0.0f, 0.0f, 0.0f), //full night

			new Color(1.0f, 0.523f, 0.264f, 0.5f), //sunrise

			new Color(1.0f, 0.956f, 0.839f, 1.0f), //peak day
			new Color(1.0f, 0.956f, 0.839f, 1.0f), //mid day

			new Color(1.0f, 0.523f, 0.264f, 0.5f), //sunset
			false
		);

		[Range(0,10)] public float moonLightMaxIntensity = .3f;

		public Gradient moonLightColor_Intensity = GetTODGradient(
			new Color(0.632f, 0.794f, 1.0f, 1.0f), //full night

			new Color(0.632f, 0.794f, 1.0f, 0.5f), //sunrise

			new Color(0.632f, 0.794f, 1.0f, 0f), //peak day
			new Color(0.632f, 0.794f, 1.0f, 0f), //mid day

			new Color(0.632f, 0.794f, 1.0f, 0.5f), //sunset
			false
		);













		
		#endregion

		public Gradient ambientSkyColor = GetTODGradient(
			new Color(0.047f, 0.094f, 0.180f, 1.0f),
			new Color(0.231f, 0.290f, 0.352f, 1.0f), 
			new Color(0.443f, 0.552f, 0.737f, 1.0f),
			new Color(0.443f, 0.552f, 0.737f, 1.0f),
			new Color(0.231f, 0.290f, 0.352f, 1.0f), 	
			false
		);
		public Gradient ambientEquatorColor = GetTODGradient(
			new Color(0.121f, 0.239f, 0.337f, 1.0f),
			new Color(0.650f, 0.607f, 0.349f, 1.0f),
			new Color(0.901f, 0.956f, 0.968f, 1.0f),
			new Color(0.901f, 0.956f, 0.968f, 1.0f),
			new Color(0.650f, 0.607f, 0.349f, 1.0f),
			false
		);
		public Gradient ambientGroundColor = GetTODGradient(
			new Color(0.0f, 0.0f, 0.0f, 1.0f),
			new Color(0.227f, 0.156f, 0.101f, 1.0f),
			new Color(0.355f, 0.305f, 0.269f, 1.0f),
			new Color(0.466f, 0.435f, 0.415f, 1.0f),
			new Color(0.227f, 0.156f, 0.101f, 1.0f),
			false
		);	
		public Gradient unityFogColor = GetTODGradient(
			new Color(0.121f, 0.239f, 0.337f, 1.0f),
			new Color(0.650f, 0.607f, 0.349f, 1.0f),
			new Color(0.901f, 0.956f, 0.968f, 1.0f),
			new Color(0.901f, 0.956f, 0.968f, 1.0f),
			new Color(0.650f, 0.607f, 0.349f, 1.0f),
			false
		);




        
		[System.Serializable] public class EnvironmentParticleValues {
			public AnimationCurve amount = GetTODCurve(0, false);
			public Gradient color = GetTODGradient(Color.white, false);
			public Gradient hueVariation = GetTODGradient(Color.white, false);
			public AnimationCurve moveSpeed = GetTODCurve(1, false);
			public Vector2Curve quadSize = new Vector2Curve(1,1, false);
			public Vector2Curve cameraRange = new Vector2Curve(1,1, false);
			public Vector2Curve flutterFrequency = new Vector2Curve(1,1, false);
			public Vector2Curve flutterSpeed;
			public Vector2Curve flutterMagnitude = new Vector2Curve(1,1, false);
			public Vector2Curve sizeRange = new Vector2Curve(1,1, false);

			public EnvironmentParticleValues (Color color, Color hueVariation, float moveSpeed, Vector2 quadSize, Vector2 cameraRange, Vector2 flutterFrequency, Vector2 flutterSpeed, Vector2 flutterMagnitude, Vector2 sizeRange, bool realisticTOD) {
				this.amount = GetTODCurve(0, realisticTOD);
				this.color = GetTODGradient(color, realisticTOD);
				this.hueVariation = GetTODGradient(hueVariation, realisticTOD);
				this.moveSpeed = GetTODCurve(moveSpeed, realisticTOD);
				this.quadSize = new Vector2Curve(quadSize, realisticTOD);
				this.cameraRange = new Vector2Curve(cameraRange, realisticTOD);
				this.flutterFrequency = new Vector2Curve(flutterFrequency, realisticTOD);
				this.flutterSpeed = new Vector2Curve(flutterSpeed, realisticTOD);
				this.flutterMagnitude = new Vector2Curve(flutterMagnitude, realisticTOD);
				this.sizeRange = new Vector2Curve(sizeRange, realisticTOD);
			}
		}

		[System.Serializable] public class Vector2Curve {
			public AnimationCurve x, y;

			public Vector2Curve (float x, float y, bool realisticTOD) {
				this.x = GetTODCurve(x, realisticTOD);
				this.y = GetTODCurve(y, realisticTOD);
			}
			public Vector2Curve (Vector2 xy, bool realisticTOD) {
				this.x = GetTODCurve(xy.x, realisticTOD);
				this.y = GetTODCurve(xy.y, realisticTOD);
			}
			public Vector2 Evaluate (float tod) {
				return new Vector2(x.Evaluate(tod), y.Evaluate(tod));
			}
		}
		




	

    
		[System.Serializable] public class GroundFogValues {
			public EnvironmentParticleValues baseValues;
			public AnimationCurve rotateSpeed = GetTODCurve(.1f, false);
			public AnimationCurve softParticleFactor = GetTODCurve(1f, false);
			public AnimationCurve startEndFadeRange = GetTODCurve(2f, false);
			public Vector2Curve closeCamRange = new Vector2Curve(new Vector2(1, 4), false);
			public Vector2Curve heightFadeRange = new Vector2Curve(new Vector2(0, 2), false);
			public AnimationCurve heightFadeSteepness = GetTODCurve(2f, false);

			public GroundFogValues (bool realisticTOD) {
				baseValues = new EnvironmentParticleValues(
					Color.white, Color.white, .2f, 
					new Vector2(1,1), new Vector2(0,10),
					new Vector2(.1f, .1f), new Vector2(.1f, .1f), new Vector2(.25f, .35f), 
					new Vector2(.5f, 1.5f),
					realisticTOD
				);
				rotateSpeed = GetTODCurve(.1f, realisticTOD);
				softParticleFactor = GetTODCurve(1f, realisticTOD);
				startEndFadeRange = GetTODCurve(2f, realisticTOD);
				closeCamRange = new Vector2Curve(new Vector2(1, 4), realisticTOD);
				heightFadeRange = new Vector2Curve(new Vector2(0, 2), realisticTOD);
				heightFadeSteepness = GetTODCurve(2f, realisticTOD);

			}
		}
		[System.Serializable] public class PrecipitatorValues {
			public EnvironmentParticleValues baseValues;
			public AnimationCurve maxTravelDistance;

			public PrecipitatorValues (Color color, Color hueVariation, float moveSpeed, Vector2 quadSize, Vector2 cameraRange, Vector2 flutterFrequency, Vector2 flutterSpeed, Vector2 flutterMagnitude, Vector2 sizeRange, float maxTravelDistance, bool realisticTOD) {
				this.maxTravelDistance = GetTODCurve(maxTravelDistance, realisticTOD);
				this.baseValues = new EnvironmentParticleValues(
					color, hueVariation, moveSpeed, quadSize, cameraRange, flutterFrequency, flutterMagnitude, flutterSpeed, sizeRange, realisticTOD
				);
			}
		}
		public GroundFogValues groundFog = new GroundFogValues(false);

		public PrecipitatorValues rain = new PrecipitatorValues(
			Color.white, Color.white, -5,  // color, hue var, move speed
			
			new Vector2(.0025f, .25f), //quad size

			new Vector2(0,15), //camera range
			
			new Vector2(0.988f, 1.234f), //flutter frequency
			new Vector2(.01f, .01f), //flutter speed
			new Vector2(.35f, .25f), //flutter magnitude
			new Vector2(.5f, 1f), //size range 
			15, //travel distance
			
			false);

		public PrecipitatorValues snow = new PrecipitatorValues(
			Color.white, Color.white, -.25f,  // color, hue var, move speed
			
			new Vector2(.5f, .5f), //quad size

			new Vector2(0,10), //camera range
			
			new Vector2(0.988f, 1.234f), //flutter frequency
			new Vector2(1f, .5f), //flutter speed
			new Vector2(.35f, .25f), //flutter magnitude
			new Vector2(.05f, .025f), //size range 
			10, //travel distance
			
			false);




		[System.Serializable] public class MagneticFieldValues {
			public Texture2D texture;
			public Gradient color0 = GetTODGradient(new Color(0, 1, 0, .5f), false);
			public Gradient color1 = GetTODGradient(new Color(0, 1, 1, .5f), false);
			public TileAndShiftSpeedParams colorNoiseTileAndShiftSpeed = new TileAndShiftSpeedParams(.15f, .15f, .01f, .01f, false);
			public NoiseSampleParams colorNoiseSampleParams = new NoiseSampleParams(2, 1, 2, false);
			public TileAndShiftSpeedParams tileAndShiftSpeed = new TileAndShiftSpeedParams(.25f, .3f, .001f, .002f, false);
			public NoiseSampleParams noiseSampleParams = new NoiseSampleParams(.75f, 3, 1, false);

			public AnimationCurve swimSizeX = GetTODCurve(.01f, false);
			public AnimationCurve swimSizeY = GetTODCurve(.025f, false);
			public AnimationCurve heightFadeSteepness = GetTODCurve(1, false);
			public TileAndShiftSpeedParams swimTileAndSpeed = new TileAndShiftSpeedParams(10, 8, .05f, .025f, false);

			public MagneticFieldValues () {

				this.color0 = GetTODGradient(new Color(0, 1, 0, .5f), false);
				this.color1 = GetTODGradient(new Color(0, 1, 1, .5f), false);
				this.colorNoiseTileAndShiftSpeed = new TileAndShiftSpeedParams(.15f, .15f, .01f, .01f, false);
				this.colorNoiseSampleParams = new NoiseSampleParams(2, 1, 2, false);
				this.tileAndShiftSpeed = new TileAndShiftSpeedParams(.25f, .3f, .001f, .002f, false);
				this.noiseSampleParams = new NoiseSampleParams(.75f, 3, 1, false);
				this.swimSizeX = GetTODCurve(.01f, false);
				this.swimSizeY = GetTODCurve(.025f, false);
				this.heightFadeSteepness = GetTODCurve(1, false);
				this.swimTileAndSpeed = new TileAndShiftSpeedParams(10, 8, .05f, .025f, false);

				

			}

		}
		public MagneticFieldValues magneticField = new MagneticFieldValues();



		[System.Serializable] public class NoiseSampleParams {
			public AnimationCurve prePower = GetTODCurve(1, false);
			public AnimationCurve steepness = GetTODCurve(1, false);
			public AnimationCurve postPower = GetTODCurve(1, false);


			public Vector3 Evaluate(float tod) {
				return new Vector3 (
					prePower.Evaluate(tod),
					steepness.Evaluate(tod),
					postPower.Evaluate(tod)
				);
			}
			
			public NoiseSampleParams (float prePower, float steepness, float postPower, bool realisticTOD) {
				this.prePower = GetTODCurve(prePower, realisticTOD);
				this.steepness = GetTODCurve(steepness, realisticTOD);
				this.postPower = GetTODCurve(postPower, realisticTOD);
			}
		}

		[System.Serializable] public class TileAndShiftSpeedParams {
			public AnimationCurve tileX = GetTODCurve(1, false);
			public AnimationCurve tileY = GetTODCurve(1, false);
			public AnimationCurve shiftSpeedX = GetTODCurve(1, false);
			public AnimationCurve shiftSpeedY = GetTODCurve(1, false);

			public Vector4 Evaluate(float tod) {
				return new Vector4 (
					tileX.Evaluate(tod),
					tileY.Evaluate(tod),
					shiftSpeedX.Evaluate(tod),
					shiftSpeedY.Evaluate(tod)
				);
			}

			public TileAndShiftSpeedParams (float tileX, float tileY, float shiftSpeedX, float shiftSpeedY, bool realisticTOD) {
				this.tileX = GetTODCurve(tileX, realisticTOD);
				this.tileY = GetTODCurve(tileY, realisticTOD);
				this.shiftSpeedX = GetTODCurve(shiftSpeedX, realisticTOD);
				this.shiftSpeedY = GetTODCurve(shiftSpeedY, realisticTOD);
			}
			
		}


		[System.Serializable] public class CloudInfo {
			public Gradient color = GetTODGradient(Color.white, false);
			public NoiseSampleParams noiseSampleParams = new NoiseSampleParams(1, 1, 1, false);
			public TileAndShiftSpeedParams tileAndShiftSpeed = new TileAndShiftSpeedParams(.1f, .1f, .1f, .1f, false);
		
			
			public CloudInfo (Color color, float noisePrePower, float noiseSteepness, float noisePostPower, float tileX, float tileY, float shiftSpeedX, float shiftSpeedY, bool realisticTOD) {
				
				this.color = GetTODGradient(color, realisticTOD);
				noiseSampleParams = new NoiseSampleParams(noisePrePower, noiseSteepness, noisePostPower, realisticTOD);
				tileAndShiftSpeed = new TileAndShiftSpeedParams(tileX, tileY, shiftSpeedX, shiftSpeedY, realisticTOD);
			}
		
		}

		public CloudInfo clouds0 = new CloudInfo(Color.white, 1, 1, 1, .1f, .1f, .1f, .1f, false);
		public CloudInfo clouds1 = new CloudInfo(Color.white, 1, 1, 1, .1f, .1f, .1f, .1f, false);


		public AnimationCurve windStrength = GetTODCurve(0, false);

		
			
}
