using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
namespace VRPlayer {


/*
    add this to character controller gameobject


    make camera rig child of this transform (at origin)
*/

    
public class TouchpadLocomotion : MonoBehaviour
{

    public float deadZone = .1f;

    public SteamVR_Action_Vector2 trackpackAxisAction;
    public SteamVR_Action_Boolean moveEnableAction;

    public SteamVR_Input_Sources moveHand;
    
    public float sensitivity = 0.1f;
    public float maxSpeed = 1.0f;

    public Transform cameraRig, head;
    float currentSpeed;
    CharacterController characterController;


    public bool method1 = true;

    
    // Start is called before the first frame update
    void Start()
    {
        // cameraRig = SteamVR_Render.Top().origin;
        // head = SteamVR_Render.Top().head;
    }

    // Update is called once per frame
    void Update()
    {

        if (method1) {
            Method1Update ();
        }

        else {
            Method2Update();
        }

     
        
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


    // makes sure head doesnt rotate with cc

    void HandleHead () {
        //store current rotation
        Vector3 oldCameraRigPosition = head.position;
        Quaternion oldCameraRigRotation = head.rotation;

        transform.rotation = Quaternion.Euler( new Vector3(0, head.rotation.eulerAngles.y, 0) );

        head.position = oldCameraRigPosition;
        head.rotation = oldCameraRigRotation;
    }

    void CalculateMovement () {
        // figure out movement orientation
        Vector3 orientationEuler = new Vector3(0, transform.rotation.eulerAngles.y, 0);
        Quaternion orientation = Quaternion.Euler(orientationEuler);
        Vector3 movement = Vector3.zero;


        if (moveEnableAction.GetStateUp(moveHand)) {
            currentSpeed = 0;
        }

        if (moveEnableAction.GetState(moveHand)) {
            currentSpeed += trackpackAxisAction.GetAxis(moveHand).y * sensitivity;
            currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed, maxSpeed);


            movement += orientation * (currentSpeed * Vector3.forward);
        }

        characterController.Move(movement);


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
        newCenter = Quaternion.Euler(0, -transform.eulerAngles.y, 0) * newCenter;
        
        characterController.center = newCenter;
        
    }

    void Method1Update () {
        HandleHead();    
        HandleCapsuleHeight();
        CalculateMovement();
    }


    public Transform handTransform;


    /*
        Now we set up our scene, add a Rigidbody and capsule collider to your [cameraRig] component, 
        make sure that you constrain all rotation on the rigidbody so you don’t find yourself falling over. 
        For the capsule collider all you need to set is the radius since all the other values are controlled by our code, I set mine to 0.1.

        Now we can start to code. Create a new script called movement and add it to your [CameraRig] component. Here Is the code I used:
    */

    
     public static float Angle(Vector2 p_vector2)
     {
         if (p_vector2.x < 0)
         {
             return 360 - (Mathf.Atan2(p_vector2.x, p_vector2.y) * Mathf.Rad2Deg * -1);
         }
         else
         {
             return Mathf.Atan2(p_vector2.x, p_vector2.y) * Mathf.Rad2Deg;
         }
     }





    void Method2Update()
    {
        Vector2 currentTrackpad = trackpackAxisAction.GetAxis(moveHand);
        
        
        //Set size and position of the capsule collider so it maches our head.
        characterController.height = head.localPosition.y;
        characterController.center = new Vector3(head.localPosition.x, head.localPosition.y / 2, head.localPosition.z);
        

        //get the angle of the touch and correct it for the rotation of the controller
        Vector3 moveDirection = Quaternion.AngleAxis(Angle(currentTrackpad) + handTransform.localRotation.eulerAngles.y, Vector3.up) * Vector3.forward;
        


        // if (GetComponent<Rigidbody>().velocity.magnitude < speed && trackpad.magnitude >Deadzone){//make sure the touch isn't in the deadzone and we aren't going to fast.
        //     GetComponent<Rigidbody().AddForce(moveDirection * 30);
        // }



        Vector3 velocity = new Vector3(0,0,0);
        if (currentTrackpad.magnitude > deadZone)
        {
            //make sure the touch isn't in the deadzone and we aren't going to fast.
            // CapCollider.material = NoFrictionMaterial;
            velocity = currentTrackpad;// moveDirection;
            // if (JumpAction.GetStateDown(MovementHand) && GroundCount > 0)
            // {
            //     float jumpSpeed = Mathf.Sqrt(2 * jumpHeight * 9.81f);
            //     RBody.AddForce(0, jumpSpeed, 0, ForceMode.VelocityChange);
            // }

            Vector3 velocityNew = new Vector3( 
                velocity.x * maxSpeed,// - characterController.velocity.x, 
                0, 
                velocity.y * maxSpeed //- characterController.velocity.z 
            );


            characterController.Move(transform.TransformDirection( velocityNew * Time.deltaTime ) );








            // RBody.AddForce(velocity.x*MovementSpeed - RBody.velocity.x, 0, velocity.z*MovementSpeed - RBody.velocity.z, ForceMode.VelocityChange);
            // Debug.Log("Velocity" + velocity);
            // Debug.Log("Movement Direction:" + moveDirection);
        }
        // else if(GroundCount > 0)
        // {
        //     CapCollider.material = FrictionMaterial;
        // }
    }
}



}
