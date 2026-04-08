# Sprint 2 — Pull Request Plan
## Deserted Echoes | IT8101 Games Development
**Sprint Duration:** April 15 – 21, 2026
**Sprint Theme:** Combat & Enemy AI

---

## Prerequisite — Sprint 1 Must Be Fully Merged

> Before Sprint 2 begins, verify `develop` contains all Sprint 1 work:
> - `PlayerPrefab.prefab` with `PlayerMovement.cs`, `PlayerStats.cs`, `PlayerAnimationController.cs`
> - `GameManager.cs` singleton, `SceneLoader.cs`
> - `AudioManager.cs` singleton with `PlaySFX()` / `PlayMusic()`
> - `Level_01` scene with `LevelManager`, `SpawnPoint`, `Cinemachine` camera
> - All 6 members pull latest `develop` before creating Sprint 2 branches

---

## Git Branching Structure (Sprint 2)

```
develop
  ├── feature/s2-enemy-base           (Ali Husain)  ← WAVE 0 BLOCKER
  ├── feature/s2-enemy-patrol         (Ali Husain)
  ├── feature/s2-enemy-detection      (Ali Husain)
  ├── feature/s2-enemy-attack-state   (Ali Husain)
  ├── feature/s2-player-health        (Habib)       ← WAVE 0 BLOCKER
  ├── feature/s2-attack-hitbox        (Ali Abdulla)
  ├── feature/s2-player-combat        (Ali Abdulla)
  ├── feature/s2-knockback            (Ali Abdulla)
  ├── feature/s2-health-bar-ui        (Faisal)
  ├── feature/s2-hit-effects          (Faisal)
  ├── feature/s2-death-screen         (Faisal)
  ├── feature/s2-death-respawn        (Habib)
  ├── feature/s2-goblin-sprites       (Khizar)
  ├── feature/s2-goblin-ai            (Khizar)
  ├── feature/s2-enemy-spawner        (Khizar)
  ├── feature/s2-combat-sfx           (Rawh)
  ├── feature/s2-level-combat-layout  (Rawh)
  └── feature/s2-patrol-points        (Rawh)
```

---

## PR Wave Overview (Concurrent vs Sequential)

```
DAY 1 (Apr 15)       DAY 2 (Apr 16)      DAY 3 (Apr 17)       DAY 4 (Apr 18)       DAY 5-6 (Apr 19-20)  DAY 7 (Apr 21)
───────────────────  ──────────────────  ───────────────────  ───────────────────  ───────────────────  ──────────────
 WAVE 0               WAVE 1              WAVE 2               WAVE 3               WAVE 4               WAVE 5
 [DUAL BLOCKERS]       [CONCURRENT]        [CONCURRENT]         [CONCURRENT]         [CONCURRENT]         [INTEGRATION]

 PR-01 enemy-base     PR-03 patrol         PR-06 player-combat  PR-10 death-respawn  PR-14 goblin-ai      PR-18
 PR-02 player-health  PR-04 hitbox         PR-07 health-bar-ui  PR-11 detection      PR-15 spawner        integration
                      PR-05 goblin-sprites PR-08 hit-effects    PR-12 sfx-update     PR-16 patrol-points
                      PR-09 combat-sfx     PR-13 level-layout   PR-enemy-attack-st.  PR-17 death-screen
                      PR-10a combat-layout
```

> **Two blockers this sprint:** `enemy-base` (PR-01) and `player-health` (PR-02).
> Multiple other PRs depend on both. Both can be opened on Day 1 **concurrently** with each other.

---

## Wave 0 — DUAL BLOCKERS (Day 1, Apr 15)
> ⚠️ Both PRs are independent of each other but are blockers for the rest of the sprint.
> Open both simultaneously. Assign two different reviewers for speed.

### PR-01 | `feature/s2-enemy-base`
**Owner:** Ali Husain Ali Alsaffar
**Branch:** `feature/s2-enemy-base` → `develop`
**Open:** Apr 15 AM | **Target Merge:** Apr 15 EOD
**Reviewer:** Ali Abdulla

