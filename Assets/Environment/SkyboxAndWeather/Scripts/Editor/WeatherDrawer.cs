using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Reflection;





using UnityEditor;


[CustomEditor(typeof(Weather))]
public class WeatherEditor : Editor {

	WeatherDrawer weatherDrawer = new WeatherDrawer();
	void OnEnable () {
		weatherDrawer.OnEnable(serializedObject);
	}
	public override void OnInspectorGUI () {
		weatherDrawer.OnInspectorGUI();
	}
}

public class WeatherDrawer 
{


    

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
				
				this.colorNoiseTileAndShiftSpeed = new TileAndShiftSpeedParamsDrawer(serializedObject, relativePath + ".colorNoiseTileAndShiftSpeed", "Color Noise Tile And Shift Speed", new Vector4(.15f, .15f, .01f, .01f));
				this.colorNoiseSampleParams = new NoiseSampleParamsDrawer(serializedObject, relativePath + ".colorNoiseSampleParams", "Color Noise Params", new Vector3(2, 1, 2));
				this.tileAndShiftSpeed = new TileAndShiftSpeedParamsDrawer(serializedObject, relativePath + ".tileAndShiftSpeed", "Tile And Shift Speed", new Vector4(.25f, .3f, .001f, .002f));
				this.noiseSampleParams = new NoiseSampleParamsDrawer(serializedObject, relativePath + ".noiseSampleParams", "Noise Params", new Vector3(.75f, 3, 1));
				
				this.swimSizeX = new CurveDrawer(serializedObject, relativePath + ".swimSizeX", "Swim Size X", .01f);
				this.swimSizeY = new CurveDrawer(serializedObject, relativePath + ".swimSizeY", "Swim Size Y", .025f);
				this.heightFadeSteepness = new CurveDrawer(serializedObject, relativePath + ".heightFadeSteepness", "Height Fade Steepness", .1f);
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

    
    
		public class CloudInfoDrawer {

			public GradientDrawer color;
			public NoiseSampleParamsDrawer noiseSampleParams;
			public TileAndShiftSpeedParamsDrawer tileAndShiftSpeed;
			
			public CloudInfoDrawer (SerializedObject serializedObject, string relativePath) {
				color = new GradientDrawer(serializedObject, relativePath + ".color", "Color", Color.white, true);
				noiseSampleParams = new NoiseSampleParamsDrawer(serializedObject, relativePath + ".noiseSampleParams", "Noise Params", Vector3.one);
				tileAndShiftSpeed = new TileAndShiftSpeedParamsDrawer(serializedObject, relativePath + ".tileAndShiftSpeed", "Tile And Shift Speed", new Vector4(.1f, .1f, .1f, .1f));
			}

			public void DrawProp() {
				color.DrawProp();
				noiseSampleParams.DrawProp();
				tileAndShiftSpeed.DrawProp();
			}
		}

		public class GroundFogValuesDrawer {
			public EnvironmentParticleValuesDrawer baseValues;
			public CurveDrawer rotateSpeed;// = GetTODCurve(.1f, false);
			public CurveDrawer softParticleFactor;// = GetTODCurve(1f, false);
			public CurveDrawer startEndFadeRange;// = GetTODCurve(2f, false);
			public Vector2CurveDrawer closeCamRange;// = new Vector2Curve(new Vector2(1, 4), false);
			public Vector2CurveDrawer heightFadeRange;// = new Vector2Curve(new Vector2(0, 2), false);
			public CurveDrawer heightFadeSteepness;// = GetTODCurve(2f, false);

			public void DrawProp () {
				baseValues.DrawProp();
				rotateSpeed.DrawProp();
				
				softParticleFactor.DrawProp();
				startEndFadeRange.DrawProp();
				closeCamRange.DrawProp();
				heightFadeRange.DrawProp();
				heightFadeSteepness.DrawProp();
			}

