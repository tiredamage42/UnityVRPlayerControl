// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

#ifndef SPEEDTREE_WIND_INCLUDED
#define SPEEDTREE_WIND_INCLUDED

///////////////////////////////////////////////////////////////////////
//  Wind Info

CBUFFER_START(SpeedTreeWind)
    fixed4 _ST_WindVector;
    fixed4 _ST_WindGlobal;
    fixed4 _ST_WindBranch;
    fixed4 _ST_WindBranchTwitch;
    fixed4 _ST_WindBranchWhip;
    fixed4 _ST_WindBranchAnchor;
    fixed4 _ST_WindBranchAdherences;
    fixed4 _ST_WindTurbulences;
    fixed4 _ST_WindLeaf1Ripple;
    fixed4 _ST_WindLeaf1Tumble;
    fixed4 _ST_WindLeaf1Twitch;
    fixed4 _ST_WindLeaf2Ripple;
    fixed4 _ST_WindLeaf2Tumble;
    fixed4 _ST_WindLeaf2Twitch;
    fixed4 _ST_WindFrondRipple;
    fixed4 _ST_WindAnimation;
CBUFFER_END


///////////////////////////////////////////////////////////////////////
//  UnpackNormalFromfixed

fixed3 UnpackNormalFromfixed(fixed fValue)
{
    fixed3 vDecodeKey = fixed3(16.0, 1.0, 0.0625);

    // decode into [0,1] range
    fixed3 vDecodedValue = frac(fValue / vDecodeKey);

    // move back into [-1,1] range & normalize
    return (vDecodedValue * 2.0 - 1.0);
}


///////////////////////////////////////////////////////////////////////
//  CubicSmooth

fixed4 CubicSmooth(fixed4 vData)
{
    return vData * vData * (3.0 - 2.0 * vData);
}


///////////////////////////////////////////////////////////////////////
//  TriangleWave

fixed4 TriangleWave(fixed4 vData)
{
    return abs((frac(vData + 0.5) * 2.0) - 1.0);
}


///////////////////////////////////////////////////////////////////////
//  TrigApproximate

fixed4 TrigApproximate(fixed4 vData)
{
    return (CubicSmooth(TriangleWave(vData)) - 0.5) * 2.0;
}


///////////////////////////////////////////////////////////////////////
//  RotationMatrix
//
//  Constructs an arbitrary axis rotation matrix

fixed3x3 RotationMatrix(fixed3 vAxis, fixed fAngle)
{
    // compute sin/cos of fAngle
    fixed2 vSinCos;
    #ifdef OPENGL
        vSinCos.x = sin(fAngle);
        vSinCos.y = cos(fAngle);
    #else
        sincos(fAngle, vSinCos.x, vSinCos.y);
    #endif

    const fixed c = vSinCos.y;
    const fixed s = vSinCos.x;
    const fixed t = 1.0 - c;
    const fixed x = vAxis.x;
    const fixed y = vAxis.y;
    const fixed z = vAxis.z;

    return fixed3x3(t * x * x + c,      t * x * y - s * z,  t * x * z + s * y,
                    t * x * y + s * z,  t * y * y + c,      t * y * z - s * x,
                    t * x * z - s * y,  t * y * z + s * x,  t * z * z + c);
}


///////////////////////////////////////////////////////////////////////
//  mul_fixed3x3_fixed3x3

fixed3x3 mul_fixed3x3_fixed3x3(fixed3x3 mMatrixA, fixed3x3 mMatrixB)
{
    return mul(mMatrixA, mMatrixB);
}


///////////////////////////////////////////////////////////////////////
//  mul_fixed3x3_fixed3

fixed3 mul_fixed3x3_fixed3(fixed3x3 mMatrix, fixed3 vVector)
{
    return mul(mMatrix, vVector);
}


///////////////////////////////////////////////////////////////////////
//  cross()'s parameters are backwards in GLSL

#define wind_cross(a, b) cross((a), (b))


///////////////////////////////////////////////////////////////////////
//  Roll

fixed Roll(fixed fCurrent,
           fixed fMaxScale,
           fixed fMinScale,
           fixed fSpeed,
           fixed fRipple,
           fixed3 vPos,
           fixed fTime,
           fixed3 vRotatedWindVector)
{
    fixed fWindAngle = dot(vPos, -vRotatedWindVector) * fRipple;
    fixed fAdjust = TrigApproximate(fixed4(fWindAngle + fTime * fSpeed, 0.0, 0.0, 0.0)).x;
    fAdjust = (fAdjust + 1.0) * 0.5;

    return lerp(fCurrent * fMinScale, fCurrent * fMaxScale, fAdjust);
}


