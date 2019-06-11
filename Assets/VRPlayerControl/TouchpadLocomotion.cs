using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

namespace VRPlayer {





/*

motion sickness avoid:


    avoid repeated patterns in environment (checkerboard tiles, etc..)

    
    dont move camera wihtout player input

    remove accelerations in movement

    make 3d hud (so eyes dont have to refocus from 2d to 3d)

    look up walk in place solutions (https://www.youtube.com/watch?v=IQ_jgnwHwFA)
    http://smirkingcat.software/ripmotion/


https://cubic9.com/Devel/OculusRift/BobbingBots_en/ (another bob walk in place solution)




    https://www.youtube.com/watch?v=Ti6kxSDTI8g (walking with hand movements (grab and drag))
    (takes 3 frames to go up step)

    follow redirected walking

    choose rotation when teleporting

    teleport grenade?



    walkabout method:
        reach edge, and hold button (blurs image and i guess stops tracking)
        turn around in meatspace while camera stays




climb up walls, click and drag



a decrease in FPS (frames per second) animation of camera motion to 10-15 FPS, while the world is rendered at a frequency of 90 Hz, may help. 
In this case, the brain does not recognize the movement as such and, therefore, dissonance is avoided.

you could use this technique for jumping. Make the frame at the beginning, middle, and at the top part 
of the trajectory. And, instead of Dash Teleport, only use the starting, middle, and end 
positions in order to make the key animation frames, and just get rid of the rest.




darker colors (80/20) (dark/light)


"grab and turn" https://www.youtube.com/watch?v=oSP5sjg9TIs

maybe blur isntead of vignette


move really fast towards teleport point (tested and works i guess.) maybe up FOV (to give it a smear look)
(hsould take <= 100 ms, thats the time it takes for the brain to get sick)


amplified walking (move tracking space along with camera movement vector so meatspace steps go farther)
 gain walking"

 rectangular area without gain, then add gain around it


    TODO:
        xray transparent shader when camera clips trhoug object

        add nose (or simulated) in between eyes (static frame of reference helps motion sickness)

*/


 
 


/*

    make camera rig child of this transform (at origin)

    this moves the play area around with the character controller

    rotates the play area around the camera when rotating
*/


public class TouchpadLocomotion : MonoBehaviour
{


    bool adjustedScale;

    public float defaultPlayerHeight = 1.8f;
    public float crouchHeight = 1;
    public float crouchTransitionSpeed = 10f;

    bool isCrouched;
    float targetStandScale, targetCrouchScale, currentScaleLerp;
    

    void CheckForInitialScaling () {
        if (!adjustedScale) {
            if (hmdOnHead.GetState(SteamVR_Input_Sources.Head)) {
                CalculateScaleTargets();
                adjustedScale = true;
            }
        }
    }

    void CheckForJump () {
        if (moveScript.isGrounded) {

            if (jumpAction.GetStateDown(moveHand)) {
                isCrouched = false;
                moveScript.Jump();
            }
        }
    }

    void CheckCrouched (float deltaTime) {


        if (crouchAction.GetStateDown(moveHand)) {
            isCrouched = !isCrouched;
        }

        if (!adjustedScale) {
            if (isCrouched) {
                isCrouched = false;
                Debug.LogWarning("cant crouch yet, default player scale not adjusted");
            }
            return;
        }

        currentScaleLerp = Mathf.Lerp(currentScaleLerp, isCrouched ? 0 : 1, crouchTransitionSpeed * deltaTime);        
        float currentScale = Mathf.Lerp(targetCrouchScale, targetStandScale, currentScaleLerp);
        if (transform.lossyScale.x != currentScale) {
            transform.localScale = Vector3.one * currentScale;
        }
    }

    void CalculateScaleTargets () {
        ResetPlayerHeight();
        float naturalHeadHeight = head.localPosition.y;

        targetStandScale = defaultPlayerHeight / naturalHeadHeight;
        targetCrouchScale = crouchHeight / naturalHeadHeight;
    }


    void ResetPlayerHeight () {
        transform.localScale = Vector3.one;
    }


    // any movement above this angle with forward movement cant run.
    const float runAngleThreshold = 45f;     
    
    public Transform head;
    public float capsuleRadius = .25f;
    public Vector2 minMaxHeight = new Vector2(1, 2);



