# Reward Chest System: Setup & Integration Guide

The new reward chest system is a complete, modular framework designed to handle task completion, chest progression, reward drawing, and inventory integration. It separates the "task logic" from the "reward logic", making it easy to add new chests, seasons, or tasks without breaking existing code.

## 1. Pull the Latest Code

Run the following command in your local terminal to pull the new chest system scripts:

```bash
cd "C:\Users\Nz\Documents\UnityProjects\ProductivityTrackerGarden\Assets\PlacementSystem"
git pull origin main
```

## 2. Core Architecture Overview

The system is built on several key components:

- **`ChestDefinition` (ScriptableObject):** Defines a chest (ID, name, icon, rewards per chest, rarity weights, seasonal filters).
- **`ChestProgressManager`:** Tracks how many tasks the player has completed. When the threshold is reached, it "earns" a chest and sends it to the queue.
- **`ChestQueueManager`:** Holds all earned chests that haven't been opened yet.
- **`RewardManager`:** The central hub. It receives "Task Completed" signals, grants XP, tells `ChestProgressManager` to add progress, and handles the actual opening of chests (drawing items and adding them to the inventory).
- **`RewardDrawService` & `RewardFilterPipeline`:** The engine that selects which items drop from a chest based on rarity, seasons, and future rules like duplicate protection.
- **`ChestSaveHandler`:** Saves and loads chest progress and the pending queue.

## 3. Unity Editor Setup Steps

Follow these steps to configure the system in your Unity Editor:

### Step A: Create a Chest Definition
1. In the Project window, right-click inside `Assets/PlacementSystem/Data/` (or a new `Chests` folder).
2. Select **Create → Placement System → Chest → Chest Definition**.
3. Name the file `Chest_Basic`.
4. In the Inspector:
   - **Id:** `basic_chest`
   - **Display Name:** `Basic Reward Chest`
   - **Rewards Per Chest:** `3` (or whatever you prefer)
   - Leave the other settings as default.

### Step B: Set Up the Managers
1. In the Hierarchy, select your **`ProgressionSystem`** GameObject.
2. Add the following components:
   - `ChestProgressManager`
   - `ChestQueueManager`
   - `RewardManager`
   - `ChestSaveHandler`
3. Configure **`ChestProgressManager`**:
   - **Default Chest:** Drag your new `Chest_Basic` asset here.
   - **Tasks Per Chest:** Set to `3` (meaning 3 tasks = 1 chest).
4. Configure **`RewardManager`**:
   - **Xp Per Task:** Set to `50` (or your desired amount).
   - **Chest Progress Per Task:** Set to `1`.

### Step C: Set Up the UI (Optional but Recommended)
To test the visual flow, you can set up the UI components:

1. **Progress UI:**
   - Create a Canvas UI with a Slider and a Text label.
   - Add the `ChestProgressUI` script to the UI root.
   - Assign the Slider and Text to the script.
2. **Opening UI:**
   - Create a panel for opening chests (with an image for the chest, text for the name, and a layout group for the reward cards).
   - Add the `ChestOpeningUI` script to the panel.
   - Assign the necessary UI references.
3. **Debug Task Button:**
   - Create a UI Button labeled "Complete Task".
   - Add the `TemporaryTaskUI` script to it and assign the Button.

## 4. How to Test

1. Press **Play** in Unity.
2. Click your "Complete Task" button (or call `RewardManager.CompleteTask()` via script/context menu).
3. Notice the XP increasing and the chest progress filling up.
4. After 3 tasks, a chest is earned and added to the queue.
5. Click the "Open Chest" button in your UI (or call `RewardManager.OpenNextChest()` via context menu).
6. The chest opens, draws 3 random items based on rarity, and adds them to your inventory!

## 5. Next Steps for Expansion

- **Seasons:** Create a `Chest_Spring` definition and assign `Season_Spring` to its Allowed Season Tags.
- **Animations:** Hook up Unity Animators to the `ChestOpeningUI` for a satisfying reveal sequence.
- **Task Integration:** When your actual productivity task system is built, simply have it call `RewardManager.CompleteTask()` when a real-world task is ticked off.
