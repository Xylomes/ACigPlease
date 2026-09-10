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

    public static bool IsInputInverted { get; set; } = false;

    public float SensitivityMultiplier { get; set; } = 1f;

    public void ResetCameraAngle()
    {
        xRotation = 0f;
        if (cameraTransform != null)
        {
            cameraTransform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }

    void Start()
    {
        PlayerInput lPlayerInput = GetComponent<PlayerInput>();
        lookAction = lPlayerInput.actions[LOOKACTION];
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
        Vector2 lMouseDelta = lookAction.ReadValue<Vector2>();
        float lInvertMultiplier = IsInputInverted ? -1f : 1f;
        float lEffectiveSpeed = rotationSpeed * SensitivityMultiplier;
        xRotation -= lMouseDelta.y * lEffectiveSpeed * lInvertMultiplier;
        xRotation = Mathf.Clamp(xRotation, maxLookAngleDown, maxLookAngleUp);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        transform.Rotate(Vector3.up, lMouseDelta.x * lEffectiveSpeed * lInvertMultiplier);
    }
}
