# Sprint 3 – Member Task Plan
## Deserted Echoes | IT8101 Games Development
**Sprint Duration:** April 22 – 28, 2026
**Sprint Theme:** Survival Systems, Environmental Hazards & Custom Feature Foundations
**Sprint Goal:** Build the core survival loop (stamina, temperature, day/night) and lay foundations for each member's GDD-assigned custom feature.

---

## Team Roster

| # | Name | Student ID | GDD Custom Feature |
|---|------|-----------|-------------------|
| 1 | Ali Husain Ali Ebrahim Ali Alsaffar | 201900576 | Teleport / Fast Travel System *(Scrum Master)* |
| 2 | Ali Abdulla | 202300917 | Dynamic Night Visibility System *(Advanced)* + UI/Settings/Saving |
| 3 | Faisal Alasfoor | 202304774 | Difficulty System |
| 4 | Habib Husain Alshoofa | 202100747 | Player Profile & Inventory System |
| 5 | Khizar Azhar | 202200123 | Collectible & Equipment System |
| 6 | Rawh Hasan | 202301152 | Dynamic Weather System |

---

## Sprint 2 Carry-Over (Must Be Done Before Sprint 3 Starts)

> The following Sprint 2 deliverables must be complete before Sprint 3 begins, as Sprint 3 systems depend on them:
> - `PlayerHealth.cs` with `TakeDamage()` and `onHealthChanged` event (Habib)
> - `PlayerStats.cs` stub with `currentStamina` float (Habib)
> - `EnemyBase.cs` abstract class exists (Ali Husain)
> - `AudioManager.cs` singleton live with `PlaySFX()` / `PlayMusic()` (Rawh)
> - Player attack system and hitbox working (Ali Abdulla)
> - Goblin enemy functional in test level (Khizar)

---

## Member Task Breakdown

---

### 1. Ali Husain Ali Alsaffar — Scrum Master + Teleport / Fast Travel System
**Focus:** Sprint management + Open World Hub foundation + Teleport framework

| Task ID | Task | Description | Est. Time |
|---------|------|-------------|-----------|
| S3-AH-01 | Sprint 3 Kick-off & Trello Update | Set up Sprint 3 cards in Trello with descriptions, assignees, and due dates for all 6 members. Archive completed Sprint 2 cards. | 1 hour |
| S3-AH-02 | Open World Hub Scene Creation | Create a new Unity scene called `OpenWorldHub`. Add basic terrain/layout representing the desert hub with 6 tunnel entrance placeholder objects. | 2 hours |
| S3-AH-03 | Level Transition System (Foundation) | Write a `LevelTransition.cs` script that loads a target scene by name when the player enters a trigger zone. Add this trigger to the hub tunnel entrances. | 3 hours |
| S3-AH-04 | Portal Door Prefab | Create a visual `PortalDoor` prefab with a glowing effect (shader or sprite animation). Place two instances per tunnel entrance (entry + return). | 2 hours |
| S3-AH-05 | Return Portal in Level Scenes | Add a return portal trigger at the end of at least one test level that loads back `OpenWorldHub`. | 1.5 hours |
| S3-AH-06 | Sprint Review & Integration Check | At end of week, run the game and verify all Sprint 3 systems integrate without breaking Sprint 1/2 work. Update Trello with results. | 1.5 hours |

**Sprint 3 Deliverable:** A working Open World Hub scene with portal triggers that transition between the hub and at least one level.

---

### 2. Ali Abdulla — Dynamic Night Visibility System (ADVANCED) + UI/Saving Foundation
**Focus:** Day/Night Cycle + Night Visibility reduction + Save system start

