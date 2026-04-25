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

    Transform player;

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
            player = playerObj.transform;
    }

    void FixedUpdate()
    {
        if (player == null)
            return;

        Vector2 toPlayer = player.position - transform.position;
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
            Destroy(gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag(attackHitboxTag))
            Destroy(gameObject);
    }
}
