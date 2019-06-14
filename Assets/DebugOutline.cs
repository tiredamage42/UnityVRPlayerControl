using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugOutline : MonoBehaviour
{
    void Start()
    {
        ObjectOutlines.Highlight_Renderer(GetComponent<Renderer>(), 0);
    }
}
