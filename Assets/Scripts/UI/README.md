# UI Scripts

Scripts for all canvas and UI logic in Deserted Echoes.

## Files

| Script | Attach To | Owner |
|--------|-----------|-------|
| `MainMenuUI.cs` | MainMenuCanvas (in main-menu scene) | Faisal Alasfoor |
| `HUDController.cs` | HUDCanvas (in test-level scene) | Faisal Alasfoor |

## Setup Instructions for Faisal

### Main Menu Scene
1. Create a **Canvas** → Screen Space Overlay → Reference resolution 1920×1080 → Scale With Screen Size
2. Name it `MainMenuCanvas`
3. Attach `MainMenuUI.cs` to it
4. Create 5 Button children: **Start Game**, **Instructions**, **Credits**, **Settings**, **Quit**
5. Wire each button's `OnClick()` to the corresponding method on `MainMenuUI`:
   - Start Game → `OnStartGame()`
   - Instructions → `OnInstructions()`
   - Credits → `OnCredits()`
   - Settings → `OnSettings()`
   - Quit → `OnQuit()`
6. Create 3 Panel children (hidden by default): `InstructionsPanel`, `CreditsPanel`, `SettingsPanel`
7. Assign the panels to the script's Inspector fields

### HUD Canvas (test-level scene)
1. Create a separate **Canvas** → Screen Space Overlay
2. Name it `HUDCanvas`
3. Attach `HUDController.cs` to it
4. Add: health Slider (red fill), stamina Slider (green fill), TMP score text, TMP lives text
5. Assign all 4 UI elements to the script's Inspector fields
6. Call `HUDController.UpdateHealth(value)` and `UpdateStamina(value)` from `PlayerStats.cs`
