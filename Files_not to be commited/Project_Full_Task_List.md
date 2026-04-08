# Deserted Echoes — Full Project Task List
## IT8101 Games Development | Group 3
**Version:** 1.0 | **Created:** April 8, 2026
**This document lists ALL suggested tasks across the entire project. Use as a reference backlog. Not all tasks may be completed in every sprint.**

---

## How to Read This Document

Each task has:
- **Task ID** — Unique identifier (System-Number format)
- **Task Name** — Short title
- **Description** — Exactly what needs to be done, technical details included
- **Priority** — P1 (Critical), P2 (Important), P3 (Nice to have)
- **GDD Owner** — The team member assigned this feature in the GDD
- **Suggested Sprint** — Recommended sprint to target

---

## CATEGORY 1: PROJECT SETUP & INFRASTRUCTURE

| Task ID | Task Name | Description | Priority | Owner | Sprint |
|---------|-----------|-------------|----------|-------|--------|
| SETUP-01 | Unity Project Initialization | Create Unity 2D project (URP pipeline). Set up folder structure: Assets/Scripts, Assets/Scenes, Assets/Prefabs, Assets/UI, Assets/Sprites, Assets/Audio, Assets/ScriptableObjects. Define naming conventions (PascalCase scripts, kebab-case scenes). | P1 | Ali Husain | 1 |
| SETUP-02 | Version Control Setup | Initialize Git repository. Create `main` and `develop` branches. Add `.gitignore` for Unity. All members clone and confirm access. | P1 | Ali Husain | 1 |
| SETUP-03 | Trello Board Population | Populate Trello SCRUM board: Key Features list, Backlog with all tasks, Sprint 1–8 skeleton lists. Invite all members and tutor. | P1 | Ali Husain | 1 |
| SETUP-04 | Package Manager Setup | Import required Unity packages: Cinemachine, TextMeshPro, Input System (new), Universal RP, 2D Lights, Particle System. | P1 | Ali Husain | 1 |
| SETUP-05 | Coding Standards Document | Create `CODING_STANDARDS.md` inside repo: naming conventions, folder rules, commenting style, prefab naming. | P2 | Ali Husain | 1 |
| SETUP-06 | Scene Architecture Plan | Plan scene list: MainMenu, OpenWorldHub, Level_01 through Level_12, GameOver, Victory. Create empty scenes with correct names. | P1 | Ali Husain | 1 |

---

## CATEGORY 2: PLAYER MOVEMENT SYSTEM

| Task ID | Task Name | Description | Priority | Owner | Sprint |
|---------|-----------|-------------|----------|-------|--------|
| MOVE-01 | Basic WASD Movement | Implement `PlayerMovement.cs` using `Rigidbody2D`. WASD/arrow keys move the player at `moveSpeed` (configurable). Normalize diagonal input to prevent speed boost. | P1 | Ali Abdulla | 1 |
| MOVE-02 | Sprint Mechanic | Hold `Shift` to sprint: increase `moveSpeed` by `sprintMultiplier` while stamina > 0. Decrease stamina during sprint. Revert to walk speed when stamina = 0 or shift released. | P1 | Habib | 3 |
| MOVE-03 | Dodge / Roll Mechanic | Press `Space` to trigger a dodge roll in current movement direction. Brief invincibility frames during roll (`isDodging` flag). Short cooldown (0.5s). | P2 | Ali Abdulla | 4 |
| MOVE-04 | Player Facing Direction | Flip player sprite based on movement direction (left/right). For top-down, rotate sprite to face movement direction using `Quaternion.LookRotation`. | P1 | Ali Abdulla | 1 |
| MOVE-05 | Movement Animation States | Animator Controller with states: Idle, Walk, Run, Roll, Attack, Death. Transition using `speed` float and `isAttacking` / `isDead` booleans. | P1 | Ali Abdulla | 2 |
| MOVE-06 | Ground/Terrain Collision | Configure player collider (Capsule or Box). Ensure no clipping through walls or terrain. Layer mask for `Ground` layer. | P1 | Ali Abdulla | 1 |
| MOVE-07 | Input System Integration | Migrate to Unity's new Input System. Create `PlayerInputActions` asset with action maps: Gameplay, UI. Bind WASD, Shift, Space, E, Tab, M, ESC, 1-4. | P2 | Ali Abdulla | 2 |

---

## CATEGORY 3: COMBAT SYSTEM

