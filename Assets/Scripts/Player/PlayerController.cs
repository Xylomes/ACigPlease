using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;
    public PlayerStateMachine stateMachine { get; private set; }
    public GrabSystem grabSystem { get; private set; }

    public static bool IsMovementInverted { get; set; } = false;

    /// <summary>True while the player is holding the Interact key (regardless of Hold interaction timing).</summary>
    public bool IsInteractHeld => interactAction != null && interactAction.IsPressed();

    [SerializeField] private CharacterController player;
    [SerializeField] private float moveSpeed;
    [SerializeField] private Transform cameraTransform;

    private PlayerInput playerInput;        
    private InputAction moveAction;           
    private InputAction crouchAction;
    private InputAction interactAction;
    private InputAction nextTextAction;
    private InputAction grabAction;

    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        stateMachine = new PlayerStateMachine(player, transform, moveSpeed, cameraTransform);

        playerInput = GetComponent<PlayerInput>();
        grabSystem = GetComponent<GrabSystem>();
        moveAction = playerInput.actions["Move"];
        crouchAction = playerInput.actions["Crouch"];
        interactAction = playerInput.actions["Interact"];
        nextTextAction = playerInput.actions["Jump"];
        grabAction = playerInput.actions["Grab"];


        interactAction.performed += OnInteractPerformed;
        nextTextAction.performed += OnNextTextPressed;


    }

    private void Update()
    {
        Vector2 lInput = moveAction.ReadValue<Vector2>();
        if (PlayerController.IsMovementInverted)
        {
            lInput = -lInput;
        }
        bool lCrouchPressed = crouchAction.IsPressed();
        bool lGrabPressed = grabAction.IsPressed();
        stateMachine.SetInfos(lInput, lCrouchPressed);
        grabSystem.SetGrabInfos(lGrabPressed);
        stateMachine.Update();
    }

    private void OnInteractPerformed(InputAction.CallbackContext pContext)
    {
        if(!DialogueManager.Instance.isInDialogue)
        {
            InteractionSystem.Instance.TryInteract();
        }
        else return;
        
    }
    private void OnNextTextPressed(InputAction.CallbackContext pContext)
    {
        if (DialogueManager.Instance.isInDialogue)
        {
            DialogueManager.Instance.ContinueDialogue();
        }
        else return;
    }


    private void OnDestroy()
    {
        interactAction.performed -= OnInteractPerformed;
        nextTextAction.performed -= OnNextTextPressed;
    }
}
