using UnityEngine;
using UnityEngine.InputSystem;
using System.Text;
using System;

public class PlayerMovement : MonoBehaviour
{
    public event Action AttackTriggered;

    [Header("Movement")]
    public float walkSpeed = 4f;
    public float sprintSpeed = 7f;
    public float inputDeadzone = 0.15f;
    public SurvivalSystem survival;

    [Header("Animation")]
    [SerializeField] float animationSpeed = 0.8f;
    [SerializeField] string speedParam = "Speed";
    [SerializeField] string facingParam = "Facing";
    [SerializeField] string runParam = "Run";
    [SerializeField] string attackParam = "Attack";
    [SerializeField] string hurtParam = "Hurt";
    [SerializeField] string deathParam = "Death";

    [Header("Action Timing")]
    [SerializeField] float attackLockSeconds = 0.2f;

    [Header("Fallback Attack Input")]
    [SerializeField] bool useFallbackAttackInput = true;
    [SerializeField] Key fallbackKeyboardAttackKey = Key.Space;
    [SerializeField] bool allowMouseLeftAsAttack = true;
    [SerializeField] bool allowGamepadSouthAsAttack = true;

    [Header("Debug")]
    [SerializeField, TextArea] string attackBindingHint = "";

    Rigidbody2D rb2d;
    Animator animator;
    PlayerInputActions input;

    Vector2 currentMoveInput = Vector2.zero;
    int currentFacing;
    bool isRunning;
    bool isDead;
    float attackLockUntil;

    int speedParamHash;
    int facingParamHash;
    int runParamHash;
    int attackParamHash;
    int hurtParamHash;
    int deathParamHash;

    void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        if (rb2d != null)
            rb2d.freezeRotation = true;

        animator = GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.applyRootMotion = false;
            animator.speed = animationSpeed;
        }

        input = new PlayerInputActions();
        CacheParameterHashes();
        CacheAttackBindingHint();
    }

    void OnEnable()
    {
        if (input != null)
            input.Enable();
    }

    void OnDisable()
    {
        if (input != null)
            input.Disable();
    }

    void OnDestroy()
    {
        if (input != null)
        {
            input.Dispose();
            input = null;
        }
    }

    void Update()
    {
        if (input == null)
            return;

        Vector2 moveInput = input.Player.Move.ReadValue<Vector2>();
        isRunning = input.Player.Sprint.IsPressed();
        bool attackPressed = input.Player.Attack.WasPressedThisFrame() || WasFallbackAttackPressed();

        float mag = moveInput.magnitude;
        if (mag < inputDeadzone)
            moveInput = Vector2.zero;
        else
            moveInput = moveInput.normalized * Mathf.Clamp01(mag);

        UpdateFacing(moveInput);
        currentMoveInput = moveInput;

        if (animator != null)
        {
            animator.speed = animationSpeed;
            animator.SetInteger(facingParamHash, currentFacing);
            animator.SetFloat(speedParamHash, moveInput.magnitude);
            animator.SetBool(runParamHash, isRunning);
        }

        if (isDead)
        {
            currentMoveInput = Vector2.zero;
            return;
        }

        if (attackPressed && Time.time >= attackLockUntil)
            TriggerAttack();

        float currentSpeed = walkSpeed;

        bool isSprintPressed = Keyboard.current.leftShiftKey.isPressed;

        if (isSprintPressed && survival.CanSprint && currentMoveInput.magnitude > 0f)
        {
            currentSpeed = sprintSpeed;
            survival?.UseStamina(7f * Time.deltaTime);
            survival?.SetSprinting(true);
        }
        else
        {
            survival?.SetSprinting(false);
        }
        // store for FixedUpdate movement
        currentMoveInput = moveInput * currentSpeed;
    }

    void FixedUpdate()
    {
        if (isDead)
            return;

        Vector2 displacement = currentMoveInput * Time.fixedDeltaTime;
        if (rb2d != null)
            rb2d.MovePosition(rb2d.position + displacement);
        else
            transform.position += new Vector3(displacement.x, displacement.y, 0f);
    }

    public void Attack()
    {
        if (isDead)
            return;

        TriggerAttack();
    }

    public void PlayHurt()
    {
        if (isDead || animator == null)
            return;

        animator.ResetTrigger(hurtParamHash);
        animator.SetTrigger(hurtParamHash);
    }

    public void PlayDeath()
    {
        if (isDead || animator == null)
            return;

        isDead = true;
        currentMoveInput = Vector2.zero;
        animator.SetBool(deathParamHash, true);
    }

    public void TakeDamage(int amount = 1)
    {
        if (amount <= 0 || isDead)
            return;

        PlayHurt();
    }

    void UpdateFacing(Vector2 moveInput)
    {
        if (moveInput.sqrMagnitude <= 0f)
            return;

        // Facing values must match the Animator's transition conditions.
        currentFacing = Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y)
            ? (moveInput.x > 0 ? 2 : 1)
            : (moveInput.y > 0 ? 3 : 0);
    }

    bool WasFallbackAttackPressed()
    {
        if (!useFallbackAttackInput)
            return false;

        if (Keyboard.current != null && Keyboard.current[fallbackKeyboardAttackKey].wasPressedThisFrame)
            return true;

        if (allowMouseLeftAsAttack && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            return true;

        if (allowGamepadSouthAsAttack && Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
            return true;

        return false;
    }

    void TriggerAttack()
    {
        if (animator == null)
            return;

        animator.ResetTrigger(attackParamHash);
        animator.SetTrigger(attackParamHash);
        attackLockUntil = Time.time + attackLockSeconds;
        AttackTriggered?.Invoke();
    }

    void CacheParameterHashes()
    {
        speedParamHash = Animator.StringToHash(speedParam);
        facingParamHash = Animator.StringToHash(facingParam);
        runParamHash = Animator.StringToHash(runParam);
        attackParamHash = Animator.StringToHash(attackParam);
        hurtParamHash = Animator.StringToHash(hurtParam);
        deathParamHash = Animator.StringToHash(deathParam);
    }

    void CacheAttackBindingHint()
    {
        if (input == null)
            return;

        var action = input.Player.Attack;
        if (action == null)
            return;

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < action.bindings.Count; i++)
        {
            var binding = action.bindings[i];
            if (binding.isComposite || binding.isPartOfComposite)
                continue;

            string display = action.GetBindingDisplayString(i);
            if (string.IsNullOrEmpty(display))
                continue;

            if (sb.Length > 0)
                sb.Append(" / ");

            sb.Append(display);
        }

        if (sb.Length == 0)
            sb.Append("No Attack bindings found in Input Actions asset");

        attackBindingHint = sb.ToString();
    }
}
