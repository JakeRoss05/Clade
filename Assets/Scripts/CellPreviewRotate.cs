using UnityEngine;
using UnityEngine.EventSystems;

public class CellPreviewRotate : MonoBehaviour, IDragHandler
{
    [SerializeField] private Transform target;
    [SerializeField] private float rotationSpeed = 0.25f;
    [SerializeField] private bool autoSpin = true;
    [SerializeField] private float autoSpinSpeed = 18f;

    private Vector2 lastDragDelta;

    private void Awake()
    {
        if (target == null)
        {
            target = transform;
        }
    }

    private void Update()
    {
        if (autoSpin && target != null)
        {
            target.Rotate(0f, autoSpinSpeed * Time.unscaledDeltaTime, 0f, Space.World);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (target == null)
        {
            return;
        }

        lastDragDelta = eventData.delta;

        float yaw = -lastDragDelta.x * rotationSpeed;
        float pitch = lastDragDelta.y * rotationSpeed;

        target.Rotate(Vector3.up, yaw, Space.World);
        target.Rotate(Vector3.right, pitch, Space.Self);
    }
}