| Task ID | Task | Description | Est. Time |
|---------|------|-------------|-----------|
| S3-AA-01 | Day/Night Time System | Write a `DayNightCycle.cs` script that uses a `float timeOfDay` (0–24 scale). Update every frame based on `Time.deltaTime * timeSpeed`. Expose `timeSpeed` in Inspector. | 2 hours |
| S3-AA-02 | Dynamic Sky/Lighting | Drive a `DirectionalLight` rotation and color/intensity from the time system. Use `Gradient` or `AnimationCurve` for smooth color transitions (warm dawn → bright noon → orange dusk → dark night). | 2.5 hours |
| S3-AA-03 | Night Visibility Reduction | During night hours (20:00–06:00 in-game time), apply a dark vignette overlay on the HUD Canvas using a UI Image with radial gradient. Lerp alpha based on time. | 2 hours |
| S3-AA-04 | Flashlight Prefab | Create a `Flashlight` child object on the player. Use Unity's `Light 2D` (or a cone-shaped sprite mask) to simulate a flashlight cone. Toggle with `F` key. | 2 hours |
| S3-AA-05 | Day/Night HUD Indicator | Add a small sun/moon icon to the HUD that switches based on daytime/nighttime. Show current in-game time as text (e.g., "14:35"). | 1 hour |
| S3-AA-06 | Save System Foundation | Create a `SaveData.cs` class with `[System.Serializable]` fields: player position, health, stamina, inventory list (empty for now). Write `SaveManager.cs` with `SaveGame()` and `LoadGame()` using `JsonUtility` + `File.WriteAllText`. | 3 hours |

**Sprint 3 Deliverable:** A live day/night cycle with dynamic lighting and night visibility reduction; flashlight togglable. A basic save/load manager that can serialize player state.

---

### 3. Faisal Alasfoor — Difficulty System
**Focus:** Full difficulty selection pipeline from UI to gameplay scaling

| Task ID | Task | Description | Est. Time |
|---------|------|-------------|-----------|
| S3-FA-01 | Difficulty Enum & ScriptableObject | Create `DifficultyLevel` enum (Easy, Normal, Hard) and a `DifficultySettings.cs` ScriptableObject with fields: `enemyHealthMultiplier`, `enemyDamageMultiplier`, `playerStartingHealth`, `resourceSpawnRate`. Create 3 assets (one per difficulty). | 2 hours |
| S3-FA-02 | Game Manager Difficulty Integration | In `GameManager.cs` (or create one if not existing), store the selected `DifficultySettings`. Load it at scene start and apply to relevant systems. | 1.5 hours |
| S3-FA-03 | Difficulty Selection UI | In the Main Menu scene, add a "Select Difficulty" panel with 3 buttons (Easy / Normal / Hard). On click, set the active `DifficultySettings` in `GameManager`. | 2.5 hours |
| S3-FA-04 | Enemy Health & Damage Scaling | In the Enemy base script (from Sprint 2), multiply `maxHealth` and `attackDamage` by `DifficultySettings.enemyHealthMultiplier` / `enemyDamageMultiplier` on `Start()`. | 2 hours |
| S3-FA-05 | Player Starting Resources Scaling | Adjust player starting health (and later water/food) based on `playerStartingHealth` field from selected difficulty. Hook into `PlayerStats.cs`. | 1.5 hours |
| S3-FA-06 | Difficulty Indicator on HUD | Add a small label/badge to HUD showing current difficulty (e.g., "HARD" in red). Fetch from `GameManager.currentDifficulty`. | 1 hour |

**Sprint 3 Deliverable:** A fully working difficulty pipeline — player selects Easy/Normal/Hard in the main menu, and enemy stats + player starting health scale accordingly.

---

### 4. Habib Husain Alshoofa — Player Stats + Stamina System
**Focus:** Stamina System + Temperature tracking + Player stats data layer

