// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Nature/Tree Creator Bark Optimized Custom" {
    Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
        _BumpSpecMap ("Normalmap (GA) Spec (R)", 2D) = "bump" {}
    }

    SubShader {
    CGPROGRAM

        #pragma surface surf Lambert vertex:TreeVertBark fullforwardshadows nolightmap nodynlightmap nodirlightmap nometa nolppv noshadowmask interpolateview halfasview addshadow
        #pragma fragmentoption ARB_precision_hint_fastest
                    
        // #include "UnityCG.cginc"
        #include "Lighting.cginc"
        #include "TerrainEngine_Custom.cginc"

        fixed4 _Color;

        sampler2D _MainTex;
        sampler2D _BumpSpecMap;
        
        struct Input {
            fixed2 uv_MainTex;
            fixed4 color : COLOR;
        };

        void TreeVertBark (inout appdata_full v)
        {
            v.vertex = AnimateVertex(v.vertex, v.normal, fixed4(v.color.xy, v.texcoord1.xy));
            v.normal = normalize(v.normal);
            v.color.rgb = _Color.rgb;
            // v.tangent.xyz = normalize(v.tangent.xyz);
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
