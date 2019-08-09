using UnityEngine;
public class RoomCuller : Culler
{
    void OnTriggerExit (Collider other) {
        if (other.gameObject.CompareTag(playerTag)) {
            Vector3 lPos = transform.InverseTransformPoint(other.transform.position);
            bool enabled = lPos.z > 0;
            if (state == enabled)
                return;
            EnableObjects (enabled);

        }
    }
}
