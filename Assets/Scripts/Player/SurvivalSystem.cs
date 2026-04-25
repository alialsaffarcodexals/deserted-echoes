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

    //Those values are not completed now they maybe changed later
    [Header("Depletion Rates (Per Second)")]
    public float hungerDepletionRate = 0.5f;
    public float thirstDepletionRate = 0.8f;
    public float staminaDepletionRate = 5f;

    private float currentHealth, currentStamina, currentHunger, currentThirst;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Initialize stats
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        currentHunger = maxHunger;
        currentThirst = maxThirst;

        //Now the values must be synced with the UI sliders
        healthSlider.maxValue = maxHealth;
        staminaSlider.maxValue = maxStamina;
        hungerSlider.maxValue = maxHunger;
        thirstSlider.maxValue = maxThirst;
    }

    // Update is called once per frame
    void Update()
    {
        //Here we will handle the depletion of hunger and thirst over time
        currentHunger -= hungerDepletionRate * Time.deltaTime;
        currentThirst -= thirstDepletionRate * Time.deltaTime;

        //Here we will handle the stamina regeneration when the player is not performing actions that consume stamina
        if (currentStamina < maxStamina)
        {
            currentStamina += staminaDepletionRate * Time.deltaTime; // Regenerate stamina
        }

        if (currentHunger <= 0 || currentThirst <= 0)
        {
            TakeDamage(1f * Time.deltaTime); // Take damage if hunger or thirst reaches zero
        }

        ClampAllStats();

        UpdateUI();
    }

    void UpdateUI()
    {
        healthSlider.value = currentHealth;
        staminaSlider.value = currentStamina;
        hungerSlider.value = currentHunger;
        thirstSlider.value = currentThirst;
    }

    void ClampAllStats()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger);
        currentThirst = Mathf.Clamp(currentThirst, 0, maxThirst);
    }

    public void TakeDamage(float amount) => currentHealth -= amount;

    public void UseStamina(float amount) => currentStamina -= amount;

    public void Eat(float amount) => currentHunger += amount;

    public void Drink(float amount) => currentThirst += amount;
}
