using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace InteractionSystem {


// [ExecuteInEditMode]
public class InteractableElement : MonoBehaviour
{
    // public const string interactableLayerName = "InteractableTrigger";
    // public static int interactTriggerMask {
    //     get {
    //         return 1 << LayerMask.NameToLayer(interactableLayerName); 
    //     }
    // }
    [HideInInspector] public Interactable interactable;


    void OnEnable () {

        // if (GetComponent<Collider>() != null)
        //     return;



        // BoxCollider bc = transform.parent.GetComponent<BoxCollider>();
        // if (bc != null) {
        //     BoxCollider b = gameObject.AddComponent<BoxCollider>();
        //     b.size = bc.size;
        //     b.center = bc.center;
        //     b.isTrigger = true;
        // }
        // else {
        //     MeshCollider mc = transform.parent.GetComponent<MeshCollider>();
        //     if (mc != null) {
        //         MeshCollider m = gameObject.AddComponent<MeshCollider>();
        //         m.sharedMesh = mc.sharedMesh;
        //         m.convex = true;
        //         m.isTrigger = true;
        //     }
        
        // }

        
 
    }

    public void SetInteractable (Interactable interactable) {
        this.interactable = interactable;
    }
    void Awake () {
        if (Application.isPlaying) {

            gameObject.layer = LayerMask.NameToLayer(Interactable.interactableLayerName);
            GetComponent<Collider>().isTrigger = true;

            // Rigidbody rb = GetComponent<Rigidbody>();
            Rigidbody rb = gameObject.AddComponent <Rigidbody>();
            
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }
}

}