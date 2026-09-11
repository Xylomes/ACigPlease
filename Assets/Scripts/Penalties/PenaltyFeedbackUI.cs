using TMPro;
using UnityEngine;

public class PenaltyFeedbackUI : MonoBehaviour
{
    private static readonly string[] PENALTY_LABELS = new string[]
    {
        "",
        "Vision inversée",
        "Contrôles inversés",
        "Distortion",
        "Spam de texte",
        "Vignette"
    };

    private const float FADE_DURATION = 0.3f;

    [SerializeField] private TMP_Text penaltyText;

    private CanvasGroup lCanvasGroup;

    private void Awake()
    {
        lCanvasGroup = GetComponent<CanvasGroup>();
        if (lCanvasGroup == null)
            lCanvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        PenaltyManager.OnPenaltyApplied += HandlePenaltyApplied;
        PenaltyManager.OnPenaltyCleared += HandlePenaltyCleared;
    }

    private void OnDisable()
    {
        PenaltyManager.OnPenaltyApplied -= HandlePenaltyApplied;
        PenaltyManager.OnPenaltyCleared -= HandlePenaltyCleared;
    }

    private void Start()
    {
        if (penaltyText != null)
            penaltyText.text = "";
        lCanvasGroup.alpha = 0f;
        lCanvasGroup.interactable = false;
        lCanvasGroup.blocksRaycasts = false;
    }

    private void HandlePenaltyApplied(PenaltyType pPenaltyType)
    {
        int lIndex = (int)pPenaltyType;
        string lLabel = lIndex >= 0 && lIndex < PENALTY_LABELS.Length
            ? PENALTY_LABELS[lIndex]
            : pPenaltyType.ToString();

        if (penaltyText != null)
            penaltyText.text = lLabel;

        StopAllCoroutines();
        StartCoroutine(FadeRoutine(1f));
    }

    private void HandlePenaltyCleared()
    {
        StopAllCoroutines();
        StartCoroutine(FadeRoutine(0f));
    }

    private System.Collections.IEnumerator FadeRoutine(float pTargetAlpha)
    {
        float lStartAlpha = lCanvasGroup.alpha;
        float lElapsed = 0f;

        while (lElapsed < FADE_DURATION)
        {
            lElapsed += Time.unscaledDeltaTime;
            lCanvasGroup.alpha = Mathf.Lerp(lStartAlpha, pTargetAlpha, lElapsed / FADE_DURATION);
            yield return null;
        }

        lCanvasGroup.alpha = pTargetAlpha;
    }
}
