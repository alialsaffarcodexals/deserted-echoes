using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyFollowAndDisappear : MonoBehaviour
{
    [Header("Follow")]
    [SerializeField] string playerTag = "Player";
    [SerializeField] float moveSpeed = 2.5f;
    [SerializeField] float stopDistance = 0.7f;
    [SerializeField] Rigidbody2D rb;

    [Header("Combat")]
    [SerializeField] string attackHitboxTag = "AttackHitbox";

    // ─────────────────────────────────────────────────────────────
    // NEW EXP CONFIGURATION
    // ─────────────────────────────────────────────────────────────
    [Header("Progression Rewards")]
    [SerializeField] private float expReward = 20f;

    Transform player;
    PlayerController playerController;

    void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        var col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = false;
    }

    void Start()
    {
        var playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerController = playerObj.GetComponent<PlayerController>();
        }
    }

    void FixedUpdate()
    {
        if (player == null)
            return;

        Vector2 targetPosition = player.position;
        if (playerController != null)
            playerController.TryGetEnemyAttackTargetPosition(out targetPosition);

        Vector2 toPlayer = targetPosition - (Vector2)transform.position;
        float distance = toPlayer.magnitude;
        if (distance <= stopDistance)
            return;

        Vector2 direction = toPlayer / Mathf.Max(distance, 0.0001f);
        Vector2 nextPos = (Vector2)transform.position + direction * moveSpeed * Time.fixedDeltaTime;

        if (rb != null)
            rb.MovePosition(nextPos);
        else
            transform.position = nextPos;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(attackHitboxTag))
        {
            AwardExperience();
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag(attackHitboxTag))
            Destroy(gameObject);
    }

    private void AwardExperience()
    {
        if (SurvivalSystem.Instance != null)
        {
            SurvivalSystem.Instance.AddExperience(expReward);
        }
    }
}
