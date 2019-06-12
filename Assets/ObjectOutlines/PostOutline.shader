Shader "Custom/Post Outline"{

    Properties {
        _MainTex("Main Texture", 2D)="black"{}
        _SceneTex("Scene Texture", 2D)="black"{}
    }
    SubShader {
        CGINCLUDE
        
        sampler2D _MainTex;

        #pragma vertex vert
        #pragma fragment frag
        #include "UnityCG.cginc"

        static const fixed _Size = 2;
        static const fixed _Intensity = 2;
        static const int _Iterations = 40;
        static const fixed4 _Color = fixed4(1, .5, 0, 1);


        struct v2f {
            float4 pos : SV_POSITION;
            float2 uvs : TEXCOORD0;
        };


        v2f vert (appdata_base v) {
            v2f o;

            o.pos = UnityObjectToClipPos(v.vertex);

            o.uvs = o.pos.xy / 2 + 0.5;

            return o;
        }

        ENDCG

        Pass {
            CGPROGRAM

            // how much screen space a texel occupies
            float2 _MainTex_TexelSize;

            fixed4 frag (v2f i) : COLOR  {
                
                #if UNITY_UV_STARTS_AT_TOP
                if (_MainTex_TexelSize.y > 0) {
                    i.uvs.y = 1 - i.uvs.y;
                }
                #endif

                //add color to pixels that are near red pixels in the original "simple draw" output
                //for every horizontal iteration

                //final color intensity that increments based on surrounding intensity
                float ColorIntensityInRadius = 0;
                for (int k = 0; k < _Iterations; k++) {
                    //increase our output color by the pixels in the area
                    ColorIntensityInRadius += tex2D(_MainTex, i.uvs + float2((k-_Iterations/2) * _MainTex_TexelSize.x * _Size, 0)).r / _Iterations;
                }

                return ColorIntensityInRadius;
            }


            ENDCG
        
        }
        GrabPass {}

        Pass {


            CGPROGRAM
            sampler2D _SceneTex;

            // written to during "grab pass"
            sampler2D _GrabTexture;
            float2 _GrabTexture_TexelSize;


            half4 frag (v2f i) : COLOR {

                


                #if UNITY_UV_STARTS_AT_TOP
                if (_GrabTexture_TexelSize.y > 0) {
                    i.uvs.y = 1 - i.uvs.y;
                }
                #endif

                fixed4 scene = tex2D(_SceneTex, i.uvs);

                // if something already exists underneath the fragment (in the original texture), discard the fragment
                if (tex2D(_MainTex, i.uvs.xy).r > 0) {
                    return scene;
                }
                
                
                //final color intensity that increments based on surrounding intensity
                float ColorIntensityInRadius = 0;
                
                //add color to pixels that are near red pixels in the original "simple draw" output
                //for every vretical iteration
                for (int k = 0; k < _Iterations; k++) {
                    //increase our output color by the pixels in the area
                    ColorIntensityInRadius += tex2D(_GrabTexture, i.uvs + float2(0, 
                        (k-_Iterations/2) * _GrabTexture_TexelSize.y * _Size)
                    ).r / _Iterations;
                    
                }

                // this is alpha blending, but we cant use hw blending unless we make a third pass
                // s this is probably cheaper
                half4 outcolor = ColorIntensityInRadius * _Color * _Intensity + (1-ColorIntensityInRadius) * scene;
                return outcolor;
            }
            ENDCG
        }
    }       
}