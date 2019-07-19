using System;
using UnityEngine;
using UnityEngine.Rendering;


namespace AC.LSky
{
	[AddComponentMenu("AC/LSky/LSky Manager")]
	[ExecuteInEditMode] public partial class LSky : MonoBehaviour 
	{

		[Range(0,1)] public float weatherSwitch;
		[HideInInspector] public Weather targetWeather;
		[HideInInspector] public Weather oldWeather;
		

		[Space]
		public Material  skyboxMaterial = null;  // Skybox material.
		public Texture2D moonTexture    = null;  // Moon texture.


		[Header("Outer Space")]
		public Cubemap   outerSpaceCube = null;  // RGB: Nebula, Alpha: Stars field.
		public float spaceParallaxScrollModifier = .5f;
		public Vector3 outerSpaceOffset = Vector3.zero;
		
		//------------------------------------------------------------------------------------

		Light m_SunLight = null;        // Sun light component.
		Light m_MoonLight = null;       // Moon light component.


		float spaceAngle;


		// [Header("Wind")]
		// public Transform windTransform;

		
		
		[Header("Precipitation")]
		public Texture2D precipitationNoiseMap;
		public Texture2D rainTexture, snowTexture, groundFogTexture;
		
		[Header("Rendering")]
		public float sunLightThreshold = 0.20f;
		public bool HDR = false;
		public UnityEngine.Rendering.AmbientMode ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
		public FogMode unityFogMode = FogMode.ExponentialSquared;

		
		Precipitator rainPrecipitator, snowPrecipitator;
		GroundFog groundFogPrecipitator;

		LSkyTOD timeOfDayScript;
		
		float reflectionProbeTimer; 
		
		ReflectionProbe m_Probe;
		
		[Header("Clouds")]
		public Texture2D clouds0texture;
		public Texture2D clouds1texture;

		[Header("Magnetic Field")]
		public Texture2D magneticFieldTexture;


		public float windYRotation;

		public void SetWindRotation (Quaternion rotation){
			SetWindRotation(rotation.eulerAngles.y);
		}

		public void SetWindRotation (float rotation){
			windYRotation = rotation;
		}
		public void SetRotation (Vector3 towardsDirection) {
			SetWindRotation(Quaternion.LookRotation(towardsDirection));
		}


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


			groundFogPrecipitator = GetComponentInChildren<GroundFog>();

			Transform rainChild = transform.Find("Rain");
			if (rainChild != null) {
				rainPrecipitator = rainChild.GetComponent<Precipitator>();
			}
			Transform snowChild = transform.Find("Snow");
			if (snowChild != null) {
				snowPrecipitator = snowChild.GetComponent<Precipitator>();
			}
			

		}

		bool CheckIfReady () {
			if (skyboxMaterial == null) {
				Debug.LogWarning("no skybox material");
				return false;
			}
			if (targetWeather == null) {
				Debug.LogWarning("no target weather");
				return false;
			}
			if (oldWeather == null) {
				Debug.LogWarning("no old weather");
				return false;
			}
			return true;
		}


		void SetEnvironmentParticle (EnvironmentParticles particles, Weather.EnvironmentParticleValues values0, Weather.EnvironmentParticleValues values1, Texture2D texture, float tod, float windStrength, bool onEnable) {
			if (onEnable) {
				particles.SetNoiseTexture(precipitationNoiseMap);
				particles.SetMainTexture(texture);
			}

			particles.SetParticlesAmount(LerpWeatherSwitch(values0.amount, values1.amount, tod));
			particles.SetColor (LerpWeatherSwitch(values0.color, values1.color, tod));
			particles.SetHueVariation (LerpWeatherSwitch(values0.hueVariation, values1.hueVariation, tod));
			particles.SetMoveSpeed (LerpWeatherSwitch(values0.moveSpeed, values1.moveSpeed, tod));
			particles.SetQuadSize (LerpWeatherSwitch(values0.quadSize, values1.quadSize, tod));
			particles.SetCameraRange(LerpWeatherSwitch(values0.cameraRange, values1.cameraRange, tod));
			particles.SetFlutterFrequency (LerpWeatherSwitch(values0.flutterFrequency, values1.flutterFrequency, tod));
			particles.SetFlutterSpeed (LerpWeatherSwitch(values0.flutterSpeed, values1.flutterSpeed, tod));
			particles.SetFlutterMagnitude (LerpWeatherSwitch(values0.flutterMagnitude, values1.flutterMagnitude, tod));
			particles.SetSizeRange(LerpWeatherSwitch(values0.sizeRange, values1.sizeRange, tod));

			particles.SetWindDirection (windYRotation, windStrength);
		}

