using UnityEngine;

public class GroundFog : EnvironmentParticles
{
    public void SetRotateSpeed (float rotateSpeed) {
        particleMaterial.SetFloat("_RotateSpeed", rotateSpeed);
    }
    public void SetSoftParticleFactor (float softParticleFactor) {
        particleMaterial.SetFloat("_SoftParticleFactor", softParticleFactor);
    }
    public void SetStartEndFadeRange (float startEndFadeRange) {
        particleMaterial.SetFloat("_StartEndFade", startEndFadeRange);
    }
    public void SetCloseCamRange (Vector2 closeCameRange) {
        particleMaterial.SetVector("_CloseCamRange", closeCameRange);
    }
    public void SetHeightFadeAndSteepness (Vector3 heightFadeAndSteepness) {
        particleMaterial.SetVector("_HeightRange_Steepness", heightFadeAndSteepness);
    }
    void SetMaxTravelDistance (float maxTravelDistance){
        particleMaterial.SetFloat("_MaxTravelDistance", maxTravelDistance);
    }
    
    //ground fog max travel distance depends on grid size since it moves along z axis...
    protected override void OnEnable () {
        base.OnEnable();
        SetMaxTravelDistance (gridSize);
    }

    
    protected override float GetYPositionOffset (){//Vector2Int grid, Vector3 gridCenter) {
        return 0; //standard field height
    }
}
