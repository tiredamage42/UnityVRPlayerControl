using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineController : MonoBehaviour
{
    
    // [System.Serializable]
    // public class OutlineData
    // {
    //     public Color color = Color.white;
    //     public HighlightsFX.SortingType depthType;
    //     public Renderer renderer;
    // }


    // public HighlightsFX outlinePostEffect;
    // public OutlineData[] outliners;

    public List<Renderer> renderers = new List<Renderer>();

    private void Start()
    {
        // foreach (var obj in outliners)
        // {
            HighlightsFX.instance.AddRenderers(renderers, Color.red, HighlightsFX.SortingType.DepthFiltered );
            // outlinePostEffect.AddRenderers(
            //     new List<Renderer>() { obj.renderer }, 
            //     obj.color, 
            //     obj.depthType);
        // }
    }
}
