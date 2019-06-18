using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCharacterController : MonoBehaviour
{
    

    CharacterController characterController;
    

    public float jumpSpeed = 5.0f;
    public float gravityModifier = 1.0f;
    public float maxGravityVelocity = 2;

    public Vector3 currentMoveVector;

    public bool isMoving {
        get {
            return currentMoveVector != Vector3.zero;
        }
    }

    public Vector3 GetFloor() {

        //make sure we're checking from the actual capsule position (could be offset due to height or movement)        
        Vector3 rayCheck = transform.position + (transform.rotation *characterController.center);
        rayCheck.y -= ((characterController.height * .5f) + characterController.skinWidth) - buffer;// myPos.y + buffer;
        Ray ray = new Ray (rayCheck, Vector3.down);
        
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1, groundMask)) {
            return hit.point;
        }
        return rayCheck;

    }

    public void SetMoveVector(Vector3 currentMoveVector) {
        this.currentMoveVector = currentMoveVector;
        
    }
    public LayerMask groundMask;

    public float ungroundedSpeedMultiplier = .25f;


    public bool isGrounded;

    const float buffer = .1f;
    public float floorY = 0;
    void Awake ( ) {
        characterController = GetComponent<CharacterController>();
    }

    void CheckGrounded () {

        //make sure we're checking from the actual capsule position (could be offset due to height or movement)        
        // Vector3 charControllerOffset = characterController.center;
        Vector3 rayCheck = transform.position + (transform.rotation *characterController.center);
        rayCheck.y -= ((characterController.height * .5f) + characterController.skinWidth) - buffer;// myPos.y + buffer;
        Ray ray = new Ray (rayCheck, Vector3.down);
        Debug.DrawRay(ray.origin, ray.direction, Color.cyan, 1);

        bool wasGrounded = isGrounded;
        isGrounded = false;
        RaycastHit hit;
        if (momentum.y <= 0 && Physics.Raycast(ray, out hit, buffer + (wasGrounded && (momentum.y <= 0) ? .25f : .05f), groundMask)) {
            isGrounded = true;
            
            // momentum = new Vector3(0, momentum.y, 0);
            momentum = Vector3.zero;
            
            floorY = hit.point.y;
        }
    }


    public float moemntumDecaySpeed = 1;

    public Vector3 momentum;


    public void SetMomentum (Vector3 velocity) {
        momentum = velocity;
    }

    // float yVelocity;
    public void Jump () {
        momentum.y = jumpSpeed;

        Vector3 localVelocity = transform.InverseTransformDirection(characterController.velocity);
        momentum.x = localVelocity.x;
        momentum.z = localVelocity.z;
        
        
        // yVelocity = jumpSpeed;
    }


    void FixedUpdate () {
        CheckGrounded();
        UpdateGravity(Time.fixedDeltaTime);
    }


    void UpdateGravity (float deltaTime) {
        momentum.x = Mathf.Lerp(momentum.x, 0, deltaTime * moemntumDecaySpeed);
        momentum.z = Mathf.Lerp(momentum.z, 0, deltaTime * moemntumDecaySpeed);
        
        momentum.y += Physics.gravity.y * gravityModifier * deltaTime;
        if (momentum.y < -maxGravityVelocity) {
            momentum.y = -maxGravityVelocity;
        }
    }
    public bool useRawMovement;


    public void MoveCharacterController (float deltaTime, Transform relativeTransform=null) {

        if (relativeTransform == null) {
            relativeTransform = transform;
        }

        Vector3 moveVector = currentMoveVector;

        if (!useRawMovement) {

            if (!isGrounded) {
                moveVector *= ungroundedSpeedMultiplier;
            }
        }

        characterController.Move( relativeTransform.TransformDirection( 
                useRawMovement ? moveVector : (new Vector3( 
                    moveVector.x + momentum.x, 
                    moveVector.y + momentum.y, 
                    moveVector.z + momentum.z
                ) * (useRawMovement ? 1 : deltaTime)) 
            ) 
        );
    }



}
