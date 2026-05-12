# Ambient Task Journal System Guide

The Ambient Task Journal is a low-friction, immersive productivity interface designed to feel like a magical garden journal rather than a traditional checklist app. It supports ambient visibility, intentional interaction, and calming focus transitions.

## 1. Core Architecture & Scripts

The system is broken down into modular components that strictly separate UI presentation from game logic and reward distribution.

*   **`JournalState.cs`**: Enum defining the three core states (`Hidden`, `Peek`, `Pinned`).
*   **`AmbientTaskJournalController.cs`**: The central state machine. Handles input (hover, click) and transitions between states.
*   **`TaskJournalPanel.cs`**: Presentation-only script. Handles the smooth sliding and fading of the UI panel using easing curves.
*   **`TaskRowUI.cs`**: Represents a single task row. Adapts its layout based on the current `JournalState`.
*   **`HoldCompleteInteraction.cs`**: Replaces standard checkbox clicks with a cozy "hold-to-complete" mechanic, reducing accidental completions.
*   **`JournalBlurController.cs`**: Manages the soft background darkening and optional Depth of Field blur when the journal is pinned.

## 2. Setup Instructions

The fastest way to set up the journal is using the included Editor tool:

1.  In Unity, go to the top menu: **Tools → Placement System → Build Ambient Task Journal UI**.
2.  This will automatically generate the `AmbientJournalRoot` and `JournalDarkenOverlay` in your Canvas, with all components attached and wired.
3.  **Manual Step (Event Triggers):** To enable hover detection for Peek mode:
    *   Select `AmbientJournalRoot/PeekZone` in the Hierarchy.
    *   Add an **Event Trigger** component.
    *   Add a **Pointer Enter** event and wire it to `AmbientTaskJournalController.OnPeekZoneEnter`.
    *   Add a **Pointer Exit** event and wire it to `AmbientTaskJournalController.OnPeekZoneExit`.
    *   Repeat this process for `AmbientJournalRoot/JournalPanel`, wiring to `OnJournalEnter` and `OnJournalExit`.

## 3. State Flow

The journal operates on a clean state machine managed by `AmbientTaskJournalController`:

1.  **Hidden**: Fully off-screen. `JournalBlurController` is inactive.
2.  **Peek**: Triggered when the mouse hovers over the `PeekZone` (left edge of screen) for `0.25s`. The panel partially slides in with reduced opacity. Only lightweight task info (icon, title, tiny progress) is shown.
3.  **Pinned**: Triggered when the player clicks the journal during Peek mode. The panel fully expands, opacity becomes 100%, and `JournalBlurController` activates to darken/blur the background. Full task details and the Hold-to-Complete interaction become available. Clicking outside the journal returns it to Hidden.

## 4. Blur Implementation Recommendation

The `JournalBlurController` automatically handles a full-screen darkening overlay. For the best "magical garden" feel, it is highly recommended to also use Unity's Post Processing stack:

1.  Ensure your project uses the Universal Render Pipeline (URP) or High Definition Render Pipeline (HDRP).
2.  Create a **Global Volume** in your scene.
3.  Add a **Depth of Field** override to the volume.
4.  Assign this Global Volume to the `Blur Volume` field on the `JournalBlurController`.
5.  When the journal is Pinned, the controller will smoothly animate the volume's weight, creating a soft, dreamy focus effect on the background world without a harsh modal interruption.

## 5. Mobile Adaptation Notes

The architecture is built to support future mobile adaptations with minimal changes. `AmbientTaskJournalController` includes hooks for mobile input:

*   **`OnMobileEdgeSwipe()`**: Call this when an edge swipe is detected to transition from Hidden to Peek.
*   **`OnMobileTap()`**: Call this when a tap on the Peek view is detected to transition to Pinned.

The `HoldCompleteInteraction` uses standard Unity UI pointer events (`IPointerDownHandler`, `IPointerUpHandler`), which natively support mobile touch input without modification.

## 6. Integration with RewardManager

To maintain the architectural rule that "no reward logic lives in UI scripts," the journal integrates exclusively through the central `ActivityManager`:

1.  When a task is completed via the `HoldCompleteInteraction`, `TaskRowUI` catches the event.
2.  `TaskRowUI` calls `activityManager.CompleteActivity(currentTask)`.
3.  `ActivityManager` (already built in the Focus Session update) receives the signal and routes it to `RewardManager.CompleteTask()`, applying any XP multipliers or chest progress ticks defined in the task's `ActivityRewardConfig`.

This ensures all rewards, UI popups, and analytics flow through a single, predictable pipeline.
