using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;
    public PlayerStateMachine stateMachine { get; private set; }
    public GrabSystem grabSystem { get; private set; }

    /// <summary>When true, movement input is inverted (drunk mode penalty).</summary>
    public static bool IsMovementInverted { get; set; } = false;

    // Player parameters
    [SerializeField] private CharacterController player;
    [SerializeField] private float moveSpeed;

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

        stateMachine = new PlayerStateMachine(player, transform, moveSpeed);

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
        Vector2 input = moveAction.ReadValue<Vector2>();
        if (PlayerController.IsMovementInverted)
        {
            input = -input;
        }
        bool crouchPressed = crouchAction.IsPressed();
        bool grabPressed = grabAction.IsPressed();
        stateMachine.SetInfos(input,crouchPressed);
        grabSystem.SetGrabInfos(grabPressed);
        stateMachine.Update();
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if(!DialogueManager.Instance.isInDialogue)
        {
            InteractionSystem.Instance.TryInteract();
        }
        else return;
        
    }
    private void OnNextTextPressed(InputAction.CallbackContext context)
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
