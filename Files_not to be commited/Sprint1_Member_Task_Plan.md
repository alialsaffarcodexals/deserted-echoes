# Sprint 1 – Member Task Plan
## Deserted Echoes | IT8101 Games Development
**Sprint Duration:** April 8 – 14, 2026
**Sprint Theme:** Foundation — Project Setup, Player Movement & Game Architecture
**Sprint Goal:** Get a playable skeleton running. By the end of this sprint, a player character must move on screen, a camera must follow them, a basic HUD must be visible, and the project must be on Git with everyone contributing.

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

## Member Task Breakdown

---

### 1. Ali Husain Ali Alsaffar — Scrum Master + Project Architecture
**Focus:** Set up the entire project foundation, Git workflow, folder structure, and core singleton systems so all other members can work in a clean, organized environment.

| Task ID | Task | Description | Est. Time |
|---------|------|-------------|-----------|
| S1-AH-01 | Unity Project Initialization | Create the Unity 2D project using Universal Render Pipeline (URP). Unity version: use a stable LTS release (e.g., 2022.3 LTS). Set the game resolution to 1920x1080, 2D mode. | 45 min |
| S1-AH-02 | Folder Structure Setup | Inside `Assets/`, create these subfolders: `Scripts`, `Scenes`, `Prefabs`, `Sprites`, `Animations`, `UI`, `Audio`, `ScriptableObjects`, `Materials`, `Tilemaps`. Each folder gets a `README.md` or stays empty (Unity ignores empty folders — use `.gitkeep`). | 30 min |
| S1-AH-03 | Git Repository Setup | Initialize Git repo. Create `main` and `develop` branches. Add a proper `.gitignore` for Unity (ignore `Library/`, `Temp/`, `obj/`, `.DS_Store`, `*.csproj`, `*.sln`). Push to shared remote (GitHub/GitLab). Have all 6 members clone the repo and confirm they can access it. | 1.5 hrs |
| S1-AH-04 | Naming Conventions Document | Create `CONVENTIONS.md` inside the repo root. Define: PascalCase for C# scripts and classes (`PlayerMovement.cs`), camelCase for variables (`moveSpeed`), kebab-case for scene names (`main-menu`, `level-01`), prefab names match their script (`PlayerPrefab`, `EnemyGoblinPrefab`). | 30 min |
| S1-AH-05 | Unity Package Imports | Import via Package Manager: TextMeshPro (for all UI text), Cinemachine (camera system), 2D Lights (for day/night later), New Input System. Accept TMP Essential Resources popup. | 45 min |
| S1-AH-06 | Scene List Creation | Create empty scenes with correct names: `MainMenu`, `OpenWorldHub`, `Level_01` through `Level_12`, `GameOver`. Save each to `Assets/Scenes/`. Add all scenes to File → Build Settings in the correct order. | 45 min |
| S1-AH-07 | GameManager Singleton | Write `GameManager.cs`: a persistent singleton (`DontDestroyOnLoad`). Fields for now: `currentLevel` (int), `playerLives` (int = 3), `isGamePaused` (bool). Static instance pattern. Place on a `GameManager` GameObject in the `MainMenu` scene. | 1.5 hrs |
| S1-AH-08 | SceneLoader Utility | Write `SceneLoader.cs` with a static method `LoadScene(string sceneName)` that uses `SceneManager.LoadScene`. For now, no fade — just direct load. (Fade will come in Sprint 3.) Place calls in a `SceneLoaderTest` on a button in the test scene to verify it works. | 1 hr |
| S1-AH-09 | Trello Sprint 1 Setup | Populate all Sprint 1 Trello cards with: assignee, due date, and a brief description. Create a "In Progress" label (yellow) and a "Done" label (green) for visual tracking. | 45 min |

**Sprint 1 Deliverable:** A clean, organized Unity project on Git with correct folder structure, all scenes created, GameManager singleton working, and all team members successfully cloned and pushed at least one commit.

---

### 2. Ali Abdulla — Player Movement System + Input System
**Focus:** Full top-down player movement — responsive, polished, and built on Unity's new Input System from day one so it's easy to rebind later.

