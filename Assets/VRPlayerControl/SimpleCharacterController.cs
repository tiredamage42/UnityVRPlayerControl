using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCharacterController : MonoBehaviour
{
    

    CharacterController characterController;
    

    public float jumpSpeed = 5.0f;
    public float gravityModifier = 1.0f;
    public float maxGravityVelocity = 2;

    Vector2 currentMoveVector;

    public bool isMoving {
        get {
            return currentMoveVector != Vector2.zero;
        }
    }

    public void SetMoveVector(Vector2 currentMoveVector) {
        this.currentMoveVector = currentMoveVector;
    }
    public LayerMask groundMask;


    public bool isGrounded;

    const float buffer = .1f;

    void Awake ( ) {
        characterController = GetComponent<CharacterController>();
    }

    void CheckGrounded () {

        Vector3 myPos = transform.position;
        Vector3 charControllerOffset = characterController.center;

        Vector3 rayCheck = myPos + charControllerOffset;
        rayCheck.y = myPos.y + buffer;
        bool wasGrounded = isGrounded;

        isGrounded = false;
        Ray ray = new Ray (rayCheck, Vector3.down);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, buffer + (wasGrounded ? .25f : .01f), groundMask)) {
            isGrounded = true;
        }
    }




    float yVelocity;
    public void Jump () {
        yVelocity = jumpSpeed;
    }


    void FixedUpdate () {
        CheckGrounded();
        UpdateGravity(Time.fixedDeltaTime);
    }


    void UpdateGravity (float deltaTime) {
        
        yVelocity += Physics.gravity.y * gravityModifier * deltaTime;
        if (yVelocity < -maxGravityVelocity) {
            yVelocity = -maxGravityVelocity;
        }
    }


    public void MoveCharacterController (float deltaTime, Transform relativeTransform=null) {

        if (relativeTransform == null) {
            relativeTransform = transform;
        }

        characterController.Move( relativeTransform.TransformDirection( 
            new Vector3( currentMoveVector.x, yVelocity, currentMoveVector.y) * deltaTime ) );
    }



}
