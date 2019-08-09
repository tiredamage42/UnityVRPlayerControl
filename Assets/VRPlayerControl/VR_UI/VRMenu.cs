using UnityEngine;

namespace VRPlayer {

    /*
        Add to a worldspace canvas to make it follow and face the player
    */
    public class VRMenu : MonoBehaviour
    {
        public float angleThreshold = 45;
        public bool matchXRotation;
        public float followSpeed = 5;
        // public Vector3 offset = new Vector3(0,0.5f,1);
        // public Vector3 menuScale = Vector3.one;
        Transform baseTransform, hmdTransform;
        public TransformBehavior followBehavior;


        Vector3 rotationTarget, lastRotationFWD = Vector3.forward;

        private void OnEnable() {
            lastRotationFWD = hmdTransform.forward;
            rotationTarget = hmdTransform.rotation.eulerAngles;
            rotationTarget.z = 0;
            
            if (!matchXRotation)
                rotationTarget.x = 0;
        }

        void FollowCameraPosition (float deltaTime) {


            TransformBehavior.AdjustTransform(transform, baseTransform, followBehavior, 0);
            // transform.localScale = menuScale;
            // transform.localPosition = offset;

            
            baseTransform.position = Vector3.Lerp(baseTransform.position, hmdTransform.position, deltaTime * followSpeed);

            // Vector3 targetRot = baseTransform.rotation.eulerAngles;
            if (Vector3.Angle(hmdTransform.forward, lastRotationFWD) > angleThreshold) {
                lastRotationFWD = hmdTransform.forward;
                rotationTarget = hmdTransform.rotation.eulerAngles;
                rotationTarget.z = 0;
                if (!matchXRotation)
                    rotationTarget.x = 0;
            }
                
            baseTransform.rotation = Quaternion.Slerp(baseTransform.rotation, Quaternion.Euler(rotationTarget), deltaTime * followSpeed);
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