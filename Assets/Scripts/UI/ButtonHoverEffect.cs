using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float hoverShiftX = -170f;
    [SerializeField] private float lerpSpeed = 12f;

    [SerializeField] [Range(0f, 1f)] private float whiteningAmount = 0.6f;

    private RectTransform rectTransform;
    private TextMeshProUGUI tmpText;
    private Vector2 originalAnchoredPos;
    private Color originalTextColor;
    private Color whitenedTextColor;
    private bool isHovered;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalAnchoredPos = rectTransform.anchoredPosition;

        tmpText = GetComponentInChildren<TextMeshProUGUI>();
        if (tmpText != null)
        {
            originalTextColor = tmpText.color;
            whitenedTextColor = Color.Lerp(originalTextColor, Color.white, whiteningAmount);
        }
    }

    public void OnPointerEnter(PointerEventData pEventData)
    {
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData pEventData)
    {
        isHovered = false;
    }

    private void Update()
    {
        Vector2 lTargetPos = isHovered
            ? new Vector2(originalAnchoredPos.x + hoverShiftX, originalAnchoredPos.y)
            : originalAnchoredPos;

        rectTransform.anchoredPosition = Vector2.Lerp(
            rectTransform.anchoredPosition,
            lTargetPos,
            Time.deltaTime * lerpSpeed);

        if (tmpText != null)
        {
            Color lTargetColor = isHovered ? whitenedTextColor : originalTextColor;
            tmpText.color = Color.Lerp(tmpText.color, lTargetColor, Time.deltaTime * lerpSpeed);
        }
    }
}
