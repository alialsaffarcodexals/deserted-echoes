using UnityEngine;

public class SwordCollectible : MonoBehaviour, IUsableItem
{
    [Header("Sword Settings")]
    [SerializeField] private int bonusDamage = 10;

    public void Use(GameObject user)
    {
        PlayerController playerController = user.GetComponent<PlayerController>();

        if (playerController == null)
        {
            Debug.LogWarning("SwordCollectible: PlayerController not found.");
            return;
        }

        playerController.EquipWeapon(bonusDamage);

        Debug.Log($"SwordCollectible: Equipped sword with +{bonusDamage} damage.");
    }
}