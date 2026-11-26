using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tiny sound-effect player. Clips live in <c>Resources/Sfx</c> and are loaded by
/// name on first use, so gameplay scripts can play feedback without scene wiring.
/// One-shots go through a small pool; <see cref="Loop"/> returns a dedicated source.
/// </summary>
public static class Sfx {
    private const int PoolSize = 8;
    private static readonly Dictionary<string, AudioClip> Cache = new();
    private static AudioSource[] pool;
    private static int next;
    private static Transform root;

    /// <summary>Raised with the clip name whenever a sound plays (used by capture tools).</summary>
    public static event System.Action<string> Played;

    private static AudioClip Clip(string name) {
        if (!Cache.TryGetValue(name, out var clip)) {
            clip = Resources.Load<AudioClip>("Sfx/" + name);
            if (clip == null) {
                Debug.LogWarning($"Sfx: clip '{name}' not found in Resources/Sfx.");
            }
            Cache[name] = clip;
        }
        return clip;
    }

    private static void EnsurePool() {
        if (pool != null && root != null) {
            return;
        }
        root = new GameObject("[Sfx]").transform;
        Object.DontDestroyOnLoad(root.gameObject);
        pool = new AudioSource[PoolSize];
        for (int i = 0; i < PoolSize; i++) {
            pool[i] = root.gameObject.AddComponent<AudioSource>();
            pool[i].playOnAwake = false;
        }
    }

    /// <summary>Plays a one-shot clip from Resources/Sfx.</summary>
    public static void Play(string name, float volume = 1f, float pitch = 1f) {
        var clip = Clip(name);
        if (clip == null) {
            return;
        }
        EnsurePool();
        var src = pool[next];
        next = (next + 1) % PoolSize;
        src.pitch = pitch;
        src.PlayOneShot(clip, volume);
        Played?.Invoke(name);
    }

    /// <summary>Creates a looping source for a held sound (stop it by destroying or calling Stop).</summary>
    public static AudioSource Loop(string name, float volume, Transform parent) {
        var clip = Clip(name);
        if (clip == null) {
            return null;
        }
        var src = new GameObject("[SfxLoop] " + name).AddComponent<AudioSource>();
        src.transform.SetParent(parent, false);
        src.clip = clip;
        src.loop = true;
        src.volume = volume;
        src.Play();
        Played?.Invoke(name);
        return src;
    }
}
