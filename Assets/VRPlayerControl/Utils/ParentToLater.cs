using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentToLater : MonoBehaviour
{
    public Transform toParent;
    public Vector3 position, rotation;
    

    bool parented;
    // Update is called once per frame
    void Update()
    {
        if (!parented && transform.parent != toParent) {
            transform.SetParent(toParent);
            transform.localPosition = position;
            transform.localRotation = Quaternion.Euler(rotation);
            parented = true;
        }
    }
}
