// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

#ifndef GEOMETRY_SHADER_UTILS_INCLUDED
#define GEOMETRY_SHADER_UTILS_INCLUDED


#include "AutoLightGEOM.cginc"
#include "UnityCGGEOM.cginc"


// QUAD ROTATION TOOLS
#define COS90 0
#define SIN90 1
#define SIN45 0.70710678087
#define COS45 0.70710678087



fixed4x4 rotationMatrix(fixed3 axis, fixed sinAngle, fixed cosAngle)
{
    // axis = normalize(axis);

    fixed3 oc = (1.0 - cosAngle) * axis;

    fixed ocxy = oc.x * axis.y;
    fixed oczx = oc.z * axis.x;
    fixed ocyz = oc.y * axis.z;

    fixed3 aSin = axis * sinAngle;

    return fixed4x4(
        oc.x * axis.x + cosAngle,           
        ocxy - aSin.z,
        oczx + aSin.y,  
        0.0,
        
        ocxy + aSin.z,  
        oc.y * axis.y + cosAngle,           
        ocyz - aSin.x,  
        0.0,
        
        oczx - aSin.y,  
        ocyz + aSin.x,  
        oc.z * axis.z + cosAngle,           
        0.0,
        
        0.0, 0.0, 0.0, 1.0
    );
}




fixed CalculateDistanceFade (fixed3 worldPos, fixed2 cameraRange, out fixed cameraDistance) {
    fixed3 viewDir = _WorldSpaceCameraPos - worldPos;
    cameraDistance = length(viewDir);

    fixed fadeStart = cameraRange.x;
    fixed fadeEnd =cameraRange.y;

    if (cameraDistance > fadeEnd)
        return 0;

    return 1.0 - saturate(max(cameraDistance - fadeStart, 0) / (fadeEnd - fadeStart));
}

#endif // SPEEDTREE_COMMON_INCLUDED
