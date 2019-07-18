



using UnityEngine;
using UnityEditor;
using System.Reflection;
namespace AC.LSky
{

	[CustomEditor(typeof(LSky))] 
	public class LSkyEditor : Editor
	{
		SerializedObject  serObj;
		LSky lsky;
		SerializedProperty skyboxMaterial;
		SerializedProperty moonTexture;
		SerializedProperty outerSpaceCube;
		SerializedProperty outerSpaceOffset;
		SerializedProperty HDR;
		SerializedProperty sunLightThreshold;
		SerializedProperty ambientMode;
		SerializedProperty unityFogMode;
		SerializedProperty cloudsTexture0, cloudsTexture1, magneticFieldTexture;
		public SerializedProperty rainTexture, snowTexture, groundFogTexture, precipitationNoiseMap;
		// public SerializedProperty rainPrecipitator, snowPrecipitator;
		// public SerializedProperty groundFogPrecipitator;
		// public SerializedProperty windTransform;
		SerializedProperty targetWeather, weatherSwitch;
		
		public void OnEnable()
		{
			lsky = target as LSky;

			serObj  = serializedObject;

			rainTexture = serObj.FindProperty("rainTexture");
			snowTexture = serObj.FindProperty("snowTexture");
			groundFogTexture = serObj.FindProperty("groundFogTexture");
			precipitationNoiseMap = serObj.FindProperty("precipitationNoiseMap");

			// windTransform = serObj.FindProperty("windTransform");

			// rainPrecipitator = serObj.FindProperty("rainPrecipitator");
			// snowPrecipitator = serObj.FindProperty("snowPrecipitator");
			// groundFogPrecipitator = serObj.FindProperty("groundFogPrecipitator");

		
			skyboxMaterial = serObj.FindProperty("skyboxMaterial");
			moonTexture    = serObj.FindProperty("moonTexture");
			outerSpaceCube = serObj.FindProperty("outerSpaceCube");
		
			outerSpaceOffset         = serObj.FindProperty("outerSpaceOffset");
			HDR         = serObj.FindProperty("HDR");
			sunLightThreshold   = serObj.FindProperty("sunLightThreshold");
			ambientMode	        = serObj.FindProperty("ambientMode");
			unityFogMode	       = serObj.FindProperty("unityFogMode");

			cloudsTexture0 = serObj.FindProperty("clouds0texture");
			cloudsTexture1 = serObj.FindProperty("clouds1texture");
			magneticFieldTexture = serObj.FindProperty("magneticFieldTexture");

			targetWeather = serializedObject.FindProperty("targetWeather");
			weatherSwitch = serializedObject.FindProperty("weatherSwitch");		
		}
		public WeatherDrawer weatherDrawer = new WeatherDrawer();

		public void OnWeatherChange (Weather newWeather) {
			weatherDrawer.OnEnable(newWeather != null ? new SerializedObject(newWeather) : null);
			
			lsky.SwitchWeather(newWeather, 1);
			lastWeather = newWeather;
		}

		Weather lastWeather; 

		static bool showWorldSettings = true, showTargetWeather = true;
		public override void OnInspectorGUI()
		{
			// base.OnInspectorGUI();

			serObj.Update ();
			
			EditorGUILayout.Separator();

			GUILayoutOption headerHeight = GUILayout.Height(25);

			GUIStyle style = new GUIStyle("ShurikenModuleTitle")
			{
				font          = new GUIStyle("Label").font,
				border        = new RectOffset(15, 7, 4, 4),
				fixedHeight   = 22,
				contentOffset = new Vector2(20f, -2f)
			}; 
			style.fontStyle = FontStyle.Bold;

			GUI.enabled = false;
			EditorGUILayout.ObjectField("Last Weather", lsky.oldWeather, typeof(Weather), false);//, "Last Weather");
			GUI.enabled = true;

			EditorGUILayout.PropertyField(targetWeather, new GUIContent("TargetWeather"));
			EditorGUILayout.PropertyField(weatherSwitch, new GUIContent("weatherSwitch"));
			
			Weather currentWeather = (Weather)targetWeather.objectReferenceValue;

			if (currentWeather != lastWeather) {
				OnWeatherChange(currentWeather);
			}
			
			if (SkyEditor.ShurikenFoldoutHeader("World", style, headerHeight, ref showWorldSettings))
			{
				SkyEditor.StartIndent();
				World();
				SkyEditor.EndIndent(); 
			}
			if (SkyEditor.ShurikenFoldoutHeader("Target Weather", style, headerHeight, ref showTargetWeather))
			{
				SkyEditor.StartIndent();
				weatherDrawer.OnInspectorGUI();
				SkyEditor.EndIndent(); 
			}

			serObj.ApplyModifiedProperties();
		}
		void World () {
			
			EditorGUILayout.PropertyField(skyboxMaterial, new GUIContent("Skybox Material")); 
			
			EditorGUILayout.PropertyField(cloudsTexture0, new GUIContent("Clouds 0 Texture"));
			EditorGUILayout.PropertyField(cloudsTexture1, new GUIContent("Clouds 1 Texture"));
			
			EditorGUILayout.PropertyField(magneticFieldTexture, new GUIContent("Magnetic Field Texture"));
			
			EditorGUILayout.PropertyField(moonTexture, new GUIContent("Moon Texture")); 
			
			EditorGUILayout.PropertyField (outerSpaceCube, new GUIContent ("Outer Space Cube")); 
			EditorGUILayout.HelpBox ("RGB = Nebula, A = StarField", MessageType.Info);
			EditorGUILayout.PropertyField(outerSpaceOffset, new GUIContent("Space Offsets")); 
			EditorGUILayout.PropertyField(serObj.FindProperty("spaceParallaxScrollModifier"));
			
			EditorGUILayout.PropertyField(sunLightThreshold, new GUIContent("Light Intensity Threshold")); 
			
			EditorGUILayout.PropertyField(unityFogMode, new GUIContent ("Unity Fog Mode")); 
			EditorGUILayout.PropertyField(ambientMode, new GUIContent("Ambient Mode")); 
			EditorGUILayout.PropertyField(HDR, new GUIContent ("HDR")); 

			// EditorGUILayout.PropertyField(windTransform, new GUIContent("Wind Transform")); 
			
			EditorGUILayout.PropertyField(precipitationNoiseMap, new GUIContent("Precipitation Noise Map")); 
			EditorGUILayout.PropertyField(rainTexture, new GUIContent("Rain Texture")); 
			EditorGUILayout.PropertyField(snowTexture, new GUIContent("Snow Texture")); 
			EditorGUILayout.PropertyField(groundFogTexture, new GUIContent("Ground Fog Texture")); 
			
			// EditorGUILayout.PropertyField(rainPrecipitator, new GUIContent("Rain Precipitator")); 
			// EditorGUILayout.PropertyField(snowPrecipitator, new GUIContent("Snow Precipitator")); 
			// EditorGUILayout.PropertyField(groundFogPrecipitator, new GUIContent("Ground Fog Precipitator")); 

			EditorGUILayout.Separator();
		}
	}
}