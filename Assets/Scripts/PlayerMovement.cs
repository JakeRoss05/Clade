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
    private CameraFollow cameraFollow;

    [Header("Dash")]
    public bool dashUnlocked = false;
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    public TrailRenderer dashTrail;

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
        cameraFollow = Camera.main != null ? Camera.main.GetComponent<CameraFollow>() : null;
        if (rb != null)
        {
            rb.constraints |= RigidbodyConstraints.FreezePositionY;
            rb.constraints |= RigidbodyConstraints.FreezeRotationX;
            rb.constraints |= RigidbodyConstraints.FreezeRotationZ;
        }

        // Ensure dash trail exists and is configured with a nice look
        if (dashTrail == null)
        {
            dashTrail = gameObject.GetComponent<TrailRenderer>();
            if (dashTrail == null)
            {
                dashTrail = gameObject.AddComponent<TrailRenderer>();
            }
        }

        if (dashTrail != null)
        {
            dashTrail.time = 0.25f;
            dashTrail.startWidth = 0.6f;
            dashTrail.endWidth = 0.0f;
            dashTrail.autodestruct = false;
            dashTrail.emitting = false;
            dashTrail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            dashTrail.receiveShadows = false;

            Shader spriteShader = Shader.Find("Sprites/Default");
            if (spriteShader != null)
            {
                dashTrail.material = new Material(spriteShader);
            }

            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(0.2f, 0.9f, 1f, 1f), 0.0f),
                    new GradientColorKey(new Color(0.0f, 0.5f, 1f, 0.6f), 0.5f),
                    new GradientColorKey(new Color(0.0f, 0.2f, 0.6f, 0f), 1.0f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.6f, 0.5f),
                    new GradientAlphaKey(0f, 1f)
                }
            );

            dashTrail.colorGradient = gradient;
            dashTrail.numCornerVertices = 8;
            dashTrail.numCapVertices = 8;
        }
    }

    private void FixedUpdate()
    {
        isBoosting = Keyboard.current.leftShiftKey.isPressed;
        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y);

        // Handle dash cooldown timer
        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.fixedDeltaTime;

        // Start dash on space if unlocked (checked in Update) — during dash, override movement
        if (isDashing)
        {
            // Dash movement
            rb.MovePosition(rb.position + movement.normalized * dashSpeed * Time.fixedDeltaTime);
        }
        else
        {
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
        }

        transform.localScale = Vector3.one * sizeMultiplier;
    }

    private void Update()
    {
        // Handle dash input (use Update to catch key press)
        if (dashUnlocked && !isDashing && dashCooldownTimer <= 0f && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            StartDash();
        }

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
            {
                EndDash();
            }
        }
    }

    void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;
        if (dashTrail != null)
            dashTrail.emitting = true;
        if (cameraFollow != null)
            cameraFollow.SetDashing(true);
    }

    void EndDash()
    {
        isDashing = false;
        if (dashTrail != null)
            dashTrail.emitting = false;
        if (cameraFollow != null)
            cameraFollow.SetDashing(false);
    }

    public void EnableDash()
    {
        dashUnlocked = true;
        if (dashTrail != null)
            dashTrail.emitting = false; // keep off until dashing
    }
}
