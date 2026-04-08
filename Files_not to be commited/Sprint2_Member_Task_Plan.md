# Sprint 2 – Member Task Plan
## Deserted Echoes | IT8101 Games Development
**Sprint Duration:** April 15 – 21, 2026
**Sprint Theme:** Core Combat, Enemy AI & Player Health Systems
**Sprint Goal:** The player can fight. Enemies exist in the world — they patrol, detect the player, chase, and attack. The player has a health system, can be killed, and the death/respawn flow works from end to end.

---

## Prerequisite (Sprint 1 Must Be Done First)

> The following Sprint 1 deliverables are **required** for Sprint 2 to work:
> - Player prefab on scene (Habib)
> - Player movement working (Ali Abdulla)
> - Animation states exist in Animator Controller (Habib)
> - AudioManager singleton live (Rawh)
> - Test level scene with spawn point (Rawh)
> - GameManager singleton (Ali Husain)

---

## Member Task Breakdown

---

### 1. Ali Husain Ali Alsaffar — Enemy AI System (Core)
**Focus:** Build the full enemy AI state machine — the brain that makes enemies feel alive. This is the foundation for all future enemy types.

| Task ID | Task | Description | Est. Time |
|---------|------|-------------|-----------|
| S2-AH-01 | EnemyBase Abstract Class | Create `EnemyBase.cs` (abstract class, not MonoBehaviour directly — inherit from MonoBehaviour). Fields: `maxHealth` (float), `currentHealth` (float), `moveSpeed` (float), `attackDamage` (float), `detectionRange` (float), `attackRange` (float), `attackCooldown` (float). Virtual/abstract methods: `TakeDamage(float amount)`, `Attack()`, `Die()`. All enemy types will inherit from this class. | 2 hrs |
| S2-AH-02 | Enemy State Machine | In `EnemyBase.cs`, define an `EnemyState` enum: `Idle`, `Patrol`, `Chase`, `Attack`, `Dead`. A `currentState` variable. In `Update()`, call the appropriate state method: `HandleIdle()`, `HandlePatrol()`, `HandleChase()`, `HandleAttack()`. Use a switch statement. | 1.5 hrs |
| S2-AH-03 | Patrol State | `HandlePatrol()`: enemy moves toward the current `patrolPoint` (from a `Transform[] patrolPoints` array). When close (< 0.2f distance), wait `patrolWaitTime` seconds, then advance to the next index (loop using modulo). Use `Vector2.MoveTowards(rb.position, target, moveSpeed * Time.deltaTime)`. | 2 hrs |
| S2-AH-04 | Player Detection (Chase) | In `Update()`, run `Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer)` every frame. If player found AND not dead: switch to `Chase` state. Once in Chase: if distance > `loseAggroRange` (larger than detection), switch back to Patrol. | 1.5 hrs |
| S2-AH-05 | Chase State | `HandleChase()`: move toward `player.position` using `Vector2.MoveTowards` at `chaseSpeed` (faster than patrol). Face toward player (flip sprite). If within `attackRange`: switch to `Attack` state. | 1 hr |
| S2-AH-06 | Attack State | `HandleAttack()`: stop moving. If `attackCooldown` timer is ready, call `Attack()`. Reset timer. Override `Attack()` in concrete enemy class to deal damage to player. Transition back to Chase if player leaves `attackRange`. | 1.5 hrs |
| S2-AH-07 | Detection Gizmos (Debug) | Override `OnDrawGizmosSelected()` to draw two wireframe circles: yellow = `detectionRange`, red = `attackRange`. This makes it easy to configure enemy ranges in the Scene view without guessing. | 30 min |
| S2-AH-08 | Sprint 2 Trello Management | Update Trello: archive Sprint 1 "Done" cards. Set up Sprint 2 cards with assignees. Mid-week check-in with team. | 45 min |

**Sprint 2 Deliverable:** A fully working enemy AI state machine. Enemies patrol, detect the player, chase them, and attack at range. Plug any enemy sprite in and it works out of the box.

---

### 2. Ali Abdulla — Combat System (Player Attack)
**Focus:** The player can deal damage to enemies. Melee attack with a hitbox, combo potential, knockback, and all attack animations hooked up.