		void SetRain (float tod, float windStrength, bool onEnable) {
			if (rainPrecipitator == null)
				return;
			
			SetEnvironmentParticle(rainPrecipitator, oldWeather.rain.baseValues, targetWeather.rain.baseValues, rainTexture, tod, windStrength, onEnable);
			rainPrecipitator.SetMaxTravelDistance (LerpWeatherSwitch(oldWeather.rain.maxTravelDistance, targetWeather.rain.maxTravelDistance, tod));
		}

		void SetSnow (float tod, float windStrength, bool onEnable) {
			if (snowPrecipitator == null)
				return;
			SetEnvironmentParticle(snowPrecipitator, oldWeather.snow.baseValues, targetWeather.snow.baseValues, snowTexture, tod, windStrength, onEnable);
			snowPrecipitator.SetMaxTravelDistance (LerpWeatherSwitch(oldWeather.snow.maxTravelDistance, targetWeather.snow.maxTravelDistance, tod));
		}
		
		void SetGroundFog (float tod, float windStrength, bool onEnable) {
			if (groundFogPrecipitator == null)
				return;
			
			SetEnvironmentParticle(groundFogPrecipitator, oldWeather.groundFog.baseValues, targetWeather.groundFog.baseValues, groundFogTexture, tod, windStrength, onEnable);
					
			groundFogPrecipitator.SetRotateSpeed (LerpWeatherSwitch(oldWeather.groundFog. rotateSpeed, targetWeather.groundFog.rotateSpeed, tod));
			groundFogPrecipitator.SetSoftParticleFactor (LerpWeatherSwitch(oldWeather.groundFog. softParticleFactor, targetWeather.groundFog.softParticleFactor, tod)); 
			groundFogPrecipitator.SetStartEndFadeRange (LerpWeatherSwitch(oldWeather.groundFog. startEndFadeRange, targetWeather.groundFog.startEndFadeRange, tod)); 
			groundFogPrecipitator.SetCloseCamRange (LerpWeatherSwitch(oldWeather.groundFog.closeCamRange, targetWeather.groundFog.closeCamRange, tod));

			Vector2 heightFadeRange = LerpWeatherSwitch(oldWeather.groundFog.heightFadeRange, targetWeather.groundFog.heightFadeRange, tod);

			Vector3 heightFadeAndSteepness = new Vector3(heightFadeRange.x, heightFadeRange.y, LerpWeatherSwitch(oldWeather.groundFog.heightFadeSteepness, targetWeather.groundFog.heightFadeSteepness, tod));
			groundFogPrecipitator.SetHeightFadeAndSteepness (heightFadeAndSteepness);
		}


		public void SwitchWeather (Weather newWeather, float weatherSwitchSet = 0) {
			oldWeather = targetWeather;
			targetWeather = newWeather;
			if (oldWeather == null) {
				oldWeather = targetWeather;
			}
			weatherSwitch = weatherSwitchSet;
		}


		void SetAllEnvironmentParticles (float tod, bool onEnable) {
			// if (windTransform == null) {
			// 	return;	
			// }
			// float windYRotation = windTransform.eulerAngles.y;
			float windStrength = LerpWeatherSwitch(oldWeather.windStrength, targetWeather.windStrength, tod);

			Shader.SetGlobalFloat("_ENVIRONMENT_STORM", windStrength);

			SetGroundFog(tod, windStrength, onEnable);
			SetRain(tod, windStrength, onEnable);
			SetSnow(tod, windStrength, onEnable);
		}

		

		void UpdateProbe(float updateInterval, RenderTexture rt = null)
		{
			if (!Application.isPlaying)
				return;

			float updateRate = timeOfDayScript.dayInSeconds / 24.0f;// updateInterval;// 1.0f / updateInterval;
			reflectionProbeTimer         += Time.deltaTime;

			if(reflectionProbeTimer >= updateRate) 
			{
				DynamicGI.UpdateEnvironment();
				// Debug.Log("rendering probe");
				m_Probe.RenderProbe(rt);


				reflectionProbeTimer = 0;
			}
		}
		//------------------------------------------------------------------------------------

		public void SetSunLightLocalRotation(Quaternion rot)
		{
			m_SunLight.transform.localRotation = rot;
		}

		public void SetMoonLightLocalRotation(Quaternion rot)
		{
			m_MoonLight.transform.localRotation = rot;
		}



