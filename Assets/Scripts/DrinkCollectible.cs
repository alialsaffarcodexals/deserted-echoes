using System.Collections;
using UnityEngine;

public class DrinkCollectible : MonoBehaviour, IUsableItem
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

    public void Use(GameObject user)
    {
        //SurvivalSystem survival = user.GetComponent<SurvivalSystem>();
        SurvivalSystem survival = SurvivalSystem.Instance;

        if (survival == null)
        {
            Debug.LogWarning("DrinkCollectible: SurvivalSystem not found.");
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