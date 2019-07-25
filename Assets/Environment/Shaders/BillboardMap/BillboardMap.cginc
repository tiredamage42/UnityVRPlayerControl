
#ifndef BILLBOARD_MAP_INCLUDED
#define BILLBOARD_MAP_INCLUDED

// vertex = pos
// texcoord0 = billboardUVs
// texcoord1 = width, height, bottom, scale

// VERTEX
struct VertexBuffer
{
    fixed4 basePoint : POSITION;
    fixed4 sizeParams : TEXCOORD0;
    fixed4 billboardUVs : TEXCOORD1;
};
    
VertexBuffer vert(VertexBuffer v) { 
    return v; 
}

sampler2D _MainTex;

#define NO_ROTATION_SCALE
#define BUMP_MAP
#include "../ShaderHelp.cginc"
#include "../GeometryShaderUtils.cginc"
#ifndef SHADOWCASTER
    #include "../Environment.cginc"    
#endif

    struct g2f
    {
        UNITY_POSITION(pos);

        fixed4 uv_distancemod_fog : TEXCOORD0;
#ifndef SHADOWCASTER
        fixed hueVariationAmt : TEXCOORD7;
        MY_LIGHTING_COORDS(1, 2, 3, 4, 5, 6)
#else
        V2F_SHADOW_CASTER_NOPOS(1)                       
#endif

        UNITY_VERTEX_OUTPUT_STEREO
    };

    float _Cutoff;
    
    void AddVertex (fixed4 imageTexCoords, fixed3 basePoint, fixed distFade, inout g2f OUT, inout TriangleStream<g2f> stream, VertexBuffer v, fixed3 vertex, fixed2 uv, fixed3 normal, fixed3 tangent, fixed3 bitangent, fixed hueVariationAmt) 
    {
        fixed3 lVertex = vertex - basePoint;
        fixed3 billboardPos =  lVertex.x * tangent;
        billboardPos.y = lVertex.y;
        vertex = billboardPos + basePoint;
        
        UNITY_INITIALIZE_OUTPUT(g2f, OUT);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT); 

        OUT.uv_distancemod_fog = fixed4(imageTexCoords.xy + imageTexCoords.zw * uv, distFade, 0); 
        
#ifndef SHADOWCASTER
        OUT.pos = UnityObjectToClipPos(vertex);
        OUT.hueVariationAmt = hueVariationAmt;

        CALCULATE_WORLD_SPACE_VALUES(vertex, normal, tangent.xyz) //, worldPos_n, worldNormal, worldTangent) 
        
        // output the tangent space matrix (not using macro since we calculated bitangent at base)
        OUT.tSpace0 = fixed4(tangent.x, bitangent.x, normal.x, vertex.x);
        OUT.tSpace1 = fixed4(tangent.y, bitangent.y, normal.y, vertex.y);
        OUT.tSpace2 = fixed4(tangent.z, bitangent.z, normal.z, vertex.z);
        
        CALCULATE_AMBIENT_LIGHTING(OUT)
        CALCULATE_FOG(OUT.pos, OUT.uv_distancemod_fog.w)
        UNITY_TRANSFER_LIGHTING(OUT)
#else       
        TRANSFER_SHADOW_CASTER_NOPOS(OUT, vertex, normal)          
#endif

        stream.Append(OUT);
    }

    fixed4 _CameraRange;

    [maxvertexcount(4)]
    void geom(point VertexBuffer IN[1], inout TriangleStream<g2f> stream)
    {
        VertexBuffer vertBuffer = IN[0];
        
        fixed3 normal = fixed3(0,1,0);
        fixed3 basePoint = vertBuffer.basePoint.xyz;


        CALCULATE_CAMERA_VARIABLES(basePoint)

    fixed fadeStart = _CameraRange.x;
    fixed fadeEnd = _CameraRange.y;

    if (CAMDIST < fadeStart)
        return;

    if (dot(CAMFWD, CAMDIR) < 0.5)
        return;


        fixed distFade = InverseLerp(_CameraRange, CAMDIST);

// texcoord1 = width, height, bottom, scale
fixed4 sizeParams = vertBuffer.sizeParams;

        fixed4 imageTexCoords = vertBuffer.billboardUVs;

        fixed width = sizeParams.x;
        fixed height = sizeParams.y;
        fixed bottom = sizeParams.z;
        fixed scale = sizeParams.w;

        basePoint += bottom;
        fixed3 top = basePoint + normal * height * scale;

        fixed3 perpDir = fixed3(width * .5 * scale, 0, 0);

        g2f OUT;

        fixed3 wTangent = 0, wBitangent = 0;

        fixed3 worldPos = basePoint;
        
        fixed3 eyeVec = normalize(_WorldSpaceCameraPos - basePoint);
        // #if defined(COLOR_PASS)
        // #else
        // fixed3 eyeVec;
        // //directional
        // if (_WorldSpaceLightPos0.w == 0) {
        //     eyeVec = _WorldSpaceLightPos0.xyz;
        // }
        // else {
        //     eyeVec = normalize(_WorldSpaceLightPos0.xyz - basePoint);
        // }
        // #endif

        wTangent = normalize(fixed3(-eyeVec.z, 0, eyeVec.x));            // cross(eyeVec, {0,1,0})
        
        fixed4 hueVariation = 0;

#ifndef SHADOWCASTER
        fixed quadRandom = frac(abs(basePoint.x + basePoint.y + basePoint.z) * 2);
        hueVariation = saturate( quadRandom * _HueVariation.a);
        wBitangent = cross(normal, wTangent) * unity_WorldTransformParams.w;
#endif

        AddVertex (imageTexCoords, basePoint, distFade, OUT, stream, vertBuffer, basePoint - perpDir, fixed2(0, 0), normal, wTangent, wBitangent, hueVariation);
        AddVertex (imageTexCoords, basePoint, distFade, OUT, stream, vertBuffer, basePoint + perpDir, fixed2(1, 0), normal, wTangent, wBitangent, hueVariation);
        AddVertex (imageTexCoords, basePoint, distFade, OUT, stream, vertBuffer, top - perpDir, fixed2(0, 1), normal, wTangent, wBitangent, hueVariation);
        AddVertex (imageTexCoords, basePoint, distFade, OUT, stream, vertBuffer, top + perpDir, fixed2(1, 1), normal, wTangent, wBitangent, hueVariation);
        // stream.RestartStrip();
    }

#ifndef SHADOWCASTER
    sampler2D _BumpMap;
    fixed4 _Color;

    fixed4 frag(g2f IN) : SV_Target
    {
        fixed2 uv = IN.uv_distancemod_fog.xy;
        fixed4 diffuseColor = tex2D(_MainTex, uv);
        clip(diffuseColor.a - (_Cutoff + (1-IN.uv_distancemod_fog.z)));
        diffuseColor.rgb *= _Color.rgb;
        AddHueVariation(diffuseColor.rgb, fixed4(_HueVariation.rgb, IN.hueVariationAmt));
        fixed3 tnormal = UnpackNormalWithScale(tex2D(_BumpMap, uv), 1);       
        FINISH_FRAGMENT_CALC(IN, diffuseColor.rgb, tnormal, IN.uv_distancemod_fog.w)
    }

#else

    fixed frag(g2f IN) : SV_Target {    
        fixed4 diffuseColor = tex2D(_MainTex, IN.uv_distancemod_fog.xy); 
        clip(diffuseColor.a - (_Cutoff + (1-IN.uv_distancemod_fog.z)));
        SHADOW_CASTER_FRAGMENT(IN)                  
    }

#endif

#endif 