		void Update()
		{
			if (!CheckIfReady())
				return;

			float tod = timeOfDayScript.TOD;
			bool onEnable = !Application.isPlaying;

			Sun(tod);
			Moon(tod, onEnable);
			OuterSpace(tod, onEnable);
			Atmosphere(tod);
			ColorCorrection(tod, onEnable);
			Lighting(tod, onEnable);

			SetClouds(tod, onEnable);
			SetMagneticField(tod, onEnable);

			SetAllEnvironmentParticles(tod, onEnable);
			
			UpdateProbe(15);
		}


		void Sun(float tod)
		{
			skyboxMaterial.SetVector("LSky_SunDir", -m_SunLight.transform.forward);
			skyboxMaterial.SetFloat("_SunDiscSize", LerpWeatherSwitch(oldWeather. sunDiscSize, targetWeather.sunDiscSize, tod));//.OutputValue);
		}
		
		//-----------------------------------------------------------------------------------------------------------------------------

		void Moon(float tod, bool onEnable)
		{
			if (onEnable) {
				skyboxMaterial.SetTexture("_MoonTexture", moonTexture);
			}

			skyboxMaterial.SetVector("LSky_MoonDir", -m_MoonLight.transform.forward);
			skyboxMaterial.SetMatrix("LSky_MoonMatrix", m_MoonLight.transform.worldToLocalMatrix);
		
			skyboxMaterial.SetFloat("_MoonSize", LerpWeatherSwitch(oldWeather. moonSize, targetWeather.moonSize, tod));//.OutputValue);
			skyboxMaterial.SetColor("_MoonColor", LerpWeatherSwitch(oldWeather. moonColor, targetWeather.moonColor, tod));//.OutputColor);
		
		}

		void SetMagneticField (float tod, bool onEnable) {
			if (onEnable) {
				skyboxMaterial.SetTexture("_MageneticField", magneticFieldTexture);
			}

		
			skyboxMaterial.SetColor("_MagFieldColor0", LerpWeatherSwitch(oldWeather. magneticField.color0, targetWeather.magneticField.color0, tod));
			skyboxMaterial.SetColor("_MagFieldColor1", LerpWeatherSwitch(oldWeather. magneticField.color1, targetWeather.magneticField.color1, tod));
			
			skyboxMaterial.SetVector("_MagneticFieldNoiseTileAndShiftSpeed", LerpWeatherSwitch(oldWeather. magneticField.colorNoiseTileAndShiftSpeed, targetWeather.magneticField.colorNoiseTileAndShiftSpeed, tod) );
			skyboxMaterial.SetVector("_MagFieldColorNoiseParams", LerpWeatherSwitch(oldWeather. magneticField.colorNoiseSampleParams, targetWeather.magneticField.colorNoiseSampleParams, tod ) );

			skyboxMaterial.SetVector("_MagneticFieldTileAndShiftSpeed", LerpWeatherSwitch(oldWeather. magneticField.tileAndShiftSpeed, targetWeather.magneticField.tileAndShiftSpeed, tod) );
			skyboxMaterial.SetVector("_MagFieldNoiseParams", LerpWeatherSwitch(oldWeather. magneticField.noiseSampleParams, targetWeather.magneticField.noiseSampleParams, tod ) );

			skyboxMaterial.SetVector("_MageneticFieldSwimSizeAndHeightFadeSteepness", new Vector3(
				LerpWeatherSwitch(oldWeather. magneticField.swimSizeX, targetWeather.magneticField.swimSizeX, tod), 
				LerpWeatherSwitch(oldWeather. magneticField.swimSizeY, targetWeather.magneticField.swimSizeY, tod), 
				LerpWeatherSwitch(oldWeather. magneticField.heightFadeSteepness, targetWeather.magneticField.heightFadeSteepness, tod)) 
			);
			
			skyboxMaterial.SetVector("_MagneticFieldSwimFrequencyAndSpeed", LerpWeatherSwitch(oldWeather. magneticField.swimTileAndSpeed, targetWeather.magneticField.swimTileAndSpeed, tod ) );
			
			

		}


