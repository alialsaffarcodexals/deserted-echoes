# Deserted Echoes

> **IT8101 Games Development — Group 3 | Polytechnic Bahrain**
> 2D pixel-art survival, exploration, and action game built in Unity 6.

---

## Table of Contents

- [Game Overview](#game-overview)
- [Story](#story)
- [Player Controls](#player-controls)
- [Gameplay Systems](#gameplay-systems)
- [Enemy Types](#enemy-types)
- [Level Structure](#level-structure)
- [Difficulty & Lives System](#difficulty--lives-system)
- [Custom Features](#custom-features)
- [Tech Stack](#tech-stack)
- [Team & Contributions](#team--contributions)

---

## Game Overview

**Deserted Echoes** is a 2D pixel-art survival and exploration game set in a hostile desert environment. Players take on the role of **Lucian Blaze (LB)**, a survivor navigating extreme conditions while searching for his kidnapped friends. The game blends survival mechanics, open-world exploration, and real-time combat.

| Detail | Info |
|---|---|
| Genre | 2D Pixel Art Survival / Exploration / Action |
| Engine | Unity 6 (C#) |
| Platform | PC |
| Target Audience | Age 12+, casual to intermediate players |
| Estimated Playtime | 4–6 hours total (15–25 min per level) |

---

## Story

Lucian Blaze is stranded in a vast desert after a **bandit ambush**. His friends have been kidnapped and scattered across regions controlled by desert raiders. With nothing but his will to survive, Lucian must cross the desert, battle through enemy-held territory, and rescue everyone — one level at a time.

**Progression arc:**
- **Early Game** — Basic survival, resource gathering, introduction to enemies
- **Mid Game** — Environmental hazards, increased difficulty, mini-boss encounters
- **Late Game** — Rescue missions, stronger enemies, narrative development
- **Final Stage** — Final boss confrontation and story resolution

---

## Player Controls

| Key | Action |
|---|---|
| W / A / S / D | Movement |
| Shift | Sprint |
| Space | Dodge |
| E | Interact |
| Tab | Inventory |
| M | Map |
| 1 – 4 | Item Slots |
| ESC | Pause |
| Left Click | Attack |
| Right Click | Secondary Action |
| Mouse Movement | Aim |

---

## Gameplay Systems

### Core Loop
Players continuously **explore** environments, **gather resources**, **manage survival stats**, **engage in combat**, and **progress through levels**.

### Survival Stats
- **Health** — depleted by combat and environmental hazards
- **Stamina** — consumed by sprinting and dodging; recovers over time
- **Temperature** — affected by the day/night cycle and weather (heat/cold danger zones)

### Combat
- Melee and ranged attacks
- Enemy AI with varied behavior per mob type
- Player self-damage prevention and hitbox-accurate collision
- Combat SFX for all 10 mob types and 6 boss types

### Inventory & Equipment
- Item pickup and chest system
- Equipment slots with armor stat bonuses
- Resource collection and management
- Hotbar (slots 1–4) for quick item access

### Score & XP
- Kill-based score system wired to HUD
- XP system tied to player progression

### Save / Load
- Auto-save at checkpoints and key events
- Manual save/load functionality
- Player stats and progress persistence across sessions

---

## Enemy Types

All enemies come in **Easy / Normal / Hard** variants.

| Enemy | Location | Notes |
|---|---|---|
| Skeletons | Dungeons / Caves | Sword-wielding undead warriors |
| Liches | Dungeons / Caves | Sorcerer skeletons with staff projectiles |
| Zombies | Dungeons / Caves / Wilderness | Acid-spitting hard variant |
| Orcs | Dungeons / Caves | Heavy melee fighters |
| Imps | Dungeons / Caves | Flying; hard variant shoots fireballs |
| Goblins | Dungeons / Caves / Wilderness | Spear and sword fighters |
| Rats | Dungeons / Caves / Wilderness | Hard variant infects the player |
| Vampires *(boss)* | Dungeons / Caves | Blood magic on hard variant |
| The Eye *(boss)* | Dungeons / Caves | Laser beams and fireball attacks |
| Demons *(boss)* | Dungeons / Caves | Bident-wielding hellspawn |
| Golems *(boss)* | Dungeons / Caves | Elemental; hard variant breathes fire |
| Lizards *(boss)* | Dungeons / Caves | Hard variant spits fire |

---

## Level Structure

- **12 Levels** + an **Open World Hub**
- The hub has **6 tunnels**, each connecting to 2 levels via portal doors
- Completed levels are marked **"Done"** — revisitable for resource gathering
- End-of-level return portal brings the player back to the hub

| Levels | Designer |
|---|---|
| Level 01 & 02 | Ali Alsaffar |
| Level 03 & 04 | Faisal Alasfoor |
| Level 05 & 06 | Rawh Hasan |
| Level 07 & 08 | Habib Alshoofa |
| Level 09 & 10 | Khizar Azhar |
| Level 11 & 12 (Final Boss) | Ali Abdulla |
| Open World Hub | Rawh Hasan |

---

## Difficulty & Lives System

| Difficulty | Lives per Level | Special Rule |
|---|---|---|
| Easy | 5 | — |
| Normal | 3 | — |
| Hard | 1 | Optional permadeath mode |

- Dying with lives remaining → respawn at last checkpoint
- Losing all lives → restart level, checkpoints reset
- Lives fully restored on level completion
- HUD displays remaining lives next to HP and Stamina

---

## Custom Features

Each team member implemented one custom feature as part of the GDD requirements:

| Feature | Owner | Description |
|---|---|---|
| Dynamic Night Visibility System | Ali Abdulla | Limited visibility at night; flashlight-based vision cone |
| Collectible & Equipment System | Khizar Azhar | Armor stat bonuses, resource pickups, chest loot |
| Teleport / Fast Travel System | Ali Alsaffar | Open-world hub portals with teleport selection panel |
| Difficulty System | Faisal Alasfoor | Easy / Normal / Hard scaling across enemies, lives, and stats |
| Player Profile & Inventory | Habib Alshoofa | Stats tracking, inventory UI, progress management |
| Dynamic Weather System | Rawh Hasan | Sandstorms, day/night cycle, movement and visibility impact |
| UI / Settings / Saving System | Ali Abdulla | Settings menu, audio sliders, auto-save, save profiles |

---

## Tech Stack

| Component | Detail |
|---|---|
| Engine | Unity 6 |
| Language | C# |
| Art Style | 2D Pixel Art |
| Version Control | Git / GitHub |
| Project Management | Scrum (Trello board) |
| Audio | Unity Audio Mixer (music + SFX channels) |

---

## Team & Contributions

### Ali Husain Ali Alsaffar — *Scrum Master / Project Manager*
**GDD Feature:** Teleport / Fast Travel System

- Unity project setup and repository management
- Levels 01 & 02 map design and enhancement
- Pause menu system
- Settings panel UI (Apply / Close pattern + SFX)
- Teleport system — open-world portal gates and teleport selection panel
- Full map system + minimap + fog of war
- Background music integration and audio mixer routing
- UI sound effects for all menus and panels
- Sand footstep SFX (Levels 01, 02, Open World)
- Health bar / death animation wiring (Level 01)
- Stamina bar, sprint mechanic, and exhaustion cooldown (Level 01)
- Score system — HUD wiring and enemy kill score
- Level 01 & 02 enemy and HUD setup via Editor scripts
- CC0 enemy SFX — all 10 mobs and 6 bosses
- Combat SFX — 54 prefabs for player attack, mob & boss

---

### Ali Abdulla — *Developer*
**GDD Feature:** Dynamic Night Visibility System + UI / Settings / Saving

- Player movement, animation, and camera system
- Enemy AI system
- Player combat system (melee + projectile)
- Player hurt & death animation integration
- Enemy prefabs and animations (mobs and bosses)
- Imp and Lich throwable projectile system
- Fixed player self-damage and enemy collision
- Mini-boss encounters
- Enemy expansion across levels
- Campfire animation
- Level 11 & 12 design (including final boss level)

---

### Faisal Alasfoor — *Developer*
**GDD Feature:** Difficulty System

- Main menu UI
- Game over screen UI
- Difficulty system implementation (Easy / Normal / Hard scaling)
- Level 03 and Level 04 map design and scene implementation

---

### Habib Husain Alshoofa — *Developer*
**GDD Feature:** Player Profile & Inventory System

- Sprite splitting and asset preparation
- HUD controller
- Health and damage system
- Inventory system UI
- Player stats persistence
- Stamina system
- XP system
- Level 07 and Level 08 design
- Player Stats UI — Canvas inventory integration

---

### Khizar Azhar — *Developer*
**GDD Feature:** Collectible & Equipment System

- Simple collectible system (Sprint 1)
- Equipment system
- Chest-based collectible system
- Death and respawn system
- Level 09 and Level 10 map design

---

### Rawh Hasan — *Developer*
**GDD Feature:** Dynamic Weather System

- Test level and initial map design
- Open World Hub map design
- House interior map design
- Level 05 and Level 06 map design
- Day / night cycle system
- Storm system (sandstorms, wind)
- Lighting system — first version
- Dialogue system

---

*Viva Voce: May 29, 2026 | Game Submission: May 24, 2026 | IT8101 — 35% of course grade*