///////////////////////////////////////////////////////////////////////
//  Twitch

fixed Twitch(fixed3 vPos, fixed fAmount, fixed fSharpness, fixed fTime)
{
    const fixed c_fTwitchFudge = 0.87;
    fixed4 vOscillations = TrigApproximate(fixed4(fTime + (vPos.x + vPos.z), c_fTwitchFudge * fTime + vPos.y, 0.0, 0.0));

    //fixed fTwitch = sin(fFreq1 * fTime + (vPos.x + vPos.z)) * cos(fFreq2 * fTime + vPos.y);
    fixed fTwitch = vOscillations.x * vOscillations.y * vOscillations.y;
    fTwitch = (fTwitch + 1.0) * 0.5;

    return fAmount * pow(saturate(fTwitch), fSharpness);
}


///////////////////////////////////////////////////////////////////////
//  Oscillate
//
//  This function computes an oscillation value and whip value if necessary.
//  Whip and oscillation are combined like this to minimize calls to
//  TrigApproximate( ) when possible.

fixed Oscillate(fixed3 vPos,
                fixed fTime,
                fixed fOffset,
                fixed fWeight,
                fixed fWhip,
                bool bWhip,
                bool bRoll,
                bool bComplex,
                fixed fTwitch,
                fixed fTwitchFreqScale,
                inout fixed4 vOscillations,
                fixed3 vRotatedWindVector)
{
    fixed fOscillation = 1.0;
    if (bComplex)
    {
        if (bWhip)
            vOscillations = TrigApproximate(fixed4(fTime + fOffset, fTime * fTwitchFreqScale + fOffset, fTwitchFreqScale * 0.5 * (fTime + fOffset), fTime + fOffset + (1.0 - fWeight)));
        else
            vOscillations = TrigApproximate(fixed4(fTime + fOffset, fTime * fTwitchFreqScale + fOffset, fTwitchFreqScale * 0.5 * (fTime + fOffset), 0.0));

        fixed fFineDetail = vOscillations.x;
        fixed fBroadDetail = vOscillations.y * vOscillations.z;

        fixed fTarget = 1.0;
        fixed fAmount = fBroadDetail;
        if (fBroadDetail < 0.0)
        {
            fTarget = -fTarget;
            fAmount = -fAmount;
        }

        fBroadDetail = lerp(fBroadDetail, fTarget, fAmount);
        fBroadDetail = lerp(fBroadDetail, fTarget, fAmount);

        fOscillation = fBroadDetail * fTwitch * (1.0 - _ST_WindVector.w) + fFineDetail * (1.0 - fTwitch);

        if (bWhip)
            fOscillation *= 1.0 + (vOscillations.w * fWhip);
    }
    else
    {
        if (bWhip)
            vOscillations = TrigApproximate(fixed4(fTime + fOffset, fTime * 0.689 + fOffset, 0.0, fTime + fOffset + (1.0 - fWeight)));
        else
            vOscillations = TrigApproximate(fixed4(fTime + fOffset, fTime * 0.689 + fOffset, 0.0, 0.0));

        fOscillation = vOscillations.x + vOscillations.y * vOscillations.x;

        if (bWhip)
            fOscillation *= 1.0 + (vOscillations.w * fWhip);
    }

    //if (bRoll)
    //{
    //  fOscillation = Roll(fOscillation, _ST_WindRollingBranches.x, _ST_WindRollingBranches.y, _ST_WindRollingBranches.z, _ST_WindRollingBranches.w, vPos.xyz, fTime + fOffset, vRotatedWindVector);
    //}

    return fOscillation;
}


///////////////////////////////////////////////////////////////////////
//  Turbulence

fixed Turbulence(fixed fTime, fixed fOffset, fixed fGlobalTime, fixed fTurbulence)
{
    const fixed c_fTurbulenceFactor = 0.1;

    fixed4 vOscillations = TrigApproximate(fixed4(fTime * c_fTurbulenceFactor + fOffset, fGlobalTime * fTurbulence * c_fTurbulenceFactor + fOffset, 0.0, 0.0));

    return 1.0 - (vOscillations.x * vOscillations.y * vOscillations.x * vOscillations.y * fTurbulence);
}


///////////////////////////////////////////////////////////////////////
//  GlobalWind
//
//  This function positions any tree geometry based on their untransformed
//  position and 4 wind fixeds.

