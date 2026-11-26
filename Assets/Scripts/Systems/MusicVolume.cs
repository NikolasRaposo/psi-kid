using UnityEngine;

/// <summary>
/// Scales a music AudioSource by the player's music volume and follows changes
/// made in the settings menu.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class MusicVolume : MonoBehaviour {
    [Tooltip("Volume of this track at 100% music volume.")]
    [Range(0f, 1f)]
    [SerializeField] private float baseVolume = 1f;

    private AudioSource source;

    private void Awake() {
        source = GetComponent<AudioSource>();
        Apply();
    }

    private void OnEnable() => GameSettings.Changed += Apply;
    private void OnDisable() => GameSettings.Changed -= Apply;

    private void Apply() {
        if (source != null) {
            source.volume = baseVolume * GameSettings.Music;
        }
    }
}
