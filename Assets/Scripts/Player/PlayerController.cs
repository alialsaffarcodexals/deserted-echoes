using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;

    [Header("Combat")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackPointDistance = 1f;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private int attackDamage = 25;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Health")]
    [SerializeField] private int maxHealth = 100;

    [Header("Components")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;

    private Vector2 movement;
    private Vector2 lastMoveDirection = Vector2.down;

    private int currentHealth;
    private bool isDead;
    private bool isAttacking;
    private float nextAttackTime;
    private SurvivalSystem survivalSystem;

    public bool IsSprinting { get; private set; }

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.simulated = true;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }

        if (animator == null)
            animator = GetComponentInChildren<Animator>(true);

        if (animator != null)
        {
            animator.applyRootMotion = false;
        }

        DisableOtherRigidbodyControllers();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        survivalSystem = FindObjectOfType<SurvivalSystem>();

        if (animator == null)
            return;

        animator.SetFloat("LastMoveX", 0);
        animator.SetFloat("LastMoveY", -1);
    }

    private void Update()
    {
        if (isDead)
        {
            movement = Vector2.zero;
            return;
        }

        ReadMovementInput();

        UpdateAttackPoint();
        UpdateAnimator();

        if (IsAttackPressed() && Time.time >= nextAttackTime)
        {
            Attack();
        }
    }

    private void FixedUpdate()
    {
        if (isDead)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (movement == Vector2.zero)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        bool wantsToRun = IsRunPressed();
        bool canRun = survivalSystem != null ? survivalSystem.CanSprint : true;

        IsSprinting = wantsToRun && canRun;

        if (survivalSystem != null)
            survivalSystem.SetSprinting(IsSprinting);

        float currentSpeed = IsSprinting ? runSpeed : walkSpeed;

        rb.MovePosition(rb.position + movement * currentSpeed * Time.fixedDeltaTime);
    }

    // Movement input is read here using direct key checks only.
    private void ReadMovementInput()
    {
        float moveX = 0f;
        float moveY = 0f;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            movement = Vector2.zero;
            return;
        }

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            moveX = -1f;
        else if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            moveX = 1f;

        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            moveY = -1f;
        else if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            moveY = 1f;

        Vector2 inputMovement = new Vector2(moveX, moveY);
        movement = inputMovement.sqrMagnitude > 0f ? inputMovement.normalized : Vector2.zero;

        if (movement != Vector2.zero)
        {
            lastMoveDirection = GetMainDirection(movement);
        }
    }

    private void DisableOtherRigidbodyControllers()
    {
        PlayerMovement playerMovement = GetComponent<PlayerMovement>();

        if (playerMovement != null)
            playerMovement.enabled = false;
    }

    private bool IsRunPressed()
    {
        Keyboard keyboard = Keyboard.current;
        return keyboard != null && keyboard.leftShiftKey.isPressed;
    }

    private bool IsAttackPressed()
    {
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;

        bool keyboardAttack = keyboard != null && keyboard.jKey.wasPressedThisFrame;
        bool mouseAttack = mouse != null && mouse.leftButton.wasPressedThisFrame;

        return keyboardAttack || mouseAttack;
    }

    private void UpdateAnimator()
    {
        if (animator == null)
            return;

        float speedValue = 0f;

        if (movement != Vector2.zero)
        {
            speedValue = IsRunPressed() ? runSpeed : walkSpeed;

            animator.SetFloat("MoveX", movement.x);
            animator.SetFloat("MoveY", movement.y);
            animator.SetFloat("LastMoveX", lastMoveDirection.x);
            animator.SetFloat("LastMoveY", lastMoveDirection.y);
        }
        else
        {
            animator.SetFloat("MoveX", 0);
            animator.SetFloat("MoveY", 0);
        }

        animator.SetFloat("Speed", speedValue);
        animator.SetBool("IsAttacking", isAttacking);
        animator.SetBool("IsDead", isDead);
    }

    private void UpdateAttackPoint()
    {
        if (attackPoint == null)
            return;

        attackPoint.position = GetAttackCenterWorld();
    }

    private Vector2 GetAttackCenterWorld()
    {
        return (Vector2)transform.position + lastMoveDirection * attackPointDistance;
    }

    private Vector2 GetMainDirection(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            if (direction.x > 0)
                return Vector2.right;
            else
                return Vector2.left;
        }
        else
        {
            if (direction.y > 0)
                return Vector2.up;
            else
                return Vector2.down;
        }
    }

    private void Attack()
    {
        if (isDead || isAttacking)
            return;

        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;

        if (animator != null)
        {
            animator.SetBool("IsAttacking", true);
            animator.SetTrigger("Attack");
        }

        Vector2 attackCenter = GetAttackCenterWorld();

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            attackCenter,
            attackRange,
            enemyLayer
        );

        if (hitEnemies.Length == 0)
        {
            // Fallback for cases where the Enemy layer mask is not set correctly.
            hitEnemies = Physics2D.OverlapCircleAll(
            attackCenter,
            attackRange
        );
        }

        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.SendMessage("TakeDamage", attackDamage, SendMessageOptions.DontRequireReceiver);
        }

        Invoke(nameof(EndAttack), attackCooldown);
    }

    private void EndAttack()
    {
        isAttacking = false;

        if (animator != null)
            animator.SetBool("IsAttacking", false);
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        if (survivalSystem != null)
        {
            // SurvivalSystem is the single health authority — it drives the bar
            // and calls Die() via OnDeath() when it reaches 0
            survivalSystem.TakeDamage((float)damage);
        }
        else
        {
            // Fallback: no SurvivalSystem in scene, track internally
            currentHealth -= damage;
            if (currentHealth <= 0)
            {
                Die();
                return;
            }
        }

        if (animator != null)
            animator.SetTrigger("Hurt");
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        movement = Vector2.zero;

        if (animator != null)
        {
            animator.SetBool("IsDead", true);
            animator.SetTrigger("Death");
        }

        rb.linearVelocity = Vector2.zero;
    }

    public bool TryGetEnemyAttackTargetPosition(out Vector2 targetPosition)
    {
        targetPosition = GetAttackCenterWorld();
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(GetAttackCenterWorld(), attackRange);
    }
}