**What this PR contains:**
- `EnemyBase.cs` abstract class (inherits `MonoBehaviour`):
  - Fields: `maxHealth`, `currentHealth`, `moveSpeed`, `attackDamage`, `detectionRange`, `attackRange`, `attackCooldown`, `chaseSpeed`, `loseAggroRange`
  - References: `Rigidbody2D rb`, `Transform player` (found on `Start()` by tag)
  - `EnemyState` enum: `Idle`, `Patrol`, `Chase`, `Attack`, `Dead`
  - `currentState` variable with `Update()` → switch dispatch
  - Abstract: `Attack()` (each enemy implements differently)
  - Virtual with default: `TakeDamage(float amount)`, `Die()`
  - `UnityEvent onHealthChanged` for UI binding
  - `OnDrawGizmosSelected()` debug circles

**Depends on:** Sprint 1 `develop` ✓
**Blocks:** PR-06 (PlayerCombat needs `EnemyBase` to call `TakeDamage`), PR-14 (Goblin extends EnemyBase), PR-11 (patrol/detection states built on top)

---

### PR-02 | `feature/s2-player-health`
**Owner:** Habib Husain Alshoofa
**Branch:** `feature/s2-player-health` → `develop`
**Open:** Apr 15 AM | **Target Merge:** Apr 15 EOD
**Reviewer:** Faisal Alasfoor (he needs `onHealthChanged` event for the health bar)

**What this PR contains:**
- `PlayerHealth.cs`:
  - Fields: `maxHealth (100f)`, `currentHealth`, `isInvincible (bool)`, `isAlive (bool)`
  - `UnityEvent<float, float> onHealthChanged` — fires with `(current, max)` on any change
  - `TakeDamage(float amount)`: guard `isInvincible`, subtract, clamp 0, fire event, call `HitFlash` (stub call OK for now), if `currentHealth <= 0 && isAlive` → `Die()`
  - `Heal(float amount)`: add, clamp max, fire event
  - `Die()`: stub for now — sets `isAlive = false`, logs "Player died" (full implementation in PR-10)
- `PlayerHealth` added to `PlayerPrefab`

