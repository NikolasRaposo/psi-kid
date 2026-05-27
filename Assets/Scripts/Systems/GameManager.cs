using System;
using UnityEngine;

/// <summary>
/// In-game flow actions (try again, restart, return to menu) wired to UI buttons.
/// Notifies listeners when the game should leave its paused state.
/// </summary>
public class GameManager : MonoBehaviour {
    /// <summary>Raised when gameplay should resume (used to close the pause UI).</summary>
    public static event Action OnUnpause;

    /// <summary>Restarts the current level (used by the "try again" flow).</summary>
    public void TryAgain() {
        SceneLoader.ReloadCurrent();
    }

    /// <summary>Restarts the current level and signals listeners to unpause.</summary>
    public void RestartLevel() {
        SceneLoader.ReloadCurrent();
        OnUnpause?.Invoke();
    }

    /// <summary>Loads the main menu and signals listeners to unpause.</summary>
    public void LoadMainMenu() {
        SceneLoader.LoadMainMenu();
        OnUnpause?.Invoke();
    }
}
