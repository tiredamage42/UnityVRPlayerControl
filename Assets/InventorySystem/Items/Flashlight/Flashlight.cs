// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;
using InventorySystem;
public class Flashlight : MonoBehaviour, ISceneItem
{
    public void OnEquipped(Inventory inventory) {
        EnableLight(true);
    }
    public void OnUnequipped(Inventory inventory) {
        EnableLight(false);

    }
    public void OnEquippedUpdate(Inventory inventory) {

    }
    public void OnEquippedUseStart(Inventory inventory, int useIndex) {
        EnableLight(!lightEnabled);
    }
    public void OnEquippedUseEnd(Inventory inventory, int useIndex) {

    }
    public void OnEquippedUseUpdate(Inventory inventory, int useIndex) {

    }

    Light light;
    bool lightEnabled;

    void Awake () {
        light = GetComponent<Light>();
        light.enabled = false;
    }
    
    public void EnableLight (bool enabled) {
        if (light.enabled != enabled) {
            light.enabled = enabled;
            lightEnabled = enabled;
        }
    }

    
}
