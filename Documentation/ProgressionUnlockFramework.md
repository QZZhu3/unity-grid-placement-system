# Progression & Unlock Framework

## Overview

A ScriptableObject-driven progression and unlock system for a cozy decorative sandbox game. The framework is designed to be fully data-driven, expandable without code changes, and non-restrictive to core creative gameplay.

---

## Architecture

```
ScriptableObjects (data assets)
├── PlaceableItem          — decoration item with category, rarity, seasonal tags, unlock requirement
├── ItemCategory           — unlock unit (e.g. "Water Features", "Stone Paths")
├── DecorationTheme        — groups categories under a named aesthetic (e.g. "Sakura Garden")
├── SeasonTag              — seasonal context tag (e.g. "Spring", "Halloween")
├── UnlockRequirement      — abstract base; subclass for new condition types
│   ├── LevelUnlockRequirement      — satisfied at a player level threshold
│   └── MilestoneUnlockRequirement  — satisfied when a milestone is achieved
└── ProgressionMilestone   — named achievement (e.g. "Place 10 items", "First night visit")

Runtime managers (MonoBehaviours)
├── PlayerProgressionManager  — level, XP, milestone evaluation; fires events
├── UnlockManager             — single source of truth for all unlock state
└── ItemRewardPool            — filters eligible items for reward chests via UnlockManager
```

---

## Data Flow

```
Player places item
  → InventoryPlacementBridge fires OnItemPlaced
    → PlayerProgressionManager.AddXp(amount)
      → level-up check → OnLevelUp fired
      → milestone check → OnMilestoneAchieved fired
        → UnlockManager.EvaluateUnlocks()
          → checks all theme and category UnlockRequirements
            → newly satisfied → OnThemeUnlocked / OnCategoryUnlocked fired
              → UI subscribes → shows unlock notification
              → ItemRewardPool re-queries eligible items
```

---

## Key Design Rules

| Rule | Rationale |
|---|---|
| All unlock checks go through `UnlockManager` | Single source of truth; no duplicated logic |
| `UnlockRequirement` is abstract and polymorphic | New condition types require zero changes to existing code |
| `DecorationTheme` uses `UnlockRequirement`, not a level int | Consistent with all other unlock gates; supports any condition type |
| `ItemCategory` has an optional `parentCategory` | Supports future hierarchical filtering without breaking current logic |
| `PlaceableItem` with no category is always available | Core creativity is never gated |
| Stable `id` fields on all key assets | Save files reference IDs, not asset names — safe to rename assets |
| `SeasonTag` is a ScriptableObject reference, not a string | Refactor-safe; Inspector autocomplete; no typo bugs |

---

## ScriptableObject Assets to Create

### 1. Season Tags
`Assets/Data/Progression/Seasons/`

| Asset name | id | displayName |
|---|---|---|
| `Season_Spring` | `spring` | Spring |
| `Season_Summer` | `summer` | Summer |
| `Season_Autumn` | `autumn` | Autumn |
| `Season_Winter` | `winter` | Winter |
| `Season_LunarNewYear` | `lunar_new_year` | Lunar New Year |
| `Season_Halloween` | `halloween` | Halloween |

---

### 2. Item Categories
`Assets/Data/Progression/Categories/`

| Asset name | id | displayName | parentCategory |
|---|---|---|---|
| `Category_Basics` | `basics` | Garden Basics | — |
| `Category_Paths` | `paths` | Paths & Walkways | `basics` |
| `Category_Water` | `water` | Water Features | — |
| `Category_Lighting` | `lighting` | Lighting | — |
| `Category_SakuraLanterns` | `sakura_lanterns` | Sakura Lanterns | `lighting` |
| `Category_SakuraTrees` | `sakura_trees` | Sakura Trees | — |
| `Category_NightFlowers` | `night_flowers` | Night Flowers | — |
| `Category_NightLanterns` | `night_lanterns` | Night Lanterns | `lighting` |

---

### 3. Unlock Requirements
`Assets/Data/Progression/Requirements/`

| Asset name | Type | Value |
|---|---|---|
| `Req_Level_3` | LevelUnlockRequirement | requiredLevel = 3 |
| `Req_Level_5` | LevelUnlockRequirement | requiredLevel = 5 |
| `Req_Level_8` | LevelUnlockRequirement | requiredLevel = 8 |
| `Req_Level_12` | LevelUnlockRequirement | requiredLevel = 12 |
| `Req_Milestone_FirstGarden` | MilestoneUnlockRequirement | requiredMilestone = `Milestone_FirstGarden` |

---

### 4. Progression Milestones
`Assets/Data/Progression/Milestones/`

| Asset name | id | Condition | requiredLevel |
|---|---|---|---|
| `Milestone_FirstGarden` | `first_garden` | ReachLevel | 3 |
| `Milestone_NightOwl` | `night_owl` | Manual | — |
| `Milestone_WaterLover` | `water_lover` | ReachLevel | 8 |

