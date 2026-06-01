using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 10, -8);
    public float smoothSpeed = 5f;

    [Header("Underwater View")]
    public bool forceUnderwaterView = true;
    public float waterSurfaceY = 14f;
    public float underwaterDepth = 4f;
    public Color underwaterBackgroundColor = new Color(0.02f, 0.16f, 0.22f, 1f);
    public Color underwaterFogColor = new Color(0.10f, 0.42f, 0.48f, 1f);
    public float underwaterFogDensity = 0.06f;

    [Header("Zoom")]
    public float zoomSpeed = 2f;
    public float minZoomDistance = 6f;
    public float maxZoomDistance = 18f;

    [Header("Fade In")]
    public float fadeInDuration = 1.5f;
    
    private float currentZoomDistance;
    private Vector3 offsetDirection;
    private CanvasGroup fadeOverlay;
    private Camera followCamera;
    private float defaultFOV = 60f;
    [Header("Dash FOV")]
    public float dashFOV = 75f;
    public float fovLerpSpeed = 8f;
    private bool isDashing = false;

    void Start()
    {
        followCamera = GetComponent<Camera>();

        if (target == null)
        {
            target = FindFirstObjectByType<PlayerMovement>()?.transform;
        }
        
        offsetDirection = offset.normalized;
        currentZoomDistance = offset.magnitude;
        
        // Set initial camera position
        Vector3 initialPosition = GetDesiredCameraPosition();
        transform.position = initialPosition;
        transform.LookAt(target);

        ApplyUnderwaterLook();

        // capture default FOV from the camera
        if (followCamera != null)
        {
            defaultFOV = followCamera.fieldOfView;
        }

        // Find fade overlay and start fade in
        fadeOverlay = FindFirstObjectByType<CanvasGroup>();
        if (fadeOverlay != null)
        {
            fadeOverlay.alpha = 1f;
            fadeOverlay.blocksRaycasts = true;
            fadeOverlay.interactable = false;

            var image = fadeOverlay.GetComponent<Image>();
            if (image != null)
            {
                image.raycastTarget = true;
            }

            StartCoroutine(FadeIn());
        }
    }

    private IEnumerator FadeIn()
    {
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            fadeOverlay.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeInDuration);
            yield return null;
        }
        fadeOverlay.alpha = 0f;
        
        // Disable raycast blocking so clicks go through
        var image = fadeOverlay.GetComponent<Image>();
        if (image != null)
        {
            image.raycastTarget = false;
        }
    }

    void LateUpdate()
    {
        HandleZoom();

        Vector3 desiredPosition = GetDesiredCameraPosition();
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );

        transform.LookAt(target);
        ApplyUnderwaterLook();

        // Smoothly lerp FOV when dashing
        if (followCamera != null)
        {
            float targetFOV = isDashing ? dashFOV : defaultFOV;
            followCamera.fieldOfView = Mathf.Lerp(followCamera.fieldOfView, targetFOV, fovLerpSpeed * Time.deltaTime);
        }
    }

    public void SetDashing(bool dashing)
    {
        isDashing = dashing;
    }

    private Vector3 GetDesiredCameraPosition()
    {
        Vector3 desiredPosition = target.position + (offsetDirection * currentZoomDistance);

        if (forceUnderwaterView)
        {
            desiredPosition.y = Mathf.Min(desiredPosition.y, waterSurfaceY - underwaterDepth);
        }

        return desiredPosition;
    }

    private void ApplyUnderwaterLook()
    {
        if (!forceUnderwaterView)
        {
            return;
        }
        bool isUnderwater = target != null && target.position.y < waterSurfaceY;

        // fog + camera background
        if (isUnderwater)
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = underwaterFogColor;
            RenderSettings.fogDensity = underwaterFogDensity;

            if (followCamera != null)
            {
                followCamera.clearFlags = CameraClearFlags.SolidColor;
                followCamera.backgroundColor = underwaterBackgroundColor;
            }
        }
        else
        {
            // revert minimal settings
            RenderSettings.fog = false;
            if (followCamera != null)
            {
                followCamera.clearFlags = CameraClearFlags.Skybox;
                // keep defaultFOV background unchanged
            }
        }

        // apply post-processing overrides (vignette, bloom, color grading, etc.)
        UnderwaterEffectsManager.SetUnderwater(isUnderwater, underwaterBackgroundColor, underwaterFogColor, underwaterFogDensity);
    }

    void HandleZoom()
    {
        float scrollInput = Mouse.current.scroll.y.ReadValue();
        if (scrollInput != 0f)
        {
            currentZoomDistance -= scrollInput * zoomSpeed * 0.01f;
            currentZoomDistance = Mathf.Clamp(currentZoomDistance, minZoomDistance, maxZoomDistance);
        }
    }
}
