# Sprint 1 — Pull Request Plan
## Deserted Echoes | IT8101 Games Development
**Sprint Duration:** April 8 – 14, 2026
**Sprint Theme:** Foundation & Project Setup

---

## Git Branching Strategy

```
main          ← stable, never pushed to directly
  └── develop ← integration branch, all PRs merge here
        ├── feature/s1-project-setup        (Ali Husain)
        ├── feature/s1-game-manager         (Ali Husain)
        ├── feature/s1-input-system         (Ali Abdulla)
        ├── feature/s1-player-movement      (Ali Abdulla)
        ├── feature/s1-main-menu            (Faisal)
        ├── feature/s1-hud-framework        (Faisal)
        ├── feature/s1-player-animations    (Habib)
        ├── feature/s1-player-prefab        (Habib)
        ├── feature/s1-camera-system        (Khizar)
        ├── feature/s1-collectible-proto    (Khizar)
        ├── feature/s1-audio-manager        (Rawh)
        ├── feature/s1-test-level           (Rawh)
        └── feature/s1-level-manager        (Rawh)
```

**Rules:**
- All PRs target `develop` — never `main`
- Branch from `develop` when starting, pull latest `develop` before branching
- At least 1 team member must review and approve before merge
- Resolve all merge conflicts locally before requesting review
- PR title format: `[S1] Short description — MemberName`

---

## PR Wave Overview (Concurrent vs Sequential)

```
DAY 1 (Apr 8)   DAY 2 (Apr 9)    DAY 3 (Apr 10)   DAY 4 (Apr 11)   DAY 5-6 (Apr 12-13)  DAY 7 (Apr 14)
──────────────  ───────────────  ────────────────  ────────────────  ────────────────────  ──────────────
 WAVE 0          WAVE 1           WAVE 2            WAVE 3            WAVE 4                WAVE 5
 [BLOCKING]      [CONCURRENT]     [CONCURRENT]      [CONCURRENT]      [CONCURRENT]          [INTEGRATION]

 PR-01           PR-02            PR-04             PR-07             PR-10                 PR-13
 project-setup   game-manager     player-movement   player-prefab     camera-system         develop→main
                 PR-03            PR-05             PR-08             PR-11                 (end-of-sprint)
                 input-system     main-menu         hud-framework     level-manager
                                  PR-06             PR-09             PR-12
                                  player-anims      audio-manager     game-over-screen
                                                    collectible-proto
```

> **Wave 0 is a blocker.** PR-01 (`project-setup`) must merge before anyone can push meaningful code.
> All other waves are concurrent within themselves but sequential to the wave before them.

---

## Wave 0 — BLOCKING (Day 1, Apr 8)
> ⚠️ This PR must merge before the rest of the team can start their feature branches.

### PR-01 | `feature/s1-project-setup`
**Owner:** Ali Husain Ali Alsaffar
**Branch:** `feature/s1-project-setup` → `develop`
**Open:** Apr 8 (morning) | **Target Merge:** Apr 8 (end of day)
**Reviewer:** Any 1 member (quick review — structure only, no logic)

**What this PR contains:**
- Unity 2D URP project initialized (correct version)
- Full folder structure: `Assets/Scripts`, `Scenes`, `Prefabs`, `Sprites`, `Animations`, `UI`, `Audio`, `ScriptableObjects`, `Materials`, `Tilemaps`
- `.gitignore` configured for Unity (`Library/`, `Temp/`, `obj/`, `*.csproj`, `*.sln`, `.DS_Store`)
- `CONVENTIONS.md` at repo root with naming rules
- All 14 empty scenes created and added to Build Settings
- All Unity packages imported: TextMeshPro, Cinemachine, New Input System, 2D Lights, URP

**Depends on:** Nothing — this is the foundation
**Blocks:** PR-02, PR-03, PR-04, PR-05, PR-06, PR-07, PR-09, PR-12 (everything)

---

## Wave 1 — CONCURRENT (Day 2, Apr 9)
> All 3 PRs in this wave can be opened and reviewed simultaneously.

### PR-02 | `feature/s1-game-manager`
**Owner:** Ali Husain Ali Alsaffar
**Branch:** `feature/s1-game-manager` → `develop`
**Open:** Apr 9 | **Target Merge:** Apr 9
**Reviewer:** Ali Abdulla

