using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class InnerVoiceManager : MonoBehaviour
{
    public static InnerVoiceManager Instance { get; private set; }

    /// <summary>Fired after the last line's typewriter effect completes, before the auto-hide delay.</summary>
    public event System.Action OnAllLinesTyped;

    [Header("UI References")]
    [SerializeField] private GameObject voiceParent;
    [SerializeField] private TextMeshProUGUI voiceText;

    [Header("Typing Settings")]
    [SerializeField] private float typingSpeed = 0.03f;
    [SerializeField] private float pauseOnPunctuation = 0.3f;

    [Header("Auto-Hide")]
    [SerializeField] private float hideDelayAfterComplete = 5f;

    [Header("Shake Effect")]
    [SerializeField] private float shakeIntensity = 1.2f;
    [SerializeField] private float shakeFrequency = 2f;

    [Header("Default Style")]
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
        // Do NOT deactivate parent here — GameManager.Start() may run before this
        // and call Show(), which would then be killed by our Start() deactivating the parent.
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

    /// <summary>Show a single line of inner voice text with typewriter effect, then auto-hide.</summary>
    public void Show(string text)
    {
        Show(new[] { text });
    }

    /// <summary>Show multiple lines sequentially, then auto-hide after the last line completes.</summary>
    public void Show(string[] lines)
    {
        if (voiceParent == null || voiceText == null)
        {
            Debug.LogWarning("[InnerVoice] UI references not assigned.");
            return;
        }

        StopAllCoroutines();
        shakeRanges.Clear();
        baseVertices = null;

        StartCoroutine(ShowLinesRoutine(lines));
    }

    /// <summary>Show lines from an InnerVoiceData ScriptableObject.</summary>
    public void Show(InnerVoiceData data)
    {
        if (data != null && data.lines != null && data.lines.Length > 0)
        {
            Show(data.lines);
        }
    }

    private IEnumerator ShowLinesRoutine(string[] lines)
    {
        if (!isInitialized)
            yield return null;

        voiceParent.SetActive(true);
        voiceText.enabled = true;
        voiceText.color = textColor;

        // Wait for TMP to initialize after activation
        yield return null;
        yield return null;

        for (int i = 0; i < lines.Length; i++)
        {
            shakeRanges.Clear();
            baseVertices = null;

            string processedText = ParseAndStripShakeTags(lines[i]);
            int visibleCharCount = CountVisibleCharacters(processedText);

            // Set text with ALL characters visible so TMP generates the full mesh
            voiceText.text = processedText;
            voiceText.maxVisibleCharacters = int.MaxValue;

            // Wait for TMP's natural update cycle to generate the mesh
            int meshCharCount = 0;
            for (int waitFrame = 0; waitFrame < 5; waitFrame++)
            {
                yield return null;
                meshCharCount = voiceText.textInfo.characterCount;
                if (meshCharCount > 0)
                    break;
            }

            // Cache vertices for shake effect
            CacheBaseVertices();

            // Hide all characters to prepare for typewriter
            voiceText.maxVisibleCharacters = 0;

            // Run typewriter
            yield return TypeRoutine(processedText, visibleCharCount);

            if (i < lines.Length - 1)
            {
                yield return new WaitForSeconds(hideDelayAfterComplete);
            }
        }

        OnAllLinesTyped?.Invoke();

        yield return new WaitForSeconds(hideDelayAfterComplete);
        Hide();
    }

    /// <summary>Typewriter coroutine that uses string-based character counting instead of textInfo.</summary>
    private IEnumerator TypeRoutine(string text, int totalVisibleChars)
    {
        if (totalVisibleChars == 0)
            yield break;

        int visibleIndex = 0;
        for (int i = 0; i < text.Length; i++)
        {
            // Skip rich text tags
            if (text[i] == '<')
            {
                while (i < text.Length && text[i] != '>')
                    i++;
                continue;
            }

            visibleIndex++;
            voiceText.maxVisibleCharacters = visibleIndex;

            char c = text[i];
            float delay = c == '.' || c == '!' || c == '?'
                ? pauseOnPunctuation
                : typingSpeed;

            yield return new WaitForSeconds(delay);
        }

        // Ensure all characters are visible
        voiceText.maxVisibleCharacters = int.MaxValue;
    }

    /// <summary>Count visible characters in text, excluding rich text tags.</summary>
    private int CountVisibleCharacters(string text)
    {
        int count = 0;
        bool inTag = false;
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '<') inTag = true;
            else if (text[i] == '>') inTag = false;
            else if (!inTag) count++;
        }
        return count;
    }

    /// <summary>Immediately hide the inner voice text.</summary>
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

    #region Shake Effect

    private string ParseAndStripShakeTags(string rawText)
    {
        shakeRanges.Clear();
        StringBuilder cleanText = new StringBuilder();
        int visibleCharacterIndex = 0;
        int shakeStartIndex = -1;
        bool readingTag = false;
        StringBuilder tag = new StringBuilder();

        for (int i = 0; i < rawText.Length; i++)
        {
            char c = rawText[i];

            if (c == '<' && !readingTag)
            {
                readingTag = true;
                tag.Clear();
                tag.Append(c);
                continue;
            }

            if (readingTag)
            {
                tag.Append(c);
                if (c != '>') continue;

                readingTag = false;
                string tagText = tag.ToString();
                if (tagText == "<shake>")
                {
                    shakeStartIndex = visibleCharacterIndex;
                }
                else if (tagText == "</shake>" && shakeStartIndex >= 0)
                {
                    shakeRanges.Add(new ShakeRange
                    {
                        startIndex = shakeStartIndex,
                        length = visibleCharacterIndex - shakeStartIndex
                    });
                    shakeStartIndex = -1;
                }
                else
                {
                    cleanText.Append(tagText);
                }
                continue;
            }

            cleanText.Append(c);
            visibleCharacterIndex++;
        }

        return cleanText.ToString();
    }

    private void CacheBaseVertices()
    {
        if (voiceText == null) return;

        TMP_TextInfo textInfo = voiceText.textInfo;
        if (textInfo == null || textInfo.materialCount == 0)
        {
            baseVertices = null;
            return;
        }

        baseVertices = new Vector3[textInfo.materialCount][];
        for (int i = 0; i < textInfo.materialCount; i++)
        {
            Vector3[] verts = textInfo.meshInfo[i].vertices;
            if (verts == null || verts.Length == 0)
            {
                baseVertices[i] = new Vector3[0];
                continue;
            }
            baseVertices[i] = (Vector3[])verts.Clone();
        }
    }

    private void AnimateTextMesh()
    {
        if (voiceParent == null || !voiceParent.activeSelf || baseVertices == null) return;
        if (voiceText == null || string.IsNullOrEmpty(voiceText.text)) return;
        if (lastAnimationFrame == Time.frameCount) return;
        if (shakeRanges.Count == 0) return;

        TMP_TextInfo textInfo = voiceText.textInfo;
        if (textInfo == null || textInfo.characterCount == 0) return;

        if (baseVertices.Length != textInfo.materialCount) return;
        for (int m = 0; m < textInfo.materialCount; m++)
        {
            if (textInfo.meshInfo[m].vertices == null) return;
            if (baseVertices[m].Length != textInfo.meshInfo[m].vertices.Length) return;
        }

        lastAnimationFrame = Time.frameCount;

        int visibleCharacters = voiceText.maxVisibleCharacters;
        if (visibleCharacters == 0) return;

        bool hasShakeEffect = false;
        foreach (ShakeRange range in shakeRanges)
        {
            int endIndex = Mathf.Min(range.startIndex + range.length, visibleCharacters);
            for (int i = range.startIndex; i < endIndex; i++)
            {
                if (i >= textInfo.characterCount) break;
                if (!textInfo.characterInfo[i].isVisible) continue;

                RestoreCharacterBase(textInfo, i);
                ApplyShake(textInfo, i);
                hasShakeEffect = true;
            }
        }

        if (hasShakeEffect)
        {
            voiceText.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
        }
    }

    private void RestoreCharacterBase(TMP_TextInfo textInfo, int charIndex)
    {
        TMP_CharacterInfo charInfo = textInfo.characterInfo[charIndex];
        int matIndex = charInfo.materialReferenceIndex;
        int vertIndex = charInfo.vertexIndex;

        Vector3[] targetVerts = textInfo.meshInfo[matIndex].vertices;
        Vector3[] sourceVerts = baseVertices[matIndex];

        for (int i = 0; i < 4; i++)
        {
            targetVerts[vertIndex + i] = sourceVerts[vertIndex + i];
        }
    }

    private void ApplyShake(TMP_TextInfo textInfo, int charIndex)
    {
        TMP_CharacterInfo charInfo = textInfo.characterInfo[charIndex];
        Vector3[] verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
        int vertIndex = charInfo.vertexIndex;

        float time = Time.time * shakeFrequency;
        float offsetX = (Mathf.PerlinNoise(time, charIndex) - 0.5f) * shakeIntensity * 2f;
        float offsetY = (Mathf.PerlinNoise(charIndex, time) - 0.5f) * shakeIntensity * 2f;
        Vector3 offset = new Vector3(offsetX, offsetY, 0f);

        for (int i = 0; i < 4; i++)
        {
            verts[vertIndex + i] += offset;
        }
    }

    #endregion
}
