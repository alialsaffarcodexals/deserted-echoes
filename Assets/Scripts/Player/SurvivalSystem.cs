using UnityEngine;
using UnityEngine.UI;
<<<<<<< HEAD

public class SurvivalSystem : MonoBehaviour
{
=======
using UnityEngine.SceneManagement;

public class SurvivalSystem : MonoBehaviour
{
    public static SurvivalSystem Instance { get; private set; }

>>>>>>> develop
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

<<<<<<< HEAD
    [Header("Depletion Rates")]
    public float hungerDepletionRate = 0.5f;
    public float thirstDepletionRate = 0.8f;
    public float staminaDepletionRate = 5f;
=======
    [Header("Depletion Rates (Per Second)")]
    public float hungerDepletionRate = 1.5f;
    public float thirstDepletionRate = 2.0f;
    public float staminaDepletionRate = 5f;
    public float staminaRunDepletionRate = 15f;
    public float staminaRegenRate = 4.0f;
>>>>>>> develop

    [Header("Health Settings")]
    public float starvationDamage = 1f;
    public bool canRegenerateHealth = true;
    public float healthRegenRate = 0.5f;

<<<<<<< HEAD
    private float currentHealth, currentStamina, currentHunger, currentThirst;
    private bool isSprinting;

    public bool IsDead => currentHealth <= 0;
    public bool CanSprint => currentStamina > 0 && !IsDead;

    void Start()
    {
        InitializeStats();
=======
    [Header("Stamina Exhaustion")]
    public float exhaustionCooldown = 5f;

    [Header("Game Over")]
    [SerializeField] private float gameOverShowDelay = 1.5f;

    private float currentHealth, currentStamina, currentHunger, currentThirst;
    private bool isSprinting;
    private bool isExhausted;
    private float exhaustionTimer;
    private PlayerController playerController;

    public bool IsDead => currentHealth <= 0;
    public bool CanSprint => currentStamina > 0 && !isExhausted && !IsDead;
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(transform.root.gameObject);

        InitializeStats();

        if (playerController != null)
            SetHealthStats(playerController.GetMaxHealth(), playerController.GetCurrentHealth());
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        playerController = Object.FindAnyObjectByType<PlayerController>();

        FindUIRefrecesInNewScene();
>>>>>>> develop
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

<<<<<<< HEAD
    private void HandleDepletion()
    {
        currentHunger -= hungerDepletionRate * Time.deltaTime;
        currentThirst -= thirstDepletionRate * Time.deltaTime;

        if (isSprinting && currentStamina > 0)
        {
            currentStamina -= staminaDepletionRate * Time.deltaTime;
=======
    private void FindUIRefrecesInNewScene()
    {
        GameObject healthObj = GameObject.FindWithTag("HealthSlider");
        if (healthObj) healthSlider = healthObj.GetComponent<Slider>();

        GameObject staminaObj = GameObject.FindWithTag("StaminaSlider");
        if (staminaObj) staminaSlider = staminaObj.GetComponent<Slider>();

        GameObject hungerObj = GameObject.FindWithTag("HungerSlider");
        if (hungerObj) hungerSlider = hungerObj.GetComponent<Slider>();

        GameObject thirstObj = GameObject.FindWithTag("ThirstSlider");
        if (thirstObj) thirstSlider = thirstObj.GetComponent<Slider>();

        SetupSliderMaxValues();
    }

    private void SetupSliderMaxValues()
    {
        if (healthSlider) healthSlider.maxValue = maxHealth;
        if (staminaSlider) staminaSlider.maxValue = maxStamina;
        if (hungerSlider) hungerSlider.maxValue = maxHunger;
        if (thirstSlider) thirstSlider.maxValue = maxThirst;
    }

    private void HandleDepletion()
    {

        if (isSprinting && currentStamina > 0)
        {
            currentHunger -= hungerDepletionRate * Time.deltaTime;
            currentThirst -= thirstDepletionRate * Time.deltaTime;
            currentStamina -= staminaRunDepletionRate * Time.deltaTime;

            if (currentStamina <= 0)
            {
                currentStamina = 0;
                isExhausted    = true;
                isSprinting = false;
                exhaustionTimer = exhaustionCooldown;
            }
>>>>>>> develop
        }
    }

    private void HandleRegeneration()
    {
<<<<<<< HEAD
        if (!isSprinting && currentStamina < maxStamina)
        {
            currentStamina += (staminaDepletionRate * 0.5f) * Time.deltaTime;
=======
        if (isExhausted)
        {
            maxHealth = 100f;
            exhaustionTimer -= Time.deltaTime;
            if (exhaustionTimer <= 0f)
                isExhausted = false;
        }
        else if (!isSprinting && currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
>>>>>>> develop
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

<<<<<<< HEAD
=======
    private void OnDeath()
    {
        Debug.Log("Player has died.");
        if (playerController != null)
            playerController.Die();

        if (GameOverUI.Instance != null)
            GameOverUI.Instance.ShowAfterDelay(gameOverShowDelay);
        else if (GameManager.Instance != null)
            GameManager.Instance.GameOver();
    }

>>>>>>> develop
    public void TakeDamage(float amount)
    {
        if (IsDead) return;
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
<<<<<<< HEAD
            Debug.Log("Player has died.");
        }
    }

    public void UseStamina(float amount) => currentStamina -= amount;
    public void Heal(float amount) => currentHealth += amount;
    public void Eat(float amount) => currentHunger += amount;
    public void Drink(float amount) => currentThirst += amount;
}
=======
            OnDeath();
        }
    }

    public void SetHealthStats(float newMaxHealth, float newCurrentHealth)
    {
        maxHealth = Mathf.Max(1f, newMaxHealth);
        currentHealth = Mathf.Clamp(newCurrentHealth, 0f, maxHealth);

        if (healthSlider)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    public void UseStamina(float amount) => currentStamina = Mathf.Clamp(currentStamina - amount, 0f, maxStamina);
    public void Heal(float amount) => currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
    public void Eat(float amount) => currentHunger += amount;
    public void Drink(float amount) => currentThirst += amount;
}
>>>>>>> develop
