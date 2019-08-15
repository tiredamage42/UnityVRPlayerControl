using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;


using GameBase;

namespace VRPlayer 
{

/*

    motion sickness avoid:

        avoid repeated patterns in environment (checkerboard tiles, etc..)
    
        dont move camera wihtout player input

        remove accelerations in movement

        make 3d hud (so eyes dont have to refocus from 2d to 3d)

        look up walk in place solutions :
    
            https://www.youtube.com/watch?v=IQ_jgnwHwFA
            http://smirkingcat.software/ripmotion/
            https://cubic9.com/Devel/OculusRift/BobbingBots_en/ 
            
            https://www.youtube.com/watch?v=Ti6kxSDTI8g (walking with hand movements (grab and drag))
            (takes 3 frames to go up step)

            follow redirected walking
    
            walkabout method:
                reach edge, and hold button (blurs image and i guess stops tracking)
                turn around in meatspace while camera stays

        a decrease in FPS (frames per second) animation of camera motion to 10-15 FPS, 
        while the world is rendered at a frequency of 90 Hz, may help. 
        In this case, the brain does not recognize the movement as such and, 
        therefore, dissonance is avoided.
            
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
    
        add nose (or simulated) in between eyes (static frame of reference helps motion sickness)
        
        xray transparent shader when camera clips trhoug object (character controller avoids this, but still should test out)
*/

/*    
    TODO:
    choose rotation when teleporting

    teleport grenade?

    raise FOV during teleport

*/


/*
    make camera rig child of this transform (at origin)
    this moves the play area around with the character controller
    rotates the play area around the camera when rotating
*/

public class TouchpadLocomotion : MonoBehaviour
{

    bool[] climbables = new bool[2];
    public bool isClimbing;
    SteamVR_Input_Sources currentClimbHand;
    public Vector3 climbExitForceMultiplier = new Vector3(10, 100, 10);

    InteractionSystem.Interactor interactor;

    
    void SetHandClimb (SteamVR_Input_Sources climbHand) {
        isClimbing = true;
        currentClimbHand = climbHand;
        previousHandPosition = Player.instance.GetHand(climbHand).transform.localPosition;
    }

    
    void UpdateClimbs () {
        // InteractionSystem.Interactor interactor = GetComponent<InteractionSystem.Interactor>();
        climbables[0] = interactor.HasTag("ClimbRight");
        climbables[1] = interactor.HasTag("ClimbLeft");
        
        bool justLetGo = false;

        // foreach (var hand in climbableChecks.Keys) {
        for (int hand = 0; hand < 2; hand++) {
            SteamVR_Input_Sources handVR = VRManager.Int2Hand(hand);


            // bool isClimbable = climbableChecks[hand].isClimbable;
            bool isClimbable = climbables[hand];

            // if we just clicked to start climbing, make this the primary climb hand
            if (isClimbable && climbAction.GetStateDown(handVR)) {
                SetHandClimb(handVR);
            }

            //if this is the current climbing hand
            if (isClimbing && currentClimbHand == handVR) {

                // if we've let go to stop climbing
                if (!isClimbable || climbAction.GetStateUp(handVR)) {

                    isClimbing = false;
                    justLetGo = true;

                    //check if our other hand is climbable and gripping (so we make that the primary one again)
                    SteamVR_Input_Sources otherHand = VRManager.OtherHand(handVR);

                    if (climbables[VRManager.Hand2Int(otherHand)] && climbAction.GetState(otherHand)) {
                        justLetGo = false;

                        SetHandClimb(otherHand);
                    }
                }
            }
        }

        /*
            Assumes the open equip button is the side button
            as well as teh teleport button
        */
        Teleport.instance.teleportationAllowed = !isClimbing && !Player.instance.handsTogether;

        moveScript.useRawMovement = isClimbing;
        if (isClimbing) {

            Vector3 handLocalPosition = Player.instance.GetHand(currentClimbHand).transform.localPosition;
            
            moveScript.SetInputMoveVector(previousHandPosition - handLocalPosition);
            previousHandPosition = handLocalPosition;

        }
        else {
            if (justLetGo) {
                Vector3 momentum = previousHandPosition - (Player.instance.GetHand(currentClimbHand).transform.localPosition);
                moveScript.SetMomentum(momentum.MultiplyBy(climbExitForceMultiplier)
                    
                );
            }
        }
    }

    Vector3 previousHandPosition;
        
    public SteamVR_Action_Boolean climbAction;
    public bool easyCrouch = true;

    // 1.0f / 1.8f = crouch height of 1 for default height of 1.8
    [Range(0,1)] public float crouchRatioThreshold = 1.0f / 1.8f;
    
    // this doenst count the resizing offset
    // so the actual physical act of crouching wont be difficult for taller than
    // default height players
    bool GetManualCrouching (Transform head) {
        return head.localPosition.y / Player.instance.realLifeHeight <= crouchRatioThreshold;
    }

