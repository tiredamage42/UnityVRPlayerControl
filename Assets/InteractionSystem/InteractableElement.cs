using UnityEngine;
namespace InteractionSystem {
    public class InteractableElement : MonoBehaviour
    {
        [HideInInspector] public Interactable interactable;
        void Awake () {
            if (Application.isPlaying) {
                gameObject.layer = LayerMask.NameToLayer(Interactable.interactableLayerName);
                GetComponent<Collider>().isTrigger = true;
                Rigidbody rb = gameObject.AddComponent <Rigidbody>();
                rb.isKinematic = true;
                rb.useGravity = false;
            }
        }
    }
}