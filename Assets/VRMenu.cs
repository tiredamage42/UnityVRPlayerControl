using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VRPlayer {

    /*
        Add to a worldspace canvas to make it follow and face the player
    */
    public class VRMenu : MonoBehaviour
    {

        
        public bool matchXRotation;
        public float distanceFromHead = 1;
        public float followSpeed = 5;
        public float yOffset = 1;

        Transform baseTransform;


        void FollowCameraPosition (float deltaTime, Transform hmd) {
            transform.localPosition = new Vector3(0, yOffset, distanceFromHead);
            baseTransform.position = Vector3.Lerp(baseTransform.position, hmd.position, deltaTime * followSpeed);
            
            
            Vector3 targetRot = hmd.rotation.eulerAngles;

            if (!matchXRotation) {
                targetRot.x = 0;
            }
            targetRot.z = 0;

            baseTransform.rotation = Quaternion.Slerp(baseTransform.rotation, Quaternion.Euler(targetRot), deltaTime * followSpeed);
        }

        void Awake () {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas.renderMode != RenderMode.WorldSpace) {
                Debug.LogError(name + " :: when using vr menu, canvas should be world space");
            }

            baseTransform = new GameObject(name + "_ui_base").transform;
            transform.SetParent(baseTransform);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        // Update is called once per frame
        void Update()
        {
            FollowCameraPosition (Time.deltaTime, Player.instance.hmdTransform);
        }
    }

}