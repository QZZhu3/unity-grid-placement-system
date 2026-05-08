# Save/Load System Architecture

This project implements a modular, JSON-based save/load foundation designed for scalability and future cloud sync support.

## Core Components

The save system is divided into three layers to keep logic decoupled:

1. **`GameSaveData` (Data Models)**
   - Plain C# classes (`[Serializable]`) representing the JSON structure.
   - Contains `PlacementSaveData` and `InventorySaveData`.
   - Includes a `version` string and `timestamp` for future migrations.

2. **`ISaveable` (Interface)**
   - Implemented by any system that needs to save/load state.
   - `PopulateSaveData(GameSaveData)`: Write current state to the data object.
   - `LoadFromSaveData(GameSaveData)`: Read state from the data object and apply it.

3. **`SaveManager` (Coordinator)**
   - Central script that auto-discovers all `ISaveable` components in the scene.
   - Handles the actual file I/O (JSON serialization/deserialization).
   - Because file I/O is isolated here, replacing `File.WriteAllText` with a cloud API call later is trivial.

## Data Flow

### Save Flow
1. `SaveManager.SaveGame()` is called.
2. Creates a new `GameSaveData` instance.
3. Loops through all `ISaveable` components, calling `PopulateSaveData()`.
   - `PlacementSaveHandler` iterates over `GridManager.GetAllPlacedItems()` and writes ID, position, size, and rotation.
   - `InventorySaveHandler` iterates over `InventoryManager.GetAllSlots()` and writes ID and quantity.
   - *Note: Active drag states are intentionally ignored. If the player is dragging an item, it remains in inventory and is not saved to the grid, preventing duplication.*
4. Serializes the populated `GameSaveData` to JSON.
5. Writes to `Application.persistentDataPath / savegame.json`.

### Load Flow
1. `SaveManager.LoadGame()` is called.
2. Reads JSON from disk and deserializes into `GameSaveData`.
3. Loops through all `ISaveable` components, calling `LoadFromSaveData()`.
   - `PlacementSaveHandler` clears the current grid, finds the matching `PlaceableItem` assets via `Resources.FindObjectsOfTypeAll`, instantiates their prefabs, and registers them with `GridManager`.
   - `InventorySaveHandler` clears the inventory and restores the saved quantities.

## Setup Instructions

1. Add an empty GameObject to your scene named `SaveSystem`.
2. Add the `SaveManager` component to it.
3. On your `PlacementSystem` GameObject (which holds `GridManager` and `InventoryManager`), add:
   - `PlacementSaveHandler`
   - `InventorySaveHandler`
4. You can test saving and loading by clicking the 3 dots (⋮) on the `SaveManager` component in the Inspector and selecting **Force Save** or **Force Load**.

## Cloud Sync Readiness

The `SaveManager` currently writes to a local file:

```csharp
string json = JsonUtility.ToJson(data);
File.WriteAllText(SaveFilePath, json);
```

To support cloud saves in the future, you only need to modify `SaveManager` to send the `json` string via an HTTP request (e.g., using UnityWebRequest) to your backend, without touching any of the gameplay or serialization logic.
