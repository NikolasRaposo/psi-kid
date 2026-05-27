using UnityEngine;

/// <summary>
/// Detects when the player reaches the level's finish trigger, shows the
/// "level complete" panel and freezes the game. Also exposes button hooks to
/// restart the level or return to the menu.
/// </summary>
public class EndGame : MonoBehaviour {
    [Tooltip("UI panel shown when the level is completed.")]
    [SerializeField] private GameObject levelCompleteUI;

    private void Start() {
        levelCompleteUI.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            ShowLevelComplete();
        }
    }

    private void ShowLevelComplete() {
        levelCompleteUI.SetActive(true);
        Time.timeScale = 0f;
    }

    /// <summary>Restarts the current level (wired to the restart button).</summary>
    public void RestartLevel() {
        SceneLoader.ReloadCurrent();
    }

    /// <summary>Returns to the main menu (wired to the menu button).</summary>
    public void LoadMainMenu() {
        SceneLoader.LoadMainMenu();
    }
}
