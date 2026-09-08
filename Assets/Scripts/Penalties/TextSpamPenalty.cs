using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextSpamPenalty : MonoBehaviour, IPenalty
{
    [Header("Text Spam Settings")]
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private TMP_Text textPrefab;
    [SerializeField] private float spawnInterval = 0.3f;
    [SerializeField] private float textLifetime = 3f;
    [SerializeField] private float minFontSize = 14f;
    [SerializeField] private float maxFontSize = 42f;

    private const int MAX_TEXTS_ON_SCREEN = 30;
    private static readonly string[] SPAM_MESSAGES = {
        "TU NE TROUVERAS JAMAIS",
        "IL N'Y A RIEN ICI",
        "CHERCHE ENCORE",
        "PERDU",
        "TROP TARD",
        "LA CLOPE N'EST PAS LA",
        "CONTINUE",
        "PERDU PERDU PERDU"
    };

    public PenaltyType PenaltyType => PenaltyType.TextSpam;

    private Coroutine spawnCoroutine;
    private List<TMP_Text> activeTexts = new List<TMP_Text>();

    public void Activate()
    {
        if (targetCanvas == null)
        {
            targetCanvas = FindObjectOfType<Canvas>();
        }

        if (textPrefab == null || targetCanvas == null)
            return;

        spawnCoroutine = StartCoroutine(SpawnTextsRoutine());
    }

    public void Deactivate()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        foreach (TMP_Text txt in activeTexts)
        {
            if (txt != null)
            {
                Destroy(txt.gameObject);
            }
        }
        activeTexts.Clear();
    }

    private IEnumerator SpawnTextsRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(spawnInterval);
        while (true)
        {
            if (activeTexts.Count < MAX_TEXTS_ON_SCREEN)
            {
                SpawnRandomText();
            }
            yield return wait;
        }
    }

    private void SpawnRandomText()
    {
        RectTransform canvasRect = targetCanvas.transform as RectTransform;
        if (canvasRect == null)
            return;

        TMP_Text newText = Instantiate(textPrefab, targetCanvas.transform);
        newText.text = SPAM_MESSAGES[Random.Range(0, SPAM_MESSAGES.Length)];
        newText.fontSize = Random.Range(minFontSize, maxFontSize);
        newText.color = new Color(
            Random.Range(0.5f, 1f),
            Random.Range(0f, 0.3f),
            Random.Range(0f, 0.3f),
            1f
        );

        RectTransform rect = newText.rectTransform;
        rect.anchoredPosition = new Vector2(
            Random.Range(-canvasRect.rect.width * 0.4f, canvasRect.rect.width * 0.4f),
            Random.Range(-canvasRect.rect.height * 0.4f, canvasRect.rect.height * 0.4f)
        );

        activeTexts.Add(newText);
        StartCoroutine(DestroyAfterDelay(newText, textLifetime));
    }

    private IEnumerator DestroyAfterDelay(TMP_Text text, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (text != null)
        {
            activeTexts.Remove(text);
            Destroy(text.gameObject);
        }
    }
}