    [Header("Controls")]
    public SteamVR_Action_Boolean hmdOnHead;
    
    public SteamVR_Action_Boolean moveEnableAction;
    public SteamVR_Action_Vector2 moveAction;
    public SteamVR_Action_Boolean runAction, jumpAction, crouchAction;
    


    public SteamVR_Action_Boolean turnActionLeft, turnActionRight;

    public SteamVR_Input_Sources moveHand;
    public SteamVR_Input_Sources turnHand;

    [Header("Movement")]
    SimpleCharacterController moveScript;
    public float maxSpeed = 1.0f;
    public enum RunMode { None, Held, Clicked };
    public RunMode runMode = RunMode.Clicked;
    public float runMultiplier = 3;

    bool isRunning;
    CharacterController characterController;
    // Vector2 currentMoveVector;

    public enum TurnType { None, Instant, Fast, Smooth };
    [Header("Turning")]
    public TurnType turnType = TurnType.Fast;
    public float turnDegrees = 20f;    
    public float fastTurnTime = .1f;

    public float smoothTurnSpeedAcceleration = 20;
    public float smoothTurnSpeed = 300;

    float fastTurnDegreesPerSecond { get { return turnDegrees / fastTurnTime; } }
    int smoothTurnDirection;
    float fastTurnTotalRotation = float.MaxValue;
    bool fastTurnRight;
    float smoothTurnMultiplier;

    [Header("Comfort Vignette")]
    public float vignetteIntensity = 2.0f;
    public float vignetteSpeed = 10;
    public float vignetteMovementThreshold = 1;
    VignettingVR vignette;
    float currentVignetteIntensity;



    void Awake () {

        characterController = GetComponent<CharacterController>();
        if (characterController == null) characterController = gameObject.AddComponent<CharacterController>();
        
        vignette = head.GetComponentInChildren<VignettingVR>();
        moveScript = GetComponent<SimpleCharacterController>();
    }

    void Update() {
        CheckForInitialScaling();
        InputUpdateLoop(Time.deltaTime);
    }
    void FixedUpdate () {

        // UpdateGravity(Time.fixedDeltaTime);
        FixedUpdateLoop(Time.fixedDeltaTime);
    }


    
    void HandleComfortVignetting (bool vignetteEnabled, float deltaTime) {
        float targetIntensity = vignetteEnabled ? vignetteIntensity : 0.0f;
        currentVignetteIntensity = Mathf.Lerp(currentVignetteIntensity, targetIntensity, deltaTime * vignetteSpeed);
        vignette.vignettingIntensity = currentVignetteIntensity;
    }

    void DoInstantTurn (float degrees, bool toRight) {
        transform.RotateAround(head.position, Vector3.up, toRight ? degrees : -degrees);
    }


    void DoFastTurn (bool toRight) {
        fastTurnTotalRotation = 0;
        fastTurnRight = toRight;
    }

    void HandleFastTurn (float targetDegrees, float degreesPerSecond, bool toRight, float deltaTime) {
        //keep turning if we're not there yet
        if(Mathf.Abs(fastTurnTotalRotation) < targetDegrees) {   
            float rotationAdd = degreesPerSecond * deltaTime * (toRight ? 1 : -1);
            transform.RotateAround(head.position, Vector3.up, rotationAdd);
            fastTurnTotalRotation += rotationAdd;
        }
    }
    
    void HandleSmoothTurn (int turn, float deltaTime) {
        smoothTurnMultiplier = Mathf.Lerp(smoothTurnMultiplier, turn, deltaTime * smoothTurnSpeedAcceleration);
        if (smoothTurnMultiplier != 0) {
            transform.RotateAround(head.position, Vector3.up, smoothTurnMultiplier * smoothTurnSpeed * deltaTime);
        }
    }


