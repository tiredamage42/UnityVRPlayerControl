using System;
using UnityEngine;


namespace AC.LSky
{

	public partial class LSky : MonoBehaviour 
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


		

		public Vector3 outerSpaceOffset = Vector3.zero;
		public bool HDR = false;
		public float sunLightThreshold = 0.20f;
		public UnityEngine.Rendering.AmbientMode ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
		public FogMode unityFogMode = FogMode.ExponentialSquared;





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

	
	}
}
