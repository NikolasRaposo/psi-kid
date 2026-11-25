using UnityEngine;

/// <summary>
/// The telekinesis mechanic: lets the player pick up a "Movable" object with the
/// pointer and drag it around. While an object is held its gravity is disabled so
/// it follows the cursor; releasing it restores normal physics and keeps the
/// throw velocity.
///
/// Feedback (so the power reads on screen): the held box swaps to its glowing
/// sprite and gets a purple aura, a jittering psychic beam links the kid to the
/// box, particles burst on grab, and a hum plays while the box is held.
/// </summary>
public class ObjectController : MonoBehaviour {
    private const string MovableTag = "Movable";

    [Tooltip("Physics layer the selection raycast checks against.")]
    [SerializeField] private LayerMask selectableLayer;

    [Header("Feedback")]
    [Tooltip("Colour of the psychic beam, aura and particles.")]
    [SerializeField] private Color psiColor = new Color(0.75f, 0.52f, 0.99f, 1f);
    [Tooltip("How far the beam wobbles, in world units.")]
    [SerializeField] private float beamJitter = 0.12f;
    [Tooltip("Maximum speed kept when the box is released (units per second).")]
    [SerializeField] private float maxThrowSpeed = 14f;

    private Camera cam;
    private bool isDragging;
    private GameObject selectedObject;
    private Rigidbody2D selectedObjectRb;
    private Vector3 offset;
    private Vector2 lastPos;
    private Vector2 throwVelocity;

    private Transform player;
    private LineRenderer beam;
    private SpriteRenderer aura;
    private SpriteRenderer playerAura;
    private ParticleSystem burst;
    private AudioSource hum;

    private void Start() {
        cam = Camera.main;
        var p = GameObject.FindGameObjectWithTag("Player");
        player = p != null ? p.transform : null;
        BuildFeedback();
    }

    private void Update() {
        HandleObjectSelection();
        UpdateFeedback();
    }

    private void FixedUpdate() {
        HandleDragging();
    }

    private void HandleObjectSelection() {
        if (GameInput.PointerPressed) {
            Vector2 worldPoint = cam.ScreenToWorldPoint(GameInput.PointerScreen);
            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero, Mathf.Infinity, selectableLayer);
            if (hit.collider != null && hit.collider.CompareTag(MovableTag)) {
                SelectObject(hit.collider.gameObject);
            }
        }

