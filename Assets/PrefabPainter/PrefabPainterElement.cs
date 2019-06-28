// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class PrefabPainterElement : ScriptableObject{
    public GameObject[] variations;
    public bool randomX, randomY, randomZ;
    public Vector3 localPositionOffset;
    public Vector3 localRotationOffset;
    public Vector2 sizeRange = Vector2.one;
    public bool randomizeSize;
    public bool alignNormal = true;
}
