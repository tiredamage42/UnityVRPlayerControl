using UnityEngine;
public class UnparentOnAwake : MonoBehaviour
{
    void Awake () {
        transform.SetParent(null);
    }
}
