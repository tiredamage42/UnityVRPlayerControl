Shader "Custom Environment/Water" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_Emis("Self-Ilumination", Range(0,1)) = 0.1
	_Smth("Smoothness", Range(0,1)) = 0.9
	_Parallax ("Height", Range (0.005, 0.08)) = 0.02
	_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
	_BumpMap ("Normalmap", 2D) = "bump" {}
	_ParallaxMap ("Heightmap", 2D) = "black" {}
	_ScrollSpeed("Scroll Speed", float) = 0.2
	_WaveFreq("Wave Frequency", float) = 20
	_WaveHeight("Wave Height", float) = 0.1
}

	CGINCLUDE
        #define _GLOSSYENV 1
        #define UNITY_SETUP_BRDF_INPUT MetallicSetup
    ENDCG

SubShader { 
	CGPROGRAM
	#pragma target 3.0
	// #pragma surface surf Standard vertex:vert noshadow nolightmap nodynlightmap nodirlightmap nometa nolppv noshadowmask interpolateview halfasview
	#pragma surface surf Standard vertex:vert nolightmap nodynlightmap nodirlightmap nometa nolppv noshadowmask interpolateview halfasview
	
	#pragma fragmentoption ARB_precision_hint_fastest

	#include "UnityPBSLighting.cginc"
	
	sampler2D _MainTex, _BumpMap, _ParallaxMap;
	
	fixed4 _Color;
	fixed _ScrollSpeed, _WaveFreq, _Smoothness, _WaveHeight;
	fixed _Parallax;
	fixed _Smth, _Emis;
	

struct Input {
	fixed2 uv_MainTex;
	fixed2 uv_BumpMap;
	fixed2 uv_ParallaxMap;
	fixed3 viewDir;
	INTERNAL_DATA	
};

void vert (inout appdata_full v) {
    fixed phase = _Time * _WaveFreq;
    fixed offset = (v.vertex.x + (v.vertex.z * 2)) * 8;
    v.vertex.y = sin(phase + offset) * _WaveHeight; 
}


void surf (Input IN, inout SurfaceOutputStandard o) {

	fixed scrollX = _ScrollSpeed * _Time;
	fixed scrollY = (_ScrollSpeed * _Time) * 0.5;
	
	fixed scrollX2 = (1 - _ScrollSpeed) * _Time;
	fixed scrollY2 = (1 - _ScrollSpeed * _Time) * 0.5;

	IN.uv_ParallaxMap = IN.uv_ParallaxMap + fixed2(scrollX * 0.2, scrollY * 0.2);
	
	fixed h = tex2D (_ParallaxMap, IN.uv_ParallaxMap).r;
	fixed2 offset = ParallaxOffset (h, _Parallax, IN.viewDir);


	IN.uv_MainTex = IN.uv_MainTex + offset + fixed2(scrollX, scrollY);
	
	fixed2 uv1 = IN.uv_BumpMap + offset + fixed2(scrollX, scrollY);
	fixed2 uv2 = IN.uv_BumpMap + offset + fixed2(scrollX2, scrollY2);

	fixed3 nrml = UnpackNormal(tex2D(_BumpMap, uv1));
	fixed3 nrml2 = UnpackNormal(tex2D(_BumpMap, uv2));
	
	fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
	
	fixed3 finalnormal = nrml.rgb + (nrml2.rgb * fixed3(1,1,0));

	o.Albedo = tex * _Color;
	
	o.Smoothness = _Smth;
	o.Metallic = 0;
	o.Emission = tex * _Color * _Emis;
	
	o.Normal = normalize(finalnormal);
}
ENDCG



}

FallBack "Diffuse"
}
