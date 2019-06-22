using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

namespace VRPlayer 
{
/*
    allow multiple hand hovers on interactables
*/




/*

motion sickness avoid:

    avoid repeated patterns in environment (checkerboard tiles, etc..)
    
    dont move camera wihtout player input

    remove accelerations in movement

    make 3d hud (so eyes dont have to refocus from 2d to 3d)

    look up walk in place solutions 
    
    https://www.youtube.com/watch?v=IQ_jgnwHwFA
    http://smirkingcat.software/ripmotion/
    https://cubic9.com/Devel/OculusRift/BobbingBots_en/ 
    
    https://www.youtube.com/watch?v=Ti6kxSDTI8g (walking with hand movements (grab and drag))
    (takes 3 frames to go up step)

    follow redirected walking

    choose rotation when teleporting

    teleport grenade?

    walkabout method:
        reach edge, and hold button (blurs image and i guess stops tracking)
        turn around in meatspace while camera stays

    climb up walls, click and drag (done)

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

    bool[] climbables = new bool[2];
    public bool isClimbing;
    int currentClimbHand;
    public Vector3 climbExitForceMultiplier = new Vector3(10, 100, 10);
    
    int Hand2Index(SteamVR_Input_Sources hand) 
    {
        return hand == SteamVR_Input_Sources.LeftHand ? 1 : 0;
    }
    SteamVR_Input_Sources Index2Hand (int index) {
        return index == 0 ? SteamVR_Input_Sources.RightHand : SteamVR_Input_Sources.LeftHand;
    }
    int OtherHand (int hand) {
        return hand == 0 ? 1 : 0;
    }
    // SteamVR_Input_Sources OtherHand(SteamVR_Input_Sources hand) {
    //     return Index2Hand(OtherHand(Hand2Index(hand)));
    // }


    void SetHandClimb (int climbHand) {
        isClimbing = true;
        currentClimbHand = climbHand;
        previousHandPosition = Player.instance.GetHand(Index2Hand(currentClimbHand)).transform.localPosition;
    }

    
    void UpdateClimbs () {
        InteractionSystem.Interactor interactor = GetComponent<InteractionSystem.Interactor>();
        climbables[0] = interactor.HasTag("ClimbRight");
        climbables[1] = interactor.HasTag("ClimbLeft");
        
        bool justLetGo = false;

        // foreach (var hand in climbableChecks.Keys) {
        for (int hand = 0; hand < 2; hand++) {


            // bool isClimbable = climbableChecks[hand].isClimbable;
            bool isClimbable = climbables[hand];

            // if we just clicked to start climbing, make this the primary climb hand
            if (isClimbable && climbAction.GetStateDown(Index2Hand(hand))) {
                SetHandClimb(hand);
            }

            //if this is the current climbing hand
            if (isClimbing && currentClimbHand == hand) {

                // if we've let go to stop climbing
                if (!isClimbable || climbAction.GetStateUp(Index2Hand(hand))) {

                    isClimbing = false;
                    justLetGo = true;

                    //check if our other hand is climbable and gripping (so we make that the primary one again)
                    int otherHand = OtherHand(hand);

                    if (climbables[otherHand] && climbAction.GetState(Index2Hand(otherHand))) {
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

            Vector3 handLocalPosition = Player.instance.GetHand(Index2Hand(currentClimbHand)).transform.localPosition;
            
            moveScript.SetInputMoveVector(previousHandPosition - handLocalPosition);
            previousHandPosition = handLocalPosition;

        }
        else {
            if (justLetGo) {
                Vector3 momentum = previousHandPosition - (Player.instance.GetHand(Index2Hand(currentClimbHand)).transform.localPosition);
                moveScript.SetMomentum(momentum.MultiplyBy(climbExitForceMultiplier)
                    
                );
            }
        }
    }

    Vector3 previousHandPosition;
        

    


    public SteamVR_Action_Boolean climbAction;


    public Color crouchVignetteColor = new Color(1, .5f, 0, 1);

    public bool easyCrouch = true;

    // 1.0f / 1.8f = crouch height of 1 for default height of 1.8
    [Range(0,1)] public float crouchRatioThreshold = 1.0f / 1.8f;
    
    // this doenst count the resizing offset
    // so the actual physical act of crouching wont be difficult for taller than
    // default height players
    bool GetManualCrouching () {
        return head.transform.localPosition.y / VRManager.playerRealLifeHeight <= crouchRatioThreshold;
    }

    float currentCrouchLerp;
    public float crouchTime = .25f;
    void HandleEasyCrouch (float deltaTime) {

        float target = isCrouched ? 1 : 0;

        if (currentCrouchLerp != target) {
            currentCrouchLerp += deltaTime * (1.0f/crouchTime) * (target == 0 ? -1 : 1);
            currentCrouchLerp = Mathf.Clamp01(currentCrouchLerp);
            
            float startHeight = VRManager.gamespaceHeight;
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
    void SetCharacterControllerHeight (){
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

    bool isCrouched;
    

    void CheckForJump (bool movementEnabled) {
        if (moveScript.isGrounded && movementEnabled) {
            if (!StandardizedVRInput.ActionOccupied(jumpAction, moveHand) && jumpAction.GetStateDown(moveHand)) {
                moveScript.Jump();
            }
        }
    }

    void CheckCrouched (float deltaTime) {
        if (!easyCrouch) {
            Player.instance.extraHeightOffset = 0;
            isCrouched = GetManualCrouching();
            return;
        }

        if (!VRManager.gamePaused && !StandardizedVRInput.ActionOccupied(crouchAction, moveHand) && !Player.instance.handsTogether) {

            if (crouchAction.GetStateDown(moveHand)) {
                isCrouched = !isCrouched;
            }
        }

        HandleEasyCrouch(deltaTime);
    
    }


    // any movement above this angle with forward movement cant run.
    const float runAngleThreshold = 45f;     
    
    public Transform head;
    public float capsuleRadius = .25f;
    

    [Header("Controls")]
    
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

        // InitializeClimbableChecks();
    }

    void Update() {
        InputUpdateLoop(Time.deltaTime);
    }
    void FixedUpdate () {
        UpdateClimbs();
        FixedUpdateLoop(Time.fixedDeltaTime);
    }


    
    void HandleComfortVignetting (bool vignetteEnabled, float deltaTime) {
        float targetIntensity = vignetteEnabled ? vignetteIntensity : 0.0f;
        currentVignetteIntensity = Mathf.Lerp(currentVignetteIntensity, targetIntensity, deltaTime * vignetteSpeed);

        vignette.SetIntensity( currentVignetteIntensity );
        vignette.SetColor(isCrouched ? crouchVignetteColor : Color.black);
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
    void HandleMoveInput (bool movementEnabled) {
        currentMoveVector = Vector2.zero;
        
        if (!movementEnabled) {
            isRunning = false;
            return;
        }
        currentMoveVector = StandardizedVRInput.ActionOccupied(moveAction, moveHand) ? Vector2.zero : moveAction.GetAxis(moveHand);

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
        // return currentMoveVector;
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
    Vector2 currentMoveVector;
        
     

    void InputUpdateLoop (float deltaTime) {
        CheckCrouched(deltaTime);

        bool movementEnabled = !VRManager.gamePaused && !isClimbing;
        
        CheckForJump(movementEnabled); // also when jump hand isnt hovering

        HandleTurnInput(movementEnabled);
        HandleMoveInput(movementEnabled);
        if (!isClimbing) {
            moveScript.SetInputMoveVector(new Vector3 (currentMoveVector.x, 0, currentMoveVector.y));
        }

        bool enableComfortVignette = smoothTurnDirection != 0 || !moveScript.isGrounded || isCrouched;
        if (!enableComfortVignette) {
            enableComfortVignette = moveScript.isMoving && currentMoveVector.sqrMagnitude >= vignetteMovementThreshold * vignetteMovementThreshold;
        }
        HandleComfortVignetting( enableComfortVignette, deltaTime );
    }
            
        void FixedUpdateLoop (float deltaTime) {
            HandleTurningUpdate(deltaTime);
            SetCharacterControllerHeight ();
            moveScript.MoveCharacterController(deltaTime, transform);
        }
    }

}


