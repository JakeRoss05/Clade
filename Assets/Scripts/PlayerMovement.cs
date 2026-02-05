using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Evolution Stats")]
    public float baseSpeed = 5f;
    public float sizeMultiplier = 1f;


    private PlayerInputActions input;
    private Vector2 moveInput;
    private Rigidbody rb;

    private void Awake()
    {
        input = new PlayerInputActions();

        input.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        input.Player.Move.canceled += ctx => moveInput = Vector2.zero;
    }

    private void OnEnable()
    {
        input.Enable();
    }

    private void OnDisable()
    {
        input.Disable();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y);

        if (movement != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            rb.MovePosition(
            rb.position + movement * (baseSpeed / sizeMultiplier) * Time.fixedDeltaTime
            );

        }

        transform.localScale = Vector3.one * sizeMultiplier;
    }
}
