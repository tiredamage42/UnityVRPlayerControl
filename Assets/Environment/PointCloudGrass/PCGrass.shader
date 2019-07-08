
Shader "Custom/PCGrass" {

	Properties{
		_MainTex("Albedo (RGB)", 2D) = "white" {}

		_Cutoff("Cutoff", Range(0,1)) = 0.25
        
		
	}
		SubShader{
		Tags{ "Queue" = "AlphaTest" "IgnoreProjector" = "True" "RenderType" = "TransparentCutout" }
		LOD 200

		Pass
	{

		CULL OFF

		CGPROGRAM
        #include "UnityCG.cginc" 
        #pragma vertex vert
        #pragma fragment frag
        #pragma geometry geom

        // Use shader model 4.0 target, we need geometry shader support
        #pragma target 4.0

		sampler2D _MainTex;

        static const fixed SIN45 = 0.70710678087;
        static const fixed SIN45_I = 1.0 - SIN45;

    fixed4x4 rotationMatrix(fixed3 axis, fixed sinAngle)
    {
        fixed3 oc = axis * SIN45_I;

        fixed ocxy = oc.x * axis.y;
        fixed oczx = oc.z * axis.x;
        fixed ocyz = oc.y * axis.z;

        fixed3 aSin = axis * sinAngle;
        
        return fixed4x4 (
            oc.x * axis.x + SIN45, 
            ocxy - aSin.z, 
            oczx + aSin.y, 
            0.0,
        
            ocxy + aSin.z, 
            oc.y * axis.y + SIN45, 
            ocyz - aSin.x, 
            0.0,
            
            oczx - aSin.y, 
            ocyz + aSin.x, 
            oc.z * axis.z + SIN45, 
            0.0,
            
            0.0, 0.0, 0.0, 1.0
        );
    }


	half _Cutoff;

    // get the point in

    // vertex = pos
    // normal = ground normal
    // color = tint color
    // texcoord0 = textureAtlasOffset
    // texcoord1 = textureAtlasOffsetNorm
    // texcoord2 = hueVariation
    // texcoord3 = (width, height, 0, 0)

	struct v2g
	{
		fixed4 vertex : POSITION;
		fixed3 normal : NORMAL;
        fixed4 color : COLOR;
		fixed4 textureAtlasOffset : TEXCOORD0;
        fixed4 textureAtlasOffsetNorm : TEXCOORD1;
        fixed4 hueVariation : TEXCOORD2;
        fixed4 widthHeight : TEXCOORD3;
	};

	v2g vert(v2g v)
	{
        return v;
	}


    struct g2f
	{
		fixed4 pos : SV_POSITION;
		fixed3 norm : NORMAL;
        fixed4 tintColor : COLOR;
        fixed2 uv : TEXCOORD0;
        fixed4 hueVariation : TEXCOORD1;
	};

    void AddVertex (fixed3 rawVertex, fixed3 vertex, fixed2 uv, v2g V2G, fixed3 faceNormal, inout g2f OUT, inout TriangleStream<g2f> triStream) {
        
            
        fixed3 worldPos = rawVertex + fixed3(unity_ObjectToWorld[0].w, unity_ObjectToWorld[1].w, unity_ObjectToWorld[2].w);
        OUT.hueVariation = fixed4(V2G.hueVariation.rgb, saturate(frac((worldPos.x + faceNormal.y + worldPos.z) * (faceNormal.x + worldPos.y + faceNormal.z)) * V2G.hueVariation.a));
        
        OUT.pos = UnityObjectToClipPos(vertex);
		OUT.norm = faceNormal;
        OUT.uv = uv; 
		OUT.tintColor = V2G.color;
        triStream.Append(OUT);
    }

    void DrawQuad (fixed3 base, fixed3 rawTop, fixed3 top, fixed4 uvOffset, v2g V2G, fixed3 perpDir, fixed3 faceNormal, inout g2f OUT, inout TriangleStream<g2f> triStream) {
        AddVertex (base - perpDir, base - perpDir, uvOffset.xy + fixed2(0, 0) * uvOffset.zw, V2G, faceNormal, OUT, triStream);
        AddVertex (base + perpDir, base + perpDir, uvOffset.xy + fixed2(1, 0) * uvOffset.zw, V2G, faceNormal, OUT, triStream);
        AddVertex (rawTop - perpDir, top - perpDir, uvOffset.xy + fixed2(0, 1) * uvOffset.zw, V2G, faceNormal, OUT, triStream);
        AddVertex (rawTop + perpDir, top + perpDir, uvOffset.xy + fixed2(1, 1) * uvOffset.zw, V2G, faceNormal, OUT, triStream);
        triStream.RestartStrip();
    }



    fixed4 _PCGRASS_CAMERA_RANGE;

    
    
	[maxvertexcount(12)]
	void geom(point v2g IN[1], inout TriangleStream<g2f> triStream)
	{
        v2g geometryIN = IN[0];
		fixed3 base = IN[0].vertex.xyz;





        fixed3 viewDir = _WorldSpaceCameraPos - (base + fixed3(unity_ObjectToWorld[0].w, unity_ObjectToWorld[1].w, unity_ObjectToWorld[2].w));

        fixed cameraDistance = length(viewDir);// distance(_WorldSpaceCameraPos, (base + fixed3(unity_ObjectToWorld[0].w, unity_ObjectToWorld[1].w, unity_ObjectToWorld[2].w)));

        fixed fadeStart = _PCGRASS_CAMERA_RANGE.x;
        fixed fadeEnd =_PCGRASS_CAMERA_RANGE.y;
        if (cameraDistance >= fadeEnd) {
            return;
        }

        fixed distanceMod = saturate(max(cameraDistance - fadeStart, 0) / (fadeEnd - fadeStart));
        distanceMod = 1.0 - distanceMod;



        fixed3 groundNorm = IN[0].normal;
        
        fixed width = IN[0].widthHeight.x;
        
        fixed height = IN[0].widthHeight.y;// * distanceMod;

        fixed4 uvOffset = IN[0].textureAtlasOffset;

		fixed3 rawTop = base + groundNorm * height;

        fixed3 top = base + groundNorm * height * distanceMod;
		
        fixed3 perpendicularDirection = fixed3(0, 0, 1);
		fixed3 faceNormal = cross(perpendicularDirection, groundNorm);
        perpendicularDirection = cross(groundNorm, faceNormal);

		g2f OUT;

		// Quad 1
        fixed3 quad1Perp = perpendicularDirection * .5 * width;
        fixed3 quad1Norm = cross(perpendicularDirection, groundNorm);

        DrawQuad (base, rawTop, top, uvOffset, geometryIN, quad1Perp, quad1Norm, OUT, triStream);

		// Quad 2
        fixed4x4 quad2Matrix = rotationMatrix(groundNorm, -SIN45);
        fixed3 quad2Perp = mul(quad2Matrix, quad1Perp);
        fixed3 quad2Norm = mul(quad2Matrix, quad1Norm);

        DrawQuad (base, rawTop, top, uvOffset, geometryIN, quad2Perp, quad2Norm, OUT, triStream);

		// Quad 3
        fixed4x4 quad3Matrix = rotationMatrix(groundNorm, SIN45);
        fixed3 quad3Perp = mul(quad3Matrix, quad1Perp);
        fixed3 quad3Norm = mul(quad3Matrix, quad1Norm);

        DrawQuad (base, rawTop, top, uvOffset, geometryIN, quad3Perp, quad3Norm, OUT, triStream);
	}

	half4 frag(g2f IN) : COLOR
	{

        fixed4 diffuseColor = tex2D(_MainTex, IN.uv) * IN.tintColor;
        clip(diffuseColor.a - _Cutoff);
        
        // add hue variation
        fixed3 shiftedColor = lerp(diffuseColor.rgb, IN.hueVariation.rgb, IN.hueVariation.a);
        fixed maxBase = max(diffuseColor.r, max(diffuseColor.g, diffuseColor.b));
        fixed newMaxBase = max(shiftedColor.r, max(shiftedColor.g, shiftedColor.b));
        maxBase /= newMaxBase;
        maxBase = maxBase * 0.5 + 0.5;
        // preserve vibrance
        shiftedColor.rgb *= maxBase;
        diffuseColor.rgb = saturate(shiftedColor);

        return diffuseColor;
	}

    
        
		ENDCG

	}
	}
}