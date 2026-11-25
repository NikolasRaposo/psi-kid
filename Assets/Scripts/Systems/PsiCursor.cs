using UnityEngine;

/// <summary>
/// In-game cursor: a psychic reticle that follows <see cref="GameInput.PointerScreen"/>,
/// spins while the player holds an object and hides the OS cursor. Being part of the
/// rendered frame, it also shows up in captures and trailers.
/// </summary>
public class PsiCursor : MonoBehaviour {
    [SerializeField] private float size = 1.1f;
    [SerializeField] private float spinSpeed = 220f;

    private SpriteRenderer sr;
    private Camera cam;
    private ObjectController telekinesis;

    private void Start() {
        cam = Camera.main;
        telekinesis = FindFirstObjectByType<ObjectController>();
        sr = new GameObject("PsiCursorSprite").AddComponent<SpriteRenderer>();
        sr.transform.SetParent(transform, false);
        sr.sprite = Resources.Load<Sprite>("Art/psi_cursor");
        sr.sortingOrder = 100;
        Cursor.visible = false;
    }

    private void OnDisable() {
        Cursor.visible = true;
    }

    private void LateUpdate() {
        if (cam == null || sr == null) {
            return;
        }
        Vector3 p = cam.ScreenToWorldPoint(GameInput.PointerScreen);
        p.z = 0f;
        sr.transform.position = p;
        bool holding = telekinesis != null && telekinesis.IsDragging();
        float scale = size * (holding ? 1.25f : 1f);
        sr.transform.localScale = Vector3.one * scale;
        if (holding) {
            sr.transform.Rotate(0, 0, -spinSpeed * Time.unscaledDeltaTime);
        } else {
            sr.transform.rotation = Quaternion.identity;
        }
        sr.enabled = Time.timeScale > 0f;
        Cursor.visible = !sr.enabled;
    }
}
