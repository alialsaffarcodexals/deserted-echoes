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
    public float hungerDepletionRate = 4.5f;
    public float thirstDepletionRate = 4.0f;
    public float staminaDepletionRate = 5f;
    public float staminaRunDepletionRate = 15f;
    public float staminaRegenRate = 4.0f;
    // changes here
    public float staminaRegenMultiplier = 1f;
    // end here
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
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float CurrentStamina => currentStamina;

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

        if (playerController != null)
            playerController.BindSurvivalSystem(this);

        bool isGameplayScene = scene.name != "main-menu";
        SetHUDVisible(isGameplayScene);

        if (!isGameplayScene)
            return;

        FindUIRefrecesInNewScene();

        Debug.Log($"[SurvivalSystem] Scene '{scene.name}' loaded. " +
                  $"healthSlider={(healthSlider != null ? healthSlider.gameObject.name : "NULL")}, " +
                  $"currentHealth={currentHealth}");

        // Push current values onto freshly-found slider refs immediately
        // so the bar reflects reality on the very first frame.
        UpdateUI();
    }

    void Update()
    {
        if (IsDead) return;

        HandleDepletion();
        HandleRegeneration();
        HandleEnvironmentalDamage();
        ClampAllStats();
        SyncPlayerControllerLevel();
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
        // Only search by tag when the current reference is null or has been
        // destroyed (scene-specific slider from a previous load).  If the
        // reference is still valid — e.g. an Inspector-assigned slider on
        // the DontDestroyOnLoad canvas — keep it so we don't accidentally
        // start driving a scene-specific slider while the visible
        // DontDestroyOnLoad one sits frozen at full health.
        if (!healthSlider)
        {
            GameObject healthObj = GameObject.FindWithTag("HealthSlider");
            if (healthObj) healthSlider = healthObj.GetComponent<Slider>();
        }

        if (!staminaSlider)
        {
            GameObject staminaObj = GameObject.FindWithTag("StaminaSlider");
            if (staminaObj) staminaSlider = staminaObj.GetComponent<Slider>();
        }

        if (!hungerSlider)
        {
            GameObject hungerObj = GameObject.FindWithTag("HungerSlider");
            if (hungerObj) hungerSlider = hungerObj.GetComponent<Slider>();
        }

        if (!thirstSlider)
        {
            GameObject thirstObj = GameObject.FindWithTag("ThirstSlider");
            if (thirstObj) thirstSlider = thirstObj.GetComponent<Slider>();
        }

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
        float hungerMod = TemperatureSystem.Instance?.GetHungerModifier() ?? 1f;
        float thirstMod = TemperatureSystem.Instance?.GetThirstModifier() ?? 1f;
        float staminaMod = TemperatureSystem.Instance?.GetStaminaModifier() ?? 1f;



        if (isSprinting && currentStamina > 0)
        {
            currentStamina -= staminaRunDepletionRate * staminaMod * Time.deltaTime;
            currentHunger -= hungerDepletionRate * hungerMod * Time.deltaTime;
            currentThirst -= thirstDepletionRate * thirstMod * Time.deltaTime;

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
            //changes here 

            
            // currentStamina += staminaRegenRate * Time.deltaTime; old stamina system
            currentStamina += staminaRegenRate * staminaRegenMultiplier * Time.deltaTime;
            //end here

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

        SyncPlayerControllerProgress();
        SaveManager.Instance?.SaveGame();
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

        SyncPlayerControllerProgress();

        Debug.Log($"Level Up! You are now Level {currentLevel}! Next level needs: {expNeededForNextLevel} EXP");
    }

    private void SyncPlayerControllerLevel()
    {
        if (playerController == null)
            playerController = Object.FindAnyObjectByType<PlayerController>();

        if (playerController != null && currentLevel > playerController.GetLevel())
            playerController.SyncLevelFromSurvivalSystem(currentLevel, currentExp);
    }

    private void SyncPlayerControllerProgress()
    {
        if (playerController == null)
            playerController = Object.FindAnyObjectByType<PlayerController>();

        if (playerController != null)
            playerController.SyncLevelFromSurvivalSystem(currentLevel, currentExp);
    }

    public void LoadProgressFromSave(SaveData saveData)
    {
        if (saveData == null)
            return;

        currentLevel = Mathf.Max(1, saveData.level);
        currentExp = Mathf.Max(0f, saveData.experience);
        CalculateNextLevelThreshold();

        while (currentExp >= expNeededForNextLevel)
        {
            currentExp -= expNeededForNextLevel;
            currentLevel++;
            CalculateNextLevelThreshold();
        }

        UpdateUI();
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

    /// <summary>
    /// Called by scene HUD objects (e.g. HUDController.Start) to hand their
    /// slider references directly to SurvivalSystem.  This is more reliable
    /// than tag-searching because it runs after the scene is fully set up.
    /// Pass null for any slider you don't want to change.
    /// </summary>
    public void RegisterSliders(Slider health, Slider stamina, Slider hunger, Slider thirst)
    {
        if (health  != null) { healthSlider  = health;  healthSlider.maxValue  = maxHealth; }
        if (stamina != null) { staminaSlider = stamina; staminaSlider.maxValue = maxStamina; }
        if (hunger  != null) { hungerSlider  = hunger;  hungerSlider.maxValue  = maxHunger; }
        if (thirst  != null) { thirstSlider  = thirst;  thirstSlider.maxValue  = maxThirst; }
        UpdateUI();
        string sliderName = healthSlider != null ? healthSlider.gameObject.name : "NULL";
        Debug.Log($"[SurvivalSystem] RegisterSliders called. healthSlider={sliderName}, currentHealth={currentHealth}");
    }

    /// <summary>
    /// Immediately restores health to max and refreshes the HUD.
    /// Call this before reloading the scene on retry so the bar
    /// doesn't stay stuck at the dead (0 HP) visual.
    /// </summary>
    public void RestoreFullHealth()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }

    private void SetHUDVisible(bool visible)
    {
        // Toggle every Canvas in the root hierarchy so the HUD
        // disappears in non-gameplay scenes without disabling the
        // MonoBehaviours that need to keep running.
        Canvas[] canvases = transform.root.GetComponentsInChildren<Canvas>(true);
        foreach (Canvas c in canvases)
            c.enabled = visible;
    }

    void UpdateUI()
    {
        if (healthSlider)
            healthSlider.value = currentHealth;
        else
            Debug.LogWarning("[SurvivalSystem] UpdateUI: healthSlider is NULL — bar cannot update!");

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
        Debug.Log($"[SurvivalSystem] TakeDamage({amount}) → currentHealth={currentHealth}, " +
                  $"healthSlider={(healthSlider != null ? healthSlider.gameObject.name : "NULL")}");
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            UpdateUI();   // show 0 HP before OnDeath; Update() won't run after IsDead=true
            OnDeath();
        }
        else
        {
            UpdateUI();   // reflect damage immediately, don't wait for next Update() frame
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
    public void Heal(float amount)  { currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth); UpdateUI(); }
    public void Eat(float amount)   { currentHunger = Mathf.Clamp(currentHunger + amount, 0f, maxHunger); UpdateUI(); }
    public void Drink(float amount) { currentThirst = Mathf.Clamp(currentThirst + amount, 0f, maxThirst); UpdateUI(); }
}