**What this PR contains:**
- `GameManager.cs` — persistent singleton (`DontDestroyOnLoad`), fields: `currentLevel (int)`, `playerLives (int = 3)`, `isGamePaused (bool)`, static `Instance` pattern
- `SceneLoader.cs` — static utility with `LoadScene(string sceneName)` using `SceneManager.LoadSceneAsync`

**Depends on:** PR-01 ✓
**Blocks:** PR-05 (Faisal needs SceneLoader for button wiring), PR-08 (Habib's HUD needs GameManager)

---

### PR-03 | `feature/s1-input-system`
**Owner:** Ali Abdulla
**Branch:** `feature/s1-input-system` → `develop`
**Open:** Apr 9 | **Target Merge:** Apr 9
**Reviewer:** Habib Husain (he needs this to assemble the player prefab)

**What this PR contains:**
- `PlayerInputActions.inputactions` asset in `Assets/Scripts/`
- Action Map `Gameplay` defined: Move (2D Vector/WASD), Sprint (Left Shift), Dodge (Space), Interact (E), Attack (Left Mouse), Inventory (Tab), Pause (ESC)
- Project Settings → Active Input Handling set to New Input System (screenshot in PR description confirming this)

**Depends on:** PR-01 ✓
**Blocks:** PR-04 (Ali Abdulla's movement uses this), PR-07 (Habib's prefab assembly needs input confirmed)

---

### PR-06 | `feature/s1-player-animations`
**Owner:** Habib Husain Alshoofa
**Branch:** `feature/s1-player-animations` → `develop`
**Open:** Apr 9 | **Target Merge:** Apr 10
**Reviewer:** Ali Husain (check animation clip naming follows conventions)

**What this PR contains:**
- Player sprite sheet imported with correct settings (Point filter, No compression, Multiple mode, PPU=32)
- Sprite Editor used to slice frames by grid cell size
- 6 `.anim` clips in `Assets/Animations/Player/`: `Player_Idle` (loop), `Player_Walk` (loop), `Player_Run` (loop), `Player_Attack` (no loop), `Player_Hurt` (no loop), `Player_Death` (no loop)
- `PlayerAnimator.controller` created with all 6 states, transitions set up:
  - `Idle ↔ Walk` (float `speed`, threshold 0.1)
  - `Walk ↔ Run` (float `speed`, threshold 3.0)
  - `Any State → Death` (trigger `Die`)
  - `Any State → Attack` (trigger `Attack`)
  - `Any State → Hurt` (trigger `Hurt`)
- `PlayerAnimationController.cs` — reads `PlayerMovement.moveSpeed`, sets `speed` float; exposes `TriggerAttack()`, `TriggerHurt()`, `TriggerDeath()`

**Depends on:** PR-01 ✓
**Blocks:** PR-07 (Habib's prefab assembly — animations must exist first)

---

## Wave 2 — CONCURRENT (Day 3, Apr 10)
> All PRs in this wave can be opened simultaneously, but each requires its Wave 1 dependency to have merged.

### PR-04 | `feature/s1-player-movement`
**Owner:** Ali Abdulla
**Branch:** `feature/s1-player-movement` → `develop`
**Open:** Apr 10 | **Target Merge:** Apr 11
**Reviewer:** Habib Husain

**What this PR contains:**
- `PlayerMovement.cs` — reads `Move` action via `PlayerInput` component, normalizes input, applies `rb.linearVelocity = moveInput * moveSpeed` in `FixedUpdate`
- Sprint: `Shift` held + `PlayerStats.currentStamina > 0` → multiply by `sprintMultiplier (1.6f)`, `isSprinting` bool
- Sprite flip: `spriteRenderer.flipX = moveInput.x < 0`
- `PlayerPrefab` object created with: `Rigidbody2D` (gravity=0, freeze Z), `Capsule Collider 2D`, `SpriteRenderer`, `PlayerInput` component referencing the InputActions asset

**Depends on:** PR-01 ✓, PR-03 ✓ (Input System must be merged and confirmed working)
**Blocks:** PR-07 (Habib needs PlayerMovement on the prefab to assemble it), PR-10 (Khizar's camera needs a player in scene)

---

### PR-05 | `feature/s1-main-menu`
**Owner:** Faisal Alasfoor
**Branch:** `feature/s1-main-menu` → `develop`
**Open:** Apr 10 | **Target Merge:** Apr 11
**Reviewer:** Ali Husain

**What this PR contains:**
- `MainMenu` scene built: Canvas (Screen Space Overlay, 1920×1080, Scale With Screen Size)
- "DESERTED ECHOES" TMP title — gold color `#D4AC0D`, size 80, text shadow
- 5 TMP buttons: Start Game, Instructions, Credits, Settings, Quit
- Button wiring: Start → `SceneLoader.LoadScene("Level_01")`, Quit → `Application.Quit()`, others → placeholder info panels
- Placeholder panels for Instructions, Credits, Settings (TMP text "Coming soon")
- UI Style Guide comment header in each UI script (colors, font, sizes)

**Depends on:** PR-01 ✓, PR-02 ✓ (SceneLoader must exist)
**Blocks:** Nothing critical — independent UI work continues in PR-08

---

### PR-09 | `feature/s1-audio-manager`
**Owner:** Rawh Hasan
**Branch:** `feature/s1-audio-manager` → `develop`
**Open:** Apr 10 | **Target Merge:** Apr 11
**Reviewer:** Faisal Alasfoor

**What this PR contains:**
- `AudioManager.cs` — persistent singleton (`DontDestroyOnLoad`), 2 `AudioSource` components on the same GameObject (one for music loop, one for SFX one-shot)
- Methods: `PlayMusic(AudioClip clip)`, `PlaySFX(AudioClip clip)`, `SetMusicVolume(float vol)`, `SetSFXVolume(float vol)`
- Volumes stored in `PlayerPrefs` ("MusicVolume", "SFXVolume"), loaded on `Awake()`
- `AudioManager` GameObject placed in `MainMenu` scene (persists across scene loads)
- 2 placeholder audio clips imported: `ambient_wind_loop.wav`, `sfx_coin_pickup.wav` in `Assets/Audio/`

**Depends on:** PR-01 ✓
**Blocks:** PR-12 (Rawh's LevelManager calls PlayMusic on load), all future SFX calls from other members

---

### PR-collectible | `feature/s1-collectible-proto`
**Owner:** Khizar Azhar
**Branch:** `feature/s1-collectible-proto` → `develop`
**Open:** Apr 10 | **Target Merge:** Apr 11
**Reviewer:** Rawh Hasan (he places them in the level)

**What this PR contains:**
- `ICollectible.cs` interface: `void Collect(GameObject collector)`
- `Collectible.cs` — `OnTriggerEnter2D`: check `other.CompareTag("Player")` → call `Collect()` → `Destroy(gameObject)` → `AudioManager.Instance.PlaySFX(pickupSFX)`
- `CoinCollectible.prefab` — yellow circle sprite, `CircleCollider2D` (IsTrigger=true), `Collectible.cs` attached
- 5 coin prefabs pre-placed in the `Level_01` scene at different positions

**Depends on:** PR-01 ✓, PR-09 ✓ (needs AudioManager for pickup sound)
**Blocks:** Nothing critical this sprint

---

## Wave 3 — CONCURRENT (Day 4, Apr 11)
> All PRs in this wave can open simultaneously after their Wave 2 dependencies merge.

### PR-07 | `feature/s1-player-prefab`
**Owner:** Habib Husain Alshoofa
**Branch:** `feature/s1-player-prefab` → `develop`
**Open:** Apr 11 | **Target Merge:** Apr 12
**Reviewer:** Ali Abdulla (he wrote the movement script — verifies the prefab works)

**What this PR contains:**
- `PlayerStats.cs` stub — `maxHealth=100f`, `currentHealth=100f`, `maxStamina=100f`, `currentStamina=100f` (public fields, will be expanded Sprint 3)
- `PlayerPrefab.prefab` fully assembled in `Assets/Prefabs/`:
  - `Rigidbody2D`, `Capsule Collider 2D`, `SpriteRenderer` (player sprite assigned)
  - `Animator` (PlayerAnimator controller assigned)
  - `PlayerInput` (PlayerInputActions asset assigned)
  - `PlayerMovement.cs` (from Ali Abdulla's PR-04)
  - `PlayerAnimationController.cs`, `PlayerStats.cs`
  - All Inspector references connected
- Animation transitions verified working in Play mode (Idle↔Walk↔Run)

**Depends on:** PR-04 ✓ (PlayerMovement), PR-06 ✓ (Animator + animation clips)
**Blocks:** PR-10 (Khizar needs the player in scene for camera target), PR-12 (Rawh's LevelManager spawns this prefab)

---

### PR-08 | `feature/s1-hud-framework`
**Owner:** Faisal Alasfoor
**Branch:** `feature/s1-hud-framework` → `develop`
**Open:** Apr 11 | **Target Merge:** Apr 12
**Reviewer:** Habib Husain (he owns PlayerStats — HUD references it)

**What this PR contains:**
- HUD Canvas added to `Level_01` scene (Screen Space Overlay, separate from world)
- Health bar: UI Slider (red fill, dark red background, border image), anchored top-left
- Stamina bar: UI Slider (green fill), anchored below health bar
- Gold display: coin icon + TMP text "0", anchored bottom-left
- Lives display: "♥ 3" TMP text, anchored top-right (3 heart icons if sprite available)
- `GameOver` scene complete: "GAME OVER" red heading, score placeholder, Retry + Main Menu buttons wired to SceneLoader

**Depends on:** PR-02 ✓ (SceneLoader for Game Over buttons), PR-05 ✓ (UI style consistency from Main Menu)
**Blocks:** Nothing — independent HUD scaffold

---

## Wave 4 — CONCURRENT (Day 5-6, Apr 12-13)
> Final feature PRs — depend on the player prefab being in the scene.

### PR-10 | `feature/s1-camera-system`
**Owner:** Khizar Azhar
**Branch:** `feature/s1-camera-system` → `develop`
**Open:** Apr 12 | **Target Merge:** Apr 13
**Reviewer:** Rawh Hasan (has the level scene — verifies camera fits the level)

**What this PR contains:**
- `Cinemachine Virtual Camera` added to `Level_01` scene
- Follow target: Player transform (assigned after LevelManager spawns player — use `CinemachineBrain` auto-find or assign at runtime in `LevelManager`)
- Body: Framing Transposer — Damping X/Y = 0.5, Dead Zone = 0, Ortho Size = 6
- `CinemachineConfiner2D` component with composite collider boundary matching level walls
- Camera verified: smooth follow, no jitter, stays within level boundary

**Depends on:** PR-07 ✓ (PlayerPrefab must exist to be the follow target), PR-12 needs to confirm player is spawned
**Blocks:** PR-13 (integration — needs camera working)

---

### PR-11 | `feature/s1-game-over-scene`
**Owner:** Faisal Alasfoor
**Branch:** `feature/s1-game-over-screen` → `develop`
**Open:** Apr 12 | **Target Merge:** Apr 13
**Reviewer:** Ali Husain

**What this PR contains:**
- `GameOver` scene finalized (separate from PR-08 cleanup)
- "YOU DIED" large TMP heading (red, bold, size 72)
- Placeholder stats section: "Enemies Killed: 0 | Level: 1"
- "Retry" button → `SceneLoader.LoadScene(GameManager.Instance.currentLevel)`
- "Main Menu" button → `SceneLoader.LoadScene("MainMenu")`
- Dark background with slight desert-themed overlay image

**Depends on:** PR-02 ✓, PR-08 ✓

---

### PR-12 | `feature/s1-level-manager`
**Owner:** Rawh Hasan
**Branch:** `feature/s1-level-manager` → `develop`
**Open:** Apr 12 | **Target Merge:** Apr 13
**Reviewer:** Khizar Azhar (camera target depends on this spawning the player)

**What this PR contains:**
- `LevelManager.cs` — on `Start()`: `Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity)`, then calls `AudioManager.Instance.PlayMusic(levelMusic)`
- `SpawnPoint` empty GameObject in `Level_01` at center-left
- Test level expansion: floor tilemap/sprites (~30×20 units), perimeter collider walls, 3-4 internal rock/box obstacles, basic 2D URP Global Light (warm white, intensity=1)
- Basic desert props for atmosphere
- Level verified playable: player spawns, moves, coins collectible, audio plays

**Depends on:** PR-07 ✓ (PlayerPrefab), PR-09 ✓ (AudioManager for PlayMusic call)
**Blocks:** PR-10 (camera needs player spawned in the level)

---

## Wave 5 — INTEGRATION (Day 7, Apr 14)
### PR-13 | Sprint 1 Integration Test → `develop`
**Owner:** Ali Husain (Scrum Master — initiates, all members review)
**Type:** Not a code PR — a final verification pass on `develop`

**Checklist before Sprint 1 closes:**
- [ ] All feature branches merged into `develop`
- [ ] Pull latest `develop` locally and run the game — no console errors on startup
- [ ] Player spawns in level, moves in 8 directions, sprints with Shift
- [ ] Walk / Run / Idle animation transitions correctly
- [ ] Camera smoothly follows player, stays within level bounds
- [ ] Main Menu loads, all 5 buttons respond correctly
- [ ] HUD visible: health bar, stamina bar, gold counter, lives display
- [ ] Coins in level disappear on player touch and play pickup sound
- [ ] `AudioManager.Instance` accessible from any script without NullReference
- [ ] No merge conflicts or leftover feature branches

> If issues are found: the member who owns the broken system opens a `fix/s1-[issue]` branch, fixes, and PRs immediately.

---

## Full Sprint 1 PR Timeline

| PR | Branch | Owner | Opens | Merges | Concurrent With | Blocked By |
|----|--------|-------|-------|--------|-----------------|------------|
| PR-01 | `s1-project-setup` | Ali Husain | Apr 8 AM | Apr 8 EOD | — | — |
| PR-02 | `s1-game-manager` | Ali Husain | Apr 9 | Apr 9 | PR-03, PR-06 | PR-01 |
| PR-03 | `s1-input-system` | Ali Abdulla | Apr 9 | Apr 9 | PR-02, PR-06 | PR-01 |
| PR-06 | `s1-player-animations` | Habib | Apr 9 | Apr 10 | PR-02, PR-03 | PR-01 |
| PR-04 | `s1-player-movement` | Ali Abdulla | Apr 10 | Apr 11 | PR-05, PR-09, PR-coll | PR-03 |
| PR-05 | `s1-main-menu` | Faisal | Apr 10 | Apr 11 | PR-04, PR-09, PR-coll | PR-02 |
| PR-09 | `s1-audio-manager` | Rawh | Apr 10 | Apr 11 | PR-04, PR-05, PR-coll | PR-01 |
| PR-coll | `s1-collectible-proto` | Khizar | Apr 10 | Apr 11 | PR-04, PR-05, PR-09 | PR-09 |
| PR-07 | `s1-player-prefab` | Habib | Apr 11 | Apr 12 | PR-08 | PR-04, PR-06 |
| PR-08 | `s1-hud-framework` | Faisal | Apr 11 | Apr 12 | PR-07 | PR-02, PR-05 |
| PR-10 | `s1-camera-system` | Khizar | Apr 12 | Apr 13 | PR-11, PR-12 | PR-07, PR-12 |
| PR-11 | `s1-game-over-screen` | Faisal | Apr 12 | Apr 13 | PR-10, PR-12 | PR-02, PR-08 |
| PR-12 | `s1-level-manager` | Rawh | Apr 12 | Apr 13 | PR-10, PR-11 | PR-07, PR-09 |
| PR-13 | Integration Check | Ali Husain | Apr 14 | Apr 14 | — | All above |

---

## Dependency Chain (Sequential — must follow this order)

```
PR-01 (project-setup)
  ├──► PR-02 (game-manager)
  │      └──► PR-05 (main-menu)
  │              └──► PR-08 (hud-framework)
  │                      └──► PR-11 (game-over-screen)
  │
  ├──► PR-03 (input-system)
  │      └──► PR-04 (player-movement)
  │              └──► PR-07 (player-prefab)  ◄── also needs PR-06
  │                      ├──► PR-10 (camera-system)  ◄── also needs PR-12
  │                      └──► PR-12 (level-manager)  ◄── also needs PR-09
  │
  ├──► PR-06 (player-animations) ──────────► PR-07 (player-prefab)
  │
  └──► PR-09 (audio-manager)
         ├──► PR-coll (collectible-proto)
         └──► PR-12 (level-manager)

All above ──► PR-13 (integration check)
```

---

## PR Description Template (copy for each PR)

```markdown
## Summary
Brief description of what this PR adds.

## Changes
- File 1: what was added/changed
- File 2: what was added/changed

## How to Test
1. Step to test this feature
2. Expected result

## Dependencies
- Requires: PR-XX merged ✓
- Blocks: PR-XX

## Checklist
- [ ] Code follows CONVENTIONS.md naming rules
- [ ] No console errors in Play mode
- [ ] Changes committed to correct feature branch
- [ ] Trello card updated
```

---

*Sprint 1 PR Plan | Deserted Echoes | Group 3 | Prepared: April 8, 2026*
