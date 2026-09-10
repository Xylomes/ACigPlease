using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class EyeBlinkTransition : MonoBehaviour
{
    [SerializeField] private float closeDuration = 0.18f;
    [SerializeField] private float closedHoldDuration = 0.12f;
    [SerializeField] private float openDuration = 0.3f;

    [SerializeField] private AnimationCurve closeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private AnimationCurve openCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Range(0.25f, 0.75f)]
    [SerializeField] private float eyeWidth = 0.55f;
    [Range(0.3f, 10f)]
    [SerializeField] private float irisShape = 1.0f;
    [Range(0.3f, 10f)]
    [SerializeField] private float irisHeight = 1.0f;
    [Range(0.001f, 0.3f)]
    [SerializeField] private float feather = 0.04f;

    [Range(0f, 0.5f)]
    [SerializeField] private float vignetteStrength = 0.01f;

    [SerializeField] private Color lidColor = Color.black;

    public bool IsBlinking { get; private set; }

    private Material blinkMaterial;
    private Coroutine blinkCoroutine;

    private static readonly int CLOSURE_ID = Shader.PropertyToID("_Closure");
    private static readonly int EYE_WIDTH_ID = Shader.PropertyToID("_EyeWidth");
    private static readonly int IRIS_SHAPE_ID = Shader.PropertyToID("_IrisShape");
    private static readonly int IRIS_HEIGHT_ID = Shader.PropertyToID("_IrisHeight");
    private static readonly int FEATHER_ID = Shader.PropertyToID("_Feather");
    private static readonly int VIGNETTE_ID = Shader.PropertyToID("_VignetteStrength");
    private static readonly int COLOR_ID = Shader.PropertyToID("_Color");

    private void Awake()
    {
        Image lImg = GetComponent<Image>();
        blinkMaterial = lImg.materialForRendering;
        if (blinkMaterial != null)
        {
            blinkMaterial = new Material(blinkMaterial);
            lImg.material = blinkMaterial;
        }
        ApplyAllProperties();
        SetClosure(0f);
    }

    private void ApplyAllProperties()
    {
        if (blinkMaterial == null)
            return;

        blinkMaterial.SetFloat(EYE_WIDTH_ID, eyeWidth);
        blinkMaterial.SetFloat(IRIS_SHAPE_ID, irisShape);
        blinkMaterial.SetFloat(IRIS_HEIGHT_ID, irisHeight);
        blinkMaterial.SetFloat(FEATHER_ID, feather);
        blinkMaterial.SetFloat(VIGNETTE_ID, vignetteStrength);
        blinkMaterial.SetColor(COLOR_ID, lidColor);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (blinkMaterial == null)
        {
            Image lImg = GetComponent<Image>();
            if (lImg != null)
                blinkMaterial = lImg.materialForRendering;
        }
        ApplyAllProperties();
    }
#endif

    public void Blink(Action pOnEyesClosed = null)
    {
        if (IsBlinking)
            return;

        blinkCoroutine = StartCoroutine(BlinkRoutine(pOnEyesClosed));
    }

    private IEnumerator BlinkRoutine(Action pOnEyesClosed)
    {
        IsBlinking = true;

        yield return AnimateClosure(closeDuration, 1f, closeCurve);

        pOnEyesClosed?.Invoke();
        if (closedHoldDuration > 0f)
            yield return new WaitForSecondsRealtime(closedHoldDuration);

        yield return AnimateClosure(openDuration, 0f, openCurve);

        IsBlinking = false;
    }

    private IEnumerator AnimateClosure(float pDuration, float pTargetClosure, AnimationCurve pCurve)
    {
        float lStartClosure = blinkMaterial.GetFloat(CLOSURE_ID);
        float lElapsed = 0f;

        while (lElapsed < pDuration)
        {
            lElapsed += Time.unscaledDeltaTime;
            float lT = Mathf.Clamp01(lElapsed / pDuration);
            float lEasedT = pCurve.Evaluate(lT);
            SetClosure(Mathf.Lerp(lStartClosure, pTargetClosure, lEasedT));
            yield return null;
        }

        SetClosure(pTargetClosure);
    }

    private void SetClosure(float pValue)
    {
        if (blinkMaterial != null)
            blinkMaterial.SetFloat(CLOSURE_ID, pValue);
    }

    private void OnDestroy()
    {
        if (blinkMaterial != null)
            Destroy(blinkMaterial);
    }
}
