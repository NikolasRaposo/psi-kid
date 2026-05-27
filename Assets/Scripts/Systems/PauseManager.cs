using UnityEngine;

/// <summary>
/// Toggles the paused state of the game: freezes time and shows the pause panel.
/// Listens to the pause input and to the GameManager's unpause signal.
/// </summary>
public class PauseManager : MonoBehaviour {
    [Tooltip("Panel shown while the game is paused.")]
    [SerializeField] private GameObject pausePanelUI;

    private bool isPaused;

    private void Start() {
        pausePanelUI.SetActive(false);
    }

    private void OnEnable() {
        PlayerInputHandler.OnPause += TogglePause;
        GameManager.OnUnpause += TogglePause;
    }

    private void OnDisable() {
        PlayerInputHandler.OnPause -= TogglePause;
        GameManager.OnUnpause -= TogglePause;
    }

    private void TogglePause() {
        if (isPaused) {
            ResumeGame();
        } else {
            PauseGame();
        }
    }

    private void PauseGame() {
        Time.timeScale = 0f;
        isPaused = true;
        pausePanelUI.SetActive(true);
    }

    /// <summary>Resumes gameplay and hides the pause panel.</summary>
    public void ResumeGame() {
        Time.timeScale = 1f;
        isPaused = false;
        pausePanelUI.SetActive(false);
    }
}