**Depends on:** Sprint 1 `develop` ✓
**Blocks:** PR-07 (Faisal's HealthBarUI subscribes to `onHealthChanged`), PR-10 (death/respawn expands `Die()`), PR-06 (enemy attack calls `playerHealth.TakeDamage`)

---

## Wave 1 — CONCURRENT (Day 2, Apr 16)
> All 4 PRs open simultaneously after Wave 0 merges.

### PR-03 | `feature/s2-enemy-patrol`
**Owner:** Ali Husain Ali Alsaffar
**Branch:** `feature/s2-enemy-patrol` → `develop`
**Open:** Apr 16 | **Target Merge:** Apr 16
**Reviewer:** Khizar Azhar (Goblin will use this)

**What this PR contains:**
- `HandleIdle()` in `EnemyBase`: stays still, transitions to Patrol on timer
- `HandlePatrol()` in `EnemyBase`:
  - `Transform[] patrolPoints` array (assigned in Inspector)
  - `Vector2.MoveTowards(rb.position, patrolPoints[index].position, moveSpeed * Time.deltaTime)`
  - At destination (< 0.2f): wait `patrolWaitTime` seconds (coroutine), advance index with modulo
  - Sprite flip toward movement direction

**Depends on:** PR-01 ✓
**Blocks:** PR-11 (detection/chase built on top of this), PR-16 (Rawh places patrol points that this uses)

---

### PR-04 | `feature/s2-attack-hitbox`
**Owner:** Ali Abdulla
**Branch:** `feature/s2-attack-hitbox` → `develop`
**Open:** Apr 16 | **Target Merge:** Apr 16
**Reviewer:** Habib Husain

**What this PR contains:**
- `AttackHitbox` child `GameObject` on `PlayerPrefab`: `Box Collider 2D` (IsTrigger=true, **disabled by default**)
- `HitboxDetector.cs` on the `AttackHitbox` object:
  - `OnTriggerEnter2D`: checks `other.TryGetComponent<EnemyBase>(out enemy)` — calls `enemy.TakeDamage(ownerCombat.attackDamage)`
  - `DamageDealer ownerCombat` reference (set by `PlayerCombat` on Start)
- `Knockback.cs` component:
  - `ApplyKnockback(Vector2 direction, float force, float duration)` → `rb.AddForce(dir * force, Impulse)` → coroutine resets velocity after duration

**Depends on:** PR-01 ✓ (EnemyBase must exist for `TryGetComponent<EnemyBase>`)
**Blocks:** PR-06 (PlayerCombat drives the hitbox enable/disable)

---

### PR-05 | `feature/s2-goblin-sprites`
**Owner:** Khizar Azhar
**Branch:** `feature/s2-goblin-sprites` → `develop`
**Open:** Apr 16 | **Target Merge:** Apr 17
**Reviewer:** Ali Husain (verifies naming follows conventions)

**What this PR contains:**
- Goblin sprite sheet imported (Point filter, No compression, Multiple mode)
- Sliced into frames by cell grid
- 5 `.anim` clips in `Assets/Animations/Enemies/Goblin/`: `Goblin_Idle` (loop), `Goblin_Walk` (loop), `Goblin_Attack` (no loop), `Goblin_Hurt` (no loop), `Goblin_Death` (no loop)
- `GoblinAnimator.controller` — all 5 states, transitions: Idle↔Walk (float `speed` 0.1), Attack/Hurt/Die triggers

**Depends on:** PR-01 ✓
**Blocks:** PR-14 (Goblin.cs needs the animator controller assigned)

---

### PR-09 | `feature/s2-combat-sfx`
**Owner:** Rawh Hasan
**Branch:** `feature/s2-combat-sfx` → `develop`
**Open:** Apr 16 | **Target Merge:** Apr 17
**Reviewer:** Ali Abdulla (connects SFX to combat scripts)

**What this PR contains:**
- Audio clips sourced and imported in `Assets/Audio/SFX/Combat/`: `sfx_sword_swing.wav`, `sfx_sword_hit_1.wav`, `sfx_sword_hit_2.wav`, `sfx_sword_hit_3.wav`, `sfx_player_hurt.wav`, `sfx_player_death.wav`, `sfx_enemy_hurt.wav`, `sfx_enemy_death.wav`
- `AudioManager.cs` updated:
  - `PlaySFX(AudioClip clip, float volumeScale)` overload
  - `PlayRandomSFX(AudioClip[] clips)` — picks random from array, prevents repetition
- Background exploration music loop clip imported: `music_exploration.mp3` in `Assets/Audio/Music/`

**Depends on:** Sprint 1 `AudioManager` ✓
**Blocks:** PR-06 (PlayerCombat calls `PlaySFX` for swing/hit sounds), PR-08 (HitFlash may play hurt sound)

---

### PR-combat-layout | `feature/s2-level-combat-layout`
**Owner:** Rawh Hasan
**Branch:** `feature/s2-level-combat-layout` → `develop`
**Open:** Apr 16 | **Target Merge:** Apr 17
**Reviewer:** Khizar Azhar (he places enemies here)

**What this PR contains:**
- Level expanded to ~40×30 units: patrol corridors (narrow walled paths 3-4 units wide), open combat arena (10×10 clear space), internal rock/box obstacles
- Music call: `LevelManager.Start()` updated → `AudioManager.Instance.PlayMusic(explorationMusic)`
- 4 sets of `PatrolPoints` GameObjects placed at strategic corridor positions (2-4 child waypoints per set)
- Atmosphere: 2-3 `Point Light 2D` objects with flickering animator on intensity

**Depends on:** Sprint 1 level ✓
**Blocks:** PR-16 (formal patrol point assignment to spawner), PR-15 (EnemySpawner references spawn positions here)

---

## Wave 2 — CONCURRENT (Day 3, Apr 17)
> Open all simultaneously after Wave 1 PRs merge.

### PR-06 | `feature/s2-player-combat`
**Owner:** Ali Abdulla
**Branch:** `feature/s2-player-combat` → `develop`
**Open:** Apr 17 | **Target Merge:** Apr 18
**Reviewer:** Habib Husain

**What this PR contains:**
- `PlayerCombat.cs`:
  - `attackDamage (20f)`, `attackCooldown (0.5f)`, `isAttacking (bool)`
  - On `Attack` input: check cooldown → call `PlayerAnimationController.TriggerAttack()` → enable `AttackHitbox` collider → `DisableHitboxAfter(0.3f)` coroutine
  - Attack direction: rotate `AttackHitbox` GameObject toward `mouseWorldPosition` (via `Camera.main.ScreenToWorldPoint`)
  - Calls `AudioManager.Instance.PlaySFX(swordSwingSFX)` on attack
  - Sets `HitboxDetector.ownerCombat = this` on `Start()`
- `HitboxDetector` updated: on hit → `AudioManager.Instance.PlayRandomSFX(hitSFXArray)` + spawn `DamageNumber` prefab stub (stub OK, Faisal does the prefab)
- `Knockback` applied to enemy on hit (calls enemy's `Knockback.ApplyKnockback`)

**Depends on:** PR-01 ✓, PR-02 ✓, PR-04 ✓, PR-09 ✓
**Blocks:** PR-10 (death respawn tests the full combat loop)

---

### PR-07 | `feature/s2-health-bar-ui`
**Owner:** Faisal Alasfoor
**Branch:** `feature/s2-health-bar-ui` → `develop`
**Open:** Apr 17 | **Target Merge:** Apr 18
**Reviewer:** Habib Husain

**What this PR contains:**
- `HealthBarUI.cs` on the HUD health bar:
  - `[SerializeField] PlayerHealth playerHealth` (assign in Inspector)
  - Subscribes to `playerHealth.onHealthChanged` in `OnEnable()`, unsubscribes in `OnDisable()`
  - On event: target fill = `current / max`, `Mathf.Lerp` fill each Update at `lerpSpeed`
  - TMP text updates: "85 / 100"
  - Pulse animation when `currentHealth / maxHealth < 0.25f` — `Mathf.PingPong` on bar color
- `EnemyHealthBar.prefab` — world-space `Canvas`, small slider above enemy:
  - Hidden until `TakeDamage` called, hidden again if health = max
  - Updated from `EnemyBase.onHealthChanged` event
  - Position: `transform.position + Vector3.up * 0.8f` updated in `LateUpdate`

**Depends on:** PR-02 ✓ (`PlayerHealth.onHealthChanged` event must exist)
**Blocks:** PR-08 (HitFlash is companion to health bar), PR-17 (death screen shows after health = 0 path)

---

### PR-08 | `feature/s2-hit-effects`
**Owner:** Faisal Alasfoor
**Branch:** `feature/s2-hit-effects` → `develop`
**Open:** Apr 17 | **Target Merge:** Apr 18
**Reviewer:** Ali Abdulla

**What this PR contains:**
- `HitFlash.cs` — coroutine: set `SpriteRenderer.color = red` for 0.12s → back to white. `Flash()` public method called from `PlayerHealth.TakeDamage()` and `EnemyBase.TakeDamage()`
- `DamageNumber.prefab` — world-space TMP text (`TextMeshPro` component):
  - Shows damage value (e.g., "25") in white/yellow
  - Animates: float up 0.5 units over 0.8s + alpha fades 1→0 simultaneously (coroutine)
  - `Destroy(gameObject)` after animation completes
  - `DamageNumberSpawner.cs` static helper: `Spawn(Vector3 worldPos, float damage)`
- `EnemyBase.TakeDamage()` default implementation updated to call `HitFlash.Flash()` and `DamageNumberSpawner.Spawn(transform.position, amount)`
- Lives display updated: 3 heart `Image` components in HUD, dim on life lost (alpha 0.3)

**Depends on:** PR-02 ✓, PR-07 ✓ (works alongside health bar)
**Blocks:** PR-17 (death screen logic flows from health reaching 0)

---

## Wave 3 — CONCURRENT (Day 4, Apr 18)
> Open all simultaneously after Wave 2 merges.

### PR-11 | `feature/s2-enemy-detection`
**Owner:** Ali Husain Ali Alsaffar
**Branch:** `feature/s2-enemy-detection` → `develop`
**Open:** Apr 18 | **Target Merge:** Apr 19
**Reviewer:** Khizar Azhar

**What this PR contains:**
- `HandleChase()` in `EnemyBase`:
  - `Vector2.MoveTowards(rb.pos, player.pos, chaseSpeed * Time.deltaTime)`
  - Sprite flip toward player
  - If distance < `attackRange` → switch to `Attack`
  - If distance > `loseAggroRange` → switch back to `Patrol`
- Detection in `Update()` every frame:
  - `Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer)` — if hit and not `Dead` → switch to `Chase`

**Depends on:** PR-03 ✓ (patrol must exist), PR-01 ✓
**Blocks:** PR-enemy-attack (attack state built on detection)

---

### PR-enemy-attack | `feature/s2-enemy-attack-state`
**Owner:** Ali Husain Ali Alsaffar
**Branch:** `feature/s2-enemy-attack-state` → `develop`
**Open:** Apr 18 | **Target Merge:** Apr 19
**Reviewer:** Habib Husain

**What this PR contains:**
- `HandleAttack()` in `EnemyBase`:
  - Stop movement while attacking
  - Cooldown timer: `attackTimer -= Time.deltaTime`. If ≤ 0: call `Attack()`, reset timer
  - If distance to player > `attackRange`: switch back to `Chase`
- `EnemyBase.TakeDamage()` finalized with `HitFlash`, `DamageNumber`, `onHealthChanged` fire
- `EnemyBase.Die()` default: `currentState = Dead`, trigger `Die` animator trigger, disable collider, `Destroy(gameObject, 1.5f)`, drop loot stub (just `Debug.Log("Loot drop")` for now — Khizar implements properly)

**Depends on:** PR-11 ✓, PR-08 ✓ (HitFlash + DamageNumber must exist)
**Blocks:** PR-14 (Goblin's `Attack()` override plugs into this framework)

---

### PR-10 | `feature/s2-death-respawn`
**Owner:** Habib Husain Alshoofa
**Branch:** `feature/s2-death-respawn` → `develop`
**Open:** Apr 18 | **Target Merge:** Apr 19
**Reviewer:** Faisal Alasfoor (his Game Over screen is triggered here)

**What this PR contains:**
- `PlayerHealth.Die()` fully implemented:
  - `isAlive = false`
  - Disable `PlayerMovement` and `PlayerCombat` components
  - Call `PlayerAnimationController.TriggerDeath()`
  - Disable `CapsuleCollider2D`
  - Start coroutine: wait 1.2s → `GameManager.Instance.PlayerDied()`
- `GameManager.PlayerDied()`:
  - Decrement `playerLives`
  - `playerLives > 0`: `StartCoroutine(RespawnAfter(2f))`
  - `playerLives <= 0`: `SceneLoader.LoadScene("GameOver")`
- `RespawnPlayer()`:
  - Restore `playerHealth.currentHealth = playerHealth.maxHealth`
  - Re-enable `PlayerMovement`, `PlayerCombat`, collider
  - `isAlive = true`
  - Teleport player to `LevelManager.spawnPoint.position`
  - Start `InvincibilityFrames(2f)` coroutine (sprite flicker: `sr.enabled` toggle every 0.1s for 2s)

**Depends on:** PR-02 ✓, PR-06 ✓ (combat must exist to trigger death), PR-17 is triggered by this
**Blocks:** PR-17 (Game Over scene is loaded by this)

---

### PR-combat-sfx-update | `feature/s2-combat-sfx-integration`
**Owner:** Rawh Hasan
**Branch:** `feature/s2-combat-sfx-integration` → `develop`
**Open:** Apr 18 | **Target Merge:** Apr 19
**Reviewer:** Ali Abdulla

**What this PR contains:**
- All `AudioManager.Instance.PlaySFX()` calls added where missing:
  - `EnemyBase.TakeDamage()` → `PlayRandomSFX(enemyHurtSFX[])`
  - `EnemyBase.Die()` → `PlaySFX(enemyDeathSFX)`
  - `PlayerHealth.TakeDamage()` → `PlaySFX(playerHurtSFX)`
  - `PlayerHealth.Die()` → `PlaySFX(playerDeathSFX)`
- `LevelManager.Start()` confirmed calling `PlayMusic(explorationMusic)`
- Audio clip references assigned in all relevant Inspector fields

**Depends on:** PR-09 ✓, PR-02 ✓, PR-enemy-attack ✓
**Blocks:** PR-18 (integration — verifies all audio fires correctly)

---

## Wave 4 — CONCURRENT (Day 5-6, Apr 19-20)

### PR-14 | `feature/s2-goblin-ai`
**Owner:** Khizar Azhar
**Branch:** `feature/s2-goblin-ai` → `develop`
**Open:** Apr 19 | **Target Merge:** Apr 20
**Reviewer:** Ali Husain (verify Goblin correctly extends EnemyBase)

**What this PR contains:**
- `Goblin.cs extends EnemyBase`:
  - `Attack()` override: enable `GoblinAttackHitbox` child collider for 0.3s, `GoblinHitbox.OnTriggerEnter2D` calls `playerHealth.TakeDamage(attackDamage)`, calls `AudioManager.PlaySFX(goblinAttackSFX)`
  - `Die()` override: call `base.Die()`, then `Instantiate(coinCollectiblePrefab, transform.position, Quaternion.identity)`
- `GoblinAttackHitbox` child object with `BoxCollider2D` (disabled by default)
- `EnemyGoblinPrefab.prefab` fully assembled: all components, Tag=Enemy, Layer=Enemy, GoblinAnimator assigned
- 3 prefab variants configured in Inspector:
  - Easy: HP=30, dmg=8, speed=2.5, chaseSpeed=3.0, detectionRange=5
  - Normal: HP=50, dmg=15, speed=3.5, chaseSpeed=4.5, detectionRange=6
  - Hard: HP=80, dmg=25, speed=5.0, chaseSpeed=6.5, detectionRange=7
- `EnemyHealthBar` prefab assigned to `GoblinPrefab`

**Depends on:** PR-01 ✓, PR-05 ✓ (sprites + animator), PR-enemy-attack ✓ (full AI framework)
**Blocks:** PR-15 (Spawner uses this prefab)

---

### PR-15 | `feature/s2-enemy-spawner`
**Owner:** Khizar Azhar
**Branch:** `feature/s2-enemy-spawner` → `develop`
**Open:** Apr 19 | **Target Merge:** Apr 20
**Reviewer:** Rawh Hasan (patrol points are placed by Rawh)

**What this PR contains:**
- `EnemySpawner.cs`:
  - `[System.Serializable] SpawnEntry { GameObject enemyPrefab; Transform spawnPoint; Transform[] patrolPoints; int count; }`
  - `List<SpawnEntry> enemies` — configured in Inspector
  - `Start()`: for each entry, spawn `count` enemies at `spawnPoint`, assign `patrolPoints` to each `EnemyBase`
- 4 Goblin (Easy) instances placed in Level_01 via EnemySpawner with their patrol routes
- Integration: full combat loop playtest completed (player fights goblin end-to-end)

**Depends on:** PR-14 ✓, PR-combat-layout ✓ (level must have patrol corridors)
**Blocks:** PR-18 (integration needs enemies in the scene)

---

### PR-16 | `feature/s2-patrol-points`
**Owner:** Rawh Hasan
**Branch:** `feature/s2-patrol-points` → `develop`
**Open:** Apr 19 | **Target Merge:** Apr 20
**Reviewer:** Khizar Azhar

**What this PR contains:**
- 4 `PatrolRoute_X` parent GameObjects in Level_01, each with 2-4 child `Waypoint_X` transforms
- Patrol routes form logical corridors matching the level layout from PR-combat-layout
- `EnemySpawner` Inspector fields updated to reference these patrol routes
- Level lighting polish: additional atmosphere lights adjusted, no gameplay impact

**Depends on:** PR-combat-layout ✓, PR-03 ✓
**Blocks:** PR-15 (EnemySpawner patrol assignments), PR-18 (needs patrol routes for integration test)

---

### PR-17 | `feature/s2-death-screen`
**Owner:** Faisal Alasfoor
**Branch:** `feature/s2-death-screen` → `develop`
**Open:** Apr 19 | **Target Merge:** Apr 20
**Reviewer:** Habib Husain (death triggers the scene load)

**What this PR contains:**
- `GameOver` scene fully implemented:
  - "YOU DIED" TMP heading: size 80, dark red, center-aligned
  - Stats panel: "Enemies Killed: 0  |  Level: 1" (static for now — dynamic later)
  - Brief dark fade-in overlay (UI `Image` alpha 0→1 over 0.5s via coroutine on scene load)
  - "Retry Level" button → `SceneLoader.LoadScene(GameManager.Instance.currentLevelScene)`
  - "Main Menu" button → `SceneLoader.LoadScene("MainMenu")`
- Lives counter in HUD: heart icons dim (alpha 0.3) when life lost — hooked to `GameManager.playerLives`

**Depends on:** PR-10 ✓ (PlayerDied triggers GameOver scene load), PR-08 ✓
**Blocks:** PR-18 (integration verifies death screen loads)

---

## Wave 5 — INTEGRATION (Day 7, Apr 21)

### PR-18 | Sprint 2 Integration Verification
**Owner:** Ali Husain (Scrum Master — leads, all members participate)
**Branch:** No new branch — verify `develop` is clean and working
**Date:** Apr 21

**Full Combat Loop Checklist:**
- [ ] Enemy spawns in level, patrols between waypoints
- [ ] Enemy detects player within `detectionRange`, switches to Chase
- [ ] Enemy chases player, stops at `attackRange`, deals damage
- [ ] Player health bar decreases smoothly on damage
- [ ] Red hit flash plays on both player and enemy when hit
- [ ] Damage numbers float up and fade at hit position
- [ ] Enemy health bar appears on first hit, updates correctly
- [ ] Player left-click attack enables hitbox, deals damage to Goblin
- [ ] Goblin die animation plays, coin drop spawns at death position
- [ ] Player death at 0 HP: animation plays, input disabled, 1.2s delay → life lost
- [ ] Life lost with lives remaining: respawn at spawn point with invincibility flicker
- [ ] Lives = 0: Game Over scene loads with Retry + Main Menu
- [ ] All SFX fire: sword swing, sword hit, player hurt, enemy hurt, enemy death, player death
- [ ] Music plays on level load
- [ ] No console errors during any of the above

---

## Full Sprint 2 PR Timeline

| PR | Branch | Owner | Opens | Merges | Concurrent With | Blocked By |
|----|--------|-------|-------|--------|-----------------|------------|
| PR-01 | `s2-enemy-base` | Ali Husain | Apr 15 AM | Apr 15 EOD | PR-02 | Sprint 1 done |
| PR-02 | `s2-player-health` | Habib | Apr 15 AM | Apr 15 EOD | PR-01 | Sprint 1 done |
| PR-03 | `s2-enemy-patrol` | Ali Husain | Apr 16 | Apr 16 | PR-04, PR-05, PR-09, PR-layout | PR-01 |
| PR-04 | `s2-attack-hitbox` | Ali Abdulla | Apr 16 | Apr 16 | PR-03, PR-05, PR-09, PR-layout | PR-01 |
| PR-05 | `s2-goblin-sprites` | Khizar | Apr 16 | Apr 17 | PR-03, PR-04, PR-09, PR-layout | PR-01 |
| PR-09 | `s2-combat-sfx` | Rawh | Apr 16 | Apr 17 | PR-03, PR-04, PR-05 | Sprint 1 Audio |
| PR-layout | `s2-level-combat-layout` | Rawh | Apr 16 | Apr 17 | PR-03, PR-04, PR-05, PR-09 | Sprint 1 level |
| PR-06 | `s2-player-combat` | Ali Abdulla | Apr 17 | Apr 18 | PR-07, PR-08 | PR-01, PR-02, PR-04, PR-09 |
| PR-07 | `s2-health-bar-ui` | Faisal | Apr 17 | Apr 18 | PR-06, PR-08 | PR-02 |
| PR-08 | `s2-hit-effects` | Faisal | Apr 17 | Apr 18 | PR-06, PR-07 | PR-02, PR-07 |
| PR-11 | `s2-enemy-detection` | Ali Husain | Apr 18 | Apr 19 | PR-10, PR-ea, PR-sfx-int | PR-03 |
| PR-ea | `s2-enemy-attack-state` | Ali Husain | Apr 18 | Apr 19 | PR-10, PR-11, PR-sfx-int | PR-11, PR-08 |
| PR-10 | `s2-death-respawn` | Habib | Apr 18 | Apr 19 | PR-11, PR-ea, PR-sfx-int | PR-02, PR-06 |
| PR-sfx-int | `s2-combat-sfx-integration` | Rawh | Apr 18 | Apr 19 | PR-10, PR-11, PR-ea | PR-09, PR-02 |
| PR-14 | `s2-goblin-ai` | Khizar | Apr 19 | Apr 20 | PR-15, PR-16, PR-17 | PR-05, PR-ea |
| PR-15 | `s2-enemy-spawner` | Khizar | Apr 19 | Apr 20 | PR-14, PR-16, PR-17 | PR-14, PR-layout |
| PR-16 | `s2-patrol-points` | Rawh | Apr 19 | Apr 20 | PR-14, PR-15, PR-17 | PR-layout, PR-03 |
| PR-17 | `s2-death-screen` | Faisal | Apr 19 | Apr 20 | PR-14, PR-15, PR-16 | PR-10, PR-08 |
| PR-18 | Integration check | Ali Husain | Apr 21 | Apr 21 | — | All above |

---

## Dependency Chain (Sequential)

```
[Sprint 1 develop] ──► PR-01 (enemy-base) ─────────────────────────────────────────────────────────────────────────┐
                    └► PR-02 (player-health) ────────────────────────────────────────────────────────────────────────┤
                                                                                                                     │
PR-01 ──► PR-03 (patrol) ──► PR-11 (detection) ──► PR-ea (attack-state) ──► PR-14 (goblin-ai) ──► PR-15 (spawner) ─┤
PR-01 ──► PR-04 (hitbox)                                                                                             │
PR-01 ──► PR-05 (goblin-sprites) ─────────────────────────────────────────────────────────────────► PR-14          │
                                                                                                                     │
PR-02 ──► PR-07 (health-bar-ui) ──► PR-08 (hit-effects) ──► PR-ea (attack-state)                                  │
PR-01 + PR-02 + PR-04 + PR-09 ──► PR-06 (player-combat) ──► PR-10 (death-respawn) ──► PR-17 (death-screen) ───────┤
                                                                                                                     │
Sprint 1 Audio ──► PR-09 (combat-sfx) ──► PR-sfx-int (sfx-integration)                                            │
Sprint 1 level ──► PR-layout (combat-layout) ──► PR-16 (patrol-points) ──► PR-15 (spawner)                        │
                                                                                                                     │
All above ──────────────────────────────────────────────────────────────────────────────────────────────► PR-18 (integration)
```

---

*Sprint 2 PR Plan | Deserted Echoes | Group 3 | Prepared: April 8, 2026*
