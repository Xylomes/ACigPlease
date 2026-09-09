using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class PlayerStateMachine
{
    public enum PlayerState
    {
        Idle,
        Walking,
        Running,
        Falling,
        Crouching,
        Working,
        Interacting,
        Sleeping,
        Listening
    }

    public static bool CanInteract;
    public static bool IsHoldingItem;
    public static PlayerState CurrentState;

    private CharacterController characterController;
    private Transform playerTransform;
    private Transform cameraTransform;
    
    private float moveSpeed;
    private float verticalVelocity;
    private Vector3 moveDirection;
    private bool crouch;


    private float gravity = -9.81f;
    private float runMuiltiplier = 2f;

    private Vector3 cameraStandLocalPos;
    private Vector3 cameraCrouchLocalPos;
    private float crouchLerpSpeed = 8f;
    public PlayerStateMachine(CharacterController characterController, Transform transform, float speed, Transform cameraTransform)
    {
        this.characterController = characterController;
        this.playerTransform = transform;
        this.moveSpeed = speed;
        this.cameraTransform = cameraTransform;

        cameraStandLocalPos = cameraTransform.localPosition;
        cameraCrouchLocalPos = cameraStandLocalPos - new Vector3(0f, cameraStandLocalPos.y * 0.5f, 0f);

        ChangeState(PlayerState.Idle);
    }

    public void SetInfos(Vector2 inputDirection,bool crouchPressed)
    {
        moveDirection = inputDirection;
        crouch = crouchPressed;
    }

    public void Update()
    {
        //Debug.Log("Current State: " + CurrentState);
        ApplyGravity();
        updateCrouchCamera();

        switch (CurrentState)
        {
            case PlayerState.Idle:
                DoIdle();
                break;
            case PlayerState.Walking:
                DoWalking();
                break;
            case PlayerState.Falling:
                DoFalling();
                break;
            case PlayerState.Crouching:
                DoCrouching();
                break;
            case PlayerState.Listening:
                DoListening();
                break;
        }
    }

    public void ChangeState(PlayerState newState)
    {
        OnExit(CurrentState);
        CurrentState = newState;
        OnEnter(CurrentState);
    }
    private void OnEnter(PlayerState newState)
    {
        switch (newState)
        {
            case PlayerState.Idle:
                CanInteract = true;
                break;
            case PlayerState.Walking:
                CanInteract = true;
                break;
            case PlayerState.Falling:
                CanInteract = false;
                break;
            case PlayerState.Crouching:
                CanInteract = false;
                break;
            case PlayerState.Listening:
                CanInteract = false;
                break;

        }
    }
    private void OnExit(PlayerState newState)
    {
        switch (newState)
        {
            case PlayerState.Idle:
                break;
            case PlayerState.Walking:
                break;
            case PlayerState.Falling:
                break;
            case PlayerState.Crouching:
                break;
            case PlayerState.Listening:
                break;

        }
    }

    private void DoIdle()
    {
        if(moveDirection.magnitude > 0.1f)
        {
            ChangeState(PlayerState.Walking);
        }
        else if(crouch)
        {
            ChangeState(PlayerState.Crouching);
        }
        else if(!characterController.isGrounded)
        {
            ChangeState(PlayerState.Falling);
        }
        else
        {
            CanInteract = true;
            Vector3 move = Vector3.up * verticalVelocity;
            characterController.Move(move * Time.deltaTime);
        }
    }
    private void DoWalking()
    {
        float speed = moveSpeed;

        if (moveDirection.magnitude < 0.1f)
        {
            ChangeState(PlayerState.Idle);
        }
        else if(crouch)
        {
            ChangeState(PlayerState.Crouching);
        }
        else if (!characterController.isGrounded)
        {
            ChangeState(PlayerState.Falling);
        }
        else
        {
            CanInteract = true;
            if (Input.GetKey(KeyCode.LeftShift))
            {
                speed = moveSpeed * runMuiltiplier; 
            }
            else
            {
                speed = moveSpeed;
            }
            Vector3 move = new Vector3(moveDirection.x, 0, moveDirection.y) ;
            Vector3 direction = playerTransform.TransformDirection(move);
            Vector3 motion = direction.normalized * speed + Vector3.up * verticalVelocity;
            characterController.Move(motion * Time.deltaTime);
        }
    }   
    private void DoFalling()
    {
        if (characterController.isGrounded)
        {
            if(characterController.velocity.magnitude > 0.1f)
            {
                ChangeState(PlayerState.Walking);
            }
            else
                ChangeState(PlayerState.Idle);
        }
        else
        {
            Vector3 move = new Vector3(0, verticalVelocity, 0);
            characterController.Move(move * Time.deltaTime);
        }
    }
     private void DoCrouching()
    {
        if (!crouch)
        {
            ChangeState(PlayerState.Idle);
            return;
        }

        Vector3 move = new Vector3(moveDirection.x, 0, moveDirection.y);
        Vector3 direction = playerTransform.TransformDirection(move);
        Vector3 motion = direction.normalized * (moveSpeed * 0.5f) + Vector3.up * verticalVelocity;
        characterController.Move(motion * Time.deltaTime);

        ChangeState(PlayerState.Idle);
    }
     private void DoSleeping()
    {
    }
     private void DoListening()
    {
        if (characterController.isGrounded)
        {
            Vector3 move = new Vector3(0, verticalVelocity, 0);
            characterController.Move(move * Time.deltaTime);
        }
    }

    public void EnterListening()
    {
        ChangeState(PlayerState.Listening);
    }

    public void ExitInteracting()
    {
        ChangeState(PlayerState.Idle);
    }

    private void ApplyGravity()
    {
        if (characterController.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
    }

    private void updateCrouchCamera()
    {
        Vector3 target = crouch ? cameraCrouchLocalPos : cameraStandLocalPos;
        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, target, Time.deltaTime * crouchLerpSpeed);
    }
}