| Task ID | Task Name | Description | Priority | Owner | Sprint |
|---------|-----------|-------------|----------|-------|--------|
| COMBAT-01 | Basic Melee Attack | Left-click triggers melee attack animation. Create `AttackHitbox` child GameObject with Collider2D (disabled by default). Enable during attack animation frame, detect enemies in range using `Physics2D.OverlapCircle`. | P1 | Ali Abdulla | 2 |
| COMBAT-02 | Attack Damage System | `DamageDealer.cs`: define `attackDamage` (float). On hit, call `target.TakeDamage(attackDamage)`. Apply difficulty multiplier if applicable. | P1 | Ali Abdulla | 2 |
| COMBAT-03 | Player Health System | `PlayerHealth.cs`: `currentHealth`, `maxHealth`. `TakeDamage(float amount)` reduces health. `Heal(float amount)` increases it (capped at max). Fire `onHealthChanged` UnityEvent. | P1 | Habib | 2 |
| COMBAT-04 | Health Bar UI | Canvas > HUD > HealthBar (UI Slider or Image fillAmount). Subscribe to `onHealthChanged` event. Animate with `Mathf.Lerp` for smooth change. Show numerical value (e.g., "85/100"). | P1 | Faisal | 2 |
| COMBAT-05 | Knockback Effect | When player/enemy takes damage, apply brief knockback force using `Rigidbody2D.AddForce` in the opposite direction of attacker. Configure `knockbackForce` and `knockbackDuration`. | P2 | Ali Abdulla | 2 |
| COMBAT-06 | Hit Flash Effect | When player/enemy takes damage, flash the sprite red for 0.1s using `SpriteRenderer.color`. Create `HitFlash.cs` coroutine. | P2 | Khizar | 3 |
| COMBAT-07 | Death System | When `currentHealth <= 0`, set `isDead = true`, trigger Death animation, disable player input, show Game Over screen after `deathDelay`. | P1 | Habib | 2 |
| COMBAT-08 | Respawn / Lives System | On death, decrement lives count. If lives > 0, reload current level after 2s delay. If lives = 0, show full Game Over screen. Start with 3 lives (configurable). | P1 | Habib | 3 |
| COMBAT-09 | Secondary Attack / Ranged | Right-click fires a projectile (e.g., rock throw or equipped ranged weapon). Instantiate `ProjectilePrefab`, set velocity toward mouse position. Destroy on impact or after `maxRange`. | P2 | Ali Abdulla | 5 |
| COMBAT-10 | Combat Sound Effects | Play attack SFX on swing, hit SFX on contact, death SFX on death. Use `AudioManager.PlaySFX()`. | P2 | Rawh | 3 |

---

## CATEGORY 4: ENEMY AI SYSTEM

| Task ID | Task Name | Description | Priority | Owner | Sprint |
|---------|-----------|-------------|----------|-------|--------|
| AI-01 | Enemy Base Class | `EnemyBase.cs`: abstract class with `health`, `damage`, `moveSpeed`, `detectionRange`, `attackRange`. Virtual `TakeDamage()`, `Attack()`, `Die()` methods. | P1 | Ali Husain | 2 |
| AI-02 | Patrol State | Enemy walks between `PatrolPoints[]` array (Vector2 waypoints). Moves to each point, waits `patrolWaitTime`, then moves to next. Loop indefinitely. | P1 | Ali Husain | 2 |
| AI-03 | Player Detection (Chase State) | `Physics2D.OverlapCircle(transform.position, detectionRange)` each frame to check for player on `Player` layer. If detected, switch to Chase state. | P1 | Ali Husain | 2 |
| AI-04 | Chase State | Move toward player position using `Vector2.MoveTowards` at `chaseSpeed`. Continue chasing until player leaves `loseAggroRange` distance. | P1 | Ali Husain | 2 |
| AI-05 | Attack State | When within `attackRange`, stop moving and trigger attack animation at `attackCooldown` intervals. Deal damage to player via `PlayerHealth.TakeDamage()`. | P1 | Ali Husain | 2 |
| AI-06 | Enemy Health System | Each enemy has `currentHealth`. `TakeDamage(float)` reduces it. Play hit flash. When health = 0, call `Die()` → play death animation → spawn loot → destroy after delay. | P1 | Ali Husain | 2 |
| AI-07 | Skeleton Enemy | Implement Skeleton using `EnemyBase`. Easy: fast attack speed, low health. Normal: armored (higher defense). Hard: golden armor, high crit chance. Located in Dungeons/Caves. | P2 | Ali Husain | 4 |
| AI-08 | Goblin Enemy | Goblins attack in groups. Implement group spawning: `GoblinSpawner.cs` spawns 2-5 goblins within radius. Fast movement speed. | P2 | Ali Husain | 4 |
| AI-09 | Zombie Enemy | Slow but durable. High health pool. Hard version sprints/lunges and spits acid (projectile). Location: wilderness. | P2 | Ali Husain | 5 |
| AI-10 | Orc Enemy | Heavy hitter, knockback on attack. Slow movement. Hard version has armor-based damage reduction. | P2 | Ali Husain | 5 |
| AI-11 | Rat Enemy (Swarm) | Easy rats swarm (multiple spawn on trigger). Normal: venom spit projectile. Hard: lunge + infection debuff (damage over time). | P3 | Ali Husain | 5 |
| AI-12 | Imp Enemy (Flying) | Flies (no ground collision). Dives at player. Hard version shoots fireballs. Implement separate `FlyingEnemy` class ignoring terrain colliders. | P3 | Ali Husain | 5 |
| AI-13 | Mini Boss: Vampire | Multi-phase boss. Phase 1: Fly + dive. Phase 2 (50% health): summons 3 bats. Phase 3 (25%): blood magic projectiles. Health bar UI. | P2 | Ali Husain | 6 |
| AI-14 | Mini Boss: The Eye | Flying boss. Shoots laser beams (raycast). On Normal: tentacle melee combo. On Hard: fireballs. Multi-attack pattern. | P2 | Ali Husain | 6 |
| AI-15 | Final Boss System | Boss with 3 distinct phases. Each phase changes attack patterns. Health bar with phase markers. Cutscene trigger on death. | P1 | Ali Husain | 8 |
| AI-16 | Enemy Difficulty Scaling | Hook all `EnemyBase` instances to `DifficultySettings.enemyHealthMultiplier` and `enemyDamageMultiplier` on `Awake()`. | P1 | Faisal | 3 |

