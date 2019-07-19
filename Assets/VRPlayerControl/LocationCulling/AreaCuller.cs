using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [RequireComponent(typeof(BoxCollider))]
public class AreaCuller : Culler
{
    void OnTriggerExit (Collider other) {
        if (other.gameObject.CompareTag(playerTag)) {
            if (!state) return;
            EnableObjects (false);
        }
    }
    void OnTriggerEnter (Collider other) {
        if (other.gameObject.CompareTag(playerTag)) {
            if (state) return;
            EnableObjects (true);
        }
    }
}