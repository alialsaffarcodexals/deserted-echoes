using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SurvivalSystem : MonoBehaviour
{
    public static SurvivalSystem Instance { get; private set; }

    [Header("UI Sliders")]
    public Slider healthSlider;
    public Slider staminaSlider;
    public Slider hungerSlider;
    public Slider thirstSlider;
    public Slider expSlider; // Assign your experience UI slider here or via Tag

    [Header("Max Values")]
    public float maxHealth = 100f;
    public float maxStamina = 100f;
    public float maxHunger = 100f;
    public float maxThirst = 100f;

    [Header("Experience & Leveling")]
    [SerializeField] private int baseExpPerLevel = 100; // EXP requirement scaling base factor
    private int currentLevel = 1;
    private float currentExp = 0f;
    private float expNeededForNextLevel;

    // Public getters to allow external UI scripts or combat systems to access progression metrics
    public int CurrentLevel => currentLevel;
    public float CurrentExp => currentExp;
    public float ExpNeededForNextLevel => expNeededForNextLevel;

    [Header("Depletion Rates (Per Second)")]
    public float hungerDepletionRate = 1.5f;
    public float thirstDepletionRate = 2.0f;
    public float staminaDepletionRate = 5f;
    public float staminaRunDepletionRate = 15f;
    public float staminaRegenRate = 4.0f;

    [Header("Health Settings")]
    public float starvationDamage = 1f;
    public bool canRegenerateHealth = true;
    public float healthRegenRate = 0.5f;

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
        CalculateNextLevelThreshold();
        UpdateUI();
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
        currentLevel = 1;
        currentExp = 0f;

        currentHealth = maxHealth;
        currentStamina = maxStamina;
        currentHunger = maxHunger;
        currentThirst = maxThirst;

        SetupSliderMaxValues();
    }

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

        GameObject expObj = GameObject.FindWithTag("EXPSlider");
        if (expObj) expSlider = expObj.GetComponent<Slider>();

        SetupSliderMaxValues();
    }

    private void SetupSliderMaxValues()
    {
        if (healthSlider) healthSlider.maxValue = maxHealth;
        if (staminaSlider) staminaSlider.maxValue = maxStamina;
        if (hungerSlider) hungerSlider.maxValue = maxHunger;
        if (thirstSlider) thirstSlider.maxValue = maxThirst;
        if (expSlider) expSlider.maxValue = expNeededForNextLevel;
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
                isExhausted = true;
                isSprinting = false;
                exhaustionTimer = exhaustionCooldown;
            }
        }
    }

    private void HandleRegeneration()
    {
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

    // ─────────────────────────────────────────────────────────────
    // EXP AND LEVEL SYSTEM METHODS
    // ─────────────────────────────────────────────────────────────

    public void AddExperience(float amount)
    {
        if (IsDead) return;

        currentExp += amount;
        Debug.Log($"+{amount} EXP gained. Progress: {currentExp}/{expNeededForNextLevel}");

        while (currentExp >= expNeededForNextLevel)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        // Deduct the requirement cost to leave your remaining overflow remainder
        currentExp -= expNeededForNextLevel;
        currentLevel++;

        // Explicitly force the slider to zero first to break any internal UI scaling anchors
        if (expSlider != null)
        {
            expSlider.value = 0f;
            expSlider.maxValue = expNeededForNextLevel;
            expSlider.value = currentExp;

            LayoutRebuilder.ForceRebuildLayoutImmediate(expSlider.GetComponent<RectTransform>());
        }

        // Recalculate the next level's milestone targets 
        CalculateNextLevelThreshold();

        // Restore core vitals on level up
        currentHealth = maxHealth;
        currentStamina = maxStamina;

        Debug.Log($"Level Up! You are now Level {currentLevel}! Next level needs: {expNeededForNextLevel} EXP");
    }

    private void CalculateNextLevelThreshold()
    {
        // Update the internal programmatic logic bound
        expNeededForNextLevel = currentLevel * baseExpPerLevel;

        // Update the visual UI slider boundary constraints instantly
        if (expSlider != null)
        {
            expSlider.minValue = 0f; // Ensure baseline floor anchor is hardcoded to 0
            expSlider.maxValue = expNeededForNextLevel;
        }
    }

    // ─────────────────────────────────────────────────────────────
    // UTILITIES & STAT MUTATORS
    // ─────────────────────────────────────────────────────────────

    public void SetSprinting(bool state) => isSprinting = state;

    void UpdateUI()
    {
        if (healthSlider) healthSlider.value = currentHealth;
        if (staminaSlider) staminaSlider.value = currentStamina;
        if (hungerSlider) hungerSlider.value = currentHunger;
        if (thirstSlider) thirstSlider.value = currentThirst;
        if (expSlider) expSlider.value = currentExp;
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

        if (GameOverUI.Instance != null)
            GameOverUI.Instance.ShowAfterDelay(gameOverShowDelay);
        else if (GameManager.Instance != null)
            GameManager.Instance.GameOver();
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