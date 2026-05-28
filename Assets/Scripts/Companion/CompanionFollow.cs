using UnityEngine;

public class CompanionFollow : MonoBehaviour
{
    [Header("Follow")]
    [SerializeField] private Transform player;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float stopDistance = 1.2f;
    [SerializeField] private float arriveDistance = 0.08f;
    [SerializeField] private Vector2 followOffset = new Vector2(-0.8f, -0.4f);
    [SerializeField] private bool ignorePlayerCollision = true;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string speedParam = "Speed";
    [SerializeField] private string moveXParam = "MoveX";
    [SerializeField] private string moveYParam = "MoveY";
    [SerializeField] private string lastMoveXParam = "LastMoveX";
    [SerializeField] private string lastMoveYParam = "LastMoveY";
    [SerializeField] private string isMovingParam = "IsMoving";

    private Rigidbody2D rb;
    private Collider2D[] companionColliders;
    private Vector2 movement;
    private Vector2 lastMoveDirection = Vector2.down;

    private int speedHash;
    private int moveXHash;
    private int moveYHash;
    private int lastMoveXHash;
    private int lastMoveYHash;
    private int isMovingHash;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        companionColliders = GetComponentsInChildren<Collider2D>();

        speedHash = Animator.StringToHash(speedParam);
        moveXHash = Animator.StringToHash(moveXParam);
        moveYHash = Animator.StringToHash(moveYParam);
        lastMoveXHash = Animator.StringToHash(lastMoveXParam);
        lastMoveYHash = Animator.StringToHash(lastMoveYParam);
        isMovingHash = Animator.StringToHash(isMovingParam);
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
                player = playerObject.transform;
        }

        IgnorePlayerCollisions();
        UpdateAnimator();
    }

    private void Update()
    {
        if (player == null)
        {
            movement = Vector2.zero;
            UpdateAnimator();
            return;
        }

        Vector2 targetPosition = GetFollowPosition();
        Vector2 toTarget = targetPosition - (Vector2)transform.position;

        movement = toTarget.magnitude > arriveDistance ? toTarget.normalized : Vector2.zero;

        if (movement != Vector2.zero)
            lastMoveDirection = GetMainDirection(movement);

        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        if (rb == null || movement == Vector2.zero)
            return;

        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    private void UpdateAnimator()
    {
        if (animator == null)
            return;

        animator.SetFloat(moveXHash, movement.x);
        animator.SetFloat(moveYHash, movement.y);
        animator.SetFloat(lastMoveXHash, lastMoveDirection.x);
        animator.SetFloat(lastMoveYHash, lastMoveDirection.y);
        animator.SetFloat(speedHash, movement == Vector2.zero ? 0f : moveSpeed);
        animator.SetBool(isMovingHash, movement != Vector2.zero);
    }

    private Vector2 GetFollowPosition()
    {
        Vector2 offset = followOffset;

        if (offset.sqrMagnitude < 0.0001f)
            offset = Vector2.left;

        if (offset.magnitude < stopDistance)
            offset = offset.normalized * stopDistance;

        return (Vector2)player.position + offset;
    }

    private Vector2 GetMainDirection(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            return direction.x > 0f ? Vector2.right : Vector2.left;

        return direction.y > 0f ? Vector2.up : Vector2.down;
    }

    private void IgnorePlayerCollisions()
    {
        if (!ignorePlayerCollision || player == null || companionColliders == null)
            return;

        Collider2D[] playerColliders = player.GetComponentsInChildren<Collider2D>();

        foreach (Collider2D companionCollider in companionColliders)
        {
            if (companionCollider == null)
                continue;

            foreach (Collider2D playerCollider in playerColliders)
            {
                if (playerCollider != null)
                    Physics2D.IgnoreCollision(companionCollider, playerCollider, true);
            }
        }
    }
}
