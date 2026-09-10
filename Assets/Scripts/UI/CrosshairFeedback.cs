using UnityEngine;

public class CrosshairFeedback : MonoBehaviour
{
    [Header("Scale")]
    [SerializeField] private float minScale = 1f;
    [SerializeField] private float maxScale = 2.5f;
    [SerializeField] private float lerpSpeed = 15f;

    [Header("Color")]
    [SerializeField] private Color idleColor = new Color(1f, 1f, 0f, 1f);
    [SerializeField] private Color activeColor = new Color(0f, 1f, 0.5f, 1f);

    private const float HOLD_DURATION = 0.2f;
    private const int INTERACT_KEY = 101;

    private RectTransform rectTransform;
    private UnityEngine.UI.Image cursorImage;
    private float holdTimer;
    private float currentScale = 1f;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        cursorImage = GetComponent<UnityEngine.UI.Image>();
        currentScale = minScale;
        ApplyScale(currentScale);
    }

    private void Update()
    {
        bool isPressing = IsInteractPressed();

        if (isPressing)
        {
            holdTimer += Time.deltaTime;
        }
        else
        {
            holdTimer = 0f;
        }

        float progress = Mathf.Clamp01(holdTimer / HOLD_DURATION);
        float targetScale = Mathf.Lerp(minScale, maxScale, progress);
        currentScale = Mathf.Lerp(currentScale, targetScale, Time.deltaTime * lerpSpeed);
        ApplyScale(currentScale);

        if (cursorImage != null)
        {
            cursorImage.color = Color.Lerp(idleColor, activeColor, progress);
        }
    }

    private bool IsInteractPressed()
    {
        bool inDialogue = DialogueManager.Instance != null && DialogueManager.Instance.isInDialogue;
        if (inDialogue)
            return false;

        if (PlayerController.Instance != null && PlayerController.Instance.IsInteractHeld)
            return true;

        return Input.GetKey(KeyCode.E);
    }

    private void ApplyScale(float pScale)
    {
        if (rectTransform != null)
        {
            rectTransform.localScale = new Vector3(pScale, pScale, 1f);
        }
    }
}
