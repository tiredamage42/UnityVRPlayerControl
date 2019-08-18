using UnityEngine;

using Game.InventorySystem;
public class Flashlight : MonoBehaviour, ISceneItem
{
    
    // this is specific to our flashlight model
    const int glassSharedMaterialIndex = 1;

    Material glassFlashlightOnMaterial, originalGlassMaterial;

    public Color lightColor = Color.white;


    public void OnEquipped(Inventory inventory) {
        EnableLight(true);
    }
    public void OnUnequipped(Inventory inventory) {
        EnableLight(false);
    }
    public void OnEquippedUpdate(Inventory inventory) {

    }
    public void OnEquippedUseStart(Inventory inventory, int useIndex) {
        if (useIndex == 0) {
            EnableLight(!lightEnabled);
        }
    }
    public void OnEquippedUseEnd(Inventory inventory, int useIndex) {

    }
    public void OnEquippedUseUpdate(Inventory inventory, int useIndex) {

    }

    Light lightComponent;
    bool lightEnabled;

    MeshRenderer meshRenderer;
    void Awake () {
        lightComponent = GetComponentInChildren<Light>();
        lightComponent.enabled = false;

        meshRenderer = GetComponentInChildren<MeshRenderer>();
        originalGlassMaterial = meshRenderer.sharedMaterials[glassSharedMaterialIndex];
        glassFlashlightOnMaterial = new Material(originalGlassMaterial);
        glassFlashlightOnMaterial.EnableKeyword("_EMISSION");
    }

    void SetGlassMaterial (Material material) {
        Material[] sharedMaterials = meshRenderer.sharedMaterials;
        sharedMaterials[glassSharedMaterialIndex] = material;
        meshRenderer.sharedMaterials = sharedMaterials;
    }

    public void EnableLight (bool enabled) {
        if (lightComponent.enabled != enabled) {
            lightComponent.enabled = enabled;

            if (enabled) {
                lightComponent.color = lightColor;
                glassFlashlightOnMaterial.SetColor("_EmissionColor", lightColor);
            }

            SetGlassMaterial( enabled ? glassFlashlightOnMaterial : originalGlassMaterial);

            lightEnabled = enabled;
        }
    }    
}
