Shader "Custom Environment/Tree/Tree Creator Bark Optimized" {
    Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
        _BumpSpecMap ("Normalmap (GA) Spec (R)", 2D) = "bump" {}


        [Header(Bark Wind)]
        _Bark_Wind_Speed_Range ("Speed Range", Vector) = (.2, .2, 0, 0)
        _Bark_Wind_Frequency_Range ("Frequency Range", Vector) = (.1, .1, 0, 0)
		_Bark_Wind_Scale_Min ("Scale Min", Vector) = (50, 0, 25, 0)
		_Bark_Wind_Scale_Max ("Scale Max", Vector) = (100, 0, 50, 0)
        _Bark_Wind_Height_Range ("Tree Height Range", Vector) = (25, 25, 0, 0)
		_Bark_Wind_Height_Steepness_Range ("Tree Height Steepness Range", Vector) = (2, 1, 0, 0)
		
    }

    SubShader {
        CGPROGRAM

        #pragma surface surf Lambert vertex:TreeVertBark fullforwardshadows nolightmap nodynlightmap nodirlightmap nometa nolppv noshadowmask interpolateview halfasview addshadow
        #pragma fragmentoption ARB_precision_hint_fastest
                    
        #include "Lighting.cginc"

        #define TREE_WIND
        #include "../CustomWind.cginc"


        fixed4 _Color;

        sampler2D _MainTex;
        sampler2D _BumpSpecMap;
        
        struct Input {
            fixed2 uv_MainTex;
            fixed4 color : COLOR;
        };

        void TreeVertBark (inout appdata_full v)
        {
            v.vertex.xyz = WaveBranch (v.vertex.xyz);
            v.color.rgb = _Color.rgb;
        }

        void surf (Input IN, inout SurfaceOutput o) {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb * IN.color.rgb * IN.color.a;
            o.Alpha = c.a;
            fixed4 norspc = tex2D (_BumpSpecMap, IN.uv_MainTex);
            o.Specular = norspc.r;
            o.Normal = UnpackNormalDXT5nm(norspc);
        }
        ENDCG
    }
}
