using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A lab door that slides up into the ceiling when any plate wired to it is
/// satisfied, and slides back down when none is. Replaces the old behaviour of
/// switching the door GameObject off, so the change reads on screen.
/// </summary>
public class Door : MonoBehaviour {
    [Tooltip("How far the door panels travel up when open (world units).")]
    [SerializeField] private float travel = 2f;
    [Tooltip("Seconds to open or close.")]
    [SerializeField] private float duration = 0.22f;

    private readonly HashSet<Object> openSources = new();
    private readonly List<Transform> panels = new();
    private readonly List<Vector3> closedPos = new();
    private Collider2D[] colliders;
    private float t; // 0 closed, 1 open
    private bool open;

    public bool IsOpen => open;

    private void Awake() {
        colliders = GetComponentsInChildren<Collider2D>();
        foreach (var sr in GetComponentsInChildren<SpriteRenderer>()) {
            panels.Add(sr.transform);
            closedPos.Add(sr.transform.localPosition);
            // panels hide when they slide into the ceiling
            sr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        }
        // a mask the size of the closed door: anything that slides above it is hidden
        var mask = new GameObject("DoorMask").AddComponent<SpriteMask>();
        mask.transform.SetParent(transform, false);
        mask.sprite = Resources.Load<Sprite>("Art/door_mask");
        mask.transform.localPosition = new Vector3(0f, 0f, 0f);
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
        float eased = t * t * (3f - 2f * t);
        for (int i = 0; i < panels.Count; i++) {
            panels[i].localPosition = closedPos[i] + Vector3.up * travel * eased;
        }
    }
}