---

## CATEGORY 5: SURVIVAL SYSTEM

| Task ID | Task Name | Description | Priority | Owner | Sprint |
|---------|-----------|-------------|----------|-------|--------|
| SURV-01 | Player Stats Manager | `PlayerStats.cs` singleton: holds `health`, `stamina`, `temperature`, `hunger`, `thirst`. Methods for getting/setting each. Fire change events. | P1 | Habib | 3 |
| SURV-02 | Stamina System | Stamina (0–100). Drains at `staminaDrainRate` when sprinting. Regenerates at `staminaRegenRate` after `staminaRegenDelay` seconds of not sprinting. Block sprint at 0 stamina. | P1 | Habib | 3 |
| SURV-03 | Temperature System | `temperature` float (0–100). Values below 20 = Cold zone, above 80 = Hot zone. Cause health drain when in extreme ranges. Rate: `tempDamagePerSecond`. | P1 | Khizar | 3 |
| SURV-04 | Heat System (Daytime) | During day hours (06:00–20:00), gradually increase player temperature. Rate increases with proximity to `HotZone` triggers. Shade colliders (marked `ShadeZone`) prevent heating. | P1 | Khizar | 3 |
| SURV-05 | Cold System (Nighttime) | During night hours (20:00–06:00), gradually decrease player temperature. Campfire triggers nearby (`WarmZone`) prevent cooling. Cold zone triggers accelerate cooling. | P1 | Khizar | 3 |
| SURV-06 | Hunger System | `hunger` float (0–100). Drains at 0.5 per in-game hour. Below 20 hunger: apply slow health drain. Eating food items restores hunger. | P3 | Habib | 6 |
| SURV-07 | Thirst System | `thirst` float (0–100). Drains faster than hunger (1.0 per in-game hour). Below 20 thirst: apply slow health drain + movement speed penalty. Drinking water restores thirst. | P3 | Habib | 6 |
| SURV-08 | Temperature UI | HUD thermometer icon + numeric display. Color gradient: blue (<20) → white (20–80) → red (>80). | P1 | Khizar | 3 |
| SURV-09 | Survival HUD (All Stats) | Organize HUD to clearly show: Health bar, Stamina bar, Temperature, (later) Hunger/Thirst. Use icons for visual clarity. | P1 | Faisal | 4 |

---

## CATEGORY 6: DAY/NIGHT CYCLE & DYNAMIC VISIBILITY (ADVANCED)