fixed3 GlobalWind(fixed3 vPos, fixed3 vInstancePos, bool bPreserveShape, fixed3 vRotatedWindVector, fixed time)
{
    // WIND_LOD_GLOBAL may be on, but if the global wind effect (WIND_EFFECT_GLOBAL_ST_Wind)
    // was disabled for the tree in the Modeler, we should skip it

    fixed fLength = 1.0;
    if (bPreserveShape)
        fLength = length(vPos.xyz);

    // compute how much the height contributes
    #ifdef SPEEDTREE_Z_UP
        fixed fAdjust = max(vPos.z - (1.0 / _ST_WindGlobal.z) * 0.25, 0.0) * _ST_WindGlobal.z;
    #else
        fixed fAdjust = max(vPos.y - (1.0 / _ST_WindGlobal.z) * 0.25, 0.0) * _ST_WindGlobal.z;
    #endif
    if (fAdjust != 0.0)
        fAdjust = pow(fAdjust, _ST_WindGlobal.w);

    // primary oscillation
    fixed4 vOscillations = TrigApproximate(fixed4(vInstancePos.x + time, vInstancePos.y + time * 0.8, 0.0, 0.0));
    fixed fOsc = vOscillations.x + (vOscillations.y * vOscillations.y);
    fixed fMoveAmount = _ST_WindGlobal.y * fOsc;

    // move a minimum amount based on direction adherence
    fMoveAmount += _ST_WindBranchAdherences.x / _ST_WindGlobal.z;

    // adjust based on how high up the tree this vertex is
    fMoveAmount *= fAdjust;

    // xy component
    #ifdef SPEEDTREE_Z_UP
        vPos.xy += vRotatedWindVector.xy * fMoveAmount;
    #else
        vPos.xz += vRotatedWindVector.xz * fMoveAmount;
    #endif

    if (bPreserveShape)
        vPos.xyz = normalize(vPos.xyz) * fLength;

    return vPos;
}


///////////////////////////////////////////////////////////////////////
//  SimpleBranchWind

fixed3 SimpleBranchWind(fixed3 vPos,
                        fixed3 vInstancePos,
                        fixed fWeight,
                        fixed fOffset,
                        fixed fTime,
                        fixed fDistance,
                        fixed fTwitch,
                        fixed fTwitchScale,
                        fixed fWhip,
                        bool bWhip,
                        bool bRoll,
                        bool bComplex,
                        fixed3 vRotatedWindVector)
{
    // turn the offset back into a nearly normalized vector
    fixed3 vWindVector = UnpackNormalFromfixed(fOffset);
    vWindVector = vWindVector * fWeight;

    // try to fudge time a bit so that instances aren't in sync
    fTime += vInstancePos.x + vInstancePos.y;

    // oscillate
    fixed4 vOscillations;
    fixed fOsc = Oscillate(vPos, fTime, fOffset, fWeight, fWhip, bWhip, bRoll, bComplex, fTwitch, fTwitchScale, vOscillations, vRotatedWindVector);

    vPos.xyz += vWindVector * fOsc * fDistance;

    return vPos;
}


///////////////////////////////////////////////////////////////////////
//  DirectionalBranchWind

fixed3 DirectionalBranchWind(fixed3 vPos,
                             fixed3 vInstancePos,
                             fixed fWeight,
                             fixed fOffset,
                             fixed fTime,
                             fixed fDistance,
                             fixed fTurbulence,
                             fixed fAdherence,
                             fixed fTwitch,
                             fixed fTwitchScale,
                             fixed fWhip,
                             bool bWhip,
                             bool bRoll,
                             bool bComplex,
                             bool bTurbulence,
                             fixed3 vRotatedWindVector)
{
    // turn the offset back into a nearly normalized vector
    fixed3 vWindVector = UnpackNormalFromfixed(fOffset);
    vWindVector = vWindVector * fWeight;

    // try to fudge time a bit so that instances aren't in sync
    fTime += vInstancePos.x + vInstancePos.y;

    // oscillate
    fixed4 vOscillations;
    fixed fOsc = Oscillate(vPos, fTime, fOffset, fWeight, fWhip, bWhip, false, bComplex, fTwitch, fTwitchScale, vOscillations, vRotatedWindVector);

    vPos.xyz += vWindVector * fOsc * fDistance;

    // add in the direction, accounting for turbulence
    fixed fAdherenceScale = 1.0;
    if (bTurbulence)
        fAdherenceScale = Turbulence(fTime, fOffset, _ST_WindAnimation.x, fTurbulence);

    if (bWhip)
        fAdherenceScale += vOscillations.w * _ST_WindVector.w * fWhip;

    //if (bRoll)
    //  fAdherenceScale = Roll(fAdherenceScale, _ST_WindRollingBranches.x, _ST_WindRollingBranches.y, _ST_WindRollingBranches.z, _ST_WindRollingBranches.w, vPos.xyz, fTime + fOffset, vRotatedWindVector);

    vPos.xyz += vRotatedWindVector * fAdherence * fAdherenceScale * fWeight;

    return vPos;
}


