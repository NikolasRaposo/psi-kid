using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Single read point for gameplay input, backed by the Input System (keyboard,
/// mouse and gamepad). A scripted source can take over through <see cref="Override"/>,
/// which is how the capture tools play the level for trailers and automated runs
/// without touching gameplay code.
/// </summary>
public static class GameInput {
    /// <summary>A replaceable input source (e.g. a scripted demo).</summary>
    public interface ISource {
        float MoveX { get; }
        bool JumpPressed { get; }
        bool InteractPressed { get; }
        Vector2 PointerScreen { get; }
        bool PointerPressed { get; }
        bool PointerHeld { get; }
        bool PointerReleased { get; }
    }

    /// <summary>When set, gameplay reads this source instead of the devices.</summary>
    public static ISource Override;

    public static float MoveX => Override?.MoveX ?? DeviceMoveX();
    public static bool JumpPressed => Override?.JumpPressed ?? DeviceJump();
    public static bool InteractPressed => Override?.InteractPressed ?? DeviceInteract();
    public static Vector2 PointerScreen => Override?.PointerScreen ?? (Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero);
    public static bool PointerPressed => Override?.PointerPressed ?? (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);
    public static bool PointerHeld => Override?.PointerHeld ?? (Mouse.current != null && Mouse.current.leftButton.isPressed);
    public static bool PointerReleased => Override?.PointerReleased ?? (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame);

    private static float DeviceMoveX() {
        float x = 0f;
        var kb = Keyboard.current;
        if (kb != null) {
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;
        }
        var pad = Gamepad.current;
        if (pad != null && Mathf.Abs(pad.leftStick.x.ReadValue()) > 0.3f) {
            x = Mathf.Sign(pad.leftStick.x.ReadValue());
        }
        return x;
    }

    private static bool DeviceJump() {
        var kb = Keyboard.current;
        var pad = Gamepad.current;
        return (kb != null && (kb.spaceKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame))
            || (pad != null && pad.buttonSouth.wasPressedThisFrame);
    }

    private static bool DeviceInteract() {
        var kb = Keyboard.current;
        var pad = Gamepad.current;
        return (kb != null && (kb.leftShiftKey.wasPressedThisFrame || kb.rightShiftKey.wasPressedThisFrame || kb.eKey.wasPressedThisFrame))
            || (pad != null && pad.buttonWest.wasPressedThisFrame);
    }
}
