Shader "Hidden/Tagged"
{
	Properties
	{
		_Color( "Color", Color ) = ( 1, 1, 1, 1 )
	}
	SubShader
	{
		Tags{ "Queue" = "Geometry+1" "RenderType" = "Transparent" }
		LOD 100
		Pass
		{
			Lighting Off
			ZWrite Off
			ZTest Greater
			CGPROGRAM
			#pragma vertex MainVS
			#pragma fragment MainPS
				
			#include "UnityCG.cginc"
				
			fixed4 _Color;
			fixed4 MainVS( fixed4 vertex : POSITION ) : SV_POSITION
			{
				return UnityObjectToClipPos(vertex);
			}
			fixed4 MainPS( ) : SV_Target
			{
				return _Color;
			}
			ENDCG
		}
	}
}
