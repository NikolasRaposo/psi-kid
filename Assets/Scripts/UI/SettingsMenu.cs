using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>
/// Settings overlay shared by the main menu and the pause menu: master, music and
/// effects volume, fullscreen, and the controls. Built in code from
/// <see cref="UiStyle"/> so both scenes get the same panel without prefab wiring.
/// </summary>
public class SettingsMenu : MonoBehaviour {
    private static SettingsMenu open;
    private static int closedFrame = -1;

    /// <summary>True while the panel is open (or was closed this frame), so Esc is not handled twice.</summary>
    public static bool IsBusy => open != null || closedFrame == Time.frameCount;

    public static void Open() {
        if (open != null) {
            return;
        }
        if (EventSystem.current == null) {
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }
        var go = new GameObject("[SettingsMenu]");
        open = go.AddComponent<SettingsMenu>();
        open.Build();
    }

    public static void Close() {
        if (open == null) {
            return;
        }
        Destroy(open.gameObject);
        open = null;
        closedFrame = Time.frameCount;
    }

    private void OnDestroy() {
        if (open == this) {
            open = null;
        }
    }

    private void Update() {
        var kb = Keyboard.current;
        var pad = Gamepad.current;
        if ((kb != null && kb.escapeKey.wasPressedThisFrame) || (pad != null && pad.buttonEast.wasPressedThisFrame)) {
            Close();
        }
    }

    private void Build() {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        // dim backdrop (also swallows clicks meant for the menu behind)
        var back = UiStyle.Rect("Backdrop", transform);
        back.anchorMin = Vector2.zero;
        back.anchorMax = Vector2.one;
        back.sizeDelta = Vector2.zero;
        back.gameObject.AddComponent<Image>().color = UiStyle.Backdrop;

        var panel = UiStyle.Place(UiStyle.Rect("Panel", transform), Vector2.zero, new Vector2(840f, 720f));
        UiStyle.Sliced(panel.gameObject.AddComponent<Image>(), "ui_panel");

        UiStyle.MakeText(panel, "SETTINGS", 60f, UiStyle.Title, new Vector2(0f, 280f), new Vector2(700f, 80f));

        const float y = 170f;
        Row(panel, "Master", y, GameSettings.Master, GameSettings.SetMaster);
        Row(panel, "Music", y - 90f, GameSettings.Music, GameSettings.SetMusic);
        Row(panel, "Effects", y - 180f, GameSettings.Sfx, GameSettings.SetSfx);

        UiStyle.MakeText(panel, "Fullscreen", 34f, UiStyle.Text, new Vector2(-200f, y - 270f), new Vector2(300f, 60f),
            TextAlignmentOptions.Left);
        UiStyle.MakeToggle(panel, new Vector2(-80f, y - 270f), Screen.fullScreen, on => GameSettings.Fullscreen = on);

        UiStyle.MakeText(panel,
            "Move  A / D        Jump  Space        Pause  Esc\nTelekinesis  hold left mouse        Elevator  Shift",
            24f, UiStyle.Muted, new Vector2(0f, -185f), new Vector2(760f, 80f));

        UiStyle.MakeButton(panel, "BACK", new Vector2(0f, -280f), new Vector2(300f, 90f), Close);
    }

    private static void Row(Transform panel, string label, float y, float value, UnityAction<float> set) {
        UiStyle.MakeText(panel, label, 34f, UiStyle.Text, new Vector2(-200f, y), new Vector2(300f, 60f),
            TextAlignmentOptions.Left);
        var pct = UiStyle.MakeText(panel, Percent(value), 30f, UiStyle.Accent, new Vector2(320f, y), new Vector2(110f, 60f),
            TextAlignmentOptions.Right);
        UiStyle.MakeSlider(panel, new Vector2(90f, y), new Vector2(400f, 70f), value, v => {
            set(v);
            pct.text = Percent(v);
        });
    }

    private static string Percent(float v) => Mathf.RoundToInt(v * 100f) + "%";
}
