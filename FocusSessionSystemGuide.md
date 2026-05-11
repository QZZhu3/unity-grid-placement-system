# Focus Session System Guide

## Overview

The Focus Session framework is a modular, scalable activity system for the cozy productivity decorating game. It connects real-world productivity habits (timed focus sessions) to the in-game reward loop without introducing stressful gameplay, fail states, or competitive mechanics.

All rewards flow through `RewardManager` — no reward logic lives inside UI or activity scripts.

---

## Architecture

```
Player taps "Start Session"
        ↓
FocusSessionUI.OnStartClicked()
        ↓
FocusSessionRunner.StartSession(definition)
        ↓
  [Timer counts down]
        ↓
FocusSessionRunner.CompleteSession()
        ↓
ActivityManager.CompleteActivity(definition)
        ↓
RewardManager.CompleteTask() × chestProgressTicks
        ↓
XP granted + chest progress ticked
        ↓
(if chest earned) ChestQueueManager enqueues chest
        ↓
ChestNotificationButton appears
```

### Class Responsibilities

| Class | Responsibility |
|-------|---------------|
| `ActivityType` | Enum: ChecklistTask, FocusSession, ReflectionTask, MicroInteraction |
| `ActivityDefinition` | ScriptableObject base: id, displayName, activityType, rewardConfig |
| `ActivityRewardConfig` | Serializable: xpMultiplier, chestProgressTicks, sourceTag |
| `FocusSessionDefinition` | Extends ActivityDefinition: durationMinutes, ambientType, streakHook |
| `AmbientInteractionType` | Enum: None, DriftingLeaves, WateringFlowers, LightingLanterns, FireflyInteraction |
| `ActivityManager` | Routes CompleteActivity() → RewardManager. Fires events. |
| `FocusSessionRunner` | Timer state machine: Idle → Running → Paused → Completed/Cancelled |
| `FocusSessionUI` | Presentation only: timer display, buttons, completion popup |
| `FocusSaveHandler` | ISaveable: persists active session via GameSaveData.focusSessionData |
| `FocusSessionSaveData` | Serializable snapshot: definitionId, remainingSeconds, isPaused, streak placeholders |

---

## Folder Structure

```
Assets/PlacementSystem/
├── Scripts/
│   ├── Activity/
│   │   ├── ActivityType.cs
│   │   ├── ActivityDefinition.cs
│   │   ├── ActivityRewardConfig.cs
│   │   ├── AmbientInteractionType.cs
│   │   ├── FocusSessionDefinition.cs
│   │   ├── FocusSessionRunner.cs
│   │   ├── FocusSaveHandler.cs
│   │   └── FocusSessionSaveData.cs
│   └── UI/
│       └── FocusSessionUI.cs
├── ScriptableObjects/
│   └── Activities/
│       └── FocusSession_Standard.asset   ← create this
└── FocusSessionSystemGuide.md
```

---

## Scene Setup

### Step 1 — Add components to ProgressionSystem

Select the `ProgressionSystem` GameObject and add:

1. **ActivityManager** — Add Component → Activity Manager
2. **FocusSessionRunner** — Add Component → Focus Session Runner
3. **FocusSaveHandler** — Add Component → Focus Save Handler

Leave all dependency fields empty — they auto-discover via `FindAnyObjectByType`.

### Step 2 — Create a FocusSessionDefinition asset

1. Right-click in the Project window → **Create → Productivity Garden → Activity → Focus Session Definition**
2. Name it `FocusSession_Standard`
3. Configure:
   - **Id**: `focus_session_standard`
   - **Display Name**: `Standard Focus Session`
   - **Activity Type**: FocusSession
   - **Duration Minutes**: 25
   - **Break Minutes**: 5
   - **Ambient Type**: DriftingLeaves
   - **Show Ambient Interaction**: checked
   - **Counts Toward Streak**: checked
   - **Reward Config**:
     - XP Multiplier: 1
     - Chest Progress Ticks: 1
     - Source Tag: `focus_session`

### Step 3 — Add FocusSaveHandler definition list

Select `ProgressionSystem` → `FocusSaveHandler` component → drag `FocusSession_Standard` into the **All Definitions** array. Repeat for any future definitions.

### Step 4 — Add focusSessionData to GameSaveData

Open `Scripts/SaveSystem/SaveData.cs` and add one field to `GameSaveData`:

```csharp
/// <summary>Active focus session state (null if no session is active).</summary>
public FocusSessionSaveData focusSessionData = null;
```

### Step 5 — Register FocusSaveHandler with SaveManager

