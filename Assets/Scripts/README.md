# Scripts

All C# game scripts for Deserted Echoes.

## Folder Structure

```
Scripts/
├── GameManager.cs          ← Persistent singleton — global game state
├── SceneLoader.cs          ← Static utility — all scene transitions
└── UI/
    ├── MainMenuUI.cs       ← Main menu canvas controller
    └── HUDController.cs    ← In-game HUD (health, stamina, score, lives)
```

## Key Singletons

| Class | Access | Attach To |
|-------|--------|-----------|
| `GameManager` | `GameManager.Instance` | GameObject in main-menu scene |
| `AudioManager` | `AudioManager.Instance` | GameObject in main-menu scene (Rawh) |

## Usage Examples

```csharp
// Load a scene
SceneLoader.LoadScene("level-01");
SceneLoader.ReloadCurrentScene();

// Game state
GameManager.Instance.AddScore(10);
GameManager.Instance.PauseGame();
int lives = GameManager.Instance.playerLives;

// Audio (once Rawh sets it up)
AudioManager.Instance.PlaySFX(coinPickupClip);
```

## Naming Convention
See `CONVENTIONS.md` in the repo root.
