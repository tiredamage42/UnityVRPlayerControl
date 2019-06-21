using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LooseTransform : MonoBehaviour
{

    public float speed = 5;

    Transform lockTransform;
    // Start is called before the first frame update
    void Awake()
    {   
        lockTransform = new GameObject(name + " lock transform").transform;
    }

    void FollowLock (float deltaTime) {
        transform.position = Vector3.Lerp(transform.position, lockTransform.position, deltaTime * speed);
        transform.rotation = Quaternion.Slerp(transform.rotation, lockTransform.rotation, deltaTime * speed);
    }

    public void SetParent (Transform transform, Vector3 localPosition, Quaternion localRotation) {
        lockTransform.SetParent(transform);
        lockTransform.localPosition = localPosition;
        lockTransform.localPosition = localPosition;
        
    }
    public void SetParent (Transform transform, Vector3 localPosition, Vector3 rotation) {
        SetParent(transform, localPosition, Quaternion.Euler(rotation));
    }
    public void SetParent (Transform transform) {
        SetParent(transform, Vector3.zero, Quaternion.identity);
    }
    

    // Update is called once per frame
    void Update()
    {
        FollowLock(Time.deltaTime);
    }
}
