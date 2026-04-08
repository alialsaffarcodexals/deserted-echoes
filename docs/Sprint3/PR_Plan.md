# Sprint 3 — Pull Request Plan
## Deserted Echoes | IT8101 Games Development
**Sprint Duration:** April 22 – 28, 2026
**Sprint Theme:** Survival Systems, Environmental Hazards & Custom Feature Foundations

---

## Prerequisite — Sprint 2 Must Be Fully Merged

> Before Sprint 3 begins, verify `develop` contains all Sprint 2 work:
> - `EnemyBase.cs` with full AI state machine (patrol, detect, chase, attack, die)
> - `PlayerHealth.cs` with `onHealthChanged` UnityEvent, `TakeDamage()`, `Die()`, respawn
> - `PlayerCombat.cs` with hitbox, `HitboxDetector.cs`, `Knockback.cs`
> - `AudioManager.cs` with `PlayRandomSFX()`, all combat SFX imported
> - Goblin enemy fully functional in Level_01 with EnemySpawner + patrol points
> - All 6 members pull latest `develop` before creating Sprint 3 branches

---

## Git Branching Structure (Sprint 3)

```
develop
  ├── feature/s3-scene-fader           (Ali Husain)  ← WAVE 0 BLOCKER
  ├── feature/s3-open-world-hub        (Ali Husain)
  ├── feature/s3-level-transition      (Ali Husain)
  ├── feature/s3-portal-prefab         (Ali Husain)
  ├── feature/s3-day-night-cycle       (Ali Abdulla) ← WAVE 0 BLOCKER (many depend on it)
  ├── feature/s3-night-visibility      (Ali Abdulla)
  ├── feature/s3-flashlight            (Ali Abdulla)
  ├── feature/s3-save-system           (Ali Abdulla)
  ├── feature/s3-difficulty-so         (Faisal)      ← WAVE 0 (standalone, no deps)
  ├── feature/s3-difficulty-ui         (Faisal)
  ├── feature/s3-difficulty-scaling    (Faisal)
  ├── feature/s3-difficulty-hud        (Faisal)
  ├── feature/s3-player-stats          (Habib)       ← WAVE 0 BLOCKER (env zones depend on it)
  ├── feature/s3-stamina-system        (Habib)
  ├── feature/s3-stamina-hud           (Habib)
  ├── feature/s3-inventory-panel       (Habib)
  ├── feature/s3-env-zones             (Khizar)
  ├── feature/s3-heat-cold-damage      (Khizar)
  ├── feature/s3-collectible-items     (Khizar)
  ├── feature/s3-item-pickup           (Khizar)
  ├── feature/s3-weather-system        (Rawh)
  ├── feature/s3-sandstorm-particles   (Rawh)
  ├── feature/s3-weather-effects       (Rawh)
  └── feature/s3-audio-weather         (Rawh)
```

---

## PR Wave Overview (Concurrent vs Sequential)

```
DAY 1 (Apr 22)              DAY 2 (Apr 23)         DAY 3 (Apr 24)             DAY 4 (Apr 25)         DAY 5-6 (Apr 26-27)   DAY 7 (Apr 28)
──────────────────────────  ─────────────────────  ─────────────────────────  ─────────────────────  ────────────────────  ──────────────
 WAVE 0                      WAVE 1                  WAVE 2                     WAVE 3                 WAVE 4                WAVE 5
 [CONCURRENT BLOCKERS]        [CONCURRENT]            [CONCURRENT]               [CONCURRENT]           [CONCURRENT]          [INTEGRATION]

 PR-01  scene-fader           PR-05  open-world-hub  PR-08  night-visibility     PR-12  stamina-hud    PR-16  level-transit.  PR-20
 PR-02  day-night-cycle       PR-06  env-zones        PR-09  heat-cold-damage     PR-13  difficulty-sc. PR-17  portal-prefab   integration
 PR-03  difficulty-so         PR-07  weather-system   PR-10  collectible-items    PR-14  item-pickup    PR-18  save-system
 PR-04  player-stats          PR-11  stamina-system   PR-15  sandstorm-particles  PR-19  audio-weather  PR-19a inv-panel
                              PR-11b difficulty-ui    PR-09b flashlight
```

> **Four concurrent Wave 0 blockers** this sprint — all independent of each other, all open on Day 1.
> The large number of interdependencies this sprint makes Wave 0 critical: do not skip it.

---

## Wave 0 — CONCURRENT BLOCKERS (Day 1, Apr 22)
> All 4 PRs are independent of each other. Open simultaneously. Assign different reviewers.
> Aim to merge all 4 by end of Day 1.

### PR-01 | `feature/s3-scene-fader`
**Owner:** Ali Husain Ali Alsaffar
**Branch:** `feature/s3-scene-fader` → `develop`
**Open:** Apr 22 AM | **Target Merge:** Apr 22
**Reviewer:** Ali Abdulla

**What this PR contains:**
- `SceneFader.cs` — persistent singleton, one black-fill UI Canvas `Image` (alpha 0 on start)
- `FadeOut(float duration)` coroutine: alpha 0→1 over `duration` seconds
- `FadeIn(float duration)` coroutine: alpha 1→0 over `duration` seconds
- `LoadSceneWithFade(string sceneName, float fadeDuration = 0.5f)`: FadeOut → `LoadSceneAsync` → FadeIn
- `SceneLoader.cs` updated to call `SceneFader.Instance.LoadSceneWithFade()` instead of direct load

