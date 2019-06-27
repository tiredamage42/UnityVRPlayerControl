using UnityEngine;

namespace VRPlayer {

    /*
        Add to a worldspace canvas to make it follow and face the player
    */
    public class VRMenu : MonoBehaviour
    {
        public float angleThreshold = 15;
        public bool matchXRotation;
        public float followSpeed = 5;
        public Vector3 offset = new Vector3(0,0.5f,1);
        Transform baseTransform, hmdTransform;

        Vector3 rotationTarget, lastRotationFWD = Vector3.forward;

        private void OnEnable() {
                lastRotationFWD = hmdTransform.forward;
                rotationTarget = hmdTransform.rotation.eulerAngles;
                rotationTarget.z = 0;
                
                if (!matchXRotation)
                    rotationTarget.x = 0;
                
        }

        void FollowCameraPosition (float deltaTime) {
            transform.localPosition = offset;
            
            baseTransform.position = Vector3.Lerp(baseTransform.position, hmdTransform.position, deltaTime * followSpeed);

            // Vector3 targetRot = baseTransform.rotation.eulerAngles;
            if (Vector3.Angle(hmdTransform.forward, lastRotationFWD) > angleThreshold) {
                //Vector3 
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
            hmdTransform = Player.instance.hmdTransform;

        }

        // Update is called once per frame
        void Update()
        {
            FollowCameraPosition (Time.deltaTime);
        }
    }
}