| Task ID | Task Name | Description | Priority | Owner | Sprint |
|---------|-----------|-------------|----------|-------|--------|
| DN-01 | Day/Night Time System | `DayNightCycle.cs`: float `timeOfDay` (0–24). Increment with `Time.deltaTime * timeSpeed`. Fire events: `OnDayStart` (timeOfDay ≥ 6), `OnNightStart` (timeOfDay ≥ 20). Loop back to 0 at 24. | P1 | Ali Abdulla | 3 |
| DN-02 | Dynamic Lighting (Directional) | Drive `DirectionalLight` rotation and color from `timeOfDay`. Use `Gradient` (Serializable) for sky color: dawn (peach) → noon (bright white) → dusk (orange) → night (deep blue/black). | P1 | Ali Abdulla | 3 |
| DN-03 | Night Visibility Reduction | At night, overlay a dark radial vignette (Canvas Image) that fades in. Alpha: 0 at noon → 0.85 at midnight. Lerp smoothly using `Mathf.InverseLerp`. | P1 | Ali Abdulla | 3 |
| DN-04 | Flashlight System | `Flashlight.cs` child of player. Unity 2D Light (Point or Spot) activated at night. Toggle with `F` key. Flashlight has `batteryLife` that drains over time. | P1 | Ali Abdulla | 3 |
| DN-05 | Campfire Light Source | Campfire prefab with flickering 2D Point Light. Placed in levels as warmth + light source. Player within campfire range is in `WarmZone` (prevents cold damage). | P2 | Ali Abdulla | 4 |
| DN-06 | Day/Night HUD Indicator | Small sun/moon sprite that transitions based on time. Show in-game clock text (e.g., "18:45"). | P1 | Ali Abdulla | 3 |
| DN-07 | Enemy Behavior at Night | At night, increase enemy `detectionRange` by 20% (harder to avoid). Option: spawn extra enemy patrols. | P2 | Ali Husain | 5 |

---

## CATEGORY 7: WEATHER SYSTEM

| Task ID | Task Name | Description | Priority | Owner | Sprint |
|---------|-----------|-------------|----------|-------|--------|
| WTHR-01 | Weather State Machine | `WeatherSystem.cs` with enum `WeatherState { Clear, Windy, Sandstorm }`. Random transitions after `[minTime, maxTime]` seconds. Fire `OnWeatherChanged(WeatherState)` event. | P1 | Rawh | 3 |
| WTHR-02 | Sandstorm Particles | Particle System: horizontal sand/dust particles, high emission, low alpha. Spawn from left edge, move right. Activate on Sandstorm state. Scale particle density with storm intensity. | P1 | Rawh | 3 |
| WTHR-03 | Sandstorm Movement Penalty | Subscribe to `OnWeatherChanged`. In Sandstorm: reduce `PlayerMovement.moveSpeed` by 30%. In Clear: restore speed. | P1 | Rawh | 3 |
| WTHR-04 | Sandstorm Visibility Overlay | Semi-transparent brownish UI Image overlay. Alpha: 0 (Clear) → 0.4 (Windy) → 0.7 (Sandstorm). Lerp transition. Compounds with night visibility. | P1 | Rawh | 3 |
| WTHR-05 | Wind Visual Effect | In Windy state: spawn fewer/lighter particles. Vegetation/objects sway (if animated props are used). | P2 | Rawh | 4 |
| WTHR-06 | Weather-Based Temperature | Sandstorm: slightly increase heat (desert heat trapped). Wind at night: accelerate cold. Hook into temperature system. | P2 | Rawh | 4 |
| WTHR-07 | Weather HUD Icon | Small icon (sun / wind / storm cloud) in HUD corner. Updates via `OnWeatherChanged` event. | P1 | Rawh | 3 |

---

## CATEGORY 8: INVENTORY & EQUIPMENT SYSTEM

| Task ID | Task Name | Description | Priority | Owner | Sprint |
|---------|-----------|-------------|----------|-------|--------|
| INV-01 | Item ScriptableObject | `ItemData.cs` (ScriptableObject): `itemName`, `itemIcon` (Sprite), `itemType` (enum: Consumable, Equipment, Resource, Key), `description`, `stackable` (bool), `maxStack` (int). | P1 | Khizar | 4 |
| INV-02 | Inventory Manager | `InventoryManager.cs` (singleton): `List<ItemSlot> slots` (ItemSlot = ItemData + quantity). `AddItem(ItemData)`, `RemoveItem(ItemData)`, `HasItem(ItemData)`. Fire `OnInventoryChanged`. | P1 | Habib | 4 |
| INV-03 | Inventory UI Panel | Canvas panel (Tab to toggle). 4×5 grid of `InventorySlot` prefabs. Each slot: Icon Image + Count Text. Subscribe to `OnInventoryChanged` to refresh UI. | P1 | Habib | 4 |
| INV-04 | Item Tooltip | Hover over slot → show tooltip popup with item name, description, stats. Position tooltip near cursor but ensure it stays within screen bounds. | P2 | Habib | 5 |
| INV-05 | Consumable Use | Right-click a consumable in inventory → use it. Water: restores thirst. Food: restores hunger. Health Potion: restores health. Remove 1 from stack after use. | P2 | Habib | 5 |
| INV-06 | Equipment System | `EquipmentManager.cs`: slots for Head, Body, Weapon, Accessory. Equipping applies stat bonuses from `ItemData.statBonus`. Unequipping removes bonuses. | P2 | Khizar | 4 |
| INV-07 | Equipment UI Panel | Separate panel showing equipment slots as illustrated silhouette. Click slot to open inventory filtered for that equipment type. | P2 | Khizar | 5 |
| INV-08 | Armor Defense System | Equipped armor reduces damage taken by `armorDefense` flat amount or percentage. Apply in `PlayerHealth.TakeDamage()`. | P2 | Khizar | 5 |
| INV-09 | Weapon Switching (1-4 keys) | Item slots 1–4 mapped to quick-use bar. Press key to equip/use the item in that slot instantly. Show quick-bar as persistent HUD element. | P2 | Ali Abdulla | 5 |

