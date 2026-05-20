// ------------------------------------------------------------
// StaminaPotionCollectible.cs
// Deserted Echoes | IT8101 Games Development | Group 3
// Author: Khizar Azhar
// Sprint: 7 | Created: 20/05/2026
// Description: Boosts stamina recovery rate when collected
// ------------------------------------------------------------

using System.Collections;
using UnityEngine;

public class StaminaPotionCollectible : MonoBehaviour, ICollectible
{
    [Header("Potion Settings")]
    [SerializeField] private float recoveryBoostMultiplier = 1.1f;
    [SerializeField] private float boostDuration = 10f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Collect(other.gameObject);
    }

    public void Collect(GameObject collector)
    {
        SurvivalSystem survival = collector.GetComponent<SurvivalSystem>();

        if (survival == null)
        {
            Debug.LogWarning("StaminaPotionCollectible: SurvivalSystem not found on player.");
            return;
        }

        survival.StartCoroutine(ApplyBoost(survival));

        Debug.Log("StaminaPotionCollectible: Stamina recovery boosted by 10%.");

        Destroy(gameObject);
    }

    private IEnumerator ApplyBoost(SurvivalSystem survival)
    {
        survival.staminaRegenMultiplier = recoveryBoostMultiplier;

        yield return new WaitForSeconds(boostDuration);

        survival.staminaRegenMultiplier = 1f;
    }
}   