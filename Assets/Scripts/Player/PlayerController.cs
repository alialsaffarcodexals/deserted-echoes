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
    
    [Header("Level & Experience")]
    [SerializeField] private int level = 1;
    [SerializeField] private int experience = 0;
    [SerializeField] private int experiencePerLevel = 100;

    [Header("Components")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;

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

        float currentSpeed = IsRunPressed() ? runSpeed : walkSpeed;

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
            // Fallback: search all layers except the player layer
            int allLayersExceptPlayer = ~LayerMask.GetMask("Default");
            if (gameObject.layer != LayerMask.NameToLayer("Default"))
            {
                allLayersExceptPlayer = ~LayerMask.GetMask(LayerMask.LayerToName(gameObject.layer));
            }
            
            hitEnemies = Physics2D.OverlapCircleAll(
                attackCenter,
                attackRange,
                allLayersExceptPlayer
            );
        }

        // Filter out the player itself to avoid self-damage
        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.gameObject == gameObject)
                continue;

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

        currentHealth -= damage;
        SavePlayerData();

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        if (animator != null)
            animator.SetTrigger("Hurt");
    }

    private void Die()
    {
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

    /// <summary>
    /// Loads player data from save file.
    /// </summary>
    public void LoadFromSave(SaveData saveData)
    {
        if (saveData == null)
            return;

        currentHealth = saveData.currentHealth;
        maxHealth = saveData.maxHealth;
        level = saveData.level;
        experience = saveData.experience;
        attackDamage = saveData.attackDamage;
        
        // Restore position if different scene
        if (saveData.lastSceneName == UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
        {
            transform.position = new Vector2(saveData.playerPositionX, saveData.playerPositionY);
        }

        Debug.Log($"Player loaded: HP={currentHealth}/{maxHealth}, Level={level}, Exp={experience}");
    }

    /// <summary>
    /// Saves player data to SaveManager.
    /// </summary>
    public void SavePlayerData()
    {
        if (SaveManager.Instance == null)
            return;

        SaveManager.Instance.UpdatePlayerStats(
            currentHealth,
            maxHealth,
            level,
            experience,
            transform.position
        );
    }

    /// <summary>
    /// Adds experience to the player.
    /// </summary>
    public void AddExperience(int exp)
    {
        experience += exp;
        
        // Check for level up
        while (experience >= experiencePerLevel)
        {
            experience -= experiencePerLevel;
            LevelUp();
        }

        SavePlayerData();
        Debug.Log($"Experience: {experience}/{experiencePerLevel}");
    }

    /// <summary>
    /// Levels up the player.
    /// </summary>
    private void LevelUp()
    {
        level++;
        maxHealth += 10;
        currentHealth = maxHealth;
        attackDamage += 5;

        Debug.Log($"LEVEL UP! Now Level {level}");
        SavePlayerData();
    }

    /// <summary>
    /// Gets player level.
    /// </summary>
    public int GetLevel()
    {
        return level;
    }

    /// <summary>
    /// Gets player current health.
    /// </summary>
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    /// <summary>
    /// Gets player max health.
    /// </summary>
    public int GetMaxHealth()
    {
        return maxHealth;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(GetAttackCenterWorld(), attackRange);
    }
}
