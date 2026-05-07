# Draggable Placement Architecture

## System Overview

```
InputController          (abstracts mouse/touch → world position)
       │
       ▼
PlacementManager         (orchestrates the full drag-place flow)
       │              │
       ▼              ▼
DraggableItem      PlacementValidator
       │                   │
       └──────┬────────────┘
              ▼
         GridManager      (occupancy, world↔grid conversion)
```

## Responsibilities

| Class | Single Responsibility |
|-------|----------------------|
| `InputController` | Converts raw input (mouse/touch) to world-space `Vector3`. No gameplay logic. |
| `GridManager` | Owns the occupancy grid. Converts world↔grid. Answers availability queries. |
| `PlacementValidator` | Asks GridManager if an area is free. Returns `ValidationResult`. |
| `DraggableItem` | Attached to a placed object. Follows cursor, snaps to grid, shows green/red. |
| `PlacementManager` | Picks up items, hands them to DraggableItem, finalises or cancels placement. |

## Data Flow: Picking Up an Existing Object

1. Player clicks a placed object
2. `PlacementManager.PickUp(placedObject)` is called
3. GridManager frees the object's previous cells
4. A `DraggableItem` component is activated on the object
5. Object follows cursor each frame via `InputController.WorldPosition`

## Data Flow: Placing an Object

1. Player releases (left-click confirm) while `DraggableItem` is active
2. `DraggableItem` asks `PlacementValidator.Validate(gridPos, size)`
3. If valid → `PlacementManager.Finalize()` marks cells occupied, removes `DraggableItem`
4. If invalid → placement is blocked, object stays dragging

## Data Flow: Cancelling a Drag

1. Player presses Escape or right-clicks
2. `PlacementManager.Cancel()` returns object to its original position
3. GridManager re-marks the original cells occupied

## Rotation

- `DraggableItem` tracks `currentRotation` (0, 90, 180, 270)
- R key cycles rotation via `InputController` key event
- Size footprint is swapped for 90°/270° before validation

## Input Abstraction

```csharp
public interface IInputController
{
    Vector3 WorldPosition { get; }       // cursor position on ground plane
    bool ConfirmPressed { get; }         // left-click / tap
    bool CancelPressed { get; }          // right-click / escape
    bool RotatePressed { get; }          // R key
}
```

Concrete implementations:
- `MouseInputController` — uses New Input System mouse
- (Future) `TouchInputController` — maps first touch to same interface

## Key Design Rules

- `DraggableItem` never reads input directly — it receives `IInputController`
- `PlacementManager` never touches the grid directly — it calls `GridManager` methods
- `PlacementValidator` is stateless — pure function, no MonoBehaviour state
- `GridManager` has no knowledge of UI, dragging, or input
