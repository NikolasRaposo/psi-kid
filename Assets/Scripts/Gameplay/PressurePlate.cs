using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A plate that opens its door while something rests on it.
///
/// Rules (all optional, configured per plate in the Inspector):
/// - <see cref="requiredMass"/>: the total Rigidbody2D mass on the plate must reach
///   this value (e.g. two boxes, or the kid plus a box). 0 means "anything".
/// - <see cref="alsoRequires"/>: other plates that must be pressed at the same time
///   (an AND gate). Each plate re-evaluates when one of them changes.
///
/// It counts every occupant, so the door only closes when the last one leaves.
/// Visuals: the plate swaps between its off/on sprites (red/green LED) and draws a
/// wire to its door that lights up while the plate is satisfied.
/// </summary>
public class PressurePlate : MonoBehaviour {
    [Tooltip("Door controlled by this plate.")]
    [SerializeField] private GameObject door;

    [Header("Rules")]
    [Tooltip("Minimum total mass on the plate. 0 = any player or movable object.")]
    [SerializeField] private float requiredMass;
    [Tooltip("Plates that must also be pressed for the door to open (AND).")]
    [SerializeField] private PressurePlate[] alsoRequires = Array.Empty<PressurePlate>();

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer plateRenderer;
    [SerializeField] private Sprite offSprite;
    [SerializeField] private Sprite onSprite;
    [SerializeField] private bool drawWire = true;

    /// <summary>Raised when <see cref="IsPressed"/> changes.</summary>
    public event Action<PressurePlate> Changed;

    private readonly HashSet<Collider2D> occupants = new();
    private Door doorLogic;
    private PlateWire wire;
    private bool wasSatisfied;

    /// <summary>True while enough weight rests on this plate.</summary>
    public bool IsPressed { get; private set; }

    /// <summary>True while this plate is pressed and every plate it depends on is too.</summary>
    public bool IsSatisfied {
        get {
            if (!IsPressed) {
                return false;
            }
            foreach (var other in alsoRequires) {
                if (other != null && !other.IsPressed) {
                    return false;
                }
            }
            return true;
        }
    }

    private void Awake() {
        if (plateRenderer == null) {
            plateRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        if (offSprite == null) offSprite = Resources.Load<Sprite>("Art/plate_off");
        if (onSprite == null) onSprite = Resources.Load<Sprite>("Art/plate_on");
        if (door != null) {
            doorLogic = door.GetComponent<Door>();
            if (doorLogic == null) {
                doorLogic = door.AddComponent<Door>();
            }
        }
    }

    private void Start() {
        foreach (var other in alsoRequires) {
            if (other != null) {
                other.Changed += _ => Evaluate();
            }
        }
        if (drawWire && door != null) {
            wire = PlateWire.Create(transform, door.transform);
        }
        Refresh(force: true);
    }

    private static bool Counts(Collider2D c) => c.CompareTag("Player") || c.CompareTag("Movable");

    private void OnTriggerEnter2D(Collider2D collision) {
        if (Counts(collision) && occupants.Add(collision)) {
            Refresh();
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (occupants.Remove(collision)) {
            Refresh();
        }
    }

    private void Refresh(bool force = false) {
        occupants.RemoveWhere(c => c == null || !c.isActiveAndEnabled);
        float mass = 0f;
        foreach (var c in occupants) {
            mass += c.attachedRigidbody != null ? c.attachedRigidbody.mass : 1f;
        }
        bool pressed = occupants.Count > 0 && mass + 0.0001f >= requiredMass;
        if (pressed == IsPressed && !force) {
            return;
        }
        bool changed = pressed != IsPressed;
        IsPressed = pressed;
        if (plateRenderer != null && offSprite != null && onSprite != null) {
            plateRenderer.sprite = pressed ? onSprite : offSprite;
        }
        if (changed && !force) {
            Sfx.Play(pressed ? "plate_down" : "plate_up", 0.5f);
        }
        Evaluate(force);
        if (changed) {
            Changed?.Invoke(this);
        }
    }

    private void Evaluate(bool force = false) {
        bool satisfied = IsSatisfied;
        if (satisfied == wasSatisfied && !force) {
            return;
        }
        wasSatisfied = satisfied;
        if (doorLogic != null) {
            doorLogic.SetSourceOpen(this, satisfied);
        }
        if (wire != null) {
            wire.SetLit(satisfied);
        }
    }
}
