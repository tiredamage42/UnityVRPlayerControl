



using UnityEngine;
using UnityEditor;
using System.Reflection;
namespace AC.LSky
{

	[CustomEditor(typeof(LSky))] 
	public class LSkyEditor : Editor
	{
		
		public static void Separator(int height)
		{
			GUILayout.Box("", new GUILayoutOption[] {GUILayout.ExpandWidth(true), GUILayout.Height(height)});			
		}

		
		public static void Label(string text, GUIStyle textStyle, bool center)
		{

			if(center)
			{


				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				GUILayout.Label(text, textStyle);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

			} 
			else
			{
				GUILayout.Label(text, textStyle);
			}
				
		}

		SerializedObject  serObj;
		SerializedProperty skyboxMaterial;
		SerializedProperty moonTexture;
		SerializedProperty outerSpaceCube;


		GradientDrawer waveLengths;
		CurveDrawer atmosphereThickness;

		GradientDrawer dayAtmosphereTint;
		GradientDrawer nightAtmosphereTint;
		
		CurveDrawer sunBrightness;
		CurveDrawer mie;

		GradientDrawer sunMieColor;
		CurveDrawer sunMieAnisotropy;
		CurveDrawer sunMieScattering;

		GradientDrawer moonMieColor;
		CurveDrawer moonMieAnisotropy;
		CurveDrawer moonMieScattering;
		CurveDrawer sunDiscSize;
		// SerializedProperty sunDiscColor;

		CurveDrawer moonSize;
		GradientDrawer moonColor;
		SerializedProperty outerSpaceOffset;
		GradientDrawer starsColor;
		CurveDrawer starsScintillation;
		CurveDrawer starsScintillationSpeed;

		GradientDrawer nebulaColor;
		
		//----------------------------------------------
		SerializedProperty HDR;
		CurveDrawer exposure;
		SerializedProperty sunLightThreshold;
		
		
		GradientDrawer sunLightColor_Intensity;
		SerializedProperty sunLightMaxIntensity;
		GradientDrawer moonLightColor_Intensity;
		SerializedProperty moonLightMaxIntensity;
		


		SerializedProperty ambientMode;
		GradientDrawer ambientSkyColor;
		GradientDrawer ambientEquatorColor;
		GradientDrawer ambientGroundColor;
		CurveDrawer ambientIntensity;
		//----------------------------------------------

		SerializedProperty unityFogMode;
		GradientDrawer unityFogColor;
		CurveDrawer unityFogDensity;
		CurveDrawer unityFogStartDistance;
		CurveDrawer unityFogEndDistance;

		CurveDrawer nebulaExponent;
		CurveDrawer groundAltitude;
		CurveDrawer groundFade;
		CurveDrawer horizonFade;
		//----------------------------------------------

		LSky lsky;





		public class MagneticFieldValuesDrawer {
			
			public GradientDrawer color0;
			public GradientDrawer color1;
			public TileAndShiftSpeedParamsDrawer colorNoiseTileAndShiftSpeed;// = new TileAndShiftSpeedParams(.15f, .15f, .01f, .01f, false);
			public NoiseSampleParamsDrawer colorNoiseSampleParams;// = new NoiseSampleParams(2, 1, 2, false);
			public TileAndShiftSpeedParamsDrawer tileAndShiftSpeed;// = new TileAndShiftSpeedParams(.25f, .3f, .001f, .002f, false);
			public NoiseSampleParamsDrawer noiseSampleParams;// = new NoiseSampleParams(.75f, 3, 1, false);

			public CurveDrawer swimSizeX;// = GetTODCurve(.01f, false);
			public CurveDrawer swimSizeY;// = GetTODCurve(.025f, false);
			public CurveDrawer heightFadeSteepness;// = GetTODCurve(1, false);
			public TileAndShiftSpeedParamsDrawer swimTileAndSpeed;// = new TileAndShiftSpeedParams(10, 8, .05f, .025f, false);

