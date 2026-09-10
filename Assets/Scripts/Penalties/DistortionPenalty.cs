using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DistortionPenalty : MonoBehaviour, IPenalty
{
    [SerializeField] private Volume postProcessVolume;
    [SerializeField] private float targetLensDistortion = -0.6f;
    [SerializeField] private float targetChromaticAberration = 1f;
    [SerializeField] private float lerpSpeed = 3f;

    private const float DEFAULT_LENS_DISTORTION = 0f;
    private const float DEFAULT_CHROMATIC_ABERRATION = 0f;
    private const float DEFAULT_SCALE = 1f;

    public PenaltyType PenaltyType => PenaltyType.DistortionShader;

    private LensDistortion lensDistortion;
    private ChromaticAberration chromaticAberration;
    private bool isActive;
    private float pulseTimer;

    public void Activate()
    {
        EnsureVolumeProfile();
        isActive = true;
        pulseTimer = 0f;
    }

    public void Deactivate()
    {
        isActive = false;
        if (lensDistortion != null)
        {
            lensDistortion.intensity.Override(DEFAULT_LENS_DISTORTION);
            lensDistortion.scale.Override(DEFAULT_SCALE);
        }
        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.Override(DEFAULT_CHROMATIC_ABERRATION);
        }
    }

    private void EnsureVolumeProfile()
    {
        if (postProcessVolume == null)
        {
            postProcessVolume = FindAnyObjectByType<Volume>();
        }

        if (postProcessVolume != null && postProcessVolume.profile != null)
        {
            if (!postProcessVolume.profile.TryGet(out lensDistortion))
            {
                lensDistortion = postProcessVolume.profile.Add<LensDistortion>(true);
            }
            if (!postProcessVolume.profile.TryGet(out chromaticAberration))
            {
                chromaticAberration = postProcessVolume.profile.Add<ChromaticAberration>(true);
            }
        }
    }

    private void Update()
    {
        if (!isActive || postProcessVolume == null)
            return;

        pulseTimer += Time.deltaTime;

        float lPulse = Mathf.Sin(pulseTimer * 2f) * 0.15f;
        float lDistortionTarget = targetLensDistortion + lPulse;

        if (lensDistortion != null)
        {
            lensDistortion.intensity.Override(
                Mathf.MoveTowards(lensDistortion.intensity.value, lDistortionTarget, Time.deltaTime * lerpSpeed)
            );
            lensDistortion.scale.Override(DEFAULT_SCALE);
        }

        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.Override(
                Mathf.MoveTowards(chromaticAberration.intensity.value, targetChromaticAberration, Time.deltaTime * lerpSpeed)
            );
        }
    }
}
