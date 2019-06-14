//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Used for objects that can be seen through objects in front of them
//
//=============================================================================
// UNITY_SHADER_NO_UPGRADE
Shader "Valve/VR/SeeThru"
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
			// Render State ---------------------------------------------------------------------------------------------------------------------------------------------
			// Blend SrcAlpha OneMinusSrcAlpha // Alpha blending
			// Cull Off
			ZWrite Off
			ZTest Greater
			// Stencil
			// {
			// 	Ref 2
			// 	Comp notequal
			// 	Pass replace
			// 	Fail keep
			// }

			CGPROGRAM
				// #pragma target 5.0
				// #pragma only_renderers d3d11 vulkan glcore
				// #pragma exclude_renderers gles

				#pragma vertex MainVS
				#pragma fragment MainPS
				
				// Includes -------------------------------------------------------------------------------------------------------------------------------------------------
				#include "UnityCG.cginc"
				
				// Structs --------------------------------------------------------------------------------------------------------------------------------------------------
				struct VertexInput
				{
					fixed4 vertex : POSITION;
					// fixed2 uv : TEXCOORD0;
				};
				
				struct VertexOutput
				{
					// fixed2 uv : TEXCOORD0;
					fixed4 vertex : SV_POSITION;
				};
				
				// Globals --------------------------------------------------------------------------------------------------------------------------------------------------
				// sampler2D _MainTex;
				// fixed4 _MainTex_ST;
				fixed4 _Color;
				
				// MainVs ---------------------------------------------------------------------------------------------------------------------------------------------------
				VertexOutput MainVS( VertexInput i )
				{
					VertexOutput o;
					o.vertex = UnityObjectToClipPos(i.vertex);
					// o.uv = TRANSFORM_TEX( i.uv, _MainTex );					
					return o;
				}
				
				// MainPs ---------------------------------------------------------------------------------------------------------------------------------------------------
				fixed4 MainPS( VertexOutput i ) : SV_Target
				{
					return _Color;
					// fixed4 vColor = _Color.rgba;
					// return vColor.rgba;
				}

			ENDCG
		}
	}
}
