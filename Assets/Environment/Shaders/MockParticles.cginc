// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

#ifndef MOCK_PARTICLES_INCLUDED
#define MOCK_PARTICLES_INCLUDED


#if defined (MOVE_COMPONENT) && defined (SECONDARY_COMPONENT_0) && defined (SECONDARY_COMPONENT_1)

#include "UnityCG.cginc"

// VERTEX
struct VertexBuffer
{
    fixed4 vertex : POSITION;
    fixed4 uv : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

VertexBuffer vert(VertexBuffer v) {
    return v; 
}

#define PARTICLE_AMOUNT_LEVELS 4.0

fixed2 _CameraRange;

sampler2D _MainTex;



fixed4 _Color;
fixed2 _SizeRange;

fixed4x4 _RotationMatrix;

fixed2 _QuadDimensions;

sampler2D _NoiseMap;
fixed _MaxTravelDistance;

fixed _MoveSpeed;


fixed2 _FlutterFrequency; //0.988, 1.234
fixed2 _FlutterSpeed; // 2.0, 2.5
fixed2 _FlutterMagnitude; // .35, .25

void AddFallFlutter (inout fixed3 vertex, fixed noise0, fixed noise1) {
    vertex.SECONDARY_COMPONENT_0 += sin(vertex.MOVE_COMPONENT * noise1 * _FlutterFrequency.x + _Time.y * (_FlutterSpeed.x +(_FlutterSpeed.x * noise0)) ) * _FlutterMagnitude.x;
    vertex.SECONDARY_COMPONENT_1 += cos(vertex.MOVE_COMPONENT * noise0 * _FlutterFrequency.y + _Time.y * (_FlutterSpeed.y +(_FlutterSpeed.y * noise1)) ) * _FlutterMagnitude.y;
}




fixed _ParticlesAmount;


bool VertexBelowThreshold (fixed3 vertex, fixed2 uv, fixed vertexThreshold, out fixed noiseSample1, out fixed noiseSample2, out fixed particlesAmountFade) {
    noiseSample1 = frac(tex2Dlod(_NoiseMap, fixed4(uv, 0, 0)).r + (vertex.x + vertex.y + vertex.z));
    noiseSample2 = frac(tex2Dlod(_NoiseMap, fixed4(uv.yx * 2, 0, 0)).r + (vertex.x * vertex.y * vertex.z));
    
    fixed precipitationAmountThreshold = vertexThreshold * noiseSample1;
    particlesAmountFade = min((_ParticlesAmount - precipitationAmountThreshold) * PARTICLE_AMOUNT_LEVELS, 1);
    
    return precipitationAmountThreshold > _ParticlesAmount;
}


void CalculateWidthAndHeight (fixed noise0, fixed noise1, out fixed width, out fixed height) {
    width = _QuadDimensions.x;// + (_QuadDimensions.x * noise0 * 2);
    height = _QuadDimensions.y;// + (_QuadDimensions.y * noise1 * 2);

    fixed sizeMult = lerp(_SizeRange.x, _SizeRange.y, noise0);

    width *= sizeMult;
    height *= sizeMult;
}

void MoveAlongVector (inout fixed3 vertex, fixed noise) {
    vertex.MOVE_COMPONENT += (_Time.y+1000) * (_MoveSpeed + (_MoveSpeed * noise));
}


#define FRAGMENT_UV(i) i.uv.xy
#define HUE_VARIATION(i) i.uv.z
#define DISTANCE_FADE(i) i.uv.w

#define PACK_HUE_VARIATION_AND_FADE(o, hueVar, fade) \
    o.uv.z = hueVar; \
    o.uv.w = fade;


#if defined (SOFT_PARTICLE)

sampler2D _CameraDepthTexture;
float _SoftParticleFactor;

#endif

struct g2f
{
    UNITY_POSITION(pos);
    fixed4 uv : TEXCOORD0;
#if defined (SOFT_PARTICLE)
    fixed4 projPos : TEXCOORD1;
#endif
    UNITY_VERTEX_OUTPUT_STEREO
};

fixed4 frag(g2f IN) : SV_Target
{
    
    fixed2 uv = FRAGMENT_UV(IN);
    // return fixed4(uv, 0, 1);

    fixed4 diffuseColor = tex2D(_MainTex, uv) * _Color;
    
    // add hue variation
    AddHueVariation(diffuseColor.rgb, fixed4(_HueVariation.rgb, HUE_VARIATION(IN)));
                                
    fixed4 c = diffuseColor;
    c.a *= DISTANCE_FADE(IN);

#if defined (SOFT_PARTICLE)
    float sceneZ = LinearEyeDepth (UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(IN.projPos))));
    c.a *= saturate (_SoftParticleFactor * (sceneZ-IN.projPos.z));
#endif

    return c;
}



#endif





#endif // SPEEDTREE_COMMON_INCLUDED
