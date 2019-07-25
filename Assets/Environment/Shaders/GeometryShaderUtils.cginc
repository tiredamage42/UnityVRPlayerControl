// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

#ifndef GEOMETRY_SHADER_UTILS_INCLUDED
#define GEOMETRY_SHADER_UTILS_INCLUDED

#include "UnityCG.cginc"

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


#define CAMFWD i_camFwd
#define CAMDIR i_camDir
#define CAMDIST i_camDist


#define CALCULATE_CAMERA_VARIABLES(rootPos) \
    float3 CAMFWD = mul((float3x3)unity_CameraToWorld, float3(0,0,1)); \
    CAMFWD.y = 0; \
    CAMFWD = normalize(CAMFWD); \
    fixed3 CAMDIR = rootPos - _WorldSpaceCameraPos; \
    fixed CAMDIST = length(CAMDIR); \
    CAMDIR.y = 0; \
    CAMDIR = normalize(CAMDIR);
    

fixed InverseLerp (fixed2 range, fixed value) {
    return min(max(value - range.x, 0) / (range.y - range.x), 1);
}
    

#endif // SPEEDTREE_COMMON_INCLUDED
