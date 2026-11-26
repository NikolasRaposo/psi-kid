using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A lab door made of two panels that retract like shutters (the top one into
/// the ceiling, the bottom one into the floor) when any plate wired to it is
/// satisfied, and close again when none is. No masks are involved, so it works
/// wherever the door sits, even next to other doors.
/// </summary>
public class Door : MonoBehaviour {
    [Tooltip("Seconds to open or close.")]
    [SerializeField] private float duration = 0.22f;

    private readonly HashSet<Object> openSources = new();
    private readonly List<Transform> panels = new();
    private readonly List<Vector3> closedPos = new();
    private readonly List<Vector3> closedScale = new();
    private readonly List<float> anchor = new(); // edge each panel retracts into (local y)
    private Collider2D[] colliders;
    private float t; // 0 closed, 1 open
    private bool open;

    public bool IsOpen => open;

    private void Awake() {
        colliders = GetComponentsInChildren<Collider2D>();
        foreach (var sr in GetComponentsInChildren<SpriteRenderer>()) {
            var tr = sr.transform;
            panels.Add(tr);
            closedPos.Add(tr.localPosition);
            closedScale.Add(tr.localScale);
            float half = sr.sprite != null ? sr.sprite.bounds.extents.y * tr.localScale.y : 0.5f;
            // upper panel retracts up, lower panel retracts down
            anchor.Add(tr.localPosition.y >= 0f ? tr.localPosition.y + half : tr.localPosition.y - half);
        }
    }

    /// <summary>Registers whether a given source (a plate) wants this door open.</summary>
    public void SetSourceOpen(Object source, bool wantsOpen) {
        if (wantsOpen) {
            openSources.Add(source);
        } else {
            openSources.Remove(source);
        }
        bool shouldOpen = openSources.Count > 0;
        if (shouldOpen == open) {
            return;
        }
        open = shouldOpen;
        foreach (var c in colliders) {
            c.enabled = !open;
        }
        Sfx.Play(open ? "door_open" : "door_close", 0.45f);
    }

    private void Update() {
        float target = open ? 1f : 0f;
        if (Mathf.Approximately(t, target)) {
            return;
        }
        t = Mathf.MoveTowards(t, target, Time.deltaTime / duration);
        float s = 1f - t * t * (3f - 2f * t); // remaining panel height (1 closed .. 0 open)
        for (int i = 0; i < panels.Count; i++) {
            var p = closedPos[i];
            float y = Mathf.Lerp(anchor[i], p.y, s);
            panels[i].localPosition = new Vector3(p.x, y, p.z);
            panels[i].localScale = new Vector3(closedScale[i].x, closedScale[i].y * Mathf.Max(s, 0.0001f), closedScale[i].z);
        }
    }
}
