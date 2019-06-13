using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugOutline : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ObjectOutlines.Highlight_Renderer(GetComponent<Renderer>(), 0);

        Debug.Log("Highlighted");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
