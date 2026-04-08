# Deserted Echoes — Project Documentation
## IT8101 Games Development | Group 3

This branch (`docs`) contains all planning and sprint documentation for the **Deserted Echoes** game project.

> All `.md` files are read-only references. Game code lives on the `develop` and `feature/*` branches.

---

## Team

| # | Name | Student ID | GDD Custom Feature |
|---|------|-----------|-------------------|
| 1 | Ali Husain Ali Alsaffar | 201900576 | Teleport / Fast Travel System *(Scrum Master)* |
| 2 | Ali Abdulla | 202300917 | Dynamic Night Visibility System + UI/Settings/Saving |
| 3 | Faisal Alasfoor | 202304774 | Difficulty System |
| 4 | Habib Husain Alshoofa | 202100747 | Player Profile & Inventory System |
| 5 | Khizar Azhar | 202200123 | Collectible & Equipment System |
| 6 | Rawh Hasan | 202301152 | Dynamic Weather System |

---

## Document Index

### Project-Wide
| Document | Description |
|----------|-------------|
| [Full Task List](Project/Full_Task_List.md) | All 200+ tasks across 19 categories — the master backlog for the entire project |

---

### Sprint 1 (April 8 – 14, 2026)
**Theme:** Foundation — Project Setup, Player Movement & Game Architecture

| Document | Description |
|----------|-------------|
| [Member Task Plan](Sprint1/Member_Task_Plan.md) | Per-member task breakdown with IDs, descriptions, and estimated hours |
| [PR Plan](Sprint1/PR_Plan.md) | 13 pull requests — wave schedule, dependencies, timeline, and PR template |

---

### Sprint 2 (April 15 – 21, 2026)
**Theme:** Core Combat, Enemy AI & Player Health Systems

| Document | Description |
|----------|-------------|
| [Member Task Plan](Sprint2/Member_Task_Plan.md) | Per-member task breakdown for combat and enemy AI systems |
| [PR Plan](Sprint2/PR_Plan.md) | 18 pull requests — dual Wave 0 blockers, dependency chain, full timeline |

---

### Sprint 3 (April 22 – 28, 2026)
**Theme:** Survival Systems, Environmental Hazards & Custom Feature Foundations

| Document | Description |
|----------|-------------|
| [Member Task Plan](Sprint3/Member_Task_Plan.md) | Per-member task breakdown for survival systems and custom GDD features |
| [PR Plan](Sprint3/PR_Plan.md) | 24 pull requests — four concurrent Wave 0 blockers, full dependency map |

---

## Sprint Roadmap

| Sprint | Dates | Theme |
|--------|-------|-------|
| Sprint 1 | Apr 8 – 14 | Foundation, player movement, project setup |
| Sprint 2 | Apr 15 – 21 | Combat, enemy AI, player health |
| Sprint 3 | Apr 22 – 28 | Survival systems, day/night, weather, inventory |
| Sprint 4 | Apr 29 – May 5 | Loot, XP, level design, more enemies |
| Sprint 5 | May 6 – 12 | Advanced enemies, ranged combat, equipment |
| Sprint 6 | May 13 – 19 | Mini-bosses, hunger/thirst, advanced UI |
| Sprint 7 | May 20 – 26 | Polish, bug fixing, audio, VFX |
| Sprint 8 | May 27 – Jun 2 | Final boss, balancing, release candidate |

---

## Git Branch Strategy

```
main          ← stable release — never pushed to directly
  └── develop ← integration branch — all feature PRs merge here
        └── feature/s[N]-[feature-name] ← one branch per PR
```

**Rules:**
- All PRs target `develop`, never `main`
- Branch from latest `develop` before starting
- At least 1 team member must approve before merge
- PR title format: `[SN] Short description — MemberName`

---

*Prepared by: Ali Husain (Scrum Master) | April 8, 2026*
