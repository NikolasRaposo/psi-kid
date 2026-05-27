using TMPro;
using UnityEngine;

/// <summary>
/// Counts down a per-level time budget and displays it as mm:ss on a TMP label.
/// Stops at zero. (Formerly named GameController.)
/// </summary>
public class LevelTimer : MonoBehaviour {
    [Tooltip("TMP label that shows the remaining time.")]
    [SerializeField] private TMP_Text timerText;
    [Tooltip("Level time budget in seconds (600 = 10 minutes).")]
    [SerializeField] private float durationSeconds = 600f;

    private float timeRemaining;
    private bool isRunning;

    private void Start() {
        timeRemaining = durationSeconds;
        isRunning = true;
    }

    private void Update() {
        if (!isRunning) {
            return;
        }

        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0f) {
            timeRemaining = 0f;
            isRunning = false;
        }

        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
