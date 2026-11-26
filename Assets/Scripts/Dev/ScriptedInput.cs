using UnityEngine;

/// <summary>
/// A <see cref="GameInput.ISource"/> driven by code (the demo director, tests).
/// Set <see cref="moveX"/>, <see cref="pointerWorld"/> and <see cref="pointerHeld"/>;
/// one-frame presses (jump, interact, pointer down/up) are derived here so gameplay
/// sees exactly what it would see from a real device.
/// </summary>
[DefaultExecutionOrder(-1000)]
public class ScriptedInput : MonoBehaviour, GameInput.ISource {
    public float moveX;
    public Vector2 pointerWorld;
    public bool pointerHeld;

    private bool jumpQueued, interactQueued;
    private bool jumpNow, interactNow, pressedNow, releasedNow, prevHeld;
    private Camera cam;

    public void Jump() => jumpQueued = true;
    public void Interact() => interactQueued = true;

    private void OnEnable() => GameInput.Override = this;

    private void OnDisable() {
        if (ReferenceEquals(GameInput.Override, this)) {
            GameInput.Override = null;
        }
    }

    private void Update() {
        jumpNow = jumpQueued;
        interactNow = interactQueued;
        jumpQueued = interactQueued = false;
        pressedNow = pointerHeld && !prevHeld;
        releasedNow = !pointerHeld && prevHeld;
        prevHeld = pointerHeld;
    }

    public float MoveX => moveX;
    public bool JumpPressed => jumpNow;
    public bool InteractPressed => interactNow;
    public bool PointerPressed => pressedNow;
    public bool PointerHeld => pointerHeld;
    public bool PointerReleased => releasedNow;

    public Vector2 PointerScreen {
        get {
            if (cam == null) {
                cam = Camera.main;
            }
            return cam != null ? (Vector2)cam.WorldToScreenPoint(pointerWorld) : Vector2.zero;
        }
    }
}
