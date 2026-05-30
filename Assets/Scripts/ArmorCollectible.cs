using UnityEngine;

public class ArmorCollectible : MonoBehaviour, IUsableItem
{
    [Header("Armor Settings")]
    [SerializeField] private int defenseBonus = 5;

    public void Use(GameObject user)
    {
        PlayerController playerController = user.GetComponent<PlayerController>();

        if (playerController == null)
        {
            Debug.LogWarning("ArmorCollectible: PlayerController not found.");
            return;
        }

        playerController.EquipArmor(defenseBonus);

        Debug.Log($"ArmorCollectible: Equipped armor with {defenseBonus} defense.");
    }
}