		void SetClouds (float tod, bool onEnable) {
			if (onEnable) {
				skyboxMaterial.SetTexture("_Clouds0", clouds0texture);
				skyboxMaterial.SetTexture("_Clouds1", clouds1texture);
			}

			skyboxMaterial.SetColor("_Clouds0Color", LerpWeatherSwitch(oldWeather. clouds0.color, targetWeather.clouds0.color, tod));
			skyboxMaterial.SetVector("_Clouds0NoiseParams", LerpWeatherSwitch(oldWeather. clouds0.noiseSampleParams, targetWeather.clouds0.noiseSampleParams, tod) );
			skyboxMaterial.SetVector("_Clouds0TileAndShiftSpeed", LerpWeatherSwitch(oldWeather. clouds0.tileAndShiftSpeed, targetWeather.clouds0.tileAndShiftSpeed, tod ) );

			skyboxMaterial.SetColor("_Clouds1Color", LerpWeatherSwitch(oldWeather. clouds1.color, targetWeather.clouds1.color, tod));
			skyboxMaterial.SetVector("_Clouds1NoiseParams", LerpWeatherSwitch(oldWeather. clouds1.noiseSampleParams, targetWeather.clouds1.noiseSampleParams, tod) );
			skyboxMaterial.SetVector("_Clouds1TileAndShiftSpeed", LerpWeatherSwitch(oldWeather. clouds1.tileAndShiftSpeed, targetWeather.clouds1.tileAndShiftSpeed, tod ) );
		}


		void OuterSpace(float tod,bool onEnable)
		{

			if (onEnable) {

				skyboxMaterial.SetTexture("_OuterSpaceCube", outerSpaceCube);
			}
			
			
			if (Application.isPlaying) {
				spaceAngle += Time.deltaTime * ( 360f / timeOfDayScript.dayInSeconds) * spaceParallaxScrollModifier;
			}
			else {
				spaceAngle = tod * 360 * spaceParallaxScrollModifier;
			}
			spaceAngle = Mathf.Repeat(spaceAngle, 360);

			

			skyboxMaterial.SetMatrix("_OuterSpaceMatrix", Matrix4x4.TRS (Vector3.zero, Quaternion.Euler(outerSpaceOffset) * (Quaternion.Euler(spaceAngle,0,0) * transform.rotation), Vector3.one));//spaceTransform.GetChild(0).worldToLocalMatrix);

			skyboxMaterial.SetColor("_StarsColor", LerpWeatherSwitch(oldWeather. starsColor, targetWeather.starsColor, tod));//.OutputColor);
			
			skyboxMaterial.SetFloat("_StarsScintillation", LerpWeatherSwitch(oldWeather. starsScintillation, targetWeather.starsScintillation, tod));//.OutputValue);
			skyboxMaterial.SetFloat("_TwinkleSpeed", LerpWeatherSwitch(oldWeather. starsScintillationSpeed, targetWeather.starsScintillationSpeed, tod));//.OutputValue);

			skyboxMaterial.SetColor("_NebulaColor", LerpWeatherSwitch(oldWeather. nebulaColor, targetWeather.nebulaColor, tod));//.OutputColor);
			

		}

