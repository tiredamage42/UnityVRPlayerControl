using UnityEngine;

public class Precipitator : EnvironmentParticles
{
    public float yOffset = 10;
    protected override float GetYPositionOffset () {
        return yOffset;
    }

    public override void SetWindDirection (float windYRotation, float strength) {
        base.SetWindDirection(windYRotation, strength);
        particleRotation.x = -Mathf.Lerp(0, 45, strength);
    }

    public void SetMaxTravelDistance (float maxTravelDistance){
        particleMaterial.SetFloat("_MaxTravelDistance", maxTravelDistance);
    }

    

}
