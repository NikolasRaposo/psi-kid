using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Main menu buttons: start the first level, open/close the settings panel and
/// quit the application.
/// </summary>
public class MainMenuController : MonoBehaviour {
    [Tooltip("Settings panel toggled by the settings button.")]
    [SerializeField] private GameObject settingsPanel;

    /// <summary>Loads the first gameplay scene (build index 1).</summary>
    public void StartGame() {
        SceneManager.LoadScene(1);
    }

    public void OpenSettings() {
        settingsPanel.SetActive(true);
    }

    public void CloseSettings() {
        settingsPanel.SetActive(false);
    }

    /// <summary>Quits the game (stops play mode in the editor).</summary>
    public void ExitGame() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
