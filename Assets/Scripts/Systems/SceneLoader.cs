using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Centralizes scene transitions. Always restores <see cref="Time.timeScale"/>
/// before loading, so a paused or finished level never carries a frozen time
/// scale into the next scene.
/// </summary>
public static class SceneLoader {
    private const int MainMenuSceneIndex = 0;

    /// <summary>Reloads the currently active scene.</summary>
    public static void ReloadCurrent() {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>Loads the main menu scene (build index 0).</summary>
    public static void LoadMainMenu() {
        Time.timeScale = 1f;
        SceneManager.LoadScene(MainMenuSceneIndex);
    }
}
