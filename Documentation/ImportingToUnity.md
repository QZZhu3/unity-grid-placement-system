# How to Import the Placement System into Unity

This guide explains how to open and import the grid-based placement system files into your Unity project.

## Method 1: Direct File Copy (Recommended)

### Step 1: Locate Your Unity Project
1. Open your file explorer (Windows Explorer, Finder, or file manager).
2. Navigate to your Unity project folder.
3. Open the `Assets` folder inside your project.

### Step 2: Copy Script Files
1. Download or extract the `UnityPlacementSystem` folder.
2. Navigate to the `Scripts` folder inside it.
3. Copy all `.cs` files:
   - GridManager.cs
   - PlaceableItem.cs
   - PlacementController.cs
   - PlacementPreview.cs
   - PlacementUIManager.cs
   - RotationHandler.cs

4. Paste them into your `Assets` folder (or a subfolder like `Assets/Scripts/PlacementSystem`).

### Step 3: Verify Import
1. Return to Unity and wait a moment for it to compile.
2. Check the Console window for any errors.
3. If no errors appear, the scripts are successfully imported!

## Method 2: Using the Zip File

### Step 1: Extract the Zip
1. Download the `UnityPlacementSystem_Complete.zip` file.
2. Right-click and select **Extract All** (Windows) or double-click (Mac).
3. Choose a location to extract.

### Step 2: Copy to Assets Folder
1. Open the extracted `UnityPlacementSystem` folder.
2. Open the `Scripts` subfolder.
3. Copy all `.cs` files.
4. In your Unity project, open the `Assets` folder.
5. Create a new folder called `PlacementSystem` (optional but recommended).
6. Paste the `.cs` files into this folder.

### Step 3: Wait for Compilation
1. Switch to Unity.
2. The editor will automatically detect the new files and compile them.
3. Wait for the compilation to complete (check the bottom-right corner of the editor).

## Method 3: Drag and Drop

### Step 1: Arrange Windows
1. Open your file explorer with the `Scripts` folder visible.
2. Arrange your screen so you can see both the file explorer and Unity editor.

### Step 2: Drag Files
1. In the file explorer, select all `.cs` files (Ctrl+A or Cmd+A).
2. Drag them into the Unity Project window (the folder tree on the left side).
3. Drop them into the `Assets` folder or a subfolder.

### Step 3: Verify
1. The files should appear in the Project window.
2. Check the Console for compilation errors.

## Opening Documentation Files

### In a Text Editor
1. The `.md` files (QuickStart.md, SceneSetupGuide.md, etc.) are Markdown files.
2. You can open them with any text editor:
   - **Windows**: Notepad, Visual Studio Code, Notepad++
   - **Mac**: TextEdit, Visual Studio Code
   - **Linux**: gedit, VS Code, nano

### In a Markdown Viewer
For better formatting, use a Markdown viewer:
- **Visual Studio Code** (recommended) - Free, with built-in Markdown preview
- **GitHub** - Paste the file content into a GitHub gist
- **Online Markdown Viewers** - Search "Markdown viewer" online

### In Unity
You can also view Markdown files directly in Unity:
1. Copy the `.md` files into your `Assets` folder.
2. Select them in the Project window.
3. They'll display in the Inspector (read-only).

## Verifying Successful Import

After importing the scripts, verify everything is working:

1. **Check for Errors**: Open `Window > General > Console` and look for red error messages.
2. **Check for Warnings**: Yellow warnings are usually okay, but review them.
3. **Create a Test GameObject**: 
   - Right-click in the Hierarchy
   - Select `Create Empty`
   - Drag the `GridManager` script onto it
   - You should see the script component appear in the Inspector

## Troubleshooting Import Issues

### "Assets/Scripts/GridManager.cs(1,1): error CS0246: The type or namespace name 'UnityEngine' could not be found"

**Solution**: This usually means Unity didn't recognize the files as C# scripts. Try:
1. Restart Unity completely.
2. Delete the `Library` folder in your project (it will rebuild).
3. Re-import the scripts.

### "The namespace already contains a definition for 'GridManager'"

**Solution**: You've imported the files twice. Delete the duplicate copies and keep only one set.

### Scripts appear but won't attach to GameObjects

**Solution**: 
1. Check the Console for compilation errors.
2. Ensure all scripts are in the `Assets` folder (not in a subfolder outside Assets).
3. Make sure the script names match the class names exactly.

### "Cannot find type 'TextMeshProUGUI'"

**Solution**: TextMeshPro isn't imported in your project. In Unity:
1. Go to `Window > TextMeshPro > Import TMP Essential Resources`
2. Click Import
3. Wait for the import to complete

## Next Steps

Once the scripts are imported:

1. **Read QuickStart.md** - Get the system running in 5 minutes
2. **Read SceneSetupGuide.md** - Detailed configuration instructions
3. **Read APIReference.md** - Understand all available methods
4. **Read Architecture.md** - Learn how to extend the system

## File Organization Best Practices

After importing, organize your project like this:

```
Assets/
├── Scripts/
│   └── PlacementSystem/
│       ├── GridManager.cs
│       ├── PlacementController.cs
│       ├── PlaceableItem.cs
│       ├── PlacementPreview.cs
│       ├── PlacementUIManager.cs
│       └── RotationHandler.cs
├── Prefabs/
│   └── Items/
│       ├── Wall.prefab
│       └── Floor.prefab
├── ScriptableObjects/
│   └── Items/
│       ├── WallItem.asset
│       └── FloorItem.asset
└── Scenes/
    └── PlacementTest.unity
```

This organization keeps your project clean and makes it easy to find files later.
