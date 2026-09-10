using UnityEngine;

public class InvertedMousePenalty : MonoBehaviour, IPenalty
{
    [SerializeField] private float wobbleSpeed = 2f;
    [SerializeField] private float wobbleAmplitude = 3f;

    public PenaltyType PenaltyType => PenaltyType.InvertedMouse;

    private Transform cameraTransform;
    private float wobbleTimer;
    private bool isActive;

    public void Activate()
    {
        PlayerRotation.IsInputInverted = true;
        isActive = true;

        cameraTransform = PlayerController.Instance != null
            ? PlayerController.Instance.transform.GetChild(0)
            : Camera.main?.transform;
    }

    public void Deactivate()
    {
        PlayerRotation.IsInputInverted = false;
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
