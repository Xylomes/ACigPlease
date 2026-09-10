using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextSpamPenalty : MonoBehaviour, IPenalty
{
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
            targetCanvas = FindAnyObjectByType<Canvas>();
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

        foreach (TMP_Text lTxt in activeTexts)
        {
            if (lTxt != null)
            {
                Destroy(lTxt.gameObject);
            }
        }
        activeTexts.Clear();
    }

    private IEnumerator SpawnTextsRoutine()
    {
        WaitForSeconds lWait = new WaitForSeconds(spawnInterval);
        while (true)
        {
            if (activeTexts.Count < MAX_TEXTS_ON_SCREEN)
            {
                SpawnRandomText();
            }
            yield return lWait;
        }
    }

    private void SpawnRandomText()
    {
        RectTransform lCanvasRect = targetCanvas.transform as RectTransform;
        if (lCanvasRect == null)
            return;

        TMP_Text lNewText = Instantiate(textPrefab, targetCanvas.transform);
        lNewText.text = SPAM_MESSAGES[Random.Range(0, SPAM_MESSAGES.Length)];
        lNewText.fontSize = Random.Range(minFontSize, maxFontSize);
        lNewText.color = new Color(
            Random.Range(0.5f, 1f),
            Random.Range(0f, 0.3f),
            Random.Range(0f, 0.3f),
            1f
        );

        RectTransform lRect = lNewText.rectTransform;
        lRect.anchoredPosition = new Vector2(
            Random.Range(-lCanvasRect.rect.width * 0.4f, lCanvasRect.rect.width * 0.4f),
            Random.Range(-lCanvasRect.rect.height * 0.4f, lCanvasRect.rect.height * 0.4f)
        );

        activeTexts.Add(lNewText);
        StartCoroutine(DestroyAfterDelay(lNewText, textLifetime));
    }

    private IEnumerator DestroyAfterDelay(TMP_Text pText, float pDelay)
    {
        yield return new WaitForSeconds(pDelay);
        if (pText != null)
        {
            activeTexts.Remove(pText);
            Destroy(pText.gameObject);
        }
    }
}
