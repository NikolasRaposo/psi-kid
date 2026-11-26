using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Shared look for the menus: pixel-art 9-slice sprites from Resources/Art/ui_*,
/// the Barlow font and the lab palette. Used by the scene setup (editor) and by
/// UI built at runtime, so every button and panel looks the same.
/// </summary>
public static class UiStyle {
    /// <summary>UI pixels per art pixel on a 1920x1080 reference canvas.</summary>
    public const float ArtPixel = 5f;

    public static readonly Color Text = new Color32(226, 230, 245, 255);
    public static readonly Color TextHover = new Color32(255, 255, 255, 255);
    public static readonly Color Title = new Color32(233, 213, 255, 255);
    public static readonly Color Muted = new Color32(150, 158, 190, 255);
    public static readonly Color Accent = new Color32(94, 234, 255, 255);
    public static readonly Color Backdrop = new Color32(8, 6, 16, 200);

    public static Sprite Sprite(string name) => Resources.Load<Sprite>("Art/" + name);
    public static TMP_FontAsset Font => Resources.Load<TMP_FontAsset>("Fonts/Barlow-Bold SDF");

    /// <summary>Pixel-per-unit multiplier that renders one art pixel as <paramref name="artPixel"/> UI pixels.</summary>
    public static float Multiplier(Sprite s, float artPixel = ArtPixel) =>
        s == null ? 1f : 100f / (s.pixelsPerUnit * artPixel);

    public static void Sliced(Image img, string sprite, float artPixel = ArtPixel) {
        img.sprite = Sprite(sprite);
        img.type = Image.Type.Sliced;
        img.pixelsPerUnitMultiplier = Multiplier(img.sprite, artPixel);
        img.color = Color.white;
    }

    public static void Label(TMP_Text t, string text, float size, Color color) {
        var font = Font;
        if (font != null) {
            t.font = font;
            t.fontSharedMaterial = font.material;
        }
        t.text = text;
        t.fontSize = size;
        t.enableAutoSizing = false;
        t.fontStyle = FontStyles.Normal;
        t.characterSpacing = 4f;
        t.color = color;
        t.alignment = TextAlignmentOptions.Center;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.raycastTarget = false;
    }

    /// <summary>Gives an existing button the pixel-art skin, a label and hover/click feedback.</summary>
    public static void Button(Button b, string label, float fontSize = 40f) {
        var img = b.GetComponent<Image>();
        Sliced(img, "ui_button_normal");
        b.transition = Selectable.Transition.SpriteSwap;
        b.spriteState = new SpriteState {
            highlightedSprite = Sprite("ui_button_hover"),
            pressedSprite = Sprite("ui_button_pressed"),
            selectedSprite = Sprite("ui_button_normal"),
            disabledSprite = Sprite("ui_button_normal"),
        };

        var t = b.GetComponentInChildren<TMP_Text>(true);
        if (t != null) {
            Label(t, label, fontSize, Text);
            var rt = t.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            // the face sits above the 2-pixel shadow strip
            rt.offsetMin = new Vector2(0f, ArtPixel * 2f);
            rt.offsetMax = Vector2.zero;
        }
        if (b.GetComponent<UiButtonFx>() == null) {
            b.gameObject.AddComponent<UiButtonFx>();
        }
    }

    // ---------------------------------------------------------------- runtime builders

    public static RectTransform Rect(string name, Transform parent) {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = 5; // UI
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        return rt;
    }

    public static RectTransform Place(RectTransform rt, Vector2 pos, Vector2 size) {
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return rt;
    }

    public static TMP_Text MakeText(Transform parent, string text, float size, Color color, Vector2 pos, Vector2 box,
        TextAlignmentOptions align = TextAlignmentOptions.Center) {
        var t = Place(Rect("Text", parent), pos, box).gameObject.AddComponent<TextMeshProUGUI>();
        Label(t, text, size, color);
        t.alignment = align;
        return t;
    }