			public MagneticFieldValuesDrawer (SerializedObject serializedObject, string relativePath) {

				this.color0 = new GradientDrawer(serializedObject, relativePath + ".color0", "Color 0", new Color(0, 1, 0, .5f), true);
				this.color1 = new GradientDrawer(serializedObject, relativePath + ".color1", "Color 1", new Color(0, 1, 1, .5f), true);
				
				this.colorNoiseTileAndShiftSpeed = new TileAndShiftSpeedParamsDrawer(serializedObject, relativePath + ".colorNoiseTileAndShiftSpeed", "Color Noise ", new Vector4(.15f, .15f, .01f, .01f));
				this.colorNoiseSampleParams = new NoiseSampleParamsDrawer(serializedObject, relativePath + ".colorNoiseSampleParams", "Color Noise ", new Vector3(2, 1, 2));
				this.tileAndShiftSpeed = new TileAndShiftSpeedParamsDrawer(serializedObject, relativePath + ".tileAndShiftSpeed", "", new Vector4(.25f, .3f, .001f, .002f));
				this.noiseSampleParams = new NoiseSampleParamsDrawer(serializedObject, relativePath + ".noiseSampleParams", "", new Vector3(.75f, 3, 1));
				
				this.swimSizeX = new CurveDrawer(serializedObject, relativePath + ".swimSizeX", "Swim Size X", 0, 2, .01f);
				this.swimSizeY = new CurveDrawer(serializedObject, relativePath + ".swimSizeY", "Swim Size Y", 0, 2, .025f);
				this.heightFadeSteepness = new CurveDrawer(serializedObject, relativePath + ".heightFadeSteepness", "Height Fade Steepness", .001f, 10, .1f);
				this.swimTileAndSpeed = new TileAndShiftSpeedParamsDrawer(serializedObject, relativePath + ".swimTileAndSpeed", "Swim", new Vector4(10, 8, .05f, .025f));
			}


			public void DrawProp () {
				color0.DrawProp();
				color1.DrawProp();
				colorNoiseTileAndShiftSpeed.DrawProp();
				colorNoiseSampleParams.DrawProp();
				tileAndShiftSpeed.DrawProp();
				noiseSampleParams.DrawProp();

				swimSizeX.DrawProp();
				swimSizeY.DrawProp();
				heightFadeSteepness.DrawProp();
				swimTileAndSpeed.DrawProp();

			}

				



		}
		public MagneticFieldValuesDrawer magneticField;

		public class NoiseSampleParamsDrawer {

			public CurveDrawer prePower, steepness, postPower;

			public NoiseSampleParamsDrawer(SerializedObject serializedObject, string relativePath, string prefix, Vector3 defaultValues) {
				prePower = new CurveDrawer(serializedObject, relativePath + ".prePower", prefix + "Pre Power", 0, 10, defaultValues.x);
				steepness = new CurveDrawer(serializedObject, relativePath + ".steepness", prefix + "Steepness", 0.01f, 10, defaultValues.y);
				postPower = new CurveDrawer(serializedObject, relativePath + ".postPower", prefix + "Post Power", 0, 10, defaultValues.z);
			}
			public void DrawProp () {
				prePower.DrawProp();
				steepness.DrawProp();
				postPower.DrawProp();
			}
		}

		public class TileAndShiftSpeedParamsDrawer {
			public CurveDrawer tileX, tileY, shiftSpeedX, shiftSpeedY;
			public TileAndShiftSpeedParamsDrawer(SerializedObject serializedObject, string relativePath, string prefix, Vector4 defaultValues) {
				tileX = new CurveDrawer(serializedObject, relativePath + ".tileX", prefix + "Tile X", 0, 50, defaultValues.x);
				tileY = new CurveDrawer(serializedObject, relativePath + ".tileY", prefix + "Tile Y", 0, 50, defaultValues.y);
				shiftSpeedX = new CurveDrawer(serializedObject, relativePath + ".shiftSpeedX", prefix + "Shift Speed X", 0, 50, defaultValues.z);
				shiftSpeedY = new CurveDrawer(serializedObject, relativePath + ".shiftSpeedY", prefix + "Shift Speed Y", 0, 50, defaultValues.w);
			}
			public void DrawProp () {
				tileX.DrawProp();
				tileY.DrawProp();
				shiftSpeedX.DrawProp();
				shiftSpeedY.DrawProp();
			}
			
		}


