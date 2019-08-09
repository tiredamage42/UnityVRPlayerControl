using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Culler : MonoBehaviour
{
    
    
    public GameObject[] objectsHandled;
    BoxCollider box;

    protected const string playerTag = "Player";

    protected void EnableObjects (bool enabled) {
        
        for (int i = 0; i < objectsHandled.Length; i++) {
            objectsHandled[i].SetActive(enabled);
        }
        state = enabled;
    }
    protected bool state;

    void Awake () {
        box = GetComponent<BoxCollider>();
        box.isTrigger = true;
        EnableObjects(false);
    }
    
    void OnDrawGizmos () {
        if (box == null) {
            box = GetComponent<BoxCollider>();
            box.isTrigger = true;
        }
            

        Gizmos.color = new Color(0,0,1,.25f);
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
        Gizmos.DrawCube(box.center, box.size);
        Gizmos.matrix = oldMatrix;
    }   
}