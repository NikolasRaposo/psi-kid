using UnityEngine;

public class ObjectController : MonoBehaviour {
    private const string MOVABLE_TAG = "Movable";
    private Camera cam;
    private bool isDragging = false;
    private GameObject selectedObject;
    private Rigidbody2D selectedObjectRb;
    public LayerMask selectableLayer;
    private Vector3 offset;

    void Start() {
        cam = Camera.main;
    }

    void Update() {
        HandleObjectSelection();
        HandleDragging();
    }

    private void HandleObjectSelection() {
        if (Input.GetMouseButtonDown(0)) {
            RaycastHit2D hit = Physics2D.Raycast(cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, Mathf.Infinity, selectableLayer);
            if (hit.collider != null && hit.collider.CompareTag(MOVABLE_TAG)) {
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

        // Ativa o sprite "selecionado" do objeto atual
        selectedObject.transform.Find("boxSelected").gameObject.SetActive(true);
        selectedObject.transform.Find("boxUnselected").gameObject.SetActive(false);
    }

    private void DeselectObject() {
        isDragging = false;

        if (selectedObjectRb != null) {
            selectedObjectRb.gravityScale = 1f;
        }

        // Restaura o sprite "n�o selecionado" do objeto atual
        selectedObject.transform.Find("boxSelected").gameObject.SetActive(false);
        selectedObject.transform.Find("boxUnselected").gameObject.SetActive(true);

        selectedObject = null;
        selectedObjectRb = null;
    }

    private void HandleDragging() {
        if (isDragging && selectedObject != null && selectedObjectRb != null) {
            Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition) + offset;
            Vector2 targetPosition = new Vector2(mousePos.x, mousePos.y);
            selectedObjectRb.MovePosition(targetPosition);
        }
    }

    public bool IsDragging() => isDragging;
}
