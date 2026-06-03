using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class UnderwaterEffectsManager
{
    static Volume globalVolume;
    static VolumeProfile profile;

    static Bloom bloom;
    static Vignette vignette;
    static ColorAdjustments colorAdjust;
    static ChromaticAberration chroma;
    static DepthOfField dof;

    static bool initialized = false;

    static void Init()
    {
        if (initialized) return;
        var vols = Object.FindObjectsByType<Volume>(FindObjectsSortMode.None);
        foreach (var v in vols)
        {
            if (v.isGlobal && v.sharedProfile != null)
            {
                globalVolume = v;
                profile = v.sharedProfile;
                break;
            }
        }

        if (profile != null)
        {
            profile.TryGet(out bloom);
            profile.TryGet(out vignette);
            profile.TryGet(out colorAdjust);
            profile.TryGet(out chroma);
            profile.TryGet(out dof);
        }

        initialized = true;
    }

    public static void SetUnderwater(bool on, Color? bg = null, Color? fog = null, float fogDensity = 0.06f)
    {
        Init();

        RenderSettings.fog = on;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        if (fog.HasValue) RenderSettings.fogColor = fog.Value;
        RenderSettings.fogDensity = on ? fogDensity : 0f;

        if (globalVolume != null && profile != null)
        {
            // enable/disable and tweak settings gracefully if they exist
            if (bloom != null) { bloom.active = on; bloom.intensity.value = on ? 1.4f : 0.6f; bloom.threshold.value = on ? 0.3f : 1f; }
            if (vignette != null) { vignette.active = on; vignette.intensity.value = on ? 0.45f : 0f; vignette.smoothness.value = 0.6f; }
            if (colorAdjust != null)
            {
                colorAdjust.active = on;
                colorAdjust.colorFilter.value = on ? new Color(0.55f, 0.9f, 1f) : Color.white;
                colorAdjust.saturation.value = on ? -10f : 0f;
                colorAdjust.postExposure.value = on ? -0.2f : 0f;
            }
            if (chroma != null) { chroma.active = on; chroma.intensity.value = on ? 0.12f : 0f; }
            if (dof != null) { dof.active = on; dof.focusDistance.value = on ? 2f : 10f; dof.aperture.value = on ? 8f : 32f; }
        }
    }
}