		public class CloudInfoDrawer {

			public GradientDrawer color;
			public NoiseSampleParamsDrawer noiseSampleParams;
			public TileAndShiftSpeedParamsDrawer tileAndShiftSpeed;

			
			public CloudInfoDrawer (SerializedObject serializedObject, string relativePath) {
				color = new GradientDrawer(serializedObject, relativePath + ".color", "Color", Color.white, true);
				noiseSampleParams = new NoiseSampleParamsDrawer(serializedObject, relativePath + ".noiseSampleParams", "", Vector3.one);
				tileAndShiftSpeed = new TileAndShiftSpeedParamsDrawer(serializedObject, relativePath + ".tileAndShiftSpeed", "", new Vector4(.1f, .1f, .1f, .1f));
			}

			public void DrawProp() {
				color.DrawProp();
				noiseSampleParams.DrawProp();
				tileAndShiftSpeed.DrawProp();
			}
		}

		CloudInfoDrawer clouds0, clouds1;
		SerializedProperty cloudsTexture0, cloudsTexture1, magneticFieldTexture;


		public void OnEnable()
		{
			lsky = target as LSky;

			serObj  = serializedObject;// new SerializedObject(target);
		
			skyboxMaterial = serObj.FindProperty("skyboxMaterial");
			moonTexture    = serObj.FindProperty("moonTexture");
			outerSpaceCube = serObj.FindProperty("outerSpaceCube");
		
			outerSpaceOffset         = serObj.FindProperty("outerSpaceOffset");
			HDR         = serObj.FindProperty("HDR");
			sunLightMaxIntensity	= serObj.FindProperty("sunLightMaxIntensity");
			moonLightMaxIntensity	= serObj.FindProperty("moonLightMaxIntensity");
			sunLightThreshold   = serObj.FindProperty("sunLightThreshold");
			ambientMode	        = serObj.FindProperty("ambientMode");
			unityFogMode	       = serObj.FindProperty("unityFogMode");

			cloudsTexture0 = serObj.FindProperty("clouds0.texture");
			cloudsTexture1 = serObj.FindProperty("clouds1.texture");
			magneticFieldTexture = serObj.FindProperty("magneticField.texture");

			clouds0 = new CloudInfoDrawer(serializedObject, "clouds0");
			clouds1 = new CloudInfoDrawer(serializedObject, "clouds1");
			magneticField = new MagneticFieldValuesDrawer(serializedObject, "magneticField");

			dayAtmosphereTint = new GradientDrawer(serializedObject, "dayAtmosphereTint", "Day Tint", Color.white, false);
			nightAtmosphereTint = new GradientDrawer(serializedObject, "nightAtmosphereTint", "Night Tint", new Color(0.03f, 0.05f, 0.09f, 1.0f), false);
			sunMieColor = new GradientDrawer(serializedObject, "sunMieColor", "Mie Color", new Color(1.0f, 0.95f, 0.83f, 1.0f), false);
			moonMieColor = new GradientDrawer(serializedObject, "moonMieColor", "Mie Color", new Color(0.507f, 0.695f,  1.0f, 1.0f), false);
			moonColor = new GradientDrawer(serializedObject, "moonColor","Color", Color.white, true);
			starsColor = new GradientDrawer(serializedObject, "starsColor", "Color", Color.white, true);
			nebulaColor = new GradientDrawer(serializedObject, "nebulaColor", "Outer Space Color", Color.white, true);
			sunLightColor_Intensity = new GradientDrawer(serializedObject, "sunLightColor_Intensity", "Sun Color / Intensity", new Color(1.0f, 0.956f, 0.839f, 1.0f), true);
			moonLightColor_Intensity = new GradientDrawer(serializedObject, "moonLightColor_Intensity", "Moon Color / Intensity", new Color(0.632f, 0.794f, 1.0f, 1.0f), true);
			ambientSkyColor = new GradientDrawer(serializedObject, "ambientSkyColor", "Sky Color", new Color(0.443f, 0.552f, 0.737f, 1.0f), false, "Ambient Triplanar/Color");
			ambientEquatorColor = new GradientDrawer(serializedObject, "ambientEquatorColor", "Equator Color", new Color(0.901f, 0.956f, 0.968f, 1.0f), false, "Ambient Triplanar");
			ambientGroundColor = new GradientDrawer(serializedObject, "ambientGroundColor", "Ground Color", new Color(0.466f, 0.435f, 0.415f, 1.0f), false);
			unityFogColor = new GradientDrawer(serializedObject, "unityFogColor", "Color", new Color(0.901f, 0.956f, 0.968f, 1.0f), false);
			waveLengths = new GradientDrawer(serializedObject, "waveLengths", "Wavelengths", new Color(.65f, .57f, .475f, 1), false);

			unityFogEndDistance = new CurveDrawer(serializedObject, "unityFogEndDistance", "End Distance", 0, 1000, 300, "Fog Linear");
			unityFogStartDistance = new CurveDrawer(serializedObject, "unityFogStartDistance", "Start Distance", 0, 1000, 0, "Fog Linear");
			unityFogDensity = new CurveDrawer(serializedObject, "unityFogDensity", "Density", 0, 2, 0.01f, "Fog Exponential");
			ambientIntensity = new CurveDrawer(serializedObject, "ambientIntensity", "Intensity", 0, 8, 1f, "Skybox Ambient");
			starsScintillationSpeed = new CurveDrawer(serializedObject, "starsScintillationSpeed", "Twinkle Speed", 0, 5, 1f);
			starsScintillation = new CurveDrawer(serializedObject, "starsScintillation", "Twinkle Intensity", 0, 1, .9f);
			moonSize = new CurveDrawer(serializedObject, "moonSize", "Size", 0, 5, .3f);
			sunDiscSize = new CurveDrawer(serializedObject, "sunDiscSize", "Mie Size", 0, 25, 5f);
			moonMieAnisotropy = new CurveDrawer(serializedObject, "moonMieAnisotropy", "Mie Anisotropy", 0, .98f, .93f);
			moonMieScattering = new CurveDrawer(serializedObject, "moonMieScattering", "Mie Scattering", 00001f, 25f, .5f);
			sunMieAnisotropy = new CurveDrawer(serializedObject, "sunMieAnisotropy", "Mie Anisotropy", 0, .98f, .75f);
			sunMieScattering = new CurveDrawer(serializedObject, "sunMieScattering", "Mie Scattering", 00001f, 25f, .5f);
			mie = new CurveDrawer(serializedObject, "mie", "Sun Mie", 0, 5f, .01f);
			sunBrightness = new CurveDrawer(serializedObject, "sunBrightness", "Brightness", 0, 100f, 30f);
			atmosphereThickness = new CurveDrawer(serializedObject, "atmosphereThickness", "Atmosphere Thickness", 0, 50, 1);
			exposure = new CurveDrawer(serializedObject, "exposure", "Exposure", 0, 10, 1.3f);
			nebulaExponent = new CurveDrawer(serializedObject, "nebulaExponent", "Outer Space Steepness", 0.01f, 10, 2f);
			groundAltitude = new CurveDrawer(serializedObject, "groundAltitude", "Ground Altitude", -1f, 1, 0f);
			groundFade = new CurveDrawer(serializedObject, "groundFade", "Ground Fade", 0.0f, 60, 30f);
			horizonFade = new CurveDrawer(serializedObject, "horizonFade", "Horizon Fade", -.5f, .5f, .006f);
		}
		public static void ShurikenFoldoutHeader(string text, GUIStyle texStyle)
		{
			GUIStyle h = new GUIStyle("ShurikenModuleTitle")
			{
				font          = new GUIStyle("Label").font,
				border        = new RectOffset(15, 7, 4, 4),
				fixedHeight   = 22,
				contentOffset = new Vector2(20f, -2f)
			}; 

			EditorGUILayout.BeginHorizontal(h, GUILayout.Height(25));
			{
				Label (text, texStyle, true);
			}
			EditorGUILayout.EndHorizontal();
		}

