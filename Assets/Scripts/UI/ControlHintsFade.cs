using System.Collections;
using TMPro;
using UnityEngine;

public class ControlHintsFade : MonoBehaviour
{
    private const float VISIBLE_DURATION = 10f;
    private const float FADE_DURATION = 5f;

    private TextMeshProUGUI[] hintTexts;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        hintTexts = GetComponentsInChildren<TextMeshProUGUI>(true);
    }

    public void ShowHints()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        foreach (TextMeshProUGUI lText in hintTexts)
        {
            if (lText != null)
            {
                Color lColor = lText.color;
                lColor.a = 1f;
                lText.color = lColor;
                lText.gameObject.SetActive(true);
            }
        }

        fadeCoroutine = StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeOutRoutine()
    {
        yield return new WaitForSeconds(VISIBLE_DURATION);

        float lElapsed = 0f;
        while (lElapsed < FADE_DURATION)
        {
            lElapsed += Time.deltaTime;
            float lAlpha = Mathf.Lerp(1f, 0f, lElapsed / FADE_DURATION);

            foreach (TextMeshProUGUI lText in hintTexts)
            {
                if (lText != null)
                {
                    Color lColor = lText.color;
                    lColor.a = lAlpha;
                    lText.color = lColor;
                }
            }

            yield return null;
        }

        foreach (TextMeshProUGUI lText in hintTexts)
        {
            if (lText != null)
            {
                Color lColor = lText.color;
                lColor.a = 0f;
                lText.color = lColor;
            }
        }

        fadeCoroutine = null;
    }
}
