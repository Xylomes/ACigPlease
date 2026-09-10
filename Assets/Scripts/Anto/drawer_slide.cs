using System.Collections;
using UnityEngine;

public class drawer_slide : MonoBehaviour
{
    public enum SlideAxis { X, Y, Z }

    [Tooltip("Distance to slide when opening, in local units.")]
    public float slideDistance = 0.5f;

    [Tooltip("Axis along which the drawer slides.")]
    public SlideAxis slideAxis = SlideAxis.Z;

    [Tooltip("Duration of the slide animation in seconds.")]
    public float duration = 1f;

    private bool isOpen = false;
    private bool isAnimating = false;
    private Vector3 closedPosition;
    private Vector3 openPosition;
    private Coroutine currentCoroutine;

    void Awake()
    {
        closedPosition = transform.localPosition;

        Vector3 offset = Vector3.zero;
        switch (slideAxis)
        {
            case SlideAxis.X: offset = new Vector3(slideDistance, 0f, 0f); break;
            case SlideAxis.Y: offset = new Vector3(0f, slideDistance, 0f); break;
            case SlideAxis.Z: offset = new Vector3(0f, 0f, slideDistance); break;
        }
        openPosition = closedPosition + offset;
    }

    public void ToggleDoor()
    {
        if (isAnimating) return;

        isOpen = !isOpen;
        Vector3 target = isOpen ? openPosition : closedPosition;

        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        currentCoroutine = StartCoroutine(SlideDrawer(target));
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
        transform.localPosition = closedPosition;
    }

    private IEnumerator SlideDrawer(Vector3 target)
    {
        isAnimating = true;
        Vector3 startPos = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localPosition = Vector3.Lerp(startPos, target, t);
            yield return null;
        }

        transform.localPosition = target;
        isAnimating = false;
    }
}
