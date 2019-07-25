#ifndef ENVIRONMENT_INCLUDED
#define ENVIRONMENT_INCLUDED


fixed4 _HueVariation;

void AddHueVariation (inout fixed3 diffuseColor, fixed4 hueVariation) {
    fixed3 shiftedColor = lerp(diffuseColor.rgb, hueVariation.rgb, hueVariation.a);

    fixed maxBase = max(diffuseColor.r, max(diffuseColor.g, diffuseColor.b));
    fixed newMaxBase = max(shiftedColor.r, max(shiftedColor.g, shiftedColor.b));
    // preserve vibrance
    diffuseColor = saturate(shiftedColor * ((maxBase/newMaxBase) * 0.5 + 0.5));
}

#endif // ENVIRONMENT_INCLUDED