---

## CATEGORY 9: COLLECTIBLE & LOOT SYSTEM

| Task ID | Task Name | Description | Priority | Owner | Sprint |
|---------|-----------|-------------|----------|-------|--------|
| COLL-01 | Collectible Prefab | `Collectible.cs`: Sprite + CircleCollider2D (IsTrigger). On `OnTriggerEnter2D` with player tag, call `InventoryManager.AddItem(itemData)`, show pickup notification, destroy self. | P1 | Khizar | 3 |
| COLL-02 | Resource Collectibles | Create ItemData assets: Desert Water Canteen, Dried Meat, Gold Coin, Bandage. Place as collectibles in levels. | P1 | Khizar | 4 |
| COLL-03 | Pickup Notification UI | Small pop-up (bottom of screen or near player): "+ Desert Water Canteen". Fades out after 1.5s. Queue multiple pickups. | P2 | Khizar | 4 |
| COLL-04 | Loot System (Enemy Drops) | On enemy `Die()`, roll loot table (`LootTable.cs`): array of `LootEntry { ItemData item, float dropChance }`. Randomly instantiate 0–3 items at enemy position. | P2 | Khizar | 5 |
| COLL-05 | Chest System | `Chest.cs`: interact with `E` key. Opens animation, spawns loot. One-time open (save chest state). Chests have `lootTable` reference. | P2 | Khizar | 6 |
| COLL-06 | Currency System (Gold) | `CurrencyManager.cs`: `int gold`. `AddGold(int)`, `SpendGold(int)` (returns bool if sufficient funds). Display gold amount in HUD. | P2 | Khizar | 5 |

---

## CATEGORY 10: XP & LEVELING SYSTEM

| Task ID | Task Name | Description | Priority | Owner | Sprint |
|---------|-----------|-------------|----------|-------|--------|
| XP-01 | XP System | `XPManager.cs`: `currentXP`, `xpToNextLevel`. On enemy kill, award XP based on enemy type/difficulty. Fire `OnXPGained(int)` and `OnLevelUp(int newLevel)` events. | P2 | Habib | 5 |
| XP-02 | Level Up Effect | On level up: show "Level Up!" overlay animation, restore some health/stamina, unlock stat increase. | P2 | Habib | 5 |
| XP-03 | Stat Upgrades on Level Up | Each level up grants stat points. Player allocates to: MaxHealth +10, MaxStamina +15, AttackDamage +2, MoveSpeed +0.1. Simple UI panel. | P3 | Habib | 6 |
| XP-04 | XP HUD Bar | Progress bar showing XP progress toward next level. Label "LVL X". | P2 | Faisal | 6 |

---

## CATEGORY 11: TELEPORT / FAST TRAVEL SYSTEM

| Task ID | Task Name | Description | Priority | Owner | Sprint |
|---------|-----------|-------------|----------|-------|--------|
| TP-01 | Open World Hub Scene | Create `OpenWorldHub` scene with 6 tunnel entrances, desert environment layout, ambient lighting, spawn point. | P1 | Ali Husain | 3 |
| TP-02 | Level Transition System | `LevelTransition.cs`: `OnTriggerEnter2D` detects player, fades screen to black, loads target scene by name using `SceneManager.LoadScene`. Fade-in on arrival. | P1 | Ali Husain | 3 |
| TP-03 | Portal Door Prefab | Glowing portal prefab (sprite animation or shader). Shows target level name on hover. Has `LevelTransition.cs` attached with `targetScene` field. | P1 | Ali Husain | 3 |
| TP-04 | Return Portal | At the end of each level, a return portal brings player back to `OpenWorldHub`. Trigger save on return. | P1 | Ali Husain | 4 |
| TP-05 | Level Completion Marker | After completing a level (defeating boss or reaching exit), mark it as "Done" in a persistent `LevelProgressManager`. Show visual indicator on Hub portal (green glow + "Done" text). | P2 | Ali Husain | 5 |
| TP-06 | Fast Travel Map (M key) | Press `M` to open a map showing the Open World Hub with all 6 tunnels/12 levels. Click a completed level to fast-travel. | P2 | Ali Husain | 6 |
| TP-07 | Scene Fade Transition | Reusable `SceneFader.cs` singleton: fade canvas from black → transparent on scene load, transparent → black on scene unload. Configurable `fadeDuration`. | P1 | Ali Husain | 3 |

