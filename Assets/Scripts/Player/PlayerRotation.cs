using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRotation : MonoBehaviour
{

    [SerializeField] private CharacterController player;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private float maxLookAngleUp = 60f;
    [SerializeField] private float maxLookAngleDown = -50f;

    private InputAction lookAction;
    private float xRotation = 0f;
    private const string LOOKACTION = "Look";

    /// <summary>When true, look input is inverted (drunk mode penalty).</summary>
    public static bool IsInputInverted { get; set; } = false;

    /// <summary>Multiplier applied to rotation speed (set by options menu). Defaults to 1.</summary>
    public float SensitivityMultiplier { get; set; } = 1f;

    void Start()
    {
        PlayerInput playerInput = GetComponent<PlayerInput>();
        lookAction = playerInput.actions[LOOKACTION];
    }

    void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        Vector2 mouseDelta = lookAction.ReadValue<Vector2>();
        float invertMultiplier = IsInputInverted ? -1f : 1f;
        float effectiveSpeed = rotationSpeed * SensitivityMultiplier;
        xRotation -= mouseDelta.y * effectiveSpeed * invertMultiplier;
        xRotation = Mathf.Clamp(xRotation, maxLookAngleDown, maxLookAngleUp);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        transform.Rotate(Vector3.up, mouseDelta.x * effectiveSpeed * invertMultiplier);
    }
}