		public override void OnInspectorGUI()
		{
			serObj.Update ();
			
			EditorGUILayout.Separator();

			GUIStyle style = new GUIStyle(EditorStyles.label); 
			style.fontStyle = FontStyle.Bold;
			
			ShurikenFoldoutHeader("World", style);
			World();
			
			ShurikenFoldoutHeader("Atmosphere", style);
			Atmosphere();
			
			ShurikenFoldoutHeader("Sun", style);
			Sun();
			
			ShurikenFoldoutHeader("Moon", style);
			Moon();
			
			ShurikenFoldoutHeader("Stars", style);
			Stars();
			
			ShurikenFoldoutHeader("Lights", style);
			Lighting();
			
			ShurikenFoldoutHeader("Ambient Lighting", style);
			AmbientLighting();

			ShurikenFoldoutHeader("Fog", style);
			Fog();
			
			ShurikenFoldoutHeader("Clouds 0", style);
			clouds0.DrawProp();
			
			ShurikenFoldoutHeader("Clouds 1", style);
			clouds1.DrawProp();
			
			ShurikenFoldoutHeader("Magnetic Field", style);
			magneticField.DrawProp();

			serObj.ApplyModifiedProperties();
		}


		void World () {
			
			EditorGUILayout.PropertyField(skyboxMaterial, new GUIContent("Skybox Material")); 
			
			EditorGUILayout.PropertyField(moonTexture, new GUIContent("Moon Texture")); 
			
			EditorGUILayout.PropertyField (outerSpaceCube, new GUIContent ("Outer Space Cube")); 
			EditorGUILayout.HelpBox ("RGB = Nebula, A = StarField", MessageType.Info);
			
			EditorGUILayout.PropertyField(cloudsTexture0, new GUIContent("Clouds 0 Texture"));
			EditorGUILayout.PropertyField(cloudsTexture1, new GUIContent("Clouds 1 Texture"));
			EditorGUILayout.PropertyField(magneticFieldTexture, new GUIContent("Magnetic Field Texture"));

			EditorGUILayout.PropertyField(sunLightThreshold, new GUIContent("Light Intensity Threshold")); 
			
			EditorGUILayout.PropertyField(serObj.FindProperty("spaceParallaxScrollModifier"));
			EditorGUILayout.PropertyField(outerSpaceOffset, new GUIContent("Space Offsets")); 
			EditorGUILayout.PropertyField(unityFogMode, new GUIContent ("Unity Fog Mode")); 
			EditorGUILayout.PropertyField(ambientMode, new GUIContent("Ambient Mode")); 
			EditorGUILayout.PropertyField(HDR, new GUIContent ("HDR")); 
			EditorGUILayout.Separator();

		}

		
		void Atmosphere()
		{
			horizonFade.DrawProp();
			groundFade.DrawProp();
			groundAltitude.DrawProp();
			exposure.DrawProp();
			waveLengths.DrawProp();
			atmosphereThickness.DrawProp();
			dayAtmosphereTint.DrawProp();
			nightAtmosphereTint.DrawProp();
		}