| Task ID | Task | Description | Est. Time |
|---------|------|-------------|-----------|
| S1-AA-01 | New Input System Setup | In Package Manager, confirm Input System is installed. Go to Edit → Project Settings → Player → Active Input Handling → set to "Input System Package (New)". Create an `InputActions` asset (`Assets/Scripts/PlayerInputActions.inputactions`). Define an Action Map called `Gameplay` with actions: `Move` (2D Vector composite — WASD), `Sprint` (Button — Left Shift), `Dodge` (Button — Space), `Interact` (Button — E), `Attack` (Button — Left Mouse), `Inventory` (Button — Tab), `Pause` (Button — Escape). | 1.5 hrs |
| S1-AA-02 | Player Prefab Base | Create a `Player` GameObject: Sprite Renderer (placeholder colored square or imported character sprite), Rigidbody2D (Gravity Scale = 0 for top-down, Freeze Rotation Z = true), Capsule Collider 2D. Tag it `Player`. Layer: `Player`. Save as `Assets/Prefabs/PlayerPrefab.prefab`. | 45 min |
| S1-AA-03 | PlayerMovement Script | Write `PlayerMovement.cs`. Read `Move` action value each `FixedUpdate`. Normalize the input vector (prevents diagonal speed boost). Apply via `rb.linearVelocity = moveInput * moveSpeed`. Expose `moveSpeed` (default 5f) in Inspector. | 1.5 hrs |
| S1-AA-04 | Sprint Mechanic | In `PlayerMovement.cs`, when `Sprint` action is held AND `currentStamina > 0` (connect to a `PlayerStats` stub for now — just use a local float), multiply `moveSpeed` by `sprintMultiplier` (default 1.6f). Add `isSprinting` bool flag. | 1 hr |
| S1-AA-05 | Player Facing Direction | When the player moves, flip the SpriteRenderer on the X axis based on horizontal input: `sr.flipX = moveInput.x < 0`. For top-down all-direction movement: optionally rotate the player transform to face the move direction using `Quaternion.LookRotation(Vector3.forward, moveInput)` — pick one approach and document it in code comments. | 45 min |
| S1-AA-06 | Movement Boundary / Collision Test | Ensure the player can't pass through wall colliders. In the test level, add some Box Collider 2D walls and confirm the player stops against them correctly. No physics issues (jitter, tunneling). | 30 min |
| S1-AA-07 | Input Actions Integration Verification | Run the game. Confirm: WASD moves the player in all 8 directions. Shift speeds them up. Movement feels smooth and responsive. Write a brief note in the code about what `Time.fixedDeltaTime` is used for (good practice). | 30 min |

**Sprint 1 Deliverable:** The player character moves in all directions with WASD, sprints with Shift, stops at walls, and the sprite flips correctly based on movement direction. All input goes through the new Input System.

---

### 3. Faisal Alasfoor — Main Menu UI + Basic HUD Framework
**Focus:** Build the Main Menu scene and a placeholder HUD Canvas so the team has a UI structure to build on in later sprints.

