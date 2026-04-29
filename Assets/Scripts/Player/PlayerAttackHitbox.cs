using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerAttackHitbox : MonoBehaviour
{
    [SerializeField] PlayerMovement playerMovement;
    [SerializeField] Collider2D hitboxCollider;
    [SerializeField] float startupDelay = 0.03f;
    [SerializeField] float activeTime = 0.12f;

    Coroutine hitboxRoutine;

    void Awake()
    {
        if (playerMovement == null)
            playerMovement = GetComponentInParent<PlayerMovement>();

        if (hitboxCollider == null)
            hitboxCollider = GetComponent<Collider2D>();

        if (hitboxCollider != null)
            hitboxCollider.isTrigger = true;

        gameObject.tag = "AttackHitbox";
        SetHitboxEnabled(false);
    }

    void OnEnable()
    {
        if (playerMovement != null)
            playerMovement.AttackTriggered += OnPlayerAttackTriggered;
    }

    void OnDisable()
    {
        if (playerMovement != null)
            playerMovement.AttackTriggered -= OnPlayerAttackTriggered;

        SetHitboxEnabled(false);
    }

    void OnPlayerAttackTriggered()
    {
        if (hitboxRoutine != null)
            StopCoroutine(hitboxRoutine);

        hitboxRoutine = StartCoroutine(HitboxWindow());
    }

    IEnumerator HitboxWindow()
    {
        SetHitboxEnabled(false);

        if (startupDelay > 0f)
            yield return new WaitForSeconds(startupDelay);

        SetHitboxEnabled(true);

        if (activeTime > 0f)
            yield return new WaitForSeconds(activeTime);

        SetHitboxEnabled(false);
        hitboxRoutine = null;
    }

    void SetHitboxEnabled(bool isEnabled)
    {
        if (hitboxCollider != null)
            hitboxCollider.enabled = isEnabled;
    }
}
