using UnityEngine;

public class SlimeController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 1.5f;
    [SerializeField] private float runSpeed = 3.5f;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 6f;
    [SerializeField] private float runRange = 3f;
    [SerializeField] private float attackRange = 0.8f;
    [SerializeField] private Vector2 playerTargetOffset = new Vector2(0f, -0.3f);

    [Header("Combat")]
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackCooldown = 1f;

    [Header("Health")]
    [SerializeField] private int maxHealth = 50;
    [SerializeField] private float deathDestroyDelay = 1.2f;

    [Header("Components")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;

    private Transform player;
    private PlayerController playerController;
    private Collider2D[] slimeColliders;

    private Vector2 movement;
    private Vector2 lastMoveDirection = Vector2.down;

    private int currentHealth;
    private bool isDead;
    private bool isAttacking;
    private float nextAttackTime;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        slimeColliders = GetComponentsInChildren<Collider2D>();
    }

    private void Start()
    {
        currentHealth = maxHealth;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
            playerController = playerObject.GetComponent<PlayerController>();
            IgnorePlayerBodyCollisions(playerObject);
        }

        animator.SetFloat("LastMoveX", 0);
        animator.SetFloat("LastMoveY", -1);
    }

    private void Update()
    {
        if (isDead)
            return;

        if (player == null || isAttacking)
        {
            movement = Vector2.zero;
            UpdateAnimator();
            return;
        }

        HandleSlimeLogic();
        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        if (isDead || isAttacking || player == null)
        {
            movement = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 playerTargetPosition = GetPlayerTargetPosition();
        float distanceToPlayer = Vector2.Distance(transform.position, playerTargetPosition);

        if (distanceToPlayer <= detectionRange && distanceToPlayer > attackRange)
        {
            float speed = GetCurrentMoveSpeed(distanceToPlayer);
            rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);
        }
        else
        {
            movement = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void HandleSlimeLogic()
    {
        if (isAttacking || isDead)
        {
            movement = Vector2.zero;
            return;
        }

        Vector2 playerTargetPosition = GetPlayerTargetPosition();
        float distanceToPlayer = Vector2.Distance(transform.position, playerTargetPosition);

        if (distanceToPlayer > detectionRange)
        {
            movement = Vector2.zero;
            return;
        }

        Vector2 directionToPlayer = (playerTargetPosition - (Vector2)transform.position).normalized;
        movement = directionToPlayer;

        if (movement != Vector2.zero)
        {
            lastMoveDirection = GetMainDirection(movement);
        }

        if (distanceToPlayer <= attackRange)
        {
            movement = Vector2.zero;

            if (Time.time >= nextAttackTime)
            {
                AttackPlayer();
            }
        }
    }

    private float GetCurrentMoveSpeed(float distanceToPlayer)
    {
        if (distanceToPlayer <= runRange)
            return runSpeed;

        return walkSpeed;
    }

    private Vector2 GetPlayerTargetPosition()
    {
        return (Vector2)player.position + playerTargetOffset;
    }

    private void AttackPlayer()
    {
        if (isDead || playerController == null)
            return;

        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;

        animator.SetBool("IsAttacking", true);

        playerController.TakeDamage(attackDamage);

        Invoke(nameof(EndAttack), attackCooldown);
    }

    private void IgnorePlayerBodyCollisions(GameObject playerObject)
    {
        Collider2D[] playerColliders = playerObject.GetComponentsInChildren<Collider2D>();

        foreach (Collider2D slimeCollider in slimeColliders)
        {
            if (slimeCollider == null || slimeCollider.isTrigger)
                continue;

            foreach (Collider2D playerCollider in playerColliders)
            {
                if (playerCollider == null || playerCollider.isTrigger)
                    continue;

                Physics2D.IgnoreCollision(slimeCollider, playerCollider, true);
            }
        }
    }

    private void EndAttack()
    {
        isAttacking = false;
        animator.SetBool("IsAttacking", false);
    }

    private void UpdateAnimator()
    {
        float speedValue = 0f;

        if (movement != Vector2.zero && player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, GetPlayerTargetPosition());
            speedValue = GetCurrentMoveSpeed(distanceToPlayer);

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

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        animator.SetTrigger("Hurt");
    }

    private void Die()
    {
        isDead = true;
        movement = Vector2.zero;

        animator.SetBool("IsDead", true);
        animator.SetTrigger("Death");

        rb.linearVelocity = Vector2.zero;

        Destroy(gameObject, deathDestroyDelay);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.DrawWireSphere(transform.position, runRange);
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