    public static Button MakeButton(Transform parent, string label, Vector2 pos, Vector2 size, UnityAction onClick) {
        var rt = Place(Rect("Btn" + label.Replace(" ", ""), parent), pos, size);
        rt.gameObject.AddComponent<Image>();
        var b = rt.gameObject.AddComponent<Button>();
        Rect("Label", rt).gameObject.AddComponent<TextMeshProUGUI>();
        Button(b, label, 36f);
        b.onClick.AddListener(onClick);
        return b;
    }

    public static Slider MakeSlider(Transform parent, Vector2 pos, Vector2 size, float value, UnityAction<float> onChange) {
        var root = Place(Rect("Slider", parent), pos, size);
        var bg = Rect("Track", root);
        bg.anchorMin = new Vector2(0f, 0.5f);
        bg.anchorMax = new Vector2(1f, 0.5f);
        bg.sizeDelta = new Vector2(0f, 8f * ArtPixel);
        Sliced(bg.gameObject.AddComponent<Image>(), "ui_slider_track");

        var area = Rect("Fill Area", root);
        area.anchorMin = new Vector2(0f, 0.5f);
        area.anchorMax = new Vector2(1f, 0.5f);
        area.sizeDelta = new Vector2(0f, 8f * ArtPixel);
        var fill = Rect("Fill", area);
        fill.anchorMin = Vector2.zero;
        fill.anchorMax = new Vector2(0f, 1f);
        fill.sizeDelta = Vector2.zero;
        var fillImg = fill.gameObject.AddComponent<Image>();
        Sliced(fillImg, "ui_slider_fill");
        fillImg.raycastTarget = false;

        var handleArea = Rect("Handle Area", root);
        handleArea.anchorMin = Vector2.zero;
        handleArea.anchorMax = Vector2.one;
        handleArea.offsetMin = new Vector2(5f * ArtPixel, 0f);
        handleArea.offsetMax = new Vector2(-5f * ArtPixel, 0f);
        var handle = Rect("Handle", handleArea);
        handle.anchorMin = Vector2.zero;
        handle.anchorMax = new Vector2(0f, 1f);
        handle.sizeDelta = new Vector2(10f * ArtPixel, 0f);
        var hImg = handle.gameObject.AddComponent<Image>();
        hImg.sprite = Sprite("ui_knob");

        var s = root.gameObject.AddComponent<Slider>();
        s.fillRect = fill;
        s.handleRect = handle;
        s.targetGraphic = hImg;
        s.direction = Slider.Direction.LeftToRight;
        s.minValue = 0f;
        s.maxValue = 1f;
        s.SetValueWithoutNotify(value);
        s.onValueChanged.AddListener(onChange);
        var c = s.colors;
        c.highlightedColor = new Color(1f, 0.92f, 1f);
        c.pressedColor = new Color(0.85f, 0.8f, 1f);
        c.selectedColor = Color.white;
        s.colors = c;
        return s;
    }

    public static Toggle MakeToggle(Transform parent, Vector2 pos, bool value, UnityAction<bool> onChange) {
        float size = 12f * ArtPixel;
        var root = Place(Rect("Toggle", parent), pos, new Vector2(size, size));
        var bg = root.gameObject.AddComponent<Image>();
        bg.sprite = Sprite("ui_toggle_off");
        var check = Rect("Check", root);
        check.anchorMin = Vector2.zero;
        check.anchorMax = Vector2.one;
        check.sizeDelta = Vector2.zero;
        var cImg = check.gameObject.AddComponent<Image>();
        cImg.sprite = Sprite("ui_toggle_on");
        cImg.raycastTarget = false;
        var t = root.gameObject.AddComponent<Toggle>();
        t.targetGraphic = bg;
        t.graphic = cImg;
        t.toggleTransition = Toggle.ToggleTransition.None;
        t.SetIsOnWithoutNotify(value);
        t.onValueChanged.AddListener(onChange);
        root.gameObject.AddComponent<UiButtonFx>();
        return t;
    }
}