| Task ID | Task | Description | Est. Time |
|---------|------|-------------|-----------|
| S2-AA-01 | Attack Hitbox Setup | On the Player prefab, create a child GameObject called `AttackHitbox`. Add a Box Collider 2D (IsTrigger = true). Position it slightly in front of the player (e.g., 0.5 units forward). **Disable** the collider by default — it will only be enabled during the attack animation frames. | 45 min |
| S2-AA-02 | PlayerCombat Script | Write `PlayerCombat.cs`. On `Attack` input (Left Mouse Click): check `attackCooldown` timer. If ready: call `TriggerAttack()`, enable `AttackHitbox` collider, start `DisableHitboxAfter(duration)` coroutine. `attackDamage` (float, default 20f), `attackCooldown` (0.5f), `attackRange` (1f). | 2 hrs |
| S2-AA-03 | Damage Detection | In `AttackHitbox`, add a separate `HitboxDetector.cs` script. In `OnTriggerEnter2D(Collider2D other)`: check `other.TryGetComponent<EnemyBase>(out enemy)`. If true: call `enemy.TakeDamage(playerCombat.attackDamage)`. Fire a `DamageNumber` pop-up (optional). Call `AudioManager.Instance.PlaySFX(hitSFX)`. | 1.5 hrs |
| S2-AA-04 | Attack Animation Hook | In `PlayerAnimationController.cs` (from Sprint 1): the `TriggerAttack()` method calls `animator.SetTrigger("Attack")`. The hitbox enable/disable must sync with the animation frames. Use Animation Events: add an event at the "swing start" frame calling `EnableHitbox()` and another at "swing end" calling `DisableHitbox()`. | 1.5 hrs |
| S2-AA-05 | Knockback on Hit | Write `Knockback.cs`: a method `ApplyKnockback(Vector2 direction, float force, float duration)`. Applies `rb.AddForce(direction * force, ForceMode2D.Impulse)`. After `duration` seconds, velocity returns to normal. Attach to both player and enemies so both can receive knockback. | 1.5 hrs |
| S2-AA-06 | Attack Direction | Player attack hitbox should be positioned based on the direction the player is facing (or toward the mouse cursor). If using mouse: `Vector2 dir = (mouseWorldPos - player.position).normalized`. Rotate the hitbox GameObject in that direction. For simple top-down: just use the current facing direction (left/right flip). | 1 hr |
| S2-AA-07 | Attack SFX | In `PlayerCombat.cs`, call `AudioManager.Instance.PlaySFX(swordSwingSFX)` on attack. Call `AudioManager.Instance.PlaySFX(hitSFX)` when the hitbox actually connects with an enemy (from `HitboxDetector`). Assign clips in Inspector (placeholder clips okay). | 30 min |

**Sprint 2 Deliverable:** Player left-clicks to attack with a synced hitbox. Enemies within range take damage. Knockback is applied. Attack animation plays. Hit SFX fires. All configurable from the Inspector.

---

### 3. Faisal Alasfoor — Health Bar UI + Game Over & Death Screen
**Focus:** Make the player's health visible and reactive. The health bar updates when the player takes damage, and the death screen shows when health reaches zero.

