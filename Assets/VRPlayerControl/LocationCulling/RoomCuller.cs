using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[RequireComponent(typeof(BoxCollider))]
public class RoomCuller : MonoBehaviour
{
    public GameObject[] objectsHandled;
    
    const string playerTag = "Player";

    void Awake () {
        EnableObjects(false);
    }

    void EnableObjects (bool enabled) {
        
        for (int i = 0; i < objectsHandled.Length; i++) {
            objectsHandled[i].SetActive(enabled);
        }
        state = enabled;
    }
    bool state;

    void OnTriggerExit (Collider other) {
        if (other.gameObject.CompareTag(playerTag)) {
            Vector3 lPos = transform.InverseTransformPoint(other.transform.position);
            bool enabled = lPos.z > 0;
            if (state == enabled)
                return;
            EnableObjects (enabled);

        }
    }


    BoxCollider box;
    void OnDrawGizmos () {
        if (box == null)
            box = GetComponent<BoxCollider>();

        Gizmos.color = new Color(0,0,1,.25f);
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
        Gizmos.DrawCube(box.center, box.size);
        Gizmos.matrix = oldMatrix;
    }
}
