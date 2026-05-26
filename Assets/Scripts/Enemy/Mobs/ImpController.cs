using UnityEngine;

public class ImpController : EnemyControllerBase
{
    [SerializeField] private GameObject impFireProjectilePrefab;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private float projectileSpeed = 6f;
    [SerializeField] private float projectileSpawnOffset = 0.8f;
    [SerializeField] private float projectileThrowDelay = 0.25f;

    private bool throwWindowOpen;
    private bool hasThrownThisAttack;

    protected override void Start()
    {
        base.Start();
        OnAttackPerformed += HandleAttackPerformed;
        
        // If throwPoint not assigned, create one at this position
        if (throwPoint == null)
        {
            GameObject throwObj = new GameObject("ThrowPoint");
            throwObj.transform.SetParent(transform);
            throwObj.transform.localPosition = Vector3.right * 0.4f;
            throwPoint = throwObj.transform;
        }
    }

    private void OnDestroy()
    {
        OnAttackPerformed -= HandleAttackPerformed;
    }

    public void ThrowFireball()
    {
        if (throwWindowOpen && hasThrownThisAttack)
            return;

        if (throwWindowOpen)
            hasThrownThisAttack = true;

        if (impFireProjectilePrefab == null)
        {
            Debug.LogError("ImpController: impFireProjectilePrefab is NOT ASSIGNED in Inspector!");
            return;
        }

        if (throwPoint == null)
        {
            Debug.LogError("ImpController: throwPoint is NOT ASSIGNED.");
            return;
        }

        Vector2 throwDirection = GetThrowDirection();
        Vector3 spawnPosition = throwPoint.position + (Vector3)(throwDirection * projectileSpawnOffset);
        GameObject projectileObj = Instantiate(impFireProjectilePrefab, spawnPosition, Quaternion.identity);
        Debug.Log($"{name}: Spawned {projectileObj.name} at {spawnPosition} toward {throwDirection}.");
        IgnoreEnemyCollisions(projectileObj);

        ProjectileBase projectile = projectileObj.GetComponent<ProjectileBase>();
        Animator projAnimator = projectileObj.GetComponent<Animator>();

        if (projectile != null)
        {
            projectile.Launch(throwDirection, projectileSpeed);

            if (projAnimator != null)
            {
                projAnimator.SetFloat("DirX", throwDirection.x);
                projAnimator.SetFloat("DirY", throwDirection.y);
                projAnimator.SetBool("IsMoving", true);
                projAnimator.Play("Loop", 0, 0f);
            }
        }
        else
        {
            Debug.LogError("Projectile has NO ProjectileBase component!");
        }
    }

    private void HandleAttackPerformed()
    {
        Debug.Log($"{name}: Imp attack event received, scheduling fireball.");
        throwWindowOpen = true;
        hasThrownThisAttack = false;

        CancelInvoke(nameof(ThrowFireballFromAttack));
        CancelInvoke(nameof(CloseThrowWindow));
        Invoke(nameof(ThrowFireballFromAttack), projectileThrowDelay);
        Invoke(nameof(CloseThrowWindow), 0.75f);
    }

    private void ThrowFireballFromAttack()
    {
        Debug.Log($"{name}: Imp fallback throw firing.");
        ThrowFireball();
    }

    private void CloseThrowWindow()
    {
        throwWindowOpen = false;
        hasThrownThisAttack = false;
    }

    public void ThrowProjectile()
    {
        ThrowFireball();
    }

    public void FireProjectile()
    {
        ThrowFireball();
    }

    private Vector2 GetThrowDirection()
    {
        if (Player == null)
            return Vector2.right;

        Vector2 direction = (Vector2)Player.position - (Vector2)throwPoint.position;
        return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;
    }

    private void IgnoreEnemyCollisions(GameObject projectileObj)
    {
        Collider2D[] projectileColliders = projectileObj.GetComponentsInChildren<Collider2D>();
        EnemyControllerBase[] enemies = FindObjectsByType<EnemyControllerBase>(FindObjectsSortMode.None);

        foreach (Collider2D projectileCollider in projectileColliders)
        {
            if (projectileCollider == null)
                continue;

            foreach (EnemyControllerBase enemy in enemies)
            {
                if (enemy == null)
                    continue;

                Collider2D[] enemyColliders = enemy.GetComponentsInChildren<Collider2D>();
                foreach (Collider2D enemyCollider in enemyColliders)
                {
                    if (enemyCollider != null)
                        Physics2D.IgnoreCollision(projectileCollider, enemyCollider, true);
                }
            }
        }
    }
}
