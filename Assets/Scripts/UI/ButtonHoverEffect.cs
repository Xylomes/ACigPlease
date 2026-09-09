using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>Smooth hover effect for UI buttons: shifts the button horizontally and whitens its TMP text.</summary>
public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Movement")]
    [Tooltip("Decalage en X vers la gauche quand le joueur over le bouton.")]
    [SerializeField] private float hoverShiftX = -170f;
    [Tooltip("Vitesse de la transition (plus grand = plus rapide).")]
    [SerializeField] private float lerpSpeed = 12f;

    [Header("Text Whitening")]
    [Tooltip("Pourcentage de melange vers le blanc (0 = couleur d'origine, 1 = blanc complet).")]
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }

    private void Update()
    {
        Vector2 targetPos = isHovered
            ? new Vector2(originalAnchoredPos.x + hoverShiftX, originalAnchoredPos.y)
            : originalAnchoredPos;

        rectTransform.anchoredPosition = Vector2.Lerp(
            rectTransform.anchoredPosition,
            targetPos,
            Time.deltaTime * lerpSpeed);

        if (tmpText != null)
        {
            Color targetColor = isHovered ? whitenedTextColor : originalTextColor;
            tmpText.color = Color.Lerp(tmpText.color, targetColor, Time.deltaTime * lerpSpeed);
        }
    }
}
