// using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class PrefabPainterElement : ScriptableObject{
    
    public List<GameObject> variations = new List<GameObject>();
    public Vector3 posOffsetMin, posOffsetMax, rotOffsetMin, rotOffsetMax;
    public Vector2 sizeMultiplierRange = Vector2.one;
    public bool alignNormal = true;
}
