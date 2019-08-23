using UnityEngine;

namespace VRPlayer {

    /*
        Add to a worldspace canvas to make it follow and face the player
    */

    [System.Serializable] public class VRMenuFollowerParameters {
        public float angleThreshold = 45;
        public bool matchXRotation;
        public float followSpeed = 20;
        public float turnSpeed = 20;
        public VRMenuFollowerParameters () {
            angleThreshold = 45;
            followSpeed = 20;
            turnSpeed = 20;
        }
    }
    [RequireComponent(typeof(Canvas))]
    public class VRMenu : MonoBehaviour
    {
        public VRMenuFollowerParameters parameters = new VRMenuFollowerParameters();
        Transform baseTransform, hmdTransform;
        public TransformBehavior followBehavior;


        

        float currentYRotation;
        void FollowCameraPosition (float deltaTime) {


            TransformBehavior.AdjustTransform(transform, baseTransform, followBehavior, 0);
            
            baseTransform.position = Vector3.Lerp(baseTransform.position, hmdTransform.position, deltaTime * parameters.followSpeed);

            Vector3 headEuler = hmdTransform.rotation.eulerAngles;

            float yRotation = headEuler.y;
            if (Mathf.Abs(yRotation - currentYRotation) >= parameters.angleThreshold) {
                int yGrid = (int)(yRotation / parameters.angleThreshold);
                float ySnap = ((yRotation % parameters.angleThreshold) / parameters.angleThreshold) < .5f ? yGrid : yGrid + 1;
                ySnap *= parameters.angleThreshold;

                currentYRotation = ySnap;
            }

            Quaternion rotationTarget = Quaternion.Euler(parameters.matchXRotation ? headEuler.x : 0, currentYRotation, 0);
                                
            baseTransform.rotation = Quaternion.Slerp(baseTransform.rotation, (rotationTarget), deltaTime * parameters.turnSpeed);
        }

        void Awake () {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas.renderMode != RenderMode.WorldSpace) {
                Debug.LogError(name + " :: when using vr menu, canvas should be world space");
            }

            baseTransform = new GameObject(name + "_ui_base").transform;
            transform.ResetAtParent(baseTransform);

            hmdTransform = VRManager.instance.hmdTransform;
        }

        void Update()
        {
            FollowCameraPosition (Time.deltaTime);
        }
    }
}