    float currentCrouchLerp;
    public float crouchTime = .25f;
    void HandleEasyCrouch (float deltaTime) {

        float target = isCrouched ? 1 : 0;

        if (currentCrouchLerp != target) {
            currentCrouchLerp += deltaTime * (1.0f/crouchTime) * (target == 0 ? -1 : 1);
            currentCrouchLerp = Mathf.Clamp01(currentCrouchLerp);
            
            float startHeight = Player.instance.gamespaceHeight;
            float crouchOffsetTarget = (startHeight * crouchRatioThreshold) - startHeight;

            Player.instance.extraHeightOffset = Mathf.Lerp(0, crouchOffsetTarget, currentCrouchLerp); 

            if (moveScript.isGrounded) {
                //mae sure this happens after any teleportation
                //keep our transform flush with the ground to prevent jittering
                Player.instance.KeepTransformFlushWithGround(moveScript.floorY);
                
            }
        }
    }


    const float minCapsuleHeight = .1f;

    // in order to achieve height changes we'll offset the entire play area in order to keep
    // consisten heights between players / accessibility / crouching
    // this is done by offsetting the character controller capsules center and height
    // so the tracking origin can sit below or above the virtual game floor
    void SetCharacterControllerHeight (Transform head){
        float radius = capsuleRadius;
        characterController.radius = radius;

        float height = head.localPosition.y;


        //adjust for crouch + resizing player
        float heightOffset = Player.instance.totalHeightOffset;
        
        height += heightOffset;

        if (height <= minCapsuleHeight) {
            height = minCapsuleHeight;
        }

        characterController.height = height;        

        // push the cc up on the center, so when we're crouched, our trackign origin transform can dip
        // below the floor, that way the camera is closer to the floor (giving hte illusion of crouched stance)

        // if resizing and player is shorter in real life than default player height,
        // the bottom of the capsule should protrude out from below our tracking origin
        // pushing it up
        
        Vector3 newCenter = new Vector3(0, (height * .5f) - heightOffset, 0);
        newCenter.y += characterController.skinWidth;

        //move capsule in local space to adjust for headset moving around on its own
        newCenter.x = head.localPosition.x;
        newCenter.z = head.localPosition.z;

        //rotate
        // newCenter = Quaternion.Euler(0, -transform.eulerAngles.y, 0) * newCenter;
        
        characterController.center = newCenter;        
    }

    [HideInInspector] public bool isCrouched;
    
    void CheckForJump (bool movementEnabled) {
        if (moveScript.isGrounded && movementEnabled) {
            if (jumpAction.GetStateDown(moveHand)) {
                if (!StandardizedVRInput.ActionOccupied(jumpAction, moveHand)) {
                    moveScript.Jump();
                }
            }
        }
    }

    void CheckCrouched (float deltaTime, Transform head) {
        if (!easyCrouch) {
            Player.instance.extraHeightOffset = 0;
            isCrouched = GetManualCrouching(head);
            return;
        }

        if (!GameManager.isPaused && !StandardizedVRInput.ActionOccupied(crouchAction, moveHand) && !Player.instance.handsTogether) {

            if (crouchAction.GetStateDown(moveHand)) {
                isCrouched = !isCrouched;
            }
        }

        HandleEasyCrouch(deltaTime);
    
    }


    // any movement above this angle with forward movement cant run.
    const float runAngleThreshold = 45f;     
    public float capsuleRadius = .25f;
    

    [Header("Controls")]
    
    public SteamVR_Action_Vector2 moveAction;
    public SteamVR_Action_Boolean runAction, jumpAction, crouchAction;
    
    public SteamVR_Action_Boolean turnActionLeft, turnActionRight;

    public SteamVR_Input_Sources moveHand { get { return VRManager.instance.offHand; } }
    public SteamVR_Input_Sources turnHand { get { return VRManager.instance.mainHand; } }
    

    [Header("Movement")]
    SimpleCharacterController moveScript;
    public float maxSpeed = 1.0f;
    public enum RunMode { None, Held, Clicked };
    public RunMode runMode = RunMode.Clicked;
    public float runMultiplier = 3;

    bool isRunning;
    CharacterController characterController;

    public enum TurnType { None, Instant, Fast, Smooth };
    [Header("Turning")]
    public TurnType turnType = TurnType.Fast;
    public float turnDegrees = 20f;    
    public float fastTurnTime = .1f;

    public float smoothTurnSpeedAcceleration = 20;
    public float smoothTurnSpeed = 300;

    float fastTurnDegreesPerSecond { get { return turnDegrees / fastTurnTime; } }
    [HideInInspector] public int smoothTurnDirection;
    float fastTurnTotalRotation = float.MaxValue;
    float smoothTurnMultiplier;

    void Awake () {

        interactor = GetComponent<InteractionSystem.Interactor>();

        characterController = GetComponent<CharacterController>();
        if (characterController == null) characterController = gameObject.AddComponent<CharacterController>();
        
        moveScript = GetComponent<SimpleCharacterController>();
    }