---

## CATEGORY 12: DIFFICULTY SYSTEM

| Task ID | Task Name | Description | Priority | Owner | Sprint |
|---------|-----------|-------------|----------|-------|--------|
| DIFF-01 | Difficulty ScriptableObject | `DifficultySettings.cs`: `enemyHealthMult`, `enemyDamageMult`, `playerStartHealth`, `resourceSpawnRate`, `staminaDrainMult`, `environmentDamageMult`. Create 3 assets: Easy, Normal, Hard. | P1 | Faisal | 3 |
| DIFF-02 | Difficulty Selection UI | Main Menu "Select Difficulty" panel: 3 buttons with descriptions. Selection persists in `GameManager.activeDifficulty`. | P1 | Faisal | 3 |
| DIFF-03 | Enemy Stat Scaling | In `EnemyBase.Awake()`: multiply `maxHealth` and `damage` by `GameManager.activeDifficulty` multipliers. | P1 | Faisal | 3 |
| DIFF-04 | Player Starting Stats by Difficulty | Easy: 150 HP. Normal: 100 HP. Hard: 75 HP. Apply in `PlayerStats.Start()` from `activeDifficulty.playerStartHealth`. | P1 | Faisal | 3 |
| DIFF-05 | Resource Spawn Rate Scaling | Easy: more collectibles. Hard: fewer. Use `resourceSpawnRate` multiplier in level spawner scripts. | P2 | Faisal | 4 |
| DIFF-06 | Difficulty HUD Badge | Small text or icon in corner: "EASY" (green), "NORMAL" (yellow), "HARD" (red). | P1 | Faisal | 3 |
| DIFF-07 | Mid-Game Difficulty Change | Allow player to change difficulty from Pause Menu (with warning: "This will reset enemy stats"). | P3 | Faisal | 7 |

---

## CATEGORY 13: SAVE / LOAD SYSTEM

| Task ID | Task Name | Description | Priority | Owner | Sprint |
|---------|-----------|-------------|----------|-------|--------|
| SAVE-01 | SaveData Class | `[System.Serializable] SaveData`: `playerPosition`, `currentHealth`, `currentStamina`, `inventoryItems[]`, `equippedItems[]`, `currentLevel`, `levelsCompleted[]`, `gold`, `xp`, `playerLevel`, `difficulty`. | P1 | Ali Abdulla | 3 |
| SAVE-02 | SaveManager | `SaveManager.cs` singleton: `SaveGame()` serializes `SaveData` to JSON via `JsonUtility.ToJson()`, writes to `Application.persistentDataPath/save.json`. `LoadGame()` reads and deserializes. | P1 | Ali Abdulla | 3 |
| SAVE-03 | Auto-Save at Checkpoints | `Checkpoint.cs`: on player trigger, call `SaveManager.SaveGame()`. Show "Game Saved" HUD notification for 2s. | P1 | Ali Abdulla | 4 |
| SAVE-04 | Manual Save/Load in Pause | Pause Menu: "Save Game" and "Load Game" buttons. Confirm dialog on Load (warn about overwriting progress). | P2 | Ali Abdulla | 5 |
| SAVE-05 | Multiple Save Slots | Support 3 save slots. `SaveManager` writes to `save_slot1.json`, `save_slot2.json`, `save_slot3.json`. Save slot selection UI on Main Menu. | P3 | Ali Abdulla | 7 |
| SAVE-06 | Save Data Validation | On load, validate save file version and data integrity. If corrupted, show error and offer to reset. | P2 | Ali Abdulla | 7 |

---

## CATEGORY 14: PLAYER PROFILE SYSTEM

| Task ID | Task Name | Description | Priority | Owner | Sprint |
|---------|-----------|-------------|----------|-------|--------|
| PROF-01 | Player Profile Data | `PlayerProfile.cs`: `playerName`, `totalPlaytime`, `enemiesKilled`, `levelsCompleted`, `deathCount`, `itemsCollected`. Persisted with SaveData. | P2 | Habib | 4 |
| PROF-02 | Stats Tracking | Increment `enemiesKilled` on each enemy death. Increment `deathCount` on player death. Track `totalPlaytime` using `Time.time`. | P2 | Habib | 4 |
| PROF-03 | Profile UI Screen | Accessible from Main Menu. Shows player name, play stats, levels completed. | P3 | Habib | 7 |

---

## CATEGORY 15: LEVEL DESIGN