///////////////////////////////////////////////////////////////////////
//  DirectionalBranchWindFrondStyle

fixed3 DirectionalBranchWindFrondStyle(fixed3 vPos,
                                       fixed3 vInstancePos,
                                       fixed fWeight,
                                       fixed fOffset,
                                       fixed fTime,
                                       fixed fDistance,
                                       fixed fTurbulence,
                                       fixed fAdherence,
                                       fixed fTwitch,
                                       fixed fTwitchScale,
                                       fixed fWhip,
                                       bool bWhip,
                                       bool bRoll,
                                       bool bComplex,
                                       bool bTurbulence,
                                       fixed3 vRotatedWindVector,
                                       fixed3 vRotatedBranchAnchor)
{
    // turn the offset back into a nearly normalized vector
    fixed3 vWindVector = UnpackNormalFromfixed(fOffset);
    vWindVector = vWindVector * fWeight;

    // try to fudge time a bit so that instances aren't in sync
    fTime += vInstancePos.x + vInstancePos.y;

    // oscillate
    fixed4 vOscillations;
    fixed fOsc = Oscillate(vPos, fTime, fOffset, fWeight, fWhip, bWhip, false, bComplex, fTwitch, fTwitchScale, vOscillations, vRotatedWindVector);

    vPos.xyz += vWindVector * fOsc * fDistance;

    // add in the direction, accounting for turbulence
    fixed fAdherenceScale = 1.0;
    if (bTurbulence)
        fAdherenceScale = Turbulence(fTime, fOffset, _ST_WindAnimation.x, fTurbulence);

    //if (bRoll)
    //  fAdherenceScale = Roll(fAdherenceScale, _ST_WindRollingBranches.x, _ST_WindRollingBranches.y, _ST_WindRollingBranches.z, _ST_WindRollingBranches.w, vPos.xyz, fTime + fOffset, vRotatedWindVector);

    if (bWhip)
        fAdherenceScale += vOscillations.w * _ST_WindVector.w * fWhip;

    fixed3 vWindAdherenceVector = vRotatedBranchAnchor - vPos.xyz;
    vPos.xyz += vWindAdherenceVector * fAdherence * fAdherenceScale * fWeight;

    return vPos;
}


///////////////////////////////////////////////////////////////////////
//  BranchWind

// Apply only to better, best, palm winds
fixed3 BranchWind(bool isPalmWind, fixed3 vPos, fixed3 vInstancePos, fixed4 vWindData, fixed3 vRotatedWindVector, fixed3 vRotatedBranchAnchor)
{
    if (isPalmWind)
    {
        vPos = DirectionalBranchWindFrondStyle(vPos, vInstancePos, vWindData.x, vWindData.y, _ST_WindBranch.x, _ST_WindBranch.y, _ST_WindTurbulences.x, _ST_WindBranchAdherences.y, _ST_WindBranchTwitch.x, _ST_WindBranchTwitch.y, _ST_WindBranchWhip.x, true, false, true, true, vRotatedWindVector, vRotatedBranchAnchor);
    }
    else
    {
        vPos = SimpleBranchWind(vPos, vInstancePos, vWindData.x, vWindData.y, _ST_WindBranch.x, _ST_WindBranch.y, _ST_WindBranchTwitch.x, _ST_WindBranchTwitch.y, _ST_WindBranchWhip.x, false, false, true, vRotatedWindVector);
    }

    return vPos;
}


///////////////////////////////////////////////////////////////////////
//  LeafRipple

fixed3 LeafRipple(fixed3 vPos,
                  inout fixed3 vDirection,
                  fixed fScale,
                  fixed fPackedRippleDir,
                  fixed fTime,
                  fixed fAmount,
                  bool bDirectional,
                  fixed fTrigOffset)
{
    // compute how much to move
    fixed4 vInput = fixed4(fTime + fTrigOffset, 0.0, 0.0, 0.0);
    fixed fMoveAmount = fAmount * TrigApproximate(vInput).x;

    if (bDirectional)
    {
        vPos.xyz += vDirection.xyz * fMoveAmount * fScale;
    }
    else
    {
        fixed3 vRippleDir = UnpackNormalFromfixed(fPackedRippleDir);
        vPos.xyz += vRippleDir * fMoveAmount * fScale;
    }

    return vPos;
}


