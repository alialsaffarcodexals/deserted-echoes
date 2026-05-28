using System.Collections;
using UnityEngine;

public class StaminaPotionCollectible : MonoBehaviour, IUsableItem
{
    [Header("Potion Settings")]
    [SerializeField] private float recoveryBoostMultiplier = 1.1f;
    [SerializeField] private float boostDuration = 10f;

    public void Use(GameObject user)
    {
        //SurvivalSystem survival = user.GetComponent<SurvivalSystem>();
        SurvivalSystem survival = SurvivalSystem.Instance;
        if (survival == null)
        {
            Debug.LogWarning("StaminaPotionCollectible: SurvivalSystem not found.");
            return;
        }

        survival.StartCoroutine(ApplyBoost(survival));

        Debug.Log("StaminaPotionCollectible: Stamina recovery boosted.");
    }

    private IEnumerator ApplyBoost(SurvivalSystem survival)
    {
        survival.staminaRegenMultiplier = recoveryBoostMultiplier;

        yield return new WaitForSeconds(boostDuration);

        survival.staminaRegenMultiplier = 1f;
    }
}