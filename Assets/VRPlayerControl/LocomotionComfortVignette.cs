using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRPlayer.Locomotion {
    public class LocomotionComfortVignette : MonoBehaviour
    {

        public float vignetteIntensity = 2.0f;
        public float vignetteSpeed = 10;
        public float vignetteMovementThreshold = 1;
        
        VignettingVR vignette;
        float currentVignetteIntensity;

        TouchpadLocomotion locomotionScript;
        SimpleCharacterController moveScript;

        void Awake () {
            locomotionScript = GetComponent<TouchpadLocomotion>();
            moveScript = GetComponent<SimpleCharacterController>();
            
            vignette = VRManager.instance.GetComponentInChildren<VignettingVR>();
        }
        

        void Update () {
            HandleComfortVignetting( Time.deltaTime );
        }

        void HandleComfortVignetting (float deltaTime) {
            bool enableComfortVignette = locomotionScript.smoothTurnDirection != 0 || !moveScript.isGrounded || locomotionScript.isCrouched;
            enableComfortVignette = enableComfortVignette || moveScript.isMoving && (locomotionScript.currentMoveVector.sqrMagnitude >= vignetteMovementThreshold * vignetteMovementThreshold);
            
            float targetIntensity = enableComfortVignette ? vignetteIntensity : 0.0f;
            currentVignetteIntensity = Mathf.Lerp(currentVignetteIntensity, targetIntensity, deltaTime * vignetteSpeed);

            vignette.SetIntensity( currentVignetteIntensity );
            vignette.SetColor(Color.black);//isCrouched ? crouchVignetteColor : Color.black);
        }


        




    }
}