		void Sun () {
			mie.DrawProp();			
			sunBrightness.DrawProp();
			sunMieColor.DrawProp();
			sunMieAnisotropy.DrawProp();
			sunMieScattering.DrawProp();
			sunDiscSize.DrawProp();
		}

		void Moon () {
			moonMieColor.DrawProp();
			moonMieAnisotropy.DrawProp();
			moonMieScattering.DrawProp();
			moonSize.DrawProp();
			moonColor.DrawProp();
		}
			
		void Stars () {
			starsColor.DrawProp();
			starsScintillation.DrawProp();
			starsScintillationSpeed.DrawProp();
			nebulaColor.DrawProp();
			nebulaExponent.DrawProp();
		}

		void AmbientLighting () {
			ambientIntensity.DrawProp();
			ambientSkyColor.DrawProp();
			ambientEquatorColor.DrawProp();
			ambientGroundColor.DrawProp();
		}

		void Fog() {
			unityFogColor.DrawProp();
			unityFogDensity.DrawProp();
			unityFogStartDistance.DrawProp();
			unityFogEndDistance.DrawProp();			
		}

		void Lighting() {
			sunLightColor_Intensity.DrawProp();
			EditorGUILayout.PropertyField(sunLightMaxIntensity);
			moonLightColor_Intensity.DrawProp();
			EditorGUILayout.PropertyField(moonLightMaxIntensity);
		}
		
