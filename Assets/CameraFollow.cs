using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 10, -8);
    public float smoothSpeed = 5f;

    [Header("Zoom")]
    public float zoomSpeed = 2f;
    public float minZoomDistance = 6f;
    public float maxZoomDistance = 18f;

    [Header("Fade In")]
    public float fadeInDuration = 1.5f;
    
    private float currentZoomDistance;
    private Vector3 offsetDirection;
    private CanvasGroup fadeOverlay;

    void Start()
    {
        if (target == null)
        {
            target = FindFirstObjectByType<PlayerMovement>()?.transform;
        }
        
        offsetDirection = offset.normalized;
        currentZoomDistance = offset.magnitude;
        
        // Set initial camera position
        Vector3 initialPosition = target.position + (offsetDirection * currentZoomDistance);
        transform.position = initialPosition;
        transform.LookAt(target);

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

        Vector3 desiredPosition = target.position + (offsetDirection * currentZoomDistance);
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );

        transform.LookAt(target);
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
