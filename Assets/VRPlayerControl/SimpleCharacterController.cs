﻿using System.Collections;
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
    public float floorY = 0;
    void Awake ( ) {
        characterController = GetComponent<CharacterController>();
    }

    void CheckGrounded () {

        //make sure we're checking from the actual capsule position (could be offset due to height or movement)        
        Vector3 charControllerOffset = characterController.center;
        Vector3 rayCheck = transform.position + characterController.center;
        rayCheck.y -= ((characterController.height * .5f) + characterController.skinWidth) - buffer;// myPos.y + buffer;

        bool wasGrounded = isGrounded;

        isGrounded = false;
        Ray ray = new Ray (rayCheck, Vector3.down);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, buffer + (wasGrounded ? .25f : .05f), groundMask)) {
            isGrounded = true;
            momentum = Vector3.zero;
            floorY = hit.point.y;
        }
    }


    public float moemntumDecaySpeed = 1;

    Vector3 momentum;


    public void SetMomentum (Vector3 velocity) {
        momentum = velocity;
    }




    // float yVelocity;
    public void Jump () {
        momentum.y = jumpSpeed;
        momentum.x = characterController.velocity.x;
        momentum.x = characterController.velocity.z;
        
        
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


    public void MoveCharacterController (float deltaTime, Transform relativeTransform=null) {

        if (relativeTransform == null) {
            relativeTransform = transform;
        }

        characterController.Move( relativeTransform.TransformDirection( 
                new Vector3( 
                    currentMoveVector.x + momentum.x, 
                    momentum.y, 
                    currentMoveVector.y + momentum.z
                ) * deltaTime 
            ) 
        );
    }



}