		void Atmosphere(float tod)
		{
		
			// Atmosphere based in GPUGems2(Sean Oneil).
			// Inverse wave legths (reciprocal).
			Color wl = LerpWeatherSwitch(oldWeather. waveLengths, targetWeather.waveLengths, tod);
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

			float kSunBrightness = LerpWeatherSwitch(oldWeather. sunBrightness, targetWeather.sunBrightness, tod);
			float kMie           = LerpWeatherSwitch(oldWeather. mie, targetWeather.mie, tod);
			float kKmESun        = kMie * kSunBrightness;
			float kKm4PI         = kMie * 4.0f * Mathf.PI;
			//-----------------------------------------------------------------------------------------------------------------------------

			float kRayleigh      = 0.0025f * LerpWeatherSwitch(oldWeather. atmosphereThickness, targetWeather.atmosphereThickness, tod);
			float kKrESun        = kRayleigh * kSunBrightness;
			float kKr4PI         = kRayleigh * 4.0f * Mathf.PI;
			//-----------------------------------------------------------------------------------------------------------------------------


			skyboxMaterial.SetFloat("LSky_kCameraHeight", kCameraHeight);
			skyboxMaterial.SetFloat("LSky_kInnerRadius",  kInnerRadius);
			skyboxMaterial.SetFloat("LSky_kInnerRadius2", kInnerRadius2);
			skyboxMaterial.SetFloat("LSky_kOuterRadius",  kOuterRadius);
			skyboxMaterial.SetFloat("LSky_kOuterRadius2", kOuterRadius2);
			skyboxMaterial.SetFloat("LSky_kScale",               kScale);
			skyboxMaterial.SetFloat("LSky_kScaleDepth",          kScaleDepth);
			skyboxMaterial.SetFloat("LSky_kScaleOverScaleDepth", kScaleOverScaleDepth);
			skyboxMaterial.SetFloat("LSky_kKm4PI",  kKm4PI);
			skyboxMaterial.SetFloat("LSky_kKmESun", kKmESun);
			skyboxMaterial.SetFloat("LSky_kKrESun", kKrESun);
			skyboxMaterial.SetFloat("LSky_kKr4PI",  kKr4PI);
			skyboxMaterial.SetVector("LSky_InvWavelength", InvWavelength);
			skyboxMaterial.SetColor("LSky_DayAtmosphereTint", LerpWeatherSwitch(oldWeather. dayAtmosphereTint, targetWeather.dayAtmosphereTint, tod));
			skyboxMaterial.SetColor("LSky_NightAtmosphereTint", LerpWeatherSwitch(oldWeather. nightAtmosphereTint, targetWeather.nightAtmosphereTint, tod));

			skyboxMaterial.SetVector("LSky_SunBetaMiePhase", SunBetaMiePhase( LerpWeatherSwitch(oldWeather. sunMieAnisotropy, targetWeather.sunMieAnisotropy,tod)));
			skyboxMaterial.SetFloat("LSky_SunMieScattering", LerpWeatherSwitch(oldWeather. sunMieScattering, targetWeather.sunMieScattering, tod));
			skyboxMaterial.SetColor("LSky_SunMieColor", LerpWeatherSwitch(oldWeather. sunMieColor, targetWeather.sunMieColor, tod));//.OutputColor);
			
			skyboxMaterial.SetVector("LSky_MoonBetaMiePhase", MoonBetaMiePhase(LerpWeatherSwitch(oldWeather. moonMieAnisotropy, targetWeather.moonMieAnisotropy, tod)));
			skyboxMaterial.SetFloat("LSky_MoonMieScattering", LerpWeatherSwitch(oldWeather. moonMieScattering, targetWeather.moonMieScattering, tod));// * moonMieMultiplier.OutputValue);
			skyboxMaterial.SetColor("LSky_MoonMieColor", LerpWeatherSwitch(oldWeather. moonMieColor, targetWeather.moonMieColor, tod));//.OutputColor);

			skyboxMaterial.SetFloat("_HorizonFade", LerpWeatherSwitch(oldWeather. horizonFade, targetWeather.horizonFade, tod));
			skyboxMaterial.SetFloat("_GroundFade", LerpWeatherSwitch(oldWeather. groundFade, targetWeather.groundFade, tod));
			skyboxMaterial.SetFloat("_GroundAltitude", LerpWeatherSwitch(oldWeather. groundAltitude, targetWeather.groundAltitude, tod));
			skyboxMaterial.SetFloat("_NebulaExponent", LerpWeatherSwitch(oldWeather. nebulaExponent, targetWeather.nebulaExponent, tod));
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

		void ColorCorrection(float tod, bool onEnable)
		{
			if (onEnable) {
				if(!HDR) skyboxMaterial.DisableKeyword("LSKY_HDR"); 
				else     skyboxMaterial.EnableKeyword("LSKY_HDR");
				
				if(QualitySettings.activeColorSpace == ColorSpace.Gamma)
					skyboxMaterial.EnableKeyword("LSKY_GAMMA_COLOR_SPACE");
				else
					skyboxMaterial.DisableKeyword("LSKY_GAMMA_COLOR_SPACE");
			}

			skyboxMaterial.SetFloat("LSky_Exposure", LerpWeatherSwitch(oldWeather. exposure, targetWeather.exposure, tod));

		}

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


		void Lighting(float tod, bool onEnable)
		{
			if (onEnable) {
				RenderSettings.skybox = skyboxMaterial;
				RenderSettings.fog = true;
				RenderSettings.ambientMode = ambientMode;
				RenderSettings.fogMode  = unityFogMode;
			}

			Color sunColor = LerpWeatherSwitch(oldWeather. sunLightColor_Intensity, targetWeather.sunLightColor_Intensity, tod);
			float intensity = LerpWeatherSwitch(oldWeather.sunLightMaxIntensity, targetWeather.sunLightMaxIntensity) * sunColor.a;

			m_SunLight.color     = sunColor;
			m_SunLight.intensity = intensity;

			CheckLight(m_SunLight, sunLightThreshold);
			
			Color moonColor = LerpWeatherSwitch(oldWeather. moonLightColor_Intensity, targetWeather.moonLightColor_Intensity, tod);
			intensity = LerpWeatherSwitch(oldWeather.moonLightMaxIntensity, targetWeather.moonLightMaxIntensity) * moonColor.a;

			m_MoonLight.color     = moonColor;
			m_MoonLight.intensity = intensity;
			CheckLight(m_MoonLight, sunLightThreshold);
			


			switch (ambientMode) 
			{
				case UnityEngine.Rendering.AmbientMode.Skybox:
				RenderSettings.ambientIntensity = LerpWeatherSwitch(oldWeather. ambientIntensity, targetWeather.ambientIntensity, tod);
				break;

				case UnityEngine.Rendering.AmbientMode.Trilight: 
				RenderSettings.ambientSkyColor     = LerpWeatherSwitch(oldWeather. ambientSkyColor, targetWeather.ambientSkyColor, tod);
				RenderSettings.ambientEquatorColor = LerpWeatherSwitch(oldWeather. ambientEquatorColor, targetWeather.ambientEquatorColor, tod);
				break;

				case UnityEngine.Rendering.AmbientMode.Flat :
				RenderSettings.ambientSkyColor     = LerpWeatherSwitch(oldWeather. ambientSkyColor, targetWeather.ambientSkyColor, tod);
				break;
			}

			RenderSettings.ambientGroundColor   = LerpWeatherSwitch(oldWeather. ambientGroundColor, targetWeather.ambientGroundColor, tod);
			skyboxMaterial.SetColor("LSky_GroundColor", RenderSettings.ambientGroundColor);
			
			RenderSettings.fogColor = LerpWeatherSwitch(oldWeather. unityFogColor, targetWeather.unityFogColor,tod);

			if (unityFogMode == FogMode.Linear)
			{
				RenderSettings.fogStartDistance = LerpWeatherSwitch(oldWeather. unityFogStartDistance, targetWeather.unityFogStartDistance, tod);
				RenderSettings.fogEndDistance   = LerpWeatherSwitch(oldWeather. unityFogEndDistance, targetWeather.unityFogEndDistance,tod);
			}
			else {
				RenderSettings.fogDensity = LerpWeatherSwitch(oldWeather. unityFogDensity, targetWeather.unityFogDensity,tod);
			}
		}




		
		float LerpWeatherSwitch (AnimationCurve a, AnimationCurve b, float tod) {
			if (weatherSwitch == 1)
				return b.Evaluate(tod);
			else if (weatherSwitch == 0)
				return a.Evaluate(tod);
			else
				return Mathf.Lerp(a.Evaluate(tod), b.Evaluate(tod), weatherSwitch);
		}
		float LerpWeatherSwitch (float a, float b) {
			if (weatherSwitch == 1) 
				return b;
			else if (weatherSwitch == 0)
				return a;
			else 
				return Mathf.Lerp(a, b, weatherSwitch);
		}
		Vector2 LerpWeatherSwitch (Weather.Vector2Curve a, Weather.Vector2Curve b, float tod) {
			if (weatherSwitch == 1)
				return b.Evaluate(tod);
			else if (weatherSwitch == 0) 
				return a.Evaluate(tod);
			else 
				return Vector2.Lerp(a.Evaluate(tod), b.Evaluate(tod), weatherSwitch);
		}
		Vector3 LerpWeatherSwitch (Weather.NoiseSampleParams a, Weather.NoiseSampleParams b, float tod) {
			if (weatherSwitch == 1)
				return b.Evaluate(tod);
			else if (weatherSwitch == 0)
				return a.Evaluate(tod);
			else 
				return Vector3.Lerp(a.Evaluate(tod), b.Evaluate(tod), weatherSwitch);
		}
		Vector4 LerpWeatherSwitch (Weather.TileAndShiftSpeedParams a, Weather.TileAndShiftSpeedParams b, float tod) {
			if (weatherSwitch == 1)
				return b.Evaluate(tod);
			else if (weatherSwitch == 0)
				return a.Evaluate(tod);
			else 
				return Vector4.Lerp(a.Evaluate(tod), b.Evaluate(tod), weatherSwitch);
		}
		Color LerpWeatherSwitch (Gradient a, Gradient b, float tod) {
			if (weatherSwitch == 1) 
				return b.Evaluate(tod);
			else if (weatherSwitch == 0)
				return a.Evaluate(tod);
			else 
				return Color.Lerp(a.Evaluate(tod), b.Evaluate(tod), weatherSwitch);
		}

	}
}
