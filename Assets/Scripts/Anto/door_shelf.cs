using System.Collections;
using UnityEngine;

public class door_shelf : MonoBehaviour
{
    public enum RotationAxis { X, Y, Z }

    public float openAngle = 90f;
    public float duration = 1f;
    [Tooltip("Axis around which the door rotates.")]
    public RotationAxis rotationAxis = RotationAxis.Y;

    private bool isOpen = false;
    private bool isAnimating = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Coroutine currentCoroutine;

    void Awake()
    {
        closedRotation = transform.rotation;
        Vector3 euler = Vector3.zero;
        switch (rotationAxis)
        {
            case RotationAxis.X: euler = new Vector3(openAngle, 0f, 0f); break;
            case RotationAxis.Y: euler = new Vector3(0f, openAngle, 0f); break;
            case RotationAxis.Z: euler = new Vector3(0f, 0f, openAngle); break;
        }
        openRotation = closedRotation * Quaternion.Euler(euler);
    }

    public void ToggleDoor()
    {
        if (isAnimating) return;

        isOpen = !isOpen;
        Quaternion target = isOpen ? openRotation : closedRotation;

        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        currentCoroutine = StartCoroutine(RotateDoor(target));
    }

    /// <summary>Force the door to its closed state regardless of current internal state.</summary>
    public void ForceClose()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }
        isOpen = false;
        isAnimating = false;
        transform.rotation = closedRotation;
    }

    private IEnumerator RotateDoor(Quaternion target)
    {
        isAnimating = true;
        Quaternion startRotation = transform.rotation;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.rotation = Quaternion.Slerp(startRotation, target, t);
            yield return null;

        }

        transform.rotation = target;
        isAnimating = false;
    }
}