    void HandleTurnInput (bool movementEnabled) {
        smoothTurnDirection = 0;
     
        if (!movementEnabled)
            return;

        switch(turnType) {
            case TurnType.None:
                break;
            case TurnType.Instant:
                if (turnActionLeft.GetStateDown(turnHand)) {
                    DoInstantTurn(turnDegrees, false);
                }
                else if (turnActionRight.GetStateDown(turnHand)) {
                    DoInstantTurn(turnDegrees, true);
                }
                break;
            case TurnType.Fast:            
                if (turnActionLeft.GetStateDown(turnHand)) {
                    DoFastTurn(false);
                }
                else if (turnActionRight.GetStateDown(turnHand)) {
                    DoFastTurn(true);
                }
                break;
            case TurnType.Smooth:
                if (turnActionLeft.GetState(turnHand)) {
                    smoothTurnDirection = -1;
                }
                else if (turnActionRight.GetState(turnHand)) {
                    smoothTurnDirection = 1;
                }
                break;
        }
    }


    // set move velocity to zero every frame for instant stop
    // when not moving
    Vector2 HandleMoveInput (bool movementEnabled) {
        Vector2 currentMoveVector = Vector2.zero;
        
        if (!movementEnabled) {
            isRunning = false;
            return currentMoveVector;
        }
        currentMoveVector = moveAction.GetAxis(moveHand);

        if (currentMoveVector != Vector2.zero) {

            if (currentMoveVector.sqrMagnitude > 1) {
                currentMoveVector.Normalize();
            }

            if (runMode != RunMode.None) {

                bool runPossible = Vector2.Angle(currentMoveVector, new Vector2(0, 1)) <= runAngleThreshold;
                if (!runPossible) {
                    // we're used to walking forward, lateral and backwards/up down movement feels wrong
                    // so it's speed needs to be clamped to prevent motion sickness
                    isRunning = false;
                }
                else {
                    if (runMode == RunMode.Held) {
                        isRunning = runAction.GetState(moveHand);
                    }
                    else {
                        if (runAction.GetStateDown(moveHand)) {
                            isRunning = true;
                        }
                    }
                }
            }
            currentMoveVector = currentMoveVector * maxSpeed * (isRunning ? runMultiplier : 1);
        }
        return currentMoveVector;
    }

    void HandleTurningUpdate(float deltaTime) {
        switch(turnType) {
            case TurnType.None: break;
            case TurnType.Instant: break;
            case TurnType.Fast:
                HandleFastTurn(turnDegrees, fastTurnDegreesPerSecond, fastTurnRight, deltaTime);
                break;
            case TurnType.Smooth:
                HandleSmoothTurn (smoothTurnDirection, deltaTime);
                break;
        }
    }
        
     

    void InputUpdateLoop (float deltaTime) {
        CheckCrouched(deltaTime);
        CheckForJump();
        

        bool movementEnabled = true;//moveEnableAction.GetState(moveHand);
        HandleTurnInput(movementEnabled);
        Vector2 currentMoveVector = HandleMoveInput(movementEnabled);

        moveScript.SetMoveVector(currentMoveVector);

        bool enableComfortVignette = smoothTurnDirection != 0;
        if (!enableComfortVignette) {
            // bool isMoving = currentMoveVector != Vector2.zero;
            enableComfortVignette = moveScript.isMoving && currentMoveVector.sqrMagnitude >= vignetteMovementThreshold * vignetteMovementThreshold;
        }
        HandleComfortVignetting( enableComfortVignette, deltaTime );
    }
        
    void FixedUpdateLoop (float deltaTime) {
        HandleTurningUpdate(deltaTime);
        SetCharacterControllerHeight (Mathf.Clamp(head.localPosition.y, minMaxHeight.x, minMaxHeight.y), capsuleRadius);
        
        moveScript.MoveCharacterController(deltaTime, transform);
        // MoveCharacterController(deltaTime);        
    }

    // void MoveCharacterController (float deltaTime) {

    //     Transform relativeTransform = transform;

    //     characterController.Move( relativeTransform.TransformDirection( new Vector3( currentMoveVector.x, 0, currentMoveVector.y) * deltaTime ) );
    // }


    void SetCharacterControllerHeight (float height, float radius) {

        characterController.height = height;        
        characterController.radius = radius;
        
        Vector3 newCenter = new Vector3(0, height * .5f, 0);
        newCenter.y += characterController.skinWidth;

        //move capsule in local space to adjust for headset moving around on its own
        newCenter.x = head.localPosition.x;
        newCenter.z = head.localPosition.z;

        //rotate
        // newCenter = Quaternion.Euler(0, -transform.eulerAngles.y, 0) * newCenter;
        
        characterController.center = newCenter;        
    }
}

}