**Depends on:** Sprint 2 `develop` ✓
**Blocks:** PR-05 (Hub scene uses portal transitions), PR-16 (LevelTransition uses this for scene loading)

---

### PR-02 | `feature/s3-day-night-cycle`
**Owner:** Ali Abdulla
**Branch:** `feature/s3-day-night-cycle` → `develop`
**Open:** Apr 22 AM | **Target Merge:** Apr 22
**Reviewer:** Khizar Azhar (his heat/cold system depends on IsDay/IsNight methods)

**What this PR contains:**
- `DayNightCycle.cs` singleton:
  - `float timeOfDay` (0–24), increments `timeOfDay += Time.deltaTime * timeSpeed`
  - Loops back to 0 at 24
  - `UnityEvent OnDayStart` (fired when timeOfDay crosses 6.0)
  - `UnityEvent OnNightStart` (fired when timeOfDay crosses 20.0)
  - Public methods: `bool IsDay()` → `timeOfDay >= 6 && timeOfDay < 20`, `bool IsNight()` → inverse
  - `float GetNightBlend()` → 0–1 value representing how deep into night (for lerping visibility)
- `DirectionalLight` (or `Global Light 2D`) driven by `DayNightCycle`:
  - `Gradient dayNightColorCurve` (Serializable): dawn peach → noon bright white → dusk orange → night deep blue
  - Intensity curve: noon=1.0, midnight=0.15
- Day/Night HUD indicator: sun/moon sprite swap + in-game clock TMP "18:45" format
- `DayNightCycle` GameObject placed in `Level_01` scene

**Depends on:** Sprint 2 `develop` ✓
**Blocks:** PR-08 (night visibility needs `GetNightBlend()`), PR-09 (heat/cold uses `IsDay()`/`IsNight()`), PR-09b (flashlight enabled at night), PR-07 (weather can hook into day/night events)

---

### PR-03 | `feature/s3-difficulty-so`
**Owner:** Faisal Alasfoor
**Branch:** `feature/s3-difficulty-so` → `develop`
**Open:** Apr 22 AM | **Target Merge:** Apr 22
**Reviewer:** Habib Husain (PlayerStats reads difficulty on start)

**What this PR contains:**
- `DifficultySettings.cs` ScriptableObject:
  - Fields: `string difficultyName`, `float enemyHealthMult`, `float enemyDamageMult`, `float playerStartingHealth`, `float resourceSpawnRate`, `float environmentDamageMult`
- 3 ScriptableObject assets in `Assets/ScriptableObjects/Difficulty/`:
  - `Easy.asset`: enemyHealthMult=0.7, enemyDamageMult=0.7, playerStartingHealth=150, resourceSpawnRate=1.5, envDamageMult=0.5
  - `Normal.asset`: all multipliers=1.0, playerStartingHealth=100, resourceSpawnRate=1.0, envDamageMult=1.0
  - `Hard.asset`: enemyHealthMult=1.5, enemyDamageMult=1.5, playerStartingHealth=75, resourceSpawnRate=0.6, envDamageMult=1.5
- `GameManager.cs` updated: add `DifficultySettings activeDifficulty` field, default = Normal

**Depends on:** Sprint 2 `develop` ✓
**Blocks:** PR-11b (difficulty UI sets `activeDifficulty`), PR-13 (enemy scaling reads from `activeDifficulty`), PR-04 (PlayerStats reads `playerStartingHealth`)

---

### PR-04 | `feature/s3-player-stats`
**Owner:** Habib Husain Alshoofa
**Branch:** `feature/s3-player-stats` → `develop`
**Open:** Apr 22 AM | **Target Merge:** Apr 22
**Reviewer:** Khizar Azhar (EnvironmentalZone calls ApplyHeat/ApplyCold)

**What this PR contains:**
- `PlayerStats.cs` fully expanded (replaces Sprint 1 stub):
  - `float maxHealth`, `currentHealth` (from `GameManager.activeDifficulty.playerStartingHealth` on `Start()`)
  - `float maxStamina = 100f`, `currentStamina = 100f`
  - `float temperature = 50f` (0=freezing, 100=burning, 50=safe)
  - `UnityEvent<float> onStaminaChanged`, `UnityEvent<float> onTemperatureChanged`
  - `public void ApplyHeat(float amount)` — increases temperature, clamp 0-100, fire event
  - `public void ApplyCold(float amount)` — decreases temperature, clamp 0-100, fire event
  - `public void DrainStamina(float amount)` and `RegenerateStamina(float amount)` — fire `onStaminaChanged`
- `PlayerStats` connected to `PlayerHealth.maxHealth` on `Start()` via `activeDifficulty`
- `PlayerPrefab` updated with expanded `PlayerStats` component

**Depends on:** PR-03 ✓ (reads `activeDifficulty.playerStartingHealth`)
**Blocks:** PR-11 (stamina system calls DrainStamina/RegenerateStamina), PR-06 (env zones call ApplyHeat/ApplyCold), PR-12 (stamina HUD subscribes to events)

