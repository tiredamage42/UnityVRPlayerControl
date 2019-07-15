#ifndef ATMOSPHERIC_SCATTERING_INCLUDED
#define ATMOSPHERIC_SCATTERING_INCLUDED



float RayleighPhase(float cosTheta)
{
	return 0.75 + 0.75 * (1.0 + cosTheta * cosTheta);
}

float Scale(float Cos)
{
	float x = 1.0 - Cos;
	return 0.25 * exp( -0.00287 + x * (0.459 + x * (3.83 + x * (-6.80 + x * 5.25))) );
}
//----------------------------------------------------------------------------------------------


inline void AtmosphericScattering(float3 ray, out half3 inScatter, out half4 outScatter)//, bool clampScatter)
{

	ray.y = max(0.0, ray.y); // fix downside.

	float3 cameraPos = float3(0.0, LSky_kInnerRadius + LSky_kCameraHeight, 0.0); 
	float  far       = sqrt(LSky_kOuterRadius2 + LSky_kInnerRadius2 * ray.y * ray.y - LSky_kInnerRadius2) - LSky_kInnerRadius * ray.y;
	float3 pos       = cameraPos + far * ray;
	//-------------------------------------------------------------------------------------------------------------------------------------

//	float startDepth  = exp(kScaleOverScaleDepth * (kInnerRadius - kCameraHeight));
	float startDepth  = exp(LSky_kScaleOverScaleDepth * (-LSky_kCameraHeight));  
	float startHeight = LSky_kInnerRadius + LSky_kCameraHeight;
	float startAngle  = dot(ray, cameraPos) / startHeight;
	float startOffset = startDepth * Scale(startAngle);
	//-------------------------------------------------------------------------------------------------------------------------------------

	const float kSamples = 2;

	float  sampleLength = far / kSamples;
	float  scaledLength = sampleLength * LSky_kScale;
	float3 sampleRay    = ray * sampleLength;
	float3 samplePoint  = cameraPos + sampleRay * 0.5;
	//-------------------------------------------------------------------------------------------------------------------------------------


    float3 betaAtten   = (LSky_InvWavelength * LSky_kKr4PI) + LSky_kKm4PI;
    fixed upScaled = Scale(half3(0,1,0));

	float3 frontColor = 0.0; float4 outColor = 0.0; 

	for(int i = 0; i < int(kSamples); i++)
	{

		float height    = length(samplePoint);
		float invHeight = 1.0 / height; // reciprocal.
		//---------------------------------------------------------------------------------------------------------------------------------

		float  depth       = exp(LSky_kScaleOverScaleDepth * (LSky_kInnerRadius - height));
        fixed scaledDepth = depth * scaledLength;

		float  cameraAngle = dot(ray, samplePoint) * invHeight;
        fixed scaledCamera = Scale(cameraAngle);


		float lightAngle  = dot(LSky_SunDir.xyz, samplePoint) * invHeight; 
		float scatter     = startOffset + depth * ( Scale(lightAngle) - scaledCamera );
		float3 attenuate   = exp(-clamp(scatter, 0.0, 50) * betaAtten);
		float3 dayColor    = attenuate * (scaledDepth) * LSky_DayAtmosphereTint;
		
		
		//---------------------------------------------------------------------------------------------------------------------------------


		float nightLightAngle =  dot(LSky_MoonDir.xyz, samplePoint) * invHeight; 
		float nightScatter   = startOffset + depth * (Scale(nightLightAngle) - scaledCamera);
		float3 nightAttenuate = exp(-clamp(nightScatter, 0.0, 50) * betaAtten );
		float3 nightColor  = nightAttenuate * (scaledDepth) * LSky_NightAtmosphereTint;
		frontColor += nightColor;
		
		//---------------------------------------------------------------------------------------------------------------------------------

		frontColor   += dayColor; 
		outColor.rgb += dayColor; 
		outColor.a   += exp(-(startOffset + depth * (upScaled - scaledCamera)) * betaAtten);
		//---------------------------------------------------------------------------------------------------------------------------------

		samplePoint  += sampleRay;
		//---------------------------------------------------------------------------------------------------------------------------------

	}

	float cosTheta  = dot(ray, LSky_SunDir.xyz);
	inScatter       =((frontColor * (LSky_InvWavelength * LSky_kKrESun))) * RayleighPhase(cosTheta);
	//-------------------------------------------------------------------------------------------------------------------------------------

	
	outScatter.rgb  = outColor * LSky_kKmESun;
	outScatter.a    = (Desaturate(outColor.a));
	//-------------------------------------------------------------------------------------------------------------------------------------
}


#endif //ATMOSPHERIC_SCATTERING_INCLUDED
