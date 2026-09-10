using UnityEngine;

public class InvertedControlsPenalty : MonoBehaviour, IPenalty
{
    [SerializeField] private float wobbleSpeed = 2f;
    [SerializeField] private float wobbleAmplitude = 3f;

    public PenaltyType PenaltyType => PenaltyType.InvertedControls;

    private Transform cameraTransform;
    private float wobbleTimer;
    private bool isActive;

    public void Activate()
    {
        PlayerController.IsMovementInverted = true;
        isActive = true;

        cameraTransform = PlayerController.Instance != null
            ? PlayerController.Instance.transform.GetChild(0)
            : Camera.main?.transform;
    }

    public void Deactivate()
    {
        PlayerController.IsMovementInverted = false;
        isActive = false;
    }

    private void LateUpdate()
    {
        if (isActive && cameraTransform != null)
        {
            wobbleTimer += Time.deltaTime * wobbleSpeed;
            float lZTilt = Mathf.Sin(wobbleTimer) * wobbleAmplitude;
            cameraTransform.localRotation = cameraTransform.localRotation * Quaternion.Euler(0, 0, lZTilt);
        }
    }
}