			public GroundFogValuesDrawer (SerializedObject so, string relativePath) {
				baseValues = new EnvironmentParticleValuesDrawer(so, relativePath + ".baseValues",
					Color.white, Color.white, .2f, 
					new Vector2(1,1), new Vector2(0,10),
					new Vector2(.1f, .1f), new Vector2(.1f, .1f), new Vector2(.25f, .35f), 
					new Vector2(.5f, 1.5f)
					
				);
				rotateSpeed = new CurveDrawer(so, relativePath + ".rotateSpeed", "Rotate Speed", .1f);
				softParticleFactor = new CurveDrawer(so, relativePath + ".softParticleFactor", "Soft Particles Factor", 1f);
				startEndFadeRange = new CurveDrawer(so, relativePath + ".startEndFadeRange", "Start End Fade Range", 2f);
				closeCamRange = new Vector2CurveDrawer(so, relativePath + ".closeCamRange", "Close Cam Range", new Vector2(1, 4));
				heightFadeRange = new Vector2CurveDrawer(so, relativePath + ".heightFadeRange", "Height Fade Range", new Vector2(0, 2));
				heightFadeSteepness = new CurveDrawer(so, relativePath + ".heightFadeSteepness", "Height Fade Steepness", 2f);
			}
		}
		[System.Serializable] public class PrecipitatorValuesDrawer{
			public void DrawProp () {
				baseValues.DrawProp();
				maxTravelDistance.DrawProp();
			}
			public EnvironmentParticleValuesDrawer baseValues;
			public CurveDrawer maxTravelDistance;

			public PrecipitatorValuesDrawer (SerializedObject so, string relativePath, Color color, Color hueVariation, float moveSpeed, Vector2 quadSize, Vector2 cameraRange, Vector2 flutterFrequency, Vector2 flutterSpeed, Vector2 flutterMagnitude, Vector2 sizeRange, float maxTravelDistance) {
				this.maxTravelDistance = new CurveDrawer(so, relativePath + ".maxTravelDistance", "Max Travel Distance", maxTravelDistance);
				this.baseValues = new EnvironmentParticleValuesDrawer(
					so, relativePath + ".baseValues",
					color, hueVariation, moveSpeed, quadSize, cameraRange, flutterFrequency, flutterMagnitude, flutterSpeed, sizeRange
				);
			}
		}
    	
		public class EnvironmentParticleValuesDrawer {
			public void DrawProp () {
				amount.DrawProp();
				color.DrawProp();
				hueVariation.DrawProp();
				moveSpeed.DrawProp();
				quadSize.DrawProp();
				cameraRange.DrawProp();
				flutterParams.DrawProp();
				// flutterFrequency.DrawProp();
				// flutterSpeed.DrawProp();
				// flutterMagnitude.DrawProp();
				sizeRange.DrawProp();
			}