| Task ID | Task | Description | Est. Time |
|---------|------|-------------|-----------|
| S2-FA-01 | Health Bar Script | Write `HealthBarUI.cs`. Drag reference to a UI Slider (or use `Image.fillAmount`). Subscribe to `PlayerHealth.onHealthChanged` UnityEvent. When event fires: `Mathf.Lerp(currentFill, targetFill, lerpSpeed * Time.deltaTime)` for a smooth drain animation. Show current/max as text ("85 / 100"). | 2 hrs |
| S2-FA-02 | Enemy Health Bar (World Space) | Create a small health bar prefab (`EnemyHealthBar`) that appears above each enemy. Use a world-space Canvas (or a UI Image with fill). Show on first damage, hide when full. Update from `EnemyBase.onHealthChanged` event. | 2 hrs |
| S2-FA-03 | Hit Flash Effect | Write `HitFlash.cs`. A coroutine that temporarily sets `SpriteRenderer.color` to red for 0.12 seconds then back to white. Call it from `PlayerHealth.TakeDamage()` and `EnemyBase.TakeDamage()`. Makes taking damage feel impactful. | 1 hr |
| S2-FA-04 | Damage Number Pop-Up | Create a `DamageNumber` prefab: world-space TMP text that shows the damage value (e.g., "25"). Animate it: float upward 0.5 units over 0.8 seconds while fading out (alpha 1 → 0). Destroy after animation. Instantiate from `HitboxDetector.cs` at the hit position. | 2 hrs |
| S2-FA-05 | HUD Health Bar Polish | In the HUD Canvas (from Sprint 1), expand the health bar: add a background bar (dark red), the fill bar (bright red), and a border image. Add an animated pulse effect when health drops below 25% (use a UI Animation or `Mathf.PingPong` on the bar's color). | 1.5 hrs |
| S2-FA-06 | Lives Display | In HUD Canvas, add a lives counter: show heart icons (3 hearts = 3 lives). When a life is lost, the corresponding heart fades or goes dark. Update from `GameManager.playerLives`. | 1 hr |
| S2-FA-07 | Death Screen UI | In the `GameOver` scene (from Sprint 1 — Faisal set this up): fully implement it. Show: "YOU DIED" heading (large, red TMP). Stats: "Enemies Killed: X | Level: Y". Buttons: "Retry" (`SceneLoader.LoadScene(GameManager.currentLevel)`) and "Main Menu" (`SceneLoader.LoadScene("MainMenu")`). Add a dark overlay animation (fade from black). | 1.5 hrs |

**Sprint 2 Deliverable:** Health bar updates in real time with smooth animation. Enemies have individual health bars. Taking damage shows a red flash and damage number. Death leads to a proper Game Over screen. Lives are displayed.

---

### 4. Habib Husain Alshoofa — Player Health System & Death / Respawn Flow
**Focus:** The full player health pipeline — taking damage, dying, losing lives, respawning. This is what ties everything together.

| Task ID | Task | Description | Est. Time |
|---------|------|-------------|-----------|
| S2-HH-01 | PlayerHealth Script | Write `PlayerHealth.cs`. Fields: `maxHealth` (100f), `currentHealth` (100f), `isInvincible` (bool — for dodge frames later), `isAlive` (bool). Method `TakeDamage(float amount)`: if `isInvincible` return. Subtract amount from `currentHealth`. Clamp to 0. Fire `onHealthChanged(currentHealth, maxHealth)` UnityEvent. Call `HitFlash`. If `currentHealth <= 0` and `isAlive`: call `Die()`. | 2.5 hrs |
| S2-HH-02 | Heal Method | Add `Heal(float amount)` to `PlayerHealth.cs`: adds amount to `currentHealth`, clamps to `maxHealth`. Fire `onHealthChanged`. Play heal SFX. This will be called by consumable items in later sprints. | 30 min |
| S2-HH-03 | Die Method | `Die()` in `PlayerHealth.cs`: set `isAlive = false`. Disable `PlayerMovement` and `PlayerCombat` components (player becomes uncontrollable). Call `PlayerAnimationController.TriggerDeath()`. Disable player collider (enemies can't hit a corpse). Call `GameManager.Instance.PlayerDied()` after a 1.2s delay (coroutine). | 1.5 hrs |
| S2-HH-04 | GameManager.PlayerDied() | In `GameManager.cs`, add `PlayerDied()`: decrement `playerLives`. If `playerLives > 0`: call `RespawnPlayer()` after `respawnDelay` (2f seconds). If `playerLives <= 0`: load `GameOver` scene. | 1.5 hrs |
| S2-HH-05 | Respawn System | `RespawnPlayer()` in `GameManager.cs`: restore `PlayerHealth.currentHealth = maxHealth`. Re-enable `PlayerMovement` and `PlayerCombat`. Reset `isAlive = true`. Move player back to `SpawnPoint` position. Give 2 seconds of invincibility (set `isInvincible = true` for 2f seconds — brief flash effect to show invincibility frames). | 1.5 hrs |
| S2-HH-06 | Invincibility Frames | During respawn invincibility and dodge (future): player sprite flashes (alternates between visible and invisible every 0.1s using a coroutine on `SpriteRenderer.enabled`). After duration ends, `isInvincible = false`. | 1 hr |
| S2-HH-07 | Enemy TakeDamage Override | Implement `TakeDamage()` in `EnemyBase.cs` (non-abstract default): subtract from `currentHealth`. Call `HitFlash`. Fire `onHealthChanged` event. If `currentHealth <= 0`: call `Die()`. `Die()` default: play death animation, disable collider, call `Destroy(gameObject, 1.5f)`. | 1.5 hrs |

**Sprint 2 Deliverable:** Player takes damage, health bar updates, at 0 HP player plays death animation and becomes inactive. If lives remain, player respawns at spawn point with invincibility frames. If no lives, Game Over scene loads.

---

### 5. Khizar Azhar — First Enemy Implementation (Goblin)
**Focus:** Create the first concrete enemy using `EnemyBase`. A Goblin — fast, attacks in groups, configurable for Easy/Normal/Hard. This validates the entire AI system in practice.

| Task ID | Task | Description | Est. Time |
|---------|------|-------------|-----------|
| S2-KA-01 | Goblin Sprite Import & Slice | Import the Goblin sprite sheet. Slice it in Sprite Editor (grid by cell size, e.g., 48×48). Create animation clips: `Goblin_Idle`, `Goblin_Walk`, `Goblin_Attack`, `Goblin_Hurt`, `Goblin_Death`. | 2 hrs |
| S2-KA-02 | GoblinAnimator Controller | Create `GoblinAnimator.controller`. States: Idle (default), Walk, Attack, Hurt, Death. Transitions: Idle ↔ Walk (float: `speed`, threshold 0.1), Idle/Walk → Attack (trigger: `Attack`), Any → Hurt (trigger: `Hurt`), Any → Death (trigger: `Die`). | 1 hr |
| S2-KA-03 | Goblin Class | Create `Goblin.cs` extending `EnemyBase`. Override `Attack()`: create a melee hitbox (a child `GoblinAttackHitbox` collider enabled for 0.3s on attack animation event). Deal `attackDamage` to player via `PlayerHealth.TakeDamage()`. Override `Die()`: trigger death animation, drop a coin collectible at position, call `Destroy(gameObject, 1.5f)`. | 2 hrs |
| S2-KA-04 | Goblin Prefab | Create `EnemyGoblinPrefab`: SpriteRenderer, Rigidbody2D (no gravity, freeze Z rotation), CapsuleCollider2D, `Goblin.cs`, Animator (GoblinAnimator controller). Assign patrol points array in Inspector. Tag: `Enemy`, Layer: `Enemy`. | 1 hr |
| S2-KA-05 | Goblin Variants Configuration | Using the Inspector, configure 3 variants by adjusting fields on the same prefab (or create 3 prefab variants): Easy Goblin (health: 30, damage: 8, speed: 2.5), Normal Goblin (health: 50, damage: 15, speed: 3.5), Hard Goblin (health: 80, damage: 25, speed: 5.0). | 45 min |
| S2-KA-06 | Goblin Spawner | Write `EnemySpawner.cs`: a list of `SpawnEntry { GameObject enemyPrefab, Transform spawnPoint, int count }`. On `Start()`, instantiate each enemy at their spawn point. Place 3-4 Goblins in the test level in different positions with patrol routes. | 1.5 hrs |
| S2-KA-07 | Combat Integration Test | Verify the full combat loop with the Goblin: player walks into detection range → Goblin chases → attacks player → player takes damage (health bar drops) → player attacks back → Goblin takes damage (health bar drops, hit flash, damage number) → Goblin dies (animation, coin drops). Document any bugs as Trello cards. | 1 hr |

**Sprint 2 Deliverable:** A fully functional Goblin enemy in the test level: it patrols, detects the player, chases, attacks, takes damage, and dies with a loot drop. 3 difficulty variants are configured.

---

### 6. Rawh Hasan — AudioManager Expansion + Level Design for Combat Testing
**Focus:** Expand the AudioManager with combat sounds, and build out the test level to be suitable for combat — enemies need space to patrol, clear sightlines, and room for the player to fight.

| Task ID | Task | Description | Est. Time |
|---------|------|-------------|-----------|
| S2-RH-01 | Combat SFX Collection | Source/create placeholder audio clips: sword swing (whoosh), sword hit (thud/slash), player hurt (grunt), player death (collapse sound), enemy hurt (yelp), enemy death (groan), coin pickup (chime). All must be `.wav` or `.mp3`, short (< 2s). Place in `Assets/Audio/SFX/Combat/`. | 2 hrs |
| S2-RH-02 | AudioManager SFX Variations | Update `AudioManager.cs`: add `PlaySFX(AudioClip clip, float volumeScale)` overload. Add `PlayRandomSFX(AudioClip[] clips)` method that picks a random clip from an array and plays it. This prevents repetitive hit sounds (e.g., 3 different hit sounds, one plays randomly). | 1.5 hrs |
| S2-RH-03 | Background Music | Add an ambient desert exploration loop track. Call `AudioManager.Instance.PlayMusic(explorationMusic)` from `LevelManager.cs` on scene load. The music should loop seamlessly. Volume: not too loud (around 0.3–0.5). | 1 hr |
| S2-RH-04 | Combat Level Layout | Expand the test level to support combat testing. Add: defined patrol corridors (walls forming paths for Goblins to patrol), an open combat arena area (large open space for the player to fight), some obstacles (boxes, rocks) that block movement but not sightlines. Level should be ~40×30 units total. | 2 hrs |
| S2-RH-05 | Patrol Point Setup | Place 4 sets of `PatrolPoints` GameObjects in the level (empty GameObjects as waypoints). Each set has 2-4 patrol points forming a simple route. Assign each set to the corresponding Goblin spawned by the EnemySpawner. | 1 hr |
| S2-RH-06 | Level Lighting Polish | Add a few `Spot Light 2D` or `Point Light 2D` objects to create atmosphere in the combat level (e.g., a torch flickering effect using an `Animator` on a Point Light). Keep the global light slightly lower (intensity 0.8) to create some contrast. | 1 hr |
| S2-RH-07 | Sprint 2 Integration Test | After all systems are merged on `develop`: pull the branch, run the full scene. Test: player moves, attacks Goblin, Goblin chases/attacks, health bar updates, death/respawn works, SFX fire on events, music plays on load. Write a brief "Integration Test Report" comment in the Trello Sprint 2 card. | 1 hr |

**Sprint 2 Deliverable:** Combat SFX fully integrated with the combat system. Music plays on level load. Test level has a proper combat layout with patrol corridors, combat arena, and patrol points set up. Full integration test passes.

---

## Sprint 2 Summary Table

| Member | Primary Focus | Est. Hours |
|--------|--------------|------------|
| Ali Husain | Enemy AI state machine (patrol/detect/chase/attack) | ~11h |
| Ali Abdulla | Player combat system (attack, hitbox, knockback) | ~9h |
| Faisal Alasfoor | UI: Health bars, hit effects, damage numbers, death screen | ~11h |
| Habib Husain | PlayerHealth, Death system, Respawn, Invincibility frames | ~10h |
| Khizar Azhar | Goblin enemy (sprites, AI, variants, spawner) | ~9h |
| Rawh Hasan | Combat SFX, AudioManager updates, level layout | ~9.5h |

---

## Integration Points (Sprint 2)

| From | To | What |
|------|----|------|
| Habib (PlayerHealth) | Faisal (HealthBarUI) | `onHealthChanged` UnityEvent subscription |
| Habib (PlayerHealth) | Faisal (HitFlash) | `TakeDamage()` calls `HitFlash` coroutine |
| Ali Abdulla (HitboxDetector) | Habib (EnemyBase.TakeDamage) | Hitbox calls `enemy.TakeDamage(damage)` |
| Ali Abdulla (HitboxDetector) | Faisal (DamageNumber) | Instantiate damage number at hit position |
| Ali Husain (EnemyBase) | Khizar (Goblin.cs) | Goblin extends EnemyBase — inherits all AI logic |
| Ali Husain (EnemyBase.Attack) | Habib (PlayerHealth) | Enemy attack calls `playerHealth.TakeDamage()` |
| Ali Husain (GameManager) | Habib (Death system) | `PlayerDied()` called from `PlayerHealth.Die()` |
| Rawh (AudioManager) | Ali Abdulla (Combat) | `PlaySFX()` calls in PlayerCombat and HitboxDetector |
| Rawh (Patrol Points) | Khizar (Goblin) | PatrolPoints arrays assigned in EnemySpawner |

---

## Definition of Done (Sprint 2)

- [ ] Enemy patrols between waypoints, detects player, chases, attacks
- [ ] Player left-click attack deals damage to enemies with hitbox detection
- [ ] Health bar drains smoothly when damage is taken
- [ ] Enemies have individual world-space health bars
- [ ] Hit flash (red) plays on both player and enemy damage
- [ ] Damage numbers appear and float up at hit position
- [ ] Player dies when health reaches 0 (death animation, becomes inactive)
- [ ] Lives system: lose life → respawn; no lives → Game Over scene loads
- [ ] Goblin enemy works end-to-end: patrol → detect → chase → attack → die → loot drop
- [ ] Combat SFX fires on swing, hit, hurt, death
- [ ] Music plays on level load
- [ ] All work pushed to `develop` branch
- [ ] All Trello Sprint 2 cards updated with results

---

*Prepared by: Ali Husain (Scrum Master) | Date: April 8, 2026*