///////////////////////////////////////////////////////////////////////
//  LeafTumble

fixed3 LeafTumble(fixed3 vPos,
                  inout fixed3 vDirection,
                  fixed fScale,
                  fixed3 vAnchor,
                  fixed3 vGrowthDir,
                  fixed fTrigOffset,
                  fixed fTime,
                  fixed fFlip,
                  fixed fTwist,
                  fixed fAdherence,
                  fixed3 vTwitch,
                  fixed4 vRoll,
                  bool bTwitch,
                  bool bRoll,
                  fixed3 vRotatedWindVector)
{
    // compute all oscillations up front
    fixed3 vFracs = frac((vAnchor + fTrigOffset) * 30.3);
    fixed fOffset = vFracs.x + vFracs.y + vFracs.z;
    fixed4 vOscillations = TrigApproximate(fixed4(fTime + fOffset, fTime * 0.75 - fOffset, fTime * 0.01 + fOffset, fTime * 1.0 + fOffset));

    // move to the origin and get the growth direction
    fixed3 vOriginPos = vPos.xyz - vAnchor;
    fixed fLength = length(vOriginPos);

    // twist
    fixed fOsc = vOscillations.x + vOscillations.y * vOscillations.y;
    fixed3x3 matTumble = RotationMatrix(vGrowthDir, fScale * fTwist * fOsc);

    // with wind
    fixed3 vAxis = wind_cross(vGrowthDir, vRotatedWindVector);
    fixed fDot = clamp(dot(vRotatedWindVector, vGrowthDir), -1.0, 1.0);
    #ifdef SPEEDTREE_Z_UP
        vAxis.z += fDot;
    #else
        vAxis.y += fDot;
    #endif
    vAxis = normalize(vAxis);

    fixed fAngle = acos(fDot);

    fixed fAdherenceScale = 1.0;
    //if (bRoll)
    //{
    //  fAdherenceScale = Roll(fAdherenceScale, vRoll.x, vRoll.y, vRoll.z, vRoll.w, vAnchor.xyz, fTime, vRotatedWindVector);
    //}

    fOsc = vOscillations.y - vOscillations.x * vOscillations.x;

    fixed fTwitch = 0.0;
    if (bTwitch)
        fTwitch = Twitch(vAnchor.xyz, vTwitch.x, vTwitch.y, vTwitch.z + fOffset);

    matTumble = mul_fixed3x3_fixed3x3(matTumble, RotationMatrix(vAxis, fScale * (fAngle * fAdherence * fAdherenceScale + fOsc * fFlip + fTwitch)));

    vDirection = mul_fixed3x3_fixed3(matTumble, vDirection);
    vOriginPos = mul_fixed3x3_fixed3(matTumble, vOriginPos);

    vOriginPos = normalize(vOriginPos) * fLength;

    return (vOriginPos + vAnchor);
}


///////////////////////////////////////////////////////////////////////
//  LeafWind
//  Optimized (for instruction count) version. Assumes leaf 1 and 2 have the same options

fixed3 LeafWind(bool isBestWind,
                bool bLeaf2,
                fixed3 vPos,
                inout fixed3 vDirection,
                fixed fScale,
                fixed3 vAnchor,
                fixed fPackedGrowthDir,
                fixed fPackedRippleDir,
                fixed fRippleTrigOffset,
                fixed3 vRotatedWindVector)
{

    vPos = LeafRipple(vPos, vDirection, fScale, fPackedRippleDir,
                            (bLeaf2 ? _ST_WindLeaf2Ripple.x : _ST_WindLeaf1Ripple.x),
                            (bLeaf2 ? _ST_WindLeaf2Ripple.y : _ST_WindLeaf1Ripple.y),
                            false, fRippleTrigOffset);

    if (isBestWind)
    {
        fixed3 vGrowthDir = UnpackNormalFromfixed(fPackedGrowthDir);
        vPos = LeafTumble(vPos, vDirection, fScale, vAnchor, vGrowthDir, fPackedGrowthDir,
                          (bLeaf2 ? _ST_WindLeaf2Tumble.x : _ST_WindLeaf1Tumble.x),
                          (bLeaf2 ? _ST_WindLeaf2Tumble.y : _ST_WindLeaf1Tumble.y),
                          (bLeaf2 ? _ST_WindLeaf2Tumble.z : _ST_WindLeaf1Tumble.z),
                          (bLeaf2 ? _ST_WindLeaf2Tumble.w : _ST_WindLeaf1Tumble.w),
                          (bLeaf2 ? _ST_WindLeaf2Twitch.xyz : _ST_WindLeaf1Twitch.xyz),
                          0.0f,
                          (bLeaf2 ? true : true),
                          (bLeaf2 ? true : true),
                          vRotatedWindVector);
    }

    return vPos;
}