			public CurveDrawerRanged amount;
			public GradientDrawer color, hueVariation;
			public CurveDrawer moveSpeed;
			public Vector2CurveDrawer quadSize, cameraRange, sizeRange;// = new Vector2Curve(1,1, false);
			// public Vector2CurveDrawer quadSize, cameraRange, flutterFrequency, flutterSpeed, flutterMagnitude, sizeRange;// = new Vector2Curve(1,1, false);
			public FlutterParamsDrawer flutterParams;
			public EnvironmentParticleValuesDrawer (SerializedObject so, string relativePath, Color color, Color hueVariation, float moveSpeed, Vector2 quadSize, Vector2 cameraRange, Vector2 flutterFrequency, Vector2 flutterSpeed, Vector2 flutterMagnitude, Vector2 sizeRange) {
				this.amount = new CurveDrawerRanged(so, relativePath + ".amount", "Amount", 0, 1, 0);
				this.color = new GradientDrawer(so, relativePath + ".color", "Color", color, true);
				this.hueVariation = new GradientDrawer(so, relativePath + ".hueVariation", "Hue Variation", hueVariation, true);
				this.moveSpeed = new CurveDrawer(so, relativePath + ".moveSpeed", "Move Speed", moveSpeed);
				this.quadSize = new Vector2CurveDrawer(so, relativePath + ".quadSize", "Quad Size", quadSize);
				this.cameraRange = new Vector2CurveDrawer(so, relativePath + ".cameraRange", "Camera Range", cameraRange);
				
				flutterParams = new FlutterParamsDrawer(so, relativePath, flutterFrequency, flutterSpeed, flutterMagnitude);
				// this.flutterFrequency = new Vector2CurveDrawer(so, relativePath + ".flutterFrequency", "Flutter Frequency", flutterFrequency);
				// this.flutterSpeed = new Vector2CurveDrawer(so, relativePath + ".flutterSpeed", "Flutter Speed", flutterSpeed);
				// this.flutterMagnitude = new Vector2CurveDrawer(so, relativePath + ".flutterMagnitude", "Flutter Magnitude", flutterMagnitude);
				this.sizeRange = new Vector2CurveDrawer(so, relativePath + ".sizeRange", "Size Range", sizeRange);
			}
		}

		


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
					SkyEditor.StartIndent();
					// EditorGUI.indentLevel+=2;
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
					SkyEditor.EndIndent();
					// EditorGUI.indentLevel-=2;
				}
				SkyEditor.Separator(SkyEditor.SeparatorHeight);
			}

			protected abstract void DrawWorkingValue();
		}

		public class FlutterParamsDrawer {
			public Vector2CurveDrawer flutterFrequency, flutterSpeed, flutterMagnitude;
			bool foldout = false;
			string gui;
			

			public FlutterParamsDrawer (SerializedObject so, string relativePath, Vector2 flutterFrequency, Vector2 flutterSpeed, Vector2 flutterMagnitude) {
				gui = "Flutter";
				this.flutterFrequency = new Vector2CurveDrawer(so, relativePath + ".flutterFrequency", "Frequency", flutterFrequency);
				this.flutterSpeed = new Vector2CurveDrawer(so, relativePath + ".flutterSpeed", "Speed", flutterSpeed);
				this.flutterMagnitude = new Vector2CurveDrawer(so, relativePath + ".flutterMagnitude", "Magnitude", flutterMagnitude);
			}

			public void DrawProp() {
				foldout = GUILayout.Toggle(foldout, new GUIContent(gui), EditorStyles.foldout, GUILayout.Width(25));
				if (foldout) {

					SkyEditor.StartIndent();
					// EditorGUI.indentLevel+=2;

					flutterFrequency.DrawProp();
					flutterSpeed.DrawProp();
					flutterMagnitude.DrawProp();
					



					SkyEditor.EndIndent();
					
					// EditorGUI.indentLevel-=2;
				}
				SkyEditor.Separator(SkyEditor.SeparatorHeight);				
			}
				
		}
		
		public class Vector2CurveDrawer {
			public CurveDrawer x, y;
			bool foldout = false;
			string gui;
			

			public Vector2CurveDrawer (SerializedObject so, string relativePath, string prefix, Vector2 defaultValue) {
				gui = prefix;
				this.x = new CurveDrawer(so, relativePath + ".x", "X", defaultValue.x);
				this.y = new CurveDrawer(so, relativePath + ".y", "Y", defaultValue.y);	
			}

			public void DrawProp() {
				foldout = GUILayout.Toggle(foldout, new GUIContent(gui), EditorStyles.foldout, GUILayout.Width(25));
				if (foldout) {

					SkyEditor.StartIndent();
					// EditorGUI.indentLevel+=2;
				
					x.DrawProp();
					y.DrawProp();

					SkyEditor.EndIndent();
					
					// EditorGUI.indentLevel-=2;
				}
				SkyEditor.Separator(SkyEditor.SeparatorHeight);				
			}
		}
		public class NoiseSampleParamsDrawer {

			public CurveDrawer prePower, steepness, postPower;
			bool foldout = false;
			string gui;

			public NoiseSampleParamsDrawer(SerializedObject serializedObject, string relativePath, string prefix, Vector3 defaultValues) {
				gui = prefix;
				
				prePower = new CurveDrawer(serializedObject, relativePath + ".prePower", "Pre Power", defaultValues.x);
				steepness = new CurveDrawer(serializedObject, relativePath + ".steepness", "Steepness", defaultValues.y);
				postPower = new CurveDrawer(serializedObject, relativePath + ".postPower", "Post Power", defaultValues.z);
			}
			public void DrawProp () {
				foldout = GUILayout.Toggle(foldout, new GUIContent(gui), EditorStyles.foldout, GUILayout.Width(25));
				if (foldout) {

					SkyEditor.StartIndent();
				
				prePower.DrawProp();
				steepness.DrawProp();
				postPower.DrawProp();
						SkyEditor.EndIndent();
					
					// EditorGUI.indentLevel-=2;
				}
				SkyEditor.Separator(SkyEditor.SeparatorHeight);				
			
			}
		}

		public class TileAndShiftSpeedParamsDrawer {
			public CurveDrawer tileX, tileY, shiftSpeedX, shiftSpeedY;
			bool foldout = false;
			string gui;
			public TileAndShiftSpeedParamsDrawer(SerializedObject serializedObject, string relativePath, string prefix, Vector4 defaultValues) {
				gui = prefix;
				
				tileX = new CurveDrawer(serializedObject, relativePath + ".tileX", "Tile X", defaultValues.x);
				tileY = new CurveDrawer(serializedObject, relativePath + ".tileY", "Tile Y", defaultValues.y);
				shiftSpeedX = new CurveDrawer(serializedObject, relativePath + ".shiftSpeedX", "Shift Speed X", defaultValues.z);
				shiftSpeedY = new CurveDrawer(serializedObject, relativePath + ".shiftSpeedY", "Shift Speed Y", defaultValues.w);
			}
			public void DrawProp () {
				foldout = GUILayout.Toggle(foldout, new GUIContent(gui), EditorStyles.foldout, GUILayout.Width(25));
				if (foldout) {

					SkyEditor.StartIndent();
				
				tileX.DrawProp();
				tileY.DrawProp();
				shiftSpeedX.DrawProp();
				shiftSpeedY.DrawProp();
						SkyEditor.EndIndent();
					
					// EditorGUI.indentLevel-=2;
				}
				SkyEditor.Separator(SkyEditor.SeparatorHeight);				
			
			}	
		}
		

		public class CurveDrawer : ValueDrawer<float> {
			public CurveDrawer(SerializedObject serializedObject, string propName, string displayName, float defaultValue, string hint="") {
				InitializeDrawer ( serializedObject, propName, displayName, defaultValue, hint);
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
				workingValue = EditorGUILayout.FloatField(workingValue);
			}
		}
		public class CurveDrawerRanged : CurveDrawer {
			float minValue, maxValue;
		
			public CurveDrawerRanged(SerializedObject serializedObject, string propName, string displayName, float minValue, float maxValue, float defaultValue, string hint="") :
				base(serializedObject, propName, displayName, defaultValue, hint) {
				// InitializeDrawer ( serializedObject, propName, displayName, defaultValue, hint);
				this.minValue = minValue;
				this.maxValue = maxValue;
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

    public GroundFogValuesDrawer groundFog;
		public PrecipitatorValuesDrawer rain;
		public PrecipitatorValuesDrawer snow;
		
    
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
		GradientDrawer starsColor;
		CurveDrawer starsScintillation;
		CurveDrawer starsScintillationSpeed;

		GradientDrawer nebulaColor;
		
    CurveDrawer exposure;
		
		GradientDrawer sunLightColor_Intensity;
		SerializedProperty sunLightMaxIntensity;
		GradientDrawer moonLightColor_Intensity;
		SerializedProperty moonLightMaxIntensity;
		

    GradientDrawer ambientSkyColor;
		GradientDrawer ambientEquatorColor;
		GradientDrawer ambientGroundColor;
		CurveDrawer ambientIntensity;
    GradientDrawer unityFogColor;
		CurveDrawer unityFogDensity;
		CurveDrawer unityFogStartDistance;
		CurveDrawer unityFogEndDistance;

		CurveDrawer nebulaExponent;
		CurveDrawer groundAltitude;
		CurveDrawer groundFade;
		CurveDrawer horizonFade;


        SerializedObject serializedObject;

        

		void InitializePrecipitationProps (SerializedObject so) {
// 			rainTexture = so.FindProperty("rainTexture");
// 			snowTexture = so.FindProperty("snowTexture");
// 			groundFogTexture = so.FindProperty("groundFogTexture");
// 			precipitationNoiseMap = so.FindProperty("precipitationNoiseMap");

// 			windTransform = so.FindProperty("windTransform");


// rainPrecipitator = so.FindProperty("rainPrecipitator");
// snowPrecipitator = so.FindProperty("snowPrecipitator");
// groundFogPrecipitator = so.FindProperty("groundFogPrecipitator");





			windStrength = new CurveDrawerRanged(so, "windStrength", "Wind Strength", 0, 1, 0);


			groundFog = new GroundFogValuesDrawer(so, "groundFog");

			rain = new PrecipitatorValuesDrawer(so, "rain", 
			Color.white, Color.white, -5,  // color, hue var, move speed
			
			new Vector2(.0025f, .25f), //quad size

			new Vector2(0,15), //camera range
			
			new Vector2(0.988f, 1.234f), //flutter frequency
			new Vector2(.01f, .01f), //flutter speed
			new Vector2(.35f, .25f), //flutter magnitude
			new Vector2(.5f, 1f), //size range 
			15 //travel distance
			
			);

		snow = new PrecipitatorValuesDrawer(so, "snow",
			Color.white, Color.white, -.25f,  // color, hue var, move speed
			
			new Vector2(.5f, .5f), //quad size

			new Vector2(0,10), //camera range
			
			new Vector2(0.988f, 1.234f), //flutter frequency
			new Vector2(1f, .5f), //flutter speed
			new Vector2(.35f, .25f), //flutter magnitude
			new Vector2(.05f, .025f), //size range 
			10 //travel distance
			
			);
		}
				public CurveDrawerRanged windStrength;

public MagneticFieldValuesDrawer magneticField;
CloudInfoDrawer clouds0, clouds1;
		


        public void OnEnable (SerializedObject newWeather) {
            serializedObject = newWeather;
			if (newWeather == null)
			{
				return;
			}

            			InitializePrecipitationProps(serializedObject);



                        sunLightMaxIntensity	= serializedObject.FindProperty("sunLightMaxIntensity");
			moonLightMaxIntensity	= serializedObject.FindProperty("moonLightMaxIntensity");
			


            
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

			unityFogEndDistance = new CurveDrawerRanged(serializedObject, "unityFogEndDistance", "End Distance", 0, 1000, 300, "Fog Linear");
			unityFogStartDistance = new CurveDrawerRanged(serializedObject, "unityFogStartDistance", "Start Distance", 0, 1000, 0, "Fog Linear");
			unityFogDensity = new CurveDrawerRanged(serializedObject, "unityFogDensity", "Density", 0, 2, 0.01f, "Fog Exponential");
			
			ambientIntensity = new CurveDrawerRanged(serializedObject, "ambientIntensity", "Intensity", 0, 8, 1f, "Skybox Ambient");
			starsScintillationSpeed = new CurveDrawerRanged(serializedObject, "starsScintillationSpeed", "Twinkle Speed", 0, 5, 1f);
			starsScintillation = new CurveDrawerRanged(serializedObject, "starsScintillation", "Twinkle Intensity", 0, 1, .9f);
			moonSize = new CurveDrawerRanged(serializedObject, "moonSize", "Size", 0, 5, .3f);
			sunDiscSize = new CurveDrawerRanged(serializedObject, "sunDiscSize", "Mie Size", 0, 25, 5f);
			moonMieAnisotropy = new CurveDrawerRanged(serializedObject, "moonMieAnisotropy", "Mie Anisotropy", 0, .98f, .93f);
			moonMieScattering = new CurveDrawerRanged(serializedObject, "moonMieScattering", "Mie Scattering", 00001f, 25f, .5f);
			sunMieAnisotropy = new CurveDrawerRanged(serializedObject, "sunMieAnisotropy", "Mie Anisotropy", 0, .98f, .75f);
			sunMieScattering = new CurveDrawerRanged(serializedObject, "sunMieScattering", "Mie Scattering", 00001f, 25f, .5f);
			mie = new CurveDrawerRanged(serializedObject, "mie", "Sun Mie", 0, 5f, .01f);
			sunBrightness = new CurveDrawerRanged(serializedObject, "sunBrightness", "Brightness", 0, 100f, 30f);
			atmosphereThickness = new CurveDrawerRanged(serializedObject, "atmosphereThickness", "Atmosphere Thickness", 0, 50, 1);
			exposure = new CurveDrawerRanged(serializedObject, "exposure", "Exposure", 0, 10, 1.3f);
			nebulaExponent = new CurveDrawerRanged(serializedObject, "nebulaExponent", "Outer Space Steepness", 0.01f, 10, 2f);
			groundAltitude = new CurveDrawerRanged(serializedObject, "groundAltitude", "Ground Altitude", -1f, 1, 0f);
			groundFade = new CurveDrawerRanged(serializedObject, "groundFade", "Ground Fade", 0.0f, 60, 30f);
			horizonFade = new CurveDrawerRanged(serializedObject, "horizonFade", "Horizon Fade", -.5f, .5f, .006f);
		


        }


        static bool[] showHeaders = new bool[16];



        
    public static bool ShurikenFoldoutHeader(string text, GUIStyle texStyle, GUILayoutOption headerHeight, int foldoutIndex)
		{
            showHeaders[foldoutIndex] = SkyEditor.ShurikenFoldoutHeader(text, texStyle, headerHeight, ref showHeaders[foldoutIndex]);
			return showHeaders[foldoutIndex];
		}




		public void OnInspectorGUI()
		{
			if (serializedObject == null) {

				EditorGUILayout.HelpBox("No Weather to draw...", MessageType.Info);
				return;
			}



			serializedObject.Update ();
			
			EditorGUILayout.Separator();

			// GUIStyle style = new GUIStyle(EditorStyles.label); 

			GUILayoutOption headerHeight = GUILayout.Height(25);

			GUIStyle style = new GUIStyle("ShurikenModuleTitle")
			{
				font          = new GUIStyle("Label").font,
				border        = new RectOffset(15, 7, 4, 4),
				fixedHeight   = 22,
				contentOffset = new Vector2(20f, -2f)
			}; 
			style.fontStyle = FontStyle.Bold;
			
			if (ShurikenFoldoutHeader("Atmosphere", style, headerHeight, 1))
				{
			SkyEditor.	StartIndent();
				
				Atmosphere();
				SkyEditor.EndIndent(); 
			}
			
			if (ShurikenFoldoutHeader("Sun", style, headerHeight, 2))
			{
				SkyEditor.StartIndent();
				
				Sun();
				SkyEditor.EndIndent(); 
			}
			
			if (ShurikenFoldoutHeader("Moon", style, headerHeight, 3))
			{
				SkyEditor.StartIndent();
				
				Moon();
				SkyEditor.EndIndent(); 
			}
			
			if (ShurikenFoldoutHeader("Stars", style, headerHeight, 4))
			{
				SkyEditor.StartIndent();
				
				Stars();
				SkyEditor.EndIndent(); 
			}
			
			if (ShurikenFoldoutHeader("Lights", style, headerHeight, 5))
			{
				SkyEditor.StartIndent();
				
				Lighting();
				SkyEditor.EndIndent(); 
			}
			
			if (ShurikenFoldoutHeader("Ambient Lighting", style, headerHeight, 6))
			{
				SkyEditor.StartIndent();
				
				AmbientLighting();
	SkyEditor.EndIndent(); 
			}
			
			if (ShurikenFoldoutHeader("Fog", style, headerHeight, 7))
			{
				SkyEditor.StartIndent();
				
				Fog();
				SkyEditor.EndIndent(); 
			}
			
			if (ShurikenFoldoutHeader("Clouds 0", style, headerHeight, 8))
			{
				SkyEditor.StartIndent();
				
				clouds0.DrawProp();
				SkyEditor.EndIndent(); 
			}
			
			if (ShurikenFoldoutHeader("Clouds 1", style, headerHeight, 9))
			{
				SkyEditor.StartIndent();
				
				clouds1.DrawProp();
				SkyEditor.EndIndent(); 
			}
			
			if (ShurikenFoldoutHeader("Magnetic Field", style, headerHeight, 10))
			{
				SkyEditor.StartIndent();
				
				magneticField.DrawProp();
					SkyEditor.EndIndent(); 
			}
			

			if (ShurikenFoldoutHeader("Wind", style, headerHeight, 11))
			{
				SkyEditor.StartIndent();
				
				
				windStrength.DrawProp();
					SkyEditor.EndIndent(); 
			}
			
			if (ShurikenFoldoutHeader("Rain", style, headerHeight, 12))
			{
				SkyEditor.StartIndent();
				
				rain.DrawProp();
					SkyEditor.EndIndent(); 
			}
			
			
			if (ShurikenFoldoutHeader("Snow", style, headerHeight, 13))
			{
				SkyEditor.StartIndent();
				
				snow.DrawProp();
					SkyEditor.EndIndent(); 
			}
			
			
			if (ShurikenFoldoutHeader("Ground Fog", style, headerHeight, 14))
			{
				SkyEditor.StartIndent();
				
				groundFog.DrawProp();
				SkyEditor.EndIndent(); 
			}
			

			serializedObject.ApplyModifiedProperties();
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
			SkyEditor.Separator(SkyEditor.SeparatorHeight);

			moonLightColor_Intensity.DrawProp();
			EditorGUILayout.PropertyField(moonLightMaxIntensity);
			SkyEditor.Separator(SkyEditor.SeparatorHeight);
		}



}