---

## Wave 1 — CONCURRENT (Day 2, Apr 23)
> All PRs in this wave open simultaneously after their specific Wave 0 dependency merges.

### PR-05 | `feature/s3-open-world-hub`
**Owner:** Ali Husain Ali Alsaffar
**Branch:** `feature/s3-open-world-hub` → `develop`
**Open:** Apr 23 | **Target Merge:** Apr 24
**Reviewer:** Rawh Hasan (level layout review)

**What this PR contains:**
- `OpenWorldHub` scene created and built:
  - Desert terrain/floor (~50×50 units), atmospheric desert art, warm lighting (Global Light 2D, daytime)
  - 6 `TunnelEntrance` GameObjects arranged around the hub (each labels which 2 levels it leads to)
  - `PlayerSpawnPoint` at hub center
  - `LevelManager` instance in hub scene (spawns player)
  - `DayNightCycle` instance in hub scene
  - Placeholder portal door positions (visual markers) — actual prefab in PR-17

**Depends on:** PR-01 ✓ (SceneFader needed for portal transitions)
**Blocks:** PR-16 (LevelTransition.cs script placed on portal triggers in this scene), PR-17 (Portal prefab placed here)

---

### PR-06 | `feature/s3-env-zones`
**Owner:** Khizar Azhar
**Branch:** `feature/s3-env-zones` → `develop`
**Open:** Apr 23 | **Target Merge:** Apr 23
**Reviewer:** Habib Husain (PlayerStats methods are called here)

**What this PR contains:**
- `EnvironmentalZone.cs`:
  - `ZoneType` enum: `Normal`, `Hot`, `Cold`, `Shade`, `Warm`
  - `float damagePerSecond` (configurable per zone)
  - `OnTriggerStay2D`: find `PlayerStats` on other → call `ApplyHeat(damagePerSecond * Time.deltaTime)` (Hot) or `ApplyCold(damagePerSecond * Time.deltaTime)` (Cold)
  - `OnTriggerEnter2D` / `OnTriggerExit2D`: track overlapping zones, priority: Shade overrides Hot, Warm overrides Cold
- `ShadeZone.prefab` and `WarmZone.prefab` (invisible box colliders, tagged zone type)
- Test: 2 Hot zones and 1 Cold zone placed in Level_01 for testing

**Depends on:** PR-04 ✓ (PlayerStats.ApplyHeat/ApplyCold must exist)
**Blocks:** PR-09 (heat/cold damage via day/night uses same zone system)

---

### PR-07 | `feature/s3-weather-system`
**Owner:** Rawh Hasan
**Branch:** `feature/s3-weather-system` → `develop`
**Open:** Apr 23 | **Target Merge:** Apr 23
**Reviewer:** Ali Abdulla

**What this PR contains:**
- `WeatherSystem.cs` singleton:
  - `WeatherState` enum: `Clear`, `Windy`, `Sandstorm`
  - `currentState`, random transition after `[minDuration, maxDuration]` seconds (configurable: default min=30s, max=120s)
  - `UnityEvent<WeatherState> OnWeatherChanged` — fired on every transition
  - Subscribes optionally to `DayNightCycle.OnNightStart` to increase Sandstorm chance at night
  - Public `GetCurrentWeather()` method
- `WeatherSystem` GameObject placed in `Level_01` and `OpenWorldHub` scenes

**Depends on:** PR-02 ✓ (can optionally subscribe to day/night events)
**Blocks:** PR-15 (sandstorm particles activated by WeatherSystem), PR-weather-effects (movement/visibility driven by state), PR-audio-weather (ambient sounds driven by state)

---

### PR-11 | `feature/s3-stamina-system`
**Owner:** Habib Husain Alshoofa
**Branch:** `feature/s3-stamina-system` → `develop`
**Open:** Apr 23 | **Target Merge:** Apr 23
**Reviewer:** Ali Abdulla (PlayerMovement.cs references stamina for sprint)

**What this PR contains:**
- Stamina logic in `PlayerStats.cs` (built on top of PR-04 methods):
  - `staminaDrainRate (15f/s)`, `staminaRegenRate (10f/s)`, `staminaRegenDelay (1.5f)`
  - `float regenTimer` — resets on drain, only counts up when not draining
- `PlayerMovement.cs` updated: sprint check `if (playerStats.currentStamina > 0)` → call `playerStats.DrainStamina(rate * Time.deltaTime)`; when not sprinting + `regenTimer > staminaRegenDelay` → call `playerStats.RegenerateStamina(rate * Time.deltaTime)`
- Sprint disabled (`isSprinting = false`, speed reverts) when stamina hits 0
- `PlayerStats.onStaminaChanged` event fires on every change

**Depends on:** PR-04 ✓
**Blocks:** PR-12 (stamina HUD subscribes to event from this PR)

---

### PR-11b | `feature/s3-difficulty-ui`
**Owner:** Faisal Alasfoor
**Branch:** `feature/s3-difficulty-ui` → `develop`
**Open:** Apr 23 | **Target Merge:** Apr 23
**Reviewer:** Ali Husain