| Task ID | Task | Description | Est. Time |
|---------|------|-------------|-----------|
| S1-FA-01 | Main Menu Scene Setup | Open the `MainMenu` scene. Add a Canvas (Screen Space — Overlay, reference resolution 1920×1080, Scale With Screen Size). Add a background Image (dark desert color or black for now). | 45 min |
| S1-FA-02 | Game Title Text | Add a TextMeshPro text element: "DESERTED ECHOES". Font size ~80, bold. Position at top-center. Color: sandy gold (hex #D4AC0D or similar). Add a subtle text shadow using the TMP shadow component. | 30 min |
| S1-FA-03 | Main Menu Buttons | Create 5 buttons using TMP buttons: Start Game, Instructions, Credits, Settings, Quit. Each button: consistent size (e.g., 300×60), spaced evenly, left-aligned or center. Use TextMeshPro for button text. Highlight color on hover. | 1.5 hrs |
| S1-FA-04 | Button Functionality | Wire up buttons: Start Game → `SceneLoader.LoadScene("Level_01")` (or `OpenWorldHub` later). Instructions → show a placeholder panel with "Instructions coming soon." Credits → show a placeholder panel. Settings → show a placeholder panel. Quit → `Application.Quit()` (and `UnityEditor.EditorApplication.isPlaying = false` for editor). | 1.5 hrs |
| S1-FA-05 | HUD Canvas (Gameplay Scene) | In the test level scene (coordinate with Rawh), create a separate Canvas for HUD (also Screen Space Overlay). Add: a placeholder health bar (UI Slider, red fill), a placeholder stamina bar (UI Slider, green fill), a gold coin icon + TextMeshPro text "0", a Lives display "♥ 3". Anchor everything correctly so it works at different resolutions. | 2 hrs |
| S1-FA-06 | Game Over Screen Placeholder | Create a `GameOver` scene (it exists from Ali Husain's scene list). Add: "GAME OVER" TMP text (large, red), a "Retry" button that calls `SceneLoader.LoadScene("Level_01")`, a "Main Menu" button. | 1 hr |
| S1-FA-07 | UI Style Guide | Document the UI color palette and fonts chosen in a comment at the top of each UI script, or in a `UI_StyleGuide.md` file. Include: primary color (gold), background color (dark), accent color, chosen TMP font. This ensures consistency for later sprints. | 30 min |

**Sprint 1 Deliverable:** A working Main Menu with 5 functional buttons. A HUD Canvas in the test level with placeholder health bar, stamina bar, gold counter, and lives display. A Game Over placeholder scene.

---

### 4. Habib Husain Alshoofa — Player Prefab, Animator & Animation Setup
**Focus:** Set up the player's Animator Controller with all necessary animation states so the rest of the team can hook into them. Also set up the player prefab as a complete object.

| Task ID | Task | Description | Est. Time |
|---------|------|-------------|-----------|
| S1-HH-01 | Player Sprite Sheet Import | Import the player character sprite sheet (Lucian Blaze). In Unity Inspector: Texture Type = Sprite (2D and UI), Sprite Mode = Multiple, Pixels Per Unit = 32 (or match sprite size), Filter Mode = Point (no filter) for pixel art, Compression = None. Use Sprite Editor to slice into individual frames (auto-slice by grid if evenly spaced). | 1.5 hrs |
| S1-HH-02 | Animation Clips Creation | Create animation clips (`.anim` files) in `Assets/Animations/Player/`: `Player_Idle.anim` (looping), `Player_Walk.anim` (looping), `Player_Run.anim` (looping), `Player_Attack.anim` (NOT looping), `Player_Hurt.anim` (NOT looping), `Player_Death.anim` (NOT looping). Drag the correct sprite frames from the sliced sprite sheet into each clip in the Animation window. | 2.5 hrs |
| S1-HH-03 | Animator Controller | Create `PlayerAnimator.controller` in `Assets/Animations/`. Add all 6 animation states. Set `Player_Idle` as the default state. Create transitions: Any State → `Player_Death` (trigger: `Die`), `Idle` ↔ `Walk` (float: `speed`, threshold 0.1), `Walk` ↔ `Run` (float: `speed`, threshold 3.0), `Idle/Walk/Run` → `Attack` (trigger: `Attack`), → `Hurt` (trigger: `Hurt`). Assign this controller to the Animator component on the Player prefab. | 2 hrs |
| S1-HH-04 | PlayerAnimationController Script | Write `PlayerAnimationController.cs`. In `Update()`, read `moveSpeed` from `PlayerMovement` component and set `animator.SetFloat("speed", speed)`. This makes walk/run transitions automatic. Expose public methods: `TriggerAttack()`, `TriggerHurt()`, `TriggerDeath()` for other scripts to call. | 1.5 hrs |
| S1-HH-05 | Player Prefab Assembly | On the Player prefab, confirm all components are present and configured: Rigidbody2D, Capsule Collider 2D (correct size matching sprite), Sprite Renderer, Animator (with controller assigned), `PlayerMovement.cs`, `PlayerAnimationController.cs`. Assign the Animator reference in the Inspector. Test the animation transitions work by pressing play and moving. | 1 hr |
| S1-HH-06 | PlayerStats Stub | Write `PlayerStats.cs` (stub for Sprint 1): public fields `maxHealth = 100f`, `currentHealth = 100f`, `maxStamina = 100f`, `currentStamina = 100f`. These will be fully expanded in Sprint 3, but creating the stub now means `PlayerMovement.cs` can reference `currentStamina` for the sprint mechanic. | 45 min |

**Sprint 1 Deliverable:** The player character has correctly sliced sprite sheets, all 6 animation states created, an Animator Controller with working transitions, and a `PlayerStats` stub so the movement system has stamina to reference.

---

### 5. Khizar Azhar — Camera System + Basic Collectible Prototype
**Focus:** A smooth-following camera using Cinemachine, plus an early prototype of the collectible system (which is Khizar's GDD custom feature) so the concept is validated.

| Task ID | Task | Description | Est. Time |
|---------|------|-------------|-----------|
| S1-KA-01 | Cinemachine Virtual Camera | In the test level scene, add a Cinemachine Virtual Camera. Set `Follow` target to the Player transform. Set `Look At` to None (top-down — we don't need to look at anything). Body: Framing Transposer. Set `Lookahead Time` to 0, `Damping` to 0.5 on X and Y (smooth follow). `Dead Zone Width/Height`: 0 (camera stays centered). Orthographic size: 6 (adjust to show enough level). | 1 hr |
| S1-KA-02 | Camera Zoom Test | In the Cinemachine brain settings, ensure Blend Style is set to Cut or Ease In Out. Test that the camera follows the player smoothly and never shows artifacts. Test at different `Orthographic Size` values (5, 6, 7) and pick the best fit for the test level. Document the chosen value in a comment. | 30 min |
| S1-KA-03 | Camera Confiner (Optional) | If Rawh's test level has defined boundaries, add a `CinemachineConfiner2D` component to the virtual camera. Create a composite collider as the boundary. This prevents the camera from showing areas outside the level. Mark as "nice to have — implement if time allows." | 1 hr |
| S1-KA-04 | Collectible System Prototype | Create `Collectible.cs`: in `OnTriggerEnter2D(Collider2D other)`, check if `other.CompareTag("Player")`. If true: play a pickup sound (placeholder), destroy `gameObject`. For now just log "Collected!" to Console. This is the foundation of Khizar's GDD custom feature. | 1 hr |
| S1-KA-05 | Collectible Prefab | Create a `CoinCollectible` prefab: Circle Sprite (yellow), CircleCollider2D (IsTrigger = true), `Collectible.cs` attached. Place 5 of them in the test level scattered around. | 45 min |
| S1-KA-06 | ICollectible Interface | Create `ICollectible.cs` interface with method `void Collect(GameObject collector)`. Have `Collectible.cs` implement it. This is clean architecture — other collectible types (water, food) will implement the same interface. | 30 min |
| S1-KA-07 | Camera Follow Verification | Run the game and verify: camera follows player at all times with smooth damping. Player moving to edges of test level: camera stops at confiner boundary (if implemented). No camera jitter. | 30 min |

**Sprint 1 Deliverable:** Cinemachine camera smoothly follows the player. A working collectible prototype where coins in the level get destroyed on player touch. `ICollectible` interface exists for future extension.

---

### 6. Rawh Hasan — Test Level Scene + Basic Lighting + Audio Setup Foundation
**Focus:** Build the test level where all Sprint 1 systems are tested together. Also lay the audio foundation since audio (AudioManager) is needed from early sprints for proper sound integration.

| Task ID | Task | Description | Est. Time |
|---------|------|-------------|-----------|
| S1-RH-01 | Test Level Scene | Open `Level_01` scene (or create a separate `TestLevel` scene). Add a basic floor: use Unity Tilemap or a series of flat Sprite objects for the ground. Size: roughly 30×20 units. This is the "sandbox" for Sprint 1 testing. | 1.5 hrs |
| S1-RH-02 | Level Boundaries (Walls) | Add Box Collider 2D invisible walls around the perimeter of the level so the player can't walk off the edge. Or add visible wall sprites. Layer: `Ground`. Add a few internal walls/obstacles to give the player something to navigate around. | 1 hr |
| S1-RH-03 | Level Spawn Point | Create an empty GameObject called `SpawnPoint` at the center-left of the level. This is where the player will instantiate. In a `LevelManager.cs` stub, on `Start()`, instantiate the `PlayerPrefab` at `spawnPoint.position`. Assign the PlayerPrefab reference in Inspector. | 1 hr |
| S1-RH-04 | Basic 2D Lighting Setup | In the URP 2D Renderer settings, enable 2D lighting. Add a `Global Light 2D` to the scene (Intensity: 1, Color: warm white for daytime). This sets up the lighting system that Ali Abdulla will animate in Sprint 3. Keep it simple for now — just make sure it looks like a bright desert day. | 45 min |
| S1-RH-05 | Environment Props Placeholder | Add a few simple props to make the level look less empty: sand-colored background sprites, a rock or two, simple colored rectangles representing structures. This is aesthetic — just enough so the level doesn't feel completely empty during testing. | 1 hr |
| S1-RH-06 | AudioManager Singleton | Write `AudioManager.cs`: a persistent singleton (`DontDestroyOnLoad`). Two `AudioSource` components: one for music (loop = true), one for SFX (loop = false). Methods: `PlayMusic(AudioClip clip)`, `PlaySFX(AudioClip clip)`, `SetMusicVolume(float vol)`, `SetSFXVolume(float vol)`. Store volumes in `PlayerPrefs` so they persist between sessions. Place on an `AudioManager` GameObject in `MainMenu` scene. | 2 hrs |
| S1-RH-07 | Placeholder Audio Clips | Find or create very basic placeholder audio (even just sine-wave beeps from Bfxr/sfxr or free CC0 sounds): a coin pickup sound, a background ambient track (even silence or very quiet wind). Assign them in the AudioManager for testing. | 1 hr |
| S1-RH-08 | Integration Test Run | After all Sprint 1 members have pushed their work to `develop`, pull the branch and run the scene. Confirm: player spawns, moves, camera follows, HUD is visible, coins can be collected, AudioManager doesn't throw errors. Document any issues found in Trello as bug cards. | 1 hr |

**Sprint 1 Deliverable:** A functional test level with spawn point, boundaries, basic props, and 2D lighting. `AudioManager` singleton is live and other members can call `AudioManager.Instance.PlaySFX(clip)` from anywhere. Integration test passes.

---

## Sprint 1 Summary Table

| Member | Primary Focus | Est. Hours |
|--------|--------------|------------|
| Ali Husain | Project setup, Git, GameManager, scene architecture | ~7.5h |
| Ali Abdulla | Player movement, Input System, sprint mechanic | ~6.5h |
| Faisal Alasfoor | Main Menu UI, HUD framework, Game Over screen | ~7.5h |
| Habib Husain | Player sprite setup, Animator, animation clips | ~9h |
| Khizar Azhar | Cinemachine camera, collectible prototype | ~5h |
| Rawh Hasan | Test level, lighting, AudioManager singleton | ~8.5h |

---

## Integration Points (Sprint 1)

| From | To | What |
|------|----|------|
| Ali Husain (SceneLoader) | Faisal (Buttons) | `SceneLoader.LoadScene()` wired to menu buttons |
| Ali Husain (GameManager) | All | Singleton accessible from any script via `GameManager.Instance` |
| Habib (PlayerPrefab) | Ali Abdulla (Movement) | Movement scripts live on the same prefab |
| Habib (PlayerStats stub) | Ali Abdulla (Sprint) | `currentStamina` float read in PlayerMovement |
| Khizar (Cinemachine) | Rawh (Level) | Camera `Follow` target = Player spawned by LevelManager |
| Rawh (AudioManager) | All | All members can call `AudioManager.Instance.PlaySFX()` |
| Rawh (SpawnPoint/LevelManager) | Habib (PlayerPrefab) | PlayerPrefab assigned in LevelManager Inspector |

---

## Definition of Done (Sprint 1)

- [ ] Unity project pushed to Git — all 6 members have made at least 1 commit
- [ ] Folder structure exists and is clean (no loose files in root)
- [ ] Player moves in all 8 directions with WASD, sprint with Shift
- [ ] Camera smoothly follows player in the test level
- [ ] Player has walk/run/idle animations that transition correctly
- [ ] Main Menu loads and all 5 buttons are functional (or show placeholder)
- [ ] HUD Canvas shows health bar, stamina bar, gold counter, lives
- [ ] AudioManager exists and can be called without errors
- [ ] Coins exist in the level and disappear on player touch
- [ ] All Trello Sprint 1 cards updated with results

---

*Prepared by: Ali Husain (Scrum Master) | Date: April 8, 2026*
