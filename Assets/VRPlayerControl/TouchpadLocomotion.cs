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

    public float comfortVignetteIntensity = 1.0f;

    // public float turnSpeed = 5;
    // public float deadZone = .1f;

    public SteamVR_Action_Boolean moveEnableAction;
    public SteamVR_Action_Vector2 moveAction;

    public SteamVR_Action_Boolean turnActionLeft, turnActionRight;
    public SteamVR_Action_Boolean runAction;

    
    public SteamVR_Input_Sources moveHand;
    public SteamVR_Input_Sources turnHand;
    
    // public SteamVR_Action_Vector2 trackpackAxisAction;
    
    
    // public float sensitivity = 0.1f;
    public float maxSpeed = 1.0f;

    // public Transform cameraRig, head;
    public Transform head;
    
    // float currentSpeed;
    CharacterController characterController;


    // public bool method1 = true;

    
    // Start is called before the first frame update
    // void Start()
    // {
        // cameraRig = SteamVR_Render.Top().origin;
        // head = SteamVR_Render.Top().head;
    // }

    // Update is called once per frame
    void Update()
    {

        // if (method1) {
            // Method1Update ();
        // }

        // else {
            // Method2Update(Time.deltaTime);
        // }

        InputUpdateLoop();
    }



    void FixedUpdate () {
        FixedUpdateLoop(Time.fixedDeltaTime);
    }


    void Awake () {

        Method1Initialize();
        // if (method1) {
        // }
        // else {
        //     Method2Initialize();
        // }

    }


    void Method1Initialize () {

        characterController = GetComponent<CharacterController>();
        if (characterController == null) {
            characterController = gameObject.AddComponent<CharacterController>();
        }
    }


    /*
    */
    

    public float turnDegrees = 45f;    

    public float fastTurnTime = .25f;

    float fastTurnDegreesPerSecond {
        get {
            return turnDegrees / fastTurnTime;
        }
    }
    

    // makes sure head doesnt rotate with cc
    // float currentTurnSpeed;
    public float smoothTurnSpeedAcceleration = 5;
    public float smoothTurnSpeed = 40;

    public enum TurnType {
        None, Instant, Fast, Smooth
    };

    public TurnType turnType;


    void DoInstantTurn (float degrees, bool toRight) {
        transform.RotateAround(head.position, Vector3.up, toRight ? degrees : -degrees);
    }


    float fastTurnTotalRotation = float.MaxValue;
    bool fastTurnRight;
    void DoFastTurn (bool toRight) {
        fastTurnTotalRotation = 0;
        fastTurnRight = toRight;
    }

    bool HandleFastTurn (float targetDegrees, float degreesPerSecond, bool toRight, float deltaTime) {
        
        //keep turning if we're not there yet
        if(Mathf.Abs(fastTurnTotalRotation) < targetDegrees) {
            
            float rotationAdd = degreesPerSecond * deltaTime * (toRight ? 1 : -1);
            transform.RotateAround(head.position, Vector3.up, rotationAdd);
            fastTurnTotalRotation += rotationAdd;

            return true;
        }
        return false;
    }

    float smoothTurnMultiplier;

    bool HandleSmoothTurn (int turn, float deltaTime) {
        smoothTurnMultiplier = Mathf.Lerp(smoothTurnMultiplier, turn, deltaTime * smoothTurnSpeedAcceleration);
        if (smoothTurnMultiplier != 0) {
            transform.RotateAround(head.position, Vector3.up, smoothTurnMultiplier * smoothTurnSpeed * deltaTime);
            return true;


        }
        return false;
    }

    int smoothTurnDirection;

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

    public float runMultiplier = 2;

    bool isRunning;

    // any movement above this angle with forward movement cant run.
    const float runAngleThreshold = 45f; 

    public enum RunMode { None, Held, Clicked };
    public RunMode runMode;

    Vector2 currentMoveVector;


    // set move velocity to zero every frame for instant stop
    // when not moving
    void HandleMoveInput (bool movementEnabled) {
        currentMoveVector = Vector2.zero;
        
        if (!movementEnabled) {
            isRunning = false;
            return;
        }
        currentMoveVector = moveAction.GetAxis(moveHand);

        if (currentMoveVector != Vector2.zero) {

            if (currentMoveVector.sqrMagnitude > 1) {
                Debug.LogError("NORMALIZING");
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
                    
                    if (!isRunning) {
                        
                    }

                }
            }

        }





        currentMoveVector = currentMoveVector * maxSpeed * (isRunning ? runMultiplier : 1);
    }

        


    bool HandleTurningUpdate(float deltaTime) {
        switch(turnType) {
            case TurnType.None: break;
            case TurnType.Instant: break;
            case TurnType.Fast:
                return HandleFastTurn(turnDegrees, fastTurnDegreesPerSecond, fastTurnRight, deltaTime);
            case TurnType.Smooth:
                return HandleSmoothTurn (smoothTurnDirection, deltaTime);
        }
        return false;
    }
        
        /*
        


    void HandleHead (float deltaTime) {
        //store current rotation
        Vector3 oldCameraRigPosition = head.position;
        // Quaternion oldCameraRigRotation = head.rotation;





        bool isTurning = false;
        bool left = false;
        if (moveEnableAction.GetState(lookHand)) {
            float axis = trackpackAxisAction.GetAxis(lookHand).x;
            if (Mathf.Abs(axis) >= deadZone) {
                isTurning = true;
                left = axis < 0;

                
                // transform.position = new Vector3(oldCameraRigPosition.x, transform.position.y, oldCameraRigPosition.z);
                // transform.Rotate(0, axis * turnSpeed * deltaTime, 0);
            }
        }
        currentTurnSpeed = Mathf.Lerp(currentTurnSpeed, isTurning ? (left ?  -1 : 1) : 0, deltaTime * turnSpeedAccelSpeed);
        if (currentTurnSpeed != 0) {
            transform.RotateAround(oldCameraRigPosition, Vector3.up, currentTurnSpeed * turnSpeed * deltaTime);
        }
        else {
            // transform.rotation = Quaternion.Euler( new Vector3(0, oldCameraRigRotation.eulerAngles.y, 0) );
        }

        
        // if (isTurning) {

        //     head.position = oldCameraRigPosition;
        // }

        // if (!isTurning) {

        //     head.rotation = oldCameraRigRotation;
        // }
    }
     */


    // void CalculateMovement () {
    //     // figure out movement orientation
    //     Vector3 orientationEuler = new Vector3(0, transform.rotation.eulerAngles.y, 0);
    //     Quaternion orientation = Quaternion.Euler(orientationEuler);
    //     Vector3 movement = Vector3.zero;


    //     if (moveEnableAction.GetStateUp(moveHand)) {
    //         currentSpeed = 0;
    //     }

    //     if (moveEnableAction.GetState(moveHand)) {
    //         currentSpeed += trackpackAxisAction.GetAxis(moveHand).y * sensitivity;
    //         currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed, maxSpeed);


    //         movement += orientation * (currentSpeed * Vector3.forward);
    //     }

    //     characterController.Move(movement);
    // }

    void InputUpdateLoop () {

        bool movementEnabled = moveEnableAction.GetState(moveHand);
        HandleTurnInput(movementEnabled);
        HandleMoveInput(movementEnabled);
    }
    void FixedUpdateLoop (float deltaTime) {
        bool isTurning = HandleTurningUpdate(deltaTime);
        HandleCapsuleHeight();
        MoveCharacterController(deltaTime);

        bool isMoving = currentMoveVector != Vector2.zero;

        if (isTurning || isMoving) {
            SetComfortVignette(isTurning || isMoving ? comfortVignetteIntensity : 0);
        }
    }

    void SetComfortVignette(float intensity) {

        VignettingVR vignetting = head.GetComponentInChildren<VignettingVR>();
        vignetting.vignettingIntensity = intensity;
        
    }

    void MoveCharacterController (float deltaTime) {


        Vector3 velocity = new Vector3( currentMoveVector.x, 0, currentMoveVector.y);

        characterController.Move( transform.TransformDirection( velocity * deltaTime ) );
    }



    void HandleCapsuleHeight () {
        float headHeight = Mathf.Clamp(head.localPosition.y, 1, 2);
        SetCharacterControllerHeight (headHeight, .25f);
    }

    


    void SetCharacterControllerHeight (float height, float radius) {
        if (characterController == null)
            return;


        characterController.radius = radius;
        
        characterController.height = height;
        
        Vector3 newCenter = new Vector3(0, height * .5f, 0);
        newCenter.y += characterController.skinWidth;


        //move capsul in local space to adjust for headset moving around on its own
        newCenter.x = head.localPosition.x;
        newCenter.z = head.localPosition.z;

        //rotate
        // newCenter = Quaternion.Euler(0, -transform.eulerAngles.y, 0) * newCenter;
        
        characterController.center = newCenter;
        
    }

    // void Method1Update () {
        // HandleHead();    
        
        // HandleCapsuleHeight();
        // CalculateMovement();
    // }


    // public Transform handTransform;


    /*
        Now we set up our scene, add a Rigidbody and capsule collider to your [cameraRig] component, 
        make sure that you constrain all rotation on the rigidbody so you don’t find yourself falling over. 
        For the capsule collider all you need to set is the radius since all the other values are controlled by our code, I set mine to 0.1.

        Now we can start to code. Create a new script called movement and add it to your [CameraRig] component. Here Is the code I used:
    */

    
    //  public static float Angle(Vector2 p_vector2)
    //  {
    //      if (p_vector2.x < 0)
    //      {
    //          return 360 - (Mathf.Atan2(p_vector2.x, p_vector2.y) * Mathf.Rad2Deg * -1);
    //      }
    //      else
    //      {
    //          return Mathf.Atan2(p_vector2.x, p_vector2.y) * Mathf.Rad2Deg;
    //      }
    //  }





    // void Method2Update(float deltaTime)
    // {
    //     HandleHead(deltaTime);

    //     HandleCapsuleHeight();

    //     Vector2 currentTrackpad = trackpackAxisAction.GetAxis(moveHand);
        
        
    //     //Set size and position of the capsule collider so it maches our head.
    //     // characterController.height = head.localPosition.y;
    //     // characterController.center = new Vector3(head.localPosition.x, head.localPosition.y / 2, head.localPosition.z);
        

    //     //get the angle of the touch and correct it for the rotation of the controller
    //     // Vector3 moveDirection = Quaternion.AngleAxis(Angle(currentTrackpad) + handTransform.localRotation.eulerAngles.y, Vector3.up) * Vector3.forward;
        


    //     // if (GetComponent<Rigidbody>().velocity.magnitude < speed && trackpad.magnitude >Deadzone){//make sure the touch isn't in the deadzone and we aren't going to fast.
    //     //     GetComponent<Rigidbody().AddForce(moveDirection * 30);
    //     // }



    //     // Vector3 velocity = new Vector3(0,0,0);
    //     if (currentTrackpad.magnitude > deadZone)
    //     {
    //         //make sure the touch isn't in the deadzone and we aren't going to fast.
    //         // CapCollider.material = NoFrictionMaterial;
            
    //         // velocity = currentTrackpad;// moveDirection;
            
    //         // if (JumpAction.GetStateDown(MovementHand) && GroundCount > 0)
    //         // {
    //         //     float jumpSpeed = Mathf.Sqrt(2 * jumpHeight * 9.81f);
    //         //     RBody.AddForce(0, jumpSpeed, 0, ForceMode.VelocityChange);
    //         // }
    //         currentTrackpad = currentTrackpad * maxSpeed * deltaTime;

    //         Vector3 velocityNew = new Vector3( 
    //             currentTrackpad.x,// * maxSpeed,// - characterController.velocity.x, 
    //             0, 
    //             currentTrackpad.y// * maxSpeed //- characterController.velocity.z 
    //         );


    //         characterController.Move(
    //             // maybe head
    //             transform
    //                 .TransformDirection( velocityNew ) 
    //         );

    //         // RBody.AddForce(velocity.x*MovementSpeed - RBody.velocity.x, 0, velocity.z*MovementSpeed - RBody.velocity.z, ForceMode.VelocityChange);
    //         // Debug.Log("Velocity" + velocity);
    //         // Debug.Log("Movement Direction:" + moveDirection);
    //     }
    //     // else if(GroundCount > 0)
    //     // {
    //     //     CapCollider.material = FrictionMaterial;
    //     // }
    // }
}



}


