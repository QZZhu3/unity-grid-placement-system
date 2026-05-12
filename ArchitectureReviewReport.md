# Architecture Review Report
**Author:** Manus AI  
**Date:** May 13, 2026  

This report provides a comprehensive audit of the Productivity Garden codebase, evaluating scripts against the newly established architectural principles [1]. The goal is to identify systems with overlapping responsibilities, direct coupling risks, and potential "god objects," offering prioritised recommendations without unnecessarily rewriting working systems.

## 1. Executive Summary

Overall, the codebase demonstrates a reasonable separation between core progression data and presentation layers. The introduction of `GameInputState` and `ActivityManager` has successfully established event-driven patterns for input blocking and activity routing. However, several legacy systems—particularly `PlacementManager` and `RewardManager`—exhibit monolithic tendencies and direct coupling with UI components, violating the strict separation of concerns [1].

## 2. High-Risk Systems & "God Objects"

### 2.1. PlacementManager (351 lines)
The `PlacementManager` is the largest script in the project and acts as a partial "god object."

*   **Overloaded Responsibilities:** It handles input polling, grid validation math, object instantiation, drag-and-drop state, and inventory return logic.
*   **Direct Coupling Risk:** It directly manages the lifecycle of `DraggableItem` and communicates directly with `InventoryPlacementBridge`.
*   **Recommendation:** Extract the input polling and raycasting logic into a dedicated `PlacementInputHandler`. Move the instantiation and cleanup logic into a `PlacementFactory`. `PlacementManager` should solely coordinate state transitions (Idle $\rightarrow$ Dragging $\rightarrow$ Placed) and fire events.

### 2.2. RewardManager (151 lines)
While conceptually sound, `RewardManager` currently acts as a central hub that knows too much about the specific mechanics of other systems.

*   **Overloaded Responsibilities:** It directly calls `progressionManager.AddXp()` and `chestProgress.AddProgress()`, acting as a master controller rather than a pure event router.
*   **Direct Coupling Risk:** It holds direct references to `PlayerProgressionManager`, `ChestProgressManager`, `ChestQueueManager`, and `ItemRewardPool`.
*   **Recommendation:** Convert `RewardManager` into a pure event broadcaster. When `CompleteTask()` is called, it should simply fire `OnTaskCompleted`. The `PlayerProgressionManager` and `ChestProgressManager` should listen to this event and apply their respective logic independently.

## 3. UI and Logic Coupling

The principle that UI scripts must never dictate game rules or modify core progression data is generally upheld, but there are notable exceptions [1].

### 3.1. ChestOpeningUI (194 lines)
*   **Violation:** The `OnOpenButtonClicked()` method directly calls `rewardManager.OpenNextChest()`. While it correctly waits for the `OnChestOpened` event to play animations, the direct method call couples the UI to the progression logic.
*   **Recommendation:** The UI should fire a generic `OnOpenRequested` event. A separate logic controller (e.g., `ChestFlowController`) should listen to the UI event and command the `RewardManager` to open the chest.

### 3.2. InventoryUI (294 lines)
*   **Violation:** `InventoryUI` holds a direct reference to `PlacementManager` to facilitate drag-and-drop interactions from inventory slots.
*   **Recommendation:** Use an interface (e.g., `IDragDropHandler`) or a dedicated bridge script to handle the transition of an item from the UI layer to the 3D placement layer, severing the direct dependency.

## 4. Areas for Event-Driven Improvement

### 4.1. The Bridge Scripts
The project uses "Bridge" scripts (e.g., `ProgressionBridge`, `InventoryPlacementBridge`) to connect systems. While this is better than direct coupling within the core managers, these bridges still rely on hardcoded method calls.

*   **InventoryPlacementBridge:** Directly calls `inventoryManager.RemoveItem()` and `inventoryManager.AddItem()`.
*   **Recommendation:** Transition to a true event bus or scriptable object event architecture. Instead of a bridge script explicitly calling methods on `InventoryManager`, the `PlacementManager` should fire a `PlacementEvent(ItemData, ActionType)`, and the `InventoryManager` should natively listen and react to it.

## 5. Prioritised Recommendations

The following table outlines the recommended architectural improvements, ordered by impact and feasibility.

| Priority | System | Recommendation | Rationale |
| :--- | :--- | :--- | :--- |
| **High** | `RewardManager` | Decouple from `PlayerProgressionManager` and `ChestProgressManager` by firing events instead of making direct method calls. | Prevents `RewardManager` from becoming a monolithic dependency hub. |
| **High** | `PlacementManager` | Extract input handling and object instantiation into separate classes. | Reduces the size and complexity of the project's largest script. |
| **Medium** | `ChestOpeningUI` | Replace direct `rewardManager.OpenNextChest()` calls with a UI event listener pattern. | Enforces the "UI is Presentation-Only" rule [1]. |
| **Low** | Bridge Scripts | Phase out bridge scripts in favour of a global event bus or native event listening. | Creates a truly modular architecture where systems can be added or removed without breaking links. |

## References
[1] Manus AI, *ARCHITECTURE.md*, Productivity Garden Repository, May 13, 2026.
