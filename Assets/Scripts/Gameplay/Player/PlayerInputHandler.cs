using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Bridges Unity's Input System actions to plain C# events that gameplay scripts
/// subscribe to. Centralizing input here keeps the rest of the game decoupled
/// from the concrete key/mouse bindings defined in the input asset.
/// </summary>
public class PlayerInputHandler : MonoBehaviour {
    public static event Action OnPause;
    public static event Action OnJump;
    public static event Action OnInteraction;
    public static event Action<bool> OnMouseClick;
    public static event Action<Vector2> OnMove;
    public static event Action<Vector2> OnStop;

    private PlayerInputSystem controls;

    private void Awake() {
        controls = new PlayerInputSystem();
    }

    private void OnEnable() {
        controls.Enable();
        controls.Player.Pause.performed += HandlePause;
        controls.Player.Jump.performed += HandleJump;
        controls.Player.Interaction.performed += HandleInteraction;
        controls.Player.MouseClick.performed += HandleMouseDown;
        controls.Player.MouseClick.canceled += HandleMouseUp;
        controls.Player.Move.performed += HandleMove;
        controls.Player.Move.canceled += HandleStop;
    }

    private void OnDisable() {
        // Unsubscribe with the same method references used to subscribe, otherwise
        // the handlers would leak (a fresh lambda is a different delegate instance).
        controls.Player.Pause.performed -= HandlePause;
        controls.Player.Jump.performed -= HandleJump;
        controls.Player.Interaction.performed -= HandleInteraction;
        controls.Player.MouseClick.performed -= HandleMouseDown;
        controls.Player.MouseClick.canceled -= HandleMouseUp;
        controls.Player.Move.performed -= HandleMove;
        controls.Player.Move.canceled -= HandleStop;
        controls.Disable();
    }

    private void HandlePause(InputAction.CallbackContext _) => OnPause?.Invoke();
    private void HandleJump(InputAction.CallbackContext _) => OnJump?.Invoke();
    private void HandleInteraction(InputAction.CallbackContext _) => OnInteraction?.Invoke();
    private void HandleMouseDown(InputAction.CallbackContext _) => OnMouseClick?.Invoke(true);
    private void HandleMouseUp(InputAction.CallbackContext _) => OnMouseClick?.Invoke(false);
    private void HandleMove(InputAction.CallbackContext context) => OnMove?.Invoke(context.ReadValue<Vector2>());
    private void HandleStop(InputAction.CallbackContext _) => OnStop?.Invoke(Vector2.zero);
}