///////////////////////////////////////////////////////////////////////
//  RippleFrondOneSided

fixed3 RippleFrondOneSided(fixed3 vPos,
                           inout fixed3 vDirection,
                           fixed fU,
                           fixed fV,
                           fixed fRippleScale
#ifdef WIND_EFFECT_FROND_RIPPLE_ADJUST_LIGHTING
                           , fixed3 vBinormal
                           , fixed3 vTangent
#endif
                           )
{
    fixed fOffset = 0.0;
    if (fU < 0.5)
        fOffset = 0.75;

    fixed4 vOscillations = TrigApproximate(fixed4((_ST_WindFrondRipple.x + fV) * _ST_WindFrondRipple.z + fOffset, 0.0, 0.0, 0.0));

    fixed fAmount = fRippleScale * vOscillations.x * _ST_WindFrondRipple.y;
    fixed3 vOffset = fAmount * vDirection;
    vPos.xyz += vOffset;

    #ifdef WIND_EFFECT_FROND_RIPPLE_ADJUST_LIGHTING
        vTangent.xyz = normalize(vTangent.xyz + vOffset * _ST_WindFrondRipple.w);
        fixed3 vNewNormal = normalize(wind_cross(vBinormal.xyz, vTangent.xyz));
        if (dot(vNewNormal, vDirection.xyz) < 0.0)
            vNewNormal = -vNewNormal;
        vDirection.xyz = vNewNormal;
    #endif

    return vPos;
}

///////////////////////////////////////////////////////////////////////
//  RippleFrondTwoSided

fixed3 RippleFrondTwoSided(fixed3 vPos,
                           inout fixed3 vDirection,
                           fixed fU,
                           fixed fLengthPercent,
                           fixed fPackedRippleDir,
                           fixed fRippleScale
#ifdef WIND_EFFECT_FROND_RIPPLE_ADJUST_LIGHTING
                           , fixed3 vBinormal
                           , fixed3 vTangent
#endif
                           )
{
    fixed4 vOscillations = TrigApproximate(fixed4(_ST_WindFrondRipple.x * fLengthPercent * _ST_WindFrondRipple.z, 0.0, 0.0, 0.0));

    fixed3 vRippleDir = UnpackNormalFromfixed(fPackedRippleDir);

    fixed fAmount = fRippleScale * vOscillations.x * _ST_WindFrondRipple.y;
    fixed3 vOffset = fAmount * vRippleDir;

    vPos.xyz += vOffset;

    #ifdef WIND_EFFECT_FROND_RIPPLE_ADJUST_LIGHTING
        vTangent.xyz = normalize(vTangent.xyz + vOffset * _ST_WindFrondRipple.w);
        fixed3 vNewNormal = normalize(wind_cross(vBinormal.xyz, vTangent.xyz));
        if (dot(vNewNormal, vDirection.xyz) < 0.0)
            vNewNormal = -vNewNormal;
        vDirection.xyz = vNewNormal;
    #endif

    return vPos;
}


///////////////////////////////////////////////////////////////////////
//  RippleFrond

fixed3 RippleFrond(fixed3 vPos,
                   inout fixed3 vDirection,
                   fixed fU,
                   fixed fV,
                   fixed fPackedRippleDir,
                   fixed fRippleScale,
                   fixed fLenghtPercent
                #ifdef WIND_EFFECT_FROND_RIPPLE_ADJUST_LIGHTING
                   , fixed3 vBinormal
                   , fixed3 vTangent
                #endif
                   )
{
    return RippleFrondOneSided(vPos,
                                vDirection,
                                fU,
                                fV,
                                fRippleScale
                            #ifdef WIND_EFFECT_FROND_RIPPLE_ADJUST_LIGHTING
                                , vBinormal
                                , vTangent
                            #endif
                                );
}

#endif // SPEEDTREE_WIND_INCLUDED
