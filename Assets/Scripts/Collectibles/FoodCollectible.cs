// ------------------------------------------------------------
// FoodCollectible.cs
// Deserted Echoes | IT8101 Games Development | Group 3
// Author: Khizar Azhar
// Sprint: 7 | Created: 20/05/2026
// Description: Handles food collection and hunger restoration/reduction
// ------------------------------------------------------------

using System.Collections;
using UnityEngine;

public class FoodCollectible : MonoBehaviour, ICollectible
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
            Debug.LogWarning("FoodCollectible: SurvivalSystem not found on player.");
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

            Debug.Log($"FoodCollectible: Rotten food reducing hunger over time.");
        }

        Destroy(gameObject);
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