**What this PR contains:**
- `MainMenu` scene updated with "Select Difficulty" panel:
  - 3 TMP buttons: Easy (green), Normal (yellow), Hard (red)
  - Each button description: Easy — "Relaxed survival. More resources, weaker enemies.", Normal — "Balanced experience.", Hard — "Unforgiving. Limited resources, stronger enemies."
  - `DifficultySelector.cs`: `OnDifficultySelected(DifficultySettings settings)` → `GameManager.Instance.activeDifficulty = settings`
  - Difficulty assets assigned to each button in Inspector
  - Selected difficulty shown with highlighted border
- Panel appears after "Start Game" clicked, before scene load

**Depends on:** PR-03 ✓ (DifficultySettings assets must exist)
**Blocks:** PR-13 (enemy scaling needs `activeDifficulty` to be set)

---

## Wave 2 — CONCURRENT (Day 3, Apr 24)
> Open all simultaneously after their specific Wave 1 dependencies merge.

### PR-08 | `feature/s3-night-visibility`
**Owner:** Ali Abdulla
**Branch:** `feature/s3-night-visibility` → `develop`
**Open:** Apr 24 | **Target Merge:** Apr 25
**Reviewer:** Rawh Hasan (weather visibility compounds with this)

**What this PR contains:**
- `NightVisibilityOverlay.cs` on HUD Canvas:
  - UI `Image` (black-fill, `raycastTarget=false`) covering full screen
  - In `Update()`: `alpha = Mathf.Lerp(0f, 0.85f, dayNightCycle.GetNightBlend())`
  - Smooth transition — never pops
- Day/Night HUD indicator fully implemented:
  - Sun sprite visible during day, moon sprite at night (swap based on `IsDay()`)
  - In-game clock text: `TimeSpan.FromHours(timeOfDay).ToString(@"hh\:mm")`

**Depends on:** PR-02 ✓ (`DayNightCycle.GetNightBlend()` and `IsDay()` must exist)
**Blocks:** PR-09b (flashlight makes night navigable), PR-weather-effects (sandstorm overlay compounds)

---

### PR-09b | `feature/s3-flashlight`
**Owner:** Ali Abdulla
**Branch:** `feature/s3-flashlight` → `develop`
**Open:** Apr 24 | **Target Merge:** Apr 25
**Reviewer:** Khizar Azhar

**What this PR contains:**
- `Flashlight.cs` child of player:
  - Unity `Light 2D` (Point Light or Spot Light) component
  - `Toggle with F key`: `playerInput.Gameplay.Flashlight.performed` → `light2D.enabled = !light2D.enabled`
  - Auto-enable at night start (`DayNightCycle.OnNightStart` subscription), auto-disable at day start
  - `batteryLife (float = 120f)` — drains while on (`batteryLife -= Time.deltaTime`), disables at 0
  - `FlashlightBattery` HUD indicator (small bar, optional for this sprint)
- `Flashlight` child GameObject prefab added to `PlayerPrefab`

**Depends on:** PR-02 ✓, PR-08 ✓ (night visibility must exist to make flashlight meaningful)
**Blocks:** PR-18 (integration — flashlight is part of night system test)

---

### PR-09 | `feature/s3-heat-cold-damage`
**Owner:** Khizar Azhar
**Branch:** `feature/s3-heat-cold-damage` → `develop`
**Open:** Apr 24 | **Target Merge:** Apr 25
**Reviewer:** Habib Husain

**What this PR contains:**
- Daytime heat damage loop in `PlayerStats.cs`:
  - If `IsDay()` and player is NOT inside a `ShadeZone`: `ApplyHeat(outdoorHeatRate * activeDifficulty.environmentDamageMult * Time.deltaTime)`
- Nighttime cold damage loop:
  - If `IsNight()` and player is NOT inside a `WarmZone`: `ApplyCold(outdoorColdRate * activeDifficulty.environmentDamageMult * Time.deltaTime)`
- Temperature damage to health: if `temperature > 80f` OR `temperature < 20f`: `playerHealth.TakeDamage(tempDamagePerSecond * Time.deltaTime)` — no hit flash (environmental damage, not combat)
- Temperature HUD indicator: thermometer icon + numeric text "47°C". Color gradient: blue (<20) → white (20-80) → orange (>70) → red (>85). Updated via `onTemperatureChanged` event

**Depends on:** PR-02 ✓ (`IsDay()`/`IsNight()`), PR-04 ✓ (PlayerStats methods), PR-06 ✓ (zone system), PR-03 ✓ (difficulty multiplier)
**Blocks:** PR-18 (integration — player must take environment damage correctly)

---

### PR-10 | `feature/s3-collectible-items`
**Owner:** Khizar Azhar
**Branch:** `feature/s3-collectible-items` → `develop`
**Open:** Apr 24 | **Target Merge:** Apr 25
**Reviewer:** Rawh Hasan

**What this PR contains:**
- `ItemData.cs` ScriptableObject: `string itemName`, `Sprite icon`, `ItemType` enum (Consumable/Equipment/Resource/Key), `string description`, `bool stackable`, `int maxStack`
- 3 `ItemData` assets: `WaterCanteen.asset`, `DriedMeat.asset`, `GoldCoin.asset`
- `Collectible.cs` updated: `ItemData itemData` field, `OnTriggerEnter2D` → call `InventoryManager.Instance.AddItem(itemData)` (stub if InventoryManager not yet merged), `AudioManager.PlaySFX(pickupSFX)`, `Destroy(gameObject)`
- 3 collectible prefabs: WaterCanteen (blue sprite), DriedMeat (brown sprite), GoldCoin (yellow sprite) — each with assigned `ItemData` and updated `Collectible.cs`
- 2-3 of each placed in `Level_01` for testing

