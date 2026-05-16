using UnityEngine;
using UnityEngine.UI;

public class SurvivalSystem : MonoBehaviour
{
    [Header("UI Sliders")]
    public Slider healthSlider;
    public Slider staminaSlider;
    public Slider hungerSlider;
    public Slider thirstSlider;

    [Header("Max Values")]
    public float maxHealth = 100f;
    public float maxStamina = 100f;
    public float maxHunger = 100f;
    public float maxThirst = 100f;

    [Header("Depletion Rates (Per Second)")]
    public float hungerDepletionRate = 0.5f;
    public float thirstDepletionRate = 0.8f;
    public float staminaDepletionRate = 5f;

    [Header("Health Settings")]
    public float starvationDamage = 1f;
    public bool canRegenerateHealth = true;
    public float healthRegenRate = 0.5f;

    private float currentHealth, currentStamina, currentHunger, currentThirst;
    private bool isSprinting;
    private PlayerController playerController;

    public bool IsDead => currentHealth <= 0;
    public bool CanSprint => currentStamina > 0 && !IsDead;

    void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        InitializeStats();
    }

    void Update()
    {
        if (IsDead) return;

        HandleDepletion();
        HandleRegeneration();
        HandleEnvironmentalDamage();
        ClampAllStats();
        UpdateUI();
    }

    private void InitializeStats()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        currentHunger = maxHunger;
        currentThirst = maxThirst;

        if (healthSlider) healthSlider.maxValue = maxHealth;
        if (staminaSlider) staminaSlider.maxValue = maxStamina;
        if (hungerSlider) hungerSlider.maxValue = maxHunger;
        if (thirstSlider) thirstSlider.maxValue = maxThirst;
    }

    private void HandleDepletion()
    {
        currentHunger -= hungerDepletionRate * Time.deltaTime;
        currentThirst -= thirstDepletionRate * Time.deltaTime;

        if (isSprinting && currentStamina > 0)
        {
            currentStamina -= staminaDepletionRate * Time.deltaTime;
        }
    }

    private void HandleRegeneration()
    {
        if (!isSprinting && currentStamina < maxStamina)
        {
            currentStamina += (staminaDepletionRate * 0.5f) * Time.deltaTime;
        }

        if (canRegenerateHealth && currentHunger > 20f && currentThirst > 20f && currentHealth < maxHealth)
        {
            currentHealth += healthRegenRate * Time.deltaTime;
        }
    }

    private void HandleEnvironmentalDamage()
    {
        if (currentHunger <= 0 || currentThirst <= 0)
        {
            TakeDamage(starvationDamage * Time.deltaTime);
        }
    }

    public void SetSprinting(bool state) => isSprinting = state;

    void UpdateUI()
    {
        if (healthSlider) healthSlider.value = currentHealth;
        if (staminaSlider) staminaSlider.value = currentStamina;
        if (hungerSlider) hungerSlider.value = currentHunger;
        if (thirstSlider) thirstSlider.value = currentThirst;
    }

    void ClampAllStats()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger);
        currentThirst = Mathf.Clamp(currentThirst, 0, maxThirst);
    }

    private void OnDeath()
    {
        Debug.Log("Player has died.");
        if (playerController != null)
            playerController.Die();
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            OnDeath();
        }
    }

    public void UseStamina(float amount) => currentStamina -= amount;
    public void Heal(float amount) => currentHealth += amount;
    public void Eat(float amount) => currentHunger += amount;
    public void Drink(float amount) => currentThirst += amount;
}
