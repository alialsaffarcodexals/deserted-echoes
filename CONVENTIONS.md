# Deserted Echoes — Code & Git Conventions

> **All team members must follow these conventions without exception.**
> Inconsistent naming causes merge conflicts and confusion. When in doubt, check here first.

---

## 1. C# Script Naming

| Element | Convention | Example |
|---------|-----------|---------|
| Script filename | PascalCase | `PlayerMovement.cs` |
| Class name | Matches filename | `class PlayerMovement` |
| Method names | PascalCase | `void TakeDamage()` |
| Private / local variables | camelCase | `float moveSpeed` |
| Constants | ALL_CAPS_SNAKE | `const int MAX_HEALTH = 100` |
| Public fields / properties | PascalCase | `public float MoveSpeed` |
| Boolean names | Prefix with `is` / `has` | `bool isAlive`, `bool hasKey` |
| Interfaces | Prefix with `I` | `ICollectible`, `IDamageable` |

**Use `[SerializeField]` for private Inspector-exposed fields — NOT `public`.**
```csharp
[SerializeField] private float moveSpeed = 5f;   // ✓ correct
public float moveSpeed = 5f;                       // ✗ avoid
```

Use `[Header("Section Name")]` to group Inspector fields for readability.

---

## 2. Scene Naming

- **Format:** kebab-case
- **Location:** `Assets/Scenes/`

| Scene | Filename |
|-------|---------|
| Main Menu | `main-menu.unity` |
| Open World Hub | `open-world-hub.unity` |
| Test Level | `test-level.unity` |
| Levels | `level-01.unity` … `level-12.unity` |
| Game Over | `game-over.unity` |

---

## 3. Prefab Naming

- **Format:** PascalCase + `Prefab` suffix
- **Location:** `Assets/Prefabs/[Category]/`

```
PlayerPrefab.prefab
EnemyGoblinPrefab.prefab
CoinCollectiblePrefab.prefab
DesertRockPrefab.prefab
```

---

## 4. Asset Folder Naming

- Top-level folders: PascalCase (`Characters`, `Scripts`, `Audio`)
- Character sub-folders: descriptive only — **NO source URLs or craftpix IDs**

```
✓  LucianBlaze_Level1-3/
✓  EnemyGoblin/
✓  BossGolem/
✗  craftpix-net-710530-goblin-pixel-art-character-sprite-pack/
```

---

## 5. Animation Naming

Format: `[Character]_[State].anim`

```
Player_Idle.anim       Player_Walk.anim
Player_Run.anim        Player_Attack.anim
Player_Hurt.anim       Player_Death.anim
Enemy_Idle.anim        Enemy_Attack.anim
```

---

## 6. Git Branch Naming

| Type | Format | Example |
|------|--------|---------|
| New feature | `feature/[description]` | `feature/player-movement` |
| Bug fix | `fix/[description]` | `fix/camera-jitter` |
| Sprint milestone | `sprint-N` | `sprint-1` |

**Rules:**
- Never commit directly to `main`
- `main` is updated only at sprint-end via PR from `develop`
- All feature work goes on a `feature/*` branch → PR into `develop`
- Delete feature branches after merging

---

## 7. Git Commit Messages

**Format:** `type: short description` (under 72 characters)

| Type | When to use |
|------|------------|
| `feat` | New feature added |
| `fix` | Bug fixed |
| `chore` | Setup, config, structure, no logic change |
| `docs` | Documentation only |
| `refactor` | Code restructured, behaviour unchanged |
| `test` | Test files only |

**Examples:**
```
feat: add PlayerMovement script with WASD input
fix: resolve camera jitter at level boundary
chore: rename craftpix folders to meaningful names
docs: add CONVENTIONS.md with naming and git standards
```

**Rules:**
- Present tense, imperative mood ("add" not "added")
- Lowercase after the colon
- No period at the end
- No "Co-Authored-By: Claude Code" lines

---

## 8. Layers & Tags

**Layers:** `Player` · `Enemy` · `Ground` · `Collectible` · `UI`

**Tags:** `Player` · `Enemy` · `Collectible` · `NPC` · `Interactable`

---

## 9. Script File Header Template

```csharp
// ─────────────────────────────────────────────────────────────
// [ClassName].cs
// Deserted Echoes | IT8101 Games Development | Group 3
// Author: [Your Name]
// Sprint: [N] | Created: [Date]
// Description: [One-line summary of what this script does]
// ─────────────────────────────────────────────────────────────
```

---

*Last updated: April 9, 2026 — Ali Husain Ali Alsaffar (Scrum Master)*