| Task ID | Task Name | Description | Priority | Owner | Sprint |
|---------|-----------|-------------|----------|-------|--------|
| LVL-01 | Open World Hub Design | Desert hub with 6 tunnels. Each tunnel entrance links to 2 levels. Include campfire, NPC placeholder, ambient props. | P1 | Ali Husain | 3 |
| LVL-02 | Level 1 Design | Tutorial level. Desert plains. Introduce WASD movement, basic collectibles, first enemy encounter (Goblin Easy). Exit portal to hub. | P1 | All | 4 |
| LVL-03 | Level 2 Design | Desert ruins. Introduce day/night timing challenge. More enemies, first chest. | P1 | All | 4 |
| LVL-04 | Level 3 Design | Bandit camp. Introduce heat hazard zones. Mixed enemy types. First mini-boss hint. | P1 | All | 5 |
| LVL-05 | Level 4 Design | Desert caves entrance. Introduce Skeleton and Orc enemies. Dark lighting. | P1 | All | 5 |
| LVL-06 | Level 5 Design | Deep Dungeon. First mini-boss (Vampire - Easy). Puzzle elements. | P1 | All | 5 |
| LVL-07 | Levels 6–10 Design | Mid-game levels: increasing difficulty, introduce hard enemy variants, environmental hazard combos (storm + night). | P2 | All | 6 |
| LVL-08 | Level 11 Design | Pre-final boss level. Heavy enemy gauntlet. | P2 | All | 7 |
| LVL-09 | Level 12 Design | Final Boss Arena. Cutscene trigger, boss fight, victory screen trigger on defeat. | P1 | Ali Husain | 7 |
| LVL-10 | Checkpoint Placement | Place checkpoint objects in each level at strategic points (mid-level, before boss). | P1 | Ali Husain | 4 |

---

## CATEGORY 16: UI SYSTEM

| Task ID | Task Name | Description | Priority | Owner | Sprint |
|---------|-----------|-------------|----------|-------|--------|
| UI-01 | Main Menu Screen | Canvas: Game Title, Start Game, Instructions, Credits, Settings, Quit buttons. Background: desert scene or artwork. | P1 | Faisal | 1 |
| UI-02 | HUD Layout | Persistent gameplay HUD: health bar, stamina bar, temperature, gold count, XP bar, day/night indicator, weather icon, difficulty badge, quick-item slots 1–4. | P1 | Faisal | 2 |
| UI-03 | Pause Menu | ESC to pause: Resume, Save Game, Load Game, Settings, Quit to Main Menu. `Time.timeScale = 0` on open, 1 on close. | P1 | Faisal | 4 |
| UI-04 | Settings Menu | Audio: Master, Music, SFX volume sliders. Graphics: Resolution dropdown, Quality dropdown. Controls: Key binding display. | P2 | Ali Abdulla | 5 |
| UI-05 | Instructions Screen | From Main Menu. Show control scheme with icons (WASD, Shift, Space, etc.) and brief game description. | P2 | Faisal | 4 |
| UI-06 | Credits Screen | Scrolling credits list: all 6 team member names, IDs, roles. | P2 | Faisal | 7 |
| UI-07 | Game Over Screen | Show on death with 0 lives. "You Died" title, death count, "Retry Level" and "Main Menu" buttons. Desert themed art. | P1 | Faisal | 3 |
| UI-08 | Victory Screen | Show on completing final level. "You Win!" title, total play time, enemies killed, "Main Menu" button. | P1 | Faisal | 7 |
| UI-09 | Level Complete Screen | Brief overlay on level completion: "Level X Complete!", XP gained, items collected. Auto-transitions to hub after 3s. | P2 | Faisal | 5 |

---

## CATEGORY 17: AUDIO SYSTEM

| Task ID | Task Name | Description | Priority | Owner | Sprint |
|---------|-----------|-------------|----------|-------|--------|
| AUDIO-01 | Audio Manager | `AudioManager.cs` singleton: two AudioSources (music, SFX). `PlayMusic(AudioClip, bool loop)`, `PlaySFX(AudioClip)`, `SetMusicVolume(float)`, `SetSFXVolume(float)`. Settings save to PlayerPrefs. | P1 | Rawh | 3 |
| AUDIO-02 | Background Music | Calm exploration track (day), tense ambient (night), intense combat music (enemy detected). Use `AudioManager.PlayMusic()`. Crossfade between tracks. | P1 | Rawh | 4 |
| AUDIO-03 | Ambient Desert Sound | Wind loop (clear weather), stronger wind (windy), sandstorm audio. Layered ambient sounds. | P1 | Rawh | 3 |
| AUDIO-04 | Combat SFX | Sword swing, hit impact, enemy death, player hurt, block. Attach to animation events or call from scripts. | P2 | Rawh | 4 |
| AUDIO-05 | UI SFX | Button hover, button click, inventory open/close, pickup jingle, level complete fanfare. | P2 | Rawh | 4 |
| AUDIO-06 | Enemy SFX | Enemy alert sound (player detected), enemy attack, enemy death. Different per enemy type if resources allow. | P2 | Rawh | 5 |
| AUDIO-07 | Environmental SFX | Campfire crackle, portal hum, chest open, day/night transition chime. | P2 | Rawh | 5 |
| AUDIO-08 | Boss Music | Unique boss track per mini-boss and final boss. Trigger on boss aggro. | P2 | Rawh | 6 |

