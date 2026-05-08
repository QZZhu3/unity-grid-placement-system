# Cozy 2.5D Camera Setup Guide

This guide explains how to set up the new `CameraController` system for smooth, multi-angle 2.5D views while maintaining accurate grid placement.

## 1. Scene Setup

1. Create an Empty GameObject at `(0,0,0)` (or the centre of your grid) and name it `CameraFocus`.
2. Select your `Main Camera` and add the `CameraController` component.
3. In the `CameraController` inspector:
   - Assign **Target Focus** -> `CameraFocus`
   - Assign **Cam** -> `Main Camera`
   - Set **Transition Speed** -> `5`

## 2. Configuring Camera Presets

The system uses presets to define camera angles. In the `CameraController` inspector, expand the **Presets** array and set its size to `4` (for four isometric angles).

Configure them like this (adjust offsets to suit your grid size):

### Preset 0 (South)
- **Name**: `South`
- **Position Offset**: `(0, 15, -15)`
- **Rotation**: `(45, 0, 0)`
- **Field Of View**: `40`

### Preset 1 (East)
- **Name**: `East`
- **Position Offset**: `(15, 15, 0)`
- **Rotation**: `(45, -90, 0)`
- **Field Of View**: `40`

### Preset 2 (North)
- **Name**: `North`
- **Position Offset**: `(0, 15, 15)`
- **Rotation**: `(45, 180, 0)`
- **Field Of View**: `40`

### Preset 3 (West)
- **Name**: `West`
- **Position Offset**: `(-15, 15, 0)`
- **Rotation**: `(45, 90, 0)`
- **Field Of View**: `40`

## 3. UI Buttons (Optional)

If you want on-screen buttons to rotate the camera (great for mobile):
1. Create a UI Button on your Canvas.
2. Add the `CameraAngleButton` component.
3. Check/uncheck **Is Next Button** depending on whether you want it to rotate left or right.
4. (You can still use **Q** and **E** keys to rotate on PC).

## 4. How it interacts with Placement

The `MouseInputController` already uses `Camera.main.ScreenPointToRay()`. Because the `CameraController` actually moves and rotates the physical camera object, Unity's physics raycasts automatically adapt. 

Whether you are looking from the South, East, North, or West, clicking the screen will accurately hit the correct grid cell. The placement system calculates grid occupancy using world coordinates, which are entirely independent of the camera angle.
