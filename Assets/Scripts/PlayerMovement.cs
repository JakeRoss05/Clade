using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Evolution Stats")]
    public float baseSpeed = 5f;
    public float sizeMultiplier = 1f;

    [Header("Boost")]
    public float boostMultiplier = 2f;
    public float boostEnergyCostPerSecond = 10f;

    private bool isBoosting;
    private Energy energy;


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
        energy = GetComponent<Energy>();
    }

    private void FixedUpdate()
    {
        isBoosting = Keyboard.current.leftShiftKey.isPressed;
        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y);

        if (movement != Vector3.zero)
        {
            float currentSpeed = baseSpeed / sizeMultiplier;
            if (Keyboard.current.leftShiftKey.isPressed && energy != null && energy.currentEnergy > 0)
            {
                currentSpeed *= boostMultiplier;
                energy.currentEnergy -= boostEnergyCostPerSecond * energy.boostEnergyCostMultiplier * Time.fixedDeltaTime;
            }

            rb.MovePosition(rb.position + movement.normalized * currentSpeed * Time.fixedDeltaTime);
        }

        transform.localScale = Vector3.one * sizeMultiplier;
    }
}
