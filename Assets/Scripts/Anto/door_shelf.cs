using System.Collections;
using UnityEngine;

public class door_shelf : MonoBehaviour
{
    public float openAngle = 90f;
    public float duration = 1f;

    private bool isOpen = false;
    private bool isAnimating = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Coroutine currentCoroutine;

    void Start()
    {
        closedRotation = transform.rotation;
        openRotation = closedRotation * Quaternion.Euler(0f, openAngle, 0f);
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
