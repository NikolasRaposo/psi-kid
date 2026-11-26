using System;
using UnityEngine;

/// <summary>
/// Player settings (master, music and effects volume, fullscreen), saved in
/// PlayerPrefs and applied on startup. Audio scripts read the volumes from here.
/// </summary>
public static class GameSettings {
    private const string MasterKey = "settings.master";
    private const string MusicKey = "settings.music";
    private const string SfxKey = "settings.sfx";

    // Defaults are deliberately quiet: the CC0 tracks are mastered loud.
    public const float DefaultMaster = 0.8f;
    public const float DefaultMusic = 0.35f;
    public const float DefaultSfx = 0.6f;

    /// <summary>Raised after any setting changes.</summary>
    public static event Action Changed;

    public static float Master { get; private set; } = DefaultMaster;
    public static float Music { get; private set; } = DefaultMusic;
    public static float Sfx { get; private set; } = DefaultSfx;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Load() {
        Master = PlayerPrefs.GetFloat(MasterKey, DefaultMaster);
        Music = PlayerPrefs.GetFloat(MusicKey, DefaultMusic);
        Sfx = PlayerPrefs.GetFloat(SfxKey, DefaultSfx);
        Changed = null;
        AudioListener.volume = Master;
    }

    public static void SetMaster(float v) {
        Master = Mathf.Clamp01(v);
        AudioListener.volume = Master;
        Save(MasterKey, Master);
    }

    public static void SetMusic(float v) {
        Music = Mathf.Clamp01(v);
        Save(MusicKey, Music);
    }

    public static void SetSfx(float v) {
        Sfx = Mathf.Clamp01(v);
        Save(SfxKey, Sfx);
    }

    public static bool Fullscreen {
        get => Screen.fullScreen;
        set {
            Screen.fullScreen = value;
            Changed?.Invoke();
        }
    }

    private static void Save(string key, float v) {
        PlayerPrefs.SetFloat(key, v);
        PlayerPrefs.Save();
        Changed?.Invoke();
    }
}
