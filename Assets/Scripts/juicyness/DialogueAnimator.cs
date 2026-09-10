using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class DialogueAnimator : MonoBehaviour
{
    [SerializeField] private GameObject dialogueParent;
    [SerializeField] private TextMeshProUGUI speakerName;
    [SerializeField] private TextMeshProUGUI dialogueText;

    public bool IsTyping;

    [SerializeField] private float typingTime = 0.03f;
    [SerializeField] private float pauseTypingTime = 0.3f;

    [SerializeField] private float shakeIntensity = 1.2f;
    [SerializeField] private float shakeFrequency = 2f;

    private readonly List<ShakeRange> shakeRanges = new List<ShakeRange>();
    private Coroutine typingCoroutine;
    private Vector3[][] baseVertices;
    private int lastAnimationFrame = -1;

    private struct ShakeRange
    {
        public int startIndex;
        public int length;
    }

    private void OnEnable()
    {
        Canvas.willRenderCanvases += AnimateTextMesh;
    }

    private void Start()
    {
        dialogueParent.SetActive(false);
        dialogueText.maxVisibleCharacters = 0;
        IsTyping = false;
    }

    private void OnDisable()
    {
        Canvas.willRenderCanvases -= AnimateTextMesh;
    }

    public void ShowLine(string pSpeakerNameText, string pDialogueTextContent)
    {
        StopTypingCoroutine();
        shakeRanges.Clear();
        baseVertices = null;

        dialogueParent.SetActive(true);
        speakerName.text = pSpeakerNameText;
        dialogueText.text = ParseAndStripShakeTags(pDialogueTextContent);

        dialogueText.maxVisibleCharacters = int.MaxValue;
        dialogueText.ForceMeshUpdate();
        CacheBaseVertices();
        dialogueText.maxVisibleCharacters = 0;
        dialogueText.ForceMeshUpdate();

        typingCoroutine = StartCoroutine(Type());
    }

    public void HideLine()
    {
        StopTypingCoroutine();
        shakeRanges.Clear();
        baseVertices = null;
        dialogueText.maxVisibleCharacters = 0;
        speakerName.text = string.Empty;
        dialogueText.text = string.Empty;
        dialogueParent.SetActive(false);
    }

    public void CompleteTyping()
    {
        StopTypingCoroutine();
        dialogueText.maxVisibleCharacters = dialogueText.textInfo.characterCount;
        IsTyping = false;
    }

    private void StopTypingCoroutine()
    {
        if (typingCoroutine == null) return;

        StopCoroutine(typingCoroutine);
        typingCoroutine = null;
    }

    private IEnumerator Type()
    {
        int lTotalCharacters = dialogueText.textInfo.characterCount;
        IsTyping = true;

        for (int i = 0; i < lTotalCharacters; i++)
        {
            TMP_CharacterInfo lCharacterInfo = dialogueText.textInfo.characterInfo[i];
            dialogueText.maxVisibleCharacters++;

            float lDelay = lCharacterInfo.character == '.' || lCharacterInfo.character == '!' || lCharacterInfo.character == '?'
                ? pauseTypingTime
                : typingTime;

            yield return new WaitForSeconds(lDelay);
        }

        IsTyping = false;
        typingCoroutine = null;
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
            char lCharacter = pRawText[i];

            if (lCharacter == '<' && !lReadingTag)
            {
                lReadingTag = true;
                lTag.Clear();
                lTag.Append(lCharacter);
                continue;
            }

            if (lReadingTag)
            {
                lTag.Append(lCharacter);
                if (lCharacter != '>') continue;

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

            lCleanText.Append(lCharacter);
            lVisibleCharacterIndex++;
        }

        return lCleanText.ToString();
    }

    private void CacheBaseVertices()
    {
        TMP_TextInfo lTextInfo = dialogueText.textInfo;
        baseVertices = new Vector3[lTextInfo.materialCount][];

        for (int i = 0; i < lTextInfo.materialCount; i++)
        {
            baseVertices[i] = (Vector3[])lTextInfo.meshInfo[i].vertices.Clone();
        }
    }

    private void AnimateTextMesh()
    {
        if (!dialogueParent.activeSelf || baseVertices == null) return;
        if (lastAnimationFrame == Time.frameCount) return;

        TMP_TextInfo lTextInfo = dialogueText.textInfo;
        if (lTextInfo.characterCount == 0) return;

        if (baseVertices.Length != lTextInfo.materialCount) return;
        for (int m = 0; m < lTextInfo.materialCount; m++)
        {
            if (baseVertices[m].Length != lTextInfo.meshInfo[m].vertices.Length) return;
        }

        lastAnimationFrame = Time.frameCount;

        int lVisibleCharacters = dialogueText.maxVisibleCharacters;
        if (lVisibleCharacters == 0) return;

        if (shakeRanges.Count == 0) return;

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
            dialogueText.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
        }
    }

    private void RestoreCharacterBase(TMP_TextInfo pTextInfo, int pCharacterIndex)
    {
        TMP_CharacterInfo lCharacterInfo = pTextInfo.characterInfo[pCharacterIndex];
        int lMaterialIndex = lCharacterInfo.materialReferenceIndex;
        int lVertexIndex = lCharacterInfo.vertexIndex;

        Vector3[] lTargetVertices = pTextInfo.meshInfo[lMaterialIndex].vertices;
        Vector3[] lSourceVertices = baseVertices[lMaterialIndex];

        for (int i = 0; i < 4; i++)
        {
            lTargetVertices[lVertexIndex + i] = lSourceVertices[lVertexIndex + i];
        }
    }

    private void ApplyShake(TMP_TextInfo pTextInfo, int pCharacterIndex)
    {
        TMP_CharacterInfo lCharacterInfo = pTextInfo.characterInfo[pCharacterIndex];
        Vector3[] lVertices = pTextInfo.meshInfo[lCharacterInfo.materialReferenceIndex].vertices;
        int lVertexIndex = lCharacterInfo.vertexIndex;

        float lTime = Time.time * shakeFrequency;
        float lOffsetX = (Mathf.PerlinNoise(lTime, pCharacterIndex) - 0.5f) * shakeIntensity * 2f;
        float lOffsetY = (Mathf.PerlinNoise(pCharacterIndex, lTime) - 0.5f) * shakeIntensity * 2f;
        Vector3 lOffset = new Vector3(lOffsetX, lOffsetY, 0f);

        for (int i = 0; i < 4; i++)
        {
            lVertices[lVertexIndex + i] += lOffset;
        }
    }
}