**Depends on:** PR-04 ✓ (PlayerStats exists for later stat-modification use), Sprint 1 `ICollectible` ✓
**Blocks:** PR-14 (item pickup UI + InventoryManager integration)

---

### PR-15 | `feature/s3-sandstorm-particles`
**Owner:** Rawh Hasan
**Branch:** `feature/s3-sandstorm-particles` → `develop`
**Open:** Apr 24 | **Target Merge:** Apr 25
**Reviewer:** Ali Husain

**What this PR contains:**
- `SandstormParticleSystem.cs`:
  - Subscribes to `WeatherSystem.OnWeatherChanged`
  - On `Sandstorm`: `particleSystem.Play()`, scale emission rate to high (500/s)
  - On `Windy`: `particleSystem.Play()`, emission rate low (80/s)
  - On `Clear`: `particleSystem.Stop()`, `particleSystem.Clear()`
- Particle System configuration: horizontal particles, wind direction (world-space, left→right), sandy tan color with 20% alpha, size 0.05-0.15, lifetime 2-4s
- Weather HUD icon: small sprite (sun☀ / wind 🌬 / storm⛈ equivalent icon set), swaps via `OnWeatherChanged`

**Depends on:** PR-07 ✓ (WeatherSystem must exist with OnWeatherChanged event)
**Blocks:** PR-weather-effects (movement/visibility reference particle state), PR-audio-weather (audio tracks same state)

---

### PR-13 | `feature/s3-difficulty-scaling`
**Owner:** Faisal Alasfoor
**Branch:** `feature/s3-difficulty-scaling` → `develop`
**Open:** Apr 24 | **Target Merge:** Apr 25
**Reviewer:** Ali Husain

**What this PR contains:**
- `EnemyBase.Awake()` updated:
  - `maxHealth *= GameManager.Instance.activeDifficulty.enemyHealthMult`
  - `attackDamage *= GameManager.Instance.activeDifficulty.enemyDamageMult`
  - `currentHealth = maxHealth`
- `PlayerStats.Start()` updated:
  - `maxHealth = GameManager.Instance.activeDifficulty.playerStartingHealth`
  - `currentHealth = maxHealth`
  - `PlayerHealth.maxHealth = maxHealth` (sync)
- Verified: Easy Goblin has 30×0.7=21 HP, Hard Goblin has 30×1.5=45 HP
- `environmentDamageMult` already passed to PlayerStats in PR-09

**Depends on:** PR-03 ✓, PR-11b ✓ (activeDifficulty must be set before scene loads)
**Blocks:** PR-diff-hud (badge shows active difficulty)

---

## Wave 3 — CONCURRENT (Day 4, Apr 25)

### PR-12 | `feature/s3-stamina-hud`
**Owner:** Habib Husain Alshoofa
**Branch:** `feature/s3-stamina-hud` → `develop`
**Open:** Apr 25 | **Target Merge:** Apr 26
**Reviewer:** Faisal Alasfoor (HUD consistency)

**What this PR contains:**
- `StaminaBarUI.cs` on HUD stamina bar:
  - Subscribes to `PlayerStats.onStaminaChanged`
  - Smooth fill lerp: `Mathf.Lerp(currentFill, target, lerpSpeed * Time.deltaTime)`
  - Color shift when low: green → yellow → red as stamina drops below 30
  - Pulse animation at stamina < 15 (`Mathf.PingPong` on bar color)
- Temperature HUD thermometer: color gradient from PR-09 connected to `onTemperatureChanged`
- Sprint indicator: small "SPRINTING" text fades in when `isSprinting=true`, fades out otherwise

**Depends on:** PR-11 ✓ (stamina events), PR-09 ✓ (temperature events)
**Blocks:** PR-18 (integration needs HUD to reflect all stats correctly)

---

### PR-weather-effects | `feature/s3-weather-effects`
**Owner:** Rawh Hasan
**Branch:** `feature/s3-weather-effects` → `develop`
**Open:** Apr 25 | **Target Merge:** Apr 26
**Reviewer:** Ali Abdulla (movement penalty touches PlayerMovement)