| Task ID | Task | Description | Est. Time |
|---------|------|-------------|-----------|
| S3-HH-01 | Player Stats Data Class | Create `PlayerStats.cs` with properties: `currentHealth`, `maxHealth`, `currentStamina`, `maxStamina`, `temperature`, `isAlive`. Use `UnityEvent` callbacks for onHealthChanged, onStaminaChanged. | 2 hours |
| S3-HH-02 | Stamina Depletion on Sprint | Detect `Shift` key in movement script. While sprinting, decrease `currentStamina` by `staminaDrainRate * Time.deltaTime`. Disable sprint when stamina hits 0. | 2 hours |
| S3-HH-03 | Stamina Regeneration | When not sprinting and stamina < max, increase by `staminaRegenRate * Time.deltaTime` after a `staminaRegenDelay` cooldown. | 1.5 hours |
| S3-HH-04 | Stamina Bar UI | Add a stamina bar (UI Slider or Image with fill amount) below the health bar in HUD. Subscribe to `onStaminaChanged` event to update the bar smoothly with `Mathf.Lerp`. | 2 hours |
| S3-HH-05 | Temperature Stat Tracking | Add `temperature` float to `PlayerStats`. Expose it to the HUD. Provide public methods `ApplyHeat(float amount)` and `ApplyCold(float amount)` that Khizar's environmental zones will call. | 1.5 hours |
| S3-HH-06 | Player Profile Foundation | Create `PlayerProfile.cs` with `playerName`, `totalPlayTime`, `levelsCompleted[]`, `currentLevel`. Store and retrieve from `SaveManager`. | 1.5 hours |
| S3-HH-07 | Inventory UI Framework | Create a Canvas-based inventory panel (toggled with `Tab`). For now, display a grid of empty slots (4x5 grid). Each slot is a prefab `InventorySlot` with an icon Image and count Text. | 2 hours |

**Sprint 3 Deliverable:** Stamina system fully working (drain on sprint, regen on rest, UI bar). Temperature stat exposed and ready for environmental hooks. Inventory panel visible but empty.

---

### 5. Khizar Azhar — Heat/Cold Environmental System + Item Pickup Foundation
**Focus:** Environmental hazard zones (heat/cold) + collectible pickup system