---

## CATEGORY 18: SPECIAL EFFECTS & POLISH

| Task ID | Task Name | Description | Priority | Owner | Sprint |
|---------|-----------|-------------|----------|-------|--------|
| FX-01 | Sandstorm Particle System | Full sandstorm particle effect (see WTHR-02). Wind streak particles. | P1 | Rawh | 3 |
| FX-02 | Hit Impact Particles | Small dust/blood particle burst on hit. `HitEffect.cs` instantiates prefab at hit position, auto-destroys after 0.5s. | P2 | Khizar | 4 |
| FX-03 | Death Particles | On enemy death: brief particle burst (dust/ash cloud). | P2 | Khizar | 5 |
| FX-04 | Campfire Particle | Fire particle system at campfire location. Flickering warm light (2D Light). | P2 | Ali Abdulla | 4 |
| FX-05 | Portal Glow Effect | Portal prefab with swirling particle effect or animated sprite loop. | P2 | Ali Husain | 4 |
| FX-06 | Level Up Flash | Bright flash + sparkle particles when player levels up. | P3 | Habib | 6 |
| FX-07 | Post-Processing | Universal RP: Bloom (lantern/fire glow), Color Grading (warm day, cool night), Vignette (edges of screen for tension). | P2 | All | 7 |

---

## CATEGORY 19: EXPORT & TESTING

| Task ID | Task Name | Description | Priority | Owner | Sprint |
|---------|-----------|-------------|----------|-------|--------|
| TEST-01 | Playtest Sprint 1–3 Systems | Team-wide playtest of movement, combat, survival systems. Document bugs in Trello "Bug" list. | P1 | All | 4 |
| TEST-02 | Playtest All Levels | Full run-through of all 12 levels. Check for out-of-bounds, stuck enemies, broken spawners. | P1 | All | 7 |
| TEST-03 | PC Build Export | Build → PC, Mac & Linux Standalone. Target: Windows. `game.exe` in `/Build` folder. Test the build (not editor). | P1 | Ali Husain | 8 |
| TEST-04 | WebGL Build (Optional) | Build for WebGL as secondary platform if time allows. | P3 | Ali Husain | 8 |
| TEST-05 | Bug Fix Sprint | Dedicated sprint/time for fixing all known bugs before submission. | P1 | All | 8 |
| TEST-06 | Performance Optimization | Profile in Unity Profiler. Target 60 FPS. Reduce overdraw, optimize particle systems, use object pooling for projectiles/enemies. | P2 | All | 8 |
| TEST-07 | SCRUM Logbook Completion | Ensure all sprint logs are filled and submitted on Trello. Each sprint log: Feature ID, Feature, Member Responsible, Est Time, End of Sprint Result. | P1 | Ali Husain | 8 |

---

## Sprint Roadmap Summary

| Sprint | Week | Theme | Key Deliverables |
|--------|------|-------|-----------------|
| Sprint 1 | Apr 5–12 | Foundation | Unity setup, player movement, camera, basic UI, test scene |
| Sprint 2 | Apr 13–19 | Core Combat | Combat system, Enemy AI, Health system, Death system |
| Sprint 3 | Apr 20–26 | Survival & Environment | Day/Night, stamina, heat/cold, weather, portals, difficulty |
| Sprint 4 | Apr 27–May 3 | Inventory & Level Design | Full inventory, equipment, levels 1–4, checkpoints, save/load |
| Sprint 5 | May 4–10 | Enemies & Loot | All enemy types, loot system, XP system, currency |
| Sprint 6 | May 11–14 | Mid-Bosses & Content | Mini-bosses, levels 5–10, chest system, more levels |
| Sprint 7 | May 15–19 | Polish & Advanced UI | Map system, difficulty tweaks, credits, UI polish, FX |
| Sprint 8 | May 20–24 | Final Boss & Submission | Final boss, full test, build export, SCRUM logbook final |

---

*Document prepared by: Ali Husain (Scrum Master) | Deserted Echoes | Group 3 | IT8101*