		static internal int SeparatorHeight{ get{ return 2; } }
		
		public enum TODValuePhase {
			All_Phases = 0, Night = 1, Sunrise = 2, Day = 3, Midday = 4, Sunset = 5
		}

		static int[][] indiciesPerPhase_CURVE = new int[][] {
			new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 },
			new int[] { 0, 1, 7, 8 },
			new int[] { 2 },
			new int[] { 3, 5 },
			new int[] { 4 },
			new int[] { 6 },
		};
		static int[][] indiciesPerPhase_GRADIENT= new int[][] {
			new int[] { 0, 1, 2, 3, 4, 5, 6 },
			new int[] { 0, 6 },
			new int[] { 1 },
			new int[] { 2, 4 },
			new int[] { 3 },
			new int[] { 5 },
		};

		public abstract class ValueDrawer<T> {
			protected T workingValue, defaultValue;
			protected SerializedProperty prop;

			protected void InitializeDrawer (SerializedObject serializedObject, string propName, string displayName, T defaultValue, string hint) {
				prop = serializedObject.FindProperty(propName);
				this.defaultValue = defaultValue;
				workingValue = defaultValue;
				gui = new GUIContent(displayName, "Default Value: " + defaultValue + " :: [" + hint + "]");				
			}

			protected TODValuePhase valuePhase;
			protected bool foldout = false;
			protected GUIContent gui;
			protected abstract void RetreiveValueButtons();
			protected abstract void SetValueButtons();

			public void DrawProp () {
				foldout = GUILayout.Toggle(foldout, new GUIContent(gui), EditorStyles.foldout, GUILayout.Width(25));
				if (foldout) {
					EditorGUI.indentLevel+=2;
					EditorGUILayout.PropertyField (prop, GUIContent.none);
					EditorGUILayout.BeginHorizontal();
					valuePhase = (TODValuePhase)EditorGUILayout.EnumPopup(valuePhase);

					if (valuePhase != TODValuePhase.All_Phases) {
						RetreiveValueButtons();
					}

					if (valuePhase == TODValuePhase.All_Phases) {
						GUI.backgroundColor = new Color(1, .25f, .25f, 1);
					}

					SetValueButtons();
					GUI.backgroundColor = Color.white;
					EditorGUILayout.EndHorizontal();
					DrawWorkingValue ();
					EditorGUI.indentLevel-=2;
				}
				Separator(SeparatorHeight);
			}

			protected abstract void DrawWorkingValue();
		}

		public class CurveDrawer : ValueDrawer<float> {
			float minValue, maxValue;
		
			public CurveDrawer(SerializedObject serializedObject, string propName, string displayName, float minValue, float maxValue, float defaultValue, string hint="") {
				InitializeDrawer ( serializedObject, propName, displayName, defaultValue, hint);
				this.minValue = minValue;
				this.maxValue = maxValue;
			}
			protected override void SetValueButtons () {
				if (GUILayout.Button("Set Value", EditorStyles.miniButton)) {
					AnimationCurve c = prop.animationCurveValue;
					Keyframe[] keyframes = c.keys;
					int[] frameIndicies = indiciesPerPhase_CURVE[(int)valuePhase];
					for (int i = 0; i < frameIndicies.Length; i++) {
						keyframes[frameIndicies[i]].value = workingValue;
					}
					c.keys = keyframes;
					prop.animationCurveValue = c;
				}
			}
			protected override void RetreiveValueButtons ( ) {
				if (GUILayout.Button("Retreive Value", EditorStyles.miniButton)) {
					workingValue = prop.animationCurveValue.keys[indiciesPerPhase_CURVE[(int)valuePhase][0]].value;	
				}
			}
			protected override void DrawWorkingValue() {
				workingValue = EditorGUILayout.Slider(workingValue, minValue, maxValue);
			}
		}

