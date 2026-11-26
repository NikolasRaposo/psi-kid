using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Main menu buttons: start the first level, open the settings panel and quit
/// the application (hidden in the browser build, where quitting does nothing).
/// </summary>
public class MainMenuController : MonoBehaviour {
    [Tooltip("Quit button, hidden on WebGL.")]
    [SerializeField] private GameObject exitButton;

    private void Start() {
        if (exitButton != null && Application.platform == RuntimePlatform.WebGLPlayer) {
            exitButton.SetActive(false);
        }
    }

    /// <summary>Loads the first gameplay scene (build index 1).</summary>
    public void StartGame() {
        SceneManager.LoadScene(1);
    }

    public void OpenSettings() {
        SettingsMenu.Open();
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
