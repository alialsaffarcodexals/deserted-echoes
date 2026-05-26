using UnityEngine;

public class Score : MonoBehaviour
{
    [Header("Score Configuration")]
    [Tooltip("How many points are added to the player's overall score profile.")]
    public int scoreValue = 100;

    [Header("EXP Conversion")]
    [Tooltip("How much experience progression points this item yields directly to the survival bar.")]
    [SerializeField] private float expRewardValue = 25f;

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object crossing into the trigger zone has the Player tag
        if (other.CompareTag("Player"))
        {
            // Locate the statistical tracking logic running on your player framework
            PlayerStats stats = Object.FindAnyObjectByType<PlayerStats>();
            if (stats != null)
            {
                stats.AddScore(scoreValue);
            }

            // Feed the reward points straight into the active persistent survival instance
            if (SurvivalSystem.Instance != null)
            {
                SurvivalSystem.Instance.AddExperience(expRewardValue);
            }
            else
            {
                Debug.LogWarning("Score2D: Found player but could not locate SurvivalSystem instance to award EXP!");
            }

            
            Destroy(gameObject);
        }
    }
}