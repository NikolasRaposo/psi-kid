using UnityEngine;

/// <summary>
/// The telekinesis mechanic: lets the player pick up a "Movable" object with the
/// mouse and drag it around. While an object is held its gravity is disabled so
/// it follows the cursor; releasing it restores normal physics.
/// </summary>
public class ObjectController : MonoBehaviour {
    private const string MovableTag = "Movable";

    [Tooltip("Physics layer the selection raycast checks against.")]
    [SerializeField] private LayerMask selectableLayer;

    private Camera cam;
    private bool isDragging;
    private GameObject selectedObject;
    private Rigidbody2D selectedObjectRb;
    private Vector3 offset;

    private void Start() {
        cam = Camera.main;
    }

    private void Update() {
        HandleObjectSelection();
        HandleDragging();
    }

    private void HandleObjectSelection() {
        if (Input.GetMouseButtonDown(0)) {
            Vector2 worldPoint = cam.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero, Mathf.Infinity, selectableLayer);
            if (hit.collider != null && hit.collider.CompareTag(MovableTag)) {
                SelectObject(hit.collider.gameObject);
            }
        }

        if (Input.GetMouseButtonUp(0) && isDragging) {
            DeselectObject();
        }
    }

    private void SelectObject(GameObject hitObject) {
        isDragging = true;
        selectedObject = hitObject;
        selectedObjectRb = selectedObject.GetComponent<Rigidbody2D>();

        offset = selectedObject.transform.position - cam.ScreenToWorldPoint(Input.mousePosition);
        selectedObjectRb.gravityScale = 0f;
        selectedObjectRb.linearVelocity = Vector2.zero;

        SetSelectionVisual(true);
    }

    private void DeselectObject() {
        isDragging = false;

        if (selectedObjectRb != null) {
            selectedObjectRb.gravityScale = 1f;
        }
        if (selectedObject != null) {
            SetSelectionVisual(false);
        }

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
            Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition) + offset;
            selectedObjectRb.MovePosition(new Vector2(mousePos.x, mousePos.y));
        }
    }

    /// <summary>True while the player is currently holding an object.</summary>
    public bool IsDragging() => isDragging;
}