**What this PR contains:**
- `WeatherEffectsController.cs`:
  - Subscribes to `WeatherSystem.OnWeatherChanged`
  - On `Sandstorm`: `PlayerMovement.moveSpeed *= 0.7f` (30% reduction) + brownish `Image` overlay (alpha 0.55, color #8B7355) lerped in over 2s
  - On `Windy`: slight overlay (alpha 0.2) + minor speed reduction (10%)
  - On `Clear`: restore `moveSpeed` to baseline, overlay alpha → 0
  - Speed restoration uses saved baseline (don't compound multiple reductions)
- Sandstorm + Night visibility compound: both overlays stack (semi-transparent, different colors)

**Depends on:** PR-07 ✓, PR-15 ✓ (particles), PR-08 ✓ (night visibility to verify compound works)
**Blocks:** PR-audio-weather (audio tracks same state as this), PR-18 (movement penalty part of integration)

---

### PR-diff-hud | `feature/s3-difficulty-hud`
**Owner:** Faisal Alasfoor
**Branch:** `feature/s3-difficulty-hud` → `develop`
**Open:** Apr 25 | **Target Merge:** Apr 26
**Reviewer:** Habib Husain

**What this PR contains:**
- Difficulty badge in HUD (top-right corner below lives): TMP text showing difficulty name
- Color: Easy=green (`#2ECC71`), Normal=yellow (`#F4D03F`), Hard=red (`#E74C3C`)
- `DifficultyHUDBadge.cs`: on `Start()`, reads `GameManager.Instance.activeDifficulty.difficultyName`, sets text + color
- `DifficultySettings` asset: `.difficultyName` field populated if not already ("Easy", "Normal", "Hard")

**Depends on:** PR-03 ✓, PR-13 ✓ (scaling confirms difficulty is applied)
**Blocks:** PR-18 (HUD completeness check in integration)

---

### PR-14 | `feature/s3-item-pickup`
**Owner:** Khizar Azhar
**Branch:** `feature/s3-item-pickup` → `develop`
**Open:** Apr 25 | **Target Merge:** Apr 26
**Reviewer:** Habib Husain

**What this PR contains:**
- `InventoryManager.cs` singleton:
  - `[System.Serializable] ItemSlot { ItemData data; int quantity; }`
  - `List<ItemSlot> slots` (max 20)
  - `AddItem(ItemData item)`: find existing stack → increment, else add new slot. Fire `OnInventoryChanged`
  - `RemoveItem(ItemData item)`: decrement quantity, remove slot if 0. Fire `OnInventoryChanged`
  - `UnityEvent OnInventoryChanged`
- Pickup notification UI: `PickupNotification.cs` — small TMP popup bottom-center: "+ Water Canteen", fades out over 1.5s, queue system (multiple pickups don't overlap)
- `Collectible.cs` updated: calls `InventoryManager.Instance.AddItem(itemData)` (replaces stub)
- `GoldCoin` collectible: `CurrencyManager.cs` stub — `int gold`, `AddGold(int)`, updates gold display in HUD

**Depends on:** PR-10 ✓ (ItemData + collectible prefabs)
**Blocks:** PR-19a (inventory panel UI subscribes to `OnInventoryChanged`)

---

## Wave 4 — CONCURRENT (Day 5-6, Apr 26-27)

### PR-16 | `feature/s3-level-transition`
**Owner:** Ali Husain Ali Alsaffar
**Branch:** `feature/s3-level-transition` → `develop`
**Open:** Apr 26 | **Target Merge:** Apr 27
**Reviewer:** Ali Abdulla (save system can be triggered by portal enter)

**What this PR contains:**
- `LevelTransition.cs`:
  - `string targetScene` (set in Inspector per portal)
  - `bool saveOnTransition` flag
  - `OnTriggerEnter2D`: check Player tag → if `saveOnTransition && SaveManager exists`: `SaveManager.Instance.SaveGame()` → `SceneFader.Instance.LoadSceneWithFade(targetScene)`
- Portal trigger colliders placed at each `TunnelEntrance` in `OpenWorldHub` — each with `LevelTransition` component and `targetScene` set
- Return portal trigger placed at end of `Level_01` → transitions back to `OpenWorldHub`
- `DoneIndicator` placeholder: green tint on completed portal (static for now, dynamic in Sprint 4)

**Depends on:** PR-01 ✓ (SceneFader), PR-05 ✓ (Hub scene must exist)
**Blocks:** PR-18 (portal transition is key integration feature)

---

### PR-17 | `feature/s3-portal-prefab`
**Owner:** Ali Husain Ali Alsaffar
**Branch:** `feature/s3-portal-prefab` → `develop`
**Open:** Apr 26 | **Target Merge:** Apr 27
**Reviewer:** Rawh Hasan (audio plays on portal enter)

**What this PR contains:**
- `PortalDoor.prefab`:
  - Animated sprite (swirling gate, or glowing arch — use available sprite or simple colored shape with material)
  - `Point Light 2D` child — blue/purple glow, medium intensity (0.8)
  - Particle System: swirling particles around portal origin
  - `LevelTransition.cs` attached (Inspector configures `targetScene` per instance)
  - Hover interaction: `OnMouseOver` / proximity — show TMP label "→ Level 1" above portal
- Portal instances placed in `OpenWorldHub` at all 6 tunnel entrance positions
- Return portal prefab variant: smaller, green tint, label "→ Return to Hub"

**Depends on:** PR-05 ✓, PR-16 ✓
**Blocks:** PR-18 (portal visual part of integration test)

---

### PR-18save | `feature/s3-save-system`
**Owner:** Ali Abdulla
**Branch:** `feature/s3-save-system` → `develop`
**Open:** Apr 26 | **Target Merge:** Apr 27
**Reviewer:** Ali Husain (portal transition calls SaveGame)

**What this PR contains:**
- `SaveData.cs` `[System.Serializable]` class:
  - `Vector3 playerPosition`, `float currentHealth`, `float currentStamina`, `float temperature`, `int gold`, `int playerLives`, `string currentScene`, `string difficultyName`
  - `List<string> inventoryItemNames` (names of ItemData assets for now)
- `SaveManager.cs` singleton:
  - `SaveGame()`: builds `SaveData`, `JsonUtility.ToJson()`, `File.WriteAllText(path + "/save.json", json)`
  - `LoadGame()`: read file → `JsonUtility.FromJson<SaveData>()` → restore player state
  - `bool SaveExists()`: `File.Exists(savePath)`
  - Path: `Application.persistentDataPath + "/save.json"`
- "Game Saved" HUD notification: 2s TMP popup bottom-center when `SaveGame()` called

**Depends on:** PR-04 ✓ (PlayerStats data to save), PR-10 ✓ (collectible items to save list)
**Blocks:** PR-16 (portal calls `SaveManager.SaveGame()` on enter), PR-18 (integration verifies save works)

---

### PR-19a | `feature/s3-inventory-panel`
**Owner:** Habib Husain Alshoofa
**Branch:** `feature/s3-inventory-panel` → `develop`
**Open:** Apr 26 | **Target Merge:** Apr 27
**Reviewer:** Faisal Alasfoor (UI consistency)

**What this PR contains:**
- `InventoryPanel.cs` — UI Canvas panel (Tab to toggle, `Time.timeScale` unchanged):
  - 4×5 grid of `InventorySlot` prefabs: each slot = background Image + icon Image + count TMP text
  - Subscribes to `InventoryManager.OnInventoryChanged` → `RefreshUI()` rebuilds all slots
  - `RefreshUI()`: loops `InventoryManager.slots`, sets icon from `ItemData.icon`, count text
  - Empty slots show dimmed placeholder icon
- `PlayerProfile.cs` stub: `string playerName`, `float totalPlaytime` (incremented in Update), `int enemiesKilled`, `int levelsCompleted`

**Depends on:** PR-14 ✓ (InventoryManager with AddItem and OnInventoryChanged)
**Blocks:** PR-18 (inventory opens correctly in integration test)

---

### PR-audio-weather | `feature/s3-audio-weather`
**Owner:** Rawh Hasan
**Branch:** `feature/s3-audio-weather` → `develop`
**Open:** Apr 26 | **Target Merge:** Apr 27
**Reviewer:** Ali Abdulla

**What this PR contains:**
- `WeatherAudio.cs` — subscribes to `WeatherSystem.OnWeatherChanged`:
  - `Clear`: crossfade to `ambient_wind_soft.mp3` (gentle loop, vol 0.3)
  - `Windy`: crossfade to `ambient_wind_strong.mp3` (vol 0.55)
  - `Sandstorm`: crossfade to `ambient_sandstorm.mp3` (vol 0.8) + play one-shot `sfx_sandstorm_start.wav`
  - Crossfade: `StartCoroutine(CrossfadeMusic(newClip, fadeDuration=1.5f))` using two AudioSources (ping-pong fading)
- `AudioManager` updated: second music `AudioSource` added for crossfade support
- 3 ambient audio clips sourced and imported in `Assets/Audio/Ambient/`

**Depends on:** PR-07 ✓, PR-15 ✓ (particles confirmed working), Sprint 2 AudioManager ✓
**Blocks:** PR-18 (audio is part of integration test)

---

## Wave 5 — INTEGRATION (Day 7, Apr 28)

### PR-20 | Sprint 3 Integration Verification
**Owner:** Ali Husain (Scrum Master)
**Branch:** No new branch — verify `develop` is clean
**Date:** Apr 28

**Full Sprint 3 Integration Checklist:**
- [ ] Day/Night cycle visible — lighting changes from warm day to dark night
- [ ] Night visibility vignette fades in at night, clears at dawn
- [ ] Flashlight toggles with F key, drains battery over time
- [ ] Player stamina drains while sprinting, HUD bar updates smoothly
- [ ] Player stops sprinting when stamina = 0, regen starts after delay
- [ ] Player takes heat damage outdoors during day — temperature HUD rises
- [ ] Player takes cold damage outdoors at night — temperature HUD drops
- [ ] Sandstorm triggers randomly — particles visible, player slows down
- [ ] Sandstorm visibility overlay visible and compounds with night vignette
- [ ] Ambient audio changes between Clear/Windy/Sandstorm (crossfade works)
- [ ] Water/Food/Coin collectibles in level — player picks them up, inventory shows items
- [ ] Pickup notification appears and fades correctly
- [ ] Tab key opens inventory panel — items visible in grid slots
- [ ] Difficulty selection on Main Menu — Hard mode = enemies have more HP confirmed
- [ ] Difficulty badge visible on HUD
- [ ] OpenWorldHub scene loads, portals visible with glow + particle effect
- [ ] Walking into portal trigger → fade to black → load Level_01 → fade in
- [ ] Return portal at end of level → fade → Hub scene
- [ ] "Game Saved" notification appears on portal entry
- [ ] No console errors during all of the above

---

## Full Sprint 3 PR Timeline

| PR | Branch | Owner | Opens | Merges | Concurrent With | Blocked By |
|----|--------|-------|-------|--------|-----------------|------------|
| PR-01 | `s3-scene-fader` | Ali Husain | Apr 22 AM | Apr 22 | PR-02,03,04 | Sprint 2 done |
| PR-02 | `s3-day-night-cycle` | Ali Abdulla | Apr 22 AM | Apr 22 | PR-01,03,04 | Sprint 2 done |
| PR-03 | `s3-difficulty-so` | Faisal | Apr 22 AM | Apr 22 | PR-01,02,04 | Sprint 2 done |
| PR-04 | `s3-player-stats` | Habib | Apr 22 AM | Apr 22 | PR-01,02,03 | PR-03 |
| PR-05 | `s3-open-world-hub` | Ali Husain | Apr 23 | Apr 24 | PR-06,07,11,11b | PR-01 |
| PR-06 | `s3-env-zones` | Khizar | Apr 23 | Apr 23 | PR-05,07,11,11b | PR-04 |
| PR-07 | `s3-weather-system` | Rawh | Apr 23 | Apr 23 | PR-05,06,11,11b | PR-02 |
| PR-11 | `s3-stamina-system` | Habib | Apr 23 | Apr 23 | PR-05,06,07,11b | PR-04 |
| PR-11b | `s3-difficulty-ui` | Faisal | Apr 23 | Apr 23 | PR-05,06,07,11 | PR-03 |
| PR-08 | `s3-night-visibility` | Ali Abdulla | Apr 24 | Apr 25 | PR-09,09b,10,13,15 | PR-02 |
| PR-09 | `s3-heat-cold-damage` | Khizar | Apr 24 | Apr 25 | PR-08,09b,10,13,15 | PR-02,04,06,03 |
| PR-09b | `s3-flashlight` | Ali Abdulla | Apr 24 | Apr 25 | PR-08,09,10,13,15 | PR-02,08 |
| PR-10 | `s3-collectible-items` | Khizar | Apr 24 | Apr 25 | PR-08,09,09b,13,15 | PR-04 |
| PR-13 | `s3-difficulty-scaling` | Faisal | Apr 24 | Apr 25 | PR-08,09,09b,10,15 | PR-03,11b |
| PR-15 | `s3-sandstorm-particles` | Rawh | Apr 24 | Apr 25 | PR-08,09,09b,10,13 | PR-07 |
| PR-12 | `s3-stamina-hud` | Habib | Apr 25 | Apr 26 | PR-we,PR-dh,PR-14 | PR-11,09 |
| PR-we | `s3-weather-effects` | Rawh | Apr 25 | Apr 26 | PR-12,PR-dh,PR-14 | PR-07,15,08 |
| PR-dh | `s3-difficulty-hud` | Faisal | Apr 25 | Apr 26 | PR-12,PR-we,PR-14 | PR-03,13 |
| PR-14 | `s3-item-pickup` | Khizar | Apr 25 | Apr 26 | PR-12,PR-we,PR-dh | PR-10 |
| PR-16 | `s3-level-transition` | Ali Husain | Apr 26 | Apr 27 | PR-17,18s,19a,aw | PR-01,05 |
| PR-17 | `s3-portal-prefab` | Ali Husain | Apr 26 | Apr 27 | PR-16,18s,19a,aw | PR-05,16 |
| PR-18s | `s3-save-system` | Ali Abdulla | Apr 26 | Apr 27 | PR-16,17,19a,aw | PR-04,10 |
| PR-19a | `s3-inventory-panel` | Habib | Apr 26 | Apr 27 | PR-16,17,18s,aw | PR-14 |
| PR-aw | `s3-audio-weather` | Rawh | Apr 26 | Apr 27 | PR-16,17,18s,19a | PR-07,15 |
| PR-20 | Integration check | Ali Husain | Apr 28 | Apr 28 | — | All above |

---

## Dependency Chain (Sequential Critical Path)

```
[Sprint 2 develop]
  │
  ├──► PR-03 (difficulty-so) ──► PR-11b (diff-ui) ──► PR-13 (diff-scaling) ──► PR-dh (diff-hud)
  │        └──────────────────────────────────────────────────────────────────────────────────────────┐
  │                                                                                                   │
  ├──► PR-04 (player-stats) ──► PR-11 (stamina) ──► PR-12 (stamina-hud)                            │
  │         └──► PR-06 (env-zones) ──► PR-09 (heat-cold) ──► PR-12                                  │
  │         └──► PR-10 (collectibles) ──► PR-14 (item-pickup) ──► PR-19a (inventory-panel)          │
  │                                                                                                   │
  ├──► PR-02 (day-night) ──► PR-08 (night-visibility) ──► PR-09b (flashlight)                       │
  │         └──────────────────► PR-09 (heat-cold)                                                   │
  │         └──► PR-07 (weather) ──► PR-15 (particles) ──► PR-we (weather-effects)                  │
  │                                       └──► PR-aw (audio-weather)                                 │
  │                                                                                                   │
  └──► PR-01 (scene-fader) ──► PR-05 (hub-scene) ──► PR-16 (level-transition) ──► PR-17 (portal)   │
                                                           └──► PR-18s (save-system)                 │
                                                                                                     │
All above ──────────────────────────────────────────────────────────────────────────────► PR-20 (integration)
```

---

*Sprint 3 PR Plan | Deserted Echoes | Group 3 | Prepared: April 8, 2026*
