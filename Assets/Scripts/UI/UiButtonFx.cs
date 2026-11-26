using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Hover and click feedback for menu buttons: a small pop, a brighter label, the
/// label sinking with the pressed sprite, and soft UI sounds. Works while paused.
/// </summary>
public class UiButtonFx : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler {
    private const float HoverScale = 1.04f;

    private Selectable selectable;
    private TMP_Text label;
    private Vector2 labelPos;
    private bool hasLabel;
    private float target = 1f;

    private void Awake() {
        selectable = GetComponent<Selectable>();
        label = GetComponentInChildren<TMP_Text>(true);
        hasLabel = label != null;
        if (hasLabel) {
            labelPos = label.rectTransform.anchoredPosition;
        }
    }

    private void OnDisable() {
        target = 1f;
        transform.localScale = Vector3.one;
        SetLabel(false, false);
    }

    private void Update() {
        float s = Mathf.MoveTowards(transform.localScale.x, target, Time.unscaledDeltaTime * 1.2f);
        transform.localScale = new Vector3(s, s, 1f);
    }

    private bool Interactable => selectable == null || selectable.IsInteractable();

    public void OnPointerEnter(PointerEventData e) {
        if (!Interactable) {
            return;
        }
        target = HoverScale;
        SetLabel(true, false);
        Sfx.Play("ui_hover", 0.35f);
    }

    public void OnPointerExit(PointerEventData e) {
        target = 1f;
        SetLabel(false, false);
    }

    public void OnPointerDown(PointerEventData e) {
        if (Interactable) {
            SetLabel(true, true);
        }
    }

    public void OnPointerUp(PointerEventData e) => SetLabel(e.hovered.Contains(gameObject), false);

    public void OnPointerClick(PointerEventData e) {
        if (Interactable) {
            Sfx.Play("ui_click", 0.6f);
        }
    }

    private void SetLabel(bool hot, bool pressed) {
        if (!hasLabel) {
            return;
        }
        label.color = hot ? UiStyle.TextHover : UiStyle.Text;
        // the pressed sprite drops the face by one art pixel; the label follows
        label.rectTransform.anchoredPosition = labelPos + (pressed ? new Vector2(0f, -UiStyle.ArtPixel) : Vector2.zero);
    }
}
