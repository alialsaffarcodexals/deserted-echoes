using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyControllerBase : MonoBehaviour
{
    protected enum Difficulty
    {
        Easy,
        Normal,
        Hard
    }

    protected enum AttackAnimationMode
    {
        Single,
        Directional
    }

    [Serializable]
    protected struct DifficultyProfile
    {
        [Min(0.1f)] public float moveSpeedMultiplier;
        [Min(0.1f)] public float damageMultiplier;
        [Min(0.1f)] public float healthMultiplier;
        [Min(0.1f)] public float attackCooldownMultiplier;
    }

    [Header("Difficulty")]
    [SerializeField] private Difficulty difficulty = Difficulty.Normal;
    [SerializeField] private DifficultyProfile easyProfile;
    [SerializeField] private DifficultyProfile normalProfile;
    [SerializeField] private DifficultyProfile hardProfile;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 1.5f;
    [SerializeField] private float runSpeed = 3f;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 6f;
    [SerializeField] private float runRange = 3f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float chaseStopDistance = 1f;
    [SerializeField] private Vector2 fallbackTargetOffset = new Vector2(0f, -0.3f);

    [Header("Combat")]
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackAnimationDuration = 0.5f;

    [Header("Health")]
    [SerializeField] private int maxHealth = 50;
    [SerializeField] private float deathDestroyDelay = 1.2f;
    [SerializeField] private float damageDistanceToPlayer = 1f;

    [Header("Score")]
    [SerializeField] private int scoreValue = 100;

    [Header("Animation")]
    [SerializeField] private AttackAnimationMode attackAnimationMode = AttackAnimationMode.Single;

    [Header("Components")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;

    protected Transform Player => player;

    private Transform player;
    private PlayerController playerController;
    private Collider2D[] enemyColliders;
    private Collider2D[] playerColliders;

    private Vector2 movement;
    private Vector2 lastMoveDirection = Vector2.down;
    private Vector2 currentTargetPosition;

    private int currentHealth;
    private bool isDead;
    private bool isAttacking;

    public event System.Action OnAttackPerformed;
    public event System.Action OnHitReceived;
    private float nextAttackTime;
    private float currentTargetDistance;
    private float currentPlayerBodyDistance;

    private float resolvedWalkSpeed;
    private float resolvedRunSpeed;
    private int resolvedAttackDamage;
    private float resolvedAttackCooldown;
    private int resolvedMaxHealth;

    private bool hasAttackTrigger;
    private bool hasAttackIdleTrigger;
    private bool hasAttackWalkTrigger;
    private bool hasAttackRunTrigger;
    private bool hasHurtTrigger;
    private bool hasDeathTrigger;
    private int hurtTriggerHash;
    private int enemyHurtStateHash;
    private int hurtStateHash;

    private void OnValidate()
    {
        detectionRange = Mathf.Max(0.1f, detectionRange);
        runRange = Mathf.Clamp(runRange, 0.1f, detectionRange);
        attackRange = Mathf.Clamp(attackRange, 0.05f, runRange);

        // Allow chaseStopDistance to be configured independently of attackRange.
        // It may be set larger than attackRange intentionally to make enemies stop
        // at a comfortable distance before attacking. Clamp to detectionRange.
        chaseStopDistance = Mathf.Clamp(chaseStopDistance, 0f, detectionRange);

        // Allow contact damage radius to be configured up to detectionRange.
        damageDistanceToPlayer = Mathf.Clamp(damageDistanceToPlayer, 0.05f, detectionRange);
    }

    protected virtual void Awake()
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

        CacheAnimatorParameters();

        enemyColliders = GetComponentsInChildren<Collider2D>();
        ResolveDifficultyStats();
    }

    protected virtual void Start()
    {
        currentHealth = resolvedMaxHealth;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            playerController = playerObject.GetComponent<PlayerController>();
            CachePlayerBodyColliders(playerObject);
            IgnorePlayerBodyCollisions(playerObject);

            currentTargetPosition = GetPlayerTargetPosition();
            currentTargetDistance = Vector2.Distance(transform.position, currentTargetPosition);
            currentPlayerBodyDistance = GetDistanceToPlayerBody(out _);
        }
        else
        {
            currentTargetPosition = transform.position;
            currentTargetDistance = 0f;
            currentPlayerBodyDistance = float.MaxValue;
        }

        IgnoreEnemyToEnemyCollisions();

        if (animator != null)
        {
            animator.SetFloat("LastMoveX", 0f);
            animator.SetFloat("LastMoveY", -1f);
        }
    }

    protected virtual void Update()
    {
        if (isDead)
            return;

        if (player == null || isAttacking)
        {
            movement = Vector2.zero;
            currentTargetDistance = 0f;
            currentPlayerBodyDistance = float.MaxValue;
            UpdateAnimator(0f);
            return;
        }

        HandleEnemyLogic();
        UpdateAnimator(GetCurrentMoveSpeed(currentTargetDistance));
    }

    protected virtual void FixedUpdate()
    {
        if (rb == null)
            return;

        if (isDead || isAttacking || player == null)
        {
            movement = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 targetPosition = currentTargetPosition;
        float distanceToTarget = currentTargetDistance;

        if (distanceToTarget <= detectionRange && distanceToTarget > chaseStopDistance)
        {
            float speed = GetCurrentMoveSpeed(distanceToTarget);
            rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);
        }
        else
        {
            movement = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        OnHitReceived?.Invoke();
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        InterruptCurrentAction();
        PlayHurtAnimation();
    }

    private void HandleEnemyLogic()
    {
        Vector2 targetPosition = GetPlayerTargetPosition();
        float distanceToTarget = Vector2.Distance(transform.position, targetPosition);
        currentTargetPosition = targetPosition;
        currentTargetDistance = distanceToTarget;
        currentPlayerBodyDistance = GetDistanceToPlayerBody(out _);

        if (distanceToTarget > detectionRange)
        {
            movement = Vector2.zero;
            return;
        }

        Vector2 directionToTarget = (targetPosition - (Vector2)transform.position).normalized;
        movement = directionToTarget;

        if (movement != Vector2.zero)
            lastMoveDirection = GetMainDirection(movement);

        if (distanceToTarget <= attackRange)
        {
            movement = Vector2.zero;

            if (Time.time >= nextAttackTime)
                AttackPlayer();
        }
    }

    private bool TryGetClosestPlayerBodyPoint(out Vector2 closestOnBody)
    {
        closestOnBody = player != null ? (Vector2)player.position : transform.position;

        if (player == null)
            return false;

        if (playerColliders == null || playerColliders.Length == 0)
            CachePlayerBodyColliders(player.gameObject);

        if (playerColliders == null || playerColliders.Length == 0)
            return false;

        float bestDistance = float.MaxValue;

        foreach (Collider2D playerCollider in playerColliders)
        {
            if (playerCollider == null)
                continue;

            Vector2 closest = playerCollider.ClosestPoint(transform.position);
            float distance = Vector2.Distance(transform.position, closest);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                closestOnBody = closest;
            }
        }

        return bestDistance < float.MaxValue;
    }

    private void CachePlayerBodyColliders(GameObject playerObject)
    {
        if (playerObject == null)
        {
            playerColliders = Array.Empty<Collider2D>();
            return;
        }

        Collider2D[] allPlayerColliders = playerObject.GetComponentsInChildren<Collider2D>();
        List<Collider2D> bodyColliders = new List<Collider2D>(allPlayerColliders.Length);

        foreach (Collider2D collider in allPlayerColliders)
        {
            if (collider == null || collider.isTrigger)
                continue;

            bodyColliders.Add(collider);
        }

        playerColliders = bodyColliders.ToArray();
    }

    private float GetDistanceToPlayerBody(out Vector2 closestOnBody)
    {
        if (!TryGetClosestPlayerBodyPoint(out closestOnBody))
            return float.MaxValue;

        return Vector2.Distance(transform.position, closestOnBody);
    }

    private void AttackPlayer()
    {
        if (isDead || playerController == null)
            return;

        isAttacking = true;
        OnAttackPerformed?.Invoke();
        // cooldown prevents next attack; animation duration controls visual attack length
        nextAttackTime = Time.time + resolvedAttackCooldown;

        if (animator != null)
        {
            animator.SetBool("IsAttacking", true);
            TriggerAttackAnimation();
        }

        float minPlayerDist = currentPlayerBodyDistance;

        if (minPlayerDist == float.MaxValue)
            minPlayerDist = GetDistanceToPlayerBody(out _);

        if (minPlayerDist <= damageDistanceToPlayer)
        {
            playerController.TakeDamage(resolvedAttackDamage);
        }
        else
        {
            Debug.LogWarningFormat("{0} attempted attack but closest player body distance is {1:F2} (threshold {2:F2}). No damage applied.",
                name, minPlayerDist, damageDistanceToPlayer);
        }

        // End animation earlier than cooldown so animation isn't shown during cooldown
        float animDur = Mathf.Max(0.01f, attackAnimationDuration);
        Invoke(nameof(EndAttack), animDur);
    }

    private void TriggerAttackAnimation()
    {
        if (animator == null)
            return;

        if (attackAnimationMode == AttackAnimationMode.Directional)
        {
            float currentSpeed = GetCurrentMoveSpeed(currentTargetDistance);

            if (currentSpeed <= 0.01f && hasAttackIdleTrigger)
            {
                animator.SetTrigger("AttackIdle");
                return;
            }

            bool isRun = Mathf.Abs(currentSpeed - resolvedRunSpeed) <= 0.01f;
            if (isRun && hasAttackRunTrigger)
            {
                animator.SetTrigger("AttackRun");
                return;
            }

            if (hasAttackWalkTrigger)
            {
                animator.SetTrigger("AttackWalk");
                return;
            }
        }

        if (hasAttackTrigger)
            animator.SetTrigger("Attack");
    }

    private void EndAttack()
    {
        isAttacking = false;

        if (animator != null)
            animator.SetBool("IsAttacking", false);
    }

    private void Die()
    {
        InterruptCurrentAction();
        CancelInvoke();
        isDead = true;
        movement = Vector2.zero;

        PlayerStats stats = UnityEngine.Object.FindAnyObjectByType<PlayerStats>();
        if (stats != null) stats.AddScore(scoreValue);

        if (animator != null)
        {
            animator.SetBool("IsAttacking", false);
            animator.SetBool("IsDead", true);
            if (hasDeathTrigger)
            {
                animator.ResetTrigger("Death");
                animator.SetTrigger("Death");
            }
            else if (animator.HasState(0, Animator.StringToHash("Death")))
            {
                animator.Play(Animator.StringToHash("Death"), 0, 0f);
            }
        }

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        Destroy(gameObject, deathDestroyDelay);
    }

    private void InterruptCurrentAction()
    {
        isAttacking = false;
        movement = Vector2.zero;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (animator != null)
            animator.SetBool("IsAttacking", false);

        CancelInvoke(nameof(EndAttack));
    }

    private void UpdateAnimator(float speedValue)
    {
        if (animator == null)
            return;

        if (movement != Vector2.zero)
        {
            animator.SetFloat("MoveX", movement.x);
            animator.SetFloat("MoveY", movement.y);
            animator.SetFloat("LastMoveX", lastMoveDirection.x);
            animator.SetFloat("LastMoveY", lastMoveDirection.y);
        }
        else
        {
            animator.SetFloat("MoveX", 0f);
            animator.SetFloat("MoveY", 0f);
            speedValue = 0f;
        }

        animator.SetFloat("Speed", speedValue);
        animator.SetBool("IsAttacking", isAttacking);
        animator.SetBool("IsDead", isDead);
    }

    private float GetCurrentMoveSpeed(float distanceToTarget)
    {
        if (distanceToTarget <= runRange)
            return resolvedRunSpeed;

        if (distanceToTarget <= detectionRange)
            return resolvedWalkSpeed;

        return 0f;
    }

    private Vector2 GetPlayerTargetPosition()
    {
        if (player == null)
            return transform.position;

        return (Vector2)player.position + fallbackTargetOffset;
    }

    private Vector2 GetMainDirection(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            return direction.x > 0f ? Vector2.right : Vector2.left;

        return direction.y > 0f ? Vector2.up : Vector2.down;
    }

    private void IgnorePlayerBodyCollisions(GameObject playerObject)
    {
        Collider2D[] playerColliders = playerObject.GetComponentsInChildren<Collider2D>();

        foreach (Collider2D enemyCollider in enemyColliders)
        {
            if (enemyCollider == null || enemyCollider.isTrigger)
                continue;

            foreach (Collider2D playerCollider in playerColliders)
            {
                if (playerCollider == null || playerCollider.isTrigger)
                    continue;

                Physics2D.IgnoreCollision(enemyCollider, playerCollider, true);
            }
        }
    }

    private void IgnoreEnemyToEnemyCollisions()
    {
        EnemyControllerBase[] allEnemies = FindObjectsByType<EnemyControllerBase>(FindObjectsSortMode.None);

        foreach (EnemyControllerBase otherEnemy in allEnemies)
        {
            if (otherEnemy == null || otherEnemy == this)
                continue;

            Collider2D[] otherEnemyColliders = otherEnemy.GetComponentsInChildren<Collider2D>();

            foreach (Collider2D myCollider in enemyColliders)
            {
                if (myCollider == null || myCollider.isTrigger)
                    continue;

                foreach (Collider2D otherCollider in otherEnemyColliders)
                {
                    if (otherCollider == null || otherCollider.isTrigger)
                        continue;

                    Physics2D.IgnoreCollision(myCollider, otherCollider, true);
                }
            }
        }
    }

    private void PlayHurtAnimation()
    {
        if (animator == null)
            return;

        if (isDead)
            return;

        CancelInvoke(nameof(EndAttack));

        if (hasHurtTrigger)
        {
            animator.ResetTrigger(hurtTriggerHash);
            animator.SetTrigger(hurtTriggerHash);
            return;
        }

        if (animator.HasState(0, hurtStateHash))
        {
            animator.SetBool("IsAttacking", false);
            animator.Play(hurtStateHash, 0, 0f);
            return;
        }

        if (animator.HasState(0, enemyHurtStateHash))
        {
            animator.SetBool("IsAttacking", false);
            animator.Play(enemyHurtStateHash, 0, 0f);
        }
    }

    private void ResolveDifficultyStats()
    {
        easyProfile = SanitizeProfile(easyProfile, 0.85f, 0.75f, 1f, 1.2f);
        normalProfile = SanitizeProfile(normalProfile, 1f, 1f, 1f, 1f);
        hardProfile = SanitizeProfile(hardProfile, 1.2f, 1.25f, 1.35f, 0.8f);

        DifficultyProfile profile = GetProfileForDifficulty(difficulty);

        resolvedWalkSpeed = walkSpeed * profile.moveSpeedMultiplier;
        resolvedRunSpeed = runSpeed * profile.moveSpeedMultiplier;
        resolvedAttackDamage = Mathf.Max(1, Mathf.RoundToInt(attackDamage * profile.damageMultiplier));
        resolvedAttackCooldown = Mathf.Max(0.1f, attackCooldown * profile.attackCooldownMultiplier);
        resolvedMaxHealth = Mathf.Max(1, Mathf.RoundToInt(maxHealth * profile.healthMultiplier));
    }

    private DifficultyProfile SanitizeProfile(DifficultyProfile profile, float defaultSpeed, float defaultDamage, float defaultHealth, float defaultCooldown)
    {
        if (profile.moveSpeedMultiplier <= 0f)
            profile.moveSpeedMultiplier = defaultSpeed;
        if (profile.damageMultiplier <= 0f)
            profile.damageMultiplier = defaultDamage;
        if (profile.healthMultiplier <= 0f)
            profile.healthMultiplier = defaultHealth;
        if (profile.attackCooldownMultiplier <= 0f)
            profile.attackCooldownMultiplier = defaultCooldown;

        return profile;
    }

    private DifficultyProfile GetProfileForDifficulty(Difficulty selectedDifficulty)
    {
        switch (selectedDifficulty)
        {
            case Difficulty.Easy:
                return easyProfile;
            case Difficulty.Hard:
                return hardProfile;
            default:
                return normalProfile;
        }
    }

    private void CacheAnimatorParameters()
    {
        if (animator == null)
            return;

        hasAttackTrigger = HasAnimatorParameter("Attack", AnimatorControllerParameterType.Trigger);
        hasAttackIdleTrigger = HasAnimatorParameter("AttackIdle", AnimatorControllerParameterType.Trigger);
        hasAttackWalkTrigger = HasAnimatorParameter("AttackWalk", AnimatorControllerParameterType.Trigger);
        hasAttackRunTrigger = HasAnimatorParameter("AttackRun", AnimatorControllerParameterType.Trigger);
        hasHurtTrigger = HasAnimatorParameter("Hurt", AnimatorControllerParameterType.Trigger);
        hasDeathTrigger = HasAnimatorParameter("Death", AnimatorControllerParameterType.Trigger);

        hurtTriggerHash = Animator.StringToHash("Hurt");
        hurtStateHash = Animator.StringToHash("Hurt");
        enemyHurtStateHash = Animator.StringToHash("Enemy_Hurt");
    }

    private bool HasAnimatorParameter(string parameterName, AnimatorControllerParameterType type)
    {
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.name == parameterName && parameter.type == type)
                return true;
        }

        return false;
    }

    protected void SetAttackAnimationMode(AttackAnimationMode mode)
    {
        attackAnimationMode = mode;
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.85f, 0f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = new Color(0.2f, 1f, 0.8f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, runRange);

        Gizmos.color = new Color(1f, 0.25f, 0.25f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
