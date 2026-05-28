using UnityEngine;

public class HealthPotionCollectible : MonoBehaviour, IUsableItem
{
    [Header("Potion Settings")]
    [SerializeField] private float healPercentage = 0.05f;

    public void Use(GameObject user)
    {
        //SurvivalSystem survival = user.GetComponent<SurvivalSystem>();
        SurvivalSystem survival = SurvivalSystem.Instance;

        if (survival == null)
        {
            Debug.LogWarning("HealthPotionCollectible: SurvivalSystem not found.");
            return;
        }

        float healAmount = survival.maxHealth * healPercentage;
        survival.Heal(healAmount);

        Debug.Log($"HealthPotionCollectible: Restored {healPercentage * 100}% health.");
    }
}