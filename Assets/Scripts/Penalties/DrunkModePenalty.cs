using UnityEngine;

public class DrunkModePenalty : MonoBehaviour, IPenalty
{
    [Header("Drunk Camera Wobble")]
    [SerializeField] private float wobbleSpeed = 2f;
    [SerializeField] private float wobbleAmplitude = 3f;

    private const float DEFAULT_ROTATION_SPEED = 2f;
    private const float INVERTED_MULTIPLIER = -1f;

    public PenaltyType PenaltyType => PenaltyType.DrunkMode;

    private Transform cameraTransform;
    private float wobbleTimer;

    public void Activate()
    {
        // Invert player movement and look inputs
        PlayerRotation.IsInputInverted = true;
        PlayerController.IsMovementInverted = true;

        cameraTransform = PlayerController.Instance != null
            ? PlayerController.Instance.transform.GetChild(0)
            : Camera.main?.transform;
    }

    public void Deactivate()
    {
        PlayerRotation.IsInputInverted = false;
        PlayerController.IsMovementInverted = false;
    }

    private void LateUpdate()
    {
        if (PlayerRotation.IsInputInverted && cameraTransform != null)
        {
            wobbleTimer += Time.deltaTime * wobbleSpeed;
            float zTilt = Mathf.Sin(wobbleTimer) * wobbleAmplitude;
            cameraTransform.localRotation = cameraTransform.localRotation * Quaternion.Euler(0, 0, zTilt);
        }
    }
}