---

### 5. Decoration Themes
`Assets/Data/Progression/Themes/`

| Asset name | id | displayName | unlockRequirement | categories |
|---|---|---|---|---|
| `Theme_Sakura` | `sakura` | Sakura Garden | `Req_Level_5` | SakuraLanterns, SakuraTrees |
| `Theme_NightGarden` | `night_garden` | Night Garden | `Req_Milestone_FirstGarden` | NightFlowers, NightLanterns |
| `Theme_Coastal` | `coastal` | Coastal Retreat | `Req_Level_12` | (future) |

---

### 6. Standalone Categories (in UnlockManager)

| Category | unlockRequirement |
|---|---|
| `Category_Basics` | null (always available) |
| `Category_Paths` | `Req_Level_3` |
| `Category_Water` | `Req_Level_5` |
| `Category_Lighting` | `Req_Level_3` |

---

## Example Unlock Flow: Sakura Garden

**Setup:**
- `Theme_Sakura` → `unlockRequirement` = `Req_Level_5`
- `Req_Level_5` → `requiredLevel` = 5
- `Theme_Sakura` → `categories` = [SakuraLanterns, SakuraTrees]

**Runtime:**
1. Player places items → gains XP → reaches Level 5
2. `PlayerProgressionManager` fires `OnLevelUp(5)`
3. `UnlockManager.EvaluateUnlocks()` runs
4. `Req_Level_5.IsSatisfied(progression)` → `5 >= 5` → `true`
5. `UnlockManager` fires `OnThemeUnlocked(Theme_Sakura)`
6. `UnlockManager` fires `OnCategoryUnlocked(Category_SakuraLanterns)`
7. `UnlockManager` fires `OnCategoryUnlocked(Category_SakuraTrees)`
8. UI subscribes to `OnThemeUnlocked` → shows "Sakura Garden Unlocked!" notification
9. `ItemRewardPool` re-queries eligible items → Sakura items now appear in reward draws

---

## Example Unlock Flow: Night Garden (Milestone-gated)

**Setup:**
- `Theme_NightGarden` → `unlockRequirement` = `Req_Milestone_FirstGarden`
- `Req_Milestone_FirstGarden` → `requiredMilestone` = `Milestone_FirstGarden`
- `Milestone_FirstGarden` → `conditionType` = ReachLevel, `requiredLevel` = 3

**Runtime:**
1. Player reaches Level 3
2. `PlayerProgressionManager.EvaluateMilestones()` runs
3. `Milestone_FirstGarden.IsConditionMet()` → `3 >= 3` → `true`
4. `PlayerProgressionManager` fires `OnMilestoneAchieved(Milestone_FirstGarden)`
5. `UnlockManager.EvaluateUnlocks()` runs
6. `Req_Milestone_FirstGarden.IsSatisfied()` → `IsMilestoneAchieved(Milestone_FirstGarden)` → `true`
7. Night Garden theme and its categories unlock

---

## Scene Setup

Add these components to a `ProgressionSystem` GameObject in your scene:

1. `PlayerProgressionManager` — configure XP curve and milestone list
2. `UnlockManager` — assign `PlayerProgressionManager`, all themes, and standalone categories
3. `ItemRewardPool` — assign `UnlockManager` and all item assets

**Wiring order matters:** `PlayerProgressionManager` must be on the same GameObject as or found before `UnlockManager` initialises.

---

## Extending the Framework

### Adding a new unlock condition type

1. Create a new script inheriting from `UnlockRequirement`
2. Add `[CreateAssetMenu]` attribute
3. Implement `IsSatisfied(PlayerProgressionManager)` and `GetDescription()`
4. Create an asset and assign it to any theme, category, or item

No changes to `UnlockManager`, `ItemRewardPool`, or any other system required.

### Adding a new category

1. Create a `ItemCategory` asset
2. Optionally assign a `parentCategory` for hierarchy
3. Add it to a `DecorationTheme` or to `UnlockManager.standaloneCategories`
4. Set items' `category` field to the new asset

### Adding a new season

1. Create a `SeasonTag` asset
2. Tag relevant `PlaceableItem` assets with it
3. Call `ItemRewardPool.DrawItem(seasonTag)` during seasonal events

---

## Save / Load Integration

Both `PlayerProgressionManager` and `UnlockManager` expose `LoadState` and `GetSaveState` methods for integration with `SaveManager`.

```csharp
// Save
var (level, xp, milestones) = progression.GetSaveState();
var (categories, themes)    = unlockManager.GetSaveState();

// Load
progression.LoadState(level, xp, milestones);
unlockManager.LoadState(categories, themes);
```

After loading, call `UnlockManager.EvaluateUnlocks()` to ensure any newly satisfied requirements (from XP gained offline, etc.) are processed.
