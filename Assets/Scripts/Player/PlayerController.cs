using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private const int MaxPlayerAnimationLevel = 9;

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

    [Header("Combat - Difficulty Damage")]
    [Tooltip("Player base attack damage when the game difficulty is Easy.")]
    [SerializeField] private int attackDamageEasy = 25;
    [Tooltip("Player base attack damage when the game difficulty is Normal.")]
    [SerializeField] private int attackDamageNormal = 20;
    [Tooltip("Player base attack damage when the game difficulty is Hard.")]
    [SerializeField] private int attackDamageHard = 10;

    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    
    [Header("Level & Experience")]
    [SerializeField] private int level = 1;
    [SerializeField] private int experience = 0;
    [SerializeField] private int experiencePerLevel = 100;

    [Header("Level Progression")]
    [SerializeField] private RuntimeAnimatorController[] levelAnimatorControllers = new RuntimeAnimatorController[MaxPlayerAnimationLevel];
    [SerializeField] private int attackDamagePerLevel = 5;
    [SerializeField] private int maxHealthPerLevel = 10;

    [Header("Components")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;

    private Vector2 movement;
    private Vector2 lastMoveDirection = Vector2.down;

    private int currentHealth;
    private bool isDead;
    private bool isAttacking;
    private float nextAttackTime;

    public event System.Action OnAttackPerformed;
    public event System.Action OnHitReceived;
    private SurvivalSystem survivalSystem;
    private int baseAttackDamage;
    private int currentAnimationLevel = -1;
    private bool hasLoadedSave;

    public bool IsSprinting { get; private set; }

    private void Awake()
    {
        // Pick the player's base attack damage based on the chosen difficulty.
        // Easy = 25, Normal = 20, Hard = 10 (Inspector-tunable).
        GameDifficultySettings.Load();
        attackDamage = GetAttackDamageForDifficulty(GameDifficultySettings.Current);
        baseAttackDamage = attackDamage;
        currentHealth = maxHealth;

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

        ApplyLevelProgression();
        DisableOtherRigidbodyControllers();
    }

    private void Start()
    {
        if (!hasLoadedSave)
            currentHealth = maxHealth;

        survivalSystem = FindObjectOfType<SurvivalSystem>();
        ReconcileHealthOnSceneLoad();

        if (animator == null)
            return;

        animator.SetFloat("LastMoveX", 0);
        animator.SetFloat("LastMoveY", -1);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (levelAnimatorControllers == null || levelAnimatorControllers.Length != MaxPlayerAnimationLevel)
            System.Array.Resize(ref levelAnimatorControllers, MaxPlayerAnimationLevel);

        for (int i = 0; i < levelAnimatorControllers.Length; i++)
        {
            if (levelAnimatorControllers[i] != null)
                continue;

            string controllerName = $"Player_Lvl{i + 1}";
            string controllerPath = $"Assets/Animations/Player/{controllerName}/{controllerName}.controller";
            levelAnimatorControllers[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
        }
    }
#endif

    private void Update()
    {
        if (isDead)
        {
            movement = Vector2.zero;
            return;
        }

        ReadMovementInput();

        // Resolve sprinting here (Update) so FootstepSounds reads a current value
        bool wantsToRun = IsRunPressed();
        bool canRun = survivalSystem != null ? survivalSystem.CanSprint : true;
        IsSprinting = wantsToRun && canRun;
        if (survivalSystem != null)
            survivalSystem.SetSprinting(IsSprinting);

        UpdateAttackPoint();
        UpdateAnimator();

        if (IsAttackPressed() && Time.time >= nextAttackTime)
        {
            OnAttackPerformed?.Invoke();
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

        bool mapOpen = MapController.Instance != null && MapController.Instance.IsMapOpen;

        if (keyboard.aKey.isPressed || (!mapOpen && keyboard.leftArrowKey.isPressed))
            moveX = -1f;
        else if (keyboard.dKey.isPressed || (!mapOpen && keyboard.rightArrowKey.isPressed))
            moveX = 1f;

        if (keyboard.sKey.isPressed || (!mapOpen && keyboard.downArrowKey.isPressed))
            moveY = -1f;
        else if (keyboard.wKey.isPressed || (!mapOpen && keyboard.upArrowKey.isPressed))
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

    /// <summary>
    /// Called by SurvivalSystem.OnSceneLoaded to re-wire the reference every
    /// time a scene loads, so DontDestroyOnLoad players always route damage
    /// through SurvivalSystem regardless of whether Start() re-ran.
    /// </summary>
    public void BindSurvivalSystem(SurvivalSystem system)
    {
        survivalSystem = system;
        ReconcileHealthOnSceneLoad();
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        // Re-acquire SurvivalSystem if lost (e.g. DontDestroyOnLoad player
        // whose Start() never re-fires after a scene reload via Retry).
        if (survivalSystem == null)
        {
            survivalSystem = Object.FindAnyObjectByType<SurvivalSystem>();
            if (survivalSystem != null)
                Debug.Log("[PlayerController] TakeDamage: re-acquired SurvivalSystem.");
            else
                Debug.LogWarning("[PlayerController] TakeDamage: SurvivalSystem still null — damage won't update health bar.");
        }

        OnHitReceived?.Invoke();

        if (survivalSystem != null)
        {
            survivalSystem.TakeDamage((float)damage);
            currentHealth = Mathf.RoundToInt(survivalSystem.CurrentHealth);
            SavePlayerData();

            if (isDead)
                return;
        }
        else
        {
            currentHealth -= damage;
            SavePlayerData();
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

        // SurvivalSystem shows game over after Die(); this covers scenes without it.
        if (survivalSystem == null && GameOverUI.Instance != null)
            GameOverUI.Instance.ShowAfterDelay(1.5f);
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
        if (saveData.attackDamage > 0)
            attackDamage = saveData.attackDamage;

        hasLoadedSave = true;
        ApplyLevelProgression();
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        SyncSurvivalHealth();
        
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

        if (survivalSystem != null)
            currentHealth = Mathf.RoundToInt(survivalSystem.CurrentHealth);

        SaveManager.Instance.UpdatePlayerStats(
            currentHealth,
            maxHealth,
            level,
            experience,
            attackDamage,
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
        maxHealth += maxHealthPerLevel;
        currentHealth = maxHealth;
        ApplyLevelProgression();
        SyncSurvivalHealth();

        Debug.Log($"LEVEL UP! Now Level {level}, HP {currentHealth}/{maxHealth}, Power {attackDamage}, Animation Player_Lvl{currentAnimationLevel}");
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

    private void ApplyLevelProgression()
    {
        level = Mathf.Max(1, level);
        attackDamage = baseAttackDamage + ((level - 1) * attackDamagePerLevel);
        ApplyAnimationForLevel();
    }

    private int GetAttackDamageForDifficulty(GameDifficulty difficulty)
    {
        switch (difficulty)
        {
            case GameDifficulty.Easy:   return Mathf.Max(1, attackDamageEasy);
            case GameDifficulty.Hard:   return Mathf.Max(1, attackDamageHard);
            case GameDifficulty.Normal:
            default:                    return Mathf.Max(1, attackDamageNormal);
        }
    }

    private void SyncSurvivalHealth()
    {
        if (survivalSystem != null)
            survivalSystem.SetHealthStats(maxHealth, currentHealth);
    }

    /// <summary>
    /// Reconciles health with the persistent SurvivalSystem when a scene loads.
    /// The SurvivalSystem is DontDestroyOnLoad and carries the player's real
    /// health across teleports, so a freshly-spawned player must adopt its
    /// values instead of overwriting them with this scene's default full health.
    /// When the player was just restored from a save file the save is
    /// authoritative, so we push those values into the SurvivalSystem instead.
    /// </summary>
    private void ReconcileHealthOnSceneLoad()
    {
        if (survivalSystem == null)
            return;

        if (hasLoadedSave)
        {
            survivalSystem.SetHealthStats(maxHealth, currentHealth);
        }
        else
        {
            maxHealth = Mathf.Max(1, Mathf.RoundToInt(survivalSystem.MaxHealth));
            currentHealth = Mathf.Clamp(Mathf.RoundToInt(survivalSystem.CurrentHealth), 0, maxHealth);
        }
    }

    private void ApplyAnimationForLevel()
    {
        if (animator == null || levelAnimatorControllers == null)
            return;

        int animationLevel = GetAnimationLevelForPlayerLevel(level);
        int controllerIndex = animationLevel - 1;
        if (controllerIndex < 0 || controllerIndex >= levelAnimatorControllers.Length)
            return;

        RuntimeAnimatorController controller = levelAnimatorControllers[controllerIndex];
        if (controller == null)
        {
            Debug.LogWarning($"PlayerController: Missing Animator Controller for Player_Lvl{animationLevel}.");
            return;
        }

        if (animator.runtimeAnimatorController == controller)
        {
            currentAnimationLevel = animationLevel;
            return;
        }

        animator.runtimeAnimatorController = controller;
        currentAnimationLevel = animationLevel;

        animator.SetFloat("LastMoveX", lastMoveDirection.x);
        animator.SetFloat("LastMoveY", lastMoveDirection.y);
        animator.SetBool("IsAttacking", isAttacking);
        animator.SetBool("IsDead", isDead);
    }

    private int GetAnimationLevelForPlayerLevel(int playerLevel)
    {
        if (playerLevel <= 5) return 1;
        if (playerLevel <= 10) return 2;
        if (playerLevel <= 15) return 3;
        if (playerLevel <= 25) return 4;
        if (playerLevel <= 35) return 5;
        if (playerLevel <= 45) return 6;
        if (playerLevel <= 60) return 7;
        if (playerLevel <= 75) return 8;
        return 9;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(GetAttackCenterWorld(), attackRange);
    }
}
