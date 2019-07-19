using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCharacterController : MonoBehaviour
{
    

    
    public float jumpSpeed = 5.0f;
    public float gravityModifier = 1.0f;
    public float maxGravityVelocity = 2;
    public float moemntumDecaySpeed = 1;
    public LayerMask groundMask;
    public float ungroundedSpeedMultiplier = .25f;
    // [HideInInspector] 
    public bool useRawMovement;
    // [HideInInspector] 
    public bool isGrounded;
    // [HideInInspector] 
    public float floorY = 0;
    public bool isMoving {
        get {
            return inputMoveVector != Vector3.zero;
        }
    }
    Vector3 momentum;
    const float buffer = .1f;
    Vector3 inputMoveVector;
    CharacterController characterController;
    

    public void SetInputMoveVector(Vector3 inputMoveVector) {
        this.inputMoveVector = inputMoveVector;
        
    }
    

    void Awake ( ) {
        characterController = GetComponent<CharacterController>();
    }

    Ray GetGroundCheckRay () {
        //make sure we're checking from the actual capsule position (could be offset due to height or movement)        
        
        Vector3 rayCheckPos = transform.position + (transform.rotation * characterController.center);
        rayCheckPos.y -= ((characterController.height * .5f) + characterController.skinWidth) - buffer;
        return new Ray( rayCheckPos, Vector3.down );
    }

    void CheckGrounded () {

        Ray ray = GetGroundCheckRay();
        
        bool wasGrounded = isGrounded;
        isGrounded = false;
        RaycastHit hit;
        if (momentum.y <= 0 && Physics.Raycast(ray, out hit, buffer + (wasGrounded && (momentum.y <= 0) ? .25f : .05f), groundMask)) {
            isGrounded = true;
            momentum = Vector3.zero;
            momentum.y = -maxGravityVelocity;
            floorY = hit.point.y;
        }
    }

    public Vector3 GetFloor() {

        Ray ray = GetGroundCheckRay();
          
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1, groundMask)) {
            return hit.point;
        }
        return ray.origin;

    }


    

    public void SetMomentum (Vector3 velocity) {
        momentum = velocity;
    }
    public void Jump () {
        momentum.y = jumpSpeed;
        Vector3 localVelocity = transform.InverseTransformDirection(characterController.velocity);
        momentum.x = localVelocity.x;
        momentum.z = localVelocity.z;
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

    public void MoveCharacterController (float deltaTime, Transform relativeTransform=null) {


        Vector3 moveVector = inputMoveVector;

        if (!useRawMovement) {

            if (!isGrounded) {
                moveVector *= ungroundedSpeedMultiplier;
            }

            moveVector += momentum;
            moveVector *= deltaTime;
        }

        if (relativeTransform == null) {
            relativeTransform = transform;
        }

        characterController.Move( relativeTransform.TransformDirection( moveVector ) );
    
    }



}
