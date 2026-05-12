# Productivity Garden: Architecture & Development Guidelines

This document outlines the strict architectural principles governing the Productivity Garden project. All future code, scripts, and systems must adhere to these guidelines to ensure a clean, maintainable, and scalable codebase.

## 1. Strict Separation of Concerns

Each script should have a single, well-defined responsibility. Do not mix data management, user input, visual presentation, and game state logic within the same file.

*   **Data Models**: Should hold state and configuration (e.g., `ActivityDefinition`, `SaveData`).
*   **Controllers/Runners**: Should handle logic, timing, and state transitions (e.g., `FocusSessionRunner`, `AmbientTaskJournalController`).
*   **Presentation (UI)**: Should only handle visual updates and animations (e.g., `TaskJournalPanel`, `FocusSessionUI`).

## 2. Avoid Monolithic Manager Scripts

Do not create "god classes" (e.g., `GameManager`, `UIManager`) that know about and control everything in the scene.

*   Break down large systems into smaller, focused managers.
*   For example, instead of a single `ProgressionManager`, the project uses distinct components: `RewardManager` (grants XP/chests), `ActivityManager` (routes activity completions), and `ChestQueueManager` (handles chest queuing).
*   Use interfaces or specific dependency injection rather than having every script reference a global singleton.

## 3. Use Event-Driven Communication Where Appropriate

Systems should communicate state changes via events (C# `event Action` or `UnityEvent`) rather than tightly coupling components through direct method calls.

*   **Example**: `GameInputState.OnInputBlockChanged` notifies listeners when input is blocked, rather than `GameInputState` directly telling the player controller to stop moving.
*   **Example**: `ActivityManager.OnActivityCompleted` allows the UI to react to task completions without the `ActivityManager` needing a reference to the UI.
*   **Benefit**: This allows new features (like analytics, sound effects, or achievements) to be added simply by subscribing to existing events, without modifying the core logic.

## 4. Keep UI Presentation Logic Separate from Gameplay/Reward Logic

UI scripts must never dictate game rules, grant rewards, or modify core progression data.

*   **UI Scripts** (e.g., `TaskRowUI`, `ChestOpeningPanel`) are responsible for animating, updating text, filling progress bars, and capturing clicks.
*   When a user interacts with the UI (e.g., holding a button to complete a task), the UI script must pass a signal to a logic controller (e.g., `ActivityManager.CompleteActivity()`).
*   The logic controller then validates the action and routes it to the appropriate system (e.g., `RewardManager`).
*   **Rule of Thumb**: If you are calling `rewardManager.AddXP()` or `SaveSystem.Save()` from inside a script that also manipulates a `CanvasGroup` or `TextMeshProUGUI`, you are violating this principle.

## 5. Prefer Modular Systems with Clear Ownership Boundaries

Systems should be self-contained and modular, capable of being tested or modified without cascading side effects.

*   **Dependency Injection**: Expose dependencies via the Inspector (`[SerializeField]`) rather than relying heavily on `FindObjectOfType()`. If auto-discovery is used (e.g., in `Awake()`), it should be a fallback, not the primary architecture.
*   **Prefabs & ScriptableObjects**: Use ScriptableObjects for configuration data (e.g., `FocusSessionDefinition`) so designers can create new content without touching code. Use prefabs for UI elements to ensure consistency.
*   **Clear Ownership**: Understand which script "owns" a piece of data. For example, `RewardManager` owns the player's XP and Level; no other script should directly modify `progressionData.xp`.

## Summary Checklist for New Features

Before submitting new code, verify:
- [ ] Does this script do only one thing?
- [ ] Is this script free of "manager-of-everything" tendencies?
- [ ] Does it use events to notify other systems instead of calling them directly?
- [ ] Is the UI purely visual, with no reward or progression logic?
- [ ] Are dependencies explicitly defined and boundaries respected?
