// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

namespace VRPlayer{
    public static class Utils
    {
        public static void SetParent(this Transform transform, Transform newParent, Vector3 localPosition, Quaternion localRotation) {
            transform.SetParent(newParent);
            transform.localPosition = localPosition;
            transform.localRotation = localRotation;
        }
        public static void SetParent(this Transform transform, Transform newParent, Vector3 localPosition, Vector3 localRotation) {
            transform.SetParent(newParent, localPosition, Quaternion.Euler(localRotation));
        }
        public static void ResetAtParent(this Transform transform, Transform newParent) {
            transform.SetParent(newParent, Vector3.zero, Quaternion.identity); 
        }
        public static Vector3 MultiplyBy(this Vector3 a, Vector3 b) {
            return new Vector3(
                a.x * b.x, 
                a.y * b.y, 
                a.z * b.z
            );
        }
    }
}
