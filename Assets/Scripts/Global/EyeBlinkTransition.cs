using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Animates an eye-blink transition using a custom "UI/EyeBlink" shader material.
/// The shader draws two lens-shaped (eye-like) openings that close and open.
/// All eye shape parameters are exposed in the Inspector for live tweaking.
/// Attach to a GameObject with an Image component whose material uses the EyeBlink shader.
/// </summary>
[RequireComponent(typeof(Image))]
public class EyeBlinkTransition : MonoBehaviour
{
    [Header("Timing")]
    [Tooltip("Duration of the closing phase in seconds.")]
    [SerializeField] private float closeDuration = 0.18f;
    [Tooltip("Duration the eyes stay fully closed before opening.")]
    [SerializeField] private float closedHoldDuration = 0.12f;
    [Tooltip("Duration of the opening phase in seconds.")]
    [SerializeField] private float openDuration = 0.3f;

    [Header("Easing")]
    [Tooltip("Curve used for closing.")]
    [SerializeField] private AnimationCurve closeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [Tooltip("Curve used for opening.")]
    [SerializeField] private AnimationCurve openCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Eye Shape")]
    [Tooltip("How wide each eye is as a fraction of the screen (0.5 = half screen each).")]
    [Range(0.25f, 0.75f)]
    [SerializeField] private float eyeWidth = 0.55f;
    [Tooltip("Controls horizontal stretch of the iris shape. 1.0 = perfect circle, <1 = narrower, >1 = wider.")]
    [Range(0.3f, 10f)]
    [SerializeField] private float irisShape = 1.0f;
    [Tooltip("Controls vertical stretch of the iris shape. 1.0 = perfect circle, <1 = shorter, >1 = taller.")]
    [Range(0.3f, 10f)]
    [SerializeField] private float irisHeight = 1.0f;
    [Tooltip("Softness of the eyelid edge. Smaller = sharper, larger = blurrier.")]
    [Range(0.001f, 0.3f)]
    [SerializeField] private float feather = 0.04f;

    [Header("Vignette")]
    [Tooltip("Strength of the darkening at screen edges. 0 = none.")]
    [Range(0f, 0.5f)]
    [SerializeField] private float vignetteStrength = 0.01f;

    [Header("Color")]
    [Tooltip("Color of the eyelids (what covers the screen when eyes are closed).")]
    [SerializeField] private Color lidColor = Color.black;

    /// <summary>True while a blink transition is in progress.</summary>
    public bool IsBlinking { get; private set; }

    private Material blinkMaterial;
    private Coroutine blinkCoroutine;

    // Shader property IDs
    private static readonly int CLOSURE_ID = Shader.PropertyToID("_Closure");
    private static readonly int EYE_WIDTH_ID = Shader.PropertyToID("_EyeWidth");
    private static readonly int IRIS_SHAPE_ID = Shader.PropertyToID("_IrisShape");
    private static readonly int IRIS_HEIGHT_ID = Shader.PropertyToID("_IrisHeight");
    private static readonly int FEATHER_ID = Shader.PropertyToID("_Feather");
    private static readonly int VIGNETTE_ID = Shader.PropertyToID("_VignetteStrength");
    private static readonly int COLOR_ID = Shader.PropertyToID("_Color");

    private void Awake()
    {
        Image img = GetComponent<Image>();
        blinkMaterial = img.materialForRendering;
        if (blinkMaterial != null)
        {
            blinkMaterial = new Material(blinkMaterial);
            img.material = blinkMaterial;
        }
        ApplyAllProperties();
        SetClosure(0f);
    }

    /// <summary>
    /// Pushes all Inspector-controlled shader properties to the material.
    /// Called on Awake and whenever a value changes in the Inspector.
    /// </summary>
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
    /// <summary>Called when a value changes in the Inspector — updates the material live.</summary>
    private void OnValidate()
    {
        if (blinkMaterial == null)
        {
            Image img = GetComponent<Image>();
            if (img != null)
                blinkMaterial = img.materialForRendering;
        }
        ApplyAllProperties();
    }
#endif

    /// <summary>
    /// Play a full blink transition. The callback is invoked at full eye closure
    /// — the ideal moment to swap UI states, enable/disable gameplay, etc.
    /// </summary>
    /// <param name="onEyesClosed">Action invoked when the eyes are fully closed.</param>
    public void Blink(Action onEyesClosed = null)
    {
        if (IsBlinking)
            return;

        blinkCoroutine = StartCoroutine(BlinkRoutine(onEyesClosed));
    }

    private IEnumerator BlinkRoutine(Action onEyesClosed)
    {
        IsBlinking = true;

        // --- Close ---
        yield return AnimateClosure(closeDuration, targetClosure: 1f, closeCurve);

        // --- Hold closed ---
        onEyesClosed?.Invoke();
        if (closedHoldDuration > 0f)
            yield return new WaitForSecondsRealtime(closedHoldDuration);

        // --- Open ---
        yield return AnimateClosure(openDuration, targetClosure: 0f, openCurve);

        IsBlinking = false;
    }

    private IEnumerator AnimateClosure(float duration, float targetClosure, AnimationCurve curve)
    {
        float startClosure = blinkMaterial.GetFloat(CLOSURE_ID);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = curve.Evaluate(t);
            SetClosure(Mathf.Lerp(startClosure, targetClosure, easedT));
            yield return null;
        }

        SetClosure(targetClosure);
    }

    private void SetClosure(float value)
    {
        if (blinkMaterial != null)
            blinkMaterial.SetFloat(CLOSURE_ID, value);
    }

    private void OnDestroy()
    {
        if (blinkMaterial != null)
            Destroy(blinkMaterial);
    }
}
