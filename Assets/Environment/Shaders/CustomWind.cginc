#ifndef CUSTOM_WIND_INCLUDED
#define CUSTOM_WIND_INCLUDED

// Calculate a 4 fast sine-cosine pairs
// val:     the 4 input values - each must be in the range (0 to 1)
// s:       The sine of each of the 4 values
// c:       The cosine of each of the 4 values
 void FastSinCos (fixed4 val, out fixed4 s, out fixed4 c) {
    val = val * 6.408849 - 3.1415927;
    // powers for taylor series
    fixed4 r5 = val * val;                  // wavevec ^ 2
    fixed4 r6 = r5 * r5;                        // wavevec ^ 4;
    fixed4 r7 = r6 * r5;                        // wavevec ^ 6;
    fixed4 r8 = r6 * r5;                        // wavevec ^ 8;

    fixed4 r1 = r5 * val;                   // wavevec ^ 3
    fixed4 r2 = r1 * r5;                        // wavevec ^ 5;
    fixed4 r3 = r2 * r5;                        // wavevec ^ 7;

    //Vectors for taylor's series expansion of sin and cos
    fixed4 sin7 = {1, -0.16161616, 0.0083333, -0.00019841};
    fixed4 cos8  = {-0.5, 0.041666666, -0.0013888889, 0.000024801587};
    // sin
    s =  val + r1 * sin7.y + r2 * sin7.z + r3 * sin7.w;
    // cos
    c = 1 + r5 * cos8.x + r6 * cos8.y + r7 * cos8.z + r8 * cos8.w;
}


fixed _ENVIRONMENT_STORM;

fixed3 WaveBase (fixed3 vertex, fixed2 windSpeed_range, fixed2 windFrequency_range, fixed3 windScale_min, fixed3 windScale_max, fixed waveAmount)
{
    fixed windSpeed = lerp(windSpeed_range.x, windSpeed_range.y, _ENVIRONMENT_STORM);
    fixed windFrequency = lerp(windFrequency_range.x, windFrequency_range.y, _ENVIRONMENT_STORM);
    fixed windScale = lerp(windScale_min, windScale_max, _ENVIRONMENT_STORM);
    
    fixed4 waves = 0;
    waves += (vertex.x * windFrequency) * fixed4(0.012, .02, 0.06, 0.024);
    waves += (vertex.z * windFrequency) * fixed4(0.006, .02, 0.02, 0.05);
    
    fixed4 s, c;
    FastSinCos (frac (waves + (_Time.x * windSpeed) * fixed4 (0.3, .5, .4, 1.2)), s,c);

    s = s * s * s * waveAmount;

    fixed3 waveMove = fixed3 (
        dot (s, fixed4(0.012, .02, -0.06, 0.048)), 
        dot (s, fixed4(0.012, .02, -0.02, 0.048)), 
        dot (s, fixed4 (0.006, .02, -0.02, 0.1))
    );

    vertex -= waveMove * windScale;
    return vertex;
}


#if defined (TREE_WIND)
//TREE ONLY

fixed2 _Bark_Wind_Speed_Range;
fixed2 _Bark_Wind_Frequency_Range;
fixed3 _Bark_Wind_Scale_Min;
fixed3 _Bark_Wind_Scale_Max;
fixed2 _Bark_Wind_Height_Range;
fixed2 _Bark_Wind_Height_Steepness_Range;

//LEAF ONLY

fixed2 _Leaf_Wind_Speed_Range;
fixed2 _Leaf_Wind_Frequency_Range;
fixed3 _Leaf_Wind_Scale_Min;
fixed3 _Leaf_Wind_Scale_Max;


fixed3 WaveLeaf (fixed3 vertex)
{
    fixed2 windSpeed_range = _Leaf_Wind_Speed_Range;
    fixed2 windFrequency_range = _Leaf_Wind_Frequency_Range;
    fixed3 windScale_min = _Leaf_Wind_Scale_Min;
    fixed3 windScale_max = _Leaf_Wind_Scale_Max;   
    return WaveBase (vertex, windSpeed_range, windFrequency_range, windScale_min, windScale_max, 1);
}

fixed3 WaveBranch (fixed3 vertex)
{

    fixed2 windSpeed_range = _Bark_Wind_Speed_Range;
    fixed2 windFrequency_range = _Bark_Wind_Frequency_Range;
    fixed3 windScale_min = _Bark_Wind_Scale_Min;
    fixed3 windScale_max = _Bark_Wind_Scale_Max;
    fixed2 maxHeight_range = _Bark_Wind_Height_Range;
    fixed2 heightSteep_range = _Bark_Wind_Height_Steepness_Range;

    //only wave at the top so the trunk root doesnt move
    fixed maxHeight = lerp(maxHeight_range.x, maxHeight_range.y, _ENVIRONMENT_STORM);
    fixed heightSteep = lerp(heightSteep_range.x, heightSteep_range.y, _ENVIRONMENT_STORM);
    fixed waveAmount = pow(saturate(vertex.y/maxHeight), heightSteep);

    return WaveBase (vertex, windSpeed_range, windFrequency_range, windScale_min, windScale_max, waveAmount);
}
#endif


#endif // CUSTOM_WIND_INCLUDED