		public class GradientDrawer : ValueDrawer<Color> {
			bool useAlpha;
			public GradientDrawer(SerializedObject serializedObject, string propName, string displayName, Color defaultValue, bool useAlpha, string hint="") {
				InitializeDrawer ( serializedObject, propName, displayName, defaultValue, hint);
				this.useAlpha = useAlpha;
			}

			static PropertyInfo GetPropertyInfo (SerializedProperty sp) {
				return sp.GetType().GetProperty("gradientValue", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, typeof(Gradient), new System.Type[0], null);
			}

			static Gradient SafeGradientValue(SerializedProperty sp) {
				PropertyInfo propertyInfo = GetPropertyInfo(sp);
				if (propertyInfo == null) return null;
				return propertyInfo.GetValue(sp, null) as Gradient;
			}
			static void SetGradient(SerializedProperty sp, Gradient gradient) {
				PropertyInfo propertyInfo = GetPropertyInfo(sp);
				if (propertyInfo == null) return;
				propertyInfo.SetValue(sp, gradient);
			}


			Color CopyColorRGB(Color original, Color toCopy) {
				return new Color(toCopy.r, toCopy.g, toCopy.b, original.a);
			}
			Color CopyColorAlpha(Color original, float toCopy) {
				return new Color(original.r, original.g, original.b, toCopy);
			}

			protected override void SetValueButtons () {
				if (GUILayout.Button("Set Color Value", EditorStyles.miniButton)) {
					Gradient workingGradient = SafeGradientValue(prop);
					GradientColorKey[] colorKeys = workingGradient.colorKeys;

					int[] frameIndicies = indiciesPerPhase_GRADIENT[(int)valuePhase];
					for (int i = 0; i < frameIndicies.Length; i++) {
						colorKeys[frameIndicies[i]].color = workingValue;
					}
					workingGradient.colorKeys = colorKeys;
					SetGradient(prop, workingGradient);
				}

				if (useAlpha)
				{
					if (GUILayout.Button("Set Alpha Value", EditorStyles.miniButton)) {
						Gradient workingGradient = SafeGradientValue(prop);
						GradientAlphaKey[] colorKeys = workingGradient.alphaKeys;
						int[] frameIndicies = indiciesPerPhase_GRADIENT[(int)valuePhase];
						for (int i = 0; i < frameIndicies.Length; i++) {
							colorKeys[frameIndicies[i]].alpha = workingValue.a;
						}
						workingGradient.alphaKeys = colorKeys;
						SetGradient(prop, workingGradient);
					}
				}
			}
			protected override void RetreiveValueButtons () {
				if (GUILayout.Button("Retreive Color Value", EditorStyles.miniButton)) {
					workingValue = CopyColorRGB(workingValue, SafeGradientValue(prop).colorKeys[indiciesPerPhase_GRADIENT[(int)valuePhase][0]].color);
				}
				if (useAlpha) {
					if (GUILayout.Button("Retreive Alpha Value", EditorStyles.miniButton)) {
						workingValue = CopyColorAlpha(workingValue, SafeGradientValue(prop).alphaKeys[indiciesPerPhase_GRADIENT[(int)valuePhase][0]].alpha);
					}
				}
			}

			protected override void DrawWorkingValue() {
					EditorGUILayout.BeginHorizontal();
					
					workingValue = EditorGUILayout.ColorField(workingValue);

					/* 
						next section needs to be here to auto update the gradient visual in editor for some reason 
					*/
					GUILayoutOption[] opts = new GUILayoutOption[] { GUILayout.Width(0), GUILayout.Height(0) };
					for (int i = 0; i < 10; i++) {
						//need to retrieve every iteration... dont know why
						Gradient grad = SafeGradientValue(prop);
						EditorGUILayout.GradientField(grad, opts);
					}
					//---------------------------------------------------------

					EditorGUILayout.EndHorizontal();

			}
		}
	}
}