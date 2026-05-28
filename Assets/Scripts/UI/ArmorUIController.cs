using UnityEngine;
using UnityEngine.UI;

public class ArmorUIController : MonoBehaviour
{
    [Header("Armor UI")]
    [SerializeField] private Image[] armorHearts;
    [SerializeField] private Sprite woodenHeartSprite;

    private PlayerController playerController;

    private void Start()
    {
        playerController = FindFirstObjectByType<PlayerController>();

        if (playerController != null)
        {
            playerController.OnArmorEquipped += UpdateArmorUI;
        }

        ClearArmorUI();
    }

    private void OnDestroy()
    {
        if (playerController != null)
        {
            playerController.OnArmorEquipped -= UpdateArmorUI;
        }
    }

    private void UpdateArmorUI(int defenseBonus)
    {
        ClearArmorUI();

        int heartCount = 0;

        if (defenseBonus == 5) // Wooden armor
        {
            heartCount = 3;
        }

        for (int i = 0; i < heartCount && i < armorHearts.Length; i++)
        {
            armorHearts[i].sprite = woodenHeartSprite;
            armorHearts[i].enabled = true;
        }
    }

    private void ClearArmorUI()
    {
        foreach (Image heart in armorHearts)
        {
            if (heart != null)
            {
                heart.sprite = null;
                heart.enabled = false;
            }
        }
    }
}