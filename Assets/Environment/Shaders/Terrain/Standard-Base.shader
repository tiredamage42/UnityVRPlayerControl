Shader "Hidden/TerrainEngine/Splatmap/Standard-Base_Custom" {
    Properties {
        _MainTex ("Base (RGB) Smoothness (A)", 2D) = "white" {}
    }
    SubShader {
        Tags { "RenderType" = "Opaque" }
        Pass {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }
            CGPROGRAM
            #pragma vertex vert_surf
            #pragma fragment frag_surf
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_fog
            #pragma multi_compile_fwdbase
            #pragma target 2.0
        
            #define VERTEX_LIGHTS
            #define NO_ROTATION_SCALE

            #include "../ShaderHelp.cginc"
            sampler2D _MainTex;

            struct v2f_surf {
                UNITY_POSITION(pos);
                float3 uv : TEXCOORD0; // tc
                MY_LIGHTING_COORDS(1, 2, 3, 4, 5, 6)
            };
            v2f_surf vert_surf (appdata_full v) {
                INITIALIZE_FRAGMENT_IN(v2f_surf, o, v)    
                o.uv.xy = v.texcoord.xy;
                o.pos = UnityObjectToClipPos(v.vertex);
                FINISH_VERTEX_CALC(o, v.vertex, v.normal, v.tangent.xyz, v.tangent.w, o.pos, o.uv.z)
                return o;
            }
            fixed4 frag_surf (v2f_surf IN) : SV_Target {
                fixed3 Albedo = tex2D (_MainTex, IN.uv.xy).rgb;
                FINISH_FRAGMENT_CALC(IN, Albedo, 0, IN.uv.z)
            }
            ENDCG
        }
        Pass {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On ZTest LEqual //Cull Off
            CGPROGRAM
            #pragma vertex vertexSHADOW
            #pragma fragment fragSHADOW
            #pragma fragmentoption ARB_precision_hint_fastest   
            #pragma multi_compile_shadowcaster               
            #pragma target 2.0
           
            #define SHADOWCASTER
            #define NO_ROTATION_SCALE
            #include "../ShaderHelp.cginc"
            ENDCG
        } 
    }
}