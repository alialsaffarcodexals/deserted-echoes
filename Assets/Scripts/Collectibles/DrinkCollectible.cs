// ------------------------------------------------------------
// DrinkCollectible.cs
// Deserted Echoes | IT8101 Games Development | Group 3
// Author: Khizar Azhar
// Sprint: 7 | Created: 20/05/2026
// Description: Handles drink collection and thirst restoration/reduction
// ------------------------------------------------------------

using System.Collections;
using UnityEngine;

public class DrinkCollectible : MonoBehaviour, ICollectible
{
    private enum DrinkType
    {
        Normal,
        Contaminated
    }

    [Header("Drink Settings")]
    [SerializeField] private DrinkType drinkType;
    [SerializeField] private float thirstPercentage = 0.1f;
    [SerializeField] private float contaminatedDuration = 15f;

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
            Debug.LogWarning("DrinkCollectible: SurvivalSystem not found on player.");
            return;
        }

        float amount = survival.maxThirst * thirstPercentage;

        if (drinkType == DrinkType.Normal)
        {
            survival.Drink(amount);
            Debug.Log($"DrinkCollectible: Restored {thirstPercentage * 100}% thirst.");
        }
        else
        {
            survival.StartCoroutine(ApplyContaminatedEffect(survival, amount));
            Debug.Log("DrinkCollectible: Contaminated drink reducing thirst over time.");
        }

        Destroy(gameObject);
    }

    private IEnumerator ApplyContaminatedEffect(SurvivalSystem survival, float totalAmount)
    {
        float elapsed = 0f;

        while (elapsed < contaminatedDuration)
        {
            float amountPerSecond = totalAmount / contaminatedDuration;

            survival.Drink(-amountPerSecond * Time.deltaTime);

            elapsed += Time.deltaTime;

            yield return null;
        }
    }
}