Shader "Custom/DrawSimple" {
    SubShader {			
        Lighting Off
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            fixed4 _OutColor;
            float4 vert(float4 v:POSITION) : POSITION { return UnityObjectToClipPos (v); }
            fixed4 frag() : COLOR { return _OutColor; }
            ENDCG
        }
    }
}