Open `Scripts/SaveSystem/SaveManager.cs` and ensure it discovers all `ISaveable` components automatically, or manually add `FocusSaveHandler` to its list.

### Step 6 — Build the UI hierarchy

In the Canvas, create a `FocusSessionPanel` with this child structure:

```
FocusSessionPanel
├── SessionNameText    (TextMeshProUGUI) — e.g. "Standard Focus Session"
├── TimerText          (TextMeshProUGUI) — e.g. "25:00"
├── ProgressBar        (Slider, optional)
├── StartButton        (Button + TMP Label "Start")
├── PauseResumeButton  (Button + TMP Label "Pause", inactive by default)
├── CancelButton       (Button + TMP Label "Cancel", inactive by default)
└── CompletionPopup    (GameObject, inactive by default)
    ├── CompletionText (TextMeshProUGUI)
    └── DismissButton  (Button + TMP Label "OK")
```

Attach `FocusSessionUI` to `FocusSessionPanel` and wire all fields in the Inspector.

---

## Save / Load Integration

`FocusSaveHandler` implements `ISaveable` and hooks into the existing save pipeline:

- **PopulateSaveData**: calls `FocusSessionRunner.GetSaveData()` → stores result in `data.focusSessionData`
- **LoadFromSaveData**: reads `data.focusSessionData` → calls `FocusSessionRunner.LoadFromSaveData()` with a resolver function

If no session is active at save time, `focusSessionData` is `null` and nothing is restored on load.

---

## Event Flow

| Event | Fired by | Listened by |
|-------|----------|-------------|
| `OnSessionStarted` | FocusSessionRunner | FocusSessionUI |
| `OnSessionPaused` | FocusSessionRunner | FocusSessionUI |
| `OnSessionResumed` | FocusSessionRunner | FocusSessionUI |
| `OnSessionCompleted` | FocusSessionRunner | FocusSessionUI, ActivityManager |
| `OnSessionCancelled` | FocusSessionRunner | FocusSessionUI |
| `OnTimerTick` | FocusSessionRunner | FocusSessionUI (updates display) |
| `OnActivityCompleted` | ActivityManager | (analytics, future systems) |
| `OnFocusSessionCompleted` | ActivityManager | (streak system, future) |

---

## Future Extension Points

The framework is designed for easy expansion:

### Adding a new ActivityType

1. Add a value to `ActivityType` enum.
2. Create a new ScriptableObject that extends `ActivityDefinition`.
3. Create a new Runner MonoBehaviour (similar to `FocusSessionRunner`).
4. Call `ActivityManager.CompleteActivity(definition)` on completion.
5. No changes needed to `RewardManager`, `ChestProgressManager`, or save system.

### Streak System

`FocusSessionSaveData` already contains placeholder fields:
- `lastCompletedDateUtc` — date of last completed session
- `currentStreak` — consecutive days count

Implement a `StreakManager` that subscribes to `ActivityManager.OnFocusSessionCompleted` and updates these fields.

### Daily Goals

Subscribe to `ActivityManager.OnActivityCompleted` and count completions per day. Store in a new `DailyGoalSaveData` field in `GameSaveData`.

### Seasonal Focus Events

Add a `seasonTag` field to `FocusSessionDefinition` (matching the existing `SeasonTag` ScriptableObject pattern). Pass it as `sourceTag` in `ActivityRewardConfig` so `RewardFilterPipeline` can apply seasonal reward bonuses.

### Productivity Analytics

Subscribe to `ActivityManager.OnActivityCompleted` and log to a local analytics buffer. Each `ActivityDefinition` already carries an `id` and `activityType` for categorization.

### Ambient Interaction Controllers

Create a MonoBehaviour for each `AmbientInteractionType`:

```csharp
public class DriftingLeavesController : MonoBehaviour
{
    public void Activate() { /* start particle system */ }
    public void Deactivate() { /* stop particle system */ }
}
```

`FocusSessionUI` can activate/deactivate the appropriate controller based on `definition.AmbientType` when a session starts.

---

## XP Multiplier Note

`ActivityRewardConfig.xpMultiplier` is defined but not yet fully implemented. `RewardManager.CompleteTask()` currently grants a fixed XP amount. To support per-activity multipliers, add an overload:

```csharp
public void CompleteTask(float xpMultiplier = 1f)
{
    if (progressionManager != null && xpPerTask > 0f)
        progressionManager.AddXp(xpPerTask * xpMultiplier);
    // ... rest of method
}
```

Then update `ActivityManager.GrantRewards()` to pass `config.xpMultiplier`.
