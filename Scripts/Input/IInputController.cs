using UnityEngine;

/// <summary>
/// Abstracts all player input used by the placement system.
/// Gameplay scripts depend only on this interface -- never on
/// UnityEngine.Input or UnityEngine.InputSystem directly.
/// Implement this interface to support mouse, touch, or gamepad.
/// </summary>
public interface IInputController
{
    /// <summary>
    /// The current cursor position projected onto the world ground plane.
    /// Returns Vector3.zero if no valid ground hit is found.
    /// </summary>
    Vector3 WorldPosition { get; }

    /// <summary>
    /// True on the frame the player confirms placement (left-click or tap).
    /// </summary>
    bool ConfirmPressed { get; }

    /// <summary>
    /// True on the frame the player cancels placement (right-click or Escape).
    /// </summary>
    bool CancelPressed { get; }

    /// <summary>
    /// True on the frame the player requests a 90-degree rotation (R key).
    /// </summary>
    bool RotatePressed { get; }

    /// <summary>
    /// True on the frame the player clicks or taps on a world object.
    /// Used to initiate pick-up of an existing placed item.
    /// </summary>
    bool PickUpPressed { get; }

    /// <summary>
    /// The screen-space position of the cursor (used for raycasting against scene objects).
    /// </summary>
    Vector2 ScreenPosition { get; }
}
