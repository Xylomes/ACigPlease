using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class InnerVoiceManager : MonoBehaviour
{
    public static InnerVoiceManager Instance { get; private set; }

    public event System.Action OnAllLinesTyped;

    [SerializeField] private GameObject voiceParent;
    [SerializeField] private TextMeshProUGUI voiceText;

    [SerializeField] private float typingSpeed = 0.03f;
    [SerializeField] private float pauseOnPunctuation = 0.3f;

    [SerializeField] private float hideDelayAfterComplete = 5f;

    [SerializeField] private float shakeIntensity = 1.2f;
    [SerializeField] private float shakeFrequency = 2f;

    [SerializeField] private Color textColor = new Color(0.8f, 0.8f, 0.85f, 1f);

    private bool isInitialized;
    private readonly List<ShakeRange> shakeRanges = new List<ShakeRange>();
    private Vector3[][] baseVertices;
    private int lastAnimationFrame = -1;

    private struct ShakeRange
    {
        public int startIndex;
        public int length;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (voiceText != null)
        {
            voiceText.text = string.Empty;
            voiceText.maxVisibleCharacters = 0;
        }
        isInitialized = true;
    }

    private void OnEnable()
    {
        Canvas.willRenderCanvases += AnimateTextMesh;
    }

    private void OnDisable()
    {
        Canvas.willRenderCanvases -= AnimateTextMesh;
    }

    public void Show(string pText)
    {
        Show(new[] { pText });
    }

    public void Show(string[] pLines)
    {
        if (voiceParent == null || voiceText == null)
            return;

        StopAllCoroutines();
        shakeRanges.Clear();
        baseVertices = null;

        StartCoroutine(ShowLinesRoutine(pLines));
    }

    public void Show(InnerVoiceData pData)
    {
        if (pData != null && pData.lines != null && pData.lines.Length > 0)
        {
            Show(pData.lines);
        }
    }

    private IEnumerator ShowLinesRoutine(string[] pLines)
    {
        if (!isInitialized)
            yield return null;

        voiceParent.SetActive(true);
        voiceText.enabled = true;
        voiceText.color = textColor;

        yield return null;
        yield return null;

        for (int i = 0; i < pLines.Length; i++)
        {
            shakeRanges.Clear();
            baseVertices = null;

            string lProcessedText = ParseAndStripShakeTags(pLines[i]);
            int lVisibleCharCount = CountVisibleCharacters(lProcessedText);

            voiceText.text = lProcessedText;
            voiceText.maxVisibleCharacters = int.MaxValue;

            int lMeshCharCount = 0;
            for (int waitFrame = 0; waitFrame < 5; waitFrame++)
            {
                yield return null;
                lMeshCharCount = voiceText.textInfo.characterCount;
                if (lMeshCharCount > 0)
                    break;
            }

            CacheBaseVertices();

            voiceText.maxVisibleCharacters = 0;

            yield return TypeRoutine(lProcessedText, lVisibleCharCount);

            if (i < pLines.Length - 1)
            {
                yield return new WaitForSeconds(hideDelayAfterComplete);
            }
        }

        OnAllLinesTyped?.Invoke();

        yield return new WaitForSeconds(hideDelayAfterComplete);
        Hide();
    }

    private IEnumerator TypeRoutine(string pText, int pTotalVisibleChars)
    {
        if (pTotalVisibleChars == 0)
            yield break;

        int lVisibleIndex = 0;
        for (int i = 0; i < pText.Length; i++)
        {
            if (pText[i] == '<')
            {
                while (i < pText.Length && pText[i] != '>')
                    i++;
                continue;
            }

            lVisibleIndex++;
            voiceText.maxVisibleCharacters = lVisibleIndex;

            char lC = pText[i];
            float lDelay = lC == '.' || lC == '!' || lC == '?'
                ? pauseOnPunctuation
                : typingSpeed;

            yield return new WaitForSeconds(lDelay);
        }

        voiceText.maxVisibleCharacters = int.MaxValue;
    }

    private int CountVisibleCharacters(string pText)
    {
        int lCount = 0;
        bool lInTag = false;
        for (int i = 0; i < pText.Length; i++)
        {
            if (pText[i] == '<') lInTag = true;
            else if (pText[i] == '>') lInTag = false;
            else if (!lInTag) lCount++;
        }
        return lCount;
    }

    public void Hide()
    {
        StopAllCoroutines();
        shakeRanges.Clear();
        baseVertices = null;

        if (voiceText != null)
        {
            voiceText.maxVisibleCharacters = 0;
            voiceText.text = string.Empty;
        }

        if (voiceParent != null)
            voiceParent.SetActive(false);
    }

    private string ParseAndStripShakeTags(string pRawText)
    {
        shakeRanges.Clear();
        StringBuilder lCleanText = new StringBuilder();
        int lVisibleCharacterIndex = 0;
        int lShakeStartIndex = -1;
        bool lReadingTag = false;
        StringBuilder lTag = new StringBuilder();

        for (int i = 0; i < pRawText.Length; i++)
        {
            char lC = pRawText[i];

            if (lC == '<' && !lReadingTag)
            {
                lReadingTag = true;
                lTag.Clear();
                lTag.Append(lC);
                continue;
            }

            if (lReadingTag)
            {
                lTag.Append(lC);
                if (lC != '>') continue;

                lReadingTag = false;
                string lTagText = lTag.ToString();
                if (lTagText == "<shake>")
                {
                    lShakeStartIndex = lVisibleCharacterIndex;
                }
                else if (lTagText == "</shake>" && lShakeStartIndex >= 0)
                {
                    shakeRanges.Add(new ShakeRange
                    {
                        startIndex = lShakeStartIndex,
                        length = lVisibleCharacterIndex - lShakeStartIndex
                    });
                    lShakeStartIndex = -1;
                }
                else
                {
                    lCleanText.Append(lTagText);
                }
                continue;
            }

            lCleanText.Append(lC);
            lVisibleCharacterIndex++;
        }

        return lCleanText.ToString();
    }

    private void CacheBaseVertices()
    {
        if (voiceText == null) return;

        TMP_TextInfo lTextInfo = voiceText.textInfo;
        if (lTextInfo == null || lTextInfo.materialCount == 0)
        {
            baseVertices = null;
            return;
        }

        baseVertices = new Vector3[lTextInfo.materialCount][];
        for (int i = 0; i < lTextInfo.materialCount; i++)
        {
            Vector3[] lVerts = lTextInfo.meshInfo[i].vertices;
            if (lVerts == null || lVerts.Length == 0)
            {
                baseVertices[i] = new Vector3[0];
                continue;
            }
            baseVertices[i] = (Vector3[])lVerts.Clone();
        }
    }

    private void AnimateTextMesh()
    {
        if (voiceParent == null || !voiceParent.activeSelf || baseVertices == null) return;
        if (voiceText == null || string.IsNullOrEmpty(voiceText.text)) return;
        if (lastAnimationFrame == Time.frameCount) return;
        if (shakeRanges.Count == 0) return;

        TMP_TextInfo lTextInfo = voiceText.textInfo;
        if (lTextInfo == null || lTextInfo.characterCount == 0) return;

        if (baseVertices.Length != lTextInfo.materialCount) return;
        for (int m = 0; m < lTextInfo.materialCount; m++)
        {
            if (lTextInfo.meshInfo[m].vertices == null) return;
            if (baseVertices[m].Length != lTextInfo.meshInfo[m].vertices.Length) return;
        }

        lastAnimationFrame = Time.frameCount;

        int lVisibleCharacters = voiceText.maxVisibleCharacters;
        if (lVisibleCharacters == 0) return;

        bool lHasShakeEffect = false;
        foreach (ShakeRange lRange in shakeRanges)
        {
            int lEndIndex = Mathf.Min(lRange.startIndex + lRange.length, lVisibleCharacters);
            for (int i = lRange.startIndex; i < lEndIndex; i++)
            {
                if (i >= lTextInfo.characterCount) break;
                if (!lTextInfo.characterInfo[i].isVisible) continue;

                RestoreCharacterBase(lTextInfo, i);
                ApplyShake(lTextInfo, i);
                lHasShakeEffect = true;
            }
        }

        if (lHasShakeEffect)
        {
            voiceText.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
        }
    }

    private void RestoreCharacterBase(TMP_TextInfo pTextInfo, int pCharIndex)
    {
        TMP_CharacterInfo lCharInfo = pTextInfo.characterInfo[pCharIndex];
        int lMatIndex = lCharInfo.materialReferenceIndex;
        int lVertIndex = lCharInfo.vertexIndex;

        Vector3[] lTargetVerts = pTextInfo.meshInfo[lMatIndex].vertices;
        Vector3[] lSourceVerts = baseVertices[lMatIndex];

        for (int i = 0; i < 4; i++)
        {
            lTargetVerts[lVertIndex + i] = lSourceVerts[lVertIndex + i];
        }
    }

    private void ApplyShake(TMP_TextInfo pTextInfo, int pCharIndex)
    {
        TMP_CharacterInfo lCharInfo = pTextInfo.characterInfo[pCharIndex];
        Vector3[] lVerts = pTextInfo.meshInfo[lCharInfo.materialReferenceIndex].vertices;
        int lVertIndex = lCharInfo.vertexIndex;

        float lTime = Time.time * shakeFrequency;
        float lOffsetX = (Mathf.PerlinNoise(lTime, pCharIndex) - 0.5f) * shakeIntensity * 2f;
        float lOffsetY = (Mathf.PerlinNoise(pCharIndex, lTime) - 0.5f) * shakeIntensity * 2f;
        Vector3 lOffset = new Vector3(lOffsetX, lOffsetY, 0f);

        for (int i = 0; i < 4; i++)
        {
            lVerts[lVertIndex + i] += lOffset;
        }
    }

}