    void Update() {
        InputUpdateLoop(VRManager.instance.hmdTransform, Time.deltaTime);
    }
    void FixedUpdate () {
        UpdateClimbs();
        FixedUpdateLoop(VRManager.instance.hmdTransform, Time.fixedDeltaTime);
    }

    void DoInstantTurn (Transform head, float degrees, bool toRight) {
        transform.RotateAround(head.position, Vector3.up, toRight ? degrees : -degrees);
    }

    void HandleFastTurn (Transform head, float targetDegrees, float degreesPerSecond, float deltaTime) {
    
        //keep turning if we're not there yet
        if(Mathf.Abs(fastTurnTotalRotation) < targetDegrees) {   
            float rotationAdd = degreesPerSecond * deltaTime * smoothTurnDirection;//  (toRight ? 1 : -1);
            transform.RotateAround(head.position, Vector3.up, rotationAdd);
            fastTurnTotalRotation += rotationAdd;
        }
        else {
            smoothTurnDirection = 0;
        }
    }
    
    void HandleSmoothTurn (Transform head, int turn, float deltaTime) {
        smoothTurnMultiplier = Mathf.Lerp(smoothTurnMultiplier, turn, deltaTime * smoothTurnSpeedAcceleration);
        if (smoothTurnMultiplier != 0) {
            transform.RotateAround(head.position, Vector3.up, smoothTurnMultiplier * smoothTurnSpeed * deltaTime);
        }
    }


    void HandleTurnInput (Transform head, bool movementEnabled) {

        if (turnType != TurnType.Fast) {

            smoothTurnDirection = 0;
        }
     
        if (!movementEnabled)
            return;

        switch(turnType) {
            case TurnType.None:
                break;
            case TurnType.Instant:
                if (turnActionLeft.GetStateDown(turnHand)) {
                    DoInstantTurn(head, turnDegrees, false);
                }
                else if (turnActionRight.GetStateDown(turnHand)) {
                    DoInstantTurn(head, turnDegrees, true);
                }
                break;
            case TurnType.Fast:            
                if (turnActionLeft.GetStateDown(turnHand)) {
                    smoothTurnDirection = -1;
                    fastTurnTotalRotation = 0;
                }
                else if (turnActionRight.GetStateDown(turnHand)) {
                    smoothTurnDirection = 1;
                    fastTurnTotalRotation = 0;
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
    void HandleMoveInput (bool movementEnabled) {
        // currentMoveVector = Vector2.zero;
        
        // if (!movementEnabled) {
        //     isRunning = false;
        //     return;
        // }
        currentMoveVector = !movementEnabled || StandardizedVRInput.ActionOccupied(moveAction, moveHand) ? Vector2.zero : moveAction.GetAxis(moveHand);
        if (currentMoveVector == Vector2.zero)
        {
            isRunning = false;
            return;   
        }
        // if (currentMoveVector != Vector2.zero) {

        if (currentMoveVector.sqrMagnitude > 1) {
            currentMoveVector.Normalize();
        }

        if (runMode != RunMode.None) {

            // we're used to walking forward, lateral and backwards/up down movement feels wrong
            // so it's speed needs to be clamped to prevent motion sickness
            bool runPossible = Vector2.Angle(currentMoveVector, new Vector2(0, 1)) <= runAngleThreshold;
            if (!runPossible) {
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
        // }
    }

    void HandleTurningUpdate(Transform head, float deltaTime) {
        switch(turnType) {
            case TurnType.None: break;
            case TurnType.Instant: break;
            case TurnType.Fast:
                HandleFastTurn(head, turnDegrees, fastTurnDegreesPerSecond, deltaTime);
                break;
            case TurnType.Smooth:
                HandleSmoothTurn (head, smoothTurnDirection, deltaTime);
                break;
        }
    }
    [HideInInspector] public Vector2 currentMoveVector;
        
     

    void InputUpdateLoop (Transform head, float deltaTime) {
        CheckCrouched(deltaTime, head);

        bool movementEnabled = !GameManager.isPaused && !isClimbing;
        
        CheckForJump(movementEnabled && !VRUIInput.HandOccupied(moveHand)); // also when jump hand isnt hovering

        HandleTurnInput(head, movementEnabled && !VRUIInput.HandOccupied(turnHand));
        HandleMoveInput(movementEnabled && !VRUIInput.HandOccupied(moveHand));
        if (!isClimbing) {
            moveScript.SetInputMoveVector(new Vector3 (currentMoveVector.x, 0, currentMoveVector.y));
        }
    }
            
        void FixedUpdateLoop (Transform head, float deltaTime) {
            HandleTurningUpdate(head, deltaTime);
            SetCharacterControllerHeight (head);

            if (VRManager.headsetIsOnPlayerHead) {

                moveScript.MoveCharacterController(deltaTime, transform);
            }
        }
    }

}


