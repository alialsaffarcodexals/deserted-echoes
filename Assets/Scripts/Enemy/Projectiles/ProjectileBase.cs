using UnityEngine;

public abstract class ProjectileBase : MonoBehaviour
{
    [SerializeField] protected float moveSpeed = 5f;
    [SerializeField] protected int damage = 5;
    [SerializeField] protected float destroyDelay = 5f;

    protected Rigidbody2D rb;
    protected Animator animator;
    protected Vector2 moveDirection = Vector2.right;
    protected PlayerController playerController;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
            playerController = playerObject.GetComponent<PlayerController>();
    }

    protected virtual void Start()
    {
        // Auto-destroy after delay if not destroyed by collision
        Destroy(gameObject, destroyDelay);
    }

    protected virtual void FixedUpdate()
    {
        if (rb != null)
            rb.linearVelocity = moveDirection * moveSpeed;
    }

    public void SetDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude <= 0.0001f)
            return;

        moveDirection = direction.normalized;
    }

    public void Launch(Vector2 direction, float speed)
    {
        SetDirection(direction);
        moveSpeed = Mathf.Max(0f, speed);

        if (rb != null)
            rb.linearVelocity = moveDirection * moveSpeed;
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        HandleCollision(collision);
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.collider);
    }

    protected abstract void HandleCollision(Collider2D collision);

    protected void DamagePlayer()
    {
        if (playerController != null)
            playerController.TakeDamage(damage);
    }

    protected virtual void DestroyProjectile()
    {
        Destroy(gameObject);
    }
}