        if (GameInput.PointerReleased && isDragging) {
            DeselectObject();
        }
    }

    private void SelectObject(GameObject hitObject) {
        isDragging = true;
        selectedObject = hitObject;
        selectedObjectRb = selectedObject.GetComponent<Rigidbody2D>();

        offset = selectedObject.transform.position - cam.ScreenToWorldPoint(GameInput.PointerScreen);
        selectedObjectRb.gravityScale = 0f;
        selectedObjectRb.linearVelocity = Vector2.zero;
        lastPos = selectedObjectRb.position;
        throwVelocity = Vector2.zero;

        SetSelectionVisual(true);
        aura.transform.SetParent(selectedObject.transform, false);
        aura.transform.localPosition = Vector3.zero;
        aura.enabled = true;
        burst.transform.position = selectedObject.transform.position;
        burst.Play();
        Sfx.Play("grab", 0.5f);
        hum = Sfx.Loop("psi_hum", 0.18f, transform);
    }

    private void DeselectObject() {
        isDragging = false;

        if (selectedObjectRb != null) {
            selectedObjectRb.gravityScale = 1f;
            selectedObjectRb.linearVelocity = Vector2.ClampMagnitude(throwVelocity, maxThrowSpeed);
        }
        if (selectedObject != null) {
            SetSelectionVisual(false);
        }

        aura.enabled = false;
        aura.transform.SetParent(transform, false);
        if (hum != null) {
            Destroy(hum.gameObject);
        }
        Sfx.Play("release", 0.45f);

        selectedObject = null;
        selectedObjectRb = null;
    }

    /// <summary>Toggles the "selected" and "unselected" child sprites of the held object.</summary>
    private void SetSelectionVisual(bool selected) {
        selectedObject.transform.Find("boxSelected").gameObject.SetActive(selected);
        selectedObject.transform.Find("boxUnselected").gameObject.SetActive(!selected);
    }

    private void HandleDragging() {
        if (isDragging && selectedObject != null && selectedObjectRb != null) {
            Vector3 mousePos = cam.ScreenToWorldPoint(GameInput.PointerScreen) + offset;
            selectedObjectRb.MovePosition(new Vector2(mousePos.x, mousePos.y));
            throwVelocity = (new Vector2(mousePos.x, mousePos.y) - lastPos) / Time.fixedDeltaTime;
            lastPos = new Vector2(mousePos.x, mousePos.y);
        }
    }

    // ------------------------------------------------------------------ feedback

    private void BuildFeedback() {
        var spriteMat = new Material(Shader.Find("Sprites/Default"));

        beam = new GameObject("PsiBeam").AddComponent<LineRenderer>();
        beam.transform.SetParent(transform, false);
        beam.material = spriteMat;
        beam.positionCount = 12;
        beam.widthCurve = new AnimationCurve(new Keyframe(0, 0.05f), new Keyframe(0.5f, 0.11f), new Keyframe(1, 0.05f));
        beam.startColor = new Color(psiColor.r, psiColor.g, psiColor.b, 0.35f);
        beam.endColor = psiColor;
        beam.sortingOrder = 5;
        beam.enabled = false;

        aura = MakeGlow("BoxAura", 2.4f, 4);
        aura.enabled = false;
        if (player != null) {
            playerAura = MakeGlow("KidAura", 2.0f, 0);
            playerAura.transform.SetParent(player, false);
            playerAura.transform.localPosition = new Vector3(0, 0.3f, 0);
            playerAura.enabled = false;
        }

        burst = new GameObject("PsiBurst").AddComponent<ParticleSystem>();
        burst.transform.SetParent(transform, false);
        burst.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = burst.main;
        main.playOnAwake = false;
        main.loop = false;
        main.duration = 0.3f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.5f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.16f);
        main.startColor = psiColor;
        main.gravityModifier = -0.1f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        var emission = burst.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0, 18) });
        var shape = burst.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.35f;
        var col = burst.colorOverLifetime;
        col.enabled = true;
        var g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(psiColor, 1) },
                  new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(0, 1) });
        col.color = g;
        var rend = burst.GetComponent<ParticleSystemRenderer>();
        rend.material = new Material(spriteMat);
        var tex = Resources.Load<Texture2D>("Art/psi_particle");
        if (tex != null) {
            rend.material.mainTexture = tex;
        }
        rend.sortingOrder = 6;
    }

    private SpriteRenderer MakeGlow(string name, float size, int order) {
        var sr = new GameObject(name).AddComponent<SpriteRenderer>();
        sr.transform.SetParent(transform, false);
        sr.sprite = Resources.Load<Sprite>("Art/glow");
        sr.color = new Color(psiColor.r, psiColor.g, psiColor.b, 0.55f);
        sr.sortingOrder = order;
        sr.transform.localScale = Vector3.one * size;
        return sr;
    }

    private void UpdateFeedback() {
        bool active = isDragging && selectedObject != null && player != null;
        beam.enabled = active;
        if (playerAura != null) {
            playerAura.enabled = active;
        }
        if (!active) {
            return;
        }

        float t = Time.time;
        float pulse = 1f + Mathf.Sin(t * 12f) * 0.08f;
        aura.transform.localScale = Vector3.one * 2.4f * pulse;
        if (playerAura != null) {
            playerAura.transform.localScale = Vector3.one * 2.0f * pulse;
        }

        Vector3 a = player.position + new Vector3(0, 0.45f, 0);
        Vector3 b = selectedObject.transform.position;
        Vector3 n = Vector3.Cross((b - a).normalized, Vector3.forward);
        for (int i = 0; i < beam.positionCount; i++) {
            float k = i / (float)(beam.positionCount - 1);
            float wobble = Mathf.Sin(k * Mathf.PI) * (Mathf.Sin(t * 30f + k * 9f) * beamJitter + Mathf.Sin(t * 7f) * beamJitter * 0.6f);
            beam.SetPosition(i, Vector3.Lerp(a, b, k) + n * wobble);
        }
    }

    /// <summary>True while the player is currently holding an object.</summary>
    public bool IsDragging() => isDragging;
}
