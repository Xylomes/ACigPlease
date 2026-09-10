using System.Collections;
using UnityEngine;

public class door_shelf : MonoBehaviour
{
    public enum RotationAxis { X, Y, Z }

    public float openAngle = 90f;
    public float duration = 1f;
    public RotationAxis rotationAxis = RotationAxis.Y;

    private bool isOpen = false;
    private bool isAnimating = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Coroutine currentCoroutine;

    void Awake()
    {
        closedRotation = transform.rotation;
        Vector3 lEuler = Vector3.zero;
        switch (rotationAxis)
        {
            case RotationAxis.X: lEuler = new Vector3(openAngle, 0f, 0f); break;
            case RotationAxis.Y: lEuler = new Vector3(0f, openAngle, 0f); break;
            case RotationAxis.Z: lEuler = new Vector3(0f, 0f, openAngle); break;
        }
        openRotation = closedRotation * Quaternion.Euler(lEuler);
    }

    public void ToggleDoor()
    {
        if (isOpen)
        {
            SoundManager.Instance.PlaySFX(SoundManager.SoundType.CloseCloset);
        }
        else
        {
            SoundManager.Instance.PlaySFX(SoundManager.SoundType.OpenCloset);
        }


        if (isAnimating) return;

        isOpen = !isOpen;
        Quaternion lTarget = isOpen ? openRotation : closedRotation;

        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        currentCoroutine = StartCoroutine(RotateDoor(lTarget));
    }

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

    private IEnumerator RotateDoor(Quaternion pTarget)
    {
        isAnimating = true;
        Quaternion lStartRotation = transform.rotation;
        float lElapsed = 0f;

        while (lElapsed < duration)
        {
            lElapsed += Time.deltaTime;
            float lT = lElapsed / duration;
            transform.rotation = Quaternion.Slerp(lStartRotation, pTarget, lT);
            yield return null;

        }

        transform.rotation = pTarget;
        isAnimating = false;
    }
}
