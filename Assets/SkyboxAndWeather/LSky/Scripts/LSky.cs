using System;
using UnityEngine;
using UnityEngine.Rendering;


namespace AC.LSky
{
	[AddComponentMenu("AC/LSky/LSky Manager")]
	[ExecuteInEditMode] public partial class LSky : MonoBehaviour 
	{
		public Material  skyboxMaterial = null;  // Skybox material.
		public Texture2D moonTexture    = null;  // Moon texture.
		public Cubemap   outerSpaceCube = null;  // RGB: Nebula, Alpha: Stars field.
		//------------------------------------------------------------------------------------

		Light m_SunLight = null;        // Sun light component.
		Light m_MoonLight = null;       // Moon light component.
		
		LSkyTOD timeOfDayScript;
		//------------------------------------------------------------------------------------

		void OnEnable () {
			CacheComponents();
		}
		// Cache necessary components.
		void CacheComponents()
		{
			Light[] lights = GetComponentsInChildren<Light>();

			if (lights.Length > 0) {
				m_SunLight = lights[0];
			}
			if (lights.Length > 1) {
				m_MoonLight = lights[1];
			}

			timeOfDayScript = GetComponent<LSkyTOD>();

			m_Probe              = GetComponentInChildren<ReflectionProbe>();
			m_Probe.mode         = ReflectionProbeMode.Realtime;
			m_Probe.refreshMode  = ReflectionProbeRefreshMode.ViaScripting;

		}

		float reflectionProbeTimer; 

		void UpdateProbe(float updateInterval, RenderTexture rt = null)
		{
			if (!Application.isPlaying)
				return;

			float updateRate = 1.0f / updateInterval;
			reflectionProbeTimer         += Time.deltaTime;

			if(reflectionProbeTimer >= updateRate) 
			{
				m_Probe.RenderProbe(rt);
				reflectionProbeTimer = 0;
			}
		}
		ReflectionProbe m_Probe;
		//------------------------------------------------------------------------------------

		/// <summary>
		/// Set sun local rotation.
		/// </summary>
		/// <param name="rot">Rot.</param>
		public void SetSunLightLocalRotation(Quaternion rot)
		{
			m_SunLight.transform.localRotation = rot;
		}

		//------------------------------------------------------------------------------------


		/// <summary>
		/// Set sun local rotation.
		/// </summary>
		/// <param name="rot">Rot.</param>
		public void SetMoonLightLocalRotation(Quaternion rot)
		{
			m_MoonLight.transform.localRotation = rot;
		}



		#region Unity


		void Update()
		{

			float tod = timeOfDayScript.TOD;

			Sun(tod);
			Moon(tod);
			OuterSpace(tod);
			Atmosphere(tod);
			ColorCorrection(tod);
			Lighting(tod);

			SetClouds(tod);
			SetMagneticField(tod);
			
			UpdateProbe(15);

		}


		#endregion

		#region Celestials

		void Sun(float tod)
		{
			Shader.SetGlobalVector("LSky_SunDir", SunDirection);
			skyboxMaterial.SetFloat("_SunDiscSize", sunDiscSize.Evaluate(tod));//.OutputValue);
		}
		
		//-----------------------------------------------------------------------------------------------------------------------------

		void Moon(float tod)
		{

			Shader.SetGlobalVector("LSky_MoonDir", MoonDirection);
			Shader.SetGlobalMatrix("LSky_MoonMatrix", m_MoonLight.transform.worldToLocalMatrix);

			skyboxMaterial.EnableKeyword("LSKY_ENABLE_MOON");
			skyboxMaterial.SetTexture("_MoonTexture", moonTexture);
			skyboxMaterial.SetFloat("_MoonSize", moonSize.Evaluate(tod));//.OutputValue);
			skyboxMaterial.SetColor("_MoonColor", moonColor.Evaluate(tod));//.OutputColor);
		}

		public float spaceParallaxScrollModifier = .5f;

		float spaceAngle;


		void SetMagneticField (float tod) {


			skyboxMaterial.SetTexture("_MageneticField", magneticField.texture);
			
			skyboxMaterial.SetColor("_MagFieldColor0", magneticField.color0.Evaluate(tod));
			skyboxMaterial.SetColor("_MagFieldColor1", magneticField.color1.Evaluate(tod));
			
			skyboxMaterial.SetVector("_MagneticFieldNoiseTileAndShiftSpeed", magneticField.colorNoiseTileAndShiftSpeed.Evaluate(tod) );
			skyboxMaterial.SetVector("_MagFieldColorNoiseParams", magneticField.colorNoiseSampleParams.Evaluate(tod ) );

			skyboxMaterial.SetVector("_MagneticFieldTileAndShiftSpeed", magneticField.tileAndShiftSpeed.Evaluate(tod) );
			skyboxMaterial.SetVector("_MagFieldNoiseParams", magneticField.noiseSampleParams.Evaluate(tod ) );

			skyboxMaterial.SetVector("_MageneticFieldSwimSizeAndHeightFadeSteepness", new Vector3(magneticField.swimSizeX.Evaluate(tod), magneticField.swimSizeY.Evaluate(tod), magneticField.heightFadeSteepness.Evaluate(tod)) );
			skyboxMaterial.SetVector("_MagneticFieldSwimFrequencyAndSpeed", magneticField.swimTileAndSpeed.Evaluate(tod ) );

		}



		

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
			public Texture2D texture;
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


		void SetClouds (float tod) {

			skyboxMaterial.SetTexture("_Clouds0", clouds0.texture);
			skyboxMaterial.SetColor("_Clouds0Color", clouds0.color.Evaluate(tod));
			skyboxMaterial.SetVector("_Clouds0NoiseParams", clouds0.noiseSampleParams.Evaluate(tod) );
			skyboxMaterial.SetVector("_Clouds0TileAndShiftSpeed", clouds0.tileAndShiftSpeed.Evaluate(tod ) );

			skyboxMaterial.SetTexture("_Clouds1", clouds1.texture);
			skyboxMaterial.SetColor("_Clouds1Color", clouds1.color.Evaluate(tod));
			skyboxMaterial.SetVector("_Clouds1NoiseParams", clouds1.noiseSampleParams.Evaluate(tod) );
			skyboxMaterial.SetVector("_Clouds1TileAndShiftSpeed", clouds1.tileAndShiftSpeed.Evaluate(tod ) );
		}


		void OuterSpace(float tod)
		{
			skyboxMaterial.SetTexture("_OuterSpaceCube", outerSpaceCube);
			
			if (Application.isPlaying) {
				spaceAngle += Time.deltaTime * ( 360f / timeOfDayScript.dayInSeconds) * spaceParallaxScrollModifier;
			}
			else {
				spaceAngle = tod * 360 * spaceParallaxScrollModifier;
			}
			spaceAngle = Mathf.Repeat(spaceAngle, 360);

			

			skyboxMaterial.SetMatrix("_OuterSpaceMatrix", Matrix4x4.TRS (Vector3.zero, Quaternion.Euler(outerSpaceOffset) * (Quaternion.Euler(spaceAngle,0,0) * transform.rotation), Vector3.one));//spaceTransform.GetChild(0).worldToLocalMatrix);
			
			


			skyboxMaterial.SetColor("_StarsColor", starsColor.Evaluate(tod));//.OutputColor);
			

			
			skyboxMaterial.SetFloat("_StarsScintillation", starsScintillation.Evaluate(tod));//.OutputValue);
			skyboxMaterial.SetFloat("_TwinkleSpeed", starsScintillationSpeed.Evaluate(tod));//.OutputValue);

			skyboxMaterial.SetColor("_NebulaColor", nebulaColor.Evaluate(tod));//.OutputColor);
			

		}
		//-----------------------------------------------------------------------------------------------------------------------------

		#endregion

		#region Atmosphere

		public Vector3 SunDirection { get { return -m_SunLight.transform.forward;  } }
		public Vector3 MoonDirection{ get { return -m_MoonLight.transform.forward; } }


		void Atmosphere(float tod)
		{
			

			// Atmosphere based in GPUGems2(Sean Oneil).

			// Inverse wave legths (reciprocal).
			Color wl = waveLengths.Evaluate(tod);
			Vector3 InvWavelength = new Vector3 () 
			{

				x = 1.0f / Mathf.Pow(wl.r, 4.0f),
				y = 1.0f / Mathf.Pow(wl.g, 4.0f),
				z = 1.0f / Mathf.Pow(wl.b, 4.0f)
			};
			//-----------------------------------------------------------------------------------------------------------------------------

			float kCameraHeight = 0.0001f;
			float kInnerRadius  = 1.0f;
			float kInnerRadius2 = 1.0f;
			float kOuterRadius  = 1.025f;
			float kOuterRadius2 = kOuterRadius * kOuterRadius;
			//-----------------------------------------------------------------------------------------------------------------------------

			float kScale               = (1.0f / (kOuterRadius - 1.0f));
			float kScaleDepth          = 0.25f;
			float kScaleOverScaleDepth = kScale / kScaleDepth;
			//-----------------------------------------------------------------------------------------------------------------------------

			float kSunBrightness = sunBrightness.Evaluate(tod);
			float kMie           = mie.Evaluate(tod);
			float kKmESun        = kMie * kSunBrightness;
			float kKm4PI         = kMie * 4.0f * Mathf.PI;
			//-----------------------------------------------------------------------------------------------------------------------------

			float kRayleigh      = 0.0025f * atmosphereThickness.Evaluate(tod);
			float kKrESun        = kRayleigh * kSunBrightness;
			float kKr4PI         = kRayleigh * 4.0f * Mathf.PI;
			//-----------------------------------------------------------------------------------------------------------------------------


			Shader.SetGlobalFloat("LSky_kCameraHeight", kCameraHeight);
			Shader.SetGlobalFloat("LSky_kInnerRadius",  kInnerRadius);
			Shader.SetGlobalFloat("LSky_kInnerRadius2", kInnerRadius2);
			Shader.SetGlobalFloat("LSky_kOuterRadius",  kOuterRadius);
			Shader.SetGlobalFloat("LSky_kOuterRadius2", kOuterRadius2);
			
			Shader.SetGlobalFloat("LSky_kScale",               kScale);
			Shader.SetGlobalFloat("LSky_kScaleDepth",          kScaleDepth);
			Shader.SetGlobalFloat("LSky_kScaleOverScaleDepth", kScaleOverScaleDepth);
			
			Shader.SetGlobalFloat("LSky_kKm4PI",  kKm4PI);
			Shader.SetGlobalFloat("LSky_kKmESun", kKmESun);
			Shader.SetGlobalFloat("LSky_kKrESun", kKrESun);
			Shader.SetGlobalFloat("LSky_kKr4PI",  kKr4PI);
			
			Shader.SetGlobalVector("LSky_InvWavelength", InvWavelength);
			
			Shader.SetGlobalColor("LSky_DayAtmosphereTint", dayAtmosphereTint.Evaluate(tod));
			Shader.SetGlobalColor("LSky_NightAtmosphereTint", nightAtmosphereTint.Evaluate(tod));

			Shader.SetGlobalVector("LSky_SunBetaMiePhase", SunBetaMiePhase(sunMieAnisotropy.Evaluate(tod)));
			Shader.SetGlobalFloat("LSky_SunMieScattering", sunMieScattering.Evaluate(tod));
			Shader.SetGlobalColor("LSky_SunMieColor", sunMieColor.Evaluate(tod));//.OutputColor);
			
			Shader.SetGlobalVector("LSky_MoonBetaMiePhase", MoonBetaMiePhase(moonMieAnisotropy.Evaluate(tod)));
			Shader.SetGlobalFloat("LSky_MoonMieScattering", moonMieScattering.Evaluate(tod));// * moonMieMultiplier.OutputValue);
			Shader.SetGlobalColor("LSky_MoonMieColor", moonMieColor.Evaluate(tod));//.OutputColor);

			Shader.SetGlobalFloat("_HorizonFade", horizonFade.Evaluate(tod));
			Shader.SetGlobalFloat("_GroundFade", groundFade.Evaluate(tod));
			
			Shader.SetGlobalFloat("_GroundAltitude", groundAltitude.Evaluate(tod));
			Shader.SetGlobalFloat("_NebulaExponent", nebulaExponent.Evaluate(tod));
		}
		
		//-----------------------------------------------------------------------------------------------------------------------------

		// Henyey Greenstein beta phase(Cornette Sharks).
		public Vector3 SunBetaMiePhase(float g) 
		{

			float g2 = g * g;
			return new Vector3() 
			{
				x = (1.0f - g2) / (2.0f + g2),
				y = (1.0f + g2),
				z = (2.0f * g) 
			};
		}

		// Henyey Greenstein beta mie phase.
		public Vector3 MoonBetaMiePhase(float g) 
		{

			float g2 = g * g;
			return new Vector3() 
			{
				x = (1.0f - g2),
				y = (1.0f + g2),
				z = (2.0f * g) 
			};
		}
		//-----------------------------------------------------------------------------------------------------------------------------

		#endregion

		#region Color Correction

		void ColorCorrection(float tod)
		{

			Shader.SetGlobalFloat("LSky_Exposure", exposure.Evaluate(tod));

			if(!HDR) Shader.DisableKeyword("LSKY_HDR"); 
			else     Shader.EnableKeyword("LSKY_HDR");
			
			if(QualitySettings.activeColorSpace == ColorSpace.Gamma)
				Shader.EnableKeyword("LSKY_GAMMA_COLOR_SPACE");
			else
				Shader.DisableKeyword("LSKY_GAMMA_COLOR_SPACE");
		}

		#endregion

		#region Lighting

		void CheckLight (Light light, float threshold) {
			if (light.intensity <= threshold) {
				if (light.enabled) {
					light.enabled = false;
				}
			}
			else {
				if (!light.enabled) {
					light.enabled = true;
				}
			}
		}

		void Lighting(float tod)
		{

			Color sunColor = sunLightColor_Intensity.Evaluate(tod);
			float intensity = sunLightMaxIntensity * sunColor.a;

			m_SunLight.color     = sunColor;
			m_SunLight.intensity = intensity;

			CheckLight(m_SunLight, sunLightThreshold);
			
			Color moonColor = moonLightColor_Intensity.Evaluate(tod);
			intensity = moonLightMaxIntensity * moonColor.a;

			m_MoonLight.color     = moonColor;
			m_MoonLight.intensity = intensity;
			CheckLight(m_MoonLight, sunLightThreshold);
			
			RenderSettings.skybox = skyboxMaterial;


			RenderSettings.ambientMode = ambientMode;
			switch (ambientMode) 
			{
				case UnityEngine.Rendering.AmbientMode.Skybox:
				RenderSettings.ambientIntensity = ambientIntensity.Evaluate(tod);
				break;

				case UnityEngine.Rendering.AmbientMode.Trilight: 
				RenderSettings.ambientSkyColor     = ambientSkyColor.Evaluate(tod);
				RenderSettings.ambientEquatorColor = ambientEquatorColor.Evaluate(tod);
				break;

				case UnityEngine.Rendering.AmbientMode.Flat :
				RenderSettings.ambientSkyColor     = ambientSkyColor.Evaluate(tod);
				break;
			}

			RenderSettings.ambientGroundColor   = ambientGroundColor.Evaluate(tod);
			Shader.SetGlobalColor("LSky_GroundColor", ambientGroundColor.Evaluate(tod));
			//---------------------------------------------------------------------------------------------------------------------------

			RenderSettings.fog = true;
			RenderSettings.fogMode  = unityFogMode;
			RenderSettings.fogColor = unityFogColor.Evaluate(tod);

			if (unityFogMode == FogMode.Linear)
			{
				RenderSettings.fogStartDistance = unityFogStartDistance.Evaluate(tod);
				RenderSettings.fogEndDistance   = unityFogEndDistance.Evaluate(tod);
			}
			else {
				RenderSettings.fogDensity = unityFogDensity.Evaluate(tod);
			}
		}


		#endregion

		// public bool IsDay{ get{ return m_SunLightEnable; } }
		// public bool IsNight{ get{ return !IsDay; }}
	}
}
