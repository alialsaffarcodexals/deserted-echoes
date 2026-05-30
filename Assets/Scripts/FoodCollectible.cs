// ------------------------------------------------------------
// FoodCollectible.cs
// Deserted Echoes | IT8101 Games Development | Group 3
// Author: Khizar Azhar
// Sprint: 8 | Updated: 25/05/2026
// Description: Handles food usage from the hotbar inventory system
// ------------------------------------------------------------

using System.Collections;
using UnityEngine;

public class FoodCollectible : MonoBehaviour, IUsableItem
{
    private enum FoodType
    {
        Normal,
        Rotten
    }

    [Header("Food Settings")]
    [SerializeField] private FoodType foodType;
    [SerializeField] private float hungerPercentage = 0.1f;
    [SerializeField] private float rottenDuration = 15f;

    public void Use(GameObject user)
    {
        SurvivalSystem survival = SurvivalSystem.Instance;

        if (survival == null)
        {
            Debug.LogWarning("FoodCollectible: SurvivalSystem not found.");
            return;
        }

        float amount = survival.maxHunger * hungerPercentage;

        if (foodType == FoodType.Normal)
        {
            survival.Eat(amount);

            Debug.Log($"FoodCollectible: Restored {hungerPercentage * 100}% hunger.");
        }
        else
        {
            survival.StartCoroutine(ApplyRottenEffect(survival, amount));

            Debug.Log("FoodCollectible: Rotten food reducing hunger over time.");
        }
    }

    private IEnumerator ApplyRottenEffect(SurvivalSystem survival, float totalAmount)
    {
        float elapsed = 0f;

        while (elapsed < rottenDuration)
        {
            float amountPerSecond = totalAmount / rottenDuration;

            survival.Eat(-amountPerSecond * Time.deltaTime);

            elapsed += Time.deltaTime;

            yield return null;
        }
    }
}