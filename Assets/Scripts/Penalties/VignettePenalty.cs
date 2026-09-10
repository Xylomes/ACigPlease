using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VignettePenalty : MonoBehaviour, IPenalty
{
    [SerializeField] private Volume postProcessVolume;
    [SerializeField] private float targetIntensity = 0.85f;
    [SerializeField] private float targetSmoothness = 0.5f;
    [SerializeField] private float lerpSpeed = 3f;

    private const float DEFAULT_INTENSITY = 0f;
    private const float DEFAULT_SMOOTHNESS = 0.2f;

    public PenaltyType PenaltyType => PenaltyType.Vignette;

    private Vignette vignette;
    private bool isActive;

    public void Activate()
    {
        EnsureVolumeProfile();
        isActive = true;
    }

    public void Deactivate()
    {
        isActive = false;
        if (vignette != null)
        {
            vignette.intensity.Override(DEFAULT_INTENSITY);
            vignette.smoothness.Override(DEFAULT_SMOOTHNESS);
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
            if (!postProcessVolume.profile.TryGet(out vignette))
            {
                vignette = postProcessVolume.profile.Add<Vignette>(true);
            }
        }
    }

    private void Update()
    {
        if (!isActive || vignette == null)
            return;

        vignette.intensity.Override(Mathf.MoveTowards(vignette.intensity.value, targetIntensity, Time.deltaTime * lerpSpeed));
        vignette.smoothness.Override(Mathf.MoveTowards(vignette.smoothness.value, targetSmoothness, Time.deltaTime * lerpSpeed));
    }
}