| Task ID | Task | Description | Est. Time |
|---------|------|-------------|-----------|
| S3-KA-01 | Environmental Zone System | Create `EnvironmentalZone.cs` with a `ZoneType` enum (Normal, Hot, Cold). On `OnTriggerStay2D`, call `PlayerStats.ApplyHeat()` or `ApplyCold()` with a configurable `damagePerSecond`. | 2 hours |
| S3-KA-02 | Daytime Heat Hazard | During daytime (hookup with Ali Abdulla's `DayNightCycle`), apply heat damage automatically to the player outdoors (if no shade collider is covering them). | 2 hours |
| S3-KA-03 | Nighttime Cold Hazard | During nighttime hours, apply cold damage. Cold and heat reduce player health over time if temperature stat exceeds safe thresholds. | 1.5 hours |
| S3-KA-04 | Temperature HUD Indicator | Add a thermometer icon + text to HUD showing current temperature value. Color-code: blue (cold) → white (normal) → orange → red (hot). | 1.5 hours |
| S3-KA-05 | Collectible Item Prefab | Create a `Collectible.cs` prefab (sprite + collider). On `OnTriggerEnter2D` with player, call an `ICollectible` interface method. Types: Water, Food, Gold Coin. | 2 hours |
| S3-KA-06 | Item Pickup System | On collection, remove the item from scene, add to inventory (notify `InventoryManager`), and show a small pop-up notification ("+ Water Collected"). | 2 hours |
| S3-KA-07 | Equipment Slot Framework | In `PlayerStats`, add `equippedArmor` and `equippedWeapon` slots. Create empty `EquipmentItem` ScriptableObject with fields: name, icon, statBonus. Foundation only — no pickup logic yet. | 1.5 hours |

**Sprint 3 Deliverable:** Player takes heat/cold damage based on time of day and zone. Collectibles spawn in level, player can pick them up and they appear in inventory.

---

### 6. Rawh Hasan — Dynamic Weather System + Audio Foundation
**Focus:** Sandstorm / Wind weather events + ambient audio setup

| Task ID | Task | Description | Est. Time |
|---------|------|-------------|-----------|
| S3-RH-01 | Weather State Machine | Create `WeatherSystem.cs` with states: Clear, Windy, Sandstorm. Randomly trigger transitions after configurable time intervals. Expose event `OnWeatherChanged(WeatherState)`. | 2.5 hours |
| S3-RH-02 | Sandstorm Particle System | Create a `SandstormParticles` GameObject with a Particle System: horizontal left-to-right sand particles, high emission rate, low alpha. Activate/deactivate based on weather state. | 2 hours |
| S3-RH-03 | Sandstorm Movement Penalty | On sandstorm start, reduce player `moveSpeed` by 30%. Restore on clear. Hook into `PlayerMovement.cs` via `WeatherSystem.OnWeatherChanged` event. | 1.5 hours |
| S3-RH-04 | Visibility Reduction During Storm | During sandstorm, add a brownish/orange UI overlay (semi-transparent Image on Canvas) that increases opacity. Combine with night visibility for compounding effect. | 1.5 hours |
| S3-RH-05 | Audio Manager Setup | Create `AudioManager.cs` (singleton). Support `PlaySFX(AudioClip)`, `PlayMusic(AudioClip)`, `SetMusicVolume(float)`, `SetSFXVolume(float)`. Use two `AudioSource` components (one for music, one for SFX). | 2 hours |
| S3-RH-06 | Ambient Desert Sounds | Hook ambient wind audio to weather state. Play soft wind loop on "Clear", stronger wind on "Windy", full sandstorm audio on "Sandstorm". Crossfade between tracks using `AudioSource.volume` lerp. | 2 hours |
| S3-RH-07 | Weather HUD Indicator | Add a small weather icon to HUD (sun / wind / storm icons). Update based on `WeatherSystem` current state. | 1 hour |

**Sprint 3 Deliverable:** Sandstorm triggers randomly, particles visible, player slows down in storm. Audio manager set up; ambient sounds change with weather.

---

## Sprint 3 Summary Table

| Member | Primary Task | Supporting | Est. Hours |
|--------|-------------|-----------|------------|
| Ali Husain | Open World Hub + Portal/Teleport | Sprint Management | ~11h |
| Ali Abdulla | Day/Night Cycle + Flashlight | Save System Foundation | ~12.5h |
| Faisal Alasfoor | Difficulty System End-to-End | UI Integration | ~10.5h |
| Habib Husain | Stamina System + Player Stats | Inventory UI | ~12.5h |
| Khizar Azhar | Heat/Cold Hazards + Collectibles | Equipment Foundation | ~12.5h |
| Rawh Hasan | Weather System + Audio Manager | HUD Weather Indicator | ~12.5h |

---

## Integration Points (Cross-Member Dependencies)

| From | To | What |
|------|----|------|
| Ali Abdulla (DayNight) | Khizar (Heat/Cold) | Expose `IsDay()` / `IsNight()` method from `DayNightCycle.cs` |
| Ali Abdulla (DayNight) | Rawh (Weather) | Subscribe weather system to time events |
| Habib (PlayerStats) | Khizar (Zones) | `ApplyHeat()` / `ApplyCold()` public methods |
| Faisal (Difficulty) | Habib (PlayerStats) | `playerStartingHealth` applied on spawn |
| Rawh (AudioManager) | All | Everyone uses `AudioManager.PlaySFX()` for their systems |
| Khizar (Collectibles) | Habib (Inventory) | `InventoryManager.AddItem()` called on pickup |
| Ali Husain (Portals) | Ali Abdulla (Saving) | Portal triggers `SaveManager.SaveGame()` on level enter |

---

## Definition of Done (Sprint 3)

- [ ] Day/Night cycle visible in-game with dynamic lighting
- [ ] Stamina depletes when sprinting, regenerates when resting
- [ ] Player takes damage in hot/cold conditions
- [ ] Sandstorm spawns randomly with particles and audio
- [ ] Collectibles can be picked up and shown in inventory UI
- [ ] Difficulty selection works and scales enemy stats
- [ ] Open World Hub scene exists with at least one working portal
- [ ] All work pushed to `develop` branch on Git
- [ ] Trello Sprint 3 cards updated with results

---

*Prepared by: Ali Husain (Scrum Master) | Date: April 8, 2026 | Sprint starts: April 22, 2026*
