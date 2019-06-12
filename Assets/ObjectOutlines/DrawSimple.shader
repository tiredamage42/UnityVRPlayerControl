// This shader goes on the objects themselves. It just draws the object as white, 
// and has the "Outline" tag.
 
Shader "Custom/DrawSimple"
{
    SubShader 
    {
        ZWrite Off ZTest Always Lighting Off
        Pass
        {
            CGPROGRAM
            #pragma vertex VShader
            #pragma fragment FShader
 
            struct VertexToFragment { float4 pos : POSITION; };
 
            //just get the position correct
            VertexToFragment VShader(VertexToFragment i)
            {
                VertexToFragment o;
                o.pos = UnityObjectToClipPos(i.pos);
                return o;
            }
            //return white
            fixed4 FShader() : COLOR
            {
                return fixed4(1,1,1,1);
            }
            ENDCG
        }